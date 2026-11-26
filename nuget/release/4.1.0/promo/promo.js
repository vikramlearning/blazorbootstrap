// Blazor Bootstrap 4.1.0 promo: scenes drawn on a canvas, timed to the music in music.js.
// Open promo.html?format=16x9 (1920x1080) or ?format=1x1 (1080x1080). Add &t=12.5 to show one still frame.
// render.mjs drives renderVideo() and renderStill() in headless Edge to write the MP4s and posters.
import { renderMusic, musicStats, DURATION, BEAT } from './music.js';

const params = new URLSearchParams(location.search);
const FORMAT = params.get('format') === '1x1' ? '1x1' : '16x9';
const SQUARE = FORMAT === '1x1';
const W = SQUARE ? 1080 : 1920;
const H = 1080;

const canvas = document.getElementById('stage');
canvas.width = W;
canvas.height = H;
const g = canvas.getContext('2d');

// The release identity from nuget/artwork: warm paper, ink type, violet paper ribbons.
const C = {
    paper: '#F3F1EC',
    ink: '#1E1A2E',
    muted: '#5D5870',
    violet: '#712CF9',
    deep: '#4C0BCE',
    lilac: '#B9A0FF',
    accent: '#FFE484',
    line: '#DED9E8',
    white: '#FFFFFF'
};
const FONT = '"Segoe UI Variable Display", "Segoe UI", system-ui, sans-serif';
const MONO = '"Cascadia Mono", "Cascadia Code", Consolas, monospace';

const ROOT = '../../../../';
const SHOT = name => `${ROOT}BlazorBootstrap.Demo.RCL/wwwroot/images/screenshots/${name}.png`;
const SITE = slug => `${ROOT}BlazorBootstrap.Demo.RCL/wwwroot/showcase/gallery/${slug}-light.webp`;

const FEATURED = [
    { name: 'Calendar', shot: 'calendar', url: 'calendar', text: 'Month, week, day and agenda views. Drag to move and resize events.' },
    { name: 'Timeline', shot: 'timeline', url: 'timeline', text: 'Events in order: vertical, horizontal or alternating.' },
    { name: 'Mind Map', shot: 'mind-map', url: 'mind-map', text: 'Collapsible trees that grow in any direction, fully keyboard friendly.' },
    { name: 'Table', shot: 'table', url: 'table', text: 'Styled Bootstrap 5.3 tables from a list, with captions and footers.' }
];
const MORE = ['Avatar', 'Rating Input', 'Navbar', 'List Group', 'Popover', 'Input Group', 'Button Group'];
const SITES = ['help-desk', 'hr-portal', 'monitoring', 'saas', 'fintech', 'healthcare'];
const MORE_SHOTS = ['navbar', 'avatar', 'rating-input', 'list-group'];

// ---------- helpers ----------
const clamp = (v, a = 0, b = 1) => Math.min(b, Math.max(a, v));
const prog = (t, a, b) => clamp((t - a) / (b - a));
const easeOut = p => 1 - Math.pow(1 - p, 3);
const easeInOut = p => (p < 0.5 ? 4 * p * p * p : 1 - Math.pow(-2 * p + 2, 3) / 2);
const easeBack = p => {
    const c = 1.5;
    return 1 + (c + 1) * Math.pow(p - 1, 3) + c * Math.pow(p - 1, 2);
};

const images = {};
function loadImage(key, src) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => { images[key] = img; resolve(); };
        img.onerror = () => reject(new Error(`Could not load ${src}`));
        img.src = src;
    });
}

function text(str, x, y, { size = 40, weight = 600, color = C.ink, alpha = 1, align = 'left', font = FONT, spacing = 0 } = {}) {
    if (alpha <= 0) return 0;
    g.save();
    g.globalAlpha *= alpha;
    g.font = `${weight} ${size}px ${font}`;
    g.letterSpacing = `${spacing}px`;
    g.fillStyle = color;
    g.textAlign = align;
    g.textBaseline = 'alphabetic';
    g.fillText(str, x, y);
    const width = g.measureText(str).width;
    g.restore();
    return width;
}

