#!/usr/bin/env bash
# Re-render the puzzle pictures. Requires Manim Community and ffmpeg on PATH.
# SwapTube is not used: it needs a CUDA or HIP GPU.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
WORK="${TMPDIR:-/tmp}/manim-tem-tong-media"
mkdir -p "$WORK" "$ROOT/TemTong/Media"

manim render \
  --renderer cairo \
  -r 960,540 \
  --fps 24 \
  --media_dir "$WORK" \
  --disable_caching \
  "$ROOT/animations/trials.py" \
  Trial01 Trial02 Trial03 Trial04 Trial05 Trial06 \
  Trial07 Trial08 Trial09 Trial10 Trial10Reveal

copy_scene() {
  local scene="$1"
  local dest="$2"
  local src
  src="$(find "$WORK/videos" -type f -name "${scene}.mp4" | head -n 1)"
  if [[ -z "$src" ]]; then
    echo "Missing render for ${scene}" >&2
    exit 1
  fi
  ffmpeg -y -i "$src" -an -c:v libx264 -pix_fmt yuv420p -movflags +faststart -crf 26 -preset slow \
    "$ROOT/TemTong/Media/${dest}.mp4"
  ffmpeg -y -sseof -0.08 -i "$ROOT/TemTong/Media/${dest}.mp4" -update 1 -frames:v 1 \
    "$ROOT/TemTong/Media/${dest}.png"
}

copy_scene Trial01 trial-01
copy_scene Trial02 trial-02
copy_scene Trial03 trial-03
copy_scene Trial04 trial-04
copy_scene Trial05 trial-05
copy_scene Trial06 trial-06
copy_scene Trial07 trial-07
copy_scene Trial08 trial-08
copy_scene Trial09 trial-09
copy_scene Trial10 trial-10
copy_scene Trial10Reveal trial-10-reveal

echo "Wrote pictures to $ROOT/TemTong/Media"
