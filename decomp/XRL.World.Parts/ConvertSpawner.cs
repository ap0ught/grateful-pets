using System;
using Qud.API;
using XRL.Rules;
using XRL.Wish;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class ConvertSpawner : IPart
{
	public string Faction = "Mechanimists";

	public bool DoesWander = true;

	public bool IsPilgrim;

	public const int CHANCE_FOR_NONGOATFOLK_JUNGLE_CONVERT = 40;

	public const int CHANCE_FOR_ALL_JUNGLE_CONVERT = 10;

	public const int CHANCE_FOR_HUMANOID_CONVERT = 15;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		GameObject gameObject;
		if (Faction == "Kyakukya")
		{
			gameObject = CreateKyakukyanConvert(null, DoesWander);
		}
		else
		{
			gameObject = ((!(Faction == "YdFreehold")) ? CreateMechanimistConvert(null, DoesWander, IsPilgrim, ParentObject.HasTag("IsLibrarian")) : CreateYdFreeholdConvert(null, DoesWander));
		}
		gameObject.FireEvent("VillageInit");
		gameObject.SetIntProperty("Social", 1);
		gameObject.SetStringProperty("SpawnedFrom", ParentObject.Blueprint);
		E.ReplacementObject = gameObject;
		return base.HandleEvent(E);
	}

	public static GameObject CreateMechanimistConvert(GameObject ParentObject = null, bool DoesWander = true, bool IsPilgrim = false, bool IsLibrarian = false)
	{
		GameObject gameObject = ParentObject;
		if (ParentObject == null)
		{
			do
			{
				gameObject = GetBaseObject(IsLibrarian);
			}
			while (gameObject.Brain == null || gameObject.Brain.Allegiance.ContainsKey("Mechanimists"));
		}
		gameObject.Brain.Allegiance.Clear();
		gameObject.Brain.Allegiance.Add("Mechanimists", 100);
		gameObject.Brain.Allegiance.Hostile = false;
		if (DoesWander)
		{
			gameObject.Brain.Wanders = true;
			gameObject.Brain.WandersRandomly = true;
			gameObject.AddPart(new AIShopper());
		}
		else
		{
			gameObject.Brain.Wanders = false;
			gameObject.Brain.WandersRandomly = false;
			gameObject.AddPart(new AISitting());
		}
		gameObject.ReceiveObject("Canticles3");
		ConversationScript part = gameObject.GetPart<ConversationScript>();
		if (IsPilgrim)
		{
			gameObject.RequirePart<AIPilgrim>();
			if (part != null)
			{
				part.Append = "\n\nGlory to Shekhinah.~\n\nHumble before my Fathers, I walk.~\n\nShow mercy to a weary pilgrim.~\n\nPraise be upon Nisroch, who shelters us stiltseekers.";
			}
		}
		else
		{
			gameObject.RemovePart<AIPilgrim>();
			if (part != null)
			{
				part.Append = "\n\nGlory to Shekhinah.~\n\nMay the ground shake but the Six Day Stilt never tumble!~\n\nPraise our argent Fathers! Wisest of all beings.";
			}
		}
		if (IsLibrarian)
		{
			gameObject.AddPart(new MechanimistLibrarian());
			gameObject.SetStringProperty("Mayor", "Mechanimists");
		}
		else
		{
			gameObject.RequirePart<SocialRoles>().RequireRole("Mechanimist convert");
		}
		return gameObject;
	}

	public static GameObject CreateKyakukyanConvert(GameObject ParentObject = null, bool DoesWander = true)
	{
		GameObject gameObject = ParentObject;
		if (ParentObject == null)
		{
			do
			{
				gameObject = GetBaseObject_Kyakukya();
			}
			while (gameObject.Brain == null || gameObject.Brain.Allegiance.ContainsKey("Kyakukya"));
		}
		gameObject.Brain.Allegiance.Clear();
		gameObject.Brain.Allegiance.Add("Kyakukya", 100);
		gameObject.Brain.Allegiance.Hostile = false;
		if (DoesWander)
		{
			gameObject.Brain.Wanders = true;
			gameObject.Brain.WandersRandomly = true;
		}
		else
		{
			gameObject.Brain.Wanders = false;
			gameObject.Brain.WandersRandomly = false;
			gameObject.AddPart(new AISitting());
		}
		gameObject.RemovePart<AIPilgrim>();
		gameObject.ReceiveObject("Grave Goods");
		gameObject.ReceiveObject("Plump Mushroom", Stat.Random(2, 5));
		ConversationScript part = gameObject.GetPart<ConversationScript>();
		if (part != null)
		{
			part.Append = "\n\nSix fingers to the earthen lips.~\n\nPlease you to seek him.~\n\nCome gather! -and weave for the passing.~\n\nOur roots loosen jewels from the soil.~\n\nBe ape-still and muse.~\n\nWhat will you strum and dust for Saad?";
		}
		gameObject.RequirePart<SocialRoles>().RequireRole("worshipper of Oboroqoru");
		return gameObject;
	}

	public static GameObject CreateYdFreeholdConvert(GameObject ParentObject = null, bool DoesWander = true)
	{
		GameObject gameObject = ParentObject;
		if (ParentObject == null)
		{
			do
			{
				gameObject = GetBaseObject();
			}
			while (gameObject.Brain == null || gameObject.Brain.Allegiance.ContainsKey("YdFreehold"));
		}
		gameObject.Brain.Allegiance.Clear();
		gameObject.Brain.Allegiance.Add("YdFreehold", 100);
		gameObject.Brain.Allegiance.Hostile = false;
		gameObject.Reefer = true;
		if (DoesWander)
		{
			gameObject.Brain.Wanders = true;
			gameObject.Brain.WandersRandomly = true;
			gameObject.SetIntProperty("WanderUpStairs", 1);
			gameObject.SetIntProperty("WanderDownStairs", 1);
		}
		else
		{
			gameObject.Brain.Wanders = false;
			gameObject.Brain.WandersRandomly = false;
			gameObject.AddPart(new AISitting());
		}
		gameObject.RemovePart<AIPilgrim>();
		gameObject.RequirePart<SocialRoles>().RequireRole("denizen of the Yd Freehold");
		return gameObject;
	}

	private static GameObject GetBaseObject(bool IsLibrarian = false)
	{
		if (!IsLibrarian)
		{
			return EncountersAPI.GetALegendaryEligibleCreature((GameObjectBlueprint o) => !o.HasTag("ExcludeFromVillagePopulations"));
		}
		return EncountersAPI.GetALegendaryEligibleCreatureWithAnInventory((GameObjectBlueprint o) => !o.HasTag("NoLibrarian") && !o.HasTag("ExcludeFromVillagePopulations"));
	}

	private static GameObject GetBaseObject_Kyakukya()
	{
		int num = Stat.Roll(1, 100);
		if (num <= 40)
		{
			return EncountersAPI.GetALegendaryEligibleCreature((GameObjectBlueprint o) => !o.HasTag("ExcludeFromVillagePopulations") && o.HasTag("DynamicObjectsTable:Jungle_Creatures") && !o.InheritsFrom("Goatfolk"));
		}
		if (num <= 50)
		{
			return EncountersAPI.GetALegendaryEligibleCreature((GameObjectBlueprint o) => !o.HasTag("ExcludeFromVillagePopulations") && o.HasTag("DynamicObjectsTable:Jungle_Creatures"));
		}
		if (num <= 65)
		{
			return EncountersAPI.GetALegendaryEligibleCreature((GameObjectBlueprint o) => !o.HasTag("ExcludeFromVillagePopulations") && o.HasTag("Humanoid") && !o.InheritsFrom("Goatfolk"));
		}
		return EncountersAPI.GetALegendaryEligibleCreature((GameObjectBlueprint o) => !o.HasTag("ExcludeFromVillagePopulations"));
	}

	[WishCommand("convert", null)]
	public static void Wish(string Param)
	{
		Param.Split(':', out var First, out var Second);
		if (Second.IsNullOrEmpty())
		{
			Second = First;
			First = "mechanimist";
		}
		WishResult wishResult = WishSearcher.SearchForBlueprint(Second);
		GameObject parentObject = GameObjectFactory.Factory.CreateObject(wishResult.Result, 0, 0, null, null, null, "Wish");
		if (First.Contains("freehold", StringComparison.OrdinalIgnoreCase))
		{
			parentObject = CreateYdFreeholdConvert(parentObject);
		}
		else
		{
			parentObject = ((!First.Contains("kyakukya", StringComparison.OrdinalIgnoreCase)) ? CreateMechanimistConvert(parentObject) : CreateKyakukyanConvert(parentObject));
		}
		parentObject.FireEvent("VillageInit");
		The.PlayerCell.getClosestEmptyCell().AddObject(parentObject);
	}
}
