# Wish commands for Grateful Pets debugging

This is the debugging playbook for testing Grateful Pets with Caves of Qud's Wish console.

Primary reference: [Official Wishes wiki](https://wiki.cavesofqud.com/wiki/Wishes).
The wiki warns that wishes are case-sensitive. If input does not exactly match a
registered wish, Qud falls back to a fuzzy search over object blueprints and quests;
a near miss may spawn an unintended object or start a quest. Use exact spelling and
keep a disposable test save.

## Open the Wish console

Bind `Wish` under **Escape → Key Mapping → Debug → Wish**. It is commonly bound to
`Ctrl-W`, but the key can differ by installation.

## Useful baseline commands

| Wish | Use in a Grateful Pets test |
|---|---|
| `where?` | Write the current zone ID to the message log and `Player.log`. |
| `where` | Show the current zone and coordinates in the message log. |
| `xy` | Show the player's tile coordinates. |
| `whatami` | Show the currently controlled creature's blueprint. Useful after switching bodies. |
| `checkforpart:GratefulPets` | Check that the player has the mod's `GratefulPets` part. The part is attached by the new-game mutator and again after every save load. |
| `removepart:GratefulPets` | Remove the player part for a reattachment test. Do this only in a throwaway save; reload the save afterward to test the load hook. |
| `idkfa` | Toggle God Mode while testing dangerous pet, corpse, or robot interactions. Toggle it again when finished. |
| `calm` | Toggle Calm Mode. Useful for freezing most AI while inspecting a state, but do not use it when testing pet movement or pellet delivery. |
| `mutationbgone` | Open the mutation-removal picker if a typo accidentally resolves to a mutation. |

`checkforpart:GratefulPets` checks the player, not the pet. Grateful Pets stores the
feed log on each pet and digestion state on each interacted pet, so this command only
verifies the player-side event hook is installed.

## Spawning test inputs

The wiki documents these general spawning forms:

```text
item:<blueprint>:<number>
modify:<blueprint>:<mod>
```

Use an exact Qud blueprint name. Examples relevant to this mod include:

```text
item:Bandage:1
item:Desalination Pellet:1
item:Portable Beehive:1
```

The first is useful as a care/reward comparison; the latter two verify blueprint
resolution and pellet/container-related behavior. For feeding, use an actual carried
food item. Do not assume a display name is a blueprint name; verify the blueprint in
Qud's shipped object XML or use an item already present in the save.

A fuzzy creature wish can create a nearby creature, but it will only become a
Grateful Pets target if the resulting object has one of these exact conditions:

- the `Pet` tag;
- the `StartingPet` tag; or
- the `Pettable` part.

Being player-led, beguiled, or proselytized is not enough. The implementation
intentionally excludes followers that do not satisfy the real-pet checks.

## Recommended test sequences

### 1. Verify the player hook

```text
checkforpart:GratefulPets
```

Expected result: the part exists on the player. If it does not, restart Qud and load
the save again; the mod's `CallAfterGameLoaded` handler should attach it.

### 2. Verify Feed

1. Put a known food item in the player's inventory.
2. Place a real pet adjacent to the player.
3. Use the pet's inventory/action menu and choose **Feed**, or use **Feed to pet**
   from the food item.
4. Confirm one food item and one turn are consumed.
5. Confirm one treasure reward appears immediately.
6. Confirm one desalination pellet is produced immediately. It may appear as an
   unidentified odd trinket until used or identified.
7. Advance turns near the pet and confirm the remaining four pellets are produced
   at roughly 300-turn intervals over the in-game day.

The feed log is per pet and per food type. Feeding the same food five effective times
makes the pet tired of it and downgrades the treasure roll; pellet production is not
downgraded.

### 3. Verify Care

1. Put a suitable tonic/syringe in the inventory of an organic pet's player.
2. Place a real pet adjacent to the player.
3. Chat with the pet and choose **Let me care for you.**
4. Confirm the tonic's external effect is applied to the pet and a healing treasure is
   returned.

For a robotic pet, use a spent energy cell instead of a biological tonic. The mod
consumes the spent cell, returns a fresh cell, and grants a healing treasure. Robot
pets also have a 20% bonus energy-cell roll on feed/care.

### 4. Verify corpse rewards

Use a pet that can butcher corpses, then observe the pet as the actor for five
butchered corpses. The milestone should produce two treasure rewards and five
pellets. `calm` is unsuitable for this test because it suppresses normal AI behavior.

## How Wish interacts with the mod

Grateful Pets currently declares no custom `[WishCommand]` handlers in
`mod/Scripts/`. Wish input therefore goes through Qud's normal wish system; the mod
does not add commands such as `gratefulpets:feed` or `gratefulpets:reset`.

The interaction is indirect:

1. Wish changes the game state, spawns an object, or toggles a debug mode.
2. Grateful Pets observes ordinary Qud events on the player and nearby objects.
3. If a spawned creature satisfies the real-pet check and is in the required range,
   the normal Feed/Care actions become available.
4. Feeding and care attach or update the mod's serialized per-pet state. A wish does
   not bypass the adjacency, inventory, pet-identity, or conversation checks.

Important consequences:

- `item:...` can provide test inventory, but it does not itself make a creature a
  pet or attach the mod's digestion state.
- A spawned object that lacks `Pet`, `StartingPet`, and `Pettable` will not expose
  Feed/Care, even if it is player-led.
- `checkforpart:GratefulPets` validates the player hook, not pet eligibility.
- `calm` may make a test appear stuck because normal pet AI and movement are paused;
  turn it off before testing pellet delivery or corpse behavior.
- `idkfa` protects the player but does not grant pet status, food, adjacency, or a
  digestion state.
- Wish typos can invoke fuzzy blueprint/quest matching. If an accidental mutation is
  added, use `mutationbgone` rather than trying more approximate commands.

## Evidence in the source

- Player attachment: `mod/Scripts/GratefulPets.cs`, `GratefulPetsPlayerMutator`
  and `GratefulPetsLoadHandler`.
- Pet eligibility: `GratefulPets.IsPet`, which checks `Pet`, `StartingPet`, and
  `Pettable`.
- Wish-command search: no `[WishCommand]` or `[HasWishCommand]` declarations exist
  under `mod/Scripts/`.
- Feed, Care, digestion, and corpse handling: `mod/Scripts/GratefulPets.cs`.
