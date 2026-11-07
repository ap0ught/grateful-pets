# 10 — The missing-pellets bug, and the feed redesign

**Date:** 2026-11-07 (second half of the session)
**Status:** **root cause found — there was never a bug** (see 1.7). Redesign specified, not yet
shipped.

---

## Part 1 — "He hands me the pellets, but I don't get them"

### 1.1 The report

Feeding a pet prints all three messages in order:

```
E-Ros gobbles up the beetle jerky.
E-Ros hands you 5 desalination pellets.
E-Ros gives you your rough topaz gemstone.
```

The **topaz arrives**; the **pellets never show up** in inventory.

### 1.2 What the messages already prove

The pellet message is gated in `mod/Scripts/GratefulPets.cs`:

```csharp
int pellets = Actor.ReceiveObject(PELLET_BLUEPRINT, PELLET_COUNT);
if (pellets > 0)
    Messaging.XDidYToZ(Pet, "hand", null, Actor, pellets + " desalination pellets");
```

The message printed `5`, so **`ReceiveObject("Desalination Pellet", 5)` returned `5`**. Whatever
went wrong, it went wrong *after* the API claimed success. That rules out "wrong blueprint name",
"wrong count", and "the handler never ran".

The same messages were recovered from the save file itself (see 1.6), so this is not
misremembering — the strings are in `Primary.sav.gz`.

### 1.3 The full call chain (verified in `decomp/`)

| Step | Where | What happens |
|------|-------|--------------|
| 1 | `GameObject.cs:6097` `ReceiveObject(string, int, …)` | → `TakeObject(Blueprint, Number, NoStack:false, Silent:true, EnergyCost:0, …)` |
| 2 | `GameObject.cs:6014` `TakeObject(string, int, …)` | creates **one** object; if `Number > 1 && !NoStack && CanGenerateStacked() && Stacker != null && StackCount == 1` it sets **`Stacker.StackCount = Number`** and takes that single object, returning `Number` on success / `0` on failure. Otherwise loops `Number` times taking one at a time. |
| 3 | `GameObject.cs:6051` `TakeObject(GameObject, …)` | pins the event pool, fires **`CommandTakeObject`** (the `Silent` + `EnergyCost` pooled variant), returns `FireEvent(...)` |
| 4 | `Inventory.cs:2185` `CommandTakeObject` | checks containment loop / frozen / stasis → fires `BeginTake` → `BeginBeingTaken` → **`PerformTake`** |
| 5 | `Inventory.cs:1510` `PerformTake` | if not already in `Objects`: `RemoveFromContext` → (messaging, skipped when silent) → **`AddObject(obj, null, Silent:false, NoStack:true, FlushTransient:true, E)`** → `TakenEvent` → `TookEvent` → `CheckStacks()` |
| 6 | `Inventory.cs:255` `AddObject` | three guards, **each of which returns without adding**: `!physics.Takeable`, `IsInGraveyard()`, `IsInvalid()` |

Step 6 is the classic silent-failure candidate: `AddObject` returns the object either way, so
`PerformTake` keeps going, `FireEvent` returns `true`, and `TakeObject` returns `Number`. The
caller sees success and the item is not there.

### 1.4 Why step 6 is ruled out *(verified)*

`AddObject`'s three guards all call `MetricsManager.LogError`. Both possible sinks end up in
**`Player.log`** (`MetricsManager.cs:366` and `:308`):

* `ModManager.TryGetCallingMod` finds a mod on the stack (our part *is* on the stack) →
  `Mod.Error` → `LogModError` → `UnityEngine.Debug.LogError("MODERROR [GratefulPets] - …")`
* otherwise → `UnityEngine.Debug.LogError("ERROR  - …")`
* both also raise a blocking popup when `Options.ShowErrorPopups` is on

Grepped `Player.log`, `Player-prev.log`, `game_log.txt` for `untakeable`, `graveyard object`,
`invalid object`, `Attempting to add`, `MODERROR`, `ERROR  -`: **nothing** (the only `ERROR  -`
line is an unrelated `ErosTeleportation` mutation-registration warning that predates this work).

