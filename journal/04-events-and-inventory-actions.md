# 04 — Events & inventory actions *(the important one)*

This is the entry the whole session pivoted on. The game has **no "feed" verb**, so one had to be
invented, and inventing a verb means understanding exactly how verbs are attached and executed.

Everything here was read out of `decomp/`, not guessed.

---

## 1. Two event systems coexist

**Legacy:** string IDs, `Event.New("Name", "Key", value, …)`, `obj.FireEvent(e)`, handlers
registered by name via `Registrar.Register("Name")`, gated with `GO.HasRegisteredEvent("Name")`.

**Modern (prefer this):** pooled `MinEvent` subclasses with a static `ID`.

```csharp
[GameEvent(Cache = Cache.Pool)]                       // or [GameEvent(Base = true)]
public class MyEvent : MinEvent
{
    public new static readonly int ID = MinEvent.RegisterEvent(typeof(MyEvent), null, CountPool, ResetPool);
    public static List<MyEvent> Pool;  public static int PoolCounter;
    public static MyEvent FromPool() { … }
    public static void ResetTo(ref MyEvent E) { … }   // returns it to the pool
    public override bool Dispatch(IEventHandler Handler) => base.Dispatch(Handler) && Handler.HandleEvent(this);
    public static void Send(…) { var E = FromPool(); …; target.HandleEvent(E); ResetTo(ref E); }
}
```

Consumers implement two methods on `IPart` (or any `IComponent`):

```csharp
public override bool WantEvent(int ID, int cascade)
    => base.WantEvent(ID, cascade) || ID == MyEvent.ID;

public override bool HandleEvent(MyEvent E) { …; return base.HandleEvent(E); }
```

Events dispatch **to the parts of one specific object**. Which object depends entirely on who calls
`Send`/`HandleEvent` — that's the whole game.

Useful base types:

- `IActOnItemEvent` → fields `GameObject Actor`, `GameObject Item`
- `IInventoryActionsEvent` → fields `GameObject Actor`, `GameObject Object`, `Dictionary<string, InventoryAction> Actions`
- `MinEvent` → `RequestInterfaceExit()`, `InterfaceExitRequested()`, `CascadeLevel`

---

## 2. Where actions come from

`EquipmentAPI.GetInventoryActions(GameObject GO, GameObject Actor, …)`
(`decomp/Qud.API/EquipmentAPI.cs:378`):

```csharp
if (GO.HasTag("NoInventoryActions")) return null;
…
// legacy string events first
if (GO.HasRegisteredEvent("GetInventoryActions"))       GO.FireEvent(Event.New("GetInventoryActions", "Actions", value));
if (GO.HasRegisteredEvent("GetInventoryActionsAlways")) GO.FireEvent(Event.New("GetInventoryActionsAlways", …));
if (Actor.HasRegisteredEvent("OwnerGetInventoryActions")) Actor.FireEvent(Event.New("OwnerGetInventoryActions", "Actions", value, "Object", GO));
// then the modern ones
GetInventoryActionsEvent.Send(Actor, GO, table);            // → GO.HandleEvent
GetInventoryActionsAlwaysEvent.Send(Actor, GO, table);      // → GO.HandleEvent
OwnerGetInventoryActionsEvent.Send(Actor, GO, table);       // → ACTOR.HandleEvent   ← !!
```

And `OwnerGetInventoryActionsEvent.Send` (`decomp/XRL.World/OwnerGetInventoryActionsEvent.cs:63`):

```csharp
public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
{
    var E = FromPool();
    E.Actor = Actor; E.Object = Object; E.Actions = Actions;
    Actor.HandleEvent(E);      // ← the ACTOR (normally the player), with E.Object = the twiddlee
    ResetTo(ref E);
}
```

**So a part on the *player* can put a verb on *any other object's* menu.** That is the trick.

`OwnerGetInventoryActionsEvent.CascadeLevel = 273`, `InventoryActionEvent.CascadeLevel = 256`.

---

## 3. Where actions get executed

`EquipmentAPI.TwiddleObject(Owner, GO, ref Done, out InventoryAction, Distant, TelekineticOnly, …)`
(`decomp/Qud.API/EquipmentAPI.cs:154`) is the loop behind the "twiddle" popup (examine → actions,
right-click, interact-nearby):

```csharp
while (true) {
    var table = new Dictionary<string, InventoryAction>(16);
    …build table (three Sends above)…
    var action = ShowInventoryActionMenu(table, Owner, GO, Distant, …);
    if (action == null) { GO.CheckStack(); break; }                 // player cancelled
    if (!action.IsUsable(Owner, GO, Distant, out telekinetic)) { Popup.Show("You cannot do that from here."); continue; }
    IEvent result = action.Process(GO, Owner, Telekinetic);
    if (result != null && result.InterfaceExitRequested()) { Done = true; break; }
    // if the target didn't move/change state, loop → the menu REOPENS
    if (!GO.IsInvalid() && cellSame && invSame && equipSame) continue;
    break;
}
```

**Two consequences:**
- If you don't call `E.RequestInterfaceExit()`, the menu re-opens after your action runs.
- `IsUsable` only rejects you when `Distant == true`; anything with `WorksAtDistance` /
  `WorksTelekinetically` / `WorksTelepathically` false is hidden from distant twiddles.

### `InventoryAction.Process` — the fork in the road

`decomp/XRL.World/InventoryAction.cs:116`:

```csharp
public IEvent Process(GameObject GO, GameObject Owner, bool Telekinetic = false)
{
    Owner.FireEvent(Event.New("InvCommandActivating", "Object", GO));
    GameObject target = FireOnActor ? Owner : (FireOn ?? GO);      // ← WHO the event lands on
    IEvent GeneratedEvent;
    if (AsMinEvent)                                                // default true
        InventoryActionEvent.Check(out GeneratedEvent, target, Owner, GO, Command, …);
    else
        target.FireEvent(Event.New(Command, "Object", GO, "Owner", Owner));   // legacy string event
    return GeneratedEvent;
}
```

And `InventoryActionEvent.Check(out E, Object, Actor, Item, Command, …)` simply does
`Object.HandleEvent(E)`, then fires `AfterInventoryActionEvent.Send(Object, E)` and
`OwnerAfterInventoryActionEvent.Send(Actor, E)`.

| `FireOnActor` | `target` | who handles `InventoryActionEvent` | `E.Actor` | `E.Item` |
|---|---|---|---|---|
| `false` (default) | `GO` — the twiddlee | a part **on the object** | player | the object |
| **`true`** | `Owner` — the player | a part **on the player** | player | **the object** |

---

## 4. The decision table

| Verb must appear… | Put your part on… | `AddAction(…, FireOnActor:)` | Handle `InventoryActionEvent` on… |
|---|---|---|---|
| in an object's own menu, code lives on that object | the object (XML `Load="Merge"`, or `AddPart` at runtime) | `false` | that object |
| in another object's menu, **code lives on the player** | the player | **`true`** | the player, using `E.Item` as the target |

**grateful-pets uses row 2.** No XML blueprint editing, no per-pet part injection, works for
base-game pets, DLC pets, and any pet another mod adds — as long as it carries the pet tags.

---

## 5. `AddAction` signature

`decomp/XRL.World/IInventoryActionsEvent.cs:27`:

```csharp
public bool AddAction(
    string Name,                       // dictionary key — must be unique per menu
    string Display = null,             // menu text
    string Command = null,             // what lands in InventoryActionEvent.Command
    string PreferToHighlight = null,   // substring to hotkey-highlight in Display
    char Key = ' ',
    bool FireOnActor = false,          // ← see table above
    int Default = 0,                   // sort weight, HIGHER FIRST
    int Priority = 0,
    bool Override = false,
    bool WorksAtDistance = false,      // visible/usable from far away
    bool WorksTelekinetically = false,
    bool WorksTelepathically = false,
    bool AsMinEvent = true,            // false → legacy string event instead
    GameObject FireOn = null,          // explicit target override
    bool ReturnToModernUI = false)
```

**Menu behaviour** (`ShowInventoryActionMenu`, `EquipmentAPI.cs:30`):
- actions are filtered by `IsVisible(Owner, GO, Distant)`, then sorted by
  `InventoryAction.Comparer` → `SortCompare`: keyed actions first, then by key char, then
  **`Default` descending** (`-a.Default.CompareTo(b.Default)`).
- hotkeys conflicting with `UINav`/`Menus` keybindings are blanked; two actions with the same key
  → the second silently loses its key (it stays clickable by mouse).
- so `Default: 10` puts your action above `Pettable`'s `"Pet"` (`Default: 5`).

---

## 6. Patterns already in the game (copy these)

**Base game — `Tinkering_Repair` (a skill part, i.e. it lives on the *player*):**

```csharp
// decomp/XRL.World.Parts.Skill/Tinkering_Repair.cs:33
public override bool HandleEvent(OwnerGetInventoryActionsEvent E) {
    E.AddAction("Repair", "repair", "Repair", null, 'R', FireOnActor: true, 25);
    return base.HandleEvent(E);
}
public override bool HandleEvent(InventoryActionEvent E) {
    if (E.Command == "Repair") { …E.Item is the broken object… }
}
```

Exactly the shape grateful-pets needs.

**Base game — `Pettable` (a part on the *pet*):**

```csharp
// decomp/XRL.World.Parts/Pettable.cs
public override bool WantEvent(int ID, int cascade)
    => base.WantEvent(ID, cascade) || ID == CanSmartUseEvent.ID || ID == CommandSmartUseEvent.ID
       || ID == GetInventoryActionsEvent.ID || ID == IdleQueryEvent.ID || ID == InventoryActionEvent.ID;

public override bool HandleEvent(GetInventoryActionsEvent E) {
    E.AddAction("Pet", "pet", "Pet", null, 'p', FireOnActor: false, 5);
    return base.HandleEvent(E);
}
public override bool HandleEvent(InventoryActionEvent E) {
    if (E.Command == "Pet" && Pet(E.Actor)) E.RequestInterfaceExit();
    return base.HandleEvent(E);
}
```

**Third-party mod — `ManageLiquids`** (`workshop 3316416182`) uses
`OwnerGetInventoryActionsEvent` **without** `FireOnActor`, because its action lives on the player's
own inventory (so `GO == Owner == player` either way). It is the cleanest end-to-end example of
part + `WantEvent` + both handlers and is preserved at `reference/examples/ManageLiquids.cs`.

---

## 7. Rules of thumb learned

1. `WantEvent` must return true for **every** event you handle, or the handler is never called.
2. `HandleEvent` returning `false` aborts the dispatch chain and makes `Check` report failure —
   return `false` only when you want the action to visibly *fail*.
3. `E.RequestInterfaceExit()` closes the twiddle menu; omit it and the menu re-opens.
4. Never touch a `GameObject` you have already `Destroy()`ed — pooled events validate their
   arguments (`GameObject.Validate(ref Object)`) and will silently do nothing.
5. Owner-scoped events (`OwnerGet*`, `OwnerAfter*`) always fire on the actor, **even when the
   action was opened on something else** — always filter on `E.Object` / `E.Item`.
