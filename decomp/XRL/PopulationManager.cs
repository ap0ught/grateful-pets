using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Qud.API;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.Capabilities;

namespace XRL;

[HasModSensitiveStaticCache]
[HasWishCommand]
public class PopulationManager
{
	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, PopulationInfo> _Populations = null;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, MethodInfo> _PopulationCalls = null;

	private static string[] zonetierSplitter = null;

	private static Dictionary<string, Action<XmlDataHelper>> Nodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "populations", HandleNodes },
		{ "population", HandlePopulationNode }
	};

	private static PopulationInfo CurrentReadingPopulation;

	private static Dictionary<string, Action<XmlDataHelper>> PopulationSubNodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "object", HandleObjectNode },
		{ "table", HandleTableNode },
		{ "group", HandleGroupNode }
	};

	private static Dictionary<string, Action<XmlDataHelper>> GroupSubNodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "object", HandleGroupObjectNode },
		{ "table", HandleGroupTableNode },
		{ "group", HandleGroupGroupNode }
	};

	private static PopulationGroup CurrentReadingGroup;

	public static Dictionary<string, PopulationInfo> Populations
	{
		get
		{
			CheckInit();
			return _Populations;
		}
	}

	public static Dictionary<string, MethodInfo> PopulationCalls
	{
		get
		{
			if (_PopulationCalls == null)
			{
				_PopulationCalls = new Dictionary<string, MethodInfo>();
			}
			return _PopulationCalls;
		}
	}

	public static string resolveCallBlueprintSlug(string slug)
	{
		if (slug.StartsWith("$CALLBLUEPRINTMETHOD:", StringComparison.InvariantCultureIgnoreCase))
		{
			MethodInfo value = null;
			if (!PopulationCalls.TryGetValue(slug, out value))
			{
				string text = slug.Substring("$CALLBLUEPRINTMETHOD:".Length);
				int num = text.LastIndexOf(".");
				string typeID = text.Substring(0, num);
				string name = text.Substring(num + 1, text.Length - num - 1);
				PopulationCalls.Add(slug, null);
				Type type = ModManager.ResolveType(typeID);
				if (type != null)
				{
					value = type.GetMethod(name);
					PopulationCalls[slug] = value;
				}
			}
			if (value != null)
			{
				if (value.ReturnType == typeof(string))
				{
					int num2 = value.GetParameters().Length;
					return (string)value.Invoke(null, (num2 == 0) ? null : new object[num2]);
				}
				MetricsManager.LogError("Can't resolve resturn value to a string: " + slug);
			}
			else
			{
				MetricsManager.LogError("Undefined population call: " + slug);
			}
		}
		else
		{
			MetricsManager.LogError("Trying to resolve non-call slug: " + slug);
		}
		return null;
	}

	public static List<XRL.World.GameObject> resolveCallObjectSlug(string slug)
	{
		List<XRL.World.GameObject> list = XRL.World.Event.NewGameObjectList();
		if (slug.StartsWith("$CALLOBJECTMETHOD:", StringComparison.InvariantCultureIgnoreCase))
		{
			MethodInfo value = null;
			if (!PopulationCalls.TryGetValue(slug, out value))
			{
				string text = slug.Substring("$CALLOBJECTMETHOD:".Length);
				int num = text.LastIndexOf(".");
				string typeID = text.Substring(0, num);
				string name = text.Substring(num + 1, text.Length - num - 1);
				PopulationCalls.Add(slug, null);
				Type type = ModManager.ResolveType(typeID);
				if (type != null)
				{
					value = type.GetMethod(name);
					PopulationCalls[slug] = value;
				}
			}
			if (value != null)
			{
				if (value.ReturnType == typeof(XRL.World.GameObject))
				{
					int num2 = value.GetParameters().Length;
					list.Add((XRL.World.GameObject)value.Invoke(null, (num2 == 0) ? null : new object[num2]));
				}
				else if ((IEnumerable<XRL.World.GameObject>)value.ReturnType != null)
				{
					int num3 = value.GetParameters().Length;
					list.AddRange((IEnumerable<XRL.World.GameObject>)value.Invoke(null, (num3 == 0) ? null : new object[num3]));
				}
				else
				{
					MetricsManager.LogError("Can't resolve resturn value to a GameObject or GameObject collection: " + slug);
				}
			}
			else
			{
				MetricsManager.LogError("Undefined population call: " + slug);
			}
		}
		else
		{
			MetricsManager.LogError("Trying to resolve non-call slug: " + slug);
		}
		return list;
	}

	public static void CheckInit()
	{
		if (_Populations == null)
		{
			Loading.LoadTask("Loading PopulationTables.xml", LoadFiles);
		}
	}

	public static List<string> ResultToStringList(List<PopulationResult> Result)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < Result.Count; i++)
		{
			for (int j = 0; j < Result[i].Number; j++)
			{
				list.Add(Result[i].Blueprint);
			}
		}
		return list;
	}

	public static bool TableExists(string Name)
	{
		RequireTable(Name);
		return _Populations.ContainsKey(Name);
	}

	public static void RequireTable(string Name)
	{
		CheckInit();
		if (_Populations.ContainsKey(Name) || !Name.Contains(':', StringComparison.Ordinal))
		{
			return;
		}
		if (Name.StartsWith("DynamicObjectsTable:", StringComparison.Ordinal))
		{
			string[] array = Name.Split(':');
			if (array.Length > 2 && array[2].StartsWith("Tier"))
			{
				if (ResolveTier(array[2], out var low, out var high))
				{
					string targetTag = "DynamicObjectsTable:" + array[1];
					GameObjectFactory.Factory.FabricateMultitierDynamicPopulationTable(Name, GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint b) => EncountersAPI.IsEligibleForDynamicEncounters(b) && b.HasTag(targetTag)), low, high);
				}
			}
			else
			{
				GameObjectFactory.Factory.FabricateDynamicObjectsTable(Name);
			}
		}
		else if (Name.StartsWith("StaticObjectsTable:", StringComparison.Ordinal))
		{
			GameObjectFactory.Factory.FabricateStaticObjectsTable(Name);
		}
		else if (Name.StartsWith("DynamicSemanticTable:", StringComparison.Ordinal))
		{
			GameObjectFactory.Factory.FabricateDynamicSemanticTable(Name);
		}
		else if (Name.StartsWith("DynamicArtifactsTable:", StringComparison.Ordinal))
		{
			GameObjectFactory.Factory.FabricateDynamicArtifactsTable();
		}
		else if (Name.StartsWith("DynamicHasPartTable:", StringComparison.Ordinal))
		{
			string[] parts = Name.Split(':');
			if (parts.Length > 2 && parts[2].StartsWith("Tier") && ResolveTier(parts[2], out var low2, out var high2))
			{
				GameObjectFactory.Factory.FabricateMultitierDynamicPopulationTable(Name, GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint b) => EncountersAPI.IsEligibleForDynamicEncounters(b) && b.HasPart(parts[1])), low2, high2);
			}
			if (!_Populations.ContainsKey(Name))
			{
				string baseName = Name.Split(':')[1];
				GameObjectFactory.Factory.FabricateDynamicHasPartTable(baseName);
			}
		}
		else
		{
			if (!Name.StartsWith("DynamicInheritsTable:", StringComparison.Ordinal))
			{
				return;
			}
			string[] parts2 = Name.Split(':');
			if (parts2.Length > 2 && parts2[2].StartsWith("Tier") && ResolveTier(parts2[2], out var low3, out var high3))
			{
				GameObjectFactory.Factory.FabricateMultitierDynamicPopulationTable(Name, GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint b) => EncountersAPI.IsEligibleForDynamicEncounters(b) && b.DescendsFrom(parts2[1])), low3, high3);
			}
			if (!_Populations.ContainsKey(Name))
			{
				string baseName2 = Name.Split(':')[1];
				GameObjectFactory.Factory.FabricateDynamicInheritsTable(baseName2);
			}
		}
	}

	public static bool ResolveTier(string origSpec, out int low, out int high)
	{
		low = 0;
		high = 0;
		if (!origSpec.StartsWith("Tier"))
		{
			Debug.LogError("Tier specification does not start with Tier: " + origSpec);
			return false;
		}
		string text = origSpec.Substring(4);
		string text2 = null;
		while (text.Contains("{zonetier"))
		{
			if (text == text2)
			{
				Debug.LogError("Stalled parsing " + origSpec + " at " + text);
				return false;
			}
			text2 = text;
			if (text == "{zonetier}")
			{
				low = (high = ZoneManager.zoneGenerationContextTier);
				return true;
			}
			if (text == "{zonetier+1}")
			{
				low = (high = Tier.Constrain(ZoneManager.zoneGenerationContextTier + 1));
				return true;
			}
			if (zonetierSplitter == null)
			{
				zonetierSplitter = new string[1] { "{zonetier" };
			}
			string[] array = text.Split(zonetierSplitter, StringSplitOptions.None);
			if (array.Length > 1 || array[0] != "" || (array.Length > 1 && array[1].Length > 3))
			{
				StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
				stringBuilder.Append(array[0]);
				for (int i = 1; i < array.Length; i++)
				{
					int num = array[i].IndexOf('}');
					if (num == 0)
					{
						stringBuilder.Append(ZoneManager.zoneGenerationContextTier).Append(array[i].Substring(1));
						continue;
					}
					if (num > 0 && array[i].StartsWith("+"))
					{
						string text3 = array[i].Substring(1, num - 1);
						try
						{
							int num2 = Convert.ToInt32(text3);
							stringBuilder.Append(Tier.Constrain(ZoneManager.zoneGenerationContextTier + num2)).Append(array[i].Substring(num + 1));
						}
						catch (Exception x)
						{
							MetricsManager.LogError("Bad zone tier offset in " + origSpec + ": " + text3, x);
							return false;
						}
						continue;
					}
					if (num > 0 && array[i].StartsWith("-"))
					{
						string text4 = array[i].Substring(1, num - 1);
						try
						{
							int num3 = Convert.ToInt32(text4);
							stringBuilder.Append(Tier.Constrain(ZoneManager.zoneGenerationContextTier - num3)).Append(array[i].Substring(num + 1));
						}
						catch (Exception x2)
						{
							MetricsManager.LogError("Bad zone tier offset in " + origSpec + ": " + text4, x2);
							return false;
						}
						continue;
					}
					MetricsManager.LogError("Bad zone tier offset in " + origSpec + " at " + array[i]);
					return false;
				}
				text = stringBuilder.ToString();
				continue;
			}
			int num4 = array[1].IndexOf('}');
			if (num4 != array[1].Length - 1)
			{
				MetricsManager.LogError("Internal inconsistency parsing " + origSpec + " at " + array[1]);
				return false;
			}
			if (num4 == 0)
			{
				MetricsManager.LogWarning("This case for " + origSpec + " should be unreachable");
				low = (high = ZoneManager.zoneGenerationContextTier);
				return true;
			}
			if (num4 > 0 && array[1].StartsWith("+"))
			{
				string text5 = array[1].Substring(1, num4 - 1);
				try
				{
					int num5 = Convert.ToInt32(text5);
					low = (high = Tier.Constrain(ZoneManager.zoneGenerationContextTier + num5));
					return true;
				}
				catch (Exception x3)
				{
					MetricsManager.LogError("Bad zone tier offset in " + origSpec + ": " + text5, x3);
					return false;
				}
			}
			if (num4 > 0 && array[1].StartsWith("-"))
			{
				string text6 = array[1].Substring(1, num4 - 1);
				try
				{
					int num6 = Convert.ToInt32(text6);
					low = (high = Tier.Constrain(ZoneManager.zoneGenerationContextTier - num6));
					return true;
				}
				catch (Exception x4)
				{
					MetricsManager.LogError("Bad zone tier offset in " + origSpec + ": " + text6, x4);
					return false;
				}
			}
			MetricsManager.LogError("Bad zone tier offset in " + origSpec + " at " + array[1]);
			return false;
		}
		int num7 = text.IndexOf('-');
		if (num7 != -1)
		{
			string text7 = text.Substring(0, num7);
			string text8 = text.Substring(num7 + 1);
			try
			{
				low = Tier.Constrain(Convert.ToInt32(text7));
			}
			catch (Exception x5)
			{
				MetricsManager.LogError("Bad low tier specification in " + origSpec + ": " + text7, x5);
				return false;
			}
			high = low;
			try
			{
				high = Tier.Constrain(Convert.ToInt32(text8));
			}
			catch (Exception x6)
			{
				MetricsManager.LogError("Bad high tier specification in " + origSpec + ": " + text8, x6);
				return false;
			}
			return true;
		}
		try
		{
			low = (high = Tier.Constrain(Convert.ToInt32(text)));
			return true;
		}
		catch (Exception x7)
		{
			MetricsManager.LogError("Bad tier specification in " + origSpec + ": " + text, x7);
			return false;
		}
	}

	public static PopulationInfo ResolvePopulation(string Name, bool MissingOkay = false)
	{
		RequireTable(Name);
		if (_Populations.TryGetValue(Name, out var value))
		{
			return value;
		}
		if (!MissingOkay)
		{
			Debug.LogWarning("Unknown Population table: " + Name);
		}
		return null;
	}

	public static bool TryResolvePopulation(string Name, out PopulationInfo Population)
	{
		if (Name == null)
		{
			Population = null;
			return false;
		}
		RequireTable(Name);
		return _Populations.TryGetValue(Name, out Population);
	}

	public static bool TryResolvePopulation(string Name, Dictionary<string, string> Vars, out PopulationInfo Population)
	{
		if (Name == null)
		{
			Population = null;
			return false;
		}
		if (!Vars.IsNullOrEmpty())
		{
			ReplaceVariables(ref Name, Vars);
		}
		RequireTable(Name);
		return _Populations.TryGetValue(Name, out Population);
	}

	public static bool HasPopulation(string PopulationName)
	{
		return ResolvePopulation(PopulationName, MissingOkay: true) != null;
	}

	public static XRL.World.GameObject CreateOneFrom(string PopulationName, Dictionary<string, string> Variables = null, int BonusModChance = 0, int SetModNumber = 0, string AutoMod = null, string Context = null, Action<XRL.World.GameObject> BeforeObjectCreated = null, Action<XRL.World.GameObject> AfterObjectCreated = null, List<XRL.World.GameObject> ProvideInventory = null)
	{
		string blueprint = RollOneFrom(PopulationName, Variables).Blueprint;
		return GameObjectFactory.Factory.CreateObject(blueprint, BonusModChance, SetModNumber, AutoMod, BeforeObjectCreated, AfterObjectCreated, Context, ProvideInventory);
	}

	public static PopulationResult RollOneFrom(string PopulationName, Dictionary<string, string> Vars = null, string DefaultIfNull = null)
	{
		List<PopulationResult> list = null;
		try
		{
			list = Generate(PopulationName, Vars);
		}
		catch (Exception message)
		{
			list = new List<PopulationResult>();
			Popup.Show("Error generating population:" + PopulationName + "\n\n, please report this error to support@freeholdgames.com");
			MetricsManager.LogError(message);
		}
		if (list.Count == 0)
		{
			return new PopulationResult(DefaultIfNull, 0);
		}
		return list.GetRandomElement();
	}

	public static bool sameAs(List<PopulationResult> r1, List<PopulationResult> r2)
	{
		if (r1.Count != r2.Count)
		{
			return false;
		}
		foreach (PopulationResult entry in r1)
		{
			if (!r2.Any((PopulationResult e) => e.Blueprint == entry.Blueprint && e.Number == entry.Number))
			{
				return false;
			}
		}
		return true;
	}

	public static List<List<PopulationResult>> RollDistinctFrom(string PopulationName, int n, Dictionary<string, string> vars = null, string defaultIfNull = null)
	{
		List<List<PopulationResult>> list = new List<List<PopulationResult>>();
		int num = n;
		int num2 = 100;
		while (num > 0 && num2 > 0)
		{
			List<PopulationResult> Ret = Generate(PopulationName, vars);
			if (!list.Any((List<PopulationResult> r) => sameAs(r, Ret)))
			{
				list.Add(Ret);
				num--;
			}
			if (num <= 0)
			{
				break;
			}
			num2--;
		}
		return list;
	}

	public static List<XRL.World.GameObject> Expand(List<PopulationResult> Input)
	{
		int num = 0;
		List<XRL.World.GameObject> list = new List<XRL.World.GameObject>();
		for (int i = 0; i < Input.Count; i++)
		{
			for (int j = 0; j < Input[i].Number; j++)
			{
				list.Add(GameObjectFactory.Factory.CreateObject(Input[i].Blueprint));
				list[list.Count - 1].SetLongProperty("Batch", num);
				num++;
				if (num >= 2147483646)
				{
					num = 0;
				}
			}
		}
		return list;
	}

	public static bool HasTable(string PopulationName)
	{
		CheckInit();
		if (string.IsNullOrEmpty(PopulationName))
		{
			return false;
		}
		if (!_Populations.ContainsKey(PopulationName))
		{
			if (!PopulationName.StartsWith("Dynamic"))
			{
				return false;
			}
			PopulationInfo populationInfo = new PopulationInfo();
			PopulationTable item = new PopulationTable
			{
				Name = PopulationName
			};
			populationInfo.Items.Add(item);
			populationInfo.Name = PopulationName;
			populationInfo.Generate(new Dictionary<string, string>());
		}
		return true;
	}

	public static List<string> GetEach(string PopulationName, Dictionary<string, string> Vars = null, string DefaultHint = null)
	{
		if (!Vars.IsNullOrEmpty())
		{
			ReplaceVariables(ref PopulationName, Vars);
		}
		PopulationInfo populationInfo = ResolvePopulation(PopulationName);
		if (populationInfo == null)
		{
			return new List<string>();
		}
		List<string> list = new List<string>();
		foreach (PopulationItem item in populationInfo.Items)
		{
			item.GetEachUniqueObject(list);
		}
		return list;
	}

	public static List<PopulationResult> GenerateSemantic(List<string> tags, int tier, int techTier, Dictionary<string, string> vars, string DefaultHint = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("DynamicSemanticTable:");
		for (int i = 0; i < tags.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append(tags[i]);
		}
		stringBuilder.Append(":").Append(tier).Append(":")
			.Append(techTier);
		return Generate(stringBuilder.ToString(), vars, DefaultHint);
	}

	public static List<PopulationResult> Generate(string PopulationName, Dictionary<string, string> Vars, string DefaultHint = null)
	{
		if (!TryResolvePopulation(PopulationName, Vars, out var Population))
		{
			Debug.LogError("Failed to resolve: " + PopulationName);
			return new List<PopulationResult>();
		}
		return Population.Generate(Vars, DefaultHint);
	}

	public static List<PopulationResult> Generate(string PopulationName, string VariableName1 = null, string VariableValue1 = null, string VariableName2 = null, string VariableValue2 = null, string DefaultHint = null)
	{
		Dictionary<string, string> dictionary = GetDictionary(VariableName1, VariableValue1, VariableName2, VariableValue2);
		return Generate(PopulationName, dictionary, DefaultHint);
	}

	public static PopulationResult GenerateOne(string PopulationName, Dictionary<string, string> Vars, string DefaultHint = null)
	{
		if (!TryResolvePopulation(PopulationName, Vars, out var Population))
		{
			Debug.LogError("Failed to resolve: " + PopulationName);
			return null;
		}
		return Population.GenerateOne(Vars, DefaultHint);
	}

	public static PopulationResult GenerateOne(string PopulationName, string VariableName1 = null, string VariableValue1 = null, string VariableName2 = null, string VariableValue2 = null, string DefaultHint = null)
	{
		Dictionary<string, string> dictionary = GetDictionary(VariableName1, VariableValue1, VariableName2, VariableValue2);
		return GenerateOne(PopulationName, dictionary, DefaultHint);
	}

	public static List<PopulationResult> Generate(PopulationInfo Population, Dictionary<string, string> Vars, string DefaultHint = null)
	{
		return Population.Generate(Vars, DefaultHint);
	}

	public static PopulationResult GenerateOne(PopulationInfo Population, Dictionary<string, string> Vars, string DefaultHint = null)
	{
		return Population.GenerateOne(Vars, DefaultHint);
	}

	public static PopulationStructuredResult GenerateStructured(string PopulationName, Dictionary<string, string> Vars, string DefaultHint = null)
	{
		if (!TryResolvePopulation(PopulationName, Vars, out var Population))
		{
			Debug.LogError("Failed to resolve: " + PopulationName);
			return new PopulationStructuredResult();
		}
		PopulationStructuredResult populationStructuredResult = new PopulationStructuredResult();
		populationStructuredResult.Hint = Population.Hint ?? DefaultHint;
		Population.GenerateStructured(populationStructuredResult, Vars);
		return populationStructuredResult;
	}

	public static PopulationStructuredResult GenerateStructured(string PopulationName, string VariableName1 = null, string VariableValue1 = null, string VariableName2 = null, string VariableValue2 = null, string DefaultHint = null)
	{
		Dictionary<string, string> dictionary = GetDictionary(VariableName1, VariableValue1, VariableName2, VariableValue2);
		return GenerateStructured(PopulationName, dictionary, DefaultHint);
	}

	private static Dictionary<string, string> GetDictionary(string VariableName1 = null, string VariableValue1 = null, string VariableName2 = null, string VariableValue2 = null)
	{
		Dictionary<string, string> dictionary = null;
		if (VariableName1 != null)
		{
			dictionary = new Dictionary<string, string>();
			dictionary.Add(VariableName1, VariableValue1);
		}
		if (VariableName2 != null)
		{
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, string>();
			}
			dictionary.Add(VariableName2, VariableValue2);
		}
		return dictionary;
	}

	/// <summary>Add items to a population, next to a sibling object. </summary>
	/// <param name="table">The Name of the target population.</param>
	/// <param name="sibling">The Blueprint of the sibling object.</param>
	/// <param name="items">The items to add.</param>
	/// <returns>A boolean indicating success or failure.</returns>
	public static bool AddToPopulation(string table, string sibling, params PopulationItem[] items)
	{
		if (!Populations.TryGetValue(table, out var value))
		{
			return false;
		}
		PopulationGroup populationGroup = FindGroup(value, sibling);
		if (populationGroup == null)
		{
			return false;
		}
		populationGroup.Items.AddRange(items);
		return true;
	}

	/// <summary>
	/// Recursively searches for a population group with the specified object blueprint or table name.
	/// </summary>
	public static PopulationGroup FindGroup(PopulationGroup target, string needle)
	{
		foreach (PopulationItem item in target.Items)
		{
			if (item.Name == needle && item is PopulationTable)
			{
				return target;
			}
			if ((item as PopulationObject)?.Blueprint == needle)
			{
				return target;
			}
			if (item is PopulationGroup populationGroup && populationGroup.Items.Count > 0)
			{
				PopulationGroup populationGroup2 = FindGroup(populationGroup, needle);
				if (populationGroup2 != null)
				{
					return populationGroup2;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Recursively searches for a population group with the specified object blueprint or table name.
	/// </summary>
	public static PopulationGroup FindGroup(PopulationInfo target, string needle)
	{
		foreach (PopulationItem item in target.Items)
		{
			if (item is PopulationGroup populationGroup && populationGroup.Items.Count > 0)
			{
				PopulationGroup populationGroup2 = FindGroup(populationGroup, needle);
				if (populationGroup2 != null)
				{
					return populationGroup2;
				}
			}
		}
		return null;
	}

	public static void ReplaceVariables(ref string Blueprint, Dictionary<string, string> Variables)
	{
		int num = Blueprint.IndexOf('{');
		if (num == -1)
		{
			return;
		}
		int length = Blueprint.Length;
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder().Append(Blueprint, 0, num);
		while (true)
		{
			int num2 = Blueprint.IndexOf('}', num + 1, length - num - 1);
			if (num2 == -1)
			{
				stringBuilder.Append(Blueprint, num, length - num);
				break;
			}
			int num3 = num2;
			string text = Blueprint.Substring(num + 1, num3 - num - 1);
			if (Variables.TryGetValue(text, out var value))
			{
				stringBuilder.Append(value);
			}
			else
			{
				Debug.LogWarning("Unknown population variable key:" + text);
				stringBuilder.Append(Blueprint, num, num3 - num + 1);
			}
			num2 = Blueprint.IndexOf('{', num3 + 1, length - num3 - 1);
			if (num2 == -1)
			{
				stringBuilder.Append(Blueprint, num3 + 1, length - num3 - 1);
				break;
			}
			num = num2;
			stringBuilder.Append(Blueprint, num3 + 1, num - num3 - 1);
		}
		Blueprint = stringBuilder.ToString();
	}

	private static void LoadFiles()
	{
		_Populations = new Dictionary<string, PopulationInfo>();
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Populations"))
		{
			try
			{
				HandleNodes(item);
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Error reading " + DataManager.SanitizePathForDisplay(item.BaseURI), x);
			}
		}
	}

	private static void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(Nodes);
	}

	private static void HandlePopulationNode(XmlDataHelper Reader)
	{
		if (CurrentReadingPopulation != null)
		{
			Reader.ParseWarning("Loading a new population when we seemingly never parsed the old one.");
		}
		PopulationInfo populationInfo = (CurrentReadingPopulation = new PopulationInfo());
		populationInfo.Name = Reader.ParseAttribute("Name", populationInfo.Name, required: true);
		populationInfo.Style = Reader.GetAttribute("Style");
		populationInfo.Hint = Reader.GetAttribute("Hint");
		populationInfo.Load = Reader.GetAttribute("Load").Coalesce("Merge");
		Reader.HandleNodes(PopulationSubNodes);
		if (populationInfo.Merge && Populations.TryGetValue(populationInfo.Name, out var value))
		{
			value.MergeFrom(populationInfo, value);
		}
		else if (populationInfo.Remove)
		{
			Populations.Remove(populationInfo.Name);
		}
		else if (populationInfo.Replace)
		{
			Populations[populationInfo.Name] = populationInfo;
		}
		else
		{
			Populations.Add(populationInfo.Name, populationInfo);
		}
		CurrentReadingPopulation = null;
	}

	private static void HandleObjectNode(XmlDataHelper xml)
	{
		PopulationObject item = LoadPopulationObject(xml);
		CurrentReadingPopulation.Items.Add(item);
	}

	private static void HandleTableNode(XmlDataHelper xml)
	{
		PopulationTable item = LoadPopulationTable(xml);
		CurrentReadingPopulation.Items.Add(item);
	}

	private static void HandleGroupNode(XmlDataHelper xml)
	{
		PopulationGroup populationGroup = LoadPopulationGroup(xml, CurrentReadingPopulation);
		if (populationGroup != null)
		{
			populationGroup.Parent = CurrentReadingPopulation;
			CurrentReadingPopulation.Items.Add(populationGroup);
		}
	}

	private static void HandleGroupObjectNode(XmlDataHelper xml)
	{
		PopulationObject item = LoadPopulationObject(xml);
		CurrentReadingGroup.Items.Add(item);
	}

	private static void HandleGroupTableNode(XmlDataHelper xml)
	{
		PopulationTable item = LoadPopulationTable(xml);
		CurrentReadingGroup.Items.Add(item);
	}

	private static void HandleGroupGroupNode(XmlDataHelper xml)
	{
		PopulationGroup populationGroup = LoadPopulationGroup(xml, CurrentReadingPopulation);
		if (populationGroup != null)
		{
			populationGroup.Parent = CurrentReadingGroup;
			CurrentReadingGroup.Items.Add(populationGroup);
		}
	}

	private static PopulationGroup LoadPopulationGroup(XmlDataHelper Reader, PopulationInfo Info)
	{
		PopulationGroup populationGroup = new PopulationGroup();
		populationGroup.Name = Reader.ParseAttribute("Name", populationGroup.Name, required: true);
		populationGroup.Style = Reader.GetAttribute("Style");
		populationGroup.Chance = Reader.GetAttribute("Chance");
		populationGroup.Number = Reader.GetAttribute("Number");
		populationGroup.Hint = Reader.GetAttribute("Hint");
		populationGroup.Load = Reader.GetAttribute("Load").Coalesce("Merge");
		if (uint.TryParse(Reader.GetAttribute("Weight"), out var result))
		{
			populationGroup.Weight = result;
		}
		if (!populationGroup.Name.IsNullOrEmpty() && Info.GroupLookup.ContainsKey(populationGroup.Name))
		{
			throw new XmlException("Duplicate group name '" + populationGroup.Name + "'", Reader);
		}
		PopulationGroup currentReadingGroup = CurrentReadingGroup;
		CurrentReadingGroup = populationGroup;
		Reader.HandleNodes(GroupSubNodes);
		CurrentReadingGroup = currentReadingGroup;
		if (!populationGroup.Name.IsNullOrEmpty())
		{
			Info.GroupLookup[populationGroup.Name] = populationGroup;
		}
		return populationGroup;
	}

	private static PopulationObject LoadPopulationObject(XmlDataHelper Reader)
	{
		PopulationObject populationObject = new PopulationObject();
		populationObject.Name = Reader.GetAttribute("Name");
		populationObject.Blueprint = Reader.GetAttribute("Blueprint");
		populationObject.Number = Reader.GetAttribute("Number");
		populationObject.Chance = Reader.GetAttribute("Chance");
		populationObject.Hint = Reader.GetAttribute("Hint");
		populationObject.Load = Reader.GetAttribute("Load");
		populationObject.Builder = Reader.GetAttribute("Builder");
		if (uint.TryParse(Reader.GetAttribute("Weight"), out var result))
		{
			populationObject.Weight = result;
		}
		Reader.DoneWithElement();
		return populationObject;
	}

	private static PopulationTable LoadPopulationTable(XmlDataHelper Reader)
	{
		PopulationTable populationTable = new PopulationTable();
		populationTable.Name = Reader.GetAttribute("Name");
		populationTable.Number = Reader.GetAttribute("Number");
		populationTable.Chance = Reader.GetAttribute("Chance");
		populationTable.Hint = Reader.GetAttribute("Hint");
		populationTable.Load = Reader.GetAttribute("Load");
		if (uint.TryParse(Reader.GetAttribute("Weight"), out var result))
		{
			populationTable.Weight = result;
		}
		Reader.DoneWithElement();
		return populationTable;
	}

	[WishCommand("population:generate", null)]
	private static void WishGenerate(string Value)
	{
		Value.AsDelimitedSpans('#', out var First, out var Second);
		if (!int.TryParse(Second, out var result))
		{
			if (!Second.IsEmpty)
			{
				Popup.Show("'" + new string(Second) + "' is not a valid integer.");
				return;
			}
			result = 1000;
		}
		string text = new string(First);
		if (!TryResolvePopulation(text, out var Population))
		{
			Popup.Show("No table by the name '" + text + "' could be resolved.");
			return;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		HashSet<string> hashSet = new HashSet<string>();
		int num = 0;
		for (int i = 0; i < result; i++)
		{
			foreach (PopulationResult item in Population.Generate())
			{
				if (hashSet.Add(item.Blueprint))
				{
					int value = dictionary.GetValue(item.Blueprint, 0);
					dictionary[item.Blueprint] = value + 1;
				}
			}
			hashSet.Clear();
			int num2 = i * 100 / result;
			if (num2 > num)
			{
				num = num2;
				Loading.SetLoadingStatus($"Generating {text} {num2}%...");
			}
		}
		Loading.SetLoadingStatus("Generating " + text + " 100%...");
		Loading.SetLoadingStatus(null);
		StringBuilder stringBuilder = Strings.SB.Clear();
		foreach (var (value2, num4) in dictionary.OrderByDescending((KeyValuePair<string, int> x) => x.Value))
		{
			stringBuilder.Append('\n').Append(value2).Append(": ")
				.Append(((double)num4 / (double)result).ToString("P5"));
		}
		if (stringBuilder.Length == 0)
		{
			stringBuilder.Append("The population '").Append(First).Append("' did not generate any results.");
		}
		else
		{
			stringBuilder.Insert(0, " generations;\n").Insert(0, result).Insert(0, "' distributed as follows over ")
				.Insert(0, First)
				.Insert(0, "The population '");
		}
		string text3 = stringBuilder.ToString();
		ClipboardHelper.SetClipboardData(text3);
		Popup.Show(text3, null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
	}

	[WishCommand("population:findblueprint", null)]
	private static void WishFindBlueprint(string Blueprint)
	{
		Loading.LoadTask("Fabricating semantic tables...", RequireSemanticTables);
		Dictionary<string, double> dictionary = new Dictionary<string, double>();
		Stack<PopulationInfo> stack = new Stack<PopulationInfo>();
		string key;
		while (true)
		{
			IL_0023:
			int count = Populations.Count;
			foreach (KeyValuePair<string, PopulationInfo> population in Populations)
			{
				population.Deconstruct(out key, out var value);
				string text = key;
				PopulationInfo item = value;
				if (!dictionary.ContainsKey(text))
				{
					Loading.SetLoadingStatus("Analyzing " + text + "...");
					try
					{
						double value2 = FindBlueprintProbability(item, Blueprint, stack, dictionary);
						dictionary[text] = value2;
					}
					catch (Exception ex)
					{
						Debug.LogError(text + " analyzation: " + ex);
					}
					if (Populations.Count != count)
					{
						goto IL_0023;
					}
				}
			}
			break;
		}
		Loading.SetLoadingStatus(null);
		StringBuilder stringBuilder = Strings.SB.Clear();
		foreach (KeyValuePair<string, double> item2 in dictionary.OrderByDescending((KeyValuePair<string, double> x) => x.Value))
		{
			item2.Deconstruct(out key, out var value3);
			string value4 = key;
			double num = value3;
			if (num > 0.0)
			{
				stringBuilder.Append('\n').Append(value4).Append(": ")
					.Append(num.ToString("P5"));
			}
		}
		if (stringBuilder.Length == 0)
		{
			stringBuilder.Append("The blueprint '").Append(Blueprint).Append("' was not found in any population tables.");
		}
		else
		{
			stringBuilder.Insert(0, "' has the approximate probability of generating at least once from the following tables;\n").Insert(0, Blueprint).Insert(0, "The blueprint '");
		}
		if (!GameObjectFactory.Factory.Blueprints.ContainsKey(Blueprint))
		{
			stringBuilder.Insert(0, "' was not found in any object blueprints.\n").Insert(0, Blueprint).Insert(0, "The blueprint '");
		}
		string text2 = stringBuilder.ToString();
		ClipboardHelper.SetClipboardData(text2);
		Popup.Show(text2, null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
	}

	private static double FindListProbability(PopulationList List, string Blueprint, Stack<PopulationInfo> Stack, Dictionary<string, double> TableProbabilities)
	{
		double num = 0.0;
		if (List.Style.EqualsNoCase("pickone"))
		{
			ulong totalWeight = List.TotalWeight;
			double num2 = 1.0;
			foreach (PopulationItem item in List.Items)
			{
				num2 *= 1.0 - (double)item.Weight / (double)totalWeight * FindBlueprintProbability(item, Blueprint, Stack, TableProbabilities);
			}
			return 1.0 - num2;
		}
		double num3 = 1.0;
		foreach (PopulationItem item2 in List.Items)
		{
			num3 *= 1.0 - FindBlueprintProbability(item2, Blueprint, Stack, TableProbabilities);
		}
		return 1.0 - num3;
	}

	private static double FindBlueprintProbability(PopulationItem Item, string Blueprint, Stack<PopulationInfo> Stack, Dictionary<string, double> TableProbabilities)
	{
		if (Item == null)
		{
			return 0.0;
		}
		if (Item is PopulationObject populationObject)
		{
			if (!(populationObject.Blueprint == Blueprint))
			{
				return 0.0;
			}
			return populationObject.Probability();
		}
		if (Item is PopulationGroup populationGroup)
		{
			double num = FindListProbability(populationGroup, Blueprint, Stack, TableProbabilities);
			num = 1.0 - Math.Pow(1.0 - num, populationGroup.AverageNumber());
			return num * populationGroup.Probability();
		}
		if (Item is PopulationInfo populationInfo)
		{
			if (TableProbabilities.TryGetValue(populationInfo.Name, out var value))
			{
				return value;
			}
			int num2 = 0;
			int count = Stack.Count;
			foreach (PopulationInfo item2 in Stack)
			{
				if (item2 == populationInfo)
				{
					num2++;
				}
			}
			if ((count >= 30 && num2 >= 1) || (count >= 20 && num2 >= 3) || num2 >= 5)
			{
				return 0.0;
			}
			Stack.Push(populationInfo);
			value = FindListProbability(populationInfo, Blueprint, Stack, TableProbabilities);
			TableProbabilities.TryAdd(Item.Name, value);
			PopulationInfo populationInfo2 = Stack.Pop();
			if (populationInfo2 != populationInfo)
			{
				Debug.LogError("Evaluation stack is invalid, expected table '" + populationInfo.Name + "' but got '" + populationInfo2.Name + "'.");
			}
			return value;
		}
		if (Item is PopulationTable { Name: var name } populationTable)
		{
			double num3 = 0.0;
			if (name.Contains("tier}"))
			{
				for (int i = 1; i <= 8; i++)
				{
					string newValue = i.ToString();
					PopulationInfo item = ResolvePopulation(name.Replace("{zonetier}", newValue).Replace("{ownertier}", newValue), MissingOkay: true);
					num3 += FindBlueprintProbability(item, Blueprint, Stack, TableProbabilities);
				}
				num3 /= 8.0;
			}
			else
			{
				num3 = FindBlueprintProbability(ResolvePopulation(populationTable.Name, MissingOkay: true), Blueprint, Stack, TableProbabilities);
			}
			num3 = 1.0 - Math.Pow(1.0 - num3, populationTable.AverageNumber());
			return num3 * populationTable.Probability();
		}
		Debug.LogError("Unsupported inheritor of PopulationItem: " + Item.GetType().Name + ".");
		return 0.0;
	}

	private static void RequireSemanticTables()
	{
		HashSet<string> hashSet = new HashSet<string>();
		StringBuilder sB = Strings.SB;
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			foreach (var (text3, _) in blueprint.Tags)
			{
				if (!text3.StartsWith("Semantic") || !hashSet.Add(text3))
				{
					continue;
				}
				sB.Clear().Append("DynamicSemanticTable:").Append(text3, 8, text3.Length - 8)
					.Append("::");
				int i = 1;
				int length = sB.Length;
				for (; i <= 8; i++)
				{
					sB.Length = length;
					sB.Append(i);
					string text4 = sB.ToString();
					if (!_Populations.ContainsKey(text4))
					{
						Loading.SetLoadingStatus("Fabricating " + text4 + "...");
						RequireTable(text4);
					}
				}
			}
		}
		Loading.SetLoadingStatus(null);
	}
}
