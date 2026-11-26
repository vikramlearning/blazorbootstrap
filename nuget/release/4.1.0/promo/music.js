// Original background music for the 4.1.0 promo, synthesized with Web Audio (no samples, royalty free).
// 120 BPM, 4/4, one bar = 2 s, 15 bars = 30 s. Chords I-V-vi-IV in C major.
// The arrangement follows the video's scene cuts: intro (bars 0-1), full groove (2-11),
// lift into the call to action at 24 s (12-13), and a held final chord with a fade (14).

export const BPM = 120;
export const BEAT = 60 / BPM;
export const BAR = BEAT * 4;
export const DURATION = 30;

// Final level after the limiter; lower it if musicStats() reports a peak above -1 dBFS.
const OUTPUT_TRIM = 0.9;

const CHORDS = [
    { bass: 36, pad: [60, 64, 67, 72] }, // C
    { bass: 31, pad: [59, 62, 67, 71] }, // G
    { bass: 33, pad: [60, 64, 69, 72] }, // Am
    { bass: 29, pad: [60, 65, 69, 72] }  // F
];

const hz = midi => 440 * Math.pow(2, (midi - 69) / 12);

// Seeded PRNG so every render is identical.
function rng(seed) {
    return () => {
        seed = (seed * 1664525 + 1013904223) >>> 0;
        return seed / 4294967296;
    };
}

function noiseBuffer(ctx, seconds, seed) {
    const random = rng(seed);
    const buffer = ctx.createBuffer(1, Math.ceil(ctx.sampleRate * seconds), ctx.sampleRate);
    const data = buffer.getChannelData(0);
    for (let i = 0; i < data.length; i++) data[i] = random() * 2 - 1;
    return buffer;
}

function impulse(ctx, seconds, decay, seed) {
    const random = rng(seed);
    const length = Math.ceil(ctx.sampleRate * seconds);
    const buffer = ctx.createBuffer(2, length, ctx.sampleRate);
    for (let c = 0; c < 2; c++) {
        const data = buffer.getChannelData(c);
        for (let i = 0; i < length; i++) data[i] = (random() * 2 - 1) * Math.pow(1 - i / length, decay);
    }
    return buffer;
}

function env(gain, t, peak, attack, hold, release) {
    gain.gain.setValueAtTime(0.0001, t);
    gain.gain.exponentialRampToValueAtTime(peak, t + attack);
    gain.gain.setValueAtTime(peak, t + attack + hold);
    gain.gain.exponentialRampToValueAtTime(0.0001, t + attack + hold + release);
}

/**
 * Renders the whole track offline and returns an AudioBuffer.
 * @param {number} sampleRate
 */