So the pellet object was **not** rejected by `AddObject`. It was added.

### 1.5 So what actually differs between the two paths? *(verified)*

| | pellets | gift |
|---|---|---|
| create | `GameObject.Create` inside `TakeObject(string,int)` | `GameObject.Create(blueprint)` in the mod |
| stack | **`Stacker.StackCount = 5` set *before* the take** | `StackCount == 1` |
| give | `TakeObject(GameObject, NoStack:false, Silent:true, EnergyCost:0, …)` | `ReceiveObject(GameObject)` → *exactly the same call* |
| result | not visible | visible |

The two paths converge on an **identical** `TakeObject(GameObject, …)`. The only difference is
that the pellet object is handed over as a stack of 5 rather than a single item.

**Current best hypothesis** *(inferred, not proven)*: the failure is in the stacking branch —
either `CheckStacks()`/`CheckInventoryForStacking` (`Stacker.cs:276`) merging the fresh stack
into something that then disappears, or `_StackCount` being `[NonSerialized]`
(`Stacker.cs:13`) and surviving only via the part's custom `Write`/`Read` (`Stacker.cs:103`).
Not confirmed either way.

**Decision:** stop reading, start measuring. The mod will (a) hand pellets over via the
*proven* per-object path — a loop of `GameObject.Create` + `ReceiveObject(GameObject)`, which
is byte-for-byte the call the gift uses — and (b) write a debug line recording the return
value **and** the pellet objects actually found in `Actor.GetInventory()` immediately after.
One feed, one log line, root cause.

### 1.6 Save-file forensics — mostly a dead end *(record so we don't repeat it)*

* Save path: `~/.config/unity3d/Freehold Games/CavesOfQud/Synced/Saves/<guid>/Primary.sav.gz`
  (plus `.bak`, `Checkpoint.sav.gz`, `CheckpointCache.db`).
* `gunzip -c` yields a **~3.2 MB custom binary**, magic `99 01 00 00`. It is *not* JSON —
  `file` misidentifies it as "OpenPGP Public Key", and `strings`/binary greps are the only way in.
* Strings are written **inline, length-prefixed** (one length byte when `< 128`), so
  `grep -a "hands you 5 desalination pellets"` on the raw save works — that is how 1.2 was
  established. The message log is in there as plain text.
* There is a contiguous run of **421** length-prefixed printable strings starting at offset
  `3068087` containing `Desalination Pellet` (index 35) and `Rough Topaz` (index 36) back to
  back. It looks like a blueprint/string table, but there is **no count header** before it and
  references could not be decoded as `int32`/`int16` indices (counts were meaningless — e.g.
  index 35 appeared 88× as `int32`, 632× as `int16`).
* **Conclusion:** the save format is not worth reverse-engineering for this. The presence of
  `Desalination Pellet` in that run is *suggestive* (one pellet object is referenced somewhere)
  but proves nothing about *where* it is. Use the debug log instead.

---

## Part 2 — The feed redesign (user-specified)

### 2.1 The spec, as given

1. Choosing **feed** on a pet opens a **selection of the food you are carrying**.
2. Entries are **sorted by how many of that type you have already fed that pet**, with the
   **number in brackets**: `beetle jerky [3]`.
3. Feed a pet too much of one thing and it **tires** of it → **high chance of no valuable item**.
4. **Pellets are always given**, tired or not.
5. The pet **exclaims** how much it loves or loathes the food.
6. **From the inventory screen**, if a pet is nearby, you can pick a food item and feed it.

### 2.2 Decisions taken (Q&A, 2026-11-07)

| Question | Decision |
|---|---|
| Tally scope | **Per pet** — each pet keeps its own taste history |
| Tiring | **Tired at 5**, and it **wears off** (−1 per in-game day) |
| Tired payoff | **Gift is downgraded** — always a gift, but rolled from a junk tier; pellets unaffected |
| Inventory-side feeding, 2+ pets adjacent | **Ask which pet**; feed directly when only one is adjacent |

### 2.3 Where the tally lives *(verified possible)*

