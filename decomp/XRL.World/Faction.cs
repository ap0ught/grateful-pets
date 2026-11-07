using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Occult.Engine.CodeGeneration;
using Qud.API;
using UnityEngine;
using XRL.Annals;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
[GenerateSerializationPartial]
public class Faction : IComposite
{
	public const string HEIRLOOM_TIER = "5-6";

	public int ID;

	public FactionEmblem Emblem = new FactionEmblem();

	public bool Visible = true;

	public bool Old = true;

	public bool HatesPlayer;

	public bool Pettable;

	public bool Plural = true;

	public bool ExtradimensionalVersions = true;

	public bool FormatWithArticle;

	public string WaterRitualLiquid;

	public string WaterRitualSkill;

	public int WaterRitualSkillCost = -1;

	public bool WaterRitualBuyMostValuableItem;

	public int WaterRitualFungusInfect = -1;

	public int WaterRitualHermitOath = -1;

	public int WaterRitualSkillPointAmount = -1;

	public int WaterRitualSkillPointCost = -1;

	public string WaterRitualMutation;

	public int WaterRitualMutationCost = -1;

	public string WaterRitualGifts;

	public string WaterRitualItems;

	public string WaterRitualItemBlueprint;

	public int WaterRitualItemCost = -1;

	public string WaterRitualBlueprints;

	public string WaterRitualRecipe;

	public string WaterRitualRecipeText;

	public string WaterRitualRecipeGenotype;

	public bool WaterRitualJoin = true;

	public int WaterRitualRandomMentalMutation = -1;

	public string WaterRitualAltBehaviorPart;

	public string WaterRitualAltBehaviorTag;

	public string WaterRitualAltLiquid;

	public string WaterRitualAltSkill;

	public int WaterRitualAltSkillCost = -1;

	public string WaterRitualAltGifts;

	public string WaterRitualAltItems;

	public string WaterRitualAltItemBlueprint;

	public int WaterRitualAltItemCost = -1;

	public string WaterRitualAltBlueprints;

	public string Name = "?";

	public string _DisplayName;

	public string PositiveSound = "sfx_reputation_base_positive";

	public string NegativeSound = "sfx_reputation_base_negative";

	public int HistoricalSignificance;

	/// <summary>The associated historic entity.</summary>
	public string EntityID;

	/// <summary>The founding historic event.</summary>
	public long EventID;

	public string DefaultAddress;

	public string RankTerm = "rank";

	public List<string> Ranks = new List<string>();

	public Dictionary<string, int> RankStandings = new Dictionary<string, int>();

	public List<Worshippable> Worshippables = new List<Worshippable>();

	public int DefaultFactionWorshipAttitude;

	public bool DefaultFactionWorshipAttitudeSet;

	public bool ApplyDefaultFactionWorshipAttitudeAfterSpecificFeelings;

	public Dictionary<string, int> FactionWorshipAttitudes = new Dictionary<string, int>();

	public List<MemorialTracking> QueuedMemorials;

	public List<MemorialTracking> PerformedMemorials;

	public Dictionary<string, object> Properties;

	public Dictionary<string, int> IntProperties;

	public string Parent;

	public Dictionary<string, int> FactionFeeling = new Dictionary<string, int>();

	public int InitialPlayerReputation;

	public Dictionary<string, int> PartReputation = new Dictionary<string, int>();

	public List<FactionInterest> Interests = new List<FactionInterest>();

	public List<string> HolyPlaces = new List<string>();

	public bool BuyTargetedSecrets = true;

	public bool SellTargetedSecrets = true;

	public string BuyDescription;

	public string SellDescription;

	public string BothDescription;

	public string InterestsBlurb;

	private string _TargetedSecretString;

	private string _GossipSecretString;

	private string _NoBuySecretString;

	private string _NoSellSecretString;

	[NonSerialized]
	private static List<string> BuyOnly = new List<string>(8);

	[NonSerialized]
	private static List<string> SellOnly = new List<string>(8);

	[NonSerialized]
	private static List<string> Both = new List<string>(8);

	private static List<GameObjectBlueprint> MemberCache = new List<GameObjectBlueprint>();

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	public string DisplayName
	{
		get
		{
			if (_DisplayName == null)
			{
				_DisplayName = Name;
			}
			if (Name != null && Name.StartsWith("SultanCult") && The.Game != null)
			{
				_DisplayName = The.Game.GetStringGameState("CultDisplayName_" + Name, Name);
			}
			return _DisplayName;
		}
		set
		{
			_DisplayName = value;
		}
	}

	public string TargetedSecretString => _TargetedSecretString ?? (_TargetedSecretString = "include:" + Name);

	public string GossipSecretString => _GossipSecretString ?? (_GossipSecretString = "gossip:" + Name);

	public string NoBuySecretString => _NoBuySecretString ?? (_NoBuySecretString = "nobuy:" + Name);

	public string NoSellSecretString => _NoSellSecretString ?? (_NoSellSecretString = "nosell:" + Name);

	public string Heirloom
	{
		get
		{
			if (!The.Game.HasStringGameState("Heirloom_" + Name))
			{
				Stat.ReseedFrom("Heirloom_" + Name);
				List<string> list = new List<string>(Items.ItemTableNames.Keys);
				The.Game.SetStringGameState("Heirloom_" + Name, list.GetRandomElement());
			}
			return The.Game.GetStringGameState("Heirloom_" + Name);
		}
		set
		{
			The.Game.SetStringGameState("Heirloom_" + Name, value);
		}
	}

	public string HeirloomID
	{
		get
		{
			return The.Game.GetStringGameState("HeirloomID_" + Name);
		}
		set
		{
			if (value != null)
			{
				The.Game.SetStringGameState("HeirloomID_" + Name, value);
			}
			else
			{
				The.Game.RemoveStringGameState("HeirloomID_" + Name);
			}
		}
	}

	public static Reputation PlayerReputation => The.Game?.PlayerReputation;

