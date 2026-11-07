# 07 — grateful-pets: design

**Mod ID:** `GratefulPets`
**Repo:** this repository, under [`mod/`](../mod/)

## The brief

> Any **real pet** gives gifts to the player. There is no "feed" verb in the game, so add one.
> Feeding costs **1 food item** from inventory. Every feed yields **5 Desalination Pellets
> (always)** plus **1 random valuable item from a list**. Real pets only — not followers, not NPCs.

User decisions captured mid-session:

| Question | Answer |
|---|---|
| How should feeding work? | Feed verb, **costs a food item** |
| Passive gifts over time? | No — gifts come from feeding. (Reinterpreted: *"they will always give the pellets, but they will also give something else that is valuable — each food, one thing randomly from a list, and the other is the pellets."*) |
| What counts as a pet? | **Real pets only** — `StartingPet`/`Pet` tag or `Pettable` part |

## Architecture

One C# file, no XML. A single part on the player:

```
grateful-pets/
├── manifest.json
└── Scripts/GratefulPets.cs
        ├── [PlayerMutator] MyMutator            → new games
        ├── [HasCallAfterGameLoaded] OnLoad()    → existing saves
        └── XRL.World.Parts.GratefulPets : IPart → the actual logic
```

Why player-side rather than patching every pet blueprint:

- 34 pet blueprints exist today; `Load="Merge"` would need all of them (or the templates), and
  would silently miss any pet added by another mod or by future DLC.
- `OwnerGetInventoryActionsEvent` fires **on the player** for *every* twiddle, carrying
  `E.Object` = the object being twiddled. Filter on pet-ness there and you cover every current and
  future pet with zero XML.
- `FireOnActor: true` puts the resulting `InventoryActionEvent` back on the player, where the
  handler lives, with `E.Item` = the pet.

See [04-events](04-events-and-inventory-actions.md) for the full pipeline.

## The flow

```
player twiddles a pet  (examine → actions, right-click, interact-nearby)
  └─ EquipmentAPI.TwiddleObject(Owner=player, GO=pet)
       └─ OwnerGetInventoryActionsEvent.Send(player, pet, table)   → fires on player
            └─ GratefulPets.HandleEvent:
                 if IsPet(E.Object) → E.AddAction("Feed", "feed", "FeedPet",
                                                   Key:'f', FireOnActor:true, Default:10)
player picks [F] Feed
  └─ InventoryAction.Process(GO=pet, Owner=player)
       └─ target = FireOnActor ? Owner : GO  = player
       └─ InventoryActionEvent.Check(out E, player, player, pet, "FeedPet")
            └─ GratefulPets.HandleEvent(E):
                 1. E.RequestInterfaceExit()
                 2. validate: E.Item is a pet, distance ≤ 1
                 3. find a Food-part item in inventory (skip Important items)
                 4. if none → message + stop
                 5. Messaging.XDidYToZ(pet, "gobble", "up", food)   ← BEFORE destroying
                 6. food.Destroy()
                 7. The.Player.ReceiveObject("Desalination Pellet", 5)
                 8. gift = GameObject.Create(roll weighted table)
                    The.Player.ReceiveObject(gift)
                    Messaging.XDidYToZ(pet, "give", null, The.Player, giftName)
                 9. Actor.UseEnergy(1000, "Feed Pet")               ← costs a turn
```

### Guards, in order

| Check | Failure behaviour |
|---|---|
| `E.Command == "FeedPet"` | ignore (we see every inventory action on the player) |
| `E.Item` valid **and** `IsPet(E.Item)` | ignore |
| `DistanceTo(The.Player) <= 1` | "You are too far away to feed X." + close menu |
| a `Food`-part, non-important item in inventory | "You have nothing to feed X." + close menu |

Everything above calls `E.RequestInterfaceExit()` so the twiddle menu doesn't immediately
re-open (see [04, §3](04-events-and-inventory-actions.md)).

## The gift table

Weighted, total weight **99**, implemented as parallel `string[]` / `int[]` in C#.
Every blueprint was confirmed to exist in `Base/ObjectBlueprints/Items.xml`, and the commerce
value shown is the `<part Name="Commerce" Value="…">` from that file.

| # | Blueprint | Commerce | Weight | ~Chance |
|---|---|---:|---:|---:|
| 1 | `Copper Nugget` | 10 | 12 | 12.1% |
| 2 | `Rough Agate` | 100 | 10 | 10.1% |
| 3 | `Polished Agate` | 120 | 9 | 9.1% |
| 4 | `Silver Nugget` | 50 | 8 | 8.1% |
| 5 | `Rough Topaz` | 150 | 7 | 7.1% |
| 6 | `Polished Topaz` | 180 | 6 | 6.1% |
| 7 | `Random Silver Figurine` | 100 | 6 | 6.1% |
| 8 | `Rough Jasper` | 200 | 5 | 5.1% |
| 9 | `Polished Jasper` | 200 | 5 | 5.1% |
| 10 | `Rough Amethyst` | 250 | 4 | 4.0% |
| 11 | `Polished Amethyst` | 300 | 4 | 4.0% |
| 12 | `Gold Nugget` | 200 | 4 | 4.0% |
| 13 | `Random Gold Figurine` | 200 | 3 | 3.0% |
| 14 | `Rough Sapphire` | 300 | 3 | 3.0% |
| 15 | `Polished Sapphire` | 360 | 3 | 3.0% |
| 16 | `Rough Emerald` | 350 | 2 | 2.0% |
| 17 | `Polished Emerald` | 420 | 2 | 2.0% |
| 18 | `Rough Peridot` | 350 | 2 | 2.0% |
| 19 | `Polished Peridot` | 420 | 2 | 2.0% |
| 20 | `UbernostrumTonic` | 75 | 1 | 1.0% |
| 21 | `Chiral Rings` | **1250** | 1 | 1.0% |

*`UbernostrumTonic`'s `Commerce` value is a misleading 75 — it's a **Tier 5** tonic:10 rounds of
HP regen, purge of short-term biological debuffs, and regrowth of a severed appendage (0.9/round
for True Kin). Its real worth is far above its price tag, which is why it sits in the jackpot
bucket despite the number.*

Design intent behind the shape:

- **~31% common** (copper/rough agate/polished agate) — you always get *something*.
- **~27% mid** — silver, topaz, figurines.
- **~31% rare** — jasper/amethyst/sapphire/gold, 200–360 value.
- **~10% jackpot** — emeralds/peridots at 350–420, a ubernostrum, and a 1-in-99
  **Chiral Rings** at 1250.

Chosen flavour: **pets bring you shiny things they found** — nuggets, gemstones, figurines,
jewellery — plus one powerful tonic as the oddball. No quest items, no equipment that could
break balance at low tier, nothing with `Important`/quest flags.

## Balance notes / open questions

- **Turn cost:** `UseEnergy(1000)` = one full turn, so feeding is a real action, not free menu
  spam. Combined with "costs a food item", the sink is meaningful but not punishing.
- **No cooldown.** Feed as often as you can afford food. If this feels spammable in playtesting,
  the cheap fix is a `GetCooldown`/`UseEnergy(2000)` bump or a per-pet `Property` timestamp.
- **Pellets are unconditional** — 5 every time, per the brief.
- The food consumed is simply *the first* non-important `Food`-part item in inventory order.
  A "feed the cheapest" sort would be a nice refinement if inventory order turns out to be
  arbitrary.
- Not implemented (deliberately): passive/AI-driven gifts (`GivesTreat`-style), gifts from
  non-pet followers, feeding other people's pets at a distance.