`GameObject.Property` is a `Dictionary<string,string>` and **is** written and read by the save
serializer — `GameObject.cs:1916` (`Writer.WriteOptimized(Property.Count)`) and
`GameObject.cs:2060` (`Property.Add(Reader.ReadOptimizedString(), …)`).

So per-pet state can be a plain property on the pet:

```csharp
pet.SetStringProperty("GratefulPets.FeedLog", "Beetle Jerky\t7\t41200\nWatervine\t2\t41205");
pet.GetStringProperty("GratefulPets.FeedLog", "");
```

One property holding a `\n`-separated list of `blueprint \t count \t day` is used rather than
one property per blueprint, so keys can't collide with blueprint names that contain spaces.

* `SetStringProperty` / `GetStringProperty` — `GameObject.cs:4944` / `:4935`
* `SetIntProperty` / `GetIntProperty` — `GameObject.cs:4977` (separate `IntProperty` dict,
  also serialized at `:1929` / `:2069`)
* decay is computed on read: `effective = max(0, storedCount - (todayDay - storedDay))`, then
  re-stored with `todayDay` on every feed.

**Day number** comes from `XRL.World.Calendar` (`Calendar.cs`):

```
TotalTimeTicks => XRLCore.Core.Game.TimeTicks
TurnsPerDay    = 1200      TurnsPerHour = 50      TurnsPerYear = 438000
```

→ `long today = Calendar.TotalTimeTicks / Calendar.TurnsPerDay;`

### 2.4 APIs chosen for the two pickers *(verified signatures)*

```csharp
Popup.PickGameObject(
    string Title,
    IList<GameObject> Objects,
    bool AllowEscape = false,
    bool ShowContext = false,
    bool UseYourself = true,
    Func<GameObject, string> ExtraLabels = null,   // rendered as  name [label]
    int DefaultSelected = 0,
    bool ShortDisplayNames = false)
    → GameObject   // null when the player escapes
```

`Popup.cs:1521`. The `ExtraLabels` return value is spliced in as
`text + " [" + text2 + "]"` (`Popup.cs:1547`) — that is **exactly** the requested
`beetle jerky [3]` format, so no custom menu renderer is needed. Returns `null` on escape, so
it doubles as cancel handling.

This is the same call `Beguiling`, `Persuasion_Proselytize`, `Tinkering_*` and `Campfire` use,
so it is proven in-game (and it handles the a/b/c hotkey column itself).

Alternative kept in reserve: `Popup.ShowOptionList(...)` (`Popup.cs:1646`) — returns `int`,
`-1` on escape.

**Food picker:** group inventory foods by blueprint, sort ascending by effective fed-count
(least-fed first = best value), tie-break on display name, pass the representatives to
`PickGameObject` with `ExtraLabels = o => effectiveCount.ToString()`.

**Pet picker (inventory-side feed):** collect adjacent pets, `PickGameObject` only when
`count > 1`, otherwise take `[0]` — the idiom `Beguiling.cs:145` already uses.

### 2.5 Two commands, one handler

| Command | Fired from | `E.Item` / `E.Object` |
|---|---|---|
| `FeedPet` | twiddle menu **on the pet** → food picker | `E.Item` = pet |
| `FeedPetFood` | twiddle menu **on a food item** in inventory → pet picker | `E.Item` = food |

Both are added from `OwnerGetInventoryActionsEvent` on the player part (the event fires for
inventory items too — that is how `Food` attaches its own `eat` action), and both are resolved
in the same `InventoryActionEvent` handler with `FireOnActor: true`.

`OwnerGetInventoryActionsEvent` handlers in the wild: `Telekinesis`, `Psychometry`,
`ElectricalGeneration`, `Tinkering_Repair`, `Tinkering_Disassemble`, `CyberneticsMedassistModule`.

### 2.6 Adjacent pets

* `GameObject.DistanceTo(GameObject)` — `GameObject.cs:3715` (returns `9999999` off-zone;
  the existing `FEED_RANGE = 1` check already relies on this)
