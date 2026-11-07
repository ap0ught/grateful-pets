// ---------------------------------------------------------------------------
//  grateful-pets  --  Caves of Qud mod
//
//  Feed a pet, get paid for it.
//
//      * The verb "Feed" appears in the twiddle menu of any real pet
//        (Pet / StartingPet tag, or the Pettable part).
//      * It costs one food item from your inventory and one turn.
//      * It always yields 5 Desalination Pellets, plus one random gift
//        rolled from the weighted table below.
//
//  How it is wired up (see journal/04-events-and-inventory-actions.md):
//
//      OwnerGetInventoryActionsEvent  fires on the *player* for every twiddle,
//                                    carrying E.Object = the pet.
//                                    -> we add the Feed action there.
//
//      InventoryActionEvent           is dispatched wherever InventoryAction
//                                    .Process() points it. With
//                                    FireOnActor: true that is the *player*,
//                                    with E.Item = the pet.
//                                    -> we handle it there.
//
//  Because both handlers live on a part attached to the player, this mod needs
//  no XML blueprint edits at all: it covers every current pet and any pet a
//  future mod or DLC adds, as long as it carries a pet tag.
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;

namespace GratefulPetsMod
{
	/// <summary>New-game hook: runs when the player GameObject is first created.</summary>
	[PlayerMutator]
	public class GratefulPetsPlayerMutator : IPlayerMutator
	{
		public void mutate(GameObject player)
		{
			if (player != null)
			{
				player.RequirePart<XRL.World.Parts.GratefulPets>();
			}
		}
	}

	/// <summary>Load hook: runs after every load, so existing saves get the part too.</summary>
	[HasCallAfterGameLoadedAttribute]
	public class GratefulPetsLoadHandler
	{
		[CallAfterGameLoadedAttribute]
		public static void OnGameLoaded()
		{
			GameObject player = XRLCore.Core?.Game?.Player?.Body;
			player?.RequirePart<XRL.World.Parts.GratefulPets>();
		}
	}
}

namespace XRL.World.Parts
{
	/// <summary>
	/// Lives on the player. Adds a "Feed" action to pet menus and resolves it.
	/// Stateless, so it is a plain [Serializable] IPart.
	/// </summary>
	[Serializable]
	public class GratefulPets : IPart
	{
		public const string FEED_COMMAND = "FeedPet";

		public const string PELLET_BLUEPRINT = "Desalination Pellet";
		public const int PELLET_COUNT = 5;

		public const int FEED_ENERGY_COST = 1000;   // one full turn
		public const int FEED_RANGE = 1;            // must be adjacent

		// ------------------------------------------------------------------
		//  Gift table.  Weights total 99.
		//  Every blueprint is confirmed present in Base/ObjectBlueprints/Items.xml;
		//  the numbers are their <part Name="Commerce" Value="..."/>.
		//
		//   12  Copper Nugget            10      5   Rough Topaz
		//   10  Rough Agate             100      6   Polished Topaz
		//    9  Polished Agate          120      6   Random Silver Figurine
		//    8  Silver Nugget            50      5   Rough Jasper
		//    4  Rough Amethyst          250      4   Polished Amethyst
		//    4  Gold Nugget             200      3   Random Gold Figurine
		//    3  Rough Sapphire          300      3   Polished Sapphire
		//    2  Rough Emerald           350      2   Polished Emerald
		//    2  Rough Peridot           350      2   Polished Peridot
		//    1  UbernostrumTonic       (T5)      1   Chiral Rings           1250
		//
		//  ~31% common, ~27% mid, ~31% rare, ~10% jackpot.
		// ------------------------------------------------------------------

		static readonly string[] GiftBlueprints = new string[]
		{
			"Copper Nugget",            // 12
			"Rough Agate",              // 10
			"Polished Agate",           //  9
			"Silver Nugget",            //  8
			"Rough Topaz",              //  7
			"Polished Topaz",           //  6
			"Random Silver Figurine",   //  6
			"Rough Jasper",             //  5
			"Polished Jasper",          //  5
			"Rough Amethyst",           //  4
			"Polished Amethyst",        //  4
			"Gold Nugget",              //  4
			"Random Gold Figurine",     //  3
			"Rough Sapphire",           //  3
			"Polished Sapphire",        //  3
			"Rough Emerald",            //  2
			"Polished Emerald",         //  2
			"Rough Peridot",            //  2
			"Polished Peridot",         //  2
			"UbernostrumTonic",         //  1
			"Chiral Rings"              //  1
		};

		static readonly int[] GiftWeights = new int[]
		{
			12, 10, 9, 8, 7, 6, 6, 5, 5, 4, 4, 4, 3, 3, 3, 2, 2, 2, 2, 1, 1
		};

		static readonly int GiftWeightTotal = Sum(GiftWeights);

		static int Sum(int[] Values)
		{
			int total = 0;
			for (int i = 0; i < Values.Length; i++)
			{
				total += Values[i];
			}
			return total;
		}

		// ------------------------------------------------------------------
		//  Event plumbing
		// ------------------------------------------------------------------

