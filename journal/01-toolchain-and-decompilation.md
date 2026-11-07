# 01 — Toolchain & decompilation

## Why decompile at all

The [modding wiki](https://wiki.cavesofqud.com/wiki/Modding:Overview) is good for concepts but thin
on exact signatures. Guessing a signature costs a full game launch to get a `CS` error back.
Reading the actual game code costs one `grep`. Everything else in this journal follows from having
the source readable.

## Installing the tools

The machine had no .NET SDK. Installed with the official installer:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/opencode/dotnet-install.sh
chmod +x /tmp/opencode/dotnet-install.sh
/tmp/opencode/dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
# → installed10.0.403

export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
dotnet tool install -g ilspycmd      # →11.2.0.10050
```

> **Gotcha:** the `PATH` + `DOTNET_ROOT` exports are *per shell*. Every new shell needs them
> re-exported or `dotnet`/`ilspycmd` will not be found.

## Where everything lives

| Thing | Path |
|---|---|
| Game install | `~/.steam/steam/steamapps/common/Caves of Qud/` |
| Assembly to decompile | `CoQ_Data/Managed/Assembly-CSharp.dll` (11.8 MB) |
| Game XML data | `CoQ_Data/StreamingAssets/Base/**` and `StreamingAssets/DLC/**` |
| Game config / logs | `~/.config/unity3d/Freehold Games/CavesOfQud/` |
| **Mod compile log** | `…/CavesOfQud/build_log.txt` (rotates to `.prev`) |
| Compiled mod DLLs | `…/CavesOfQud/ModAssemblies/` |
| Local mods | `…/CavesOfQud/Mods/` |
| Options incl. `OptionAllowCSMods` | `…/CavesOfQud/Local/PlayerOptions.json` |
| Saves | `…/CavesOfQud/Synced/Saves/<uuid>/` |

## Three useful `ilspycmd` invocations

```bash
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"; export DOTNET_ROOT="$HOME/.dotnet"
DLL="$HOME/.steam/steam/steamapps/common/Caves of Qud/CoQ_Data/Managed/Assembly-CSharp.dll"

# 1. one type, as C# source
ilspycmd -t XRL.World.Parts.Pettable "$DLL"

# 2. list every class (7072 entries → types.txt) — fastest way to answer
#    "does the game even have a thing for X?"
ilspycmd -l c "$DLL" > /tmp/opencode/qud/types.txt

# 3. THE WHOLE ASSEMBLY as a project (38 MB, 5436 .cs files) — this is what makes
#    "grep for every caller of X" possible, which single-type decompiling cannot do.
ilspycmd -p -o /tmp/opencode/qud/decomp "$DLL"
```

**Lesson:** steps 1–2 answer *"does this API exist?"*. Step 3 answers *"how is this API actually
used, and who calls it?"* — which is the question that actually matters when you're wiring up an
event pipeline. Almost every non-obvious fact in this journal came from grepping `decomp/`.

## Reading the game's own data

Blueprints are plain XML:

```bash
G=~/.steam/steam/steamapps/common/Caves of Qud/CoQ_Data/StreamingAssets/Base
grep -n 'Name="Desalination Pellet"' $G/ObjectBlueprints/Items.xml
grep -oE '<object Name="[^"]*"' $G/ObjectBlueprints/Items.xml   # inventory of everything
```

A quick Python pass over every blueprint (splitting `<object …>` blocks and following `Inherits`)
is how the [list of 34 pet blueprints](05-pets-and-companions.md) was produced. Base game *and*
DLC are under `StreamingAssets/`, so both must be scanned.

## Notes on the decompiled output

- `decomp/` is ILSpy's project output: one folder per namespace (`XRL.World/`, `XRL.World.Parts/`,
  `Qud.UI/`, …), plus a few third-party/vendor folders (`Newtonsoft/`, `AiUnity/`, `Sheeter/`, …).
- The `XRL.World.Parts` folder is enormous — every part in the game is there, and parts are the
  single best template library for writing your own.
- Line numbers quoted in this journal refer to the decompiled files as produced in this session;
  they will drift after a game update.