* `Cell.GetLocalAdjacentCells(int Radius, bool IncludeSelf = false)` — `Cell.cs:7434`
* `Cell.GetObjects()` / `GetObjects(Predicate<GameObject>)` — `Cell.cs:4854` / `:4940`
* `GameObject.GetInventory()` — `GameObject.cs:5360`

---

## Part 3 — Two things that were *wrong* in my assumptions

### 3.1 `food.Destroy()` on a stack does **not** eat the whole stack *(verified — assumption was wrong)*

I assumed the current code's `food.Destroy()` obliterated a whole stack of 5 beetle jerky,
making the feed cost 5 food. **It does not.** `Stacker` subscribes to
`BeforeDestroyObjectEvent` (`Stacker.cs:204`):

```csharp
public override bool HandleEvent(BeforeDestroyObjectEvent E)
{
    if (!E.Obliterate && StackCount > 1)
    {
        StackCount--;
        ParentObject?.InInventory?.Inventory?.FlushWeightCache();
        return false;              // cancels the destruction
    }
    return base.HandleEvent(E);
}
```

`Destroy()` → `BeforeDestroyObjectEvent.Check(...)` returns `false` → `Destroy` returns `false`
and the object survives with `StackCount - 1`. **The cost is already exactly 1 item.** No fix
needed; record it so nobody "fixes" it later.

### 3.2 `Player.log` is a usable oracle *(verified)*

Earlier I treated `Player.log` as Unity noise. It is not — it is where every
`UnityEngine.Debug.LogError` lands, including `MetricsManager`'s, which is where all of the
game's "silently didn't work" diagnostics go. Worth grepping before theorising.

Note also: the game was **running** throughout this investigation (`Player.log` and the saves
were being rewritten mid-session), so any rebuilt mod needs a **restart** to be compiled.

---

## Part 4 — Next steps

1. Rewrite `mod/Scripts/GratefulPets.cs`:
   * per-pet feed log on `Property`, with day-based decay;
   * food picker via `Popup.PickGameObject` + `ExtraLabels`, sorted least-fed first;
   * love / loathe / tired exclamations;
   * gift table split into a **full tier** and a **junk tier** (tired → junk tier);
   * pellets handed over one object at a time (the proven path) **plus** a debug log line
     recording `pellets` return value and the pellet objects/stack counts actually present in
     `Actor.GetInventory()` immediately after;
   * second command `FeedPetFood` for the inventory-side flow, with the pet picker.
2. Install to `~/.config/unity3d/Freehold Games/CavesOfQud/Mods/GratefulPets/`, restart the
   game, read `build_log.txt` for `Success :)`.
3. Have a pet fed once → read the debug log → **that** tells us where the pellets go.
4. Commit + push to `ap0ught/grateful-pets`; update `00-index.md`, `07`, `08`, `09`.

## Open questions

* Root cause of the missing pellets (1.5) — **unknown**, instrumenting rather than guessing.
* Whether "wears off" should be −1/day from *that pet's* last feed of that food (current plan)
  or from a global clock. Per-pet-per-food is the natural reading of "per pet" and is what
  2.3 implements.
* Junk tier contents: not yet chosen. Natural candidates from the existing table are the four
  nuggets and the rough (unpolished) stones; `Polished *`, figurines, `UbernostrumTonic` and
  `Chiral Rings` stay in the full tier.

## 1.7 Root cause — resolved (user report, 2026-11-07)

There was **no inventory bug**. `Desalination Pellet` is an `Examiner` artifact:

```xml
<!-- Base/ObjectBlueprints/Items.xml:11446 / Data.xml:37 -->
<part Name="Examiner" Complexity="2" Unknown="UnknownOddTrinket" />
```

`UnknownOddTrinket` (`BaseUnknown`) renders as an **"odd trinket"** with an unidentified tile/description until the player has identified/understood that kind of item (epistemic status/`UnderstandingTable` in `XRL.World.Parts/Examiner.cs`). The pellet *was* received (the message and the `ReceiveObject` return value were both truthful); it just wasn't recognisable in inventory yet. **Lesson:** a "missing" reward can be an identification/epistemic-status issue — check `Examiner`/`Unknown` before chasing inventory code.

