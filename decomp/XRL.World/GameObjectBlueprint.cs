using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

public class GameObjectBlueprint
{
	public string Name = "";

	public string Inherits = "";

	public string Load = "";

	public bool hasChildren;

	private string _DisplayNameStripped;

	private string _NameLC;

	private string _DisplayNameStrippedLC;

	private string _DisplayNameStrippedTitleCase;

	private int _Tier = -999;

	private int _TechTier = -999;

	private bool? _BaseBlueprint;

	private bool? _ExcludeFromDynamicEncounters;

	private GameObjectBlueprint _ShallowParent;

	public Dictionary<string, GamePartBlueprint> Parts = new Dictionary<string, GamePartBlueprint>();

	public List<string> RemovedParts = new List<string>();

	public Dictionary<string, GamePartBlueprint> Mutations = new Dictionary<string, GamePartBlueprint>();

	public Dictionary<string, GamePartBlueprint> Skills = new Dictionary<string, GamePartBlueprint>();

	public Dictionary<string, GamePartBlueprint> Builders = new Dictionary<string, GamePartBlueprint>();

	public readonly Dictionary<string, Statistic> Stats = new Dictionary<string, Statistic>(0);

	public Dictionary<string, string> Props = new Dictionary<string, string>(0);

	public Dictionary<string, int> IntProps = new Dictionary<string, int>(0);

	public Dictionary<string, string> Tags = new Dictionary<string, string>(0);

	public Dictionary<string, Dictionary<string, string>> xTags;

	public List<InventoryObject> Inventory;

	private bool? _Natural;

	private bool? _Wall;

	private bool _Seen;

	private bool? _Proper;

	public string CachedDisplayNameStripped => _DisplayNameStripped ?? (_DisplayNameStripped = DisplayName().Strip());

	public string CachedNameLC
	{
		get
		{
			if (_NameLC == null && Name != null)
			{
				_NameLC = Name.ToLower();
			}
			return _NameLC;
		}
	}

	public string CachedDisplayNameStrippedLC
	{
		get
		{
			if (_DisplayNameStrippedLC == null)
			{
				_DisplayNameStrippedLC = CachedDisplayNameStripped.ToLower();
			}
			return _DisplayNameStrippedLC;
		}
	}

	public string CachedDisplayNameStrippedTitleCase
	{
		get
		{
			if (_DisplayNameStrippedTitleCase == null)
			{
				_DisplayNameStrippedTitleCase = Grammar.MakeTitleCase(CachedDisplayNameStripped);
			}
			return _DisplayNameStrippedTitleCase;
		}
	}

	public int Tier
	{
		get
		{
			if (_Tier == -999)
			{
				if (Tags.ContainsKey("Tier"))
				{
					_Tier = Convert.ToInt32(Tags["Tier"]);
				}
				else if (HasStat("Level"))
				{
					int num = GetStat("Level").BaseValue / 5 + 1;
					if (num < 1)
					{
						num = 1;
					}
					if (num > 8)
					{
						num = 8;
					}
					_Tier = num;
				}
			}
			return _Tier;
		}
	}

	public int TechTier
	{
		get
		{
			if (_TechTier == -999)
			{
				if (Tags.ContainsKey("TechTier"))
				{
					_TechTier = Convert.ToInt32(Tags["TechTier"]);
				}
				else if (Tier != -999)
				{
					_TechTier = Tier;
				}
				else if (HasStat("Level"))
				{
					int num = GetStat("Level").BaseValue / 5 + 1;
					if (num < 1)
					{
						num = 1;
					}
					if (num > 8)
					{
						num = 8;
					}
					_TechTier = num;
				}
			}
			return _TechTier;
		}
	}

	public Dictionary<string, GamePartBlueprint> allparts => Parts;

	public GameObjectBlueprint ShallowParent
	{
		get
		{
			if (_ShallowParent == null && !Inherits.IsNullOrEmpty())
			{
				GameObjectFactory.Factory.Blueprints.TryGetValue(Inherits, out _ShallowParent);
			}
			return _ShallowParent;
		}
	}

