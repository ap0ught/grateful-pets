// ---------------------------------------------------------------------------
//  grateful-pets  --  Caves of Qud mod
//
//  Feed a pet, get paid for it.
//
//      * "Feed" on a real pet (Pet / StartingPet tag, or Pettable) opens a
//        picker of the food you carry.  Each entry is sorted by how many times
//        you've fed that pet that food type, shown in brackets: beetle jerky [3].
//      * "Feed to pet" on a food item in inventory feeds it to an adjacent pet
//        (or asks which pet, if more than one is nearby).
//      * It costs one food item and one turn.
//      * Pellets: the pet produces 1 desalination pellet right away, then the
//        other 4 over the course of the day while digesting, handing each to
//        you when it's produced.  Pellets are never downgraded.
//      * Treasure: one random gift is given immediately as thanks.  If the pet
//        is tired of that food (fed it 5+ times, effective — the count decays
//        by 1 per in-game day) the gift is rolled from a downgraded (junk)
//        tier instead of the full table.
//      * The pet exclaims how much it loves or loathes what you gave it.
//
//  History / forensics: journal/07-grateful-pets-design.md and
//  journal/10-pellet-bug-and-feed-redesign.md.  The "pellets didn't arrive"
//  report turned out to be artifact identification (Desalination Pellet has an
//  Examiner/UnknownOddTrinket part; you can't recognise it until you've used
//  one), not an inventory bug.
//
//  Wiring (see journal/04-events-and-inventory-actions.md):
//
//      OwnerGetInventoryActionsEvent  fires on the *player* for every twiddle,
//                                     E.Object = the thing being twiddled.
//                                     -> "Feed" on a pet, "Feed to pet" on food.
//      InventoryActionEvent           with FireOnActor:true lands on the player,
//                                     E.Item = the thing the action was on.
//
//  Both handlers live on a part attached to the player, so no XML blueprint
//  edits are needed and every current/future pet is covered.
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Conversations;

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
	/// Lives on the player. Adds feed actions to pet/food menus and resolves them.
	/// Stateless (the per-pet taste log is stored on the pet's string Property,
	/// which is serialized by the save system) — so a plain [Serializable] IPart.
	/// </summary>
	[Serializable]
	public class GratefulPets : IPart
	{
		public const string FEED_COMMAND = "GratefulPets_FeedPet";
		public const string FEED_FOOD_COMMAND = "GratefulPets_FeedToPet";
		public const string APPLY_PELLET_COMMAND = "GratefulPets_ApplyPelletToContainer";

		public const string PELLET_BLUEPRINT = "Desalination Pellet";
		public const int PELLET_COUNT = 5;
		public const int PELLETS_NOW = 1;
		public const int PELLETS_DIGESTING = PELLET_COUNT - PELLETS_NOW; // 4, delivered over a day
		public const int DIGESTION_INTERVAL = (int)Calendar.TurnsPerDay / PELLETS_DIGESTING; // 300 turns

		public const int FEED_ENERGY_COST = 1000;   // one full turn
		public const int FEED_RANGE = 1;            // must be adjacent

		public const int TIRED_THRESHOLD = 5;       // effective count >= 5 => tired
		public const long TURNS_PER_DAY = Calendar.TurnsPerDay; // 1200

		public const string FEEDLOG_PROPERTY = "GratefulPets.FeedLog";
		public const string DIGESTION_PROPERTY = "GratefulPets.Digestions";

		// ------------------------------------------------------------------
		//  Gift tables.  A "roll" picks a blueprint by weight.  When the pet is
		//  tired of the chosen food the gift is downgraded to the junk tier
		//  (nuggets and rough/unpolished stones) — still a gift, never a
		//  jackpot.  Pellets are unaffected.
		//
		//  Full table weights total 99.  Blueprints are confirmed present in
		//  Base/ObjectBlueprints/Items.xml (commerce values from that file):
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

		// Junk tier — tired pet: nuggets and rough stones only.
		static readonly string[] JunkGiftBlueprints = new string[]
		{
			"Copper Nugget",
			"Rough Agate",
			"Silver Nugget",
			"Rough Topaz",
			"Rough Jasper",
			"Rough Amethyst",
			"Gold Nugget",
			"Rough Sapphire",
			"Rough Emerald",
			"Rough Peridot"
		};

		static readonly int[] JunkGiftWeights = new int[]
		{
			12, 10, 8, 7, 5, 4, 4, 3, 2, 2
		};

		// ------------------------------------------------------------------
		//  Event plumbing
		// ------------------------------------------------------------------

		public override bool WantEvent(int ID, int cascade)
		{
			if (base.WantEvent(ID, cascade))
			{
				return true;
			}
			return ID == OwnerGetInventoryActionsEvent.ID || ID == InventoryActionEvent.ID || ID == BeginConversationEvent.ID;
		}

		/// <summary>Fires on the player for every object being twiddled; E.Object is it.</summary>
		public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
		{
			if (E.Object != null && !E.Object.IsInvalid())
			{
				if (E.Object.HasPart<Food>() && !E.Object.IsImportant() && E.Object.InInventory == The.Player)
				{
					E.AddAction(
						Name: "Feed to pet",
						Display: "feed to pet",
						Command: FEED_FOOD_COMMAND,
						Key: 'F',
						FireOnActor: true,
						Default: 9
					);
				}

				// Selecting a carried liquid container, or a liquid pool next to the
				// player: offer to apply a carried desalination pellet directly to it.
				GameObject actor = E.Actor ?? The.Player;
				bool carriedContainer = E.Object.LiquidVolume != null && (E.Object.InInventory == actor || E.Object.Equipped == actor);
				bool adjacentPool = E.Object.IsOpenLiquidVolume() && actor != null && actor.InSameOrAdjacentCellTo(E.Object);
				if ((carriedContainer || adjacentPool) && FindDesalinationPellet(actor) != null)
				{
					E.AddAction(
						Name: "Apply desalination pellet",
						Display: "apply desalination pellet",
						Command: APPLY_PELLET_COMMAND,
						Key: 'a',
						FireOnActor: true,
						Default: -5
					);
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == FEED_COMMAND)
			{
				DoFeedPet(E.Actor ?? The.Player, E.Item);
				E.RequestInterfaceExit();
			}
			else if (E.Command == FEED_FOOD_COMMAND)
			{
				DoFeedToPet(E.Actor ?? The.Player, E.Item);
				E.RequestInterfaceExit();
			}
			else if (E.Command == APPLY_PELLET_COMMAND)
			{
				DoApplyPelletToContainer(E.Actor ?? The.Player, E.Item);
				E.RequestInterfaceExit();
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeginConversationEvent E)
		{
			// "Feed as an option under chat" — inject into the conversation so
			// chat's availability logic (range, busy/fighting, can-start) applies
			// for free. Only for real pets.
			if (E != null && E.StartNode != null && IsPet(E.SpeakingWith))
			{
				InjectFeedChoice(E.StartNode);
			}
			return base.HandleEvent(E);
		}

		// ------------------------------------------------------------------
		//  Entry points
		// ------------------------------------------------------------------

		/// <summary>Pick a food, then feed the pet. (Also used by the chat choice.)</summary>
		public static void DoFeedPet(GameObject Actor, GameObject Pet)
		{
			Actor = ResolveActor(Actor);
			if (Actor == null || Pet == null || Pet.IsInvalid() || !IsPet(Pet))
			{
				return;
			}
			if (Pet.DistanceTo(Actor) > FEED_RANGE)
			{
				Msg("&YYou are too far away to feed " + PetName(Pet) + ".");
				return;
			}

			List<GameObject> foodChoices = CollectFeedableFoods(Actor);
			if (foodChoices.Count == 0)
			{
				Msg("&YYou have nothing to feed " + PetName(Pet) + ".");
				return;
			}

			long today = GetToday();
			Dictionary<string, int> counts = GetEffectiveCounts(Pet, today);

			// Sort least-fed first (best value) so favorites float to the top; stable by name.
			foodChoices.Sort(delegate (GameObject A, GameObject B)
			{
				int ca = GetCount(counts, A.Blueprint);
				int cb = GetCount(counts, B.Blueprint);
				if (ca != cb)
				{
					return ca.CompareTo(cb);
				}
				string na = A.GetDisplayName(Single: true, NoColor: true) ?? A.Blueprint;
				string nb = B.GetDisplayName(Single: true, NoColor: true) ?? B.Blueprint;
				return string.Compare(na, nb, StringComparison.OrdinalIgnoreCase);
			});

			GameObject chosen = Popup.PickGameObject(
				"Feed " + PetName(Pet) + " what?",
				foodChoices,
				AllowEscape: true,
				ShowContext: false,
				UseYourself: false,
				ExtraLabels: delegate (GameObject o) { return GetCount(counts, o.Blueprint).ToString(); }
			);
			if (chosen == null)
			{
				return;
			}
			Feed(Actor, Pet, chosen);
		}

		/// <summary>Twiddled a food item: pick an adjacent pet, then feed it.</summary>
		static void DoFeedToPet(GameObject Actor, GameObject FoodItem)
		{
			Actor = ResolveActor(Actor);
			if (Actor == null || FoodItem == null || FoodItem.IsInvalid())
			{
				return;
			}
			if (!FoodItem.HasPart<Food>() || FoodItem.IsImportant() || FoodItem.InInventory != Actor)
			{
				Msg("&YYou can't feed that to a pet.");
				return;
			}

			List<GameObject> pets = CollectAdjacentPets(Actor);
			if (pets.Count == 0)
			{
				Msg("&YThere is no pet nearby to feed.");
				return;
			}

			GameObject pet;
			if (pets.Count == 1)
			{
				pet = pets[0];
			}
			else
			{
				pet = Popup.PickGameObject(
					"Feed which pet?",
					pets,
					AllowEscape: true,
					ShowContext: false,
					UseYourself: false
				);
				if (pet == null)
				{
					return;
				}
			}
			Feed(Actor, pet, FoodItem);
		}

		// ------------------------------------------------------------------
		//  The feeding
		// ------------------------------------------------------------------

		static void Feed(GameObject Actor, GameObject Pet, GameObject FoodItem)
		{
			if (Actor == null || Actor.IsInvalid() || Pet == null || Pet.IsInvalid() || FoodItem == null || FoodItem.IsInvalid())
			{
				return;
			}
			if (!IsPet(Pet) || Pet.DistanceTo(Actor) > FEED_RANGE)
			{
				Msg("&YYou are too far away to feed " + PetName(Pet) + ".");
				return;
			}
			if (FoodItem.InInventory != Actor)
			{
				Msg("&YYou no longer have that.");
				return;
			}

			string foodBlueprint = FoodItem.Blueprint;
			string foodLabel = FoodItem.GetDisplayName(Single: true, NoColor: true) ?? foodBlueprint;

			long today = GetToday();
			Dictionary<string, int> counts = GetEffectiveCounts(Pet, today);
			int fedBefore = GetCount(counts, foodBlueprint);
			bool tired = fedBefore >= TIRED_THRESHOLD;

			// 1. Flavour.  Before Destroy(): Messaging drops messages for objects
			//    it can't validate, and a stacked food's last removal invalidates it.
			Messaging.XDidYToZ(Pet, "gobble", "up", FoodItem);

			// 2. The exclamation (love / getting tired / loathe).
			if (tired)
			{
				Messaging.XDidY(Pet, "make", Extra: "a face at the " + foodLabel + ". &r\"Ugh, not this again!\"");
			}
			else if (fedBefore >= 3)
			{
				Messaging.XDidY(Pet, "accept", Extra: "the " + foodLabel + ", a bit wearily. &y\"This again?\"");
			}
			else if (fedBefore == 0)
			{
				Messaging.XDidY(Pet, "perk", Extra: "up at the " + foodLabel + ". &G\"Ooh, something new!\"");
			}
			else
			{
				Messaging.XDidY(Pet, "wag", Extra: "happily over the " + foodLabel + ". &g\"My favorite!\"");
			}

			// 3. Consume one.  Stacker cancels Destroy() on stacks and decrements
			//    StackCount (Stacker.HandleEvent(BeforeDestroyObjectEvent)), so this
			//    always costs exactly 1 item.
			FoodItem.Destroy();

			// 4. Record the feeding (persists on the pet).  Stored count becomes
			//    effective-before + 1 at today's day.
			counts[foodBlueprint] = fedBefore + 1;
			SetFeedCounts(Pet, counts, today);

			// 5. Treasure right away as thanks (downgraded if the pet is tired).
			string blueprint = RollGiftBlueprint(tired);
			GameObject gift = (blueprint != null) ? GameObject.Create(blueprint) : null;
			if (gift != null && Actor.ReceiveObject(gift))
			{
				Messaging.XDidYToZ(Pet, "give", null, Actor, gift.t(int.MaxValue, null, null, AsIfKnown: false, Single: true));
			}

			// 6. Pellets: produce 1 locally now and share it, then the rest over
			//    the course of the day while digesting (produced locally, pet
			//    approaches to share each when ready).
			GratefulPetsDigestion digestion = Pet.RequirePart<GratefulPetsDigestion>();
			digestion.BeginDigestion(PELLETS_NOW, PELLETS_DIGESTING, CurrentTick());

			// 7. It costs a turn.
			Actor.UseEnergy(FEED_ENERGY_COST, "Feed Pet");
		}

		// ------------------------------------------------------------------
		//  Food & pet discovery
		// ------------------------------------------------------------------

		/// <summary>
		/// One representative GameObject per feedable food blueprint in inventory
		/// (non-important, has a Food part).  Used as the picker list.
		/// </summary>
		static List<GameObject> CollectFeedableFoods(GameObject Actor)
		{
			List<GameObject> result = new List<GameObject>();
			HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<GameObject> inventory = Actor.GetInventory();
			if (inventory == null)
			{
				return result;
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
				if (seen.Add(item.Blueprint))
				{
					result.Add(item);
				}
			}
			return result;
		}

		static List<GameObject> CollectAdjacentPets(GameObject Actor)
		{
			Actor = ResolveActor(Actor);
			List<GameObject> result = new List<GameObject>();
			Cell cell = Actor?.CurrentCell;
			if (cell == null)
			{
				return result;
			}
			foreach (Cell adjacent in cell.GetAdjacentCells(1, LocalOnly: true, BuiltOnly: false))
			{
				if (adjacent == null)
				{
					continue;
				}
				foreach (GameObject obj in adjacent.GetObjects())
				{
					if (obj != null && !obj.IsInvalid() && IsPet(obj))
					{
						result.Add(obj);
					}
				}
			}
			return result;
		}

		// ------------------------------------------------------------------
		//  Per-pet taste log (persisted in a serialized string Property)
		//
		//  Format:  blueprint \t storedCount \t day  (one record per line)
		//  Effective count decays by 1 per in-game day since 'day', floored at 0.
		// ------------------------------------------------------------------

		static long GetToday()
		{
			long ticks = (long)Calendar.TotalTimeTicks;
			if (ticks < 0)
			{
				ticks = 0;
			}
			return ticks / TURNS_PER_DAY;
		}

		static Dictionary<string, int> GetEffectiveCounts(GameObject Pet, long Today)
		{
			Dictionary<string, int> result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			string raw = Pet?.GetStringProperty(FEEDLOG_PROPERTY);
			if (string.IsNullOrEmpty(raw))
			{
				return result;
			}
			string[] lines = raw.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < lines.Length; i++)
			{
				string[] parts = lines[i].Split('\t');
				if (parts.Length < 3)
				{
					continue;
				}
				string bp = parts[0];
				int stored;
				long day;
				if (string.IsNullOrEmpty(bp) || !int.TryParse(parts[1], out stored) || !long.TryParse(parts[2], out day))
				{
					continue;
				}
				long decay = Today - day;
				if (decay < 0)
				{
					decay = 0;
				}
				int effective = stored - (int)decay;
				if (effective > 0)
				{
					result[bp] = effective;
				}
			}
			return result;
		}

		static void SetFeedCounts(GameObject Pet, Dictionary<string, int> Counts, long Today)
		{
			if (Pet == null)
			{
				return;
			}
			List<string> lines = new List<string>();
			foreach (KeyValuePair<string, int> kv in Counts)
			{
				if (kv.Value > 0 && !string.IsNullOrEmpty(kv.Key))
				{
					lines.Add(kv.Key + "\t" + kv.Value + "\t" + Today);
				}
			}
			if (lines.Count == 0)
			{
				Pet.SetStringProperty(FEEDLOG_PROPERTY, null, RemoveIfNull: true);
			}
			else
			{
				Pet.SetStringProperty(FEEDLOG_PROPERTY, string.Join("\n", lines.ToArray()));
			}
		}

		static int GetCount(Dictionary<string, int> Counts, string Blueprint)
		{
			int value;
			if (Counts != null && !string.IsNullOrEmpty(Blueprint) && Counts.TryGetValue(Blueprint, out value))
			{
				return value;
			}
			return 0;
		}

		// ------------------------------------------------------------------
		//  Gifts & helpers
		// ------------------------------------------------------------------

		static string RollGiftBlueprint(bool Tired)
		{
			if (Tired)
			{
				return RollWeighted(JunkGiftBlueprints, JunkGiftWeights);
			}
			return RollWeighted(GiftBlueprints, GiftWeights);
		}

		static string RollWeighted(string[] Blueprints, int[] Weights)
		{
			if (Blueprints == null || Blueprints.Length == 0 || Weights == null || Weights.Length == 0)
			{
				return null;
			}
			int total = 0;
			for (int i = 0; i < Weights.Length; i++)
			{
				total += Weights[i];
			}
			if (total <= 0)
			{
				return null;
			}
			int roll = Stat.Random(1, total);
			for (int i = 0; i < Blueprints.Length && i < Weights.Length; i++)
			{
				roll -= Weights[i];
				if (roll <= 0)
				{
					return Blueprints[i];
				}
			}
			return Blueprints[Blueprints.Length - 1];
		}

		static GameObject ResolveActor(GameObject Actor)
		{
			if (Actor != null && !Actor.IsInvalid())
			{
				return Actor;
			}
			return The.Player;
		}

		static string PetName(GameObject Pet)
		{
			return Pet.GetDisplayName(Single: true, WithDefiniteArticle: true);
		}

		static void Msg(string Text)
		{
			IComponent<GameObject>.AddPlayerMessage(Text);
		}

		public static long CurrentTick()
		{
			try
			{
				return (long)Calendar.TotalTimeTicks;
			}
			catch
			{
				return 0;
			}
		}

		// ------------------------------------------------------------------
		//  Conversation injection: "Feed" lives under chat so conversation
		//  gating (range, busy/fighting, can-start) applies for free.
		// ------------------------------------------------------------------

		public const string FEED_CHOICE_ID = "GratefulPets_Feed";

		static void InjectFeedChoice(Node StartNode)
		{
			if (StartNode == null)
			{
				return;
			}
			if (StartNode.Elements != null)
			{
				for (int i = 0; i < StartNode.Elements.Count; i++)
				{
					Choice existing = StartNode.Elements[i] as Choice;
					if (existing != null && existing.ID == FEED_CHOICE_ID)
					{
						return;
					}
				}
			}

			Choice choice = new Choice();
			choice.Parent = StartNode;
			choice.ID = FEED_CHOICE_ID;
			choice.Text = "Let me feed you.";
			choice.Ordinal = 5;
			choice.Target = "End"; // keep Target last (it adjusts priority)

			XRL.World.Conversations.Parts.GratefulPetsFeedChoicePart part = new XRL.World.Conversations.Parts.GratefulPetsFeedChoicePart();
			part.ParentElement = choice;
			choice.Parts = new List<IConversationPart>(1);
			choice.Parts.Add(part);
			part.Initialize();

			StartNode.Elements = StartNode.Elements ?? new List<IConversationElement>();
			StartNode.Elements.Add(choice);
		}

		// ------------------------------------------------------------------
		//  Desalination pellet -> container
		// ------------------------------------------------------------------

		static GameObject FindDesalinationPellet(GameObject Actor)
		{
			Actor = ResolveActor(Actor);
			List<GameObject> inventory = Actor?.GetInventory();
			if (inventory == null)
			{
				return null;
			}
			for (int i = 0; i < inventory.Count; i++)
			{
				GameObject item = inventory[i];
				if (item != null && !item.IsInvalid() && item.Blueprint == PELLET_BLUEPRINT)
				{
					return item;
				}
			}
			return null;
		}

		/// <summary>
		/// Apply a carried desalination pellet directly to a liquid container.
		/// Mirrors DesalinationPellet.HandleEvent(InventoryActionEvent "Apply").
		/// </summary>
		static void DoApplyPelletToContainer(GameObject Actor, GameObject Container)
		{
			Actor = ResolveActor(Actor);
			if (Actor == null || Actor.IsInvalid() || Container == null || Container.IsInvalid())
			{
				return;
			}
			if (!Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return;
			}
			if (Container.LiquidVolume == null)
			{
				Actor.ShowFailure("That can't hold liquid.");
				return;
			}

			GameObject pellet = FindDesalinationPellet(Actor);
			if (pellet == null)
			{
				Actor.ShowFailure("You have no desalination pellets.");
				return;
			}

			DesalinationPellet part = pellet.GetPart<DesalinationPellet>();
			if (part == null)
			{
				Actor.ShowFailure("That pellet can't be used.");
				return;
			}

			// Capture names before purification: PurifyLiquid(Single:true) consumes
			// one (RemoveOne) and obliterates the purifier, which can invalidate
			// the pellet reference when it's the last in a stack.
			string pelletName = pellet.an(int.MaxValue, null, null, AsIfKnown: false, Single: true);
			string containerName = Container.t(int.MaxValue, null, null, AsIfKnown: false, Single: true);
			string result = part.PurifyLiquid(Container, Actor, ShowMessage: false, Single: true, Actor.IsPlayer());
			string message = "You drop " + pelletName + " into " + containerName + ".";
			if (!string.IsNullOrEmpty(result))
			{
				message = message + "\n\n" + result;
			}
			IComponent<GameObject>.EmitMessage(Actor, message, ' ', FromDialog: true);
			Actor.UseEnergy(1000, "Item Applicator");
		}

		/// <summary>
		/// Real pets only: Pet / StartingPet tag, or the Pettable part.
		/// Deliberately not IsPlayerLed(), so beguiled/proselytized followers are excluded.
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

	/// <summary>
	/// Lives on a fed pet. Produces desalination pellets locally on schedule and
	/// approaches the party leader to share them when ready.
	/// </summary>
	[Serializable]
	public class GratefulPetsDigestion : IPart
	{
		public int RemainingToProduce;
		public long NextProductionTick;
		public int HeldForDelivery;

		public void BeginDigestion(int ProduceNow, int ProduceLater, long NowTick)
		{
			// Produce the immediate pellet(s) locally and try to share now.
			for (int i = 0; i < ProduceNow; i++)
			{
				ProduceOne();
			}
			RemainingToProduce += ProduceLater;
			if (RemainingToProduce > 0 && NextProductionTick <= 0)
			{
				NextProductionTick = NowTick + GratefulPets.DIGESTION_INTERVAL;
			}
			TryDeliverOrApproach();
		}

		public override bool WantTurnTick()
		{
			return true;
		}

		public override void TurnTick(long TimeTick, int Amount)
		{
			TickDigestion();
		}

		public void TickDigestion()
		{
			if (ParentObject == null || ParentObject.IsInvalid())
			{
				return;
			}
			long now = GratefulPets.CurrentTick();
			while (RemainingToProduce > 0 && NextProductionTick > 0 && now >= NextProductionTick)
			{
				ProduceOne();
				RemainingToProduce--;
				NextProductionTick += GratefulPets.DIGESTION_INTERVAL;
			}
			if (RemainingToProduce <= 0)
			{
				NextProductionTick = 0;
			}
			TryDeliverOrApproach();
		}

		void ProduceOne()
		{
			if (ParentObject == null || ParentObject.IsInvalid())
			{
				return;
			}
			GameObject pellet = GameObject.Create(GratefulPets.PELLET_BLUEPRINT);
			if (pellet == null)
			{
				return;
			}
			pellet.SetIntProperty("GratefulPets.GiftPellet", 1);
			ParentObject.ReceiveObject(pellet);
			HeldForDelivery++;
		}

		void TryDeliverOrApproach()
		{
			if (HeldForDelivery <= 0)
			{
				// Reconcile in case of odd state.
				HeldForDelivery = CountHeldGiftPellets();
				if (HeldForDelivery <= 0)
				{
					return;
				}
			}

			GameObject player = The.Player;
			if (player == null || player.IsInvalid() || ParentObject == null || ParentObject.IsInvalid())
			{
				return;
			}

			if (ParentObject.DistanceTo(player) <= 1)
			{
				DeliverOne(player);
				// If more remain, deliver gradually on subsequent ticks.
				return;
			}

			// Not adjacent: nudge a player-led pet to approach and share.
			if (ParentObject.IsPlayerLed())
			{
				EnsureApproach(player);
			}
		}

		void DeliverOne(GameObject Player)
		{
			GameObject pellet = FindHeldGiftPellet();
			if (pellet == null)
			{
				HeldForDelivery = 0;
				return;
			}
			pellet.RemoveIntProperty("GratefulPets.GiftPellet");
			// Move from pet to player.
			pellet.RemoveFromContext();
			if (Player.ReceiveObject(pellet))
			{
				Messaging.XDidYToZ(ParentObject, "hand", null, Player, "a desalination pellet");
			}
			else
			{
				// Fallback: try by blueprint so the player isn't cheated.
				Player.ReceiveObject(GratefulPets.PELLET_BLUEPRINT, 1);
				pellet.Obliterate();
				Messaging.XDidYToZ(ParentObject, "hand", null, Player, "a desalination pellet");
			}
			HeldForDelivery--;
			if (HeldForDelivery < 0)
			{
				HeldForDelivery = CountHeldGiftPellets();
			}
		}

		void EnsureApproach(GameObject Player)
		{
			if (ParentObject?.Brain == null || Player == null)
			{
				return;
			}
			// Pet already follows the leader; push a move goal if it's not already
			// right next to the player so it actively closes distance to share.
			if (ParentObject.DistanceTo(Player) > 1)
			{
				ParentObject.Brain.PushGoal(new MoveTo(Player, careful: false, overridesCombat: false, 1));
			}
		}

		GameObject FindHeldGiftPellet()
		{
			List<GameObject> inv = ParentObject?.GetInventory();
			if (inv == null)
			{
				return null;
			}
			for (int i = 0; i < inv.Count; i++)
			{
				GameObject item = inv[i];
				if (item != null && !item.IsInvalid() && item.GetIntProperty("GratefulPets.GiftPellet") > 0)
				{
					return item;
				}
			}
			return null;
		}

		int CountHeldGiftPellets()
		{
			int n = 0;
			List<GameObject> inv = ParentObject?.GetInventory();
			if (inv == null)
			{
				return 0;
			}
			for (int i = 0; i < inv.Count; i++)
			{
				GameObject item = inv[i];
				if (item != null && !item.IsInvalid() && item.GetIntProperty("GratefulPets.GiftPellet") > 0)
				{
					n++;
				}
			}
			return n;
		}
	}
}

namespace XRL.World.Conversations.Parts
{
	public class GratefulPetsFeedChoicePart : IConversationPart
	{
		public override bool WantEvent(int ID, int Propagation)
		{
			return base.WantEvent(ID, Propagation) || ID == EnterElementEvent.ID;
		}

		public override bool HandleEvent(EnterElementEvent E)
		{
			GameObject pet = The.Speaker;
			GameObject actor = The.Player;
			if (pet != null && actor != null && XRL.World.Parts.GratefulPets.IsPet(pet))
			{
				XRL.World.Parts.GratefulPets.DoFeedPet(actor, pet);
			}
			return base.HandleEvent(E);
		}
	}
}
