# grateful-pets

A **Caves of Qud** mod where your pets appreciate being fed — plus the research journal that
documents how the whole thing was built.

> **Feeding a pet costs one food item. In return you always get 5 Desalination Pellets, plus one
> random valuable item** — a nugget, a gemstone, a figurine, some jewellery, or, very rarely, a
> ubernostrum injector or a pair of Chiral Rings.

Any creature with the `Pet` or `StartingPet` tag, or the `Pettable` part, can be fed. That covers
every base-game pet and every *Pets of Harvest Dawn* DLC pet, plus anything another mod adds with
those tags.

---

## Contents

| Path | What it is |
|---|---|
| [`mod/`](mod/) | **the mod** — `manifest.json` + `Scripts/GratefulPets.cs` |
| [`journal/`](journal/) | **the journal** — everything learned while building it, start at [`journal/00-index.md`](journal/00-index.md) |
| [`decomp/`](decomp/) | the game's `Assembly-CSharp.dll` decompiled to C# (5,436 files) — the evidence base the journal cites |
| [`reference/`](reference/) | curated extracts: key game classes, a working third-party mod, the full type list |
| [`legacy-items-fix/`](legacy-items-fix/) | the compile fix for the unmaintained *Legacy Items* mod, also maintained at [`ap0ught/legacy-items`](https://github.com/ap0ught/legacy-items) |

---

## Installing the mod

1. Make sure **Options → Modding → Allow scripting mods** is on (it already is on this machine:
   `Local/PlayerOptions.json` → `"OptionAllowCSMods": "Yes"`).
2. Copy `mod/` into your mods folder as a directory named `GratefulPets`:

   ```
   ~/.config/unity3d/Freehold Games/CavesOfQud/Mods/GratefulPets/
   ├── manifest.json
   └── Scripts/GratefulPets.cs
   ```

3. Launch the game. Mods are enabled by default — there is nothing to tick.
4. Check it compiled:

   ```bash
   grep -A4 "GRATEFUL PETS" ~/.config/unity3d/Freehold\ Games/CavesOfQud/build_log.txt
   # expect: Compiling 1 file...  /  Success :)  /  Location: …/ModAssemblies/GratefulPets.dll
   ```

   *Verified 2026-11-07 — see [`journal/00-index.md`](journal/00-index.md#verification-result).*

5. In game: examine a pet (or right-click it, or use interact-nearby) → choose **Feed** (hotkey
   `f`). You need to be adjacent and holding at least one food item.

## Using it as a starting point for your own mod

The journal is written to be reusable even if you're building something completely different:

- **[04 — Events & inventory actions](journal/04-events-and-inventory-actions.md)** is the one
  that matters most: how a verb gets attached to an object, and the `FireOnActor` fork that
  decides whether the handler lives on the object or on the player.
- **[02 — Mod system](journal/02-mod-system.md)** — manifest, compilation, `build_log.txt`.
- **[08 — API cheatsheet](journal/08-api-cheatsheet.md)** — signatures read straight out of the
  game assembly, so you don't have to decompile them yourself.
- **[09 — Gotchas](journal/09-gotchas-and-troubleshooting.md)** — the failure modes that cost the
  most time this session.

---

## About the decompilation

`decomp/` is ILSpy's output from `CoQ_Data/Managed/Assembly-CSharp.dll`, included so the journal's
claims can be checked against the actual game code rather than taken on trust.

**The game code is © Freehold Games.** It is included here purely as a personal/reference research
artifact for modding the game the author owns a copy of. It is not my code, I claim no rights to
it, and it should not be redistributed as a product. If Freehold Games would like any part of this
removed, that's entirely reasonable — open an issue.

Modding notes and the `grateful-pets` mod source in this repo are mine.

## Legacy Items fork

The workshop mod **Legacy Items (Hand-me-downs)** (workshop `3619655640`, by *Kraut Kontrol*)
stopped compiling after a game update (`error CS1579`, 0-byte DLL). The fix and a maintained copy
live at **[ap0ught/legacy-items](https://github.com/ap0ught/legacy-items)** — original authorship
retained; see that repo's README for the details of the patch.
