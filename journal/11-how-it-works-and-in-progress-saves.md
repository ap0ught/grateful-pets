# 11 — How it works, and "will this wake up in a save already drowning in history?"

**Date:** 2026-11-07 → 2026-11-08
**Status:** implemented and installed; compile verified via `build_log.txt` (`GRATEFUL PETS` → `Success :)`, 2026-11-08). Runtime/user-verified in parts (immediate feed/treasure/pellet handoff "works much better"; digestion-over-time is manual waiting; corpse milestone hooked but edge cases unverified). This entry is the plain-language map of the whole machine.

> *Live and drink, friend. Now — live, drink, and remember to pack a beetle jerky for the tinker-hound.*

---

## The short version

1. **Chat with a real pet** → you get two choices, courtesy of the same ancient rite that governs all parley in Qud: **Feed** (food) and **Care** (a tonic/syringe, *only if you’re carrying one*). Busy in melee? On fire? The Baetyl of Conversation denies you like it denies everyone else — we get that logic for free by living inside the chat.
2. **Feed** costs 1 food, earns 1 **treasure** now (downgraded if the pet is tired of *that* food), and **1 desalination pellet now** + **4 more over the day** as the pet digests, *produces locally*, and trots over to share them.
3. **Care** applies a tonic’s real effect **to the pet** and pays you **1 healing treasure**. No tiredness. (A well-cared-for chrome companion is a generous companion.)
4. **Every 5 corpses the pet butchers** (pet is the actor) → **2 treasures + 5 pellets**, produced and shared.
5. Works on **saves already in progress**. No new game, no water ritual with the character-select screen required.

---

## 1. How the mod gets into a save (even one that predates it)

The mod needs one part, `XRL.World.Parts.GratefulPets`, attached to the **player**. It gets there two ways — belt and suspenders, because Qud is older than mercy:

| Hook | When it fires | Why it matters |
|---|---|---|
| `[PlayerMutator]` → `GratefulPetsPlayerMutator.mutate(GameObject player)` | the player GameObject is first created (new game / relevant creation paths) | fresh characters get the part at birth |
| `[HasCallAfterGameLoadedAttribute]` + `[CallAfterGameLoaded]` → `GratefulPetsLoadHandler.OnGameLoaded()` | **after every load** | **in-progress saves**: load an old save, the hook runs, `player.RequirePart<GratefulPets>()` snaps the behavior in. No reincarnation necessary. |

Both call `RequirePart<GratefulPets>()`, which is idempotent (it won’t double-attach). `GratefulPets` is `[Serializable]` and effectively stateless — per-pet state lives on the pets, not on this part.

**Verified pattern** (entry 06 / decomp): this is the standard "works on existing saves" combo.

---

## 2. Feed & Care live under chat (and that’s the whole trick)

The first draft stapled a **Feed** verb onto pet twiddle menus and invented its own gating. That fought the game. The current design moves the verbs **into the conversation**, so the game’s own conversation rules do the hard work:

- The player’s `GratefulPets` part subscribes to `BeginConversationEvent` (`ID == BeginConversationEvent.ID`; `IConversationMinEvent.DispatchAll` delivers it to the **Actor** = player, among others).
- If `E.SpeakingWith` is a **real pet** (`HasTag("Pet") || HasTag("StartingPet") || HasPart<Pettable>()` — *not* `IsPlayerLed()`), it:
  1. `E.SpeakingWith.RequirePart<GratefulPetsDigestion>()` (so digestion/corpse tracking is present), and
  2. injects choices into `E.StartNode` (deduped by ID): **`GratefulPets_Feed`** “*Let me feed you.*” and **`GratefulPets_Care`** “*Let me care for you.*” (Care only if `CollectCarriedTonics(The.Player).Count > 0`, checked at conversation start).
- Each choice is a runtime `Choice` (`Target = "End"`) carrying an `IConversationPart` (`XRL.World.Conversations.Parts.GratefulPetsFeedChoicePart` / `GratefulPetsCareChoicePart`) that handles `EnterElementEvent` → `DoFeedPet` / `DoCareForPet`.

