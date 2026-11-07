using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRL.UI;

namespace XRL.World;

[HasGameBasedStaticCache]
[HasModSensitiveStaticCache]
public static class Factions
{
	[GameBasedStaticCache(true, false, CreateInstance = false)]
	private static Dictionary<string, Faction> FactionTable;

	[GameBasedStaticCache(true, false, CreateInstance = false)]
	private static List<Faction> FactionList;

	private static bool Initializing;

	public static void CheckInit()
	{
		if (FactionTable == null)
		{
			Loading.LoadTask("Loading Factions.xml", Init, showToUser: false);
		}
	}

	public static Faction GetZoneHolyFaction(string ZoneID)
	{
		CheckInit();
		foreach (Faction faction in FactionList)
		{
			if (faction.HolyPlaces != null && faction.HolyPlaces.Contains(ZoneID))
			{
				return faction;
			}
		}
		return null;
	}

	public static bool Exists(string Name)
	{
		if (Name == null)
		{
			return false;
		}
		CheckInit();
		if (FactionTable.TryGetValue(Name, out var _))
		{
			return true;
		}
		return false;
	}

	public static Faction Get(string Name)
	{
		CheckInit();
		if (FactionTable.TryGetValue(Name, out var value))
		{
			if (value == null)
			{
				MetricsManager.LogError("faction table was out of sync for faction name" + Name);
				FactionTable[Name] = new Faction(Name);
				return FactionTable[Name];
			}
			return value;
		}
		foreach (Faction faction in FactionList)
		{
			if (faction.DisplayName == Name)
			{
				return faction;
			}
		}
		throw new Exception("unknown faction \"" + Name + "\"");
	}

	[Obsolete("use Get(), will be removed after Q2 2024")]
	public static Faction get(string Name)
	{
		return Get(Name);
	}

	public static Faction GetIfExists(string Name)
	{
		if (Name == null)
		{
			return null;
		}
		CheckInit();
		if (FactionTable.TryGetValue(Name, out var value))
		{
			return value;
		}
		return null;
	}

	public static Faction GetMostLiked(bool VisibleOnly = true)
	{
		Faction result = GetIfExists("Beasts");
		float num = float.MinValue;
		foreach (KeyValuePair<string, float> reputationValue in The.Game.PlayerReputation.ReputationValues)
		{
			Faction ifExists = GetIfExists(reputationValue.Key);
			if (ifExists != null && (!VisibleOnly || ifExists.Visible) && reputationValue.Value > num)
			{
				result = ifExists;
				num = reputationValue.Value;
			}
		}
		return result;
	}

	public static string GetMostLikedFormattedName(bool VisibleOnly = true)
	{
		return GetMostLiked(VisibleOnly).GetFormattedName();
	}

	public static Faction GetMostHated(bool VisibleOnly = true)
	{
		Faction result = GetIfExists("Pariahs");
		float num = float.MaxValue;
		foreach (KeyValuePair<string, float> reputationValue in The.Game.PlayerReputation.ReputationValues)
		{
			Faction ifExists = GetIfExists(reputationValue.Key);
			if (ifExists != null && (!VisibleOnly || ifExists.Visible) && reputationValue.Value < num)
			{
				result = ifExists;
				num = reputationValue.Value;
			}
		}
		return result;
	}

	public static Faction GetSecondMostHated(bool VisibleOnly = true)
	{
		Faction faction = GetIfExists("Pariahs");
		float num = float.MaxValue;
		Faction result = faction;
		float num2 = num;
		foreach (KeyValuePair<string, float> reputationValue in The.Game.PlayerReputation.ReputationValues)
		{
			Faction ifExists = GetIfExists(reputationValue.Key);
			if (ifExists != null && (!VisibleOnly || ifExists.Visible))
			{
				float value = reputationValue.Value;
				if (value < num)
				{
					result = faction;
					num2 = num;
					faction = ifExists;
					num = value;
				}
				else if (value < num2)
				{
					result = ifExists;
					num2 = value;
				}
			}
		}
		return result;
	}

	public static string GetMostHatedFormattedName(bool VisibleOnly = true)
	{
		return GetMostHated(VisibleOnly).GetFormattedName();
	}

	[Obsolete("use GetIfExists(), will be removed after Q2 2024")]
	public static Faction getIfExists(string Name)
	{
		return GetIfExists(Name);
	}