	public bool HasPartParameter(string Part, string Parameter)
	{
		return GetPart(Part)?.HasParameter(Parameter) == true;
	}

	/// <summary>
	///     Type safe version of GetPartParameter that uses the parsed property value.
	///     If you want the unparsed string, GetPart(PartName)?.GetParameterString(Parameter) ?? Default.
	/// </summary>
	public ResultType GetPartParameter<ResultType>(string PartName, string ParameterName, ResultType Default = default(ResultType))
	{
		GamePartBlueprint part = GetPart(PartName);
		if (part != null && part.TryGetParameter<ResultType>(ParameterName, out var Value))
		{
			return Value;
		}
		return Default;
	}

	public bool TryGetPartParameter<T>(string PartName, string ParameterName, out T Result)
	{
		if (Parts.TryGetValue(PartName, out var value))
		{
			if (value.TryGetParameter<T>(ParameterName, out Result))
			{
				return true;
			}
			return false;
		}
		Result = default;
		return false;
	}

	public string DisplayName()
	{
		return GetPartParameter<string>("Render", "DisplayName");
	}

	public string GetPrimaryFaction()
	{
		string partParameter = GetPartParameter<string>("Brain", "Factions");
		if (partParameter.IsNullOrEmpty())
		{
			return null;
		}
		if (partParameter.EndsWith("-100") && !partParameter.Contains(","))
		{
			return partParameter.Substring(0, partParameter.Length - 4);
		}
		return Brain.GetPrimaryFaction(partParameter);
	}

	public bool InheritsFrom(string what)
	{
		string inherits = Inherits;
		while (!inherits.IsNullOrEmpty())
		{
			if (inherits == what)
			{
				return true;
			}
			inherits = GameObjectFactory.Factory.Blueprints[inherits].Inherits;
		}
		return false;
	}

	public string GetBase()
	{
		if (IsBaseBlueprint())
		{
			return Name;
		}
		if (Inherits != null && GameObjectFactory.Factory.Blueprints.TryGetValue(Inherits, out var value))
		{
			return value.GetBase();
		}
		return "BaseObject";
	}

	public string GetBaseTypeName()
	{
		string text = GetBase();
		if (text.StartsWith("Base"))
		{
			text = text.Substring(4);
		}
		return text;
	}

	public static string GetBase(string Blueprint)
	{
		if (GameObjectFactory.Factory.Blueprints.TryGetValue(Blueprint, out var value))
		{
			return value.GetBase();
		}
		return "BaseObject";
	}

	public bool IsBaseBlueprint()
	{
		bool valueOrDefault = _BaseBlueprint == true;
		if (!_BaseBlueprint.HasValue)
		{
			valueOrDefault = HasTag("BaseObject");
			_BaseBlueprint = valueOrDefault;
			return valueOrDefault;
		}
		return valueOrDefault;
	}

	public bool IsExcludedFromDynamicEncounters()
	{
		bool valueOrDefault = _ExcludeFromDynamicEncounters == true;
		if (!_ExcludeFromDynamicEncounters.HasValue)
		{
			valueOrDefault = HasTag("ExcludeFromDynamicEncounters") || (TryGetTag("ExcludeFromDynamicEncountersOption", out var Value) && (Value.StartsWith("!") ? (!Options.GetOptionBool(Value.Substring(1))) : Options.GetOptionBool(Value)));
			_ExcludeFromDynamicEncounters = valueOrDefault;
			return valueOrDefault;
		}
		return valueOrDefault;
	}

	public bool TryGetTag(string Tag, out string Value)
	{
		Value = null;
		if (HasTag(Tag))
		{
			Value = GetTag(Tag);
			return true;
		}
		return false;
	}