function measure(str, size, weight = 600, font = FONT, spacing = 0) {
    g.save();
    g.font = `${weight} ${size}px ${font}`;
    g.letterSpacing = `${spacing}px`;
    const width = g.measureText(str).width;
    g.restore();
    return width;
}

function wrap(str, maxWidth, size, weight = 400) {
    const words = str.split(' ');
    const lines = [];
    let line = '';
    for (const word of words) {
        const next = line ? `${line} ${word}` : word;
        if (line && measure(next, size, weight) > maxWidth) {
            lines.push(line);
            line = word;
        } else {
            line = next;
        }
    }
    if (line) lines.push(line);
    return lines;
}

function paragraph(str, x, y, maxWidth, opts) {
    const size = opts.size;
    const lines = wrap(str, maxWidth, size, opts.weight ?? 400);
    lines.forEach((line, i) => text(line, x, y + i * size * 1.32, opts));
    return lines.length;
}

function roundRect(x, y, w, h, r) {
    g.beginPath();
    g.roundRect(x, y, w, h, r);
}

function pill(label, x, y, { size = 34, fill = C.white, stroke = C.line, color = C.ink, alpha = 1, padX = 28, weight = 600 } = {}) {
    const w = measure(label, size, weight) + padX * 2;
    const h = size * 1.9;
    g.save();
    g.globalAlpha *= alpha;
    roundRect(x, y, w, h, h / 2);
    g.fillStyle = fill;
    g.fill();
    if (stroke) {
        g.lineWidth = 2;
        g.strokeStyle = stroke;
        g.stroke();
    }
    g.restore();
    text(label, x + padX, y + h / 2 + size * 0.36, { size, weight, color, alpha });
    return w;
}

// A browser window holding a real screenshot, as on the demo site's Showcase cards.
const frameBar = w => Math.round(w * 0.045) + 10;

function browserFrame(img, x, y, w, h, url, { alpha = 1, scroll = 0, shadow = true } = {}) {
    if (alpha <= 0) return;
    const bar = frameBar(w);
    const r = 16;
    g.save();
    g.globalAlpha *= alpha;
    if (shadow) {
        g.shadowColor = 'rgba(76, 11, 206, 0.22)';
        g.shadowBlur = 50;
        g.shadowOffsetY = 24;
    }
    roundRect(x, y, w, h, r);
    g.fillStyle = C.white;
    g.fill();
    g.shadowColor = 'transparent';
    // Title bar.
    g.save();
    roundRect(x, y, w, h, r);
    g.clip();
    g.fillStyle = '#F6F4FA';
    g.fillRect(x, y, w, bar);
    g.fillStyle = C.line;
    g.fillRect(x, y + bar - 2, w, 2);
    for (let i = 0; i < 3; i++) {
        g.beginPath();
        g.arc(x + 26 + i * 22, y + bar / 2, 6.5, 0, Math.PI * 2);
        g.fillStyle = ['#C9C2D8', '#D7D1E3', '#E2DDEC'][i];
        g.fill();
    }
    if (url) {
        const size = Math.max(16, Math.round(bar * 0.36));
        const uw = Math.min(w * 0.6, measure(url, size, 400) + 40);
        roundRect(x + (w - uw) / 2, y + bar * 0.2, uw, bar * 0.6, bar * 0.3);
        g.fillStyle = C.white;
        g.fill();
        text(url, x + w / 2, y + bar * 0.5 + size * 0.36, { size, weight: 400, color: C.muted, align: 'center' });
    }
    // Page content: the screenshot at the frame's width, scrolled by `scroll` (0..1 of its overflow).
    const cw = w;
    const ch = h - bar;
    const scale = cw / img.width;
    const ih = img.height * scale;
    const overflow = Math.max(0, ih - ch);
    g.beginPath();
    g.rect(x, y + bar, cw, ch);
    g.clip();
    g.fillStyle = C.white;
    g.fillRect(x, y + bar, cw, ch);
    g.drawImage(img, x, y + bar - overflow * scroll, cw, ih);
    g.restore();
    g.lineWidth = 2;
    g.strokeStyle = C.line;
    roundRect(x, y, w, h, r);
    g.stroke();
    g.restore();
}

