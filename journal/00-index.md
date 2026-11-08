# Journal index — Caves of Qud modding

**Session date:** 2026-11-07
**Game:** Caves of Qud (Freehold Games), Steam app 333640, build `BUILD_2_0_211` / `VERSION_1_0`
**Goal of this session:**
1. Fix a third-party mod that no longer compiles after a game update.
2. Build a new mod — **grateful-pets** — where real pets gift the player items when fed.
3. Record everything learned so future sessions (or future mods) don't have to rediscover it.

---

## What this journal is

Every claim here was either **read out of the decompiled game assembly** (`Assembly-CSharp.dll`),
**read out of the game's own XML data**, or **read off the modding wiki**. Where I am guessing,
it says *(inferred)*. Where I verified something the hard way, it says *(verified)*.

The decompiled assembly lives in [`../decomp/`](../decomp/) — 5,436 `.cs` files, 38 MB. When a
journal entry says "see `XRL.World/GameObject.cs:6092`", that path is inside `decomp/`.

---

## Entries

| # | File | Contents |
|---|------|----------|
| 01 | [Toolchain & decompilation](01-toolchain-and-decompilation.md) | .NET 10 SDK, `ilspycmd`, where the game lives, how to actually read the game's code |
| 02 | [Mod system](02-mod-system.md) | mod folders, `manifest.json`, how/when mods compile, `build_log.txt` as the oracle |
| 03 | [Blueprints & XML](03-blueprints-and-xml.md) | `<object>` / `Inherits` / `Load="Merge"`, root-element indexing, population tables |
| 04 | [Events & inventory actions](04-events-and-inventory-actions.md) | **the important one** — how a verb gets onto an object and how it gets executed |
| 05 | [Pets & companions](05-pets-and-companions.md) | what counts as a pet, all 34 pet blueprints, the dead `GivesTreat` hook |
| 06 | [Injecting code onto the player](06-player-code-injection.md) | `[PlayerMutator]`, `[CallAfterGameLoaded]`, parts vs. scribed parts |
| 07 | [grateful-pets design](07-grateful-pets-design.md) | the mod: feed verb, gift table with weights, verified blueprint values |
| 08 | [API cheatsheet](08-api-cheatsheet.md) | signatures I confirmed, so I don't have to re-decompile them |
| 09 | [Gotchas & troubleshooting](09-gotchas-and-troubleshooting.md) | the async API break, silent `ReceiveObject`, Steam overwriting fixes |
| 10 | [Pellet bug & feed redesign](10-pellet-bug-and-feed-redesign.md) | the “missing pellet” parable (Examiner/odd trinket) + root cause; feed redesign, chat-under-conversation, digestion (local produce → approach/share), Care/tonics, 50-slot lists, corpse milestone, fixes |
| 11 | [How it works & in-progress saves](11-how-it-works-and-in-progress-saves.md) | plain-language end-to-end map of the current mod, and why it works on a save already in progress (with caveats) |

---

## Session timeline

1. **Diagnosed the broken mod.** `Legacy Items (Hand-me-downs)` (workshop 3619655640) failed with
   `LegacyItemPart.cs(175,38): error CS1579` and produced a 0-byte DLL. Cause: the game update
   changed `SavesAPI.GetSavedGameInfo()` from a `List<SaveGameInfo>` to a `Task<List<SaveGameInfo>>`,
   so `foreach` over it no longer compiles. Fixed by unwrapping with `.GetAwaiter().GetResult()`.
   See [09-gotchas](09-gotchas-and-troubleshooting.md).
2. **Set up a decompiler** so API research would be evidence-based instead of guess-based.
   See [01-toolchain](01-toolchain-and-decompilation.md).
3. **Reverse-engineered the verb pipeline.** The game has no "feed" verb at all, so one had to be
   invented. Working out *where* to attach it took the bulk of the research and is
   [entry 04](04-events-and-inventory-actions.md).
4. **Enumerated every pet blueprint** in base + DLC — 34 of them.
   See [entry 05](05-pets-and-companions.md).
5. **Designed grateful-pets** — feed costs 1 food item, always yields 5 desalination pellets plus
   1 weighted random gift. See [entry 07](07-grateful-pets-design.md).
6. **Chased the missing-pellets bug** — resolved as **identification, not loss**: `Desalination Pellet`
   has `Examiner`/`UnknownOddTrinket` (“odd trinket”) until identified, while the grant/message were
   real. Delivery was also hardened (no silent `ReceiveObject` loss; adjacent handoff not gated on
   `IsPlayerLed`). See [entry 10](10-pellet-bug-and-feed-redesign.md).