	public bool DescendsFrom(string Blueprint)
	{
		if (Blueprint.IsNullOrEmpty())
		{
			return false;
		}
		if (!GameObjectFactory.Factory.Blueprints.TryGetValue(Blueprint, out var value))
		{
			return false;
		}
		return DescendsFrom(value);
	}

	public bool DescendsFrom(GameObjectBlueprint Blueprint)
	{
		if (Blueprint == null)
		{
			return false;
		}
		GameObjectBlueprint gameObjectBlueprint = this;
		do
		{
			if (gameObjectBlueprint == Blueprint)
			{
				return true;
			}
			gameObjectBlueprint = gameObjectBlueprint.ShallowParent;
		}
		while (gameObjectBlueprint != null);
		return false;
	}

	public void RemovePart(string ID)
	{
		if (!RemovedParts.Contains(ID))
		{
			RemovedParts.Add(ID);
		}
	}

	public void UpdatePart(string ID, GamePartBlueprint Part)
	{
		if (Parts.TryGetValue(ID, out var value))
		{
			value.CopyFrom(Part);
		}
		else
		{
			Parts.Add(ID, Part);
		}
	}

	public bool HasPart(string ID)
	{
		return Parts.ContainsKey(ID);
	}

	public GamePartBlueprint GetPart(string ID)
	{
		if (Parts.TryGetValue(ID, out var value))
		{
			return value;
		}
		return null;
	}

	public GameObject createOne()
	{
		return GameObjectFactory.Factory.CreateObject(Name);
	}

	public GameObject createSample()
	{
		return GameObjectFactory.Factory.CreateSampleObject(Name);
	}

	public GameObject createUnmodified()
	{
		return GameObjectFactory.Factory.CreateUnmodifiedObject(Name);
	}

	public void UpdateStat(string id, Statistic stat)
	{
		Stats[id] = stat;
	}

	public bool HasStat(string id)
	{
		return Stats.ContainsKey(id);
	}

	public Statistic GetStat(string id, Statistic def = null)
	{
		Statistic value = null;
		if (Stats.TryGetValue(id, out value))
		{
			if (value == null)
			{
				return def;
			}
			return value;
		}
		return def;
	}

	public int Stat(string id, int def = 0)
	{
		Statistic value = null;
		if (Stats.TryGetValue(id, out value))
		{
			return value?.Value ?? def;
		}
		return def;
	}

	public int BaseStat(string id, int def = 0)
	{
		Statistic value = null;
		if (Stats.TryGetValue(id, out value))
		{
			return value?.BaseValue ?? def;
		}
		return def;
	}

	public bool HasTagOrProperty(string id)
	{
		if (!HasTag(id))
		{
			return HasProperty(id);
		}
		return true;
	}

	public bool HasProperty(string id)
	{
		if (Props.ContainsKey(id))
		{
			return true;
		}
		if (IntProps.ContainsKey(id))
		{
			return true;
		}
		return false;
	}

	public bool HasTag(string id, bool excludeNoInherit = false)
	{
		if (Tags != null && Tags.ContainsKey(id) && (!excludeNoInherit || Tags[id] != "*noinherit"))
		{
			return true;
		}
		return false;
	}

	public string GetProp(string Name, string Default = null)
	{
		if (Props.TryGetValue(Name, out var value))
		{
			return value;
		}
		return Default;
	}

	public string GetxTag(string Tag, string Value, string Default = null)
	{
		if (xTags != null && xTags.TryGetValue(Tag, out var value) && value.TryGetValue(Value, out var value2))
		{
			return value2;
		}
		return Default;
	}

	public string GetxTag_CommaDelimited(string Tag, string Value, string Default = null, Random R = null)
	{
		if (xTags != null && xTags.TryGetValue(Tag, out var value) && value.TryGetValue(Value, out var value2))
		{
			return value2.GetRandomSubstring(',', Trim: false, R);
		}
		return Default;
	}

