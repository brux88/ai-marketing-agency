import sharp from "sharp";
import { writeFile, mkdir } from "node:fs/promises";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const assetsDir = join(__dirname, "..", "assets");

// Brand tokens (sRGB approximations of the web oklch tokens — matches brand.dart)
const INK = "#222222";
const PAPER = "#FBFAF5";
const LIME = "#BFE836";

// Full-bleed ink square icon with white chat bubble + lime cursor.
// Based on frontend/public/brand/icon-mark-filled.svg (scaled to 1024 viewBox).
function filledSvg() {
  // original viewBox 120 120, rx 24 → in 1024 that's rx 204.8
  // stroke-width 6 → 51.2
  // we round for cleanliness
  return `<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 120 120">
  <rect width="120" height="120" rx="24" fill="${INK}"/>
  <path d="M24 32 h72 a6 6 0 0 1 6 6 v40 a6 6 0 0 1 -6 6 h-32 l-14 14 v-14 h-26 a6 6 0 0 1 -6 -6 v-40 a6 6 0 0 1 6 -6 z"
        fill="none" stroke="${PAPER}" stroke-width="6" stroke-linejoin="round"/>
  <rect x="58" y="44" width="6" height="28" fill="${LIME}"/>
</svg>`;
}

// Android adaptive foreground: artwork lives in the inner ~66% safe zone.
// Rendered on a 200-unit canvas with the bubble centered (40 padding per side),
// drawn as a paper stroke on transparent so the adaptive ink background shows
// through. Matches the filled icon look.
function foregroundSvg() {
  return `<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 200 200">
  <g transform="translate(40,40)">
    <path d="M24 32 h72 a6 6 0 0 1 6 6 v40 a6 6 0 0 1 -6 6 h-32 l-14 14 v-14 h-26 a6 6 0 0 1 -6 -6 v-40 a6 6 0 0 1 6 -6 z"
          fill="none" stroke="${PAPER}" stroke-width="6" stroke-linejoin="round"/>
    <rect x="58" y="44" width="6" height="28" fill="${LIME}"/>
  </g>
</svg>`;
}

// Splash image — just the bubble on transparent (flutter_native_splash sets the bg).
function splashSvg() {
  return `<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 120 120">
  <path d="M24 32 h72 a6 6 0 0 1 6 6 v40 a6 6 0 0 1 -6 6 h-32 l-14 14 v-14 h-26 a6 6 0 0 1 -6 -6 v-40 a6 6 0 0 1 6 -6 z"
        fill="${INK}" stroke="${INK}" stroke-width="6" stroke-linejoin="round"/>
  <rect x="58" y="44" width="6" height="28" fill="${LIME}"/>
</svg>`;
}

async function render(svg, out, size) {
  await sharp(Buffer.from(svg), { density: 400 })
    .resize(size, size)
    .png()
    .toFile(out);
  console.log("wrote", out);
}

await mkdir(assetsDir, { recursive: true });
await render(filledSvg(), join(assetsDir, "app_icon.png"), 1024);
await render(foregroundSvg(), join(assetsDir, "app_icon_foreground.png"), 1024);
await render(splashSvg(), join(assetsDir, "splash.png"), 512);