function check(x, y, s, color, p) {
    if (p <= 0) return;
    g.save();
    g.lineWidth = s * 0.16;
    g.lineCap = 'round';
    g.lineJoin = 'round';
    g.strokeStyle = color;
    g.beginPath();
    g.arc(x + s / 2, y + s / 2, s / 2, 0, Math.PI * 2);
    g.fillStyle = color;
    g.globalAlpha = 0.14 * p;
    g.fill();
    g.globalAlpha = p;
    g.beginPath();
    g.moveTo(x + s * 0.28, y + s * 0.52);
    g.lineTo(x + s * 0.44, y + s * 0.68);
    g.lineTo(x + s * 0.73, y + s * 0.34);
    g.stroke();
    g.restore();
}

// The violet paper ribbons from the release artwork, drawn as vectors so they stay sharp at any size.
// `p` is 0 off screen (bottom right) to 1 in place.
function ribbons(p) {
    if (p <= 0) return;
    const s = SQUARE ? 0.82 : 1;
    const ox = (1 - easeInOut(p)) * W * 0.7;
    const oy = (1 - easeInOut(p)) * H * 0.75;
    const bands = [
        { w: 210, a: [0.60, 1.12], c1: [0.64, 0.40], c2: [0.80, 0.08], b: [1.06, -0.04], from: C.violet, to: C.deep },
        { w: 150, a: [0.82, 1.14], c1: [0.80, 0.70], c2: [0.86, 0.30], b: [1.02, 0.22], from: '#8A4DFF', to: C.violet }
    ];
    g.save();
    // On the square video the ribbons sit lower and further right, clear of the stacked text.
    g.translate(ox + (SQUARE ? W * 0.1 : 0), oy + (SQUARE ? H * 0.34 : 0));
    for (const band of bands) {
        const pt = ([fx, fy]) => [W - (1 - fx) * W * (SQUARE ? 1.15 : 1) * s, fy * H];
        const [ax, ay] = pt(band.a);
        const [c1x, c1y] = pt(band.c1);
        const [c2x, c2y] = pt(band.c2);
        const [bx, by] = pt(band.b);
        const grad = g.createLinearGradient(ax, ay, bx, by);
        grad.addColorStop(0, band.to);
        grad.addColorStop(1, band.from);
        g.lineCap = 'round';
        g.shadowColor = 'rgba(40, 8, 110, 0.35)';
        g.shadowBlur = 60;
        g.shadowOffsetX = -18;
        g.shadowOffsetY = 26;
        g.lineWidth = band.w * s;
        g.strokeStyle = grad;
        g.beginPath();
        g.moveTo(ax, ay);
        g.bezierCurveTo(c1x, c1y, c2x, c2y, bx, by);
        g.stroke();
        // Lit paper edge.
        g.shadowColor = 'transparent';
        g.lineWidth = 6 * s;
        g.strokeStyle = 'rgba(255, 255, 255, 0.18)';
        g.beginPath();
        g.moveTo(ax - band.w * 0.42 * s, ay);
        g.bezierCurveTo(c1x - band.w * 0.42 * s, c1y, c2x - band.w * 0.3 * s, c2y, bx - band.w * 0.2 * s, by);
        g.stroke();
    }
    g.restore();
}

// Enter/exit envelope for a scene: 0 before, 1 while shown, back to 0 at the end.
function life(t, start, end, inDur = 0.4, outDur = 0.25) {
    return { in: easeOut(prog(t, start, start + inDur)), out: easeInOut(prog(t, end - outDur, end)) };
}