	public string GetPropertyOrTag(string id)
	{
		if (Props.TryGetValue(id, out var value))
		{
			return value;
		}
		if (IntProps.TryGetValue(id, out var value2))
		{
			return value2.ToString();
		}
		return GetTag(id, null);
	}

	public string GetStringPropertyOrTag(string Key, string Default = null)
	{
		if (Props.TryGetValue(Key, out var value) || Tags.TryGetValue(Key, out value))
		{
			return value;
		}
		return Default;
	}

	public bool TryGetStringPropertyOrTag(string Key, out string Value)
	{
		if (!Props.TryGetValue(Key, out Value))
		{
			return Tags.TryGetValue(Key, out Value);
		}
		return true;
	}

	public string GetTag(string Name, string Default = "")
	{
		if (Tags != null && Tags.TryGetValue(Name, out var value))
		{
			return value;
		}
		return Default;
	}

	/// <summary>
	/// Get the tile of this blueprint, painted with mask if it's a wall or fence.
	/// </summary>
	/// <param name="Mask">A bit mask indicating connecting neighbours, most significant bit is north and continues clockwise.</param>
	public string GetPaintedTile(int Mask = 0)
	{
		if (TryGetStringPropertyOrTag("PaintedWall", out var Value))
		{
			Value = Value.GetRandomSubstring(',', Trim: false, XRL.Rules.Stat.Rnd2);
			return Event.FinalizeString(Event.NewStringBuilder().Append(GetStringPropertyOrTag("PaintedWallAtlas", "Tiles/")).Append(Value)
				.Append('-')
				.AppendMask(Mask, 8)
				.Append(GetStringPropertyOrTag("PaintedWallExtension", ".bmp")));
		}
		if (TryGetStringPropertyOrTag("PaintedFence", out Value))
		{
			Value = Value.GetRandomSubstring(',', Trim: false, XRL.Rules.Stat.Rnd2);
			return Event.FinalizeString(Event.NewStringBuilder().Append(GetStringPropertyOrTag("PaintedFenceAtlas", "Tiles/")).Append(Value)
				.Append('_')
				.Append(Mask.HasBit(128) ? "n" : "")
				.Append(Mask.HasBit(8) ? "s" : "")
				.Append(Mask.HasBit(32) ? "e" : "")
				.Append(Mask.HasBit(2) ? "w" : "")
				.Append(GetStringPropertyOrTag("PaintedFenceExtension", ".bmp")));
		}
		return GetPartParameter<string>("Render", "Tile");
	}

