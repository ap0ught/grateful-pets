#!/usr/bin/env bash
set -euo pipefail

# Install/update the Grateful Pets mod into the local Caves of Qud Mods folder.
# Usage: ./install.sh [--dest /path/to/Mods]
# Env:   QUD_MODS_DIR overrides the Mods directory.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEFAULT_MODS_DIR="${QUD_MODS_DIR:-$HOME/.config/unity3d/Freehold Games/CavesOfQud/Mods}"
DEST_BASE="$DEFAULT_MODS_DIR"

if [[ "${1:-}" == "--dest" && -n "${2:-}" ]]; then
  DEST_BASE="$2"
fi

SRC="$SCRIPT_DIR/mod"
DEST="$DEST_BASE/GratefulPets"

if [[ ! -f "$SRC/manifest.json" || ! -d "$SRC/Scripts" ]]; then
  echo "error: expected $SRC/{manifest.json,Scripts/} (run from repo root)" >&2
  exit 1
fi

mkdir -p "$DEST"
rsync -a --delete "$SRC/" "$DEST/"
echo "Installed Grateful Pets -> $DEST"
echo "Restart Caves of Qud to recompile, then check: grep -B1 -A6 '=== GRATEFUL PETS ===' \"$HOME/.config/unity3d/Freehold Games/CavesOfQud/build_log.txt\" | tail -8"
