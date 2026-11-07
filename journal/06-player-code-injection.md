# 06 — Injecting code onto the player

A part is only consulted for events fired **at the object that owns it**. So to intercept
`OwnerGetInventoryActionsEvent` / `InventoryActionEvent` for the player, the part must be on
`The.Player`. Two hooks are needed, because they cover different situations.

## Hook A — new games: `[PlayerMutator]`

```csharp
// decomp/XRL/PlayerMutator.cs
[AttributeUsage(AttributeTargets.Class)]
public class PlayerMutator : Attribute { }

// decomp/XRL/IPlayerMutator.cs
public interface IPlayerMutator { void mutate(GameObject player); }   // lowercase 'm'!
```

```csharp
[PlayerMutator]
public class MyMutator : IPlayerMutator
{
    public void mutate(GameObject player)   // ← must be spelled exactly 'mutate'
    {
        player.AddPart<MyPart>();
    }
}
```

**Limitation (from the wiki, *Modding:Adding Code to the Player*):** this runs only when the
`GameObject` is first created — i.e. a **New Game**. Loading an existing save never calls it.

## Hook B — every load: `[CallAfterGameLoadedAttribute]`

```csharp
// decomp/XRL/CallAfterGameLoadedAttribute.cs
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CallAfterGameLoadedAttribute : Attribute { }
```

`ModManager.CallAfterGameLoaded()` (`decomp/XRL/ModManager.cs:374`):

```csharp
foreach (MethodInfo item in GetMethodsWithAttribute(typeof(CallAfterGameLoadedAttribute),
                                                    typeof(HasCallAfterGameLoadedAttribute)))
    try { item.Invoke(null, Array.Empty<object>()); }
    catch (Exception arg) { MetricsManager.LogAssemblyError(item, …); }
```

**Both attributes are required** — the method one *and* `[HasCallAfterGameLoadedAttribute]` on the
declaring class. The method must be `static` and parameterless (it's invoked with no args).

```csharp
[HasCallAfterGameLoadedAttribute]
public class MyLoadHandler
{
    [CallAfterGameLoadedAttribute]
    public static void OnLoad()
    {
        GameObject player = XRLCore.Core?.Game?.Player?.Body;
        player?.RequirePart<MyPart>();     // idempotent
    }
}
```

## The template both hooks copy

The **ManageLiquids** workshop mod (`3316416182`) does exactly this pair, and it is the single
best reference in the wild. Its source is preserved at `reference/examples/ManageLiquids.cs`:

```csharp
[PlayerMutator]
public class MyPlayerMutator : IPlayerMutator {
    public void mutate(GameObject player) { player.AddPart<BhodiManageLiquids>(); }
}

[HasCallAfterGameLoadedAttribute]
public class MyLoadGameHandler {
    [CallAfterGameLoadedAttribute]
    public static void MyLoadGameCallback() {
        GameObject player = XRLCore.Core?.Game?.Player?.Body;
        player?.RequirePart<BhodiManageLiquids>();
    }
}

namespace XRL.World.Parts {
    [Serializable]
    public class BhodiManageLiquids : IScribedPart { … }
}
```

## Part API

```csharp
player.AddPart<T>();          // always adds (can duplicate!)
player.RequirePart<T>();      // adds only if missing  ← use this in load hooks
player.HasPart<T>();
player.GetPart<T>();
player.HasPart("SomeName");   // string form
```

`RequirePart<T>()` / `AddPart<T>()` are generic with `where T : IPart, new()`.

## `IPart` vs `IScribedPart`

- `IPart` — base; fields are serialized by the game's own part serializer on save.
- `IScribedPart` — opt-in to custom (usually JSON/`Serialization`) persistence; used when you
  hold references or types the default serializer mishandles.

grateful-pets' part is **stateless** (it only reads the gift table and reacts to events), so it
declares `[Serializable] class GratefulPets : IPart`. ManageLiquids needed mutable settings, so it
used `IScribedPart`. Rule of thumb: **no fields → `IPart`; fields you care about → start with
`IScribedPart`.**

## Other places you might want code (not used here, for the record)

- `XRL.World.AfterObjectCreatedEvent` / `BeforeObjectCreatedEvent` / `ObjectCreatedEvent` — fire
  on object creation; parts all over the game subscribe to them (`Examiner`, `PowerSwitch`,
  `Harvestable`, `PetPhylactery`, `EnergyCell`, …). Would be the hook for "add my part to every
  pet as it spawns" if the tag-based approach hadn't worked.
- `ZoneActivatedEvent` — used by `Examiner`.
- Wish/debug commands: `[HasWishCommand]` on a class + wish handlers (see `PopulationManager`,
  `LegacyItemWishCommands.cs` in the Legacy Items mod).
