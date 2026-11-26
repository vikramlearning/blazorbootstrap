// Renders the 4.1.0 promo videos and posters with headless Microsoft Edge. No downloads, no npm packages.
//   node render.mjs                 both videos and both posters, written next to this folder
//   node render.mjs --frames <dir>  still frames at key moments, for review
//   node render.mjs --only 16x9     one format
// Requires Node 22+ (built-in fetch and WebSocket) and Microsoft Edge (or set EDGE_PATH to Chrome).
import { spawn } from 'node:child_process';
import { createServer } from 'node:http';
import { createReadStream, existsSync, mkdirSync, mkdtempSync, rmSync, statSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { extname, join, normalize, resolve, sep } from 'node:path';
import { remuxFragmented } from './remux.mjs';

const here = import.meta.dirname;
const repoRoot = resolve(here, '../../../..');
const outDir = resolve(here, '..');
const args = process.argv.slice(2);
const framesDir = args.includes('--frames') ? resolve(args[args.indexOf('--frames') + 1]) : null;
const only = args.includes('--only') ? args[args.indexOf('--only') + 1] : null;
const formats = ['16x9', '1x1'].filter(f => !only || f === only);
const POSTER_TIME = 3.2;
const REVIEW_TIMES = [0.9, 2.6, 4.8, 7.0, 9.0, 11.0, 13.0, 15.4, 17.2, 19.5, 21.5, 23.4, 25.4, 28.0];

const edge = process.env.EDGE_PATH
    ?? ['C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', 'C:/Program Files/Microsoft/Edge/Application/msedge.exe'].find(existsSync);
if (!edge) throw new Error('Microsoft Edge not found; set EDGE_PATH.');

// Static file server for the repository, so the page and its images share one origin.
const types = { '.html': 'text/html', '.js': 'text/javascript', '.mjs': 'text/javascript', '.png': 'image/png', '.webp': 'image/webp', '.svg': 'image/svg+xml' };
const server = createServer((req, res) => {
    const path = normalize(join(repoRoot, decodeURIComponent(new URL(req.url, 'http://x').pathname)));
    if (!path.startsWith(repoRoot + sep) || !existsSync(path) || !statSync(path).isFile()) {
        res.writeHead(404).end();
        return;
    }
    res.writeHead(200, { 'Content-Type': types[extname(path)] ?? 'application/octet-stream' });
    createReadStream(path).pipe(res);
});
await new Promise(r => server.listen(0, '127.0.0.1', r));
const base = `http://127.0.0.1:${server.address().port}/nuget/release/4.1.0/promo/promo.html`;

const profile = mkdtempSync(join(tmpdir(), 'bb-promo-'));
const port = 9300 + Math.floor(Math.random() * 500);
const browser = spawn(edge, [
    '--headless=new', '--no-first-run', '--hide-scrollbars', '--autoplay-policy=no-user-gesture-required',
    '--disable-background-timer-throttling', '--disable-renderer-backgrounding', '--window-size=1920,1080',
    `--remote-debugging-port=${port}`, `--user-data-dir=${profile}`, 'about:blank'
], { stdio: 'ignore' });

const sleep = ms => new Promise(r => setTimeout(r, ms));
let target;
for (let i = 0; i < 100 && !target; i++) {
    await sleep(200);
    try { target = (await (await fetch(`http://127.0.0.1:${port}/json`)).json()).find(t => t.type === 'page'); } catch { }
}
const ws = new WebSocket(target.webSocketDebuggerUrl);
await new Promise(r => ws.addEventListener('open', r));
let id = 0;
const pending = new Map();
ws.addEventListener('message', e => {
    const m = JSON.parse(e.data);
    if (m.id && pending.has(m.id)) { pending.get(m.id)(m); pending.delete(m.id); }
});
const send = (method, params = {}) => new Promise(r => { const n = ++id; pending.set(n, r); ws.send(JSON.stringify({ id: n, method, params })); });
const evaluate = async expression => {
    const r = await send('Runtime.evaluate', { expression, awaitPromise: true, returnByValue: true });
    if (r.result.exceptionDetails) throw new Error(JSON.stringify(r.result.exceptionDetails));
    return r.result.result.value;
};
const readOut = async length => {
    const parts = [];
    for (let i = 0; i < length; i += 1_000_000) parts.push(await evaluate(`window.__out.slice(${i}, ${i + 1_000_000})`));
    return Buffer.from(parts.join(''), 'base64');
};

try {
    for (const format of formats) {
        await send('Page.navigate', { url: `${base}?format=${format}` });
        for (let i = 0; i < 300; i++) {
            await sleep(100);
            const state = await evaluate('window.promoError ?? (window.promoReady ? "ready" : "")');
            if (state === 'ready') break;
            if (state) throw new Error(state);
        }
        if (framesDir) {
            mkdirSync(framesDir, { recursive: true });
            for (const t of REVIEW_TIMES) {
                const file = join(framesDir, `${format}-${String(t).padStart(4, '0')}.png`);
                writeFileSync(file, await readOut(await evaluate(`window.renderStill(${t})`)));
            }
            console.log(`${format}: review frames in ${framesDir}`);
            continue;
        }
        const poster = join(outDir, `poster-${format}.png`);
        writeFileSync(poster, await readOut(await evaluate(`window.renderStill(${POSTER_TIME})`)));
        const music = await evaluate('window.musicStats');
        console.log(`${format}: music peak ${music.peakDb} dBFS, RMS ${music.rmsDb} dBFS`);
        if (music.peakDb > -1) throw new Error('The music clips: lower OUTPUT_TRIM in music.js.');
        console.log(`${format}: recording 30 s in real time...`);
        const length = await evaluate('window.renderVideo()');
        // MediaRecorder writes a fragmented MP4; rewrite it as a standard MP4 with a real duration.
        const { output, tracks } = remuxFragmented(await readOut(length));
        const video = join(outDir, `promo-${format}.mp4`);
        writeFileSync(video, output);
        console.log(`${format}: ${video} (${(output.length / 1e6).toFixed(1)} MB, ${await evaluate('window.__mime')}), ${poster}`);
        console.log(`${format}: tracks ${JSON.stringify(tracks)}`);
    }
} finally {
    ws.close();
    browser.kill();
    server.close();
    await sleep(500);
    try { rmSync(profile, { recursive: true, force: true }); } catch { }
}