7. **Feed redesign shipped** — chat-injected **Feed** (food, taste `[n]`, tired→treasure downgrade,
   pellets never downgraded) + **Care** (tonics applied to the pet → healing treasure; hidden if no
   tonics; no tiredness); 50-slot treasure/downgraded/healing lists (verified base blueprints).
8. **Digestion reworked** — 1 pellet now + 4 over the day (300t) **produced locally** on the pet and
   **shared** on approach/adjacency (`GratefulPetsDigestion`, `TurnTick`, `MoveTo` nudge like
   `GiveATreatToPartyLeader`). Corpse milestone (pet-actor `ObjectExtracted` butcher, every 5 → 2
   treasures + 5 pellets). Defensive, versioned deserialization for in-progress saves.
9. **End-to-end map written** — see [entry 11](11-how-it-works-and-in-progress-saves.md).

## Status

- [x] Legacy Items compile fix applied locally
- [x] Research complete
- [x] grateful-pets mod written → [`../mod/`](../mod/)
- [x] **Install + verify both mods compile in-game** — *done 2026-11-07, first try, no iteration needed*
- [x] Legacy Items forked to a repo under `ap0ught` for maintenance
- [x] **Both repos published** — `ap0ught/grateful-pets` and `ap0ught/legacy-items` (public)
- [x] **“Pellets don’t arrive” resolved** — root cause was `Examiner`/“odd trinket” identification; delivery hardened (checked `ReceiveObject`, adjacent delivery independent of `IsPlayerLed`) ([entry 10](10-pellet-bug-and-feed-redesign.md))
- [x] **Feed/Care redesign implemented** — chat choices (Feed/Care), taste+tired (food only), 50-slot lists, tonic→pet apply + healing treasure; compiles (`GRATEFUL PETS` / `Success :)`, 2026-11-08)
- [x] **Works on saves already in progress** — `[CallAfterGameLoaded]` injects the player part; defensive `GratefulPetsDigestion` `Read` protects old/incompatible payloads ([entry 11](11-how-it-works-and-in-progress-saves.md))
- [x] **README rewritten + `AGENT.md` added** (repo/how-to + in-game-flavored guide)
- [ ] **Runtime edges, manual verify:** digestion-over-time (natural 300t waits across zones/led states), corpse-milestone trigger in all “pet butchers” cases, and any message-spam/approach quirks

## Verification result

Mods are compiled by the game at startup, so the build check is: install → launch → read
[`build_log.txt`](02-mod-system.md#build_logtxt-is-the-oracle) → close the game.

```
[2026-11-07T09:03:16] === INHERITANCE (LEGACY ITEMS) ===
[2026-11-07T09:03:16] Compiling 16 files...
[2026-11-07T09:03:17] Success :)
[2026-11-07T09:03:17] Location: …/ModAssemblies/LegacyItems.dll
[2026-11-07T09:03:17] Defined symbol: MOD_LEGACYITEMS

[2026-11-07T09:03:18] === GRATEFUL PETS ===
[2026-11-07T09:03:18] Compiling 1 file...
[2026-11-07T09:03:18] Success :)
[2026-11-07T09:03:18] Location: …/ModAssemblies/GratefulPets.dll
[2026-11-07T09:03:18] Defined symbol: MOD_GRATEFULPETS

[2026-11-07T09:03:18] ==== FINAL LOAD ORDER ====
…
7: FreeholdGames_DLC_PetsPack1
8: GratefulPets
```

Two takeaways:

1. **The Legacy Items fix works** — the `CS1579` that produced a 0-byte DLL is gone; all 16
   files compile into a real `LegacyItems.dll`.
2. **grateful-pets compiled clean on the first attempt.** Every signature used in
   `mod/Scripts/GratefulPets.cs` was checked against `decomp/` before writing (entry
   [08-api-cheatsheet](08-api-cheatsheet.md) is that checklist) — `RequirePart<T>`,
   `OwnerGetInventoryActionsEvent`, `AddAction`'s named parameters, `GetDisplayName`,
   `Messaging.XDidYToZ`, `Stat.Random`, `ReceiveObject(string, int)`, `UseEnergy(int, string)`.
   Not one compile round-trip was spent on a wrong signature.

**Still unverified:** actual in-game behaviour (the verb appearing on a pet, the roll, the
messages). That needs a save with a pet in the party and a human pressing `F`. Everything up to
and including compilation is confirmed.