// ---------- layout ----------
const L = SQUARE
    ? { x: 90, textW: 900, top: 150, media: { x: 90, y: 470, w: 900, h: 560 } }
    : { x: 130, textW: 640, top: 300, media: { x: 840, y: 170, w: 960, h: 760 } };

// ---------- scenes ----------
function sceneIntro(t) {
    const out = easeInOut(prog(t, 3.6, 4.0));
    const a = 1 - out;
    const logoIn = easeOut(prog(t, 0.15, 0.8));
    const y0 = SQUARE ? 200 : 280;
    g.save();
    g.globalAlpha = logoIn * a;
    const ls = SQUARE ? 84 : 96;
    g.drawImage(images.logo, L.x, y0 - ls * 0.78 + (1 - logoIn) * 20, ls, ls);
    g.restore();
    text('Blazor Bootstrap', L.x + (SQUARE ? 104 : 118), y0 + (1 - logoIn) * 20, { size: SQUARE ? 52 : 58, weight: 600, alpha: logoIn * a });

    // The version lands on the downbeat at 2 s, with the impact in the music.
    const pop = prog(t, 2.0, 2.45);
    if (pop > 0) {
        const scale = 0.86 + 0.14 * easeBack(pop);
        const size = SQUARE ? 280 : 330;
        g.save();
        g.translate(L.x, y0 + (SQUARE ? 300 : 360));
        g.scale(scale, scale);
        text('4.1.0', -10, 0, { size, weight: 700, spacing: -10, alpha: clamp(pop * 3) * a });
        g.restore();
    }
    const chip = easeOut(prog(t, 2.45, 2.9));
    pill('Now on NuGet', L.x, y0 + (SQUARE ? 370 : 430) + (1 - chip) * 16, { size: SQUARE ? 36 : 40, fill: C.accent, stroke: null, alpha: chip * a });
    paragraph('Free, open-source Bootstrap 5.3 components for Blazor', L.x, y0 + (SQUARE ? 500 : 600), SQUARE ? 520 : 900, {
        size: SQUARE ? 34 : 36, weight: 400, color: C.muted, alpha: easeOut(prog(t, 0.6, 1.2)) * a
    });
}

function sceneNew(t) {
    const { in: i, out } = life(t, 4.0, 6.0, 0.35, 0.25);
    const a = i * (1 - out);
    if (a <= 0) return;
    const y = SQUARE ? 330 : 420;
    const size = SQUARE ? 150 : 170;
    text('11 new', L.x, y + (1 - i) * 40, { size, weight: 700, spacing: -4, alpha: a });
    text('components', L.x, y + size * 1.02 + (1 - i) * 60, { size, weight: 700, spacing: -4, alpha: a });
    // Eleven tiles light up on the 16th notes, one per component.
    const tile = SQUARE ? 62 : 72;
    const gap = SQUARE ? 16 : 20;
    const ty = y + size * 1.02 + (SQUARE ? 80 : 90);
    for (let k = 0; k < 11; k++) {
        const on = easeOut(prog(t, 4.25 + k * BEAT / 4, 4.25 + k * BEAT / 4 + 0.18));
        g.save();
        g.globalAlpha = a;
        roundRect(L.x + k * (tile + gap), ty, tile, tile, 14);
        g.fillStyle = on > 0 ? C.violet : C.line;
        g.globalAlpha = a * (0.25 + 0.75 * on);
        g.fill();
        g.restore();
    }
    const subY = ty + tile + (SQUARE ? 90 : 100);
    text('Plus a better Grid, 11 sample websites and .NET 11', L.x, subY, { size: SQUARE ? 34 : 40, weight: 400, color: C.muted, alpha: a * easeOut(prog(t, 4.5, 5.0)) });
}