## Part 5 — Pellet pacing (digestion) redesign

### 5.1 Requirement (user-specified, 2026-11-07)

* Feeding produces **1 pellet right away**, then the **other 4 over the course of the day** while digesting.
* Each pellet is given to the player **when it has produced it** (not all at once).
* The **treasure is given right away** as thanks (unchanged).
* Clarification: the pet **produces the pellet locally, then has a desire to approach and share it**.

### 5.2 Key native precedent — `GiveATreatToPartyLeader` *(verified, decomp)*

`XRL.World.AI.GoalHandlers/GiveATreatToPartyLeader.cs` is almost exactly this behavior:

```csharp
public override void TakeAction()
{
    if (!ParentObject.IsPlayerLed()) { Pop(); return; }
    if (ParentObject.DistanceTo(The.Player) <= 1)
    {
        GameObject treat = GameObject.Create(PopulationManager.RollOneFrom(treatTable).Blueprint);
        Messaging.XDidYToZ(ParentObject, "give", null, The.Player, "a treat");
        The.Player.ReceiveObject(treat);
        Pop();
    }
    else
    {
        ParentBrain.PushGoal(new MoveTo(The.Player, careful: false, overridesCombat: false, 1));
    }
}
```

Triggered by `XRL.World.Parts/GivesTreat` (the dead hook noted in entry 05) via
`ParentObject.Brain.PushGoal(new GiveATreatToPartyLeader(TreatTable))`, guarded so it doesn't
stack (`!Goals.Items.Any(i => i is GiveATreatToPartyLeader)`).

**Implications for the redesign:**
* "Desire to approach and share" = push a `MoveTo(The.Player, ..., shortBy:1)` / delivery goal
  on the pet's `Brain` (`Brain.PushGoal`, `GameObject.DistanceTo`, `Brain.cs:1382`).
* **Produce locally** means the pellet object is created into the **pet's** inventory/context
  when due (not granted directly to the player), and transferred to the player on delivery
  (`The.Player.ReceiveObject(pellet)`) when adjacent.
* State (remaining pellets + next-production tick) must live/persist with the **pet** (a
  `[Serializable]` part on the pet, or pet `Property` — `Property` is serialized, `GameObject.cs:1916/2060`).
  Production timing can run off the pet's `WantTurnTick`/`TurnTick(long TimeTick, int Amount)`
  (`IComponent.cs:4272/4280`) or `SingletonEvent<EndTurnEvent>.ID` handled on the pet.
* Delivery runs when produced-while-carried *and* adjacent: if not adjacent, keep the pellet
  held by the pet and (re)push an approach/deliver goal so the pet's follow + goal brings it
  into range — mirroring `GiveATreatToPartyLeader.TakeAction`.
* 4 pellets over a day: `Calendar.TurnsPerDay == 1200` (`Calendar.cs:13`), so interval
  **300 turns** → pellets at now+300, +600, +900, +1200 (1 was given immediately).

### 5.3 Interim implementation note (superseded by 5.2)

A first pass added a **player-side** digestion tick (state on the player `GratefulPets.Digestions`
property, processed on the player's `EndTurnEvent`, granting pellets directly to the player on a
300-turn schedule). That satisfied the *timing* but **not** "produce locally, approach and share".
It must be reworked to the pet-side/local-production + approach/deliver model above, ideally
reusing the `GiveATreatToPartyLeader`/`MoveTo` goal pattern rather than inventing delivery.

## Open questions (updated)

* `GoalHandler` subclasses defined by mods: confirm they serialize/deserialize safely when pushed
  onto a brain (saves mid-goal). If risky, prefer a `[Serializable]` part on the pet driving
  `MoveTo` pushes instead of a custom saved `GoalHandler`.
* Whether a pet that is dead, lost, or in another zone should still "produce" pellets (hold for
  later delivery vs. forfeit). Simplest/safest: keep the schedule on the pet part; if the pet
  object is gone, pellets are forfeit — but confirm this matches intent.
