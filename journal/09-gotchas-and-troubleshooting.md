# 09 — Gotchas & troubleshooting

##1. The game update broke an async API (Legacy Items fix)

**Symptom**

```
LegacyItemPart.cs(175,38): error CS1579: foreach statement cannot operate on variables of type
'Task<List<SaveGameInfo>>' because 'Task<List<SaveGameInfo>>' does not contain a public instance
or extension definition for 'GetEnumerator'
```
…and the mod produced a **0-byte** `LegacyItems.dll`.

**Cause:** the game changed `SavesAPI.GetSavedGameInfo()` from returning `List<SaveGameInfo>` to
`Task<List<SaveGameInfo>>`. The mod still `foreach`-ed the raw task.

**Fix** (applied at
`~/.local/share/Steam/steamapps/workshop/content/333640/3619655640/Scripts/LegacyItemPart.cs:171`):

```csharp
// before
var saves = SavesAPI.GetSavedGameInfo();
// after
var saves = SavesAPI.GetSavedGameInfo().GetAwaiter().GetResult();
```

**Verified** no other spot in the mod's 16 scripts misuses `Task`/`SavesAPI`.

> ⚠ `.GetAwaiter().GetResult()` can deadlock on a synchronization-context-bound thread. It works
> here (this code path is invoked from the mod's own UI/action flow, not from a Unity main-thread
> await), but if a future game update makes this a real problem, the proper fix is to make the
> caller async rather than block.
>
> ⚠ The **Steam workshop folder is re-downloaded/overwritten on update or re-verify**, so this
> patch is local-only and evaporates. That's exactly why it was forked into
> [`ap0ught/legacy-items`](https://github.com/ap0ught/legacy-items) — maintain the fix in git, not
> in `steamapps/workshop`.

## 2. `build_log.txt` is the only reliable compile check

Path: `~/.config/unity3d/Freehold Games/CavesOfQud/build_log.txt` (rotates to `.prev`).

- Success marker: `Success :)` followed by `Location: …/ModAssemblies/<ID>.dll`
- Failure marker: `Failure :(` then `== COMPILER ERRORS ==` then one line per error
- `Compiling N files...` tells you how many `.cs` files your mod actually contributed
- Mod titles appear as `=== <manifest title> ===`, so you can tell which mod an error belongs to

There is no "compile mods" CLI — **launching the game is the build step.** The log is written
early (at mod init), so you don't need to reach the main menu to get a result.

## 3. Script mods are gated by an option

`Local/PlayerOptions.json` → `"OptionAllowCSMods": "Yes"` (already set on this machine).
A mod with `.cs` files and this option off shows up in the mod manager but is not compiled
(`ModScrollerOne.cs:53` gates the UI on `modInfo.IsScripting && !Options.AllowCSMods`).

New mods default to **enabled** (`ModSettings.Enabled = true`), so no registration step exists —
just create the folder.

## 4. Silent inventory APIs

`GameObject.ReceiveObject(...)` **always** passes `Silent: true` down to `TakeObject`. Giving the
player items produces *no* message. Emit your own:

```csharp
The.Player.ReceiveObject("Desalination Pellet", 5);
IComponent<GameObject>.AddPlayerMessage("&gYou receive 5 desalination pellets.");
// or, for nicer grammar:
Messaging.XDidYToZ(pet, "give", null, The.Player, "5 desalination pellets");
```

## 5. Destroyed objects silently invalidate pooled events

`Messaging.XDidYToZ(GameObject Actor, …, GameObject Object, …)` starts with
`if (!GameObject.Validate(ref Object)) return;`. If you `Destroy()` an item and *then* try to
build a message naming it, **nothing is printed and no error is raised**. Capture the display
name, or send the message, before destroying.

Same class of bug applies to any pooled `MinEvent` holding a `GameObject`.

## 6. Forgetting `WantEvent`

A `HandleEvent` override that is never preceded by a matching `WantEvent` return is dead code —
the event is never delivered. Symptom: "my handler never runs". Always pair them:

```csharp
public override bool WantEvent(int ID, int cascade)
    => base.WantEvent(ID, cascade) || ID == OwnerGetInventoryActionsEvent.ID
                                   || ID == InventoryActionEvent.ID;
```

## 7. Not calling `RequestInterfaceExit()`

In the twiddle loop (`EquipmentAPI.TwiddleObject`), if the action doesn't request interface exit
*and* the target object didn't change cell/inventory/equipment, the loop `continue`s and
**the menu re-opens immediately**. For a verb that should close the menu, call
`E.RequestInterfaceExit()` on every exit path (success *and* failure).

## 8. Hotkey collisions

`ShowInventoryActionMenu` blanks any key that conflicts with `UINav`/`Menus` bindings, and if two
actions in the same menu share a key the second one silently loses it. `Feed` uses `f`;
`Pettable`'s `Pet` uses `p`; the base game's `Feed To` (tonics) also uses `f` but only ever
appears on *tonics*, so there's no clash in a creature's menu.

## 9. Namespace conventions matter for XML

`<part Name="Foo"/>` resolves through
`{ "part": "XRL.World.Parts", "mutation": "XRL.World.Parts.Mutation",
"skill": "XRL.World.Parts.Skill", "builder": "XRL.World.ObjectBuilders" }`.
Put custom parts in `XRL.World.Parts` or XML can't find them by short name.

## 10. Random environment notes

- Every new shell needs `PATH`/`DOTNET_ROOT` re-exported for `dotnet` and `ilspycmd`
  (see [01-toolchain](01-toolchain-and-decompilation.md)).
- `ilspycmd -l c` output for the whole assembly: `journal/../reference/types.txt` (7072 classes).
- The game's own data is at
  `~/.steam/steam/steamapps/common/Caves of Qud/CoQ_Data/StreamingAssets/` — both `Base/` **and**
  `DLC/` must be scanned; on this machine `DLC/PetsPack1` (Pets of Harvest Dawn) is active.