function sceneFeature(t, item, start) {
    const end = start + 2;
    const { in: i, out } = life(t, start, end, 0.4, 0.25);
    const a = i * (1 - out);
    if (a <= 0) return;
    const shift = -out * 60;
    const nameSize = SQUARE ? 96 : 116;
    const top = SQUARE ? 140 : 360;
    text('New component', L.x + shift, top, { size: SQUARE ? 30 : 34, weight: 600, color: C.violet, alpha: a });
    text(item.name, L.x + shift, top + nameSize * 1.05 + (1 - i) * 30, { size: nameSize, weight: 700, spacing: -3, alpha: a });
    paragraph(item.text, L.x + shift, top + nameSize * 1.05 + (SQUARE ? 70 : 90), L.textW, { size: SQUARE ? 34 : 38, weight: 400, color: C.muted, alpha: a * easeOut(prog(t, start + 0.15, start + 0.55)) });
    const m = L.media;
    const img = images[item.shot];
    const fh = Math.min(m.h, frameBar(m.w) + img.height * (m.w / img.width));
    browserFrame(img, m.x + (1 - i) * 140 + shift, m.y + (m.h - fh) / 2, m.w, fh, `demos.blazorbootstrap.com/demos/${item.url}`, {
        alpha: a,
        scroll: easeInOut(prog(t, start + 0.5, end))
    });
}

function sceneMore(t) {
    const { in: i, out } = life(t, 14.0, 16.0, 0.35, 0.25);
    const a = i * (1 - out);
    if (a <= 0) return;
    const top = SQUARE ? 190 : 230;
    text('And seven more', L.x, top + (1 - i) * 30, { size: SQUARE ? 104 : 120, weight: 700, spacing: -3, alpha: a });
    // Names pop in on the 16th notes, so all seven are on screen for most of the scene.
    const size = SQUARE ? 38 : 44;
    const maxX = W - L.x;
    let x = L.x;
    let y = top + (SQUARE ? 60 : 70);
    MORE.forEach((name, k) => {
        const w = measure(name, size, 600) + 56;
        if (x + w > maxX) {
            x = L.x;
            y += size * 1.9 + 18;
        }
        const at = 14.2 + k * BEAT / 4;
        const appear = easeBack(prog(t, at, at + 0.3));
        pill(name, x, y + (1 - clamp(appear)) * 24, { size, alpha: a * clamp(appear * 1.5), fill: C.white, stroke: C.lilac });
        x += w + 18;
    });
    // A strip of real screenshots under the names.
    const shots = SQUARE ? MORE_SHOTS.slice(0, 2) : MORE_SHOTS;
    const gap = 26;
    const fw = (W - L.x * 2 - gap * (shots.length - 1)) / shots.length;
    const fh = SQUARE ? 400 : 330;
    const fy = SQUARE ? 620 : 640;
    shots.forEach((shot, k) => {
        const p = easeOut(prog(t, 14.45 + k * 0.12, 14.85 + k * 0.12));
        browserFrame(images[shot], L.x + k * (fw + gap), fy + (1 - p) * 60, fw, fh, null, { alpha: a * p });
    });
}

function sceneGrid(t) {
    const { in: i, out } = life(t, 16.0, 18.0, 0.4, 0.25);
    const a = i * (1 - out);
    if (a <= 0) return;
    const top = SQUARE ? 140 : 340;
    text('Grid', L.x, top + (1 - i) * 30, { size: SQUARE ? 96 : 116, weight: 700, spacing: -3, alpha: a });
    const lines = ['Column chooser', 'CSV export', 'Keyboard-accessible sorting'];
    const size = SQUARE ? 36 : 42;
    lines.forEach((line, k) => {
        const p = easeOut(prog(t, 16.3 + k * BEAT, 16.3 + k * BEAT + 0.3));
        const lx = SQUARE ? L.x + k * 300 : L.x;
        const ly = SQUARE ? top + 90 : top + 110 + k * (size * 1.8);
        if (SQUARE && k === 2) return;
        check(lx, ly - size * 0.9, size * 1.1, C.violet, p * a);
        text(line, lx + size * 1.5, ly, { size, weight: 600, alpha: p * a });
    });
    if (SQUARE) {
        const p = easeOut(prog(t, 16.3 + 2 * BEAT, 16.6 + 2 * BEAT));
        check(L.x, top + 160 - size * 0.9, size * 1.1, C.violet, p * a);
        text(lines[2], L.x + size * 1.5, top + 160, { size, weight: 600, alpha: p * a });
    }
    const m = SQUARE ? { ...L.media, y: L.media.y + 20, h: L.media.h - 20 } : L.media;
    browserFrame(images.grid, m.x + (1 - i) * 140, m.y + (SQUARE ? 0 : 90), m.w, SQUARE ? m.h : 600, 'demos.blazorbootstrap.com/demos/grid/column-chooser', { alpha: a });
}