* Distance/zone rules for delivery: require `DistanceTo(The.Player) <= 1` and same context as
  `GiveATreatToPartyLeader` (it checks `IsPlayerLed()` and adjacency) — sensible default.

## Part 6 — Status (implemented, not yet compile-verified in this pass)

Implemented in `mod/Scripts/GratefulPets.cs` and synced to
`~/.config/unity3d/Freehold Games/CavesOfQud/Mods/GratefulPets/`:

* Food picker on pet feed (`Popup.PickGameObject` + `ExtraLabels`) sorted by effective per-pet
  feed count `[n]`; per-pet taste log in serialized property `GratefulPets.FeedLog` with
  −1/day decay (`Calendar.TotalTimeTicks / TurnsPerDay`); tired ≥ 5 ⇒ downgraded (junk) gift;
  pellets never downgraded; love/weary/loathe exclamations.
* Inventory-side feeding: `Feed to pet` on a carried food item; 1 adjacent pet → direct,
  2+ → pet picker.
* `apply desalination pellet` on the player's **carried** liquid containers and on **adjacent
  open liquid pools** (uses `DesalinationPellet.PurifyLiquid`, vanilla message path).
* Digestion pacing reworked to **local production + approach/share** (Part 5.2): treasure now;
  1 pellet produced locally and shared immediately if adjacent; 4 more produced into the pet on
  a 300-turn schedule via `[Serializable]` `GratefulPetsDigestion` and delivered when adjacent,
  nudging with `MoveTo(The.Player, shortBy:1)`.

**Not verified here:** in-game compile/runtime (game was running earlier; needs restart, then
`build_log.txt` → `GRATEFUL PETS` / `Success :)`) and the approach/deliver feel across zones,
death/un-led states, and message spam. Treat behavior as implemented-but-unconfirmed until a
fresh launch + live test.

* **Chat gate (2026-11-07, rolled back):** attempted to block feeding when chat is unavailable via `CanStartConversationEvent.Check`. Found that "too busy (fighting/on fire)" is enforced by `ConversationScript.AttemptConversation` (not `CanStartConversationEvent.Check`), and feeding a combat-busy pet was intentionally possible before — so the gate was rolled back per user choice; feeding remains available to real pets as before. Revisit only with a precise "fundamentally can't converse" rule if needed.

* **Feed moved under chat (2026-11-07):** removed the top-level pet twiddle "Feed" action and inject a `"Let me feed you."` choice (`GratefulPets_Feed`, `Target=End`) into the pet conversation `StartNode` on `BeginConversationEvent` (player part receives via `IConversationMinEvent.DispatchAll`). Choice runs `GratefulPetsFeedChoicePart` (`EnterElementEvent`) → `DoFeedPet(The.Player, The.Speaker)`. This inherits chat's gating for free (ConversationScript busy/fighting/on-fire guards, range/can-start flow). Inventory "Feed to pet" on carried food items remains. **Compile/runtime unverified.**

## Part 7 — Fixes after live testing (2026-11-07)

* **Compile fix (CS0246):** `GratefulPetsFeedChoicePart` lives in `XRL.World.Conversations.Parts` but was referenced unqualified from `XRL.World.Parts.GratefulPets.InjectFeedChoice` — fully qualified the runtime instantiation.
* **Immediate pellet not delivered (user repro: treasure given, no pellet):** `GratefulPetsDigestion.TryDeliverOrApproach` returned early on `!ParentObject.IsPlayerLed()`, so the locally-produced pellet was never handed over (and the “1 now” share failed). **Fix:** deliver when `DistanceTo(player) <= 1` regardless of `IsPlayerLed()`; only push the `MoveTo` approach nudge when not adjacent *and* player-led. User confirmed: “that works much better” (immediate treasure + pellet handoff). *Note:* that specific pre-fix feeding ran old compiled code, so its immediate pellet isn’t retroactive.
* **Digestion-over-time (4 pellets / 300 turns) still user-testing** (natural wait). Remainder of Part 6’s “not verified” still applies to cross-zone/death/un-led edge cases and message spam.
