# Blazor Bootstrap 4.1.0 release kit

Everything needed to announce 4.1.0. Nothing here is published automatically.

| File | What it is | Where it goes |
|---|---|---|
| `github-release.md` | Release notes for the GitHub release body | GitHub release `v4.1.0` |
| `social-posts.md` | An X thread of four posts, a LinkedIn post, and alt text for the video | X (@blazorbootstrap), LinkedIn |
| `promo-16x9.mp4` | 30 s promo, 1920 × 1080, H.264 and AAC, with background music | GitHub release asset, X and LinkedIn on desktop |
| `promo-1x1.mp4` | The same promo, 1080 × 1080 | X and LinkedIn mobile feeds |
| `poster-16x9.png`, `poster-1x1.png` | Still frames of the version reveal, for thumbnails or image-only posts | Video thumbnails, link previews |
| `promo/` | The generator for both videos and posters | Re-render for the next release |

The full release notes are the draft docs post `docs/blog/2026-11-26-blazorbootstrap-4.1.0.md` (URL `/blog/blazorbootstrap-4.1.0`), next to the draft upgrade guide `docs/blog/2026-11-22-upgrade-to-v4.1.0.md` (URL `/blog/upgrade-to-v4.1.0`). Both use a `slug`, so their URLs don't change when you set the release date.

## Release checklist

1. Publish the `Blazor.Bootstrap` 4.1.0 package to NuGet and tag `v4.1.0`.
2. In both docs posts, rename the file to the release date and remove `draft: true`; deploy the docs.
3. Create the GitHub release `v4.1.0` from `github-release.md` and attach `promo-16x9.mp4`.
4. Deploy the demo site; its home page release notice now reads "Calendar, Timeline, Showcase & .NET 11".
5. Post on X and LinkedIn from `social-posts.md`, with the video and its alt text.

## The promo video

The video is drawn on a canvas in `promo/promo.js`, from the real demo screenshots in `BlazorBootstrap.Demo.RCL/wwwroot/images/screenshots/` and the Showcase gallery images. The music in `promo/music.js` is an original 120 BPM track synthesized with Web Audio, so it needs no license; scene cuts land on its beats. There is no narration, and every highlight is on screen as text, because X and LinkedIn autoplay video muted.

Scenes (seconds): version reveal 0 to 4, "11 new components" 4 to 6, Calendar, Timeline, Mind Map and Table 6 to 14, the seven other new components 14 to 16, Grid 16 to 18, Showcase 18 to 22, .NET 8 to 11 22 to 24, install command and demo site 24 to 30.

### Re-render

Requires Node 22 or later and Microsoft Edge (set `EDGE_PATH` to use Chrome). No npm packages or downloads.

```shell
cd nuget/release/4.1.0/promo
node render.mjs                      # both videos and posters, about 1 minute each (recorded in real time)
node render.mjs --only 1x1           # one format
node render.mjs --frames ./frames    # still frames at key moments, to review layout before recording
```

`render.mjs` serves the repository over a local HTTP port, records the canvas and music with `MediaRecorder` in headless Edge, then `remux.mjs` rewrites Edge's fragmented MP4 as a standard MP4 with a real duration, so players and social networks show and seek it correctly. The render stops if the music would clip (peak above -1 dBFS).

To preview in a browser, serve the repository root over HTTP (for example `npx http-server` from the repository root) and open `/nuget/release/4.1.0/promo/promo.html?format=16x9&t=7` for a still at 7 seconds.

For the next release, copy this folder, then update the version, the `FEATURED`, `MORE` and `SITES` lists, and the scene copy in `promo.js`.
