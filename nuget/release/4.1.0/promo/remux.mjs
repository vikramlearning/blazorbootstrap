// Rewrites a fragmented MP4 (what Chromium's MediaRecorder writes: an empty moov, then moof/mdat fragments)
// as a standard MP4 with one moov holding complete sample tables. Fragmented files report a wrong duration
// in players, seek badly, and some social networks reject them. Samples are copied byte for byte; nothing
// is re-encoded. Supports the box types MediaRecorder emits (ISO/IEC 14496-12).

const u32 = (b, p) => b.readUInt32BE(p);

function readBoxes(buf, start, end) {
    const list = [];
    let p = start;
    while (p + 8 <= end) {
        let size = u32(buf, p);
        const type = buf.toString('latin1', p + 4, p + 8);
        let header = 8;
        if (size === 1) { size = Number(buf.readBigUInt64BE(p + 8)); header = 16; }
        if (size === 0) size = end - p;
        list.push({ type, start: p, data: p + header, end: p + size });
        p += size;
    }
    return list;
}
const child = (buf, box, type) => readBoxes(buf, box.data, box.end).find(b => b.type === type);
const children = (buf, box, type) => readBoxes(buf, box.data, box.end).filter(b => b.type === type);

function box(type, ...payloads) {
    const body = Buffer.concat(payloads.map(p => (Buffer.isBuffer(p) ? p : Buffer.from(p))));
    const head = Buffer.alloc(8);
    head.writeUInt32BE(8 + body.length, 0);
    head.write(type, 4, 'latin1');
    return Buffer.concat([head, body]);
}
function fullBox(type, version, flags, ...payloads) {
    const vf = Buffer.alloc(4);
    vf.writeUInt32BE(((version & 0xff) << 24) | (flags & 0xffffff), 0);
    return box(type, vf, ...payloads);
}
const uint32s = values => {
    const b = Buffer.alloc(values.length * 4);
    values.forEach((v, i) => b.writeUInt32BE(v >>> 0, i * 4));
    return b;
};
const int32s = values => {
    const b = Buffer.alloc(values.length * 4);
    values.forEach((v, i) => b.writeInt32BE(v, i * 4));
    return b;
};
const raw = (buf, b) => buf.subarray(b.start, b.end);

/**
 * @param {Buffer} input fragmented MP4 bytes
 * @returns {{ output: Buffer, tracks: { id: number, handler: string, samples: number, seconds: number }[] }}
 */
