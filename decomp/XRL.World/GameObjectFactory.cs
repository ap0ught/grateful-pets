using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Qud.API;
using XRL.Collections;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Loaders;
using XRL.World.ObjectBuilders;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World;

[HasWishCommand]
[HasModSensitiveStaticCache]
[HasGameBasedStaticCache]
public class GameObjectFactory
{
	[ModSensitiveStaticCache(false)]
	private static GameObjectFactory _Factory = null;

	private bool LoadBlueprintsHappened;

	/// <remarks>
	/// The same blueprint can exist under multiple keys in Blueprints for save compatibility purposes,
	/// iterate BlueprintList for distinct blueprints.
	/// </remarks>
	public Dictionary<string, GameObjectBlueprint> Blueprints = new Dictionary<string, GameObjectBlueprint>();

	public List<GameObjectBlueprint> BlueprintList = new List<GameObjectBlueprint>();

	public static Queue<GameObject> ObjectPool = new Queue<GameObject>();

	private Dictionary<int, List<GameObjectBlueprint>> Tiers = new Dictionary<int, List<GameObjectBlueprint>>();

	private Dictionary<int, uint> TierDeltaWeights;

	private Dictionary<int, uint> TechTierDeltaWeights;

	private Dictionary<string, double> RoleWeightMultipliers;

	public long ObjectsCreated;

	[NonSerialized]
	private static Event eObjectCreated = new Event("ObjectCreated", "Context", null);

	[NonSerialized]
	private static Event eCommandTakeObject = new Event("CommandTakeObject", "Object", (object)null, "EnergyCost", 0).SetSilent(Silent: true);

	[NonSerialized]
	public static Dictionary<string, string> populationContext = new Dictionary<string, string>();

	private static Dictionary<Type, IObjectBuilder> ObjectBuilders = new Dictionary<Type, IObjectBuilder>();

	public static GameObjectFactory Factory
	{
		get
		{
			if (_Factory == null)
			{
				Loading.LoadTask("Loading object blueprints", LoadFactory);
			}
			return _Factory;
		}
	}

	private static void LoadFactory()
	{
		_Factory = new GameObjectFactory();
		_Factory.LoadBlueprints();
	}

	public static void Init()
	{
		if (_Factory == null)
		{
			Loading.LoadTask("Loading object blueprints", LoadFactory);
		}
	}

	[GameBasedCacheInit]
	private static void GameInit()
	{
		foreach (GameObjectBlueprint blueprint in Factory.BlueprintList)
		{
			blueprint.ClearGameCache();
		}
	}

	public void DispatchLoadBlueprints(bool Task = true)
	{
		if (!LoadBlueprintsHappened)
		{
			LoadBlueprintsHappened = true;
			if (Task)
			{
				Loading.LoadTask("Dispatch LoadBlueprint", CallLoadBlueprint);
			}
			else
			{
				CallLoadBlueprint();
			}
		}
	}

	public void Hotload()
	{
		Blueprints = new Dictionary<string, GameObjectBlueprint>();
		BlueprintList = new List<GameObjectBlueprint>();
		LoadBlueprints();
	}

	public static GameObject create(string blueprint)
	{
		return Factory.CreateObject(blueprint);
	}

	[Obsolete("Use GameObject.Pool (allow true) or GameObject.Clear (allow false)")]
	public void Pool(GameObject objectToPool, bool allowGameObjectPool = false)
	{
		if (allowGameObjectPool)
		{
			objectToPool.Pool();
		}
		else
		{
			objectToPool.Clear();
		}
	}

	public bool AnyFactionMembers(string Faction)
	{
		return XRL.World.Faction.AnyMembers(Faction);
	}

	public List<GameObjectBlueprint> GetFactionMembers(string Faction, bool SkipExclude = false, bool ReadOnly = true)
	{
		return XRL.World.Faction.GetMembers(Faction, null, !SkipExclude, ReadOnly);
	}

	public void InitWeights()
	{
		if (TierDeltaWeights == null)
		{
			TierDeltaWeights = new Dictionary<int, uint>();
			TierDeltaWeights.Add(-7, 10u);
			TierDeltaWeights.Add(-6, 100u);
			TierDeltaWeights.Add(-5, 1000u);
			TierDeltaWeights.Add(-4, 10000u);
			TierDeltaWeights.Add(-3, 100000u);
			TierDeltaWeights.Add(-2, 1000000u);
			TierDeltaWeights.Add(-1, 10000000u);
			TierDeltaWeights.Add(0, 100000000u);
			TierDeltaWeights.Add(1, 10000000u);
			TierDeltaWeights.Add(2, 1000000u);
			TierDeltaWeights.Add(3, 100000u);
			TierDeltaWeights.Add(4, 10000u);
			TierDeltaWeights.Add(5, 1000u);
			TierDeltaWeights.Add(6, 100u);
			TierDeltaWeights.Add(7, 10u);
		}
		if (TechTierDeltaWeights == null)
		{
			TechTierDeltaWeights = new Dictionary<int, uint>();
			TechTierDeltaWeights.Add(-7, 10u);
			TechTierDeltaWeights.Add(-6, 100u);
			TechTierDeltaWeights.Add(-5, 1000u);
			TechTierDeltaWeights.Add(-4, 10000u);
			TechTierDeltaWeights.Add(-3, 100000u);
			TechTierDeltaWeights.Add(-2, 1000000u);
			TechTierDeltaWeights.Add(-1, 10000000u);
			TechTierDeltaWeights.Add(0, 100000000u);
			TechTierDeltaWeights.Add(1, 10000000u);
			TechTierDeltaWeights.Add(2, 1000000u);
			TechTierDeltaWeights.Add(3, 100000u);
			TechTierDeltaWeights.Add(4, 10000u);
			TechTierDeltaWeights.Add(5, 1000u);
			TechTierDeltaWeights.Add(6, 100u);
			TechTierDeltaWeights.Add(7, 10u);
		}
		if (RoleWeightMultipliers == null)
		{
			RoleWeightMultipliers = new Dictionary<string, double>();
			RoleWeightMultipliers.Add("Common", 4.0);
			RoleWeightMultipliers.Add("Minion", 4.0);
			RoleWeightMultipliers.Add("Artillery", 0.25);
			RoleWeightMultipliers.Add("Skirmisher", 1.0);
			RoleWeightMultipliers.Add("Uncommon", 0.25);
			RoleWeightMultipliers.Add("Brute", 0.25);
			RoleWeightMultipliers.Add("Tank", 0.25);
			RoleWeightMultipliers.Add("Specialist", 0.1);
			RoleWeightMultipliers.Add("Rare", 0.01);
			RoleWeightMultipliers.Add("Leader", 0.1);
			RoleWeightMultipliers.Add("Hero", 0.1);
			RoleWeightMultipliers.Add("Epic", 0.01);
		}
	}