function sceneShowcase(t) {
    const { in: i, out } = life(t, 18.0, 22.0, 0.45, 0.3);
    const a = i * (1 - out);
    if (a <= 0) return;
    // Drifting collage of Showcase sites behind the headline's column.
    const fw = SQUARE ? 470 : 560;
    const fh = fw * 0.62;
    const gap = 34;
    const drift = (t - 18) * (SQUARE ? 26 : 34);
    const baseX = SQUARE ? 70 : 840;
    const baseY = SQUARE ? 420 : 90;
    SITES.forEach((slug, k) => {
        const col = k % 2;
        const row = Math.floor(k / 2);
        const stagger = easeOut(prog(t, 18.0 + k * 0.12, 18.6 + k * 0.12));
        const x = baseX + col * (fw + gap) + (row % 2) * (SQUARE ? 0 : 60) - drift * (col ? 0.6 : 1);
        const y = baseY + row * (fh + gap) - (SQUARE ? drift * 0.5 : drift) + (1 - stagger) * 80;
        browserFrame(images[`site-${slug}`], x, y, fw, fh, null, { alpha: a * stagger, shadow: true });
    });
    // Headline panel on paper, so the text stays readable over the collage.
    if (!SQUARE) {
        const grad = g.createLinearGradient(0, 0, 820, 0);
        grad.addColorStop(0, C.paper);
        grad.addColorStop(0.8, C.paper);
        grad.addColorStop(1, 'rgba(243, 241, 236, 0)');
        g.save();
        g.globalAlpha = a;
        g.fillStyle = grad;
        g.fillRect(0, 0, 820, H);
        g.restore();
    } else {
        const grad = g.createLinearGradient(0, 0, 0, 440);
        grad.addColorStop(0, C.paper);
        grad.addColorStop(0.85, C.paper);
        grad.addColorStop(1, 'rgba(243, 241, 236, 0)');
        g.save();
        g.globalAlpha = a;
        g.fillStyle = grad;
        g.fillRect(0, 0, W, 440);
        g.restore();
    }
    const top = SQUARE ? 150 : 400;
    const size = SQUARE ? 104 : 120;
    text('11 sample', L.x, top + (1 - i) * 30, { size, weight: 700, spacing: -3, alpha: a });
    text('websites', L.x, top + size * 1.02 + (1 - i) * 40, { size, weight: 700, spacing: -3, alpha: a });
    const sub = easeOut(prog(t, 18.5, 19.0));
    paragraph('Built only with Blazor Bootstrap components.', L.x, top + size * 1.02 + (SQUARE ? 70 : 90), SQUARE ? 900 : 620, { size: SQUARE ? 32 : 38, weight: 400, color: C.muted, alpha: a * sub });
}