	public static Faction GetByDisplayName(string Name)
	{
		if (Name == null)
		{
			return null;
		}
		CheckInit();
		foreach (Faction faction in FactionList)
		{
			if (faction.DisplayName == Name)
			{
				return faction;
			}
		}
		return null;
	}

	public static bool TryGet(string Name, out Faction Faction)
	{
		if (Name == null)
		{
			Faction = null;
			return false;
		}
		CheckInit();
		return FactionTable.TryGetValue(Name, out Faction);
	}

	public static IEnumerable<Faction> Loop()
	{
		CheckInit();
		foreach (Faction faction in FactionList)
		{
			yield return faction;
		}
	}

	[Obsolete("use Loop(), will be removed after Q2 2024")]
	public static IEnumerable<Faction> loop()
	{
		CheckInit();
		foreach (Faction faction in FactionList)
		{
			yield return faction;
		}
	}

	public static IReadOnlyList<Faction> GetList()
	{
		CheckInit();
		return FactionList;
	}

	public static List<string> GetFactionNames()
	{
		CheckInit();
		return new List<string>(FactionTable.Keys);
	}

	[Obsolete("use GetFactionNames(), will be removed after Q2 2024")]
	public static List<string> getFactionNames()
	{
		return GetFactionNames();
	}

	public static List<string> GetVisibleFactionNames()
	{
		CheckInit();
		return (from kv in FactionTable
			where kv.Value.Visible
			select kv.Key).ToList();
	}

	[Obsolete("use GetVisibleFactionNames(), will be removed after Q2 2024")]
	public static List<string> getVisibleFactionNames()
	{
		return GetVisibleFactionNames();
	}

	public static int GetFactionCount()
	{
		CheckInit();
		return FactionList.Count;
	}

	[Obsolete("use GetFactionCount(), will be removed after Q2 2024")]
	public static int getFactionCount()
	{
		return GetFactionCount();
	}

	public static void AddNewFaction(Faction NewFaction)
	{
		CheckInit();
		FactionTable.Add(NewFaction.Name, NewFaction);
		FactionList.Add(NewFaction);
	}

	public static void Load(SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		if (FactionTable == null)
		{
			FactionTable = new Dictionary<string, Faction>(num);
		}
		if (FactionList == null)
		{
			FactionList = new List<Faction>();
		}
		FactionTable.Clear();
		FactionTable.EnsureCapacity(num);
		FactionList.Clear();
		FactionList.EnsureCapacity(num);
		for (int i = 0; i < num; i++)
		{
			string key = Reader.ReadString();
			Faction faction = Reader.ReadComposite<Faction>();
			FactionTable[key] = faction;
			FactionList.Add(faction);
		}
		Loading.LoadTask("Loading Factions.xml", LoadXml, showToUser: false);
		InitAttitudes();
	}

	public static void Save(SerializationWriter writer)
	{
		CheckInit();
		writer.Write(FactionTable.Count);
		foreach (KeyValuePair<string, Faction> item in FactionTable)
		{
			writer.Write(item.Key);
			writer.WriteComposite(item.Value);
		}
	}

	private static void Init()
	{
		Initializing = true;
		FactionTable = new Dictionary<string, Faction>(64);
		FactionList = new List<Faction>(64);
		try
		{
			LoadXml();
			InitAttitudes();
		}
		finally
		{
			Initializing = false;
		}
	}

	private static void InitAttitudes()
	{
		Faction faction = FactionTable["Inanimate"];
		foreach (Faction faction2 in FactionList)
		{
			if (faction2 != faction && faction2.GetFeelingTowardsFaction(faction.Name) != 0)
			{
				faction2.FactionFeeling[faction.Name] = 0;
			}
		}
	}

	public static void LoadXml()
	{
		foreach (DataFile item in DataManager.GetXMLFilesWithRoot("Factions"))
		{
			try
			{
				ProcessFactionXmlFile(item, item.IsMod);
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(item.Mod, message);
			}
		}
	}

	private static bool IsVisible(Faction Faction)
	{
		return Faction.Visible;
	}

	private static bool IsVisibleAndIsOld(Faction Faction)
	{
		if (Faction.Visible)
		{
			return Faction.Old;
		}
		return false;
	}

	private static bool IsVisibleAndCanBeExtradimensional(Faction Faction)
	{
		if (Faction.Visible)
		{
			return Faction.ExtradimensionalVersions;
		}
		return false;
	}

