# reference/

Curated extracts that are **not** part of the game's own assembly — the things worth keeping
alongside `../decomp/` without making you dig for them.

| Path | What it is |
|---|---|
| [`types.txt`](types.txt) | `ilspycmd -l c` output for the whole game assembly — **7,072 class names**. The fastest way to answer *"does the game already have a thing for X?"* before writing any code. |
| [`examples/ManageLiquids.cs`](examples/ManageLiquids.cs) | A working third-party workshop mod (`3316416182`) that is the cleanest end-to-end example of the pattern grateful-pets uses: `[PlayerMutator]` + `[HasCallAfterGameLoaded]` + a stateless part on the player handling `OwnerGetInventoryActionsEvent` and `InventoryActionEvent`. Unmodified. |

## For everything else, use `../decomp/`

The journal cites paths inside `decomp/` directly. The classes that matter most for this project:

| Question | Look at |
|---|---|
| How do verbs get attached to an object? | `Qud.API/EquipmentAPI.cs` — `GetInventoryActions` (:378), `TwiddleObject` (:154), `ShowInventoryActionMenu` (:30) |
| Who does an action's event land on? | `XRL.World/InventoryAction.cs` — `Process` (:116) — the `FireOnActor` fork |
| How do I add an action from the player? | `XRL.World/OwnerGetInventoryActionsEvent.cs` — `Send` (:63) |
| What does a finished part look like? | `XRL.World.Parts/Pettable.cs`, `XRL.World.Parts/Food.cs`, `XRL.World.Parts.Skill/Tinkering_Repair.cs` |
| How does the game give the player an item? | `XRL.World/GameObject.cs` — `ReceiveObject` (:6087–6097), `TakeObject` (:5995–6060) |
| The unused gift mechanic I based the flavour on | `XRL.World.Parts/GivesTreat.cs`, `XRL.World.AI.GoalHandlers/GiveATreatToPartyLeader.cs` |
| Mod loading & compilation | `XRL/ModManager.cs`, `XRL/ModInfo.cs`, `XRL/ModFile.cs` |
| Blueprint merging | `XRL.World.Loaders/ObjectBlueprintLoader.cs` (:790 `Load == "Merge"`) |