	public void FabricateDynamicInheritsTable(string baseName)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		for (int i = 0; i < Factory.BlueprintList.Count; i++)
		{
			if (EncountersAPI.IsEligibleForDynamicEncounters(Factory.BlueprintList[i]) && Factory.BlueprintList[i].DescendsFrom(baseName))
			{
				list.Add(Factory.BlueprintList[i]);
			}
		}
		FabricateDynamicPopulationTable("DynamicInheritsTable:" + baseName, list);
	}

	public void FabricateDynamicArtifactsTable()
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		for (int i = 0; i < Factory.BlueprintList.Count; i++)
		{
			if (EncountersAPI.IsEligibleForDynamicEncounters(Factory.BlueprintList[i]) && Factory.BlueprintList[i].GetPartParameter("Examiner", "Complexity", 0) != 0)
			{
				list.Add(Factory.BlueprintList[i]);
			}
		}
		FabricateDynamicPopulationTable("DynamicArtifactsTable:", list);
	}

	public void FabricateDynamicHasPartTable(string baseName)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		for (int i = 0; i < Factory.BlueprintList.Count; i++)
		{
			if (EncountersAPI.IsEligibleForDynamicEncounters(Factory.BlueprintList[i]) && Factory.BlueprintList[i].HasPart(baseName))
			{
				list.Add(Factory.BlueprintList[i]);
			}
		}
		FabricateDynamicPopulationTable("DynamicHasPartTable:" + baseName, list);
	}

	public PopulationInfo FabricateDynamicSemanticTable(string Name)
	{
		string[] array = Name.Split(':');
		string[] array2 = array[1].Split(',');
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = "Semantic" + array2[i];
		}
		int tier = -1;
		if (array.Length > 2 && int.TryParse(array[2], out var result))
		{
			tier = result;
		}
		int techTier = -1;
		if (array.Length > 3 && int.TryParse(array[3], out result))
		{
			techTier = result;
		}
		return FabricateTagTable(Name, array2, tier, techTier, Dynamic: true, Strict: false, Cache: true);
	}

	public PopulationInfo FabricateTagTable(string Name, string Tag, int Tier = -1, int TechTier = -1, bool Dynamic = false, bool Strict = false, bool Cache = false)
	{
		using ScopeDisposedList<string> scopeDisposedList = ScopeDisposedList<string>.GetFromPool();
		scopeDisposedList.Add(Tag);
		return FabricateTagTable(Name, scopeDisposedList, Tier, TechTier, Dynamic, Strict, Cache);
	}

	public PopulationInfo FabricateTagTable(string Name, IReadOnlyList<string> Tags, int Tier = -1, int TechTier = -1, bool Dynamic = false, bool Strict = false, bool Cache = false)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		bool flag = !Strict;
		foreach (GameObjectBlueprint blueprint in BlueprintList)
		{
			if (blueprint.IsBaseBlueprint() || (Dynamic && !EncountersAPI.IsEligibleForDynamicSemanticEncounters(blueprint)) || !blueprint.HasTag(Tags[0]))
			{
				continue;
			}
			bool flag2 = true;
			int i = 1;
			for (int count = Tags.Count; i < count; i++)
			{
				if (!blueprint.HasTag(Tags[i]))
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				if (flag)
				{
					list.Clear();
					flag = false;
				}
				list.Add(blueprint);
			}
			else if (flag)
			{
				list.Add(blueprint);
			}
		}
		Tiers.Clear();
		uint num = 1u;
		InitWeights();
		PopulationInfo populationInfo = new PopulationInfo(Name);
		PopulationGroup populationGroup = new PopulationGroup();
		populationGroup.Chance = "100";
		populationGroup.Number = "1";
		populationGroup.Style = "pickone";
		string key = Name + ":Number";
		string key2 = Name + ":Builder";
		string key3 = Name + ":Weight";
		Dictionary<string, PopulationGroup> dictionary = new Dictionary<string, PopulationGroup>();
		for (int j = 0; j < list.Count; j++)
		{
			GameObjectBlueprint gameObjectBlueprint = list[j];
			if (gameObjectBlueprint.IsBaseBlueprint() || (Dynamic && !EncountersAPI.IsEligibleForDynamicSemanticEncounters(gameObjectBlueprint)))
			{
				continue;
			}
			int num2 = ((Tier == -1) ? int.MinValue : (Tier - gameObjectBlueprint.Tier));
			int num3 = ((TechTier == -1) ? int.MinValue : (Tier - gameObjectBlueprint.TechTier));
			uint num4 = num;
			if (num2 != int.MinValue && TierDeltaWeights.TryGetValue(num2, out var value))
			{
				num4 += value;
			}
			if (num3 != int.MinValue && TechTierDeltaWeights.TryGetValue(num3, out value))
			{
				num4 += value;
			}
			if ((gameObjectBlueprint.Tags.TryGetValue("Role", out var value2) || gameObjectBlueprint.Props.TryGetValue("Role", out value2)) && RoleWeightMultipliers.TryGetValue(value2, out var value3))
			{
				num4 = (uint)Math.Ceiling((double)num4 * value3);
			}
			if (gameObjectBlueprint.Tags.TryGetValue(key3, out value2))
			{
				try
				{
					double num5 = double.Parse(value2);
					num4 = (uint)Math.Ceiling((double)num4 * num5);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Invalid table weight tag on: " + j, x);
				}
			}
			if (flag)
			{
				uint num6 = 0u;
				int k = 0;
				for (int count2 = Tags.Count; k < count2; k++)
				{
					if (gameObjectBlueprint.HasTag(Tags[k]))
					{
						num6++;
					}
				}
				num4 *= num6;
			}
			if (num4 == 0)
			{
				continue;
			}
			PopulationObject populationObject = new PopulationObject();
			populationObject.Name = gameObjectBlueprint.Name;
			populationObject.Weight = num4;
			if (gameObjectBlueprint.Tags.TryGetValue(key, out var value4) && !value4.IsNullOrEmpty())
			{
				populationObject.Number = value4;
			}
			if (gameObjectBlueprint.Tags.TryGetValue(key2, out var value5) && !value5.IsNullOrEmpty())
			{
				populationObject.Builder = value5;
			}
			string tag = gameObjectBlueprint.GetTag("AggregateWith", null);
			if (!tag.IsNullOrEmpty())
			{
				if (!dictionary.TryGetValue(tag, out var value6))
				{
					value6 = new PopulationGroup();
					value6.Name = "aggregate:" + tag;
					value6.Weight = 1u;
					value6.Style = "pickone";
					dictionary.Add(tag, value6);
					populationGroup.Items.Add(value6);
				}
				value6.Weight = Math.Max(value6.Weight, num4);
				value6.Items.Add(populationObject);
			}
			else
			{
				populationGroup.Items.Add(populationObject);
			}
		}
		populationInfo.Items.Add(populationGroup);
		if (Cache && !PopulationManager.Populations.TryAdd(populationInfo.Name, populationInfo))
		{
			MetricsManager.LogWarning("Double entry during table fabrication for populationInfo: " + populationInfo.Name);
		}
		return populationInfo;
	}

	public void FabricateDynamicObjectsTable(string tableName)
	{
		int num = -1;
		int num2 = -1;
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		foreach (GameObjectBlueprint blueprint in BlueprintList)
		{
			if (blueprint.HasTag(tableName))
			{
				list.Add(blueprint);
			}
		}
		Tiers.Clear();
		uint num3 = 1u;
		InitWeights();
		PopulationInfo populationInfo = new PopulationInfo(tableName);
		PopulationGroup populationGroup = new PopulationGroup();
		populationGroup.Chance = "100";
		populationGroup.Number = "1";
		populationGroup.Style = "pickone";
		string key = tableName + ":Number";
		string key2 = tableName + ":Builder";
		string key3 = tableName + ":Weight";
		Dictionary<string, PopulationGroup> dictionary = new Dictionary<string, PopulationGroup>();
		for (int i = 0; i < list.Count; i++)
		{
			if (!EncountersAPI.IsEligibleForDynamicEncounters(list[i]))
			{
				continue;
			}
			int num4 = int.MinValue;
			int num5 = int.MinValue;
			if (num != -1)
			{
				num4 = num - list[i].Tier;
			}
			if (num2 != -1)
			{
				num5 = num2 - list[i].TechTier;
			}
			uint num6 = num3;
			if (num4 != int.MinValue && TierDeltaWeights.TryGetValue(num4, out var value))
			{
				num6 += value;
			}
			if (num5 != int.MinValue && TechTierDeltaWeights.TryGetValue(num5, out value))
			{
				num6 += value;
			}
			if ((list[i].Tags.TryGetValue("Role", out var value2) || list[i].Props.TryGetValue("Role", out value2)) && RoleWeightMultipliers.TryGetValue(value2, out var value3))
			{
				num6 = (uint)Math.Ceiling((double)num6 * value3);
			}
			if (list[i].Tags.ContainsKey(key3))
			{
				try
				{
					double num7 = Convert.ToDouble(list[i].Tags[key3]);
					num6 = (uint)Math.Ceiling((double)num6 * num7);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Invalid table weight tag on: " + i, x);
				}
			}
			if (num6 == 0)
			{
				continue;
			}
			if (!list[i].Tags.TryGetValue(key, out var value4))
			{
				value4 = "1";
			}
			if (!list[i].Tags.TryGetValue(key2, out var value5))
			{
				value5 = "1";
			}
			string tag = list[i].GetTag("AggregateWith", null);
			if (tag != null)
			{
				if (!dictionary.TryGetValue(tag, out var value6))
				{
					value6 = new PopulationGroup();
					value6.Name = "aggregate:" + tag;
					value6.Weight = 1u;
					value6.Style = "pickone";
					dictionary.Add(tag, value6);
					populationGroup.Items.Add(value6);
				}
				if (value6.Weight < num6)
				{
					value6.Weight = num6;
				}
				value6.Items.Add(new PopulationObject(list[i].Name, value4, num6, value5));
			}
			else
			{
				populationGroup.Items.Add(new PopulationObject(list[i].Name, value4, num6, value5));
			}
		}
		populationInfo.Items.Add(populationGroup);
		if (PopulationManager.Populations.ContainsKey(populationInfo.Name))
		{
			MetricsManager.LogWarning("Double entry during table fabrication for populationInfo: " + populationInfo.Name);
		}
		else
		{
			PopulationManager.Populations.Add(populationInfo.Name, populationInfo);
		}
	}

	public void FabricateStaticObjectsTable(string tableName)
	{
		int num = -1;
		int num2 = -1;
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		foreach (GameObjectBlueprint blueprint in BlueprintList)
		{
			if (blueprint.HasTag(tableName))
			{
				list.Add(blueprint);
			}
		}
		Tiers.Clear();
		uint num3 = 1u;
		InitWeights();
		PopulationInfo populationInfo = new PopulationInfo(tableName);
		PopulationGroup populationGroup = new PopulationGroup();
		populationGroup.Chance = "100";
		populationGroup.Number = "1";
		populationGroup.Style = "pickone";
		string key = tableName + ":Number";
		string key2 = tableName + ":Builder";
		string key3 = tableName + ":Weight";
		Dictionary<string, PopulationGroup> dictionary = new Dictionary<string, PopulationGroup>();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsBaseBlueprint())
			{
				continue;
			}
			int num4 = int.MinValue;
			int num5 = int.MinValue;
			if (num != -1)
			{
				num4 = num - list[i].Tier;
			}
			if (num2 != -1)
			{
				num5 = num2 - list[i].TechTier;
			}
			uint num6 = num3;
			if (num4 != int.MinValue && TierDeltaWeights.TryGetValue(num4, out var value))
			{
				num6 += value;
			}
			if (num5 != int.MinValue && TechTierDeltaWeights.TryGetValue(num5, out value))
			{
				num6 += value;
			}
			if ((list[i].Tags.TryGetValue("Role", out var value2) || list[i].Props.TryGetValue("Role", out value2)) && RoleWeightMultipliers.TryGetValue(value2, out var value3))
			{
				num6 = (uint)Math.Ceiling((double)num6 * value3);
			}
			if (list[i].Tags.ContainsKey(key3))
			{
				try
				{
					double num7 = Convert.ToDouble(list[i].Tags[key3]);
					num6 = (uint)Math.Ceiling((double)num6 * num7);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Invalid table weight tag on: " + i, x);
				}
			}
			if (num6 == 0)
			{
				continue;
			}
			if (!list[i].Tags.TryGetValue(key, out var value4))
			{
				value4 = "1";
			}
			if (!list[i].Tags.TryGetValue(key2, out var value5))
			{
				value5 = "1";
			}
			string tag = list[i].GetTag("AggregateWith", null);
			if (tag != null)
			{
				if (!dictionary.TryGetValue(tag, out var value6))
				{
					value6 = new PopulationGroup();
					value6.Name = "aggregate:" + tag;
					value6.Weight = 1u;
					value6.Style = "pickone";
					dictionary.Add(tag, value6);
					populationGroup.Items.Add(value6);
				}
				if (value6.Weight < num6)
				{
					value6.Weight = num6;
				}
				value6.Items.Add(new PopulationObject(list[i].Name, value4, num6, value5));
			}
			else
			{
				populationGroup.Items.Add(new PopulationObject(list[i].Name, value4, num6, value5));
			}
		}
		populationInfo.Items.Add(populationGroup);
		if (PopulationManager.Populations.ContainsKey(populationInfo.Name))
		{
			MetricsManager.LogWarning("Double entry during table fabrication for populationInfo: " + populationInfo.Name);
		}
		else
		{
			PopulationManager.Populations.Add(populationInfo.Name, populationInfo);
		}
	}

	public void FabricateDynamicPopulationTable(string tableName, List<GameObjectBlueprint> dynamicTableObjects)
	{
		Tiers.Clear();
		uint num = 1u;
		InitWeights();
		for (int i = 0; i <= 8; i++)
		{
			PopulationInfo populationInfo = new PopulationInfo();
			if (i == 0)
			{
				populationInfo.Name = tableName;
			}
			else
			{
				populationInfo.Name = tableName + ":Tier" + i;
			}
			PopulationGroup populationGroup = new PopulationGroup();
			populationGroup.Chance = "100";
			populationGroup.Number = "1";
			populationGroup.Style = "pickone";
			string key = tableName + ":Number";
			string key2 = tableName + ":Builder";
			string key3 = tableName + ":Weight";
			Dictionary<string, PopulationGroup> dictionary = new Dictionary<string, PopulationGroup>();
			for (int j = 0; j < dynamicTableObjects.Count; j++)
			{
				if (!EncountersAPI.IsEligibleForDynamicEncounters(dynamicTableObjects[j]))
				{
					continue;
				}
				int key4 = i - dynamicTableObjects[j].Tier;
				if (i == 0)
				{
					key4 = 0;
				}
				if (!TierDeltaWeights.TryGetValue(key4, out var value))
				{
					value = num;
				}
				if ((dynamicTableObjects[j].Tags.TryGetValue("Role", out var value2) || dynamicTableObjects[j].Props.TryGetValue("Role", out value2)) && RoleWeightMultipliers.TryGetValue(value2, out var value3))
				{
					value = (uint)Math.Ceiling((double)value * value3);
				}
				if (dynamicTableObjects[j].Tags.ContainsKey(key3))
				{
					try
					{
						double num2 = Convert.ToDouble(dynamicTableObjects[j].Tags[key3]);
						value = (uint)Math.Ceiling((double)value * num2);
					}
					catch (Exception x)
					{
						MetricsManager.LogException("Invalid table weight tag on: " + j, x);
					}
				}
				if (value == 0)
				{
					continue;
				}
				if (!dynamicTableObjects[j].Tags.TryGetValue(key, out var value4))
				{
					value4 = "1";
				}
				if (!dynamicTableObjects[j].Tags.TryGetValue(key2, out var value5))
				{
					value5 = "1";
				}
				string tag = dynamicTableObjects[j].GetTag("AggregateWith", null);
				if (tag != null)
				{
					if (!dictionary.TryGetValue(tag, out var value6))
					{
						value6 = new PopulationGroup();
						value6.Name = "aggregate:" + tag;
						value6.Weight = 1u;
						value6.Style = "pickone";
						dictionary.Add(tag, value6);
						populationGroup.Items.Add(value6);
					}
					if (value6.Weight < value)
					{
						value6.Weight = value;
					}
					value6.Items.Add(new PopulationObject(dynamicTableObjects[j].Name, value4, value, value5));
				}
				else
				{
					populationGroup.Items.Add(new PopulationObject(dynamicTableObjects[j].Name, value4, value, value5));
				}
			}
			populationInfo.Items.Add(populationGroup);
			if (PopulationManager.Populations.ContainsKey(populationInfo.Name))
			{
				MetricsManager.LogWarning("Double entry during table fabrication for populationInfo: " + populationInfo.Name);
			}
			else
			{
				PopulationManager.Populations.Add(populationInfo.Name, populationInfo);
			}
		}
	}

	public void FabricateMultitierDynamicPopulationTable(string tableName, IEnumerable<GameObjectBlueprint> dynamicTableObjects, int minTier, int maxTier)
	{
		Tiers.Clear();
		uint num = 1u;
		InitWeights();
		PopulationInfo populationInfo = new PopulationInfo();
		populationInfo.Name = tableName;
		PopulationGroup populationGroup = new PopulationGroup();
		populationGroup.Chance = "100";
		populationGroup.Number = "1";
		populationGroup.Style = "pickone";
		string key = tableName + ":Number";
		string key2 = tableName + ":Builder";
		string key3 = tableName + ":Weight";
		Dictionary<string, PopulationGroup> dictionary = new Dictionary<string, PopulationGroup>();
		foreach (GameObjectBlueprint dynamicTableObject in dynamicTableObjects)
		{
			if (!EncountersAPI.IsEligibleForDynamicEncounters(dynamicTableObject))
			{
				continue;
			}
			int num2 = 0;
			if (dynamicTableObject.Tier < minTier || dynamicTableObject.Tier > maxTier)
			{
				num2 = Math.Min(Math.Abs(minTier - dynamicTableObject.Tier), Math.Abs(minTier - dynamicTableObject.Tier));
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (!TierDeltaWeights.TryGetValue(num2, out var value))
			{
				value = num;
			}
			if ((dynamicTableObject.Tags.TryGetValue("Role", out var value2) || dynamicTableObject.Props.TryGetValue("Role", out value2)) && RoleWeightMultipliers.TryGetValue(value2, out var value3))
			{
				value = (uint)Math.Ceiling((double)value * value3);
			}
			if (dynamicTableObject.Tags.ContainsKey(key3))
			{
				try
				{
					double num3 = Convert.ToDouble(dynamicTableObject.Tags[key3]);
					value = (uint)Math.Ceiling((double)value * num3);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Invalid table weight tag on: " + dynamicTableObject, x);
				}
			}
			if (value == 0)
			{
				continue;
			}
			if (!dynamicTableObject.Tags.TryGetValue(key, out var value4))
			{
				value4 = "1";
			}
			if (!dynamicTableObject.Tags.TryGetValue(key2, out var value5))
			{
				value5 = "1";
			}
			string tag = dynamicTableObject.GetTag("AggregateWith", null);
			if (tag != null)
			{
				if (!dictionary.TryGetValue(tag, out var value6))
				{
					value6 = new PopulationGroup();
					value6.Name = "aggregate:" + tag;
					value6.Weight = 1u;
					value6.Style = "pickone";
					dictionary.Add(tag, value6);
					populationGroup.Items.Add(value6);
				}
				if (value6.Weight < value)
				{
					value6.Weight = value;
				}
				value6.Items.Add(new PopulationObject(dynamicTableObject.Name, value4, value, value5));
			}
			else
			{
				populationGroup.Items.Add(new PopulationObject(dynamicTableObject.Name, value4, value, value5));
			}
		}
		populationInfo.Items.Add(populationGroup);
		if (PopulationManager.Populations.ContainsKey(populationInfo.Name))
		{
			MetricsManager.LogWarning("Double entry during table fabrication for populationInfo: " + populationInfo.Name);
		}
		else
		{
			PopulationManager.Populations.Add(populationInfo.Name, populationInfo);
		}
	}

	public List<GameObjectBlueprint> GetBlueprintsWithTag(string Tag, bool ExcludeBase = true)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		foreach (GameObjectBlueprint blueprint in BlueprintList)
		{
			if (blueprint.HasTag(Tag) && (!ExcludeBase || !blueprint.IsBaseBlueprint()))
			{
				list.Add(blueprint);
			}
		}
		return list;
	}

	public List<GameObjectBlueprint> GetBlueprintsInheritingFrom(string Name, bool ExcludeBase = true)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		foreach (GameObjectBlueprint blueprint in BlueprintList)
		{
			if (blueprint.InheritsFrom(Name) && (!ExcludeBase || !blueprint.IsBaseBlueprint()))
			{
				list.Add(blueprint);
			}
		}
		return list;
	}

	public Dictionary<Type, (MethodInfo Load, List<GameObjectBlueprint> Blueprints)> GetBlueprintsWantingPreload()
	{
		List<Type> typesWithAttribute = ModManager.GetTypesWithAttribute(typeof(WantLoadBlueprintAttribute));
		Type[] types = new Type[1] { typeof(GameObjectBlueprint) };
		Dictionary<Type, (MethodInfo, List<GameObjectBlueprint>)> dictionary = new Dictionary<Type, (MethodInfo, List<GameObjectBlueprint>)>(typesWithAttribute.Count);
		foreach (Type item2 in typesWithAttribute)
		{
			MethodInfo method = item2.GetMethod("LoadBlueprint", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, types, null);
			if (method != null)
			{
				dictionary[item2] = (method, new List<GameObjectBlueprint>());
			}
		}
		foreach (KeyValuePair<Type, (MethodInfo, List<GameObjectBlueprint>)> item3 in dictionary)
		{
			item3.Deconstruct(out var key, out var value);
			(MethodInfo, List<GameObjectBlueprint>) tuple = value;
			Type type = key;
			List<GameObjectBlueprint> item = tuple.Item2;
			foreach (GameObjectBlueprint blueprint in BlueprintList)
			{
				foreach (KeyValuePair<string, GamePartBlueprint> part in blueprint.Parts)
				{
					if (part.Value.T == type)
					{
						item.Add(blueprint);
						break;
					}
				}
			}
		}
		return dictionary;
	}

	public void LoadBlueprints()
	{
		MutationFactory.CheckInit();
		ObjectBlueprintLoader objectBlueprintLoader = new ObjectBlueprintLoader();
		objectBlueprintLoader.LoadAllBlueprints();
		foreach (ObjectBlueprintLoader.ObjectBlueprintXMLData item in objectBlueprintLoader.BakedBlueprints())
		{
			GameObjectBlueprint gameObjectBlueprint = LoadBakedXML(item);
			Blueprints[item.Name] = gameObjectBlueprint;
			BlueprintList.Add(gameObjectBlueprint);
			if (!item.PreviousName.IsNullOrEmpty())
			{
				CompatManager.SetCompatEntry("blueprint", item.PreviousName, item.Name);
			}
		}
		foreach (GameObjectBlueprint blueprint in BlueprintList)
		{
			GameObjectBlueprint shallowParent = blueprint.ShallowParent;
			if (shallowParent != null)
			{
				shallowParent.hasChildren = true;
			}
		}
		if (!CompatManager.CompatEntries.TryGetValue("blueprint", out var Value))
		{
			return;
		}
		foreach (KeyValuePair<string, string> item2 in Value)
		{
			if (Blueprints.TryGetValue(item2.Value, out var value))
			{
				Blueprints.TryAdd(item2.Key, value);
			}
		}
	}

	public KeyValuePair<string, Dictionary<string, string>> ParseXTagNode(ObjectBlueprintLoader.ObjectBlueprintXMLChildNode node)
	{
		return new KeyValuePair<string, Dictionary<string, string>>(node.NodeName.Substring(4), new Dictionary<string, string>(node.Attributes));
	}

	public Statistic ParseStatNode(ObjectBlueprintLoader.ObjectBlueprintXMLChildNode node, GameObjectBlueprint blueprint = null)
	{
		Statistic statistic = new Statistic();
		statistic.Name = node.Name;
		try
		{
			if (node.Attributes.ContainsKey("Min"))
			{
				statistic.Min = Convert.ToInt32(node.Attributes["Min"]);
			}
			if (node.Attributes.ContainsKey("Max"))
			{
				statistic.Max = Convert.ToInt32(node.Attributes["Max"]);
			}
			if (node.Attributes.ContainsKey("Value"))
			{
				statistic.BaseValue = Convert.ToInt32(node.Attributes["Value"]);
			}
			if (node.Attributes.ContainsKey("Boost"))
			{
				statistic.Boost = Convert.ToInt32(node.Attributes["Boost"]);
			}
			if (node.Attributes.ContainsKey("sValue"))
			{
				statistic.sValue = node.Attributes["sValue"];
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Error parsing stat parameters for " + (blueprint?.Name ?? "unknown blueprint"), x);
		}
		return statistic;
	}

	public InventoryObject ParseInventoryObjectNode(ObjectBlueprintLoader.ObjectBlueprintXMLChildNode node)
	{
		bool.TryParse(node.GetAttribute("BoostModChance"), out var result);
		int.TryParse(node.GetAttribute("SetMods"), out var result2);
		string attribute = node.GetAttribute("AutoMod");
		string attribute2 = node.GetAttribute("Number");
		string text = "1";
		if (!attribute2.IsNullOrEmpty())
		{
			text = attribute2;
		}
		string attribute3 = node.GetAttribute("Chance");
		int result3 = 100;
		if (!attribute3.IsNullOrEmpty())
		{
			int.TryParse(attribute3, out result3);
		}
		bool.TryParse(node.GetAttribute("NoSell"), out var result4);
		bool.TryParse(node.GetAttribute("NoEquip"), out var result5);
		bool.TryParse(node.GetAttribute("NotReal"), out var result6);
		bool.TryParse(node.GetAttribute("Full"), out var result7);
		string attribute4 = node.GetAttribute("CellChance");
		int? cellChance = null;
		if (!attribute4.IsNullOrEmpty() && int.TryParse(attribute4, out var result8))
		{
			cellChance = result8;
		}
		string attribute5 = node.GetAttribute("CellFullChance");
		int? cellFullChance = null;
		if (!attribute5.IsNullOrEmpty() && int.TryParse(attribute5, out var result9))
		{
			cellFullChance = result9;
		}
		string attribute6 = node.GetAttribute("StringProperties");
		Dictionary<string, string> dictionary = null;
		if (!attribute6.IsNullOrEmpty())
		{
			string[] array = attribute6.Split(',');
			dictionary = new Dictionary<string, string>(array.Length);
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string[] array3 = array2[i].Split(':');
				dictionary.Add(array3[0], array3[1]);
			}
		}
		string attribute7 = node.GetAttribute("IntProperties");
		Dictionary<string, int> dictionary2 = null;
		if (!attribute7.IsNullOrEmpty())
		{
			string[] array4 = attribute7.Split(',');
			dictionary2 = new Dictionary<string, int>(array4.Length);
			string[] array2 = array4;
			for (int i = 0; i < array2.Length; i++)
			{
				string[] array5 = array2[i].Split(':');
				dictionary2.Add(array5[0], Convert.ToInt32(array5[1]));
			}
		}
		string attribute8 = node.GetAttribute("Blueprint");
		string attribute9 = node.GetAttribute("CellType");
		string number = text;
		int chance = result3;
		bool boostModChance = result;
		return new InventoryObject(attribute8, number, chance, result2, boostModChance, result5, result4, result6, result7, cellChance, cellFullChance, attribute9, attribute, dictionary, dictionary2);
	}

	public GameObjectBlueprint LoadBakedXML(ObjectBlueprintLoader.ObjectBlueprintXMLData node)
	{
		try
		{
			GameObjectBlueprint gameObjectBlueprint = new GameObjectBlueprint();
			gameObjectBlueprint.Name = node.Name;
			gameObjectBlueprint.Inherits = node.Inherits;
			Blueprints[node.Name] = gameObjectBlueprint;
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item in node.NamedNodes("part"))
			{
				GamePartBlueprint gamePartBlueprint = new GamePartBlueprint("XRL.World.Parts", item.Key);
				gamePartBlueprint.Name = item.Value.Name;
				gamePartBlueprint.Parameters = item.Value.Attributes;
				gameObjectBlueprint.UpdatePart(gamePartBlueprint.Name, gamePartBlueprint);
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item2 in node.NamedNodes("mutation"))
			{
				GamePartBlueprint gamePartBlueprint2 = new GamePartBlueprint("XRL.World.Parts.Mutation", item2.Key);
				gamePartBlueprint2.Name = item2.Value.Name;
				gamePartBlueprint2.Parameters = item2.Value.Attributes;
				gameObjectBlueprint.Mutations[gamePartBlueprint2.Name] = gamePartBlueprint2;
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item3 in node.NamedNodes("builder"))
			{
				GamePartBlueprint gamePartBlueprint3 = new GamePartBlueprint("XRL.World.ObjectBuilders", item3.Key);
				gamePartBlueprint3.Name = item3.Value.Name;
				gamePartBlueprint3.Parameters = item3.Value.Attributes;
				gameObjectBlueprint.Builders[gamePartBlueprint3.Name] = gamePartBlueprint3;
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item4 in node.NamedNodes("skill"))
			{
				GamePartBlueprint gamePartBlueprint4 = new GamePartBlueprint("XRL.World.Parts.Skill", item4.Key);
				gamePartBlueprint4.Name = item4.Value.Name;
				gamePartBlueprint4.Parameters = item4.Value.Attributes;
				gameObjectBlueprint.Skills[gamePartBlueprint4.Name] = gamePartBlueprint4;
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item5 in node.NamedNodes("stat"))
			{
				gameObjectBlueprint.UpdateStat(item5.Key, ParseStatNode(item5.Value, gameObjectBlueprint));
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item6 in node.NamedNodes("property"))
			{
				string attribute = item6.Value.GetAttribute("Value");
				if (attribute == null || (!attribute.Contains("{{{remove}}}") && !attribute.Contains("*delete")))
				{
					gameObjectBlueprint.Props[item6.Key] = attribute;
				}
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item7 in node.NamedNodes("intproperty"))
			{
				string attribute2 = item7.Value.GetAttribute("Value");
				if (attribute2 == null || (!attribute2.Contains("{{{remove}}}") && !attribute2.Contains("*delete")))
				{
					gameObjectBlueprint.IntProps[item7.Key] = Convert.ToInt32(attribute2);
				}
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item8 in node.NamedNodes("tag").Concat(node.NamedNodes("stag")))
			{
				string text = item8.Value.Name;
				if (!item8.Value.Attributes.TryGetValue("Value", out var value))
				{
					value = "";
				}
				if (item8.Value.NodeName == "stag")
				{
					text = "Semantic" + text;
					if (value == "")
					{
						value = "true";
					}
				}
				if (!value.Contains("{{{remove}}}") && !value.Contains("*delete"))
				{
					gameObjectBlueprint.Tags.Add(text, value);
				}
			}
			if (node.Children.ContainsKey("inventoryobject"))
			{
				if (node.UnnamedNodes("inventoryobject") == null)
				{
					MetricsManager.LogWarning("likely missing Blueprint attribute on inventoryobject " + node.ToString());
				}
				else
				{
					gameObjectBlueprint.Inventory = new List<InventoryObject>(node.Children["inventoryobject"].Unnamed.Count);
					foreach (ObjectBlueprintLoader.ObjectBlueprintXMLChildNode item9 in node.UnnamedNodes("inventoryobject"))
					{
						gameObjectBlueprint.Inventory.Add(ParseInventoryObjectNode(item9));
					}
				}
			}
			foreach (string key in node.Children.Keys)
			{
				if (!key.StartsWith("xtag"))
				{
					continue;
				}
				if (gameObjectBlueprint.xTags == null)
				{
					gameObjectBlueprint.xTags = new Dictionary<string, Dictionary<string, string>>();
				}
				foreach (ObjectBlueprintLoader.ObjectBlueprintXMLChildNode item10 in node.UnnamedNodes(key))
				{
					KeyValuePair<string, Dictionary<string, string>> keyValuePair = ParseXTagNode(item10);
					gameObjectBlueprint.xTags.Add(keyValuePair.Key, keyValuePair.Value);
				}
			}
			return gameObjectBlueprint;
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Error while loading baked blueprint " + node.Name, x);
		}
		return null;
	}

	public GameObject CreateObject(string ObjectBlueprint)
	{
		return CreateObject(ObjectBlueprint, 0, 0, null, null, null, null, null);
	}

	public GameObject CreateObject(string ObjectBlueprint, int BonusModChance = 0, int SetModNumber = 0, string AutoMod = null, Action<GameObject> BeforeObjectCreated = null, Action<GameObject> AfterObjectCreated = null, string Context = null, List<GameObject> ProvideInventory = null)
	{
		try
		{
			if (ObjectBlueprint == null)
			{
				return null;
			}
			ObjectsCreated++;
			GameObjectBlueprint blueprint = GetBlueprint(ObjectBlueprint);
			return CreateObject(blueprint, BonusModChance, SetModNumber, AutoMod, BeforeObjectCreated, AfterObjectCreated, Context, ProvideInventory);
		}
		catch (Exception ex)
		{
			MetricsManager.LogException("Failed creating:" + ObjectBlueprint, ex);
			GameObject gameObject = CreateObject("PhysicalObject");
			gameObject.Render.DisplayName = "[invalid blueprint:" + ObjectBlueprint + "]";
			gameObject.GetPart<Description>().Short = "Failed building: " + ObjectBlueprint + "\n\n" + ex.ToString();
			return gameObject;
		}
	}

	public GameObject CreateObject(string ObjectBlueprint, Action<GameObject> BeforeObjectCreated)
	{
		try
		{
			if (ObjectBlueprint == null)
			{
				return null;
			}
			ObjectsCreated++;
			GameObjectBlueprint blueprint = GetBlueprint(ObjectBlueprint);
			return CreateObject(blueprint, 0, 0, null, BeforeObjectCreated);
		}
		catch (Exception ex)
		{
			MetricsManager.LogException("Failed creating:" + ObjectBlueprint, ex);
			GameObject gameObject = CreateObject("PhysicalObject");
			gameObject.Render.DisplayName = "[invalid blueprint:" + ObjectBlueprint + "]";
			gameObject.GetPart<Description>().Short = "Failed building: " + ObjectBlueprint + "\n\n" + ex.ToString();
			return gameObject;
		}
	}

	private void CallLoadBlueprint()
	{
		int num = 0;
		object[] array = new object[1];
		foreach (KeyValuePair<Type, (MethodInfo, List<GameObjectBlueprint>)> item in GetBlueprintsWantingPreload())
		{
			item.Deconstruct(out var key, out var value);
			(MethodInfo, List<GameObjectBlueprint>) tuple = value;
			Type type = key;
			var (methodInfo, _) = tuple;
			foreach (GameObjectBlueprint item2 in tuple.Item2)
			{
				num++;
				if (num % 20 == 0)
				{
					Event.ResetPool();
				}
				try
				{
					array[0] = item2;
					methodInfo.Invoke(null, array);
				}
				catch (Exception x)
				{
					MetricsManager.LogError("Exception invoking '" + type.Name + "." + methodInfo.Name + "' with blueprint '" + item2.Name + "'", x);
				}
			}
		}
	}

	private static void ProcessAsInventory(GameObject Object, InventoryObject InventoryObject, Inventory TargetInventory)
	{
		if (InventoryObject != null)
		{
			if (InventoryObject.NoEquip)
			{
				Object.SetIntProperty("NoEquip", 1);
			}
			if (InventoryObject.NoSell)
			{
				Object.SetIntProperty("WontSell", 1);
			}
			if (InventoryObject.NotReal)
			{
				Object.Physics.IsReal = false;
			}
			if (InventoryObject.Full)
			{
				LiquidVolume liquidVolume = Object.LiquidVolume;
				if (liquidVolume != null && liquidVolume.MaxVolume > 0 && liquidVolume.ComponentLiquids.Count > 0)
				{
					liquidVolume.Volume = liquidVolume.MaxVolume;
					liquidVolume.FlushWeightCaches();
				}
			}
		}
		if (TargetInventory != null)
		{
			if (TargetInventory.ParentObject == Object)
			{
				MetricsManager.LogError("target inventory for " + Object.DebugName + " was self");
				return;
			}
			eCommandTakeObject.SetParameter("Object", Object);
			TargetInventory.FireEvent(eCommandTakeObject);
		}
	}

	public static void ProcessSpecification(string Blueprint, Action<GameObject> AfterObjectCreated = null, InventoryObject InventoryObject = null, int Count = 1, int BonusModChance = 0, int SetModNumber = 0, string AutoMod = null, string Context = null, Action<GameObject> BeforeObjectCreated = null, GameObject OwningObject = null, Inventory TargetInventory = null, List<GameObject> ProvideInventory = null)
	{
		try
		{
			if (TargetInventory == null && OwningObject != null)
			{
				TargetInventory = OwningObject.Inventory;
			}
			if (Blueprint.Length > 0 && Blueprint.StartsWith("$CALLBLUEPRINTMETHOD:", StringComparison.CurrentCultureIgnoreCase))
			{
				ProcessSpecification(PopulationManager.resolveCallBlueprintSlug(Blueprint), AfterObjectCreated, InventoryObject, Count, BonusModChance, SetModNumber, AutoMod, Context, BeforeObjectCreated, OwningObject, TargetInventory, ProvideInventory);
				return;
			}
			if (Blueprint.Length > 0 && Blueprint.StartsWith("$CALLOBJECTMETHOD:", StringComparison.CurrentCultureIgnoreCase))
			{
				for (int i = 0; i < Count; i++)
				{
					foreach (GameObject item in PopulationManager.resolveCallObjectSlug(Blueprint))
					{
						AfterObjectCreated?.Invoke(item);
						ProcessAsInventory(item, InventoryObject, TargetInventory);
					}
				}
				return;
			}
			if (Blueprint.Length > 0 && Blueprint[0] == '#')
			{
				List<string> list = new List<string>(Blueprint.Substring(1).Split(','));
				for (int j = 0; j < Count; j++)
				{
					if (list.Count <= 0)
					{
						break;
					}
					string randomElement = list.GetRandomElement();
					list.Remove(randomElement);
					ProcessSpecification(randomElement, AfterObjectCreated, InventoryObject, 1, BonusModChance, SetModNumber, AutoMod, Context, BeforeObjectCreated, OwningObject, TargetInventory, ProvideInventory);
				}
				return;
			}
			if (Blueprint.Length > 0 && (Blueprint[0] == '@' || Blueprint[0] == '*'))
			{
				string text = Blueprint.Substring(1);
				if (text.Contains("{zonetier}"))
				{
					text = text.Replace("{zonetier}", ZoneManager.zoneGenerationContextTier.ToString());
				}
				populationContext.Clear();
				populationContext.Add("zonetier", ZoneManager.zoneGenerationContextTier.ToString());
				populationContext.Add("zonetier+1", (ZoneManager.zoneGenerationContextTier + 1).ToString());
				if (OwningObject != null)
				{
					populationContext.Add("ownertier", OwningObject.GetTier().ToString());
					populationContext.Add("ownertechtier", OwningObject.GetTechTier().ToString());
				}
				for (int k = 0; k < Count; k++)
				{
					foreach (PopulationResult item2 in PopulationManager.Generate(text, populationContext))
					{
						for (int l = 0; l < item2.Number; l++)
						{
							InventoryObject inventoryObject = InventoryObject;
							GameObject gameObject = null;
							if (ProvideInventory != null)
							{
								foreach (GameObject item3 in ProvideInventory)
								{
									if (item3.Blueprint == item2.Blueprint)
									{
										gameObject = item3;
										break;
									}
								}
								if (gameObject != null)
								{
									inventoryObject = null;
									if (gameObject.Count == 1)
									{
										ProvideInventory.Remove(gameObject);
									}
									else
									{
										gameObject = gameObject.RemoveOne();
									}
								}
							}
							if (gameObject == null)
							{
								GameObjectFactory factory = Factory;
								string blueprint = item2.Blueprint;
								string context = Context;
								gameObject = factory.CreateObject(blueprint, BonusModChance, SetModNumber, AutoMod, BeforeObjectCreated, null, context);
							}
							if (l == 0 && item2.Number > 1 && item2.Hint != "NoStack" && gameObject.CanGenerateStacked())
							{
								Stacker stacker = gameObject.Stacker;
								if (stacker != null && stacker != null && stacker.StackCount == 1)
								{
									stacker.StackCount = item2.Number;
									l = item2.Number;
								}
							}
							AfterObjectCreated?.Invoke(gameObject);
							ProcessAsInventory(gameObject, inventoryObject, TargetInventory);
						}
					}
				}
				return;
			}
			if (ProvideInventory != null)
			{
				for (int m = 0; m < Count; m++)
				{
					InventoryObject inventoryObject2 = InventoryObject;
					GameObject gameObject2 = null;
					foreach (GameObject item4 in ProvideInventory)
					{
						if (item4.Blueprint == Blueprint)
						{
							gameObject2 = item4;
							break;
						}
					}
					if (gameObject2 != null)
					{
						inventoryObject2 = null;
						if (gameObject2.Count == 1)
						{
							ProvideInventory.Remove(gameObject2);
						}
						else
						{
							gameObject2 = gameObject2.RemoveOne();
						}
					}
					if (gameObject2 == null)
					{
						GameObjectFactory factory2 = Factory;
						string context = Context;
						gameObject2 = factory2.CreateObject(Blueprint, BonusModChance, 0, null, BeforeObjectCreated, null, context);
						if (m == 0 && Count > 1 && gameObject2.CanGenerateStacked())
						{
							Stacker stacker2 = gameObject2.Stacker;
							if (stacker2 != null && stacker2.StackCount == 1)
							{
								stacker2.StackCount = Count;
								m = Count;
							}
						}
					}
					AfterObjectCreated?.Invoke(gameObject2);
					ProcessAsInventory(gameObject2, inventoryObject2, TargetInventory);
				}
				return;
			}
			for (int n = 0; n < Count; n++)
			{
				GameObjectFactory factory3 = Factory;
				string context = Context;
				GameObject gameObject3 = factory3.CreateObject(Blueprint, BonusModChance, 0, null, BeforeObjectCreated, null, context);
				if (n == 0 && Count > 1 && gameObject3.CanGenerateStacked() && gameObject3.TryGetPart<Stacker>(out var Part) && Part.StackCount == 1)
				{
					Part.StackCount = Count;
					n = Count;
				}
				AfterObjectCreated?.Invoke(gameObject3);
				ProcessAsInventory(gameObject3, InventoryObject, TargetInventory);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("GameObjectFactory::ProcessSpecification", x);
		}
	}

	public GameObject CreateObject(GameObjectBlueprint Blueprint, int BonusModChance = 0, int SetModNumber = 0, string AutoMod = null, Action<GameObject> BeforeObjectCreated = null, Action<GameObject> AfterObjectCreated = null, string Context = null, List<GameObject> ProvideInventory = null)
	{
		if (Blueprint == null)
		{
			return null;
		}
		GameObject gameObject = GameObject.Get();
		gameObject.SetBlueprint(Blueprint);
		if (Blueprint.Tags.TryGetValue("CreateSubstituteBlueprint", out var value) && Blueprints.TryGetValue(value, out var value2))
		{
			gameObject.SetBlueprint(value2);
		}
		bool flag = Context == "Initialization";
		if (flag)
		{
			gameObject.ID = "init:" + Blueprint.Name;
		}
		gameObject.Property.Rebase(Blueprint.Props);
		gameObject.IntProperty.Rebase(Blueprint.IntProps);
		for (GameObjectBlueprint gameObjectBlueprint = Blueprint; gameObjectBlueprint != null; gameObjectBlueprint = gameObjectBlueprint.ShallowParent)
		{
			if (gameObjectBlueprint.Stats == null)
			{
				XRLCore.LogError("Null stats on blueprint for " + Blueprint.Name);
			}
			else if (gameObject.Statistics == null)
			{
				XRLCore.LogError("Null statistics on new object for " + Blueprint.Name);
			}
			else
			{
				foreach (KeyValuePair<string, Statistic> stat in gameObjectBlueprint.Stats)
				{
					if (!gameObject.Statistics.ContainsKey(stat.Key))
					{
						gameObject.Statistics.Add(stat.Key, new Statistic(stat.Value)
						{
							Owner = gameObject
						});
					}
				}
			}
		}
		gameObject.FinalizeStats();
		foreach (GamePartBlueprint value5 in Blueprint.allparts.Values)
		{
			if (Stat.Random(1, value5.ChanceOneIn) == 1)
			{
				if (value5.T == null)
				{
					XRLCore.LogError("Unknown part " + value5.Name + "!");
					return null;
				}
				IPart part = value5.Reflector?.GetInstance() ?? (Activator.CreateInstance(value5.T) as IPart);
				part.ParentObject = gameObject;
				value5.InitializePartInstance(part);
				gameObject.AddPart(part, !flag, Creation: true);
				if (value5.TryGetParameter<string>("Builder", out var Value))
				{
					(Activator.CreateInstance(ModManager.ResolveType("XRL.World.PartBuilders", Value)) as IPartBuilder).BuildPart(part, Context);
				}
			}
		}
		if (Blueprint.Mutations != null)
		{
			foreach (GamePartBlueprint value6 in Blueprint.Mutations.Values)
			{
				if (Stat.Random(1, value6.ChanceOneIn) != 1)
				{
					continue;
				}
				string text = "XRL.World.Parts.Mutation." + value6.Name;
				Type type = ModManager.ResolveType(text);
				if (type == null)
				{
					MetricsManager.LogError("Unknown mutation " + text);
					return null;
				}
				if (!((value6.Reflector?.GetNewInstance() ?? Activator.CreateInstance(type)) is BaseMutation baseMutation))
				{
					MetricsManager.LogError("Mutation " + text + " is not a BaseMutation");
					continue;
				}
				value6.InitializePartInstance(baseMutation);
				if (value6.TryGetParameter<string>("Builder", out var Value2))
				{
					(Activator.CreateInstance(ModManager.ResolveType("XRL.World.PartBuilders." + Value2)) as IPartBuilder).BuildPart(baseMutation, Context);
				}
				if (baseMutation.CapOverride == -1)
				{
					baseMutation.CapOverride = baseMutation.Level;
				}
				gameObject.RequirePart<Mutations>().AddMutation(baseMutation, baseMutation.Level);
			}
		}
		if (Blueprint.Tags != null)
		{
			if (Blueprint.Tags.TryGetValue("MutationPopulation", out var value3) && !value3.IsNullOrEmpty())
			{
				gameObject.MutateFromPopulationTable(value3, ZoneManager.zoneGenerationContextTier);
			}
			if (Blueprint.Tags.TryGetValue("Modded", out var value4))
			{
				if (!value4.IsNullOrEmpty())
				{
					int num = Convert.ToInt32(value4);
					if (SetModNumber < num)
					{
						SetModNumber = num;
					}
				}
				else if (SetModNumber < 1)
				{
					SetModNumber = 1;
				}
			}
		}
		if (Blueprint.Skills != null)
		{
			foreach (GamePartBlueprint value7 in Blueprint.Skills.Values)
			{
				if (Stat.Random(1, value7.ChanceOneIn) == 1)
				{
					string text2 = "XRL.World.Parts.Skill." + value7.Name;
					Type type2 = ModManager.ResolveType(text2);
					if (type2 == null)
					{
						MetricsManager.LogError("Unknown skill " + text2);
						return null;
					}
					if (!((value7.Reflector?.GetNewInstance() ?? Activator.CreateInstance(type2)) is BaseSkill baseSkill))
					{
						MetricsManager.LogError("Skill " + text2 + " is not a BaseSkill");
						return null;
					}
					value7.InitializePartInstance(baseSkill);
					if (value7.TryGetParameter<string>("Builder", out var Value3))
					{
						(Activator.CreateInstance(ModManager.ResolveType("XRL.World.PartBuilders." + Value3)) as IPartBuilder).BuildPart(baseSkill, Context);
					}
					gameObject.RequirePart<XRL.World.Parts.Skills>().AddSkill(baseSkill);
				}
			}
		}
		if (Blueprint.Inventory != null)
		{
			foreach (InventoryObject InventoryObject in Blueprint.Inventory)
			{
				if (!InventoryObject.Chance.in100())
				{
					continue;
				}
				int bonusModChance = (InventoryObject.BoostModChance ? 30 : 0);
				Action<GameObject> beforeObjectCreated = null;
				if (InventoryObject.NeedsToPreconfigureObject())
				{
					beforeObjectCreated = (GameObject GO) =>
					{
						InventoryObject.PreconfigureObject(GO);
					};
				}
				int count = InventoryObject.Number.RollCached();
				ProcessSpecification(InventoryObject.Blueprint, null, InventoryObject, count, bonusModChance, InventoryObject.SetMods, InventoryObject.AutoMod, Context, beforeObjectCreated, gameObject, gameObject.Inventory, ProvideInventory);
			}
		}
		string tag = gameObject.GetTag("InventoryPopulationTable");
		if (tag != null)
		{
			gameObject.EquipFromPopulationTable(tag, ZoneManager.zoneGenerationContextTier, null, Context);
			gameObject.FireEvent("InventoryPopulated");
		}
		ApplyBuilders(gameObject, Blueprint, Context);
		if (!AutoMod.IsNullOrEmpty())
		{
			if (AutoMod.Contains(","))
			{
				foreach (string item in AutoMod.CachedCommaExpansion())
				{
					gameObject.ApplyModification(item);
				}
			}
			else
			{
				gameObject.ApplyModification(AutoMod);
			}
		}
		ModificationFactory.ApplyModifications(gameObject, Blueprint, BonusModChance, SetModNumber, Context);
		BeforeObjectCreated?.Invoke(gameObject);
		GameObject ReplacementObject = null;
		BeforeObjectCreatedEvent.Process(gameObject, Context, ref ReplacementObject);
		ObjectCreatedEvent.Process(gameObject, Context, ref ReplacementObject);
		AfterObjectCreatedEvent.Process(gameObject, Context, ref ReplacementObject);
		if (ReplacementObject != null)
		{
			gameObject = ReplacementObject;
		}
		AfterObjectCreated?.Invoke(gameObject);
		return gameObject;
	}

	public GameObject CreateSampleObject(GameObjectBlueprint Blueprint, Action<GameObject> BeforeObjectCreated = null)
	{
		return CreateObject(Blueprint, -9999, 0, null, BeforeObjectCreated, null, "Sample");
	}

	public GameObject CreateSampleObject(string Blueprint, Action<GameObject> BeforeObjectCreated = null)
	{
		return CreateObject(Blueprint, -9999, 0, null, BeforeObjectCreated, null, "Sample");
	}

	public GameObject CreateUnmodifiedObject(GameObjectBlueprint Blueprint, string Context = null, Action<GameObject> BeforeObjectCreated = null)
	{
		return CreateObject(Blueprint, -9999, 0, null, BeforeObjectCreated, null, Context);
	}

	public GameObject CreateUnmodifiedObject(string Blueprint, string Context = null, Action<GameObject> BeforeObjectCreated = null)
	{
		return CreateObject(Blueprint, -9999, 0, null, BeforeObjectCreated, null, Context);
	}

	public static void ApplyBuilder(GameObject NewObject, string Builder)
	{
		ApplyBuilder(NewObject, new GamePartBlueprint(Builder));
	}

	public static void ApplyBuilder(GameObject NewObject, GamePartBlueprint Builder, string Context = null)
	{
		Type t = Builder.T;
		if (t == null)
		{
			MetricsManager.LogError("Unknown builder " + Builder.Name);
			return;
		}
		if (!ObjectBuilders.TryGetValue(t, out var value))
		{
			value = Activator.CreateInstance(t) as IObjectBuilder;
			if (value == null)
			{
				MetricsManager.LogError("Builder '" + Builder.Name + "' is not an instance of IObjectBuilder");
				return;
			}
			ObjectBuilders[t] = value;
		}
		value.Initialize();
		Builder.InitializePartInstance(value);
		value.Apply(NewObject, Context);
	}

	public static void ApplyBuilders(GameObject NewObject, GameObjectBlueprint Blueprint, string Context = null)
	{
		if (Blueprint.Builders.IsNullOrEmpty())
		{
			return;
		}
		foreach (var (_, gamePartBlueprint2) in Blueprint.Builders)
		{
			if (Stat.Random(1, gamePartBlueprint2.ChanceOneIn) == 1)
			{
				ApplyBuilder(NewObject, gamePartBlueprint2, Context);
			}
		}
	}

	public GameObjectBlueprint GetBlueprint(string Name)
	{
		if (Name == null)
		{
			MetricsManager.LogError("called with null Name");
			return null;
		}
		if (Blueprints.TryGetValue(Name, out var value))
		{
			return value;
		}
		MetricsManager.LogError("Unknown blueprint (tell support@freeholdentertainment.com the following text): " + Name);
		return null;
	}

	public GameObjectBlueprint GetBlueprintIfExists(string Name)
	{
		if (Name == null || !Blueprints.TryGetValue(Name, out var value))
		{
			return null;
		}
		return value;
	}

	public bool HasBlueprint(string Name)
	{
		return Blueprints.ContainsKey(Name);
	}

	/// <summary>
	/// Not intended for actual use by game objects, but makes finding blueprints via wish easier
	/// </summary>
	public GameObjectBlueprint GetBlueprintIgnoringCase(string name)
	{
		if (name == null)
		{
			return null;
		}
		foreach (string key in Blueprints.Keys)
		{
			if (string.Compare(key, name, ignoreCase: true) == 0)
			{
				return Blueprints[key];
			}
		}
		return null;
	}

	[WishCommand(null, null, Command = "bpxml")]
	public static void HandleBlueprintXML(string bpname)
	{
		GameObjectBlueprint blueprintIgnoringCase = Factory.GetBlueprintIgnoringCase(bpname);
		if (blueprintIgnoringCase == null)
		{
			Popup.Show("No blueprint named \"" + bpname + "\" found.");
		}
		else
		{
			Popup.Show(blueprintIgnoringCase.BlueprintXML().Replace("&", "&&").Replace("^", "^^"));
		}
	}

	[WishCommand(null, null, Command = "partpoolcount")]
	public static void PartPoolWish()
	{
		List<(Type, IPartPool)> list = new List<(Type, IPartPool)>();
		foreach (KeyValuePair<Type, GamePartBlueprint.PartReflectionCache> item in GamePartBlueprint.PartReflectionCache.CacheByType)
		{
			if (item.Value.Pool != null)
			{
				list.Add((item.Key, item.Value.Pool));
			}
		}
		list.Sort(((Type, IPartPool) a, (Type, IPartPool) b) => b.Item2.Capacity.CompareTo(a.Item2.Capacity));
		StringBuilder sB = Event.NewStringBuilder();
		foreach (var item2 in list)
		{
			sB.Compound(item2.Item1.Name, '\n').Append(": ").Append(item2.Item2.Count)
				.Append('/')
				.Append(item2.Item2.Capacity);
		}
		Popup.Show(Event.FinalizeString(sB));
	}
}