	private static bool HasAtLeastOneMember(Faction Faction)
	{
		return GameObjectFactory.Factory.AnyFactionMembers(Faction.Name);
	}

	public static Faction GetRandomFaction()
	{
		CheckInit();
		return FactionTable.Values.Where(IsVisible).GetRandomElement();
	}

	public static Faction GetRandomFaction(Predicate<Faction> Filter)
	{
		if (Filter == null)
		{
			return GetRandomFaction();
		}
		CheckInit();
		return FactionTable.Values.Where((Faction f) => IsVisible(f) && Filter(f)).GetRandomElement();
	}

	public static Faction GetRandomFaction(string Exception)
	{
		CheckInit();
		return FactionTable.Values.Where((Faction f) => IsVisible(f) && f.Name != Exception).GetRandomElement();
	}

	public static Faction GetRandomFaction(string[] Exceptions)
	{
		CheckInit();
		return FactionTable.Values.Where((Faction f) => IsVisible(f) && Array.IndexOf(Exceptions, f.Name) == -1).GetRandomElement();
	}

	public static Faction GetRandomPotentiallyExtradimensionalFaction()
	{
		CheckInit();
		return FactionTable.Values.Where(IsVisibleAndCanBeExtradimensional).GetRandomElement();
	}

	public static Faction GetRandomOldFaction()
	{
		CheckInit();
		return FactionTable.Values.Where(IsVisibleAndIsOld).GetRandomElement();
	}

	public static Faction GetRandomOldFaction(string Exception)
	{
		CheckInit();
		return FactionTable.Values.Where((Faction f) => IsVisibleAndIsOld(f) && f.Name != Exception).GetRandomElement();
	}

	public static Faction GetRandomFactionWithAtLeastOneMember()
	{
		return GetRandomFaction(HasAtLeastOneMember);
	}

	public static Faction GetRandomFactionWithAtLeastOneMember(Predicate<Faction> Filter)
	{
		if (Filter == null)
		{
			return GetRandomFactionWithAtLeastOneMember();
		}
		return GetRandomFaction((Faction f) => HasAtLeastOneMember(f) && Filter(f));
	}

	public static int GetFeelingFactionToFaction(string Faction1, string Faction2)
	{
		if (Faction1 == Faction2)
		{
			return 100;
		}
		try
		{
			return Get(Faction1).GetFeelingTowardsFaction(Faction2);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error with faction " + Faction1, x);
			return 0;
		}
	}

	public static int GetFeelingFactionToObject(string Faction, GameObject Object, IDictionary<string, int> Override = null)
	{
		try
		{
			return Get(Faction).GetFeelingTowardsObject(Object, Override);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error with faction " + Faction, x);
			return 0;
		}
	}

	public static bool IsPettable(string Faction)
	{
		return Get(Faction).Pettable;
	}

	private static void ProcessFactionXmlFile(string File, bool Mod)
	{
		using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(File);
		xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
		while (xmlTextReader.Read())
		{
			if (xmlTextReader.Name == "factions")
			{
				LoadFactionsNode(xmlTextReader, Mod);
			}
			if ((xmlTextReader.IsEmptyElement || xmlTextReader.NodeType == XmlNodeType.EndElement) && xmlTextReader.Name == "factions")
			{
				break;
			}
		}
		xmlTextReader.Close();
	}