export function remuxFragmented(input) {
    const top = readBoxes(input, 0, input.length);
    const ftyp = top.find(b => b.type === 'ftyp');
    const moov = top.find(b => b.type === 'moov');
    if (!moov || !top.some(b => b.type === 'moof')) throw new Error('Not a fragmented MP4.');

    const mvhd = child(input, moov, 'mvhd');
    const movieTimescale = u32(input, mvhd.data + (input[mvhd.data] === 1 ? 20 : 12));
    const trexById = new Map();
    const mvex = child(input, moov, 'mvex');
    for (const trex of mvex ? children(input, mvex, 'trex') : []) {
        const d = trex.data + 4;
        trexById.set(u32(input, d), { descIndex: u32(input, d + 4), duration: u32(input, d + 8), size: u32(input, d + 12), flags: u32(input, d + 16) });
    }

    const tracks = new Map();
    for (const trak of children(input, moov, 'trak')) {
        const tkhd = child(input, trak, 'tkhd');
        const id = u32(input, tkhd.data + (input[tkhd.data] === 1 ? 20 : 12));
        const mdia = child(input, trak, 'mdia');
        const mdhd = child(input, mdia, 'mdhd');
        const hdlr = child(input, mdia, 'hdlr');
        tracks.set(id, {
            id, trak, mdia, tkhd, mdhd,
            handler: input.toString('latin1', hdlr.data + 8, hdlr.data + 12),
            timescale: u32(input, mdhd.data + (input[mdhd.data] === 1 ? 20 : 12)),
            samples: [], // { offset, size, duration, flags, cto }
            firstTime: null,
            chunks: [] // { first sample index, count }
        });
    }

    // Walk the fragments in file order.
    for (let k = 0; k < top.length; k++) {
        const moof = top[k];
        if (moof.type !== 'moof') continue;
        for (const traf of children(input, moof, 'traf')) {
            const tfhd = child(input, traf, 'tfhd');
            const tfFlags = u32(input, tfhd.data) & 0xffffff;
            let p = tfhd.data + 4;
            const id = u32(input, p); p += 4;
            const track = tracks.get(id);
            const trex = trexById.get(id) ?? { duration: 0, size: 0, flags: 0 };
            let base = moof.start;
            if (tfFlags & 0x1) { base = Number(input.readBigUInt64BE(p)); p += 8; }
            if (tfFlags & 0x2) p += 4;
            let defDuration = trex.duration, defSize = trex.size, defFlags = trex.flags;
            if (tfFlags & 0x8) { defDuration = u32(input, p); p += 4; }
            if (tfFlags & 0x10) { defSize = u32(input, p); p += 4; }
            if (tfFlags & 0x20) { defFlags = u32(input, p); p += 4; }
            const tfdt = child(input, traf, 'tfdt');
            if (tfdt && track.firstTime === null) {
                track.firstTime = input[tfdt.data] === 1 ? Number(input.readBigUInt64BE(tfdt.data + 4)) : u32(input, tfdt.data + 4);
            }
            for (const trun of children(input, traf, 'trun')) {
                const version = input[trun.data];
                const flags = u32(input, trun.data) & 0xffffff;
                let q = trun.data + 4;
                const count = u32(input, q); q += 4;
                let dataOffset = 0;
                if (flags & 0x1) { dataOffset = input.readInt32BE(q); q += 4; }
                let firstFlags = null;
                if (flags & 0x4) { firstFlags = u32(input, q); q += 4; }
                let offset = base + dataOffset;
                const chunkStart = track.samples.length;
                for (let i = 0; i < count; i++) {
                    const duration = flags & 0x100 ? u32(input, (q += 4) - 4) : defDuration;
                    const size = flags & 0x200 ? u32(input, (q += 4) - 4) : defSize;
                    let sampleFlags = flags & 0x400 ? u32(input, (q += 4) - 4) : defFlags;
                    if (i === 0 && firstFlags !== null) sampleFlags = firstFlags;
                    let cto = 0;
                    if (flags & 0x800) { cto = version === 0 ? u32(input, q) : input.readInt32BE(q); q += 4; }
                    track.samples.push({ offset, size, duration, flags: sampleFlags, cto });
                    offset += size;
                }
                if (count) track.chunks.push({ first: chunkStart, count });
            }
        }
    }

    // Lay out the new mdat: chunks in the order they appeared, so audio and video stay interleaved.
    const order = [];
    for (const track of tracks.values()) for (const chunk of track.chunks) order.push({ track, chunk, at: track.samples[chunk.first].offset });
    order.sort((a, b) => a.at - b.at);
    const pieces = [];
    let mdatSize = 0;
    for (const item of order) {
        const s0 = item.track.samples[item.chunk.first];
        const last = item.track.samples[item.chunk.first + item.chunk.count - 1];
        item.length = last.offset + last.size - s0.offset;
        item.newOffset = mdatSize; // relative to mdat payload, fixed up below
        pieces.push(input.subarray(s0.offset, s0.offset + item.length));
        mdatSize += item.length;
    }

    const buildMoov = mdatPayloadStart => {
        const traks = [];
        let movieDuration = 0;
        for (const track of tracks.values()) {
            const s = track.samples;
            const mediaDuration = s.reduce((sum, x) => sum + x.duration, 0);
            const offsetTime = track.firstTime ?? 0;
            const seconds = (mediaDuration + offsetTime) / track.timescale;
            movieDuration = Math.max(movieDuration, Math.round(seconds * movieTimescale));

            // stts: run-length durations.
            const stts = [];
            for (const x of s) {
                const lastRun = stts[stts.length - 1];
                if (lastRun && lastRun[1] === x.duration) lastRun[0]++;
                else stts.push([1, x.duration]);
            }
            const stbl = [
                raw(input, child(input, child(input, child(input, track.mdia, 'minf'), 'stbl'), 'stsd')),
                fullBox('stts', 0, 0, uint32s([stts.length, ...stts.flat()]))
            ];
            if (s.some(x => x.cto !== 0)) {
                const runs = [];
                for (const x of s) {
                    const lastRun = runs[runs.length - 1];
                    if (lastRun && lastRun[1] === x.cto) lastRun[0]++;
                    else runs.push([1, x.cto]);
                }
                const negative = runs.some(r => r[1] < 0);
                const entries = Buffer.alloc(4 + runs.length * 8);
                entries.writeUInt32BE(runs.length, 0);
                runs.forEach((r, i) => { entries.writeUInt32BE(r[0], 4 + i * 8); entries.writeInt32BE(r[1], 8 + i * 8); });
                stbl.push(fullBox('ctts', negative ? 1 : 0, 0, entries));
            }
            if (track.handler === 'vide') {
                const sync = s.map((x, i) => ((x.flags >>> 16) & 0x1 ? 0 : i + 1)).filter(Boolean);
                stbl.push(fullBox('stss', 0, 0, uint32s([sync.length, ...sync])));
            }
            const myChunks = order.filter(o => o.track === track);
            const stsc = [];
            myChunks.forEach((o, i) => {
                const lastRun = stsc[stsc.length - 1];
                if (!lastRun || lastRun[1] !== o.chunk.count) stsc.push([i + 1, o.chunk.count, 1]);
            });
            stbl.push(fullBox('stsc', 0, 0, uint32s([stsc.length, ...stsc.flat()])));
            stbl.push(fullBox('stsz', 0, 0, uint32s([0, s.length, ...s.map(x => x.size)])));
            stbl.push(fullBox('stco', 0, 0, uint32s([myChunks.length, ...myChunks.map(o => mdatPayloadStart + o.newOffset)])));

            const minf = child(input, track.mdia, 'minf');
            const minfKids = readBoxes(input, minf.data, minf.end).filter(b => b.type !== 'stbl').map(b => raw(input, b));
            const mdhd = Buffer.from(raw(input, track.mdhd));
            if (mdhd[8] === 1) mdhd.writeBigUInt64BE(BigInt(mediaDuration + offsetTime), 8 + 4 + 16 + 4);
            else mdhd.writeUInt32BE(mediaDuration + offsetTime, 8 + 4 + 12);
            const mdiaKids = readBoxes(input, track.mdia.data, track.mdia.end).map(b => {
                if (b.type === 'mdhd') return mdhd;
                if (b.type === 'minf') return box('minf', ...minfKids, box('stbl', ...stbl));
                return raw(input, b);
            });
            const tkhd = Buffer.from(raw(input, track.tkhd));
            const trackMovieDuration = Math.round(seconds * movieTimescale);
            if (tkhd[8] === 1) tkhd.writeBigUInt64BE(BigInt(trackMovieDuration), 8 + 4 + 16 + 4 + 4);
            else tkhd.writeUInt32BE(trackMovieDuration, 8 + 4 + 12 + 4);
            const trakKids = [tkhd];
            // An edit list delays a track that starts after zero, so audio and video stay in sync.
            if (offsetTime > 0) {
                const empty = Math.round(offsetTime / track.timescale * movieTimescale);
                trakKids.push(box('edts', fullBox('elst', 0, 0, uint32s([2, empty]), int32s([-1]), uint32s([0x00010000, Math.round(mediaDuration / track.timescale * movieTimescale)]), uint32s([0]), uint32s([0x00010000]))));
            }
            trakKids.push(box('mdia', ...mdiaKids));
            for (const b of readBoxes(input, track.trak.data, track.trak.end)) if (!['tkhd', 'mdia', 'edts'].includes(b.type)) trakKids.push(raw(input, b));
            traks.push(box('trak', ...trakKids));
            track.seconds = seconds;
        }
        const newMvhd = Buffer.from(raw(input, mvhd));
        if (newMvhd[8] === 1) newMvhd.writeBigUInt64BE(BigInt(movieDuration), 8 + 4 + 16 + 4);
        else newMvhd.writeUInt32BE(movieDuration, 8 + 4 + 12);
        const others = readBoxes(input, moov.data, moov.end).filter(b => !['mvhd', 'trak', 'mvex'].includes(b.type)).map(b => raw(input, b));
        return box('moov', newMvhd, ...traks, ...others);
    };

    // A plain (non-fragmented) brand list.
    const newFtyp = box('ftyp', Buffer.from('isom', 'latin1'), uint32s([0x200]), Buffer.from('isomiso2avc1mp41', 'latin1'));
    // moov goes before mdat (fast start), so its size decides where the samples land; build it twice.
    let moovBox = buildMoov(0);
    const mdatHeader = 8;
    moovBox = buildMoov(newFtyp.length + moovBox.length + mdatHeader);
    const mdatHead = Buffer.alloc(8);
    mdatHead.writeUInt32BE(8 + mdatSize, 0);
    mdatHead.write('mdat', 4, 'latin1');
    void ftyp;
    const output = Buffer.concat([newFtyp, moovBox, mdatHead, ...pieces]);
    return {
        output,
        tracks: [...tracks.values()].map(t => ({ id: t.id, handler: t.handler, samples: t.samples.length, seconds: +t.seconds.toFixed(3), startsAt: t.firstTime / t.timescale }))
    };
}
