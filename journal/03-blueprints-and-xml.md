# 03 — Blueprints & XML

## XML is indexed by root element, not by filename

*(verified — `decomp/XRL/DataManager.cs:447` `CheckXMLInitialize`)*

```csharp
// base game: every *.xml under StreamingAssets, recursively
new DirectoryInfo(FilePath("")).GetFiles("*.xml", SearchOption.AllDirectories) → CacheFile(...)
// mods: every ModFile whose Type == XML
foreach (ModInfo activeMod in ModManager.ActiveMods)
    foreach (ModFile file in activeMod.Files)
        if (file.Type == ModFileType.XML) CacheFile(file.OriginalName, activeMod);
```

`CacheFile` reads the file's **root element** into `XMLFilesByRoot`. Loaders then ask for what
they want:

| Root element | Loader |
|---|---|
| `<objects>` | `ObjectBlueprintLoader.LoadAllBlueprints()` |
| `<populations>` | `PopulationManager.HandleNodes` |
| `<books>` | `BookUI` |
| `<commands>` | `CommandBindingManager` |
| `<activatedabilities>` | `ActivatedAbilities` |
| `<wishcommands>` | `WishMenu` |
| `<Skills>` | `SkillFactory` |
| `<Worlds>` | `WorldFactory` |
| `<colors>` | `MarkupShaders` |

**A mod can therefore ship `Anything.xml` with root `<objects>` and it will be treated exactly
like the base `ObjectBlueprints/*.xml`.** Filename only matters for human tidiness.

## Object blueprints

`decomp/XRL.World.Loaders/ObjectBlueprintLoader.cs`:

```xml
<object Name="Dog" Inherits="BaseDog">
  <part Name="Render" DisplayName="dog" … />
  <tag  Name="Pet" />
  <stag Name="StartingPet" />   <!-- "stack tag": stacks with N copies -->
</object>
```

- **Child node → CLR namespace** (`ObjectBlueprintXMLChildNode._tagToNamespace`):

  | XML node | namespace |
  |---|---|
  | `<part>` | `XRL.World.Parts` |
  | `<mutation>` | `XRL.World.Parts.Mutation` |
  | `<skill>` | `XRL.World.Parts.Skill` |
  | `<builder>` | `XRL.World.ObjectBuilders` |

  **This is why custom parts should be declared in `namespace XRL.World.Parts`** — otherwise
  `<part Name="YourPart"/>` can't resolve you by short name. (grateful-pets' part obeys this even
  though it doesn't need XML, purely out of consistency with every other part in the game.)

- There is a `CompatManager.ProcessSkill` / `ProcessMutation` rename layer, plus
  `CompatManager.TryGetCompatEntry("blueprint", …)` used when merging.

### `Load="Merge"` — modifying an existing blueprint from a mod

`ObjectBlueprintLoader.ReadObjectsNode` (`:790`):

```csharp
if (data.Load == "Merge") {
    if (!Objects.TryGetValue(data.Name, out var target))
        handleError($"…merge with {name} which is an unknown blueprint, node discarded");
    else
        target.Merge(data); // ← error if the base object doesn't exist
}
else if (data.Load == "MergeIfExists") {
    if (Objects.TryGetValue(data.Name, out var target)) target.Merge(data);  // silent no-op
}
else Objects[data.Name] = data;   // plain define / replace
```

`ObjectBlueprintXMLData.Merge(other)`:
- `Inherits` is overwritten if the incoming one is non-empty;
- for each child collection (`part`, `tag`, `stag`, …) it merges node-by-node:
  **existing keyed node → attribute merge, new node → appended.**

So this is legal and additive:

```xml
<object Name="Dog" Load="Merge">
  <part Name="GratefulPets" />   <!-- appended; Dog keeps everything it had -->
</object>
```

`Load="Merge"` on a name the game doesn't know logs an error. Use `MergeIfExists` if you're
unsure (e.g. patching an object that only exists when a DLC is installed).

**There is precedent in the base game:** `Base/Conversations.xml:171` uses
`<choice ID="Trade" IfTestState="ConvertSated" Load="Merge">`.

### Inheritance

`Inherits="BaseDog"` chains are resolved after all XML is loaded, so merging into a **template**
(`BaseCat`, `BaseDog`) reaches every descendant *(inferred from load order — base + all mod XML is
read before blueprints are resolved; I did not read the resolver itself)*.

**Gotcha:** there is no `abstract="true"` anywhere in `Items.xml` (`grep -c` → 0). `BaseNugget`,
`Gemstone`, `Tonic`, `Random Figurine` are all *creatable* objects. If you ever `GameObject.Create`
a `Base*` name you'll get a half-empty object.

## Population tables

`Base/PopulationTables.xml`, root `<populations>`:

```xml
<population Name="Things">
  <object Blueprint="Thing" Chance="50" />
</population>

<population Name="Widgets">
  <group Name="Widgets" Style="pickone" Chance="100,100,…">
    <object Blueprint="BrownWidget" Weight="70" />
    <object Blueprint="GoldWidget"   Weight="5"  />
  </group>
</population>
```

- Styles: `pickone`, `pickeach`; children are `<object>`, `<table Name=… Number="1-6">`, `<group>`.
- `PopulationManager.RollOneFrom(string PopulationName, Dictionary<string,string> Vars = null, string DefaultIfNull = null)` → `PopulationResult` (has `.Blueprint`).
- `PopulationManager` is `[ModSensitiveStaticCache]` — caches are rebuilt when mods change, so
  tables defined in a mod are picked up.

grateful-pets **doesn't** use a population table — its gift table is a plain weighted `string[]`
in C#, which avoids depending on XML load order entirely. Trade-off: no hot-reload/tweakability
from XML. If the table ever needs to be data-driven, `<populations>` is the mechanism.

## Data that mattered for this session

- `Desalination Pellet` → `Base/ObjectBlueprints/Items.xml` around line 11445,
  `<object Name="Desalination Pellet" Inherits="Item">`. Exact blueprint name has the space.
- Currency/gem blueprints and their `<part Name="Commerce" Value="…">` are all in `Items.xml`
  under `BaseNugget` / `Gemstone` / `Random Figurine` families — see
  [07-grateful-pets-design](07-grateful-pets-design.md) for the harvested values.
