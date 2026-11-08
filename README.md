# grateful-pets

> *“Live and drink,”* says your chrome-plated hound, who has never once paid for a drink in its life.  
> *“Live, drink, and bring snacks,”* you reply — because in Qud, affection is a barter economy and you are holding beetle jerky.

**Repository:** [`github.com/ap0ught/grateful-pets`](https://github.com/ap0ught/grateful-pets)  
A **Caves of Qud** mod where real pets appreciate being fed (and cared for) — and pay you back in desalination pellets, treasure, and the slow, deliberate generosity of something that has to *digest first*. This repo also keeps the **research journal** that explains how it was built, line by decompiled line.

---

## What it does (the pact, witnessed by no Baetyl in particular)

**Talk to a real pet** (any creature with the `Pet` / `StartingPet` tag, or the `Pettable` part — base game, *Pets of Harvest Dawn*, and anything a mod blesses with those tags). Because the verbs live **under chat**, all the usual conversation law applies for free: if your companion is too busy being gloriously perforated in melee or is, regrettably, on fire, parley waits — as it should.

You’ll see, when the stars (and your inventory) align:

- **`Let me feed you.`** *(always, for a real pet)* — pick a carried **food** item.
  - Costs **1 food** + **1 turn**. (Feeding from a stack eats one, not the whole tragic hoard — `Stacker` is merciful.)
  - **Treasure now:** 1 random pull from a **50-slot treasure list** (nuggets, rough/polished gems, figurines, and curios like `Chiral Rings` or a `Portable Beehive`). Feed the same food too often and your pet grows *weary* of it — the treasure **downgrades** (tiredness decays **−1 per in-game day**, tracked **per pet, per food**, shown as `[n]` in the picker).
  - **Desalination pellets:** **1 now** + **4 more over the course of the day** (every **300 turns**) as the pet digests. Each is **produced locally** and the pet **approaches to share** it (a grateful shuffle, not an interdimensional wire transfer). *Pellets are never downgraded.* (Unidentified pellets may masquerade as an **“odd trinket”** until identified — the handoff message still fires; Qud loves a humble disguise.)
- **`Let me care for you.`** *(only if you’re carrying something suitable)* — pick a care item.
  - **Organic pets:** applies a **tonic/syringe**’s real effect to the pet (`ApplyExternally`) and gives you **1 healing treasure** from a **50-slot healing list** (bandages, salves, ubernostrum, convalessence, and other medicinal kindnesses).
  - **Robotic pets:** biological tonics often fail on chrome, so Care takes **robot-care items** — e.g. give a **spent energy cell** and the robot *runs a software update* on it, returning a **fresh cell** — plus a healing treasure. **No tiredness.** Care, unlike cuisine, does not pall.
  - **Robotic pets also** get robot-flavored feeding reactions and a **20% bonus energy-cell** roll (added on top) on each feed/care.

**Butcher’s bonus:** every **5 corpses the pet butchers** (pet is the actor) → **2 treasures + 5 pellets**, produced and shared. A companion that helps with the grisly work is a companion that tips.

**Also handy:** you can **apply a desalination pellet** directly to your **carried** liquid containers or an **adjacent open liquid pool** from their action menu (vanilla purification path).

> *The pet does not email you the pellet. It manufactures gratitude on-site and walks it over, like a civilized being of the salt dunes.*

---

## Install

1. **Options → Modding → Allow scripting mods** = on.  
   (e.g. `~/.config/unity3d/Freehold Games/CavesOfQud/Local/PlayerOptions.json` → `"OptionAllowCSMods": "Yes"`.)
2. Install/update with the helper script (or copy manually):

   ```bash
   ./install.sh   # from this repo; uses QUD_MODS_DIR or the default Linux Mods path
   ```

   Manual copy equivalent — `mod/` → `Mods/GratefulPets/`:

   ```text
   ~/.config/unity3d/Freehold Games/CavesOfQud/Mods/GratefulPets/
   ├── manifest.json
   └── Scripts/GratefulPets.cs
   ```

3. **Restart the game** (mods compile at startup; new installs and updates both need a restart).
4. Confirm it compiled — `build_log.txt` is the oracle:

   ```bash
   grep -B1 -A5 '=== GRATEFUL PETS ===' \
     ~/.config/unity3d/Freehold\ Games/CavesOfQud/build_log.txt | tail -7
   # expect: Compiling 1 file... / Success :) / Defined symbol: MOD_GRATEFULPETS
   ```

5. In game: **chat with a pet** → choose **“Let me feed you.”** (needs carried food, be in range) or **“Let me care for you.”** (needs a carried tonic). There’s also **“Feed to pet”** on a carried food item if a chatable pet is adjacent.

If it misbehaves, the second oracle is `~/.config/unity3d/Freehold Games/CavesOfQud/Player.log` (`MODERROR`/exceptions).

---

## Works on a save already in progress?

**Yes — bring your existing regrets, they’re welcome here.** The mod hooks the player on load (`[CallAfterGameLoaded]` + `[PlayerMutator]`), injects chat choices at runtime, and adds per-pet state as you interact. No new game, no ritual rebirth at Joppa.

- **Restart after install/update** so it recompiles.
- If you ever loaded through an older/broken `GratefulPetsDigestion` serialization (we fixed this with a defensive, versioned `Read`), that pet’s *in-progress* digestion/corpse count resets safely — **feed the pet again** to re-arm it.
- Timed sharing runs on the pet’s ticks; keep your companion nearby/player-led so it can waddle over and deliver the goods. (Cross-zone edge cases are less ceremonially verified.)

Details: [`journal/11-how-it-works-and-in-progress-saves.md`](journal/11-how-it-works-and-in-progress-saves.md).

---

## How it works (for the Mechanimist in you)

| Piece | Where | Gist |
|---|---|---|
| Player hook (incl. existing saves) | `GratefulPetsPlayerMutator` `[PlayerMutator]` + `GratefulPetsLoadHandler` `[CallAfterGameLoaded]` | `RequirePart<GratefulPets>()` on the player, on create and on every load |
| Feed/Care under chat | `GratefulPets.HandleEvent(BeginConversationEvent)` → injects `Choice` + `IConversationPart` | inherits `ConversationScript` busy/range/can-start gating for free |
| Food taste (per pet, per food, decays) | `GratefulPets.FeedLog` on the pet (serialized `Property`) | `[n]` in picker; tired ≥ 5 downgrades **treasure** only |
| Digestion + sharing | `GratefulPetsDigestion` `[Serializable]` on the pet, `WantTurnTick`/`TurnTick` | 1 now + 4/300t, produced locally, handed over when adjacent; `MoveTo` nudge like `GiveATreatToPartyLeader` |
| Corpse milestone | legacy `ObjectExtracted` (`Action=="Butcher"`, pet = actor) | every 5 → 2 treasures + 5 pellets |
| Rewards | 50-slot `TreasureList` / `DowngradedTreasureList` / `HealingList` | verified base blueprints, uniform by slot (repeats weight) |

Source of truth: [`mod/Scripts/GratefulPets.cs`](mod/Scripts/GratefulPets.cs) (manifest: [`mod/manifest.json`](mod/manifest.json)).

---

## Contents of this repo

| Path | What it is |
|---|---|
| [`mod/`](mod/) | **the mod** — `manifest.json` + `Scripts/GratefulPets.cs` |
| [`journal/`](journal/) | **the research journal** — start at [`journal/00-index.md`](journal/00-index.md) (how verbs wire up, how the mod injects, and every time the pellets were “missing” because of an *odd trinket* in a trench coat) |
| [`decomp/`](decomp/) | `Assembly-CSharp.dll` decompiled to C# (5,436 files) — the evidence the journal cites |
| [`reference/`](reference/) | curated extracts and the full type list |
| [`legacy-items-fix/`](legacy-items-fix/) | the *Legacy Items* compile fix, maintained at [`ap0ught/legacy-items`](https://github.com/ap0ught/legacy-items) |
| [`AGENT.md`](AGENT.md) | how to work in this repo (for humans and well-meaning automatons) |

---

## Building on it / the journal trail

If you’re here to mod, not merely to be adored by your own pet rock: read **[04 — Events & inventory actions](journal/04-events-and-inventory-actions.md)** (the `FireOnActor` fork), **[06 — Injecting code onto the player](journal/06-player-code-injection.md)**, **[08 — API cheatsheet](journal/08-api-cheatsheet.md)**, and **[10](journal/10-pellet-bug-and-feed-redesign.md)/[11](journal/11-how-it-works-and-in-progress-saves.md)** for the real implementation. Every claim in the journal is grounded in `decomp/` or game XML, or marked *(inferred)*.

---

## Provenance & a polite bow to the Eaters

`decomp/` is ILSpy output from the game’s `Assembly-CSharp.dll`, included so claims can be checked. **Game code is © Freehold Games** — included here as a personal/reference modding artifact for a copy of the game I own; not my code, no rights claimed, not a product. If Freehold Games wants any of it removed, open an issue and it shall be returned to the salt.

The `grateful-pets` mod source and the modding notes in this repo are mine.

**Legacy Items** (workshop `3619655640`, by *Kraut Kontrol*) stopped compiling after a game update; the fix is maintained at **[ap0ught/legacy-items](https://github.com/ap0ught/legacy-items)** (original authorship retained).

*Live and drink. Tip your snapjaw.*