**What you get for free by being “under chat”:** `ConversationScript.AttemptConversation` enforces the busy/fighting/on-fire “too busy…” guards and the normal can-start/range flow *before* the conversation opens — so a pet locked in glorious melee simply won’t parley, and Feed/Care never appear as a reachable loophole. Earlier we tried a `CanStartConversationEvent.Check` gate and rolled it back: that check does **not** cover the combat-busy guard (it lives in `ConversationScript`, not the event), and combat-busy pets *could* be fed before — moving under chat resolved it correctly (entry 10, Part 6–7 notes).

> *If the creature cannot spare the breath to barter, it cannot spare the jaw to chew. Such is the water-rite of interface design.*

---

## 3. Feeding — food, taste, treasure, and a slow-blooming thank-you

- **Picker:** `Popup.PickGameObject` over carried `Food` (non-important). `ExtraLabels` renders **`[n]`** = effective times you’ve fed *that pet that food type*. Sorted least-fed first (a varied diet is a lucrative diet).
- **Per-pet taste memory:** pet `Property` **`GratefulPets.FeedLog`**, serialized (lines `blueprint \t count \t day`). Decays **−1 per in-game day** (`(long)Calendar.TotalTimeTicks / Calendar.TurnsPerDay`, `TurnsPerDay == 1200`). Effective **≥ 5** = **tired** of that food.
- **Cost:** 1 food item + 1 turn (`1000` energy). `Destroy()` on a stack is intercepted by `Stacker` (`BeforeDestroyObjectEvent`) which decrements instead — so it consumes exactly 1. *(verified; easy to “fix” wrong.)*
- **Treasure now:** 1 roll from **`TreasureList`** (50 slots, uniform by slot; repeats = weight). If tired of that food → **`DowngradedTreasureList`** (50 slots). Nuggets, rough/polished gems, figurines, a few curios (`BoxOfCrayons`, `Portable Beehive`, `Arsplice Seed`, `Chiral Rings`, `Metamorphic Polygel`, `Fixit Spray`, `Spray-a-Brain`, `Random Rare Phial`). All verified base blueprints.
- **Pellets (never downgraded, and *not* all at once):** **1 desalination pellet now** + **4 over the course of the day** while digesting — produced **locally** on the pet, then the pet **approaches to share** (see §5). This replaced the old “5 at once” grant, and also replaced a brief player-side timer that was the wrong shape (it teleported gratitude; gratitude, in Qud, walks).

---

## 4. Caring — apply the syringe, receive the salve (metaphorically)

- **Picker:** carried items with the `Tonic` part (non-important), “syringe/injector” by any name the Eaters gave them. Care option is hidden if you carry none.
- **Effect:** the tonic’s **real** effect is applied **to the pet** via `InventoryActionEvent.Check(tonic, actor, tonic, "ApplyExternally", ..., ObjectTarget: pet)` — the `Tonic` part runs the genuine apply path (and consumes the tonic on success). 1 turn. If the check fails, no healing treasure.
- **Reward:** 1 roll from **`HealingList`** (50 slots: bandages, salve/ubernostrum/sphynx-salt & friends, convalessence, a scatter of useful tonics/phials).
- **No tiredness.** Care does not touch `GratefulPets.FeedLog`. You cannot bore a patient into worse bedside manner by being too attentive. (Probably.)

---

## 5. Digestion — “produce locally, then desire to approach and share it”

Handled by `[Serializable]` **`GratefulPetsDigestion`** on the pet (`RequirePart`’d when you feed/chat). State: `RemainingToProduce`, `NextProductionTick`, `HeldForDelivery`, `CorpseCount`, with a **versioned, defensive `Write/Read`** (resets cleanly on old/incompatible save data so a format bump doesn’t brick a load).