	public bool IsBodyPartOccupied(string Name)
	{
		if (Inventory != null)
		{
			foreach (InventoryObject item in Inventory)
			{
				if (!GameObjectFactory.Factory.Blueprints.TryGetValue(item.Blueprint, out var value) || (!value.HasPart("NaturalEquipment") && !value.HasTag("NaturalGear") && ((!value.HasTagOrProperty("AlwaysEquipAsWeapon") && !value.HasTagOrProperty("AlwaysEquipAsArmor")) || !value.HasPart("Cursed"))))
				{
					continue;
				}
				string tag = value.GetTag("UsesSlots", null);
				if (tag != null)
				{
					if (tag.CachedCommaExpansion().Contains(Name))
					{
						return true;
					}
				}
				else if (value.GetPartParameter<string>("MeleeWeapon", "Slot") == Name)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ReinitializePart(IPart Part)
	{
		if (Part == null)
		{
			return false;
		}
		GamePartBlueprint value = null;
		if (allparts.TryGetValue(Part.GetType().Name, out value))
		{
			try
			{
				value.InitializePartInstance(Part);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("exception reinitializing part " + Part.Name, x);
			}
			return true;
		}
		return false;
	}

	public bool IsNatural()
	{
		bool valueOrDefault = _Natural == true;
		if (!_Natural.HasValue)
		{
			valueOrDefault = IntProps.ContainsKey("Natural") || Props.ContainsKey("Natural") || HasTag("Natural") || IntProps.ContainsKey("NaturalGear") || Props.ContainsKey("NaturalGear") || HasTag("NaturalGear");
			_Natural = valueOrDefault;
			return valueOrDefault;
		}
		return valueOrDefault;
	}

	public bool IsWall()
	{
		bool valueOrDefault = _Wall == true;
		if (!_Wall.HasValue)
		{
			valueOrDefault = Tags.ContainsKey("Wall");
			_Wall = valueOrDefault;
			return valueOrDefault;
		}
		return valueOrDefault;
	}

	public bool Seen()
	{
		if (_Seen)
		{
			return false;
		}
		The.Game.BlueprintSeen(Name);
		_Seen = true;
		return true;
	}

	public bool HasProperName()
	{
		bool valueOrDefault = _Proper == true;
		if (!_Proper.HasValue)
		{
			valueOrDefault = GetxTag("Grammar", "Proper") == "true";
			_Proper = valueOrDefault;
			return valueOrDefault;
		}
		return valueOrDefault;
	}

	public bool HasBeenSeen()
	{
		if (!_Seen)
		{
			return _Seen = XRLCore.Core.Game.HasBlueprintBeenSeen(Name);
		}
		return true;
	}

	public void ClearGameCache()
	{
		_Seen = false;
	}

	public Renderable GetRenderable()
	{
		return new Renderable(this);
	}

	private string EscapeXML(string str)
	{
		return SecurityElement.Escape(str);
	}

	public string BlueprintXML()
	{
		Dictionary<string, StringBuilder> dictionary = new Dictionary<string, StringBuilder>();
		foreach (string key in Tags.Keys)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.AppendFormat("<tag Name=\"{0}\"", EscapeXML(key));
			if (!string.IsNullOrEmpty(Tags[key]))
			{
				stringBuilder.AppendFormat(" Value=\"{0}\"", EscapeXML(Tags[key]));
			}
			stringBuilder.Append(" />");
			dictionary.Add("tag:" + key, stringBuilder);
		}
		if (xTags != null)
		{
			foreach (KeyValuePair<string, Dictionary<string, string>> xTag in xTags)
			{
				StringBuilder stringBuilder2 = Event.NewStringBuilder();
				stringBuilder2.AppendFormat("<xtag Name=\"{0}\"", EscapeXML(xTag.Key));
				foreach (KeyValuePair<string, string> item in xTag.Value)
				{
					stringBuilder2.AppendFormat(" {0}=\"{1}\"", item.Key, EscapeXML(item.Value));
				}
				stringBuilder2.Append(" />");
				dictionary.Add("xtag:" + xTag.Key, stringBuilder2);
			}
		}
		foreach (KeyValuePair<string, GamePartBlueprint> part in Parts)
		{
			StringBuilder stringBuilder3 = Event.NewStringBuilder();
			stringBuilder3.AppendFormat("<part Name=\"{0}\"", EscapeXML(part.Value.Name));
			foreach (KeyValuePair<string, string> parameterString in part.Value.GetParameterStrings())
			{
				if (parameterString.Key != "Name")
				{
					stringBuilder3.AppendFormat(" {0}=\"{1}\"", parameterString.Key, EscapeXML(parameterString.Value));
				}
			}
			stringBuilder3.Append(" />");
			dictionary.Add("apart:" + part.Value.Name, stringBuilder3);
		}
		foreach (KeyValuePair<string, GamePartBlueprint> mutation in Mutations)
		{
			StringBuilder stringBuilder4 = Event.NewStringBuilder();
			stringBuilder4.AppendFormat("<mutation Name=\"{0}\"", EscapeXML(mutation.Value.Name));
			foreach (KeyValuePair<string, string> parameterString2 in mutation.Value.GetParameterStrings())
			{
				if (parameterString2.Key != "Name")
				{
					stringBuilder4.AppendFormat(" {0}=\"{1}\"", parameterString2.Key, EscapeXML(parameterString2.Value));
				}
			}
			stringBuilder4.Append(" />");
			dictionary.Add("mutation:" + mutation.Value.Name, stringBuilder4);
		}
		foreach (KeyValuePair<string, GamePartBlueprint> skill in Skills)
		{
			StringBuilder stringBuilder5 = Event.NewStringBuilder();
			stringBuilder5.AppendFormat("<skill Name=\"{0}\"", EscapeXML(skill.Value.Name));
			foreach (KeyValuePair<string, string> parameterString3 in skill.Value.GetParameterStrings())
			{
				if (parameterString3.Key != "Name")
				{
					stringBuilder5.AppendFormat(" {0}=\"{1}\"", parameterString3.Key, EscapeXML(parameterString3.Value));
				}
			}
			stringBuilder5.Append(" />");
			dictionary.Add("skill:" + skill.Value.Name, stringBuilder5);
		}
		foreach (KeyValuePair<string, string> prop in Props)
		{
			StringBuilder stringBuilder6 = Event.NewStringBuilder();
			stringBuilder6.AppendFormat("<property Name=\"{0}\" Value=\"{1}\" />", prop.Key, EscapeXML(prop.Value));
			dictionary.Add("prop:" + prop.Key, stringBuilder6);
		}
		foreach (KeyValuePair<string, int> intProp in IntProps)
		{
			StringBuilder stringBuilder7 = Event.NewStringBuilder();
			stringBuilder7.AppendFormat("<intproperty Name=\"{0}\" Value=\"{1}\" />", intProp.Key, intProp.Value);
			dictionary.Add("propi:" + intProp.Key, stringBuilder7);
		}
		foreach (KeyValuePair<string, GamePartBlueprint> builder in Builders)
		{
			StringBuilder stringBuilder8 = Event.NewStringBuilder();
			stringBuilder8.AppendFormat("<builder Name=\"{0}\"", builder.Value.Name);
			foreach (KeyValuePair<string, string> parameterString4 in builder.Value.GetParameterStrings())
			{
				if (parameterString4.Key != "Name")
				{
					stringBuilder8.AppendFormat(" {0}=\"{1}\"", parameterString4.Key, EscapeXML(parameterString4.Value));
				}
			}
			stringBuilder8.Append(" />");
			dictionary.Add("builder:" + builder.Value.Name, stringBuilder8);
		}
		foreach (KeyValuePair<string, Statistic> stat in Stats)
		{
			StringBuilder stringBuilder9 = Event.NewStringBuilder();
			bool flag = false;
			stringBuilder9.AppendFormat("<stat Name=\"{0}\"", stat.Value.Name);
			if (stat.Value.Boost != 0)
			{
				flag = true;
				stringBuilder9.AppendFormat(" {0}=\"{1}\"", "Boost", stat.Value.Boost);
			}
			if (stat.Value.Min > 0)
			{
				flag = true;
				stringBuilder9.AppendFormat(" {0}=\"{1}\"", "Min", stat.Value.Min);
			}
			if (stat.Value.Max > 0 && stat.Value.Max != int.MaxValue)
			{
				flag = true;
				stringBuilder9.AppendFormat(" {0}=\"{1}\"", "Max", stat.Value.Max);
			}
			if (stat.Value.BaseValue > 0)
			{
				flag = true;
				stringBuilder9.AppendFormat(" {0}=\"{1}\"", "Value", stat.Value.BaseValue);
			}
			if (!string.IsNullOrEmpty(stat.Value.sValue))
			{
				flag = true;
				stringBuilder9.AppendFormat(" {0}=\"{1}\"", "sValue", EscapeXML(stat.Value.sValue));
			}
			stringBuilder9.Append(" />");
			if (flag)
			{
				dictionary.Add("stat:" + stat.Value.Name, stringBuilder9);
			}
		}
		int num = 0;
		if (Inventory != null)
		{
			foreach (InventoryObject item2 in Inventory)
			{
				StringBuilder stringBuilder10 = Event.NewStringBuilder();
				stringBuilder10.AppendFormat("<inventoryobject Blueprint=\"{0}\"", EscapeXML(item2.Blueprint));
				stringBuilder10.AppendFormat(" Number=\"{0}\"", EscapeXML(item2.Number));
				stringBuilder10.AppendFormat(" Chance=\"{0}\"", item2.Chance);
				if (item2.NoEquip)
				{
					stringBuilder10.AppendFormat(" NoEquip=\"true\"");
				}
				if (item2.NoSell)
				{
					stringBuilder10.AppendFormat(" NoSell=\"true\"");
				}
				if (item2.NotReal)
				{
					stringBuilder10.AppendFormat(" NotReal=\"true\"");
				}
				if (item2.Full)
				{
					stringBuilder10.AppendFormat(" Full=\"true\"");
				}
				if (item2.CellChance.HasValue)
				{
					stringBuilder10.AppendFormat(" CellChance=\"{0}\"", item2.CellChance);
				}
				if (!string.IsNullOrEmpty(item2.CellType))
				{
					stringBuilder10.AppendFormat(" CellType=\"{0}\"", item2.CellType);
				}
				if (item2.StringProperties != null)
				{
					stringBuilder10.Append(" StringProperties=\"");
					bool flag2 = true;
					foreach (KeyValuePair<string, string> stringProperty in item2.StringProperties)
					{
						if (!flag2)
						{
							stringBuilder10.Append(",");
						}
						flag2 = false;
						stringBuilder10.AppendFormat("{0}:{1}", EscapeXML(stringProperty.Key), EscapeXML(stringProperty.Value));
					}
					stringBuilder10.Append("\"");
				}
				if (item2.IntProperties != null)
				{
					stringBuilder10.Append(" IntProperties=\"");
					bool flag3 = true;
					foreach (KeyValuePair<string, int> intProperty in item2.IntProperties)
					{
						if (!flag3)
						{
							stringBuilder10.Append(",");
						}
						flag3 = false;
						stringBuilder10.AppendFormat("{0}:{1}", EscapeXML(intProperty.Key), intProperty.Value);
					}
					stringBuilder10.Append("\"");
				}
				if (item2.BoostModChance)
				{
					stringBuilder10.Append(" BoostModChance=\"true\"");
				}
				if (item2.SetMods > 0)
				{
					stringBuilder10.Append(" SetMods=\"" + item2.SetMods + "\"");
				}
				if (item2.AutoMod != null)
				{
					stringBuilder10.Append(" AutoMod=\"" + item2.AutoMod + "\"");
				}
				stringBuilder10.Append(" />");
				dictionary.Add("inventory:" + num++, stringBuilder10);
			}
		}
		StringBuilder stringBuilder11 = Event.NewStringBuilder();
		stringBuilder11.AppendFormat("<object Name=\"{0}\"", Name);
		if (!string.IsNullOrEmpty(Inherits))
		{
			stringBuilder11.AppendFormat(" Inherits=\"{0}\"", Inherits);
		}
		stringBuilder11.Append(">").AppendLine();
		List<string> list = dictionary.Keys.ToList();
		List<string> prioParts = new List<string> { "tag:BaseObject", "apart:Physics", "apart:Render", "apart:Description" };
		list.Sort((string k1, string k2) =>
		{
			int num2 = prioParts.FindIndex((string text) => text == k1);
			int num3 = prioParts.FindIndex((string text) => text == k2);
			if (num2 == -1)
			{
				if (num3 == -1)
				{
					return string.Compare(k1, k2);
				}
				return 1;
			}
			return (num3 == -1) ? (-1) : (num2 - num3);
		});
		foreach (string item3 in list)
		{
			stringBuilder11.Append("  ").Append(dictionary[item3]).AppendLine();
		}
		stringBuilder11.AppendLine("</object>");
		return stringBuilder11.ToString();
	}
}
