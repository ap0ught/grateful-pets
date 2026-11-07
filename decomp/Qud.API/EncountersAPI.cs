using System;
using System.Collections.Generic;
using HistoryKit;
using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace Qud.API;

public static class EncountersAPI
{
	public const int ANIMATED_OBJECT_CHANCE_ONE_IN = 5000;

	public static GameObject GetACreature(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.Create(GetACreatureBlueprint(filter));
	}

	public static GameObject GetASampleCreature(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.CreateSample(GetACreatureBlueprint(filter));
	}

	public static GameObject GetACreatureFromFaction(string faction, Predicate<GameObjectBlueprint> filter = null)
	{
		return GameObject.Create(GetACreatureBlueprintFromFaction(faction, filter));
	}

	public static GameObject GetASampleCreatureFromFaction(string faction)
	{
		return GameObjectFactory.Factory.GetFactionMembers(faction).GetRandomElement()?.createSample();
	}

	public static GameObject GetAPlant()
	{
		return GameObject.Create(GetAPlantBlueprint());
	}

	public static GameObject GetANonLegendaryCreature(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.Create(GetANonLegendaryCreatureBlueprint(filter));
	}

	public static GameObject GetANonLegendaryCreatureWithAnInventory(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.Create(GetANonLegendaryCreatureWithAnInventoryBlueprint(filter));
	}

	public static GameObject GetALegendaryEligibleCreature(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.Create(GetALegendaryEligibleCreatureBlueprint(filter));
	}

	public static GameObject GetALegendaryEligibleCreatureFromFaction(string faction, Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.Create(GetALegendaryEligibleCreatureBlueprintFromFaction(faction, filter));
	}

	public static GameObject GetALegendaryEligibleCreatureWithAnInventory(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.Create(GetALegendaryEligibleCreatureWithAnInventoryBlueprint(filter));
	}

	public static GameObject GetCreatureAroundPlayerLevel(Predicate<GameObjectBlueprint> filter = null)
	{
		if (The.Player == null)
		{
			return GetCreatureAroundLevel(15);
		}
		int num = Stat.Random(-2, 2);
		return GetCreatureAroundLevel(Math.Max(The.Player.Stat("Level") + num, 1), filter);
	}

	public static GameObject GetNonLegendaryCreatureAroundPlayerLevel(Predicate<GameObjectBlueprint> filter = null)
	{
		if (The.Player == null)
		{
			return GetNonLegendaryCreatureAroundLevel(15);
		}
		int num = Stat.Random(-2, 2);
		return GetNonLegendaryCreatureAroundLevel(Math.Max(The.Player.Stat("Level") + num, 1), filter);
	}

