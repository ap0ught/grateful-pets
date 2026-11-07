# 05 — Pets & companions

## What counts as a "pet"

Three signals, all present in real game data:

1. `<tag Name="Pet" />`
2. `<tag Name="StartingPet" />` — the tag every DLC starter pet carries
3. `<part Name="Pettable" />` — the "you may pet this" part (`decomp/XRL.World.Parts/Pettable.cs`)

Related but **not** the same thing:

- `GameObject.IsPlayerLed()` (`GameObject.cs:13223` → `Brain.IsPlayerLed()`) — anything following
  you, including proselytized/beguiled NPCs. The user explicitly chose **not** to include these.
- `XRL.World.Parts.Followers`, `Companion`, `HasCompanions`, `CompanionCapacity`, `BondedCompanion`
  — the party/follower machinery.
- `XRL.World.AI.GoalHandlers.Pet` — an *AI goal* (walk to target and `InventoryActionEvent "Pet"`),
  unrelated to pet-hood.
- `XRL.World.Parts.PetPhylactery`, `PetWraithSummoner` — templar/wraith content, name collision only.

grateful-pets' predicate:

```csharp
obj.HasTag("Pet") || obj.HasTag("StartingPet") || obj.HasPart<Pettable>()
```

## All 34 pet blueprints (base + DLC)

Produced by parsing every `<object>` in `StreamingAssets/Base/**` and `StreamingAssets/DLC/**`,
tagging any object whose own body has a pet signal **or** which transitively `Inherits` from one.

| Blueprint | Inherits | File |
|---|---|---|
| BaseCat | BaseAnimal | Base/ObjectBlueprints/Creatures.xml |
| BaseDog | BaseAnimal | Base/ObjectBlueprints/Creatures.xml |
| Astral Tabby | BaseCat | …Creatures.xml |
| Cat Golem | BaseCat | …Creatures.xml |
| Cats Cherub | BaseCat | …Creatures.xml |
| Chiliad Creature Cats | BaseCat | …HiddenObjects.xml |
| Chitinous Puma | BaseCat | …Creatures.xml |
| Mechanical Cats Cherub | BaseCat | …Creatures.xml |
| Ray Cat | BaseCat | …Creatures.xml |
| Ctesiphus | Ray Cat | …Creatures.xml |
| Farm Ray Cat | Ray Cat | …Creatures.xml |
| Joppa Ray Cat | Ray Cat | …Creatures.xml |
| Chiliad Creature Dogs | BaseDog | …HiddenObjects.xml |
| Dog | BaseDog | …Creatures.xml |
| Dog Golem | BaseDog | …Creatures.xml |
| Dogs Cherub | BaseDog | …Creatures.xml |
| Guard Dog | BaseDog | …Creatures.xml |
| Herding Dog | BaseDog | …Creatures.xml |
| Hyrkhound | BaseDog | …Creatures.xml |
| Mechanical Dogs Cherub | BaseDog | …Creatures.xml |
| Sheyd | BaseReptile | …HiddenObjects.xml |
| E-Ros | BaseRobot | DLC/PetsPack1/Freehold_Pet_E-Ros |
| Ebenshabat | BaseRobot | DLC/PetsPack1/Freehold_Pet_Ebenshabat |
| EitherOrPetSpawner | Widget | DLC/PetsPack1/Freehold_Pet_EitherOr |
| Ercolano | BaseChair | DLC/PetsPack1/Freehold_Pet_Ercolano |
| Esbear | BaseHumanoid | DLC/PetsPack1/Freehold_Pet_Esbear |
| Frondzie | MutatedVine | DLC/PetsPack1/Freehold_Pet_Frondzie |
| Gloaming | BaseBird | DLC/PetsPack1/Freehold_Pet_Gloaming |
| Herman | NPC | DLC/PetsPack1/Freehold_Pet_Herman |
| Hylaeus | BaseDog | DLC/PetsPack1/Freehold_Pet_Hylaeus |
| Iopinax | BaseAnimal | DLC/PetsPack1/Freehold_Pet_Iopinax |
| Lyy | BaseReptile | DLC/PetsPack1/Freehold_Pet_Lyy |
| Man Opener | BaseInsect | DLC/PetsPack1/Freehold_Pet_ManOpener |
| Uthabatafah | BaseAnimal | DLC/PetsPack1/Freehold_Pet_Uthabatafah |