		public override bool WantEvent(int ID, int cascade)
		{
			if (base.WantEvent(ID, cascade))
			{
				return true;
			}
			return ID == OwnerGetInventoryActionsEvent.ID || ID == InventoryActionEvent.ID;
		}

		/// <summary>
		/// Fires on the player for every object being twiddled; E.Object is that object.
		/// </summary>
		public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
		{
			if (E.Object != null && !E.Object.IsInvalid() && IsPet(E.Object))
			{
				E.AddAction(
					Name: "Feed",
					Display: "feed",
					Command: FEED_COMMAND,
					Key: 'f',
					FireOnActor: true,   // -> InventoryActionEvent lands back on the player
					Default: 10          // above Pettable's "Pet" (Default: 5)
				);
			}
			return base.HandleEvent(E);
		}

		/// <summary>
		/// Fires on the player because of FireOnActor above; E.Item is the pet.
		/// </summary>
		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == FEED_COMMAND)
			{
				// Always close the menu, on success and on failure alike, so the
				// twiddle loop does not immediately re-open it.
				Feed(E.Actor ?? The.Player, E.Item);
				E.RequestInterfaceExit();
			}
			return base.HandleEvent(E);
		}

		// ------------------------------------------------------------------
		//  The verb
		// ------------------------------------------------------------------

		static void Feed(GameObject Actor, GameObject Pet)
		{
			if (Actor == null || Actor.IsInvalid())
			{
				Actor = The.Player;
			}
			if (Actor == null || Pet == null || Pet.IsInvalid())
			{
				return;
			}
			if (!IsPet(Pet))
			{
				return;
			}

			if (Pet.DistanceTo(The.Player) > FEED_RANGE)
			{
				IComponent<GameObject>.AddPlayerMessage(
					"&YYou are too far away to feed " + Pet.GetDisplayName(Single: true, WithDefiniteArticle: true) + ".");
				return;
			}

			GameObject food = FindFood(Actor);
			if (food == null)
			{
				IComponent<GameObject>.AddPlayerMessage(
					"&YYou have nothing to feed " + Pet.GetDisplayName(Single: true, WithDefiniteArticle: true) + ".");
				return;
			}

			// 1. Flavour.  Must happen before Destroy(): Messaging validates its
			//    arguments and silently does nothing if the object is already gone.
			Messaging.XDidYToZ(Pet, "gobble", "up", food);

			// 2. Consume the food.  This is the cost of the action.
			food.Destroy();

			// 3. Guaranteed: desalination pellets.
			int pellets = Actor.ReceiveObject(PELLET_BLUEPRINT, PELLET_COUNT);
			if (pellets > 0)
			{
				Messaging.XDidYToZ(Pet, "hand", null, Actor, pellets + " desalination pellets");
			}

			// 4. The roll: one random gift from the weighted table.
			string blueprint = RollGiftBlueprint();
			GameObject gift = (blueprint != null) ? GameObject.Create(blueprint) : null;
			if (gift != null && Actor.ReceiveObject(gift))
			{
				Messaging.XDidYToZ(Pet, "give", null, Actor, gift.t(int.MaxValue, null, null, AsIfKnown: false, Single: true));
			}

			// 5. It costs a turn.
			Actor.UseEnergy(FEED_ENERGY_COST, "Feed Pet");
		}

		/// <summary>First non-important item in inventory that has a Food part.</summary>
		static GameObject FindFood(GameObject Actor)
		{
			List<GameObject> inventory = Actor.GetInventory();
			if (inventory == null)
			{
				return null;
			}
			for (int i = 0; i < inventory.Count; i++)
			{
				GameObject item = inventory[i];
				if (item == null || item.IsInvalid())
				{
					continue;
				}
				if (!item.HasPart<Food>() || item.IsImportant())
				{
					continue;
				}
				return item;
			}
			return null;
		}

		/// <summary>Weighted roll over the gift table. Total weight is 99.</summary>
		static string RollGiftBlueprint()
		{
			if (GiftBlueprints.Length == 0 || GiftWeightTotal <= 0)
			{
				return null;
			}
			int roll = Stat.Random(1, GiftWeightTotal);
			for (int i = 0; i < GiftBlueprints.Length && i < GiftWeights.Length; i++)
			{
				roll -= GiftWeights[i];
				if (roll <= 0)
				{
					return GiftBlueprints[i];
				}
			}
			// Unreachable unless the arrays disagree; fall back to the last entry.
			return GiftBlueprints[GiftBlueprints.Length - 1];
		}

		/// <summary>
		/// Real pets only: the Pet / StartingPet tag, or the Pettable part.
		/// Deliberately does NOT use IsPlayerLed(), so proselytized or beguiled
		/// followers are excluded.
		/// </summary>
		public static bool IsPet(GameObject obj)
		{
			if (obj == null || obj.IsInvalid())
			{
				return false;
			}
			if (obj.HasTag("Pet") || obj.HasTag("StartingPet"))
			{
				return true;
			}
			return obj.HasPart<Pettable>();
		}
	}
}