function sceneDotnet(t) {
    const { in: i, out } = life(t, 22.0, 24.0, 0.35, 0.3);
    const a = i * (1 - out);
    if (a <= 0) return;
    const top = SQUARE ? 320 : 380;
    const size = SQUARE ? 100 : 130;
    text('One package for', L.x, top + (1 - i) * 30, { size, weight: 700, spacing: -3, alpha: a });
    const versions = ['.NET 8', '.NET 9', '.NET 10', '.NET 11'];
    const ps = SQUARE ? 52 : 72;
    let x = L.x;
    const y = top + (SQUARE ? 60 : 80);
    versions.forEach((v, k) => {
        const at = 22.15 + k * BEAT / 2;
        const p = easeBack(prog(t, at, at + 0.3));
        const latest = k === 3;
        const w = pill(v, x, y + (1 - clamp(p)) * 24, {
            size: ps, weight: 700, padX: SQUARE ? 30 : 40,
            fill: latest ? C.violet : C.white, stroke: latest ? null : C.lilac, color: latest ? C.white : C.ink,
            alpha: a * clamp(p * 1.5)
        });
        x += w + (SQUARE ? 18 : 26);
    });
    text('Free and open source. No breaking API changes.', L.x, y + ps * 1.9 + (SQUARE ? 90 : 120), { size: SQUARE ? 34 : 44, weight: 400, color: C.muted, alpha: a * easeOut(prog(t, 22.9, 23.3)) });
}

function sceneCta(t) {
    const i = easeOut(prog(t, 24.1, 24.6));
    if (i <= 0) return;
    const y0 = SQUARE ? 180 : 250;
    const ls = SQUARE ? 84 : 96;
    g.save();
    g.globalAlpha = i;
    g.drawImage(images.logo, L.x, y0 - ls * 0.78, ls, ls);
    g.restore();
    text('Blazor Bootstrap 4.1.0', L.x + (SQUARE ? 104 : 118), y0, { size: SQUARE ? 52 : 58, weight: 600, alpha: i });

    const head = easeOut(prog(t, 24.25, 24.75));
    const hs = SQUARE ? 112 : 128;
    text('Get started', L.x, y0 + hs * 1.25 + (1 - head) * 30, { size: hs, weight: 700, spacing: -4, alpha: head });

    // The install command types out on the 16th notes.
    const cmd = 'dotnet add package Blazor.Bootstrap';
    const typed = Math.floor(clamp(prog(t, 24.6, 26.0)) * cmd.length);
    const cs = SQUARE ? 36 : 42;
    const boxY = y0 + hs * 1.25 + (SQUARE ? 70 : 80);
    const boxW = measure(cmd, cs, 500, MONO) + 120;
    const box = easeOut(prog(t, 24.45, 24.9));
    g.save();
    g.globalAlpha = box;
    roundRect(L.x, boxY, boxW, cs * 2.3, 18);
    g.fillStyle = C.ink;
    g.fill();
    g.restore();
    text('>', L.x + 34, boxY + cs * 1.5, { size: cs, weight: 600, font: MONO, color: C.lilac, alpha: box });
    text(cmd.slice(0, typed), L.x + 34 + cs * 1.1, boxY + cs * 1.5, { size: cs, weight: 500, font: MONO, color: C.white, alpha: box });
    if (box > 0 && Math.floor(t / (BEAT / 2)) % 2 === 0) {
        const cx = L.x + 34 + cs * 1.1 + measure(cmd.slice(0, typed), cs, 500, MONO) + 4;
        g.save();
        g.globalAlpha = box * 0.8;
        g.fillStyle = C.accent;
        g.fillRect(cx, boxY + cs * 0.55, cs * 0.55, cs * 1.15);
        g.restore();
    }
    const link = easeOut(prog(t, 26.1, 26.6));
    text('demos.blazorbootstrap.com', L.x, boxY + cs * 2.3 + (SQUARE ? 90 : 110), { size: SQUARE ? 48 : 56, weight: 700, color: C.violet, alpha: link });
}