	public static GameObject GetCreatureAroundLevel(int targetLevel, Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		while (targetLevel > 0)
		{
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (!IsEligibleForDynamicEncounters(blueprint) || !blueprint.HasTag("Creature") || !blueprint.HasStat("Level") || blueprint.BaseStat("Level") != targetLevel || (filter != null && !filter(blueprint)))
				{
					continue;
				}
				if (blueprint.HasTag("AggregateWith"))
				{
					string tag = blueprint.GetTag("AggregateWith");
					if (list2.Contains(tag))
					{
						continue;
					}
					list2.Add(tag);
				}
				list.Add(blueprint);
			}
			if (list.Count > 0)
			{
				return GameObject.Create(list.GetRandomElement().Name);
			}
			targetLevel--;
		}
		return GameObject.Create("Dog");
	}

	public static GameObject GetNonLegendaryCreatureAroundLevel(int targetLevel, Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		while (targetLevel > 0)
		{
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (!IsEligibleForDynamicEncounters(blueprint) || !IsLegendaryEligible(blueprint) || !blueprint.HasTag("Creature") || !blueprint.HasStat("Level") || blueprint.BaseStat("Level") != targetLevel || (filter != null && !filter(blueprint)))
				{
					continue;
				}
				if (blueprint.HasTag("AggregateWith"))
				{
					string tag = blueprint.GetTag("AggregateWith");
					if (list2.Contains(tag))
					{
						continue;
					}
					list2.Add(tag);
				}
				list.Add(blueprint);
			}
			if (list.Count > 0)
			{
				return GameObject.Create(list.GetRandomElement().Name);
			}
			targetLevel--;
		}
		return GameObject.Create("Dog");
	}

	public static GameObject GetAnObject(Predicate<GameObjectBlueprint> filter = null)
	{
		string anObjectBlueprint = GetAnObjectBlueprint(filter);
		if (anObjectBlueprint == null)
		{
			return null;
		}
		return GameObject.Create(anObjectBlueprint);
	}

	public static GameObject GetAnObjectNoExclusions(Predicate<GameObjectBlueprint> filter = null)
	{
		string anObjectBlueprintNoExclusions = GetAnObjectBlueprintNoExclusions(filter);
		if (anObjectBlueprintNoExclusions == null)
		{
			return null;
		}
		return GameObject.Create(anObjectBlueprintNoExclusions);
	}

	public static GameObject GetAnItem(Predicate<GameObjectBlueprint> filter = null)
	{
		string anItemBlueprint = GetAnItemBlueprint(filter);
		if (anItemBlueprint == null)
		{
			return null;
		}
		return GameObject.Create(anItemBlueprint);
	}

	public static GameObject GetAnAnimatedObject()
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!IsEligibleForDynamicEncounters(blueprint) || !blueprint.HasTag("Animatable"))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		GameObject gameObject = GameObject.Create(list.GetRandomElement().Name);
		if (!gameObject.HasPart<Brain>())
		{
			AnimateObject.Animate(gameObject);
		}
		return gameObject;
	}

	public static GameObjectBlueprint GetACreatureBlueprintModel(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!IsEligibleForDynamicEncounters(blueprint) || !blueprint.HasTag("Creature") || (filter != null && !filter(blueprint)))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement();
	}

	public static string GetACreatureBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		return GetACreatureBlueprintModel(filter)?.Name;
	}

	public static string GetACreatureBlueprintFromFaction(string faction, Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint factionMember in GameObjectFactory.Factory.GetFactionMembers(faction))
		{
			if (!IsEligibleForDynamicEncounters(factionMember) || (filter != null && !filter(factionMember)))
			{
				continue;
			}
			if (factionMember.HasTag("AggregateWith"))
			{
				string tag = factionMember.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(factionMember);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetAPlantBlueprint()
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!IsEligibleForDynamicEncounters(blueprint) || (!blueprint.HasTag("Plant") && !blueprint.HasTag("PlantLike")))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetANonLegendaryCreatureBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!IsEligibleForDynamicEncounters(blueprint) || !IsLegendaryEligible(blueprint))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith") && (filter == null || filter(blueprint)))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetANonLegendaryCreatureWithAnInventoryBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!IsEligibleForDynamicEncounters(blueprint) || !IsLegendaryEligible(blueprint) || !blueprint.HasPart("Inventory") || (filter != null && !filter(blueprint)))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetALegendaryEligibleCreatureBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!IsEligibleForDynamicEncounters(blueprint) || !IsLegendaryEligible(blueprint) || !blueprint.HasPart("Body") || !blueprint.HasPart("Combat") || (filter != null && !filter(blueprint)))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetALegendaryEligibleCreatureBlueprintFromFaction(string faction, Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint factionMember in GameObjectFactory.Factory.GetFactionMembers(faction))
		{
			if (!IsEligibleForDynamicEncounters(factionMember) || !IsLegendaryEligible(factionMember) || !factionMember.HasPart("Body") || !factionMember.HasPart("Combat") || (filter != null && !filter(factionMember)))
			{
				continue;
			}
			if (factionMember.HasTag("AggregateWith"))
			{
				string tag = factionMember.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(factionMember);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetALegendaryEligibleCreatureWithAnInventoryBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!IsEligibleForDynamicEncounters(blueprint) || !IsLegendaryEligible(blueprint) || !blueprint.HasPart("Body") || !blueprint.HasPart("Combat") || !blueprint.HasPart("Inventory") || (filter != null && !filter(blueprint)))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static GameObjectBlueprint GetABlueprintModel(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!filter(blueprint))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		return list.GetRandomElement();
	}

	public static string GetBlueprintWhere(Predicate<GameObjectBlueprint> filter = null)
	{
		return GetABlueprintModel(filter)?.Name;
	}

	public static GameObjectBlueprint GetAnItemBlueprintModel(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!IsEligibleForDynamicEncounters(blueprint) || !blueprint.HasTag("Item") || blueprint.IsNatural() || !blueprint.GetPartParameter("Physics", "IsReal", Default: true) || (filter != null && !filter(blueprint)))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		return list.GetRandomElement();
	}

	public static string GetAnItemBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		return GetAnItemBlueprintModel(filter)?.Name;
	}

	public static GameObjectBlueprint GetAnObjectBlueprintModel(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!IsEligibleForDynamicEncounters(blueprint) || blueprint.IsNatural() || !blueprint.GetPartParameter("Physics", "IsReal", Default: true) || (filter != null && !filter(blueprint)))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		return list.GetRandomElement();
	}

	public static string GetAnObjectBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		return GetAnObjectBlueprintModel(filter)?.Name;
	}

	public static string GetAnObjectBlueprintNoExclusions(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (blueprint.IntProps.ContainsKey("Natural") || !blueprint.GetPartParameter("Physics", "IsReal", Default: true) || (filter != null && !filter(blueprint)))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetARandomDescendantOf(string Parent)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!IsEligibleForDynamicEncounters(blueprint) || !blueprint.DescendsFrom(Parent))
			{
				continue;
			}
			if (blueprint.HasTag("AggregateWith"))
			{
				string tag = blueprint.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(blueprint);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static GameObject CreateARandomDescendantOf(string Parent)
	{
		string aRandomDescendantOf = GetARandomDescendantOf(Parent);
		if (!aRandomDescendantOf.IsNullOrEmpty())
		{
			return GameObject.Create(aRandomDescendantOf);
		}
		return null;
	}

	public static string GetATransmutationBlueprintFor(GameObject Object)
	{
		if (Object.IsCreature)
		{
			return GetACreatureBlueprint();
		}
		if (Object.HasTag("Item"))
		{
			if (!Object.HasPart<Food>())
			{
				return GetAnItemBlueprint();
			}
			return GetAnItemBlueprint((GameObjectBlueprint x) => x.HasPart("Food"));
		}
		return GetARandomDescendantOf(GetProgenitor(Object).Name);
	}

	private static GameObjectBlueprint GetProgenitor(GameObject Object)
	{
		GameObjectBlueprint gameObjectBlueprint = Object.GetBlueprint();
		while (true)
		{
			GameObjectBlueprint shallowParent = gameObjectBlueprint.ShallowParent;
			if (shallowParent == null || shallowParent.Name.EndsWith("Object"))
			{
				break;
			}
			gameObjectBlueprint = shallowParent;
		}
		return gameObjectBlueprint;
	}

	public static bool IsEligibleForDynamicEncounters(GameObjectBlueprint Blueprint)
	{
		if (Blueprint.IsBaseBlueprint())
		{
			return false;
		}
		if (!Blueprint.HasPart("Render"))
		{
			return false;
		}
		return !Blueprint.IsExcludedFromDynamicEncounters();
	}

	public static bool IsEligibleForDynamicSemanticEncounters(GameObjectBlueprint Blueprint)
	{
		if (Blueprint.IsBaseBlueprint())
		{
			return false;
		}
		if (!Blueprint.HasPart("Render"))
		{
			return false;
		}
		if (Blueprint.IsExcludedFromDynamicEncounters())
		{
			return Blueprint.HasTag("IncludeInDynamicSemanticTables");
		}
		return true;
	}

	public static bool IsLegendaryEligible(GameObjectBlueprint Blueprint)
	{
		if (!Blueprint.HasTag("Creature"))
		{
			return false;
		}
		if (Blueprint.HasPart("GivesRep"))
		{
			return false;
		}
		if (Blueprint.HasPart("Uplift"))
		{
			return false;
		}
		if (Blueprint.Name.Contains("Hero"))
		{
			return false;
		}
		return true;
	}
}