export async function renderMusic(sampleRate = 48000) {
    const ctx = new OfflineAudioContext(2, Math.ceil(sampleRate * DURATION), sampleRate);
    const noise = noiseBuffer(ctx, 2, 7);

    // Master chain: glue compression, a brick-wall style limiter, then a fixed trim. Web Audio compressors add
    // their own makeup gain, so the trim keeps the true peak near -1.5 dBFS (checked by musicStats()).
    const master = ctx.createGain();
    const comp = ctx.createDynamicsCompressor();
    comp.threshold.value = -16;
    comp.ratio.value = 3.5;
    comp.attack.value = 0.005;
    comp.release.value = 0.18;
    const limiter = ctx.createDynamicsCompressor();
    limiter.threshold.value = -6;
    limiter.knee.value = 0;
    limiter.ratio.value = 20;
    limiter.attack.value = 0.001;
    limiter.release.value = 0.06;
    const trim = ctx.createGain();
    trim.gain.value = OUTPUT_TRIM;
    master.connect(comp).connect(limiter).connect(trim).connect(ctx.destination);
    master.gain.setValueAtTime(0.0001, 0);
    master.gain.exponentialRampToValueAtTime(0.8, 0.08);
    master.gain.setValueAtTime(0.8, 28.4);
    master.gain.linearRampToValueAtTime(0.0001, DURATION);

    // Shared reverb send.
    const reverb = ctx.createConvolver();
    reverb.buffer = impulse(ctx, 2.6, 2.4, 11);
    const reverbOut = ctx.createGain();
    reverbOut.gain.value = 0.32;
    reverb.connect(reverbOut).connect(master);

    // Ducked bus for pad and bass, pumped by the kick.
    const ducked = ctx.createGain();
    ducked.connect(master);

    const send = (node, amount) => {
        const g = ctx.createGain();
        g.gain.value = amount;
        node.connect(g).connect(reverb);
    };

    // ---- instruments ----
    const pad = (t, dur, notes, cutoff) => {
        const filter = ctx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(cutoff, t);
        filter.Q.value = 0.6;
        const g = ctx.createGain();
        env(g, t, 0.055, 0.35, Math.max(0, dur - 0.6), 0.9);
        filter.connect(g);
        g.connect(ducked);
        send(g, 0.55);
        for (const n of notes) {
            for (const detune of [-8, 7]) {
                const o = ctx.createOscillator();
                o.type = 'sawtooth';
                o.frequency.value = hz(n);
                o.detune.value = detune;
                o.connect(filter);
                o.start(t);
                o.stop(t + dur + 1);
            }
        }
    };

    const bass = (t, midi, dur) => {
        const g = ctx.createGain();
        env(g, t, 0.34, 0.008, dur * 0.4, dur * 0.5);
        const filter = ctx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(900, t);
        filter.frequency.exponentialRampToValueAtTime(260, t + dur);
        filter.connect(g).connect(ducked);
        const saw = ctx.createOscillator();
        saw.type = 'sawtooth';
        saw.frequency.value = hz(midi + 12);
        const sub = ctx.createOscillator();
        sub.type = 'sine';
        sub.frequency.value = hz(midi);
        saw.connect(filter);
        sub.connect(filter);
        for (const o of [saw, sub]) { o.start(t); o.stop(t + dur + 0.1); }
    };

    const pluck = (t, midi, level) => {
        const g = ctx.createGain();
        env(g, t, level, 0.004, 0.02, 0.22);
        const filter = ctx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(5200, t);
        filter.frequency.exponentialRampToValueAtTime(900, t + 0.2);
        filter.connect(g).connect(master);
        send(g, 0.45);
        const o = ctx.createOscillator();
        o.type = 'square';
        o.frequency.value = hz(midi);
        const o2 = ctx.createOscillator();
        o2.type = 'triangle';
        o2.frequency.value = hz(midi + 12);
        o.connect(filter);
        o2.connect(filter);
        for (const x of [o, o2]) { x.start(t); x.stop(t + 0.3); }
    };

    const kick = t => {
        const o = ctx.createOscillator();
        o.type = 'sine';
        o.frequency.setValueAtTime(150, t);
        o.frequency.exponentialRampToValueAtTime(44, t + 0.11);
        const g = ctx.createGain();
        env(g, t, 0.95, 0.002, 0.04, 0.3);
        o.connect(g).connect(master);
        o.start(t);
        o.stop(t + 0.4);
        // Pump the pad and bass under the kick.
        ducked.gain.setValueAtTime(0.45, t);
        ducked.gain.linearRampToValueAtTime(1, t + 0.28);
    };

    const noiseHit = (t, type, freq, q, peak, release, reverbAmount) => {
        const src = ctx.createBufferSource();
        src.buffer = noise;
        const filter = ctx.createBiquadFilter();
        filter.type = type;
        filter.frequency.value = freq;
        filter.Q.value = q;
        const g = ctx.createGain();
        env(g, t, peak, 0.002, 0.005, release);
        src.connect(filter).connect(g).connect(master);
        if (reverbAmount) send(g, reverbAmount);
        src.start(t, (t * 0.37) % 1.5);
        src.stop(t + release + 0.05);
    };

    const clap = t => {
        noiseHit(t, 'bandpass', 1600, 0.9, 0.42, 0.16, 0.35);
        noiseHit(t + 0.012, 'bandpass', 1900, 1.2, 0.25, 0.1, 0);
    };
    const hat = (t, level = 0.11) => noiseHit(t, 'highpass', 7600, 0.7, level, 0.045, 0);

    const riser = (t, dur, peak) => {
        const src = ctx.createBufferSource();
        src.buffer = noise;
        src.loop = true;
        const filter = ctx.createBiquadFilter();
        filter.type = 'bandpass';
        filter.Q.value = 1.4;
        filter.frequency.setValueAtTime(350, t);
        filter.frequency.exponentialRampToValueAtTime(7000, t + dur);
        const g = ctx.createGain();
        g.gain.setValueAtTime(0.0001, t);
        g.gain.exponentialRampToValueAtTime(peak, t + dur);
        g.gain.exponentialRampToValueAtTime(0.0001, t + dur + 0.05);
        src.connect(filter).connect(g).connect(master);
        send(g, 0.3);
        src.start(t);
        src.stop(t + dur + 0.1);
    };

    const impact = t => {
        const o = ctx.createOscillator();
        o.type = 'sine';
        o.frequency.setValueAtTime(90, t);
        o.frequency.exponentialRampToValueAtTime(32, t + 0.8);
        const g = ctx.createGain();
        env(g, t, 0.9, 0.003, 0.1, 1.2);
        o.connect(g).connect(master);
        send(g, 0.4);
        o.start(t);
        o.stop(t + 1.5);
        noiseHit(t, 'lowpass', 3200, 0.5, 0.35, 0.9, 0.8);
    };

    // ---- arrangement ----
    for (let bar = 0; bar < 15; bar++) {
        const t = bar * BAR;
        const chord = CHORDS[bar % 4];
        const last = bar === 14;
        const intro = bar < 2;
        const lift = bar >= 12 && bar < 14;

        // Pad: dark in the intro, opening up through the groove, brightest in the lift.
        const cutoff = intro ? 650 + bar * 350 : lift ? 2600 : 1500;
        pad(t, last ? BAR * 0.95 : BAR, last ? [60, 64, 67, 72, 76] : chord.pad, last ? 1800 : cutoff);

        if (last) {
            bass(t, 36, 1.8);
            pluck(t, 72, 0.07);
            pluck(t + BEAT, 76, 0.05);
            pluck(t + BEAT * 2, 79, 0.04);
            continue;
        }

        // Arpeggio: 16ths from bar 1 (quiet), full from bar 2.
        if (bar >= 1) {
            const pattern = [0, 1, 2, 3, 2, 1, 2, 3];
            for (let s = 0; s < 16; s++) {
                const midi = chord.pad[pattern[s % 8]] + (s % 8 >= 4 ? 12 : 0);
                pluck(t + s * BEAT / 4, midi, bar === 1 ? 0.035 : lift ? 0.075 : 0.06);
            }
        }

        if (!intro) {
            for (let b = 0; b < 4; b++) {
                const bt = t + b * BEAT;
                kick(bt);
                if (b === 1 || b === 3) clap(bt);
                hat(bt + BEAT / 2);
                if (lift) hat(bt + BEAT / 4, 0.06);
                bass(bt, chord.bass, BEAT * 0.45);
                bass(bt + BEAT / 2, chord.bass, BEAT * 0.4);
            }
        }
    }

    // Accents on the two big moments: the version reveal at 2 s and the call to action at 24 s.
    riser(0.9, 1.1, 0.18);
    impact(2.0);
    riser(22.6, 1.4, 0.26);
    impact(24.0);
    // Small fill before the Showcase (18 s) to mark the change of scene.
    for (let i = 0; i < 4; i++) noiseHit(17.5 + i * BEAT / 4, 'bandpass', 1200 + i * 500, 1, 0.16 + i * 0.04, 0.08, 0.2);

    return ctx.startRendering();
}

/** Peak and loudness of a rendered buffer, in dBFS, to keep the track from clipping. */
export function musicStats(buffer) {
    let peak = 0;
    let sum = 0;
    let n = 0;
    for (let c = 0; c < buffer.numberOfChannels; c++) {
        const d = buffer.getChannelData(c);
        for (let i = 0; i < d.length; i++) {
            const v = Math.abs(d[i]);
            if (v > peak) peak = v;
            sum += v * v;
            n++;
        }
    }
    const db = v => +(20 * Math.log10(v + 1e-12)).toFixed(2);
    return { peakDb: db(peak), rmsDb: db(Math.sqrt(sum / n)) };
}