- **Schedule:** 4 pellets, every **300 turns** (`Calendar.TurnsPerDay/4`). Due check compares `Calendar.TotalTimeTicks` to `NextProductionTick` inside `WantTurnTick`/`TurnTick`.
- **Local production:** `GameObject.Create(Desalination Pellet)`, tagged `GratefulPets.GiftPellet`, placed into the **pet’s** inventory (`pet.ReceiveObject`). If that fails, it falls back to direct delivery so the pellet is never silently lost (a real bug: counting `HeldForDelivery` without checking `ReceiveObject` dropped pellets into the luminous void).
- **Sharing:** when adjacent to the player, the pet hands over held pellets (batched: “*hands you N desalination pellets*”). Not adjacent → if **player-led**, nudge with `Brain.PushGoal(new MoveTo(The.Player, careful:false, overridesCombat:false, shortBy:1))`, mirroring the native `GiveATreatToPartyLeader` goal handler. Adjacent delivery is **not** gated on `IsPlayerLed()` (fix for the “treasure but no pellet” repro — the immediate handoff must work even when leadership is quirky).

> *The pet does not email the pellet. It manufactures gratitude on-site and walks it to you, like a civilized being of the salt dunes.*

---

## 6. Corpse milestone — a butcher’s bonus

- The pet part registers the legacy `ObjectExtracted` event (`Register`/`FireEvent(Event E)`) and counts `Action == "Butcher"`. Every **`CORPSE_MILESTONE` = 5** corpses butchered by the pet → counter resets, **2 full-tier treasures** (`GiveTreasure(..., Tired:false)`) + **5 pellets** produced for delivery/share.
- **Scope (important):** it counts when **the pet is the actor** doing the butchering (the event fires on the actor). It’s attached when you chat/feed the pet. If your table uses a different “pet eats/butchers” path (player-butcher, carnivore auto-eat), the hook may need aligning — the intent is recorded, the trigger is the verified `ObjectExtracted/Butcher` seam.

---

## 7. The pellet-that-wasn’t — an identification parable

An early report: “the pet *says* it hands me desalination pellets but I don’t get them.” The code *did* grant them and `ReceiveObject` reported success. The twist: `Desalination Pellet` carries `<part Name="Examiner" Unknown="UnknownOddTrinket" .../>` — until identified/used, it presents as an **“odd trinket.”** Not a missing item. An item wearing a mask.

> *In Qud, absence is frequently just ignorance with better iconography.*

(We also hardened the actual delivery paths anyway — see §5 — because a parable is no excuse for a silent `ReceiveObject` failure.)

---

## 8. Will it work on a save already in progress?

**Yes.** This is the designed case.

- **On load**, `[CallAfterGameLoaded]` adds `GratefulPets` to the player. No new game, no rebirth at Joppa’s gates.
- **Runtime injection:** chat choices are added on `BeginConversationEvent`; per-pet digestion is `RequirePart`’d as you interact. Existing pets in an existing world become feedable/careable the moment you parley.
- **You must restart** after installing/updating the mod — Qud compiles script mods at startup (`build_log.txt` → `GRATEFUL PETS` / `Compiling 1 file...` / `Success :)`).

**Caveats, spoken plainly (so the chrome prophets can’t say we didn’t warn you):**
- **Serialization bumps:** loading a save that held an older/incompatible `GratefulPetsDigestion` payload used to throw a `String`→`Int32` deserialization error (engine skipped bytes). We added a defensive, versioned `Read` that resets the part safely so the save loads — but any *in-progress* digestion/corpse count on that pet is cleared. **Fix:** feed the pet again.
- **Timed sharing needs ticks + proximity:** the 4-over-time pellets produce on the pet’s `TurnTick`. Keep the pet around (ideally player-led, same zone). Delivery batches when adjacent; a led pet gets a `MoveTo` nudge. Cross-zone/long-separation behavior isn’t fully battle-tested.
- **“Odd trinket” identification:** unidentified desalination pellets can look like an odd trinket in inventory until used/identified — the handoff message still fires.
- **Compile is verified; some runtime edges aren’t:** digestion-over-time (natural waiting) and corpse-milestone edge cases are marked for live testing.

## 9. Files that matter

- `mod/Scripts/GratefulPets.cs` — the mod (player hooks, chat injection, feed/care, lists, `GratefulPetsDigestion`).
- `mod/manifest.json` — `id: GratefulPets`, `loadorder: 100`.
- `journal/04` (verb/event wiring), `06` (player injection), `08` (API cheatsheet), `10` (redesign/forensics/fixes), **11** (this map).
- Oracle: `~/.config/unity3d/Freehold Games/CavesOfQud/build_log.txt`. Runtime errors: `Player.log`.
