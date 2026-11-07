# 02 — The mod system

*(all verified by reading `XRL/ModManager.cs`, `XRL/ModInfo.cs`, `XRL/ModFile.cs`,
`XRL/ModSettings.cs`)*

## On-disk layout

A local mod is a **directory** under:

```
~/.config/unity3d/Freehold Games/CavesOfQud/Mods/<ModFolderName>/
```

`ModManager.Init()` calls `RefreshModDirectory()` on `DataManager.LocalPath("Mods")` (and on
`SavePath("Mods")`, `DataManager.DLCPath()` for embedded DLC, and `DataManager.EmbeddedModsPath()`),
plus Steam workshop content is folded in via `workshop.json` / `publishedFileId`.

The **folder name is not the mod ID**. The ID comes from `manifest.json`.

Files are discovered by `ModInfo.InitializeFiles()`, which **recurses the whole directory**
and skips anything with the `Hidden` attribute. Every file is classified by extension
(`XRL/ModFile.cs`):

| Extension | `ModFileType` |
|---|---|
| `.xml` | XML |
| `.json` | JSON |
| `.cs` | **CSharp** |
| `.png` | Sprite |
| `.wav` `.ogg` `.aiff` `.mp3` | Audio |
| `.dll` | Assembly |
| `.rpm` | Map |

**Key consequence:** *any* `.cs` file anywhere in the mod folder is compiled, and *any* `.xml`
file is a candidate data file. There is no manifest entry you must add to get a script compiled.

## `manifest.json`

Deserialized into `XRL.ModManifest`:

```jsonc
{
  "id": "GratefulPets",              // → C# symbol MOD_GratefulPets, → GratefulPets.dll
  "title": "Grateful Pets",          // shown in mod manager AND as === TITLE === in build_log
  "description": "…",
  "version": "1.0.0",                // parsed as System.Version
  "author": "…",
  "tags": "Pets,UI",                 // CommaDelimitedArrayConverter
  "loadorder": 100,                  // int?
  "previewimage": "preview.png",
  "loadbefore": [], "loadafter": [],
  "dependencies": { "SomeMod": "*" },
  "directories": [ … ],              // optional: restrict which subdirs are the mod
  "preloadScripts": [ "Scripts/A.cs" ]  // optional ordering hint (Legacy Items uses this)
}
```

> **`id` must be a legal C# identifier.** It gets emitted as `#define MOD_<id>` and used as the
> output DLL name. `LegacyItems`, `ManageLiquids`, `FreeholdGames_DLC_PetsPack1` are all fine.

`workshop.json` is separate and only used for Steam Workshop presentation:
`{ WorkshopId, Title, Description, Tags, Visibility, ImagePath }`. The description there is
BBCode-ish (`[h1]`, `[b]`, `[url]`).

## Settings, enablement, and script approval

`ModSettings` (stored per-ID in `Local/ModSettings.json`) — `XRL/ModSettings.cs`:

```csharp
public bool Enabled = true;      // ← defaults to ON for a brand-new mod
public string FilesHash;         // last approved hash of the mod's files
public string SourceHash;        // last approved hash of its script content
public string Title;             // mirror of manifest title, for humans reading the JSON
[JsonIgnore] public bool Failed; // set when the mod fails to compile
[JsonIgnore] public List<string> Errors, Warnings;
```

So: **drop a folder in `Mods/` and it is enabled by default.** No registry to edit.

Script mods additionally require `Options.AllowCSMods`. On this machine
`Local/PlayerOptions.json` already has `"OptionAllowCSMods": "Yes"`.

## When (and how) scripts compile

Compilation happens **at game startup**, via `RoslynCSharp` (`XRL/ModManager.cs:116`):

```csharp
RoslynCSharpCompiler c = ScriptDomain.CreateDomain("ModsDomain").RoslynCompilerService;
c.GenerateInMemory = !Options.OutputModAssembly;
c.OutputDirectory = DataManager.SavePath("ModAssemblies");
c.OutputPDBExtension = ".pdb";
// references: every loaded assembly, incl. Assembly-CSharp
// symbols:    VERSION_1_0, BUILD_2_0_211, MOD_<ModID>
```

Each scripting mod is compiled **separately, into its own assembly**, named after the mod ID
(`ModAssemblies/ManageLiquids.dll`). A failure in one mod does not stop the others — it just marks
that mod `Failed` and it is skipped.

## `build_log.txt` is the oracle

`Logger.cs` wires a `SimpleFileLogger` to `build_log.txt` in the config dir. A session looks like:

```
==== BUILDING MODS ====
Defined symbol: VERSION_1_0
Defined symbol: BUILD_2_0_211
=== INHERITANCE (LEGACY ITEMS) ===
Loading path: /
Compiling 16 files...
Failure :(
== COMPILER ERRORS ==
<...>/Scripts/LegacyItemPart.cs(175,38): error CS1579: …
=== MANAGE LIQUIDS ===
Compiling 2 files...
Success :)
Location: …/ModAssemblies/ManageLiquids.dll
Defined symbol: MOD_MANAGELIQUIDS
==== FINAL LOAD ORDER ====
1: 2667322341
…
```

Reading this file is **the** way to verify a mod without launching into a save. It rotates:
`build_log.txt` → `build_log.txt.prev`.

`Compiling N files...` tells you exactly how many `.cs` files the game found in your mod —
a cheap check that your files are where you think they are.

## Load order

`==== FINAL LOAD ORDER ====` lists numeric/workshop IDs and mod IDs. Ordering is driven by
`ModManifest.LoadOrder`, `Dependencies`, `LoadBefore`/`LoadAfter`, with the base game first.
For XML `Load="Merge"` to work, the base blueprint must already exist — base game data is always
loaded before mods *(inferred from `LoadAllBlueprints()` iterating `YieldXMLStreamsWithRoot("Objects")`,
which yields base files then mod files)*.

## Other manifest/config files `ModInfo` reads

From the comment at `XRL/ModInfo.cs:273`: *"Read mod root configuration files: `manifest.json`,
`config.json`, `workshop.json`, `modconfig.json`."* — `config.json` is deprecated in favour of
`manifest.json` (it logs a warning telling you to migrate).