	public int CurrentReputation => The.Game.PlayerReputation.Get(this);

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(ID);
		Writer.Write(Emblem);
		Writer.Write(Visible);
		Writer.Write(Old);
		Writer.Write(HatesPlayer);
		Writer.Write(Pettable);
		Writer.Write(Plural);
		Writer.Write(ExtradimensionalVersions);
		Writer.Write(FormatWithArticle);
		Writer.WriteOptimized(WaterRitualLiquid);
		Writer.WriteOptimized(WaterRitualSkill);
		Writer.WriteOptimized(WaterRitualSkillCost);
		Writer.Write(WaterRitualBuyMostValuableItem);
		Writer.WriteOptimized(WaterRitualFungusInfect);
		Writer.WriteOptimized(WaterRitualHermitOath);
		Writer.WriteOptimized(WaterRitualSkillPointAmount);
		Writer.WriteOptimized(WaterRitualSkillPointCost);
		Writer.WriteOptimized(WaterRitualMutation);
		Writer.WriteOptimized(WaterRitualMutationCost);
		Writer.WriteOptimized(WaterRitualGifts);
		Writer.WriteOptimized(WaterRitualItems);
		Writer.WriteOptimized(WaterRitualItemBlueprint);
		Writer.WriteOptimized(WaterRitualItemCost);
		Writer.WriteOptimized(WaterRitualBlueprints);
		Writer.WriteOptimized(WaterRitualRecipe);
		Writer.WriteOptimized(WaterRitualRecipeText);
		Writer.WriteOptimized(WaterRitualRecipeGenotype);
		Writer.Write(WaterRitualJoin);
		Writer.WriteOptimized(WaterRitualRandomMentalMutation);
		Writer.WriteOptimized(WaterRitualAltBehaviorPart);
		Writer.WriteOptimized(WaterRitualAltBehaviorTag);
		Writer.WriteOptimized(WaterRitualAltLiquid);
		Writer.WriteOptimized(WaterRitualAltSkill);
		Writer.WriteOptimized(WaterRitualAltSkillCost);
		Writer.WriteOptimized(WaterRitualAltGifts);
		Writer.WriteOptimized(WaterRitualAltItems);
		Writer.WriteOptimized(WaterRitualAltItemBlueprint);
		Writer.WriteOptimized(WaterRitualAltItemCost);
		Writer.WriteOptimized(WaterRitualAltBlueprints);
		Writer.WriteOptimized(Name);
		Writer.WriteOptimized(_DisplayName);
		Writer.WriteOptimized(PositiveSound);
		Writer.WriteOptimized(NegativeSound);
		Writer.WriteOptimized(HistoricalSignificance);
		Writer.WriteOptimized(EntityID);
		Writer.WriteOptimized(EventID);
		Writer.WriteOptimized(DefaultAddress);
		Writer.WriteOptimized(RankTerm);
		Writer.Write(Ranks);
		Writer.Write(RankStandings);
		Writer.WriteComposite(Worshippables);
		Writer.WriteOptimized(DefaultFactionWorshipAttitude);
		Writer.Write(DefaultFactionWorshipAttitudeSet);
		Writer.Write(ApplyDefaultFactionWorshipAttitudeAfterSpecificFeelings);
		Writer.Write(FactionWorshipAttitudes);
		Writer.WriteComposite(QueuedMemorials);
		Writer.WriteComposite(PerformedMemorials);
		Writer.Write(Properties);
		Writer.Write(IntProperties);
		Writer.WriteOptimized(Parent);
		Writer.Write(FactionFeeling);
		Writer.WriteOptimized(InitialPlayerReputation);
		Writer.Write(PartReputation);
		Writer.WriteComposite(Interests);
		Writer.Write(HolyPlaces);
		Writer.Write(BuyTargetedSecrets);
		Writer.Write(SellTargetedSecrets);
		Writer.WriteOptimized(BuyDescription);
		Writer.WriteOptimized(SellDescription);
		Writer.WriteOptimized(BothDescription);
		Writer.WriteOptimized(InterestsBlurb);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		ID = Reader.ReadOptimizedInt32();
		Emblem = (FactionEmblem)Reader.ReadComposite();
		Visible = Reader.ReadBoolean();
		Old = Reader.ReadBoolean();
		HatesPlayer = Reader.ReadBoolean();
		Pettable = Reader.ReadBoolean();
		Plural = Reader.ReadBoolean();
		ExtradimensionalVersions = Reader.ReadBoolean();
		FormatWithArticle = Reader.ReadBoolean();
		WaterRitualLiquid = Reader.ReadOptimizedString();
		WaterRitualSkill = Reader.ReadOptimizedString();
		WaterRitualSkillCost = Reader.ReadOptimizedInt32();
		WaterRitualBuyMostValuableItem = Reader.ReadBoolean();
		WaterRitualFungusInfect = Reader.ReadOptimizedInt32();
		WaterRitualHermitOath = Reader.ReadOptimizedInt32();
		WaterRitualSkillPointAmount = Reader.ReadOptimizedInt32();
		WaterRitualSkillPointCost = Reader.ReadOptimizedInt32();
		WaterRitualMutation = Reader.ReadOptimizedString();
		WaterRitualMutationCost = Reader.ReadOptimizedInt32();
		WaterRitualGifts = Reader.ReadOptimizedString();
		WaterRitualItems = Reader.ReadOptimizedString();
		WaterRitualItemBlueprint = Reader.ReadOptimizedString();
		WaterRitualItemCost = Reader.ReadOptimizedInt32();
		WaterRitualBlueprints = Reader.ReadOptimizedString();
		WaterRitualRecipe = Reader.ReadOptimizedString();
		WaterRitualRecipeText = Reader.ReadOptimizedString();
		WaterRitualRecipeGenotype = Reader.ReadOptimizedString();
		WaterRitualJoin = Reader.ReadBoolean();
		WaterRitualRandomMentalMutation = Reader.ReadOptimizedInt32();
		WaterRitualAltBehaviorPart = Reader.ReadOptimizedString();
		WaterRitualAltBehaviorTag = Reader.ReadOptimizedString();
		WaterRitualAltLiquid = Reader.ReadOptimizedString();
		WaterRitualAltSkill = Reader.ReadOptimizedString();
		WaterRitualAltSkillCost = Reader.ReadOptimizedInt32();
		WaterRitualAltGifts = Reader.ReadOptimizedString();
		WaterRitualAltItems = Reader.ReadOptimizedString();
		WaterRitualAltItemBlueprint = Reader.ReadOptimizedString();
		WaterRitualAltItemCost = Reader.ReadOptimizedInt32();
		WaterRitualAltBlueprints = Reader.ReadOptimizedString();
		Name = Reader.ReadOptimizedString();
		_DisplayName = Reader.ReadOptimizedString();
		PositiveSound = Reader.ReadOptimizedString();
		NegativeSound = Reader.ReadOptimizedString();
		HistoricalSignificance = Reader.ReadOptimizedInt32();
		EntityID = Reader.ReadOptimizedString();
		EventID = Reader.ReadOptimizedInt64();
		DefaultAddress = Reader.ReadOptimizedString();
		RankTerm = Reader.ReadOptimizedString();
		Ranks = Reader.ReadList<string>();
		RankStandings = Reader.ReadDictionary<string, int>();
		Worshippables = Reader.ReadCompositeList<Worshippable>();
		DefaultFactionWorshipAttitude = Reader.ReadOptimizedInt32();
		DefaultFactionWorshipAttitudeSet = Reader.ReadBoolean();
		ApplyDefaultFactionWorshipAttitudeAfterSpecificFeelings = Reader.ReadBoolean();
		FactionWorshipAttitudes = Reader.ReadDictionary<string, int>();
		QueuedMemorials = Reader.ReadCompositeList<MemorialTracking>();
		PerformedMemorials = Reader.ReadCompositeList<MemorialTracking>();
		Properties = Reader.ReadDictionary<string, object>();
		IntProperties = Reader.ReadDictionary<string, int>();
		Parent = Reader.ReadOptimizedString();
		FactionFeeling = Reader.ReadDictionary<string, int>();
		InitialPlayerReputation = Reader.ReadOptimizedInt32();
		PartReputation = Reader.ReadDictionary<string, int>();
		Interests = Reader.ReadCompositeList<FactionInterest>();
		HolyPlaces = Reader.ReadList<string>();
		BuyTargetedSecrets = Reader.ReadBoolean();
		SellTargetedSecrets = Reader.ReadBoolean();
		BuyDescription = Reader.ReadOptimizedString();
		SellDescription = Reader.ReadOptimizedString();
		BothDescription = Reader.ReadOptimizedString();
		InterestsBlurb = Reader.ReadOptimizedString();
	}

	public Faction()
	{
	}

	public Faction(string Name)
		: this()
	{
		this.Name = Name;
	}

	public Faction(string Name = null, bool Visibility = true, string DisplayName = null, bool Old = true, string WaterRitualLiquid = "water")
		: this(Name)
	{
		Visible = Visibility;
		this.Old = Old;
		this.WaterRitualLiquid = WaterRitualLiquid;
		this.DisplayName = DisplayName ?? Name;
	}

	public static string GetFormattedName(string FactionName)
	{
		if (FactionName == "*")
		{
			return "everyone";
		}
		try
		{
			return Factions.Get(FactionName).GetFormattedName();
		}
		catch
		{
			Debug.Log("Failed to get faction: " + FactionName);
			return "";
		}
	}

	[Obsolete("use GetFormattedName(), will be removed after Q2 2024")]
	public static string getFormattedName(string factionName)
	{
		return GetFormattedName(factionName);
	}

	public static string GetFormattedGroupName(string FactionName)
	{
		try
		{
			string text = GetFormattedName(FactionName);
			if (text.StartsWith("the ") || text.StartsWith("The "))
			{
				text = text.Substring(4);
			}
			return text;
		}
		catch
		{
			Debug.Log("Failed to get faction: " + FactionName);
			return "";
		}
	}

	[Obsolete("use GetFormattedGroupName(), will be removed after Q2 2024")]
	public static string getFormattedGroupName(string FactionName)
	{
		return GetFormattedGroupName(FactionName);
	}

	public string GetFeelingText()
	{
		string text = Markup.Color("C", ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(GetFormattedName())) + " ";
		return ((The.Game != null) ? PlayerReputation.GetAttitude(Name) : 0) switch
		{
			-2 => text + (Plural ? "despise" : "despises") + " you. Even docile " + (Plural ? "ones" : "members") + " will attack you.", 
			-1 => text + (Plural ? "dislike" : "dislikes") + " you, but docile " + (Plural ? "ones" : "members") + " won't attack you.", 
			1 => text + (Plural ? "favor" : "favors") + " you. Aggressive " + (Plural ? "ones" : "members") + " won't attack you.", 
			2 => text + (Plural ? "revere" : "reveres") + " you and " + (Plural ? "consider" : "considers") + " you one of their own.", 
			_ => text + (Plural ? "don't" : "doesn't") + " care about you, but aggressive " + (Plural ? "ones" : "members") + " will attack you.", 
		};
	}

	public string GetPetText()
	{
		if (!Pettable)
		{
			return string.Empty;
		}
		string text = ((PlayerReputation.GetAttitude(Name) >= 1) ? "will usually let you pet them" : "won't usually let you pet them");
		return Markup.Color("C", ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(GetFormattedName())) + " " + text + ".";
	}

	public string GetRankText()
	{
		if (GlobalConfig.GetBoolSetting("ShowFacitonRank"))
		{
			string text = The.Game?.PlayerReputation?.GetFactionRank(Name);
			if (text != null && !string.IsNullOrEmpty(text))
			{
				return "You hold the " + GetRankTerm() + " of " + text + " among them.";
			}
		}
		return string.Empty;
	}

	public string GetHolyPlaceText()
	{
		if ((PlayerReputation?.GetAttitude(Name) ?? 0) < 1)
		{
			return "You aren't welcome in their holy places.";
		}
		return "You are welcome in their holy places.";
	}

	public static string GetFeelingDescription(string FactionName)
	{
		Faction faction = Factions.Get(FactionName);
		string text = faction.GetFeelingText() + "\n\n";
		string text2 = "";
		string rankText = faction.GetRankText();
		if (rankText != null && !string.IsNullOrEmpty(rankText))
		{
			text = text + text2 + rankText;
			text2 = " ";
		}
		string petText = faction.GetPetText();
		if (petText != null && !string.IsNullOrEmpty(petText))
		{
			text = text + text2 + petText;
			text2 = " ";
		}
		return text + text2 + faction.GetHolyPlaceText();
	}

	public static string GetPreferredSecretDescription(string FactionName)
	{
		Faction faction = Factions.Get(FactionName);
		string formattedName = faction.GetFormattedName();
		formattedName = char.ToUpper(formattedName[0]) + formattedName.Substring(1);
		formattedName = "{{C|" + formattedName + "}}";
		string value = " " + (faction.Plural ? "are" : "is");
		BuyOnly.Clear();
		SellOnly.Clear();
		Both.Clear();
		bool flag = false;
		foreach (FactionInterest interest in faction.Interests)
		{
			if (interest.WillBuy != interest.WillSell && (interest.WillBuy || interest.WillSell))
			{
				flag = true;
				break;
			}
		}
		foreach (FactionInterest interest2 in faction.Interests)
		{
			string description = interest2.GetDescription(faction);
			if (description.IsNullOrEmpty())
			{
				continue;
			}
			if (interest2.WillBuy && interest2.WillSell)
			{
				if (flag)
				{
					BuyOnly.Add(description);
					SellOnly.Add(description);
				}
				else
				{
					Both.Add(description);
				}
			}
			else if (interest2.WillBuy)
			{
				BuyOnly.Add(description);
			}
			else if (interest2.WillSell)
			{
				SellOnly.Add(description);
			}
		}
		string sultanTerm = HistoryAPI.GetSultanTerm();
		string text = Grammar.Pluralize(sultanTerm);
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = faction.HasInterestIn("sultan", Buy: true);
		bool flag5 = faction.HasInterestIn("sultan", Buy: false, Sell: true);
		if (!flag4 || !flag5)
		{
			if (faction.Name.StartsWith("SultanCult") || faction.Name.StartsWith("PlayerCult"))
			{
				if (flag | flag4 | flag5)
				{
					if (!flag4)
					{
						BuyOnly.Add("the " + sultanTerm + " they worship");
					}
					if (!flag5)
					{
						SellOnly.Add("the " + sultanTerm + " they worship");
					}
				}
				else
				{
					Both.Add("the " + sultanTerm + " they worship");
				}
			}
			else
			{
				foreach (JournalSultanNote sultanNote in JournalAPI.GetSultanNotes())
				{
					if (!sultanNote.Has(faction.TargetedSecretString))
					{
						continue;
					}
					if (faction.BuyTargetedSecrets && faction.SellTargetedSecrets && !flag && !flag4 && !flag5 && faction.BuyDescription.IsNullOrEmpty() && faction.SellDescription.IsNullOrEmpty())
					{
						Both.Add(text + " they admire or despise");
						break;
					}
					if (faction.BuyTargetedSecrets && !flag4)
					{
						if (faction.BuyDescription.IsNullOrEmpty())
						{
							BuyOnly.Add(text + " they admire or despise");
						}
						else
						{
							flag2 = true;
						}
					}
					if (faction.SellTargetedSecrets && !flag5)
					{
						if (faction.SellDescription.IsNullOrEmpty())
						{
							SellOnly.Add(text + " they admire or despise");
						}
						else
						{
							flag3 = true;
						}
					}
					break;
				}
			}
		}
		bool flag6 = false;
		if (BuyOnly.Count > 0 && faction.BuyDescription.IsNullOrEmpty() && !faction.HasInterestIn("gossip", Buy: true) && !faction.HasInterestIn(faction.GossipSecretString, Buy: true))
		{
			BuyOnly.Add("gossip that's about them");
			flag6 = true;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (!faction.InterestsBlurb.IsNullOrEmpty())
		{
			stringBuilder.Append(formattedName).Append(faction.InterestsBlurb);
		}
		if (Both.Count > 0)
		{
			stringBuilder.Compound(formattedName, ".\n\n").Append(value).Append(faction.BothDescription.IsNullOrEmpty() ? " interested in trading secrets about " : faction.BothDescription)
				.Append(Grammar.MakeAndList(Both));
		}
		if (SellOnly.Count > 0)
		{
			stringBuilder.Compound(formattedName, ".\n\n").Append(value).Append(faction.SellDescription.IsNullOrEmpty() ? " interested in sharing secrets about " : faction.SellDescription)
				.Append(Grammar.MakeAndList(SellOnly));
		}
		if (BuyOnly.Count > 0)
		{
			stringBuilder.Compound(formattedName, ".\n\n").Append(value).Append(faction.BuyDescription.IsNullOrEmpty() ? " interested in learning about " : faction.BuyDescription)
				.Append(Grammar.MakeAndList(BuyOnly));
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Append('.');
		}
		if (!flag6 && !faction.HasInterestIn("gossip", Buy: true) && !faction.HasInterestIn(faction.GossipSecretString, Buy: true))
		{
			if (stringBuilder.Length == 0)
			{
				stringBuilder.Append(formattedName).Append(value);
			}
			else
			{
				stringBuilder.Append(" They're also");
			}
			if (flag2 & flag3)
			{
				stringBuilder.Append(" interested in trading secrets about " + text + " they admire or despise and hearing gossip that's about them.");
				flag2 = false;
				flag3 = false;
			}
			else if (flag2)
			{
				stringBuilder.Append(" interested in learning about " + text + " they admire or despise and hearing gossip that's about them.");
				flag2 = false;
			}
			else if (flag3)
			{
				stringBuilder.Append(" interested in sharing secrets about " + text + " they admire or despise and hearing gossip that's about them.");
				flag3 = false;
			}
			else
			{
				stringBuilder.Append(" interested in hearing gossip that's about them.");
			}
		}
		if (flag2 | flag3)
		{
			if (stringBuilder.Length == 0)
			{
				stringBuilder.Append(formattedName).Append(value);
			}
			else
			{
				stringBuilder.Append(" They're also");
			}
			if (flag2 & flag3)
			{
				stringBuilder.Append(" interested in trading secrets about " + text + " they admire or despise.");
				flag2 = false;
				flag3 = false;
			}
			else if (flag2)
			{
				stringBuilder.Append(" interested in learning about " + text + " they admire or despise.");
				flag2 = false;
			}
			else if (flag3)
			{
				stringBuilder.Append(" interested in sharing secrets about " + text + " they admire or despise.");
				flag3 = false;
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetRepPageDescription(string Name)
	{
		return GetFeelingDescription(Name) + "\n\n" + GetPreferredSecretDescription(Name);
	}

	public static string GetSultanFactionName(string Period)
	{
		if (Period == "6")
		{
			return "Resheph";
		}
		return "SultanCult" + Period;
	}

	public static string GetSultanFactionName(int Period)
	{
		if (Period == 6)
		{
			return "Resheph";
		}
		return "SultanCult" + Period;
	}

	public static string GetRankTerm(string Name)
	{
		return Factions.GetIfExists(Name)?.GetRankTerm() ?? "rank";
	}

	public static List<string> GetRanks(string Name)
	{
		return Factions.GetIfExists(Name)?.GetRanks();
	}

	public static Dictionary<string, int> GetRankStandings(string Name)
	{
		return Factions.GetIfExists(Name)?.GetRankStandings();
	}

	public static int GetRankStanding(string Name, string Rank)
	{
		return Factions.GetIfExists(Name)?.GetRankStanding(Rank) ?? 0;
	}

	public static string GetRankByStanding(string Name, int Standing)
	{
		return Factions.GetIfExists(Name)?.GetRankByStanding(Standing);
	}

	public static string GetLowestRank(string Name)
	{
		return Factions.GetIfExists(Name)?.GetLowestRank();
	}

	public static string GetHighestRank(string Name)
	{
		return Factions.GetIfExists(Name)?.GetHighestRank();
	}

	public static int GetLowestStanding(string Name)
	{
		return Factions.GetIfExists(Name)?.GetLowestStanding() ?? 0;
	}

	public static int GetHighestStanding(string Name)
	{
		return Factions.GetIfExists(Name)?.GetHighestStanding() ?? 0;
	}

	public static string GetDefaultAddress(string Name)
	{
		return Factions.GetIfExists(Name)?.GetDefaultAddress();
	}

	public string GetFormattedName()
	{
		if (!FormatWithArticle)
		{
			return DisplayName;
		}
		return "the " + DisplayName;
	}

	[Obsolete("Use GetFormattedName(), will be removed after Q2 2024")]
	public string getFormattedName()
	{
		return GetFormattedName();
	}

	public string GetWaterRitualLiquid(GameObject speaker)
	{
		if (WaterRitualAltLiquid.IsNullOrEmpty() || !UseAltBehavior(speaker))
		{
			return WaterRitualLiquid;
		}
		return WaterRitualAltLiquid;
	}

	public string GetRankTerm()
	{
		return RankTerm ?? "rank";
	}

	public List<string> GetRanks()
	{
		return Ranks;
	}

	public Dictionary<string, int> GetRankStandings()
	{
		return RankStandings;
	}

	public int GetRankStanding(string Rank)
	{
		if (RankStandings.TryGetValue(Rank, out var value))
		{
			return value;
		}
		return 0;
	}

	public string GetRankByStanding(int Standing)
	{
		foreach (KeyValuePair<string, int> rankStanding in RankStandings)
		{
			if (rankStanding.Value == Standing)
			{
				return rankStanding.Key;
			}
		}
		return null;
	}

	public string GetLowestRank()
	{
		int num = int.MaxValue;
		string result = null;
		foreach (KeyValuePair<string, int> rankStanding in RankStandings)
		{
			if (rankStanding.Value < num)
			{
				result = rankStanding.Key;
				num = rankStanding.Value;
			}
		}
		return result;
	}

	public string GetHighestRank()
	{
		int num = int.MinValue;
		string result = null;
		foreach (KeyValuePair<string, int> rankStanding in RankStandings)
		{
			if (rankStanding.Value > num)
			{
				result = rankStanding.Key;
				num = rankStanding.Value;
			}
		}
		return result;
	}

	public int GetLowestStanding()
	{
		int num = int.MaxValue;
		foreach (KeyValuePair<string, int> rankStanding in RankStandings)
		{
			if (rankStanding.Value < num)
			{
				num = rankStanding.Value;
			}
		}
		if (num != int.MaxValue)
		{
			return num;
		}
		return 0;
	}

	public int GetHighestStanding()
	{
		int num = int.MinValue;
		foreach (KeyValuePair<string, int> rankStanding in RankStandings)
		{
			if (rankStanding.Value < num)
			{
				num = rankStanding.Value;
			}
		}
		if (num != int.MinValue)
		{
			return num;
		}
		return 0;
	}

	public string GetDefaultAddress()
	{
		return DefaultAddress;
	}

	public object GetProperty(string Key, object Default = null)
	{
		if (Key == null || Properties == null)
		{
			return Default;
		}
		if (!Properties.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return value;
	}

	public T GetProperty<T>(string Key, T Default = null) where T : class
	{
		if (Key == null || Properties == null)
		{
			return Default;
		}
		if (!Properties.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return value as T;
	}

	public string GetStringProperty(string Key, string Default = null)
	{
		if (Key == null || Properties == null)
		{
			return Default;
		}
		if (!Properties.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return value as string;
	}

	public int GetIntProperty(string Key, int Default = 0)
	{
		if (Key == null || IntProperties == null)
		{
			return Default;
		}
		if (!IntProperties.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return value;
	}

	public float GetFloatProperty(string Key, float Default = 0f)
	{
		if (Key == null || IntProperties == null)
		{
			return Default;
		}
		if (!IntProperties.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return BitConverter.Int32BitsToSingle(value);
	}

	public void SetProperty(string Key, object Value)
	{
		if (Properties == null)
		{
			Properties = new Dictionary<string, object>(3);
		}
		Properties[Key] = Value;
	}

	public void SetProperty(string Key, int Value)
	{
		if (IntProperties == null)
		{
			IntProperties = new Dictionary<string, int>(3);
		}
		IntProperties[Key] = Value;
	}

	public void SetProperty(string Key, float Value)
	{
		if (IntProperties == null)
		{
			IntProperties = new Dictionary<string, int>(3);
		}
		IntProperties[Key] = BitConverter.SingleToInt32Bits(Value);
	}

	public bool HasProperty(string Key)
	{
		if (Properties == null || !Properties.ContainsKey(Key))
		{
			if (IntProperties != null)
			{
				return IntProperties.ContainsKey(Key);
			}
			return false;
		}
		return true;
	}

	public bool RemoveProperty(string Key)
	{
		bool flag = false;
		if (Properties != null)
		{
			flag |= Properties.Remove(Key);
		}
		if (IntProperties != null)
		{
			flag |= IntProperties.Remove(Key);
		}
		return flag;
	}

	public bool HasInterestIn(string topics, bool Buy = false, bool Sell = false)
	{
		foreach (FactionInterest interest in Interests)
		{
			if (interest.Includes(topics, Buy, Sell))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryAddFactionFeeling(string Faction, int Feeling)
	{
		return FactionFeeling.TryAdd(Faction, Feeling);
	}

	public int SetFactionFeeling(string faction, int faction_feeling)
	{
		FactionFeeling.Set(faction, faction_feeling);
		return faction_feeling;
	}

	public int SetPartReputation(string part, int feeling)
	{
		PartReputation.Set(part, feeling);
		return feeling;
	}

	public Faction SetParent(string Name)
	{
		Faction faction = Factions.Get(Name);
		Parent = faction.Name;
		InitialPlayerReputation = faction.InitialPlayerReputation;
		DefaultFactionWorshipAttitude = faction.DefaultFactionWorshipAttitude;
		DefaultFactionWorshipAttitudeSet = faction.DefaultFactionWorshipAttitudeSet;
		ApplyDefaultFactionWorshipAttitudeAfterSpecificFeelings = faction.ApplyDefaultFactionWorshipAttitudeAfterSpecificFeelings;
		return faction;
	}

	public void InheritParent()
	{
		if (Parent.IsNullOrEmpty())
		{
			return;
		}
		Faction faction = Factions.Get(Parent);
		string key;
		int value;
		foreach (KeyValuePair<string, int> item in faction.FactionFeeling)
		{
			item.Deconstruct(out key, out value);
			string key2 = key;
			int value2 = value;
			FactionFeeling.TryAdd(key2, value2);
		}
		foreach (KeyValuePair<string, int> factionWorshipAttitude in faction.FactionWorshipAttitudes)
		{
			factionWorshipAttitude.Deconstruct(out key, out value);
			string key3 = key;
			int value3 = value;
			FactionWorshipAttitudes.TryAdd(key3, value3);
		}
	}

	public virtual int GetFeelingTowardsObject(GameObject Object, IDictionary<string, int> Override = null)
	{
		Brain brain = Object.Brain;
		if (brain?._ParentObject == null)
		{
			return 0;
		}
		brain = brain.GetFinalLeaderBrain() ?? brain;
		if (brain.FactionFeelings.TryGetValue(Name, out var Value))
		{
			return Value;
		}
		int value = 0;
		if (brain.Allegiance.IsNullOrEmpty())
		{
			if (!FactionFeeling.TryGetValue("*", out value))
			{
				return 0;
			}
		}
		else
		{
			value = brain.Allegiance.GetBaseFeeling(this, Override);
		}
		if (this == HolyPlaceSystem.LastHolyFaction && value > -50 && value < 50 && brain.ParentObject.IsPlayer())
		{
			value = -50;
		}
		return value;
	}

	public virtual int GetFeelingTowardsFaction(string Faction, int? DefaultBeforeGeneral = null, int DefaultAfterGeneral = 0)
	{
		if (Faction == Name)
		{
			return 100;
		}
		if (FactionFeeling.TryGetValue(Faction, out var value))
		{
			return value;
		}
		if (DefaultBeforeGeneral.HasValue)
		{
			return DefaultBeforeGeneral.Value;
		}
		if (FactionFeeling.TryGetValue("*", out var value2))
		{
			return value2;
		}
		return DefaultAfterGeneral;
	}

	private bool SellRequiresSpecificInterest(IBaseJournalEntry note)
	{
		if (note.Has("onlySellIfTargetedAndInterested"))
		{
			return true;
		}
		return false;
	}

	public bool InterestedIn(IBaseJournalEntry note, bool Buy = false, bool Sell = false)
	{
		return GetInterestIn(note, Buy, Sell) > 0;
	}

	public int GetInterestIn(IBaseJournalEntry note, bool Buy = false, bool Sell = false)
	{
		if (Buy && note.Has(NoBuySecretString))
		{
			return 0;
		}
		if (Sell && note.Has(NoSellSecretString))
		{
			return 0;
		}
		if (Buy)
		{
			if (note.Has(GossipSecretString))
			{
				return note.Weight;
			}
			if (BuyTargetedSecrets && note.Has(TargetedSecretString))
			{
				bool flag = true;
				int num = 0;
				foreach (FactionInterest interest in Interests)
				{
					if (interest.Excludes(note, Buy, Sell))
					{
						flag = false;
						break;
					}
					if (interest.Weight > num)
					{
						num = interest.Weight;
					}
				}
				if (flag)
				{
					return note.Weight + num;
				}
			}
		}
		if (Sell && SellTargetedSecrets && note.Has(TargetedSecretString))
		{
			if (SellRequiresSpecificInterest(note))
			{
				foreach (FactionInterest interest2 in Interests)
				{
					if (interest2.Includes(note, Buy, Sell))
					{
						return note.Weight + interest2.Weight;
					}
				}
			}
			else
			{
				bool flag2 = true;
				int num2 = 0;
				foreach (FactionInterest interest3 in Interests)
				{
					if (interest3.Excludes(note, Buy, Sell))
					{
						flag2 = false;
						break;
					}
					if (interest3.Weight > num2)
					{
						num2 = interest3.Weight;
					}
				}
				if (flag2)
				{
					return note.Weight + num2;
				}
			}
		}
		if (!Sell || !SellRequiresSpecificInterest(note))
		{
			foreach (FactionInterest interest4 in Interests)
			{
				if (interest4.Includes(note, Buy, Sell))
				{
					return note.Weight + interest4.Weight;
				}
			}
		}
		return 0;
	}

	public bool InterestedIn(IBaseJournalEntry note, ref string becauseOf, bool Buy = false, bool Sell = false)
	{
		if (Buy && note.Has(NoBuySecretString))
		{
			becauseOf = "no buy secret string " + NoBuySecretString + " on note";
			return false;
		}
		if (Sell && note.Has(NoSellSecretString))
		{
			becauseOf = "no sell secret string " + NoSellSecretString + " on note";
			return false;
		}
		if (Buy && note.Has(GossipSecretString))
		{
			becauseOf = "gossip secret string " + GossipSecretString + " on note";
			return true;
		}
		if (Buy && BuyTargetedSecrets && note.Has(TargetedSecretString))
		{
			becauseOf = "targeted secret string " + TargetedSecretString + " on note for buy";
			return true;
		}
		if (Sell && SellTargetedSecrets && note.Has(TargetedSecretString))
		{
			becauseOf = "targeted secret string " + TargetedSecretString + " on note for sell";
			return true;
		}
		foreach (FactionInterest interest in Interests)
		{
			if (interest.Includes(note, Buy, Sell))
			{
				becauseOf = "interest " + interest.DebugName;
				return true;
			}
		}
		return false;
	}

	public bool UseAltBehavior(GameObject speaker)
	{
		if (!WaterRitualAltBehaviorPart.IsNullOrEmpty() && speaker.HasPart(WaterRitualAltBehaviorPart))
		{
			return true;
		}
		if (!WaterRitualAltBehaviorTag.IsNullOrEmpty() && speaker.HasTagOrProperty(WaterRitualAltBehaviorTag))
		{
			return true;
		}
		return false;
	}

	public bool HasInterestSameAs(FactionInterest interest)
	{
		foreach (FactionInterest interest2 in Interests)
		{
			if (interest2.SameAs(interest))
			{
				return true;
			}
		}
		return false;
	}

	public void AddInterestIfNew(FactionInterest interest)
	{
		if (!HasInterestSameAs(interest))
		{
			Interests.Add(interest);
		}
	}

	public static GameObject GenerateHeirloom(string Type)
	{
		int tier = "5-6".Roll();
		GameObject gameObject = GameObject.Create(PopulationManager.RollOneFrom(Type + " " + tier).Blueprint, 0, 1);
		gameObject.SetStringProperty("Mods", "None");
		string type = RelicGenerator.GetType(gameObject);
		string subtype = RelicGenerator.GetSubtype(type);
		string text = RelicGenerator.SelectElement(gameObject) ?? "might";
		int num = 20;
		if (RelicGenerator.ApplyBasicBestowal(gameObject, type, tier, subtype))
		{
			num += 20;
		}
		if (50.in100() && !text.IsNullOrEmpty())
		{
			if (RelicGenerator.ApplyElementBestowal(gameObject, text, type, tier, subtype))
			{
				num += 40;
			}
		}
		else if (RelicGenerator.ApplyBasicBestowal(gameObject, type, tier, subtype))
		{
			num += 20;
		}
		string Name = null;
		string Article = null;
		if (40.in100())
		{
			List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote note) => note.Has("ruins") && !note.Has("historic") && note.Text != "some forgotten ruins");
			if (mapNotes.Count > 0)
			{
				Name = HistoricStringExpander.ExpandString("<spice.elements." + text + ".adjectives.!random> " + HistoricStringExpander.ExpandString("<spice.itemTypes." + type + ".!random>") + " of " + mapNotes.GetRandomElement().Text);
				Article = "the";
			}
		}
		if (Name == null)
		{
			GameObject aLegendaryEligibleCreature = EncountersAPI.GetALegendaryEligibleCreature();
			HeroMaker.MakeHero(aLegendaryEligibleCreature);
			Dictionary<string, string> vars = new Dictionary<string, string>
			{
				{ "*element*", text },
				{ "*itemType*", type },
				{
					"*personNounPossessive*",
					Grammar.MakePossessive(HistoricStringExpander.ExpandString("<spice.personNouns.!random>"))
				},
				{
					"*creatureNamePossessive*",
					Grammar.MakePossessive(aLegendaryEligibleCreature.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true))
				}
			};
			Name = HistoricStringExpander.ExpandString("<spice.history.relics.names.!random>", null, null, vars);
			QudHistoryHelpers.ExtractArticle(ref Name, out Article);
		}
		gameObject.RequirePart<OriginalItemType>();
		Name = QudHistoryHelpers.Ansify(Grammar.MakeTitleCase(Name));
		gameObject.GiveProperName(Name, Force: true);
		if (!Article.IsNullOrEmpty())
		{
			gameObject.SetStringProperty("IndefiniteArticle", Article);
			gameObject.SetStringProperty("DefiniteArticle", Article);
		}
		gameObject.SetImportant(flag: true);
		if (num != 0 && gameObject.TryGetPart<Commerce>(out var Part))
		{
			Part.Value += Part.Value * (double)num / 100.0;
		}
		AfterPseudoRelicGeneratedEvent.Send(gameObject, text, type, subtype, tier);
		return gameObject;
	}

	public GameObject GenerateHeirloom()
	{
		return GenerateHeirloom(Items.ItemTableNames[Heirloom]);
	}

	public void CacheHeirloom()
	{
		HeirloomID = ZoneManager.instance.CacheObject(GenerateHeirloom());
	}

	public void RequireCachedHeirloom()
	{
		if (HeirloomID.IsNullOrEmpty())
		{
			HeirloomID = ZoneManager.instance.CacheObject(GenerateHeirloom());
		}
	}

	public GameObject GetHeirloom()
	{
		GameObject gameObject = null;
		string heirloomID = HeirloomID;
		if (!heirloomID.IsNullOrEmpty())
		{
			gameObject = ZoneManager.instance.PullCachedObject(heirloomID);
		}
		CacheHeirloom();
		return gameObject ?? GenerateHeirloom();
	}

	public static bool WantsToBuySecret(string Faction, IBaseJournalEntry Note, GameObject Object = null)
	{
		return WantsToBuySecret(Factions.Get(Faction), Note, Object);
	}

	public bool WantsToBuySecret(IBaseJournalEntry Note, GameObject Object = null)
	{
		return WantsToBuySecret(this, Note, Object);
	}

	public static bool WantsToBuySecret(Faction Faction, IBaseJournalEntry Note, GameObject Object = null)
	{
		if (!Note.Revealed)
		{
			return false;
		}
		if (!Note.Tradable)
		{
			return false;
		}
		if (Note is JournalMapNote journalMapNote && Object != null && journalMapNote.ZoneID == Object.CurrentZone.ZoneID)
		{
			return false;
		}
		if (Object != null && Object.GetStringProperty("nosecret") == Note.ID)
		{
			return false;
		}
		if (Object != null && Object.HasPart<Chef>() && Note.Has("recipe"))
		{
			return true;
		}
		if (Faction.InterestedIn(Note, Buy: true))
		{
			return true;
		}
		return false;
	}

	public static bool WantsToBuySecret(string faction, IBaseJournalEntry note, GameObject rep, ref string becauseOf)
	{
		if (!note.Revealed)
		{
			becauseOf = "note has not been revealed";
			return false;
		}
		if (!note.Tradable)
		{
			becauseOf = "note has been sold";
			return false;
		}
		if (note is JournalMapNote journalMapNote && rep != null && journalMapNote.ZoneID == rep.CurrentZone.ZoneID)
		{
			becauseOf = "note is about current zone";
			return false;
		}
		if (rep != null && rep.GetStringProperty("nosecret") == note.ID)
		{
			becauseOf = "secret ID " + note.ID + " matching nosecret property on speaker";
			return false;
		}
		if (rep != null && rep.HasPart<Chef>() && note.Has("recipe"))
		{
			becauseOf = "note is a recipe and speaker is a chef";
			return true;
		}
		if (Factions.Get(faction).InterestedIn(note, ref becauseOf, Buy: true))
		{
			return true;
		}
		return false;
	}

	public int GetBuySecretWeight(IBaseJournalEntry Note, GameObject Object = null)
	{
		return GetBuySecretWeight(this, Note, Object);
	}

	public static int GetBuySecretWeight(Faction Faction, IBaseJournalEntry Note, GameObject Object = null)
	{
		if (!Note.CanSell())
		{
			return 0;
		}
		if (Object != null && Object.GetStringProperty("nosecret") == Note.ID)
		{
			return 0;
		}
		int num = Faction.GetInterestIn(Note, Buy: true);
		if (num < Note.Weight && Object != null && Note is JournalRecipeNote && Object.HasPart(typeof(Chef)))
		{
			num = Note.Weight;
		}
		return num;
	}

	public static bool WantsToSellSecret(string Faction, IBaseJournalEntry Note)
	{
		return WantsToSellSecret(Factions.Get(Faction), Note);
	}

	public bool WantsToSellSecret(IBaseJournalEntry Note)
	{
		return WantsToSellSecret(this, Note);
	}

	public static bool WantsToSellSecret(Faction Faction, IBaseJournalEntry Note)
	{
		if (Note.Revealed)
		{
			return false;
		}
		if (Faction.InterestedIn(Note, Buy: false, Sell: true))
		{
			return true;
		}
		return false;
	}

	public static bool WantsToSellSecret(string faction, IBaseJournalEntry note, ref string becauseOf)
	{
		if (note.Revealed)
		{
			becauseOf = "note has been revealed";
			return false;
		}
		if (Factions.Get(faction).InterestedIn(note, ref becauseOf, Buy: false, Sell: true))
		{
			return true;
		}
		return false;
	}

	public int GetSellSecretWeight(IBaseJournalEntry Note)
	{
		return GetSellSecretWeight(this, Note);
	}

	public static int GetSellSecretWeight(Faction Faction, IBaseJournalEntry Note)
	{
		if (!Note.CanBuy())
		{
			return 0;
		}
		return Faction.GetInterestIn(Note, Buy: false, Sell: true);
	}

	public int GetFactionWorshipAttitude(string Faction)
	{
		if (FactionWorshipAttitudes.TryGetValue(Faction, out var value))
		{
			return value;
		}
		if (ApplyDefaultFactionWorshipAttitudeAfterSpecificFeelings)
		{
			if (DefaultFactionWorshipAttitudeSet || DefaultFactionWorshipAttitude != 0)
			{
				return GetFeelingTowardsFaction(Faction, DefaultFactionWorshipAttitude);
			}
		}
		else if (DefaultFactionWorshipAttitudeSet || DefaultFactionWorshipAttitude != 0)
		{
			return DefaultFactionWorshipAttitude;
		}
		return GetFeelingTowardsFaction(Faction);
	}

	public int GetFactionBlasphemyAttitude(string Faction)
	{
		return -GetFactionWorshipAttitude(Faction);
	}

	public void ApplyWorshipAttitude(string Faction, int Attitude)
	{
		FactionWorshipAttitudes[Faction] = GetFactionWorshipAttitude(Faction) + Attitude;
	}

	public List<Worshippable> GetWorshippables()
	{
		return Worshippables;
	}

	public void RegisterWorshippable(GameObject Object)
	{
		if (!GameObject.Validate(ref Object) || !Object.HasPropertyOrTag("Worshippable") || Object.HasIntProperty("WorshipRegistered"))
		{
			return;
		}
		Object.SetIntProperty("WorshipRegistered", 1);
		string text = Object.GetPropertyOrTag("WorshippedAs") ?? Object.GetReferenceDisplayName(int.MaxValue, null, "Worship", NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: true);
		Worshippable worshippable = null;
		bool flag = false;
		foreach (Worshippable worshippable4 in Worshippables)
		{
			if (worshippable4.Name == text)
			{
				worshippable = worshippable4;
				flag = true;
				break;
			}
		}
		if (worshippable == null)
		{
			worshippable = new Worshippable();
			worshippable.Name = text;
			worshippable.Faction = Name;
		}
		if (Object.HasID || Object.HasPropertyOrTag("WorshipForceTrackID"))
		{
			string iD = Object.ID;
			if (!worshippable.Sources.HasDelimitedSubstring(',', iD))
			{
				if (worshippable.Sources == null)
				{
					worshippable.Sources = iD;
				}
				else
				{
					Worshippable worshippable2 = worshippable;
					worshippable2.Sources = worshippable2.Sources + "," + iD;
				}
			}
		}
		if (worshippable.Blueprints == null)
		{
			worshippable.Blueprints = Object.Blueprint;
		}
		else if (!worshippable.Blueprints.HasDelimitedSubstring(";;", Object.Blueprint))
		{
			Worshippable worshippable3 = worshippable;
			worshippable3.Blueprints = worshippable3.Blueprints + ";;" + Object.Blueprint;
		}
		int result = 0;
		if (Object.HasIntProperty("WorshipPower"))
		{
			result = Object.GetIntProperty("WorshipPower");
		}
		else if (Object.HasTag("WorshipPower"))
		{
			int.TryParse(Object.GetTag("WorshipPower"), out result);
		}
		if (result > worshippable.Power)
		{
			worshippable.Power = result;
		}
		if (!flag)
		{
			Worshippables.Add(worshippable);
		}
	}

	public Worshippable RegisterWorshippable(string Name, int Power = 0)
	{
		Worshippable worshippable = null;
		foreach (Worshippable worshippable2 in Worshippables)
		{
			if (worshippable2.Name == Name)
			{
				worshippable = worshippable2;
				break;
			}
		}
		if (worshippable == null)
		{
			worshippable = new Worshippable();
			worshippable.Name = Name;
			worshippable.Faction = this.Name;
			Worshippables.Add(worshippable);
		}
		if (Power > worshippable.Power)
		{
			worshippable.Power = Power;
		}
		return worshippable;
	}

	public Worshippable FindWorshippable(string Name)
	{
		foreach (Worshippable worshippable in Worshippables)
		{
			if (worshippable.Name == Name)
			{
				return worshippable;
			}
		}
		return null;
	}

	public Worshippable FindWorshippable(GameObject Object)
	{
		if (!GameObject.Validate(ref Object))
		{
			return null;
		}
		if (!Object.HasPropertyOrTag("Worshippable"))
		{
			return null;
		}
		if (Object.HasID)
		{
			string iD = Object.ID;
			foreach (Worshippable worshippable in Worshippables)
			{
				if (worshippable.Sources.HasDelimitedSubstring(',', iD))
				{
					return worshippable;
				}
			}
		}
		string text = Object.GetPropertyOrTag("WorshippedAs") ?? Object.GetReferenceDisplayName(int.MaxValue, null, "Worship", NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: true);
		foreach (Worshippable worshippable2 in Worshippables)
		{
			if (worshippable2.Name == text)
			{
				return worshippable2;
			}
		}
		return null;
	}

	public MemorialTracking QueueMemorial(GameObject Object, bool GenerateNameIfNotNamed = false, string Eulogy = null, string Reason = null, string ThirdPersonReason = null)
	{
		if (!GameObject.Validate(ref Object))
		{
			return null;
		}
		MemorialTracking memorialTracking = new MemorialTracking();
		if (GenerateNameIfNotNamed && !Object.HasProperName)
		{
			memorialTracking.Name = NameMaker.MakeName(Object);
		}
		MemorialTracking memorialTracking2 = memorialTracking;
		if (memorialTracking2.Name == null)
		{
			memorialTracking2.Name = Object.GetReferenceDisplayName(int.MaxValue, null, "Memorial", NoColor: false, Stripped: true);
		}
		memorialTracking.Eulogy = Eulogy;
		memorialTracking.Reason = Reason;
		memorialTracking.ThirdPersonReason = ThirdPersonReason;
		if (Object.HasID)
		{
			memorialTracking.ID = Object.ID;
		}
		memorialTracking.Blueprint = Object.Blueprint;
		memorialTracking.Queued = The.CurrentTurn;
		if (QueuedMemorials == null)
		{
			QueuedMemorials = new List<MemorialTracking>();
		}
		QueuedMemorials.Add(memorialTracking);
		return memorialTracking;
	}

	public MemorialTracking MemorialPerformed(MemorialTracking Memorial, Cell Cell = null)
	{
		QueuedMemorials?.Remove(Memorial);
		Memorial.Performed = The.CurrentTurn;
		if (Cell != null)
		{
			Memorial.Location = Cell.ParentZone?.ZoneID + "@" + Cell.X + "," + Cell.Y;
		}
		if (PerformedMemorials == null)
		{
			PerformedMemorials = new List<MemorialTracking>();
		}
		PerformedMemorials.Add(Memorial);
		return Memorial;
	}

	public bool HasQueuedMemorialByName(string Name)
	{
		if (QueuedMemorials != null)
		{
			foreach (MemorialTracking queuedMemorial in QueuedMemorials)
			{
				if (queuedMemorial.Name == Name)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasQueuedMemorialByID(string ID)
	{
		if (QueuedMemorials != null)
		{
			foreach (MemorialTracking queuedMemorial in QueuedMemorials)
			{
				if (queuedMemorial.ID == ID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasQueuedMemorialByBlueprint(string Blueprint)
	{
		if (QueuedMemorials != null)
		{
			foreach (MemorialTracking queuedMemorial in QueuedMemorials)
			{
				if (queuedMemorial.Blueprint == Blueprint)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasPerformedMemorialByName(string Name)
	{
		if (PerformedMemorials != null)
		{
			foreach (MemorialTracking performedMemorial in PerformedMemorials)
			{
				if (performedMemorial.Name == Name)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasPerformedMemorialByID(string ID)
	{
		if (PerformedMemorials != null)
		{
			foreach (MemorialTracking performedMemorial in PerformedMemorials)
			{
				if (performedMemorial.ID == ID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasPerformedMemorialByBlueprint(string Blueprint)
	{
		if (PerformedMemorials != null)
		{
			foreach (MemorialTracking performedMemorial in PerformedMemorials)
			{
				if (performedMemorial.Blueprint == Blueprint)
				{
					return true;
				}
			}
		}
		return false;
	}

	public MemorialTracking GetQueuedMemorialByName(string Name)
	{
		if (QueuedMemorials != null)
		{
			foreach (MemorialTracking queuedMemorial in QueuedMemorials)
			{
				if (queuedMemorial.Name == Name)
				{
					return queuedMemorial;
				}
			}
		}
		return null;
	}

	public MemorialTracking GetQueuedMemorialByID(string ID)
	{
		if (QueuedMemorials != null)
		{
			foreach (MemorialTracking queuedMemorial in QueuedMemorials)
			{
				if (queuedMemorial.ID == ID)
				{
					return queuedMemorial;
				}
			}
		}
		return null;
	}

	public MemorialTracking GetQueuedMemorialByBlueprint(string Blueprint)
	{
		if (QueuedMemorials == null)
		{
			foreach (MemorialTracking queuedMemorial in QueuedMemorials)
			{
				if (queuedMemorial.Blueprint == Blueprint)
				{
					return queuedMemorial;
				}
			}
		}
		return null;
	}

	public MemorialTracking GetPerformedMemorialByName(string Name)
	{
		if (PerformedMemorials != null)
		{
			foreach (MemorialTracking performedMemorial in PerformedMemorials)
			{
				if (performedMemorial.Name == Name)
				{
					return performedMemorial;
				}
			}
		}
		return null;
	}

	public MemorialTracking GetPerformedMemorialByID(string ID)
	{
		if (PerformedMemorials != null)
		{
			foreach (MemorialTracking performedMemorial in PerformedMemorials)
			{
				if (performedMemorial.ID == ID)
				{
					return performedMemorial;
				}
			}
		}
		return null;
	}

	public MemorialTracking GetPerformedMemorialByBlueprint(string Blueprint)
	{
		if (PerformedMemorials == null)
		{
			foreach (MemorialTracking performedMemorial in PerformedMemorials)
			{
				if (performedMemorial.Blueprint == Blueprint)
				{
					return performedMemorial;
				}
			}
		}
		return null;
	}

	public List<GameObjectBlueprint> GetMembers(Predicate<GameObjectBlueprint> Predicate = null, bool Dynamic = true, bool ReadOnly = true)
	{
		return GetMembers(Name, Predicate, Dynamic, ReadOnly);
	}

	public static List<GameObjectBlueprint> GetMembers(string FactionName, Predicate<GameObjectBlueprint> Predicate = null, bool Dynamic = true, bool ReadOnly = true)
	{
		List<GameObjectBlueprint> list = (ReadOnly ? MemberCache : new List<GameObjectBlueprint>());
		list.Clear();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!blueprint.IsBaseBlueprint() && (!Dynamic || !blueprint.IsExcludedFromDynamicEncounters()) && IsMember(blueprint, FactionName) && (Predicate == null || Predicate(blueprint)))
			{
				list.Add(blueprint);
			}
		}
		return list;
	}

	public bool AnyMembers(Predicate<GameObjectBlueprint> Predicate = null, bool Dynamic = true)
	{
		return AnyMembers(Name, Predicate, Dynamic);
	}

	public static bool AnyMembers(string FactionName, Predicate<GameObjectBlueprint> Predicate = null, bool Dynamic = true)
	{
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!blueprint.IsBaseBlueprint() && (!Dynamic || !blueprint.IsExcludedFromDynamicEncounters()) && IsMember(blueprint, FactionName) && (Predicate == null || Predicate(blueprint)))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsMember(GameObjectBlueprint Blueprint)
	{
		return IsMember(Blueprint, Name);
	}

	public static bool IsMember(GameObjectBlueprint Blueprint, string FactionName)
	{
		if (Blueprint.Tags.TryGetValue("AssociatedFactions", out var value) && value.HasDelimitedSubstring(',', FactionName))
		{
			return true;
		}
		if (Blueprint.TryGetPartParameter<string>("Brain", "Factions", out var Result) && !Result.IsNullOrEmpty())
		{
			int num = Result.Length - 1;
			int num2 = -1;
			while (num2 < num)
			{
				num2 = Result.IndexOf(FactionName, num2 + 1, StringComparison.Ordinal);
				if (num2 == -1)
				{
					break;
				}
				if (num2 <= 0 || Result[num2 - 1] == ',')
				{
					int num3 = num2 + FactionName.Length;
					if (num3 - 1 >= num || Result[num3] == '-')
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
