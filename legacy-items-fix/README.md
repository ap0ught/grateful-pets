# legacy-items-fix

A copy of the one-line fix that keeps **Legacy Items (Hand-me-downs)** compiling, kept in this
repo so the patch isn't lost when Steam re-downloads the workshop folder.

**The maintained copy of the whole mod lives in its own repository:**
**[`ap0ught/legacy-items`](https://github.com/ap0ught/legacy-items)** — go there for the actual
mod source, install instructions and the full write-up.

## The patch

`Scripts/LegacyItemPart.cs` → `DoesSaveExist(string gameId)`:

```diff
                 // GetSavedGameInfo() returns Task<List<SaveGameInfo>> (async); unwrap it synchronously.
-                var saves = SavesAPI.GetSavedGameInfo();
+                var saves = SavesAPI.GetSavedGameInfo().GetAwaiter().GetResult();
```

Without it:

```
LegacyItemPart.cs(175,38): error CS1579: foreach statement cannot operate on variables of type
'Task<List<SaveGameInfo>>' …
```

…and a 0-byte `LegacyItems.dll`, i.e. the mod silently does nothing.

Full context — including why the workshop copy keeps losing this fix — is in
[`../journal/09-gotchas-and-troubleshooting.md`](../journal/09-gotchas-and-troubleshooting.md).