export function draw(t) {
    g.setTransform(1, 0, 0, 1, 0, 0);
    g.globalAlpha = 1;
    g.fillStyle = C.paper;
    g.fillRect(0, 0, W, H);
    // Ribbons hold the intro and the call to action, and sweep away for the content in between.
    const ribbonIn = t < 12 ? easeOut(prog(t, 0, 1.1)) * (1 - prog(t, 3.5, 4.1)) : prog(t, 23.7, 24.5);
    ribbons(ribbonIn);
    sceneIntro(t);
    sceneNew(t);
    FEATURED.forEach((item, k) => sceneFeature(t, item, 6 + k * 2));
    sceneMore(t);
    sceneGrid(t);
    sceneShowcase(t);
    sceneDotnet(t);
    sceneCta(t);
}

// ---------- boot, preview and recording ----------
let musicBuffer;

async function boot() {
    await document.fonts.load(`700 100px ${FONT}`);
    await document.fonts.load(`500 40px ${MONO}`);
    const loads = [loadImage('logo', `${ROOT}nuget/artwork/blazor-bootstrap-logo.png`), loadImage('grid', SHOT('grid'))];
    FEATURED.forEach(f => loads.push(loadImage(f.shot, SHOT(f.shot))));
    SITES.forEach(s => loads.push(loadImage(`site-${s}`, SITE(s))));
    MORE_SHOTS.forEach(s => loads.push(loadImage(s, SHOT(s))));
    await Promise.all(loads);
    musicBuffer = await renderMusic(48000);
    window.musicStats = musicStats(musicBuffer);
    const still = params.get('t');
    draw(still ? Number(still) : 2.8);
    window.promoReady = true;
}

function toBase64(bytes) {
    let binary = '';
    const chunk = 0x8000;
    for (let i = 0; i < bytes.length; i += chunk) binary += String.fromCharCode.apply(null, bytes.subarray(i, i + chunk));
    return btoa(binary);
}

/** Draws one frame and keeps it as a base64 PNG in window.__out. Returns its length. */
window.renderStill = t => {
    draw(t);
    window.__out = canvas.toDataURL('image/png').split(',')[1];
    return window.__out.length;
};

/** Plays the whole promo in real time and records it. Keeps the MP4 as base64 in window.__out. */
window.renderVideo = async () => {
    const audio = new AudioContext({ sampleRate: 48000 });
    await audio.resume();
    const dest = audio.createMediaStreamDestination();
    const source = audio.createBufferSource();
    source.buffer = musicBuffer;
    source.connect(dest);
    const stream = canvas.captureStream(30);
    stream.addTrack(dest.stream.getAudioTracks()[0]);
    const types = [
        'video/mp4;codecs=avc1.640028,mp4a.40.2',
        'video/mp4;codecs=avc1.4d0028,mp4a.40.2',
        'video/mp4;codecs=avc1.42E01E,mp4a.40.2'
    ];
    const mimeType = types.find(type => MediaRecorder.isTypeSupported(type));
    const recorder = new MediaRecorder(stream, { mimeType, videoBitsPerSecond: SQUARE ? 4_500_000 : 6_000_000, audioBitsPerSecond: 192_000 });
    const chunks = [];
    recorder.ondataavailable = e => e.data.size && chunks.push(e.data);
    const stopped = new Promise(resolve => (recorder.onstop = resolve));

    draw(0);
    recorder.start(1000);
    const startAt = audio.currentTime + 0.25;
    source.start(startAt);
    await new Promise(resolve => {
        const frame = () => {
            const t = audio.currentTime - startAt;
            draw(clamp(t, 0, DURATION));
            if (t < DURATION + 0.15) requestAnimationFrame(frame);
            else resolve();
        };
        requestAnimationFrame(frame);
    });
    recorder.stop();
    await stopped;
    await audio.close();
    const bytes = new Uint8Array(await new Blob(chunks, { type: mimeType }).arrayBuffer());
    window.__out = toBase64(bytes);
    window.__mime = mimeType;
    return window.__out.length;
};

boot().catch(error => {
    window.promoError = String(error);
    console.error(error);
});
