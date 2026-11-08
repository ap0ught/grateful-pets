# AGENT.md — field notes for humans & helpful automatons

> *You awaken in a directory. There is a decompiled assembly here. You are likely to be eaten by a grue, or, worse, by a stale assumption about `ReceiveObject`. Proceed with evidence.*

This repo is **`ap0ught/grateful-pets`**: a Caves of Qud mod (**Grateful Pets**) plus the research journal that keeps us honest. This file tells you **what we do here and how to work on it** without accidentally inventing a new way for a desalination pellet to become metaphysically absent.

---

## The lay of the land (a map, not a prophecy)

| Path | What it is | Sacred? |
|---|---|---|
| `mod/manifest.json`, `mod/Scripts/GratefulPets.cs` | **the mod** (ships as the `GratefulPets` folder) | edit with care; this is the live artifact |
| `journal/` | numbered research log — claims cite `decomp/` or game XML, or say *(inferred)* | **keep accurate**; add entries, don’t retcon evidence |
| `decomp/` | ILSpy C# from `Assembly-CSharp.dll` (5,436 files) | **reference only** — game code © Freehold Games; don’t present as ours, don’t “fix” it here |
| `reference/` | curated extracts/types | reference |
| `legacy-items-fix/` | patch for the Legacy Items mod (maintained at `ap0ught/legacy-items`) | reference / attribution matters |
| `README.md` | player-facing guide (with Qud-flavored charm) | keep current with behavior |

**Installed/test path on this machine:** `~/.config/unity3d/Freehold Games/CavesOfQud/Mods/GratefulPets/` (mirrors `mod/`).  
**The oracle:** `~/.config/unity3d/Freehold Games/CavesOfQud/build_log.txt` — mods compile at **game startup**. Runtime sins appear in `Player.log`.

---

## What the mod actually does (so you don’t “helpfully” break it)

- **Player hook works on existing saves:** `[PlayerMutator]` + `[HasCallAfterGameLoadedAttribute]/[CallAfterGameLoaded]` → `player.RequirePart<GratefulPets>()`.
- **Feed/Care are conversation choices** injected on `BeginConversationEvent` (`IConversationMinEvent.DispatchAll` reaches the player/Actor) into the pet’s `StartNode` (deduped): `GratefulPetsFeedChoicePart` / `GratefulPetsCareChoicePart` handle `EnterElementEvent` (`Target="End"`). This lives **under chat** to inherit `ConversationScript` busy/fighting/range/can-start gating *for free*. A pet locked in melee is not a buffet.
- **Real pets only:** `Pet`/`StartingPet` tag or `Pettable` part — *not* `IsPlayerLed()`.
- **Feed:** carried `Food` → 1 consumed (stack-safe: `Destroy` decremented by `Stacker`), per-pet/per-food taste in `GratefulPets.FeedLog` (decays −1/day, `Calendar.TotalTimeTicks/TurnsPerDay`), `[n]` in picker; **tired ≥ 5** downgrades **treasure only**. 1 treasure now + **1 pellet now, 4 over the day (300t)** via digestion. **Care:** carried `Tonic` → `InventoryActionEvent.Check(... "ApplyExternally" ..., ObjectTarget: pet)` applies the real tonic to the pet, +1 healing treasure. **No tiredness.** Care hidden if no tonics (checked at conversation start).
- **Digestion/sharing:** `[Serializable]` `GratefulPetsDigestion` on the pet (`WantTurnTick`/`TurnTick`), produces `Desalination Pellet` tagged `GratefulPets.GiftPellet` **locally** into the pet, delivers when adjacent (batched), nudges player-led pets with `MoveTo(The.Player, shortBy:1)` (cf. `GiveATreatToPartyLeader`). **Check `ReceiveObject` results** — silent loss is how we got here. Defensive, versioned `Write/Read` (bad old save data resets safely).
- **Corpse milestone:** legacy `ObjectExtracted`, `Action=="Butcher"`, **pet is actor** → every 5 gives 2 full-tier treasures + 5 pellets.
- **Extras:** “apply desalination pellet” to carried `LiquidVolume` containers / adjacent open pools; inventory “Feed to pet” on carried `Food`.

50-slot `TreasureList` / `DowngradedTreasureList` / `HealingList` = verified base blueprints, uniform by slot (repeats weight). **Don’t invent blueprint names** — verify against `StreamingAssets/Base/ObjectBlueprints/*.xml` or `decomp` evidence.

---

## How to work (the ritual)

1. **Ground claims in evidence.** Check `decomp/` and game XML before guessing APIs. The journal’s whole religion is “verified vs inferred.”
2. **Edit** `mod/Scripts/GratefulPets.cs` (+ `manifest.json` if needed), mirror to the installed `Mods/GratefulPets/` folder for testing.
3. **Build = restart the game.** Then:
   ```bash
   grep -B1 -A6 '=== GRATEFUL PETS ===' ~/.config/unity3d/Freehold\ Games/CavesOfQud/build_log.txt | tail -8
   ```
   Expect `Compiling 1 file...` / `Success :)`. Fix compile errors from the log; **don’t** claim it compiles if you didn’t see it.
4. **Test on a save in progress** (supported). For digestion timing, do a **fresh feed** after a recompile (serialization/part-state can reset on upgrade) and pass ~300 turns near the pet.
5. **Journal meaningful changes.** Add a numbered entry or a dated section; record *why* (the `IsPlayerLed` adjacent-delivery gate, the `ReceiveObject`-unchecked loss, the “odd trinket” Examiner parable). Future-you is a stranger; be kind.
6. **Commit style:** clear, factual messages. Don’t commit secrets. `decomp/` is large reference material with its own provenance notes (see README) — handle with the stated care.

## Sharp edges (learned the damp way)

- `ConversationScript` busy/fighting is **not** `CanStartConversationEvent.Check`. Gate under chat, or be precise.
- A “missing” pellet may be an **unidentified odd trinket** (`Examiner`/`UnknownOddTrinket`). Confirm before declaring the inventory haunted.
- `ReceiveObject`/inventory moves can fail or behave subtly; **check results**, and never count a pellet delivered into the luminous void.
- `TurnTick`/zone activity, `IsPlayerLed`, and cross-zone separation affect sharing — test the real loop.
- The in-game UI shows user-facing text; keep it clear. Save the chrome jokes for docs/commit messages, not for obscuring a failure message.

## How to interact (for agents & collaborators)

- **Ask before big behavioral/product changes** (reward balance, who counts as a pet, save semantics). Small, obvious fixes + journal notes are fine.
- **When reporting a bug, bring:** `build_log.txt` excerpt, relevant `Player.log` lines, the pet type/state (led? adjacent? in combat?), and whether you restarted after changes. Repro steps > vibes.
- **Cite files/lines** (e.g. `mod/Scripts/GratefulPets.cs`, journal entries, `decomp/...`) so the next water-ritual can be evidence-based.

*Live and drink. Commit your work. And if a value fails to arrive, remember: in Qud, it may simply be disguised as a trinket, awaiting a more enlightened appraisal.*