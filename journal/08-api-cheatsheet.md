# 08 — API cheatsheet (verified)

Every signature below was read out of `decomp/`. Paths are relative to `decomp/`.

## Objects

```csharp
// XRL.World/GameObject.cs
public static GameObject Create(string Blueprint, int BonusModChance = 0, int SetModNumber = 0,
    string AutoMod = null, Action<GameObject> BeforeObjectCreated = null,
    Action<GameObject> AfterObjectCreated = null, string Context = null,
    List<GameObject> ProvideInventory = null);              // :2300
public static GameObject CreateUnmodified(string Blueprint, string Context = null, …); // :2320

public bool ReceiveObject(GameObject Object, bool NoStack = false, string Context = null);   // :6087
public bool ReceiveObject(string Blueprint, bool NoStack = false, …);                       // :6092
public int  ReceiveObject(string Blueprint, int Number, bool NoStack = false, …);           // :6097
public int  ReceiveObjectsFromPopulation(string Table, int Number, …);                      // :6077
// ⚠ all of these route to TakeObject(..., Silent: true, …) → NO pickup message is shown.
//   You must emit your own message.

public bool Destroy(string Reason = null, bool Silent = false, bool Obliterate = false,
                    string ThirdPersonReason = null);   // :4046  (Obliterate() wraps it)

public bool HasTag(string TagName);                     // :2919
public string GetTag(string Tag, string Default = null);// :9840
public bool HasTagOrProperty(string Name);              // :9845
public bool HasPart<T>();  public T GetPart<T>();
public bool RemovePart<T>(); public void AddPart… / RequirePart<T>()

public List<GameObject> GetInventory();                            // :5360
public List<GameObject> GetInventory(Predicate<GameObject> Filter);// :5375
public List<GameObject> GetInventoryAndEquipment();                // :5409
public bool IsPlayerLed();                                         // :13223 → Brain
public bool IsImportant();                                         // :9284
public int  DistanceTo(GameObject Object);                         // :3715
public bool IsInvalid(); public static bool Validate(ref GameObject O);
public void RemoveFromContext(IEvent ParentEvent = null);          // :3032

public bool UseEnergy(int Amount, string Type, string Context = null, int? MoveSpeed = null,
                      bool Passive = false);                       // :3646   ← costs a turn
public void UseEnergy(int Amount);                                 // :3678
```

Removing an item from your inventory specifically:

```csharp
// XRL.World.Parts/Inventory.cs:333  (the Inventory *part*, not GameObject)
public bool RemoveObjectFromInventory(GameObject Object, GameObject Actor = null,
    bool Silent = false, bool NoStack = false, bool FlushTransient = true,
    string Context = null, IEvent ParentEvent = null);
public void RemoveObject(GameObject Object, IEvent ParentEvent = null);   // :350
```
Or just `item.Destroy()` — which is what the base `Food` part does when you eat something.

## Names & grammar

```csharp
// XRL.World/GameObject.cs
public string GetDisplayName(int Cutoff = int.MaxValue, string Base = null, string Context = null,
    bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false,
    bool Stripped = false, bool ColorOnly = false, bool Visible = true, bool WithoutTitles = false,
    bool ForSort = false, bool Short = false, bool BaseOnly = false,
    bool WithIndefiniteArticle = false, bool WithDefiniteArticle = false,
    string DefaultDefiniteArticle = null, bool IndicateHidden = false, bool Capitalize = false,
    bool SecondPerson = false, bool Reflexive = false, bool? IncludeAdjunctNoun = null,
    bool AsPossessed = false, GameObject AsPossessedBy = null, bool Reference = false,
    bool IncludeImplantPrefix = true);                      // :7026

public string t(int Cutoff = int.MaxValue, …);              // :7047  short, article-aware ("a berry")
public string an(…);                                        // :7037  indefinite article
public string one(…, bool WithIndefiniteArticle = false, …);// :7057
public string GetVerb(string Verb, bool PrependSpace = true, …); // :7684  → "gobbles"
```

```csharp
// XRL.World.Capabilities/Messaging.cs:200
public static void XDidYToZ(GameObject Actor, string Verb, string Preposition = null,
    GameObject Object = null, string Extra = null, string EndMark = null, …);
//  • validates Object first — returns silently if Object has been destroyed
//  • Actor == player  → "You <verb> …"
//  • Object == player → renders "you" / "your"
//  • base-game example: Messaging.XDidYToZ(pet, "give", null, The.Player, "a treat")
//                       → "E-Ros gives you a treat."
public static void XDidYToZ(string SubjectOverride, string Verb, …);   // string-subject overload

IComponent<GameObject>.AddPlayerMessage(string Message);   // plain message line
XRL.Messages.MessageQueue.AddPlayerMessage(string);        // lower-level queue
```

## Events

```csharp
// XRL.World/GetInventoryActionsEvent.cs   — dispatched on the TWIDDLEE
public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions);

// XRL.World/OwnerGetInventoryActionsEvent.cs:63  — dispatched on the ACTOR (player)
public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
{ …; Actor.HandleEvent(E); … }   // E.Actor = actor, E.Object = twiddlee

// XRL.World/IInventoryActionsEvent.cs:27
public bool AddAction(string Name, string Display = null, string Command = null,
    string PreferToHighlight = null, char Key = ' ', bool FireOnActor = false,
    int Default = 0, int Priority = 0, bool Override = false, bool WorksAtDistance = false,
    bool WorksTelekinetically = false, bool WorksTelepathically = false,
    bool AsMinEvent = true, GameObject FireOn = null, bool ReturnToModernUI = false);

// XRL.World/InventoryActionEvent.cs — :116 in XRL.World/InventoryAction.cs is what dispatches it
public static bool Check(out InventoryActionEvent E, GameObject Object, GameObject Actor,
    GameObject Item, string Command, bool Auto = false, bool OwnershipHandled = false,
    bool OverrideEnergyCost = false, bool Forced = false, bool Silent = false,
    int EnergyCostOverride = 0, int MinimumCharge = 0, int StandoffDistance = 0,
    GameObject ObjectTarget = null, Cell CellTarget = null, Cell FromCell = null,
    IInventory InventoryTarget = null);
// fields: E.Command, E.Actor, E.Item, E.RequestInterfaceExit(), E.Auto, E.Forced, …

// IPart contract
public override bool WantEvent(int ID, int cascade);
public override bool HandleEvent(SomeEvent E);
```

## Utilities

```csharp
XRL.Rules.Stat.Random(int Min, int Max)          // inclusive on both ends
XRL.PopulationManager.RollOneFrom(string PopulationName,
        Dictionary<string,string> Vars = null, string DefaultIfNull = null) → PopulationResult
GameObject.Validate(ref GameObject O)            // pooled-event argument guard
The.Player / The.PlayerCell / The.CurrentTurn / The.ActiveZone
XRLCore.Core?.Game?.Player?.Body                 // same as The.Player, but null-safe early
```

## Attributes for bootstrap

```csharp
[PlayerMutator]                       class → IPlayerMutator.mutate(GameObject player)   // XRL
[HasCallAfterGameLoadedAttribute]     class  +  [CallAfterGameLoadedAttribute] method    // XRL
    // method must be: public static void Name()   (no parameters)
[Serializable]                        on the part class
```

## Part base classes seen in the wild

```csharp
XRL.World.Parts.IPart          // stateless is fine; [Serializable]
XRL.World.Parts.IScribedPart   // needs custom persistence
XRL.World.Parts.IComponent<T>  // base of everything, provides AddPlayerMessage etc.
```