	private static void LoadFactionsNode(XmlTextReader Reader, bool Mod = false)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "faction")
			{
				LoadFactionNode(Reader, Mod);
			}
			else if (Reader.Name == "removefaction")
			{
				string attribute = Reader.GetAttribute("Name");
				if (attribute.IsNullOrEmpty())
				{
					throw new Exception("removefaction tag had no Name attribute");
				}
				if (FactionTable.ContainsKey(attribute))
				{
					FactionTable.Remove(attribute);
				}
			}
		}
	}

	private static void LoadFactionNode(XmlTextReader Reader, bool Mod = false)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute.IsNullOrEmpty())
		{
			throw new Exception("faction tag had no Name attribute");
		}
		Faction faction;
		if (FactionTable.ContainsKey(attribute))
		{
			if (!Mod && !Initializing)
			{
				Reader.SkipToEnd();
				return;
			}
			if (Reader.GetAttribute("Load") == "Replace" && Initializing)
			{
				faction = new Faction(attribute);
				FactionTable[attribute] = faction;
			}
			else
			{
				faction = Get(attribute);
			}
		}
		else
		{
			faction = new Faction(attribute);
			AddNewFaction(faction);
		}
		string attribute2 = Reader.GetAttribute("DisplayName");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.DisplayName = attribute2;
		}
		attribute2 = Reader.GetAttribute("Parent");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.SetParent(attribute2);
		}
		attribute2 = Reader.GetAttribute("ExtradimensionalVersions");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.ExtradimensionalVersions = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("FormatWithArticle");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.FormatWithArticle = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("HatesPlayer");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.HatesPlayer = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("Old");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.Old = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("InitialPlayerReputation");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.InitialPlayerReputation = TryInt(attribute2, "player initial reputation");
		}
		attribute2 = Reader.GetAttribute("Pettable");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.Pettable = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("Plural");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.Plural = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("Visible");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.Visible = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("PositiveSound");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.PositiveSound = attribute2;
		}
		attribute2 = Reader.GetAttribute("NegativeSound");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.NegativeSound = attribute2;
		}
		attribute2 = Reader.GetAttribute("HistoricalSignificance");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.HistoricalSignificance = TryInt(attribute2, "historical significance");
		}
		attribute2 = Reader.GetAttribute("DefaultAddress");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.DefaultAddress = attribute2;
		}
		attribute2 = Reader.GetAttribute("RankTerm");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.RankTerm = attribute2;
		}
		attribute2 = Reader.GetAttribute("EmblemTile");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.Emblem.Tile = attribute2;
		}
		attribute2 = Reader.GetAttribute("EmblemTileColor");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.Emblem.TileColor = "&" + attribute2;
		}
		attribute2 = Reader.GetAttribute("EmblemDetailColor");
		if (!attribute2.IsNullOrEmpty())
		{
			faction.Emblem.DetailColor = attribute2[0];
		}
		if (!Reader.IsEmptyElement)
		{
			LoadFactionChildNodes(faction, Reader, Mod);
		}
		faction.FactionFeeling.TryAdd("Player", Reputation.GetFeeling(faction.InitialPlayerReputation));
		faction.InheritParent();
	}

	private static void LoadFactionChildNodes(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "feeling")
				{
					string attribute = Reader.GetAttribute("About");
					if (int.TryParse(Reader.GetAttribute("Value"), out var result))
					{
						Entry.TryAddFactionFeeling(attribute, result);
					}
					else
					{
						MetricsManager.LogError("Invalid faction feeling: " + Entry.Name + " to " + attribute);
					}
				}
				else if (Reader.Name == "partreputation")
				{
					string attribute2 = Reader.GetAttribute("About");
					string attribute3 = Reader.GetAttribute("Value");
					try
					{
						Entry.SetPartReputation(attribute2, Convert.ToInt32(attribute3));
					}
					catch (Exception x)
					{
						MetricsManager.LogException("Error reading faction part reputation", x);
					}
				}
				else if (Reader.Name == "interests")
				{
					LoadInterestsNode(Entry, Reader, Mod);
				}
				else if (Reader.Name == "ranks")
				{
					LoadRanksNode(Entry, Reader, Mod);
				}
				else if (Reader.Name == "wellknownworshippables")
				{
					LoadWellKnownWorshippablesNode(Entry, Reader, Mod);
				}
				else if (Reader.Name == "factionworshipattitudes")
				{
					LoadFactionWorshipAttitudesNode(Entry, Reader, Mod);
				}
				else if (Reader.Name == "property")
				{
					Entry.SetProperty(Reader.GetAttribute("Name"), Reader.GetAttribute("Value"));
				}
				else if (Reader.Name == "intproperty")
				{
					Entry.SetProperty(Reader.GetAttribute("Name"), int.Parse(Reader.GetAttribute("Value")));
				}
				else if (Reader.Name == "floatproperty")
				{
					Entry.SetProperty(Reader.GetAttribute("Name"), float.Parse(Reader.GetAttribute("Value")));
				}
				else if (Reader.Name == "waterritual")
				{
					string attribute4 = Reader.GetAttribute("Liquid");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualLiquid = attribute4;
					}
					attribute4 = Reader.GetAttribute("Skill");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualSkill = attribute4;
					}
					attribute4 = Reader.GetAttribute("SkillCost");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualSkillCost = TryInt(attribute4, "water ritual skill cost");
					}
					attribute4 = Reader.GetAttribute("BuyMostValuableItem");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualBuyMostValuableItem = attribute4.EqualsNoCase("true");
					}
					attribute4 = Reader.GetAttribute("FungusInfect");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualFungusInfect = TryInt(attribute4, "water ritual fungus infect");
					}
					attribute4 = Reader.GetAttribute("HermitOath");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualHermitOath = TryInt(attribute4, "water ritual hermit oath");
					}
					attribute4 = Reader.GetAttribute("SkillPointAmount");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualSkillPointAmount = TryInt(attribute4, "water ritual skill point amount");
					}
					attribute4 = Reader.GetAttribute("SkillPointCost");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualSkillPointCost = TryInt(attribute4, "water ritual skill point cost");
					}
					attribute4 = Reader.GetAttribute("Mutation");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualMutation = attribute4;
					}
					attribute4 = Reader.GetAttribute("MutationCost");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualMutationCost = TryInt(attribute4, "water ritual mutation cost");
					}
					attribute4 = Reader.GetAttribute("Gifts");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualGifts = attribute4;
					}
					attribute4 = Reader.GetAttribute("Items");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualItems = attribute4;
					}
					attribute4 = Reader.GetAttribute("ItemBlueprint");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualItemBlueprint = attribute4;
					}
					attribute4 = Reader.GetAttribute("ItemCost");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualItemCost = TryInt(attribute4, "water ritual item cost");
					}
					attribute4 = Reader.GetAttribute("Blueprints");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualBlueprints = attribute4;
					}
					attribute4 = Reader.GetAttribute("Recipe");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualRecipe = attribute4;
					}
					attribute4 = Reader.GetAttribute("RecipeText");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualRecipeText = attribute4;
					}
					attribute4 = Reader.GetAttribute("RecipeGenotype");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualRecipeGenotype = attribute4;
					}
					attribute4 = Reader.GetAttribute("Join");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualJoin = attribute4.EqualsNoCase("true");
					}
					attribute4 = Reader.GetAttribute("RandomMentalMutation");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualRandomMentalMutation = TryInt(attribute4, "water ritual random mental mutation");
					}
					attribute4 = Reader.GetAttribute("AltBehaviorPart");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltBehaviorPart = attribute4;
					}
					attribute4 = Reader.GetAttribute("AltBehaviorTag");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltBehaviorTag = attribute4;
					}
					attribute4 = Reader.GetAttribute("AltLiquid");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltLiquid = attribute4;
					}
					attribute4 = Reader.GetAttribute("AltSkill");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltSkill = attribute4;
					}
					attribute4 = Reader.GetAttribute("AltSkillCost");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltSkillCost = TryInt(attribute4, "water ritual alt skill cost");
					}
					attribute4 = Reader.GetAttribute("AltGifts");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltGifts = attribute4;
					}
					attribute4 = Reader.GetAttribute("AltItems");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltItems = attribute4;
					}
					attribute4 = Reader.GetAttribute("AltItemBlueprint");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltItemBlueprint = attribute4;
					}
					attribute4 = Reader.GetAttribute("AltItemCost");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltItemCost = TryInt(attribute4, "water ritual alt item cost");
					}
					attribute4 = Reader.GetAttribute("AltSkill");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltSkill = attribute4;
					}
					attribute4 = Reader.GetAttribute("AltSkillCost");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltSkillCost = TryInt(attribute4, "water ritual alt skill cost");
					}
					attribute4 = Reader.GetAttribute("AltBlueprints");
					if (!attribute4.IsNullOrEmpty())
					{
						Entry.WaterRitualAltBlueprints = attribute4;
					}
				}
				else if (Reader.Name == "holyplace")
				{
					string attribute4 = Reader.GetAttribute("ZoneID");
					if (!attribute4.IsNullOrEmpty() && !Entry.HolyPlaces.Contains(attribute4))
					{
						Entry.HolyPlaces.Add(attribute4);
					}
				}
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "faction")
			{
				break;
			}
		}
	}

	private static void LoadInterestsNode(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		string attribute = Reader.GetAttribute("BuyTargetedSecrets");
		if (!attribute.IsNullOrEmpty())
		{
			Entry.BuyTargetedSecrets = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("SellTargetedSecrets");
		if (!attribute.IsNullOrEmpty())
		{
			Entry.SellTargetedSecrets = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("BuyDescription");
		if (!attribute.IsNullOrEmpty())
		{
			Entry.BuyDescription = attribute;
		}
		attribute = Reader.GetAttribute("SellDescription");
		if (!attribute.IsNullOrEmpty())
		{
			Entry.SellDescription = attribute;
		}
		attribute = Reader.GetAttribute("BothDescription");
		if (!attribute.IsNullOrEmpty())
		{
			Entry.BothDescription = attribute;
		}
		attribute = Reader.GetAttribute("Blurb");
		if (!attribute.IsNullOrEmpty())
		{
			Entry.InterestsBlurb = attribute;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element && Reader.Name == "interest")
			{
				LoadInterestNode(Entry, Reader, Mod);
			}
			if ((Reader.IsEmptyElement || Reader.NodeType == XmlNodeType.EndElement) && Reader.Name == "interests")
			{
				break;
			}
		}
	}

	private static void LoadInterestNode(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		FactionInterest factionInterest = new FactionInterest();
		factionInterest.Tags = Reader.GetAttribute("Tags");
		string attribute = Reader.GetAttribute("WillBuy");
		if (!attribute.IsNullOrEmpty())
		{
			factionInterest.WillBuy = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("WillSell");
		if (!attribute.IsNullOrEmpty())
		{
			factionInterest.WillSell = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("MatchAny");
		if (!attribute.IsNullOrEmpty())
		{
			factionInterest.MatchAny = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("Inverse");
		if (!attribute.IsNullOrEmpty())
		{
			factionInterest.Inverse = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("Description");
		if (!attribute.IsNullOrEmpty())
		{
			factionInterest.Description = attribute;
		}
		attribute = Reader.GetAttribute("Weight");
		if (!attribute.IsNullOrEmpty() && int.TryParse(attribute, out var result))
		{
			factionInterest.Weight = result;
		}
		factionInterest.SourceFileName = "Factions.xml";
		factionInterest.SourceLineNumber = Reader.LineNumber;
		factionInterest.SourceWasMod = Mod;
		Entry.AddInterestIfNew(factionInterest);
	}

	private static void LoadRanksNode(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element && Reader.Name == "rank")
			{
				LoadRankNode(Entry, Reader, Mod);
			}
			if ((Reader.IsEmptyElement || Reader.NodeType == XmlNodeType.EndElement) && Reader.Name == "ranks")
			{
				break;
			}
		}
	}

	private static void LoadRankNode(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		string attribute = Reader.GetAttribute("Name");
		if (!attribute.IsNullOrEmpty())
		{
			if (!Entry.Ranks.Contains(attribute))
			{
				Entry.Ranks.Add(attribute);
			}
			string attribute2 = Reader.GetAttribute("Standing");
			if (!attribute2.IsNullOrEmpty())
			{
				Entry.RankStandings[attribute] = TryInt(attribute2, "rank standing");
			}
			else
			{
				Entry.RankStandings[attribute] = Entry.GetHighestStanding() + 1;
			}
		}
	}

	private static void LoadWellKnownWorshippablesNode(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "wellknownworshippable")
				{
					LoadWellKnownWorshippableNode(Entry, Reader, Mod);
				}
				else if (Reader.Name == "removewellknownworshippable")
				{
					LoadRemoveWellKnownWorshippableNode(Entry, Reader, Mod);
				}
			}
			if ((Reader.IsEmptyElement || Reader.NodeType == XmlNodeType.EndElement) && Reader.Name == "wellknownworshippables")
			{
				break;
			}
		}
	}

	private static void LoadWellKnownWorshippableNode(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		string attribute = Reader.GetAttribute("Name");
		if (!attribute.IsNullOrEmpty())
		{
			Worshippable worshippable = Entry.FindWorshippable(attribute);
			if (worshippable == null)
			{
				worshippable = new Worshippable();
				worshippable.Name = attribute;
				worshippable.Faction = Entry.Name;
				Entry.Worshippables.Add(worshippable);
			}
			string attribute2 = Reader.GetAttribute("Power");
			if (!attribute2.IsNullOrEmpty())
			{
				worshippable.Power = TryInt(attribute2, "worship power");
			}
		}
	}

	private static void LoadRemoveWellKnownWorshippableNode(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		string attribute = Reader.GetAttribute("Name");
		if (!attribute.IsNullOrEmpty())
		{
			Worshippable worshippable = Entry.FindWorshippable(attribute);
			if (worshippable != null)
			{
				Entry.Worshippables.Remove(worshippable);
			}
		}
	}

	private static void LoadFactionWorshipAttitudesNode(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		string attribute = Reader.GetAttribute("Default");
		if (attribute != null)
		{
			if (attribute == "")
			{
				Entry.DefaultFactionWorshipAttitude = 0;
				Entry.DefaultFactionWorshipAttitudeSet = false;
			}
			else
			{
				Entry.DefaultFactionWorshipAttitude = TryInt(attribute, "default faction worship attitude");
				Entry.DefaultFactionWorshipAttitudeSet = true;
			}
		}
		string attribute2 = Reader.GetAttribute("ApplyDefaultAfterSpecificFeelings");
		if (!attribute2.IsNullOrEmpty())
		{
			Entry.ApplyDefaultFactionWorshipAttitudeAfterSpecificFeelings = attribute2.EqualsNoCase("true");
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "factionworshipattitude")
				{
					LoadFactionWorshipAttitudeNode(Entry, Reader, Mod);
				}
				else if (Reader.Name == "removefactionworshipattitude")
				{
					LoadRemoveFactionWorshipAttitudeNode(Entry, Reader, Mod);
				}
			}
			if ((Reader.IsEmptyElement || Reader.NodeType == XmlNodeType.EndElement) && Reader.Name == "factionworshipattitudes")
			{
				break;
			}
		}
	}

	private static void LoadFactionWorshipAttitudeNode(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		string attribute = Reader.GetAttribute("Name");
		if (!attribute.IsNullOrEmpty())
		{
			Entry.FactionWorshipAttitudes[attribute] = TryInt(Reader.GetAttribute("Attitude"), "faction worship attitude");
		}
	}

	private static void LoadRemoveFactionWorshipAttitudeNode(Faction Entry, XmlTextReader Reader, bool Mod = false)
	{
		string attribute = Reader.GetAttribute("Name");
		if (!attribute.IsNullOrEmpty())
		{
			Entry.FactionWorshipAttitudes.Remove(attribute);
		}
	}

	private static int TryInt(string Spec, string What)
	{
		try
		{
			return Convert.ToInt32(Spec);
		}
		catch
		{
			MetricsManager.LogError("Error in " + What + ": " + Spec);
		}
		return -1;
	}

	public static void RequireCachedHeirlooms()
	{
		foreach (Faction item in Loop())
		{
			item.RequireCachedHeirloom();
		}
	}

	public static void RegisterWorshippable(GameObject Object)
	{
		if (GameObject.Validate(ref Object) && Object.HasPropertyOrTag("Worshippable") && !Object.HasIntProperty("WorshipRegistered"))
		{
			string text = Object.GetPropertyOrTag("WorshipFaction") ?? Object.GetPrimaryFaction();
			if (!text.IsNullOrEmpty())
			{
				GetIfExists(text)?.RegisterWorshippable(Object);
			}
		}
	}

	public static Worshippable RegisterWorshippable(string Name, string Faction, int Power = 0)
	{
		return GetIfExists(Faction)?.RegisterWorshippable(Name, Power);
	}

	public static List<Worshippable> GetWorshippables()
	{
		List<Worshippable> list = new List<Worshippable>();
		foreach (Faction item in Loop())
		{
			list.AddRange(item.GetWorshippables());
		}
		list.Sort(Worshippable.Sort);
		return list;
	}

	public static Worshippable FindWorshippable(GameObject Object)
	{
		if (!GameObject.Validate(ref Object))
		{
			return null;
		}
		if (!Object.HasPropertyOrTag("Worshippable"))
		{
			return null;
		}
		string text = Object.GetPropertyOrTag("WorshipFaction") ?? Object.GetPrimaryFaction();
		if (text.IsNullOrEmpty())
		{
			return null;
		}
		return GetIfExists(text)?.FindWorshippable(Object);
	}

	public static Worshippable FindWorshippable(string Faction, string Name)
	{
		return GetIfExists(Faction)?.FindWorshippable(Name);
	}
}