Notes:
- `PetsPack1` is the "Pets of Harvest Dawn" DLC (`MOD_FREEHOLDGAMES_DLC_PETSPACK1` appears in
  `build_log.txt`), enabled on this machine.
- Every DLC pet carries `<tag Name="Pet"/>` **and** `<tag Name="StartingPet"/>`.
- Quirks worth knowing: `Ercolano` is a **chair** with a Pet tag; `EitherOrPetSpawner` is a
  spawner widget, not really a creature; `Herman` inherits `NPC`.
- Because grateful-pets keys off tags rather than a hardcoded list, **it covers this list plus any
  pet a mod adds** — no XML patching needed.

## The game already ships an unused gift mechanic

`GivesTreat` (`decomp/XRL.World.Parts/GivesTreat.cs`) + `GiveATreatToPartyLeader`
(`decomp/XRL.World.AI.GoalHandlers/GiveATreatToPartyLeader.cs`):

```csharp
// GivesTreat — a part with fields Chance = 1500, TreatTable = "PetMushroomTreats"
if (E.ID == "BeforeTakeAction" && ParentObject.CurrentCell != null
    && Stat.Random(1, Chance) <= 1
    && !ParentObject.Brain.Goals.Items.Any(g => g is GiveATreatToPartyLeader))
{
    ParentObject.Brain.PushGoal(new GiveATreatToPartyLeader(TreatTable));
    ParentObject.UseEnergy(1000);
}

// GiveATreatToPartyLeader.TakeAction()
if (!ParentObject.IsPlayerLed()) { Pop(); }
else if (ParentObject.DistanceTo(The.Player) <= 1) {
    var go = GameObject.Create(PopulationManager.RollOneFrom(treatTable).Blueprint);
    if (go != null) {
        Messaging.XDidYToZ(ParentObject, "give", null, The.Player, "a treat");
        The.Player.ReceiveObject(go);
    }
    Pop();
} else ParentBrain.PushGoal(new MoveTo(The.Player, careful: false, overridesCombat: false, 1));
```

**But nothing in the base game or any DLC actually uses it.** Verified by grepping all of
`StreamingAssets/` for `GivesTreat` and `PetMushroomTreats` → zero hits, and
`strings Assembly-CSharp.dll | grep PetMushroom` → nothing. The default `TreatTable`
`"PetMushroomTreats"` doesn't exist as a population table either.

So it's **dead code / an intended modding hook**. Useful as a reference for the exact idioms
(roll a table → `GameObject.Create` → `XDidYToZ` → `The.Player.ReceiveObject`), which is what
grateful-pets reuses — but grateful-pets does *not* piggyback on it, because it needs a player-
initiated verb, not a random AI impulse.

## There is no "feed" verb anywhere

Verified by grepping the whole of `StreamingAssets/` for `feed` across
`Commands.xml`, `ActivatedAbilities.xml`, `WishCommands.xml` and all object blueprints: only
flavour text ("snapjaw feeder", book prose). No `Feed` command, no ability, no inventory action.

Existing adjacent verbs worth knowing:
- `Pet` — from `Pettable`, key `p`, `FireOnActor: false`, `Default: 5`
- `Eat` — from `Food`, key `a`, `FireOnActor: false`, default priority 0/20
- `Feed To` — from `Tonic`, command `ApplyTo`, key `f` — **"feed a tonic to something"**, not
  "feed a pet". Note it already claims the `f` key, but it appears on *tonics*, not on creatures,
  so there is no hotkey clash in a pet's menu.
