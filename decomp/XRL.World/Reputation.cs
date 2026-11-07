using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using HistoryKit;
using Occult.Engine.CodeGeneration;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
[GenerateSerializationPartial]
public class Reputation : IComposite
{
	public const int LOVED = 2;

	public const int LIKED = 1;

	public const int INDIFFERENT = 0;

	public const int DISLIKED = -1;

	public const int HATED = -2;

	public Dictionary<string, float> ReputationValues = new Dictionary<string, float>(64);

	public Dictionary<string, string> FactionRanks = new Dictionary<string, string>();

	public List<WorshipTracking> WorshipTracking = new List<WorshipTracking>();

	public List<WorshipTracking> BlasphemyTracking = new List<WorshipTracking>();

	private static List<string> PositiveSounds = new List<string>();

	private static List<string> NegativeSounds = new List<string>();

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.Write(ReputationValues);
		Writer.Write(FactionRanks);
		Writer.WriteComposite(WorshipTracking);
		Writer.WriteComposite(BlasphemyTracking);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		ReputationValues = Reader.ReadDictionary<string, float>();
		FactionRanks = Reader.ReadDictionary<string, string>();
		WorshipTracking = Reader.ReadCompositeList<WorshipTracking>();
		BlasphemyTracking = Reader.ReadCompositeList<WorshipTracking>();
	}

	public string GetFactionRank(string Faction)
	{
		FactionRanks.TryGetValue(Faction, out var value);
		return GetFactionRankEvent.GetFor(The.Player, Faction, value);
	}

	public void SetFactionRank(string FactionName, string Rank, bool Message = false, bool Capitalize = true)
	{
		FactionRanks[FactionName] = Rank;
		if (Message)
		{
			Popup.Show("You are promoted to the " + Faction.GetRankTerm(FactionName) + " of " + (Capitalize ? Rank.Capitalize() : Rank) + ".");
		}
	}

	public bool PromoteIfBelow(string Faction, string Rank, bool Message = true, bool Capitalize = true)
	{
		if (IsBelowRank(Faction, Rank))
		{
			SetFactionRank(Faction, Rank, Message, Capitalize);
			return true;
		}
		return false;
	}

	public int GetFactionStanding(string Faction)
	{
		if (FactionRanks.TryGetValue(Faction, out var value))
		{
			return Factions.Get(Faction).GetRankStanding(value);
		}
		return 0;
	}

	public bool IsBelowRank(string Faction, string Rank)
	{
		string factionRank = GetFactionRank(Faction);
		if (factionRank == null)
		{
			return true;
		}
		if (factionRank == Rank)
		{
			return false;
		}
		Faction faction = Factions.Get(Faction);
		return faction.GetRankStanding(factionRank) < faction.GetRankStanding(Rank);
	}

	public bool IsAtLeastRank(string Faction, string Rank)
	{
		return !IsBelowRank(Faction, Rank);
	}

	public bool IsBelowStanding(string Faction, int Standing)
	{
		return GetFactionStanding(Faction) < Standing;
	}

	public bool IsAtLeastStanding(string Faction, int Standing)
	{
		return !IsBelowStanding(Faction, Standing);
	}

	public bool Any(string Faction)
	{
		return ReputationValues.ContainsKey(Faction);
	}

	public int Get(Faction Faction)
	{
		if (Faction == null)
		{
			return 0;
		}
		int num = ((!ReputationValues.TryGetValue(Faction.Name, out var value)) ? GetDefaultRep(Faction.Name) : ((int)value));
		GameObject player = The.Player;
		if (GameObject.Validate(player))
		{
			if (Faction.PartReputation != null)
			{
				foreach (KeyValuePair<string, int> item in Faction.PartReputation)
				{
					if (player.HasPart(item.Key))
					{
						num += item.Value;
					}
				}
			}
			if (Faction.Visible)
			{
				num += player.GetIntProperty("AllVisibleRepModifier");
			}
		}
		return num;
	}

	public int GetLevel(string Faction)
	{
		int num = Get(Faction);
		if (num >= RuleSettings.REPUTATION_LOVED)
		{
			return 2;
		}
		if (num >= RuleSettings.REPUTATION_LIKED)
		{
			return 1;
		}
		if (num > RuleSettings.REPUTATION_DISLIKED)
		{
			return 0;
		}
		if (num > RuleSettings.REPUTATION_HATED)
		{
			return -1;
		}
		return -2;
	}

	public int Get(string Faction)
	{
		return Get(Factions.Get(Faction));
	}

	public int Set(Faction Faction, int Rep)
	{
		ReputationValues[Faction.Name] = Rep;
		Faction.FactionFeeling["Player"] = GetFeeling(Faction.Name);
		return Rep;
	}

	public int Set(string Faction, int Rep)
	{
		return Set(Factions.Get(Faction), Rep);
	}

	public void FinishModify()
	{
		int num = 0;
		if (!PositiveSounds.IsNullOrEmpty())
		{
			if (PositiveSounds.Count > 2)
			{
				SoundManager.PlaySound("Sounds/Reputation/sfx_reputation_cacophonic_positive", 0f, 1f, 1f, SoundRequest.SoundEffectType.None, (float)num * 1.3f);
				num++;
			}
			else
			{
				int i = 0;
				for (int count = PositiveSounds.Count; i < count; i++)
				{
					SoundManager.PlaySound(PositiveSounds[i], 0f, 1f, 1f, SoundRequest.SoundEffectType.None, (float)num * 1.3f);
					num++;
				}
			}
		}
		if (!NegativeSounds.IsNullOrEmpty())
		{
			if (NegativeSounds.Count > 2)
			{
				SoundManager.PlaySound("Sounds/Reputation/sfx_reputation_cacophonic_negative", 0f, 1f, 1f, SoundRequest.SoundEffectType.None, (float)num * 1.3f);
				num++;
			}
			else
			{
				int j = 0;
				for (int count2 = NegativeSounds.Count; j < count2; j++)
				{
					SoundManager.PlaySound(NegativeSounds[j], 0f, 1f, 1f, SoundRequest.SoundEffectType.None, (float)num * 1.3f);
					num++;
				}
			}
		}
		PositiveSounds.Clear();
		NegativeSounds.Clear();
	}

	public float Modify(Faction Faction, int Amount, string Because = null, StringBuilder PutMessage = null, string Type = null, bool Silent = false, bool Transient = false, bool SingleLine = false, bool Multiple = false)
	{
		string name = Faction.Name;
		if (!ReputationValues.ContainsKey(name))
		{
			ReputationValues.Add(name, GetDefaultRep(name));
		}
		int num = ReputationChangeEvent.GetFor(Faction, Amount, Type, Silent, Transient);
		if (num == 0)
		{
			return Get(name);
		}
		int num2 = Get(name);
		ReputationValues[name] += num;
		Faction.FactionFeeling["Player"] = GetFeeling(Faction.Name);
		int num3 = Get(name);
		char color = GetColor(num3);
		bool flag = false;
		bool flag2 = false;
		bool state = false;
		bool state2 = false;
		bool state3 = false;
		string text = null;
		bool flag3 = false;
		if (num > 0)
		{
			if (num2 < RuleSettings.REPUTATION_LOVED && num3 >= RuleSettings.REPUTATION_LOVED)
			{
				flag = true;
				text = "loved";
			}
			else if (num2 < RuleSettings.REPUTATION_LIKED && num3 >= RuleSettings.REPUTATION_LIKED)
			{
				state2 = true;
				text = "favored";
			}
			else if (num2 <= RuleSettings.REPUTATION_DISLIKED && num3 > RuleSettings.REPUTATION_DISLIKED)
			{
				state3 = true;
				text = "indifferent";
				flag3 = true;
			}
			else if (num2 <= RuleSettings.REPUTATION_HATED && num3 > RuleSettings.REPUTATION_HATED)
			{
				state = true;
				text = "disliked";
			}
		}
		else if (num2 > RuleSettings.REPUTATION_HATED && num3 <= RuleSettings.REPUTATION_HATED)
		{
			flag2 = true;
			text = "despised";
		}
		else if (num2 > RuleSettings.REPUTATION_DISLIKED && num3 <= RuleSettings.REPUTATION_DISLIKED)
		{
			state = true;
			text = "disliked";
		}
		else if (num2 >= RuleSettings.REPUTATION_LIKED && num3 < RuleSettings.REPUTATION_LIKED)
		{
			state3 = true;
			text = "indifferent";
			flag3 = true;
		}
		else if (num2 >= RuleSettings.REPUTATION_LOVED && num3 < RuleSettings.REPUTATION_LOVED)
		{
			state2 = true;
			text = "favored";
		}
		if (Faction.Visible)
		{
			if (!Silent)
			{
				if (num3 > num2 && !Faction.PositiveSound.IsNullOrEmpty())
				{
					if (PositiveSounds == null)
					{
						PositiveSounds = new List<string>();
					}
					PositiveSounds.Add(Faction.PositiveSound);
				}
				if (num3 < num2 && !Faction.NegativeSound.IsNullOrEmpty())
				{
					if (NegativeSounds == null)
					{
						NegativeSounds = new List<string>();
					}
					NegativeSounds.Add(Faction.NegativeSound);
				}
				StringBuilder stringBuilder = PutMessage ?? Event.NewStringBuilder();
				if (!Because.IsNullOrEmpty())
				{
					stringBuilder.Append(Because.Capitalize()).Append(", your ");
				}
				else
				{
					stringBuilder.Append("Your ");
				}
				stringBuilder.Append("reputation with {{C|").Append(Faction.GetFormattedName()).Append("}} ")
					.Append((num >= 0) ? "increased" : "decreased")
					.Append(" by {{")
					.Append((num >= 0) ? 'G' : 'R')
					.Append('|')
					.Append(Math.Abs(num))
					.Append("}} to {{")
					.Append(color)
					.Append('|')
					.Append(num3)
					.Append("}}.");
				if (text != null)
				{
					if (flag3)
					{
						stringBuilder.Append(SingleLine ? ((object)' ') : "\n\n").Append(Faction.GetFormattedName().Capitalize()).Append(' ')
							.Append(Faction.Plural ? "are" : "is")
							.Append(" now {{")
							.Append(color)
							.Append("|")
							.Append(text)
							.Append("}} to you.");
					}
					else
					{
						stringBuilder.Append(SingleLine ? ((object)' ') : "\n\n").Append("You are now {{").Append(color)
							.Append("|")
							.Append(text)
							.Append("}} by {{C|")
							.Append(Faction.GetFormattedName())
							.Append("}}.");
					}
				}
				if (PutMessage == null)
				{
					FinishModify();
					Popup.Show(stringBuilder.ToString());
				}
				else
				{
					PutMessage.Append('\n');
				}
			}
			if (flag && The.Player != null)
			{
				if (!Faction.HasProperty("BecameLoved"))
				{
					JournalAPI.AddAccomplishment("You became loved among " + Faction.GetFormattedName() + " and were treated as one of their own.", "While wandering around " + Grammar.InitLowerIfArticle(Grammar.GetProsaicZoneName(The.Player.CurrentZone)) + ", =name= stumbled upon a clan of " + Faction.GetFormattedName() + " performing a secret ritual. Because of " + The.Player.GetPronounProvider().PossessiveAdjective + " " + HistoricStringExpander.ExpandString("<spice.elements." + IComponent<GameObject>.ThePlayerMythDomain + ".quality.!random>") + ", they accepted " + The.Player.GetPronounProvider().Objective + " into their fold and taught " + The.Player.GetPronounProvider().Objective + " their secrets.", "Deep in the wilds of " + Grammar.InitLowerIfArticle(Grammar.GetProsaicZoneName(The.Player.CurrentZone)) + ", =name= stumbled upon a clan of " + Faction.GetFormattedName() + " performing a secret ritual. Because of " + Grammar.MakePossessive(The.Player.BaseDisplayNameStripped) + " <spice.elements." + The.Player.GetMythicDomain() + ".quality.!random>, they accepted " + The.Player.GetPronounProvider().Objective + " into their fold and taught " + The.Player.GetPronounProvider().Objective + " their secrets.", null, "general", MuralCategory.BecomesLoved, MuralWeight.Medium, null, -1L);
					Faction.SetProperty("BecameLoved", 1);
				}
				Achievement.LOVED_BY_FACTION.Unlock();
				if (name == "Newly Sentient Beings")
				{
					Achievement.LOVED_BY_NEW_BEINGS.Unlock();
				}
			}
			if (flag2 && name == "Joppa")
			{
				Achievement.HATED_BY_JOPPA.Unlock();
			}
		}
		if (The.Player != null && The.Player.HasRegisteredEvent("ReputationChanged"))
		{
			StringBuilder stringBuilder2 = PutMessage ?? Event.NewStringBuilder();
			Event obj = Event.New("ReputationChanged");
			obj.SetParameter("Actor", The.Player);
			obj.SetParameter("Faction", name);
			obj.SetParameter("OldReputation", num2);
			obj.SetParameter("NewReputation", num3);
			obj.SetParameter("Type", Type);
			obj.SetParameter("Because", Because);
			obj.SetParameter("Message", stringBuilder2);
			obj.SetFlag("BecameLoved", flag);
			obj.SetFlag("BecameLiked", state2);
			obj.SetFlag("BecameIndifferent", state3);
			obj.SetFlag("BecameDisliked", state);
			obj.SetFlag("BecameHated", flag2);
			The.Player.FireEvent(obj);
			if (PutMessage == null && stringBuilder2.Length > 0)
			{
				FinishModify();
				Popup.Show(stringBuilder2.ToString());
			}
		}
		if (!Multiple)
		{
			FinishModify();
		}
		AfterReputationChangeEvent.Send(Faction, num2, num3, Type, Silent, Transient);
		return Get(Faction);
	}

	public float Modify(string Faction, int Amount, string Type = null, string Because = null, StringBuilder PutMessage = null, bool Silent = false, bool Transient = false)
	{
		return Modify(Factions.Get(Faction), Amount, Because, PutMessage, Type, Silent, Transient);
	}

	public float Modify(string Faction, int Amount, bool DisplayMessage)
	{
		return Modify(Factions.Get(Faction), Amount, null, null, null, !DisplayMessage);
	}

	public bool Use(string Faction, int Amount)
	{
		if (Get(Faction) < Amount)
		{
			return false;
		}
		Modify(Faction, -Amount);
		return true;
	}

	public static int GetAttitude(int Rep)
	{
		if (Rep <= RuleSettings.REPUTATION_HATED)
		{
			return -2;
		}
		if (Rep <= RuleSettings.REPUTATION_DISLIKED)
		{
			return -1;
		}
		if (Rep < RuleSettings.REPUTATION_LIKED)
		{
			return 0;
		}
		if (Rep < RuleSettings.REPUTATION_LOVED)
		{
			return 1;
		}
		return 2;
	}

	public int GetAttitude(Faction Faction)
	{
		return GetAttitude(Get(Faction));
	}

	public int GetAttitude(string Faction)
	{
		return GetAttitude(Get(Factions.Get(Faction)));
	}

	public static int GetTradePerformance(int Rep)
	{
		if (Rep <= RuleSettings.REPUTATION_HATED)
		{
			return -3;
		}
		if (Rep <= RuleSettings.REPUTATION_DISLIKED)
		{
			return -1;
		}
		if (Rep < RuleSettings.REPUTATION_LIKED)
		{
			return 0;
		}
		if (Rep < RuleSettings.REPUTATION_LOVED)
		{
			return 1;
		}
		return 3;
	}

	public int GetTradePerformance(Faction Faction)
	{
		return GetTradePerformance(Get(Faction));
	}

	public int GetTradePerformance(string Faction)
	{
		return GetTradePerformance(Factions.Get(Faction));
	}

	public static char GetColor(int Rep)
	{
		if (Rep <= RuleSettings.REPUTATION_HATED)
		{
			return 'R';
		}
		if (Rep <= RuleSettings.REPUTATION_DISLIKED)
		{
			return 'r';
		}
		if (Rep < RuleSettings.REPUTATION_LIKED)
		{
			return 'C';
		}
		if (Rep < RuleSettings.REPUTATION_LOVED)
		{
			return 'g';
		}
		return 'G';
	}

	public char GetColor(Faction Faction)
	{
		return GetColor(Get(Faction));
	}

	public char GetColor(string Faction)
	{
		return GetColor(Get(Factions.Get(Faction)));
	}

	public int GetDefaultRep(string Faction)
	{
		return 0;
	}

	public int GetFeeling(Faction Faction)
	{
		int feeling = GetFeeling(Get(Faction));
		if (feeling > -50 && feeling < 50 && Faction == HolyPlaceSystem.LastHolyFaction)
		{
			return -50;
		}
		return feeling;
	}

	public int GetFeeling(string Faction)
	{
		return GetFeeling(Factions.Get(Faction));
	}

	public static int GetFeeling(float Reputation)
	{
		if (Reputation <= (float)RuleSettings.REPUTATION_HATED)
		{
			return -100;
		}
		if (Reputation <= (float)RuleSettings.REPUTATION_DISLIKED)
		{
			return -50;
		}
		if (Reputation < (float)RuleSettings.REPUTATION_LIKED)
		{
			return 0;
		}
		if (Reputation < (float)RuleSettings.REPUTATION_LOVED)
		{
			return 50;
		}
		return 100;
	}

	public void Init()
	{
		foreach (Faction item in Factions.Loop())
		{
			int initialPlayerReputation = item.InitialPlayerReputation;
			if (initialPlayerReputation != int.MinValue)
			{
				Set(item.Name, initialPlayerReputation);
			}
		}
	}

	public void InitFeeling()
	{
		IReadOnlyList<Faction> list = Factions.GetList();
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			Faction faction = list[i];
			faction.FactionFeeling["Player"] = GetFeeling(faction);
		}
	}

	public void AfterGameLoaded()
	{
		InitFeeling();
	}

	public void WorshipPerformed(Worshippable Worship)
	{
		if (Worship == null)
		{
			return;
		}
		WorshipTracking worshipTracking = null;
		bool flag = false;
		foreach (WorshipTracking item in WorshipTracking)
		{
			if (item.Faction == Worship.Faction)
			{
				flag = true;
				if (worshipTracking == null && item.Name == Worship.Name)
				{
					worshipTracking = item;
				}
			}
		}
		if (worshipTracking == null)
		{
			worshipTracking = new WorshipTracking();
			worshipTracking.Name = Worship.Name;
			worshipTracking.Faction = Worship.Faction;
			worshipTracking.First = The.CurrentTurn;
			WorshipTracking.Add(worshipTracking);
		}
		worshipTracking.Times++;
		worshipTracking.Last = The.CurrentTurn;
		if (!flag)
		{
			ApplyFactionWorshipAttitudes(worshipTracking.Faction, worshipTracking.Name);
		}
	}

	public bool HasWorshipped(string Faction, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in WorshipTracking)
		{
			if (item.Faction == Faction && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWorshipped(Worshippable Being, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in WorshipTracking)
		{
			if (item.Name == Being.Name && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWorshippedInName(string Name, string Faction = null, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in WorshipTracking)
		{
			if (item.Name == Name && (Faction.IsNullOrEmpty() || item.Faction == Faction) && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWorshippedBySpec(string Spec, string ContextFaction = null)
	{
		if (Spec.IsNullOrEmpty())
		{
			return HasWorshipped(ContextFaction);
		}
		string value = null;
		string value2 = null;
		int result = 0;
		if (Spec.Contains("=") && Spec.Contains(";"))
		{
			Dictionary<string, string> dictionary = Spec.CachedDictionaryExpansion();
			dictionary.TryGetValue("Name", out value);
			dictionary.TryGetValue("Factions", out value2);
			if (value2 == "Context")
			{
				value2 = ContextFaction;
			}
			if (dictionary.TryGetValue("WithinTurns", out var value3))
			{
				int.TryParse(value3, out result);
			}
		}
		else
		{
			value = Spec;
		}
		if (!value.IsNullOrEmpty())
		{
			return HasWorshippedInName(value, value2, result);
		}
		return HasWorshipped(value2, result);
	}

	public List<WorshipTracking> GetWorshipTracking()
	{
		return WorshipTracking;
	}

	public int GetWorshipValence(string Faction)
	{
		int num = 0;
		foreach (Faction item in Factions.Loop())
		{
			num += item.GetFactionWorshipAttitude(Faction);
		}
		return num;
	}

	private void ApplyFactionWorshipAttitudes(string Faction, string Name)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (Faction item in Factions.Loop())
		{
			int factionWorshipAttitude = item.GetFactionWorshipAttitude(Faction);
			if (factionWorshipAttitude != 0)
			{
				num4++;
				num += factionWorshipAttitude;
				if (factionWorshipAttitude > 0)
				{
					num2 += factionWorshipAttitude;
				}
				else
				{
					num3 += factionWorshipAttitude;
				}
			}
		}
		if (num4 == 0)
		{
			return;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		StringBuilder stringBuilder2 = Event.NewStringBuilder();
		stringBuilder.Append("Your worship of ").Append(Name);
		if (num <= -1000 || num3 <= -1000)
		{
			stringBuilder.Append(" has made you infamous to many across Qud");
			if (num2 > 0)
			{
				stringBuilder.Append(" while meeting with the approval of others");
			}
		}
		else if (num >= 1000 || num2 >= 1000)
		{
			stringBuilder.Append(" has ennobled you in the eyes of many across Qud");
			if (num3 < 0)
			{
				stringBuilder.Append(" while making you infamous to others");
			}
		}
		else if (num <= -200)
		{
			stringBuilder.Append(" has made you infamous to some across Qud");
			if (num2 > 0)
			{
				stringBuilder.Append(" while meeting with the approval of others");
			}
		}
		else if (num >= 200)
		{
			stringBuilder.Append(" has ennobled you in the eyes of some across Qud");
			if (num3 < 0)
			{
				stringBuilder.Append(" while making you infamous to others");
			}
		}
		else
		{
			stringBuilder.Append(" has been noted by some across Qud");
		}
		stringBuilder.Append(".\n\n");
		foreach (Faction item2 in Factions.Loop())
		{
			int factionWorshipAttitude2 = item2.GetFactionWorshipAttitude(Faction);
			if (factionWorshipAttitude2 != 0)
			{
				int num5 = GivesRep.VaryRep(factionWorshipAttitude2);
				if (num5 != 0)
				{
					The.Game.PlayerReputation.Modify(item2, num5, null, item2.Visible ? stringBuilder : stringBuilder2, "Worship", !item2.Visible, Transient: false, SingleLine: false, Multiple: true);
				}
			}
		}
		The.Game.PlayerReputation.FinishModify();
		Popup.Show(stringBuilder.ToString());
	}

	public void BlasphemyPerformed(Worshippable Blasphemed)
	{
		if (Blasphemed == null)
		{
			return;
		}
		WorshipTracking worshipTracking = null;
		bool flag = false;
		foreach (WorshipTracking item in BlasphemyTracking)
		{
			if (item.Faction == Blasphemed.Faction)
			{
				flag = true;
				if (worshipTracking == null && item.Name == Blasphemed.Name)
				{
					worshipTracking = item;
				}
			}
		}
		if (worshipTracking == null)
		{
			worshipTracking = new WorshipTracking();
			worshipTracking.Name = Blasphemed.Name;
			worshipTracking.Faction = Blasphemed.Faction;
			worshipTracking.First = The.CurrentTurn;
			BlasphemyTracking.Add(worshipTracking);
		}
		worshipTracking.Times++;
		worshipTracking.Last = The.CurrentTurn;
		if (!flag)
		{
			ApplyFactionBlasphemyAttitudes(worshipTracking.Faction, worshipTracking.Name);
		}
	}

	public bool HasBlasphemed(string Faction, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in BlasphemyTracking)
		{
			if (item.Faction == Faction && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasBlasphemed(Worshippable Being, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in BlasphemyTracking)
		{
			if (item.Name == Being.Name && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasBlasphemedAgainstName(string Name, string Faction = null, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in BlasphemyTracking)
		{
			if (item.Name == Name && (Faction.IsNullOrEmpty() || item.Faction == Faction) && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasBlasphemedBySpec(string Spec, string ContextFaction = null)
	{
		if (Spec.IsNullOrEmpty())
		{
			return HasBlasphemed(ContextFaction);
		}
		string value = null;
		string value2 = null;
		int result = 0;
		if (Spec.Contains("=") && Spec.Contains(";"))
		{
			Dictionary<string, string> dictionary = Spec.CachedDictionaryExpansion();
			dictionary.TryGetValue("Name", out value);
			dictionary.TryGetValue("Factions", out value2);
			if (value2 == "Context")
			{
				value2 = ContextFaction;
			}
			if (dictionary.TryGetValue("WithinTurns", out var value3))
			{
				int.TryParse(value3, out result);
			}
		}
		else
		{
			value = Spec;
		}
		if (!value.IsNullOrEmpty())
		{
			return HasBlasphemedAgainstName(value, value2, result);
		}
		return HasBlasphemed(value2, result);
	}

	public List<WorshipTracking> GetBlasphemyTracking()
	{
		return BlasphemyTracking;
	}

	public int GetBlasphemyValence(string Faction)
	{
		int num = 0;
		foreach (Faction item in Factions.Loop())
		{
			num -= item.GetFactionBlasphemyAttitude(Faction);
		}
		return num;
	}

	private void ApplyFactionBlasphemyAttitudes(string Faction, string Name)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (Faction item in Factions.Loop())
		{
			int factionBlasphemyAttitude = item.GetFactionBlasphemyAttitude(Faction);
			if (factionBlasphemyAttitude != 0)
			{
				num4++;
				num += factionBlasphemyAttitude;
				if (factionBlasphemyAttitude > 0)
				{
					num2 += factionBlasphemyAttitude;
				}
				else
				{
					num3 += factionBlasphemyAttitude;
				}
			}
		}
		if (num4 == 0)
		{
			return;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		StringBuilder stringBuilder2 = Event.NewStringBuilder();
		stringBuilder.Append("Your blasphemy against ").Append(Name);
		if (num >= 1000 || num2 >= 1000)
		{
			stringBuilder.Append(" has ennobled you in the eyes of many across Qud");
			if (num3 < 0)
			{
				stringBuilder.Append(" while making you infamous to others");
			}
		}
		else if (num <= -1000 || num3 <= -1000)
		{
			stringBuilder.Append(" has made you infamous to many across Qud");
			if (num2 > 0)
			{
				stringBuilder.Append(" while meeting with the approval of others");
			}
		}
		else if (num >= 200)
		{
			stringBuilder.Append(" has ennobled you in the eyes of some across Qud");
			if (num3 < 0)
			{
				stringBuilder.Append(" while making you infamous to others");
			}
		}
		else if (num <= -200)
		{
			stringBuilder.Append(" has made you infamous to some across Qud");
			if (num2 > 0)
			{
				stringBuilder.Append(" while meeting with the approval of others");
			}
		}
		else if (num2 - num3 >= 1000)
		{
			stringBuilder.Append(" has been noted by many across Qud");
		}
		else
		{
			stringBuilder.Append(" has been noted by some across Qud");
		}
		stringBuilder.Append(".\n\n");
		foreach (Faction item2 in Factions.Loop())
		{
			int factionBlasphemyAttitude2 = item2.GetFactionBlasphemyAttitude(Faction);
			if (factionBlasphemyAttitude2 != 0)
			{
				int num5 = GivesRep.VaryRep(factionBlasphemyAttitude2);
				if (num5 != 0)
				{
					The.Game.PlayerReputation.Modify(item2, num5, null, item2.Visible ? stringBuilder : stringBuilder2, "Blasphemy", !item2.Visible, Transient: false, SingleLine: false, Multiple: true);
				}
			}
		}
		The.Game.PlayerReputation.FinishModify();
		Popup.Show(stringBuilder.ToString());
	}

	[Obsolete("Use Get(), will be removed after Q2 2024")]
	public int get(Faction Faction)
	{
		return Get(Faction);
	}

	[Obsolete("Use Get(), will be removed after Q2 2024")]
	public int get(string faction)
	{
		return Get(faction);
	}

	[Obsolete("Use Set(), will be removed after Q2 2024")]
	public int set(Faction faction, int rep)
	{
		return Set(faction, rep);
	}

	[Obsolete("Use Set(), will be removed after Q2 2024")]
	public int set(string faction, int rep)
	{
		return Set(faction, rep);
	}

	[Obsolete("Use Modify(), will be removed after Q2 2024")]
	public float modify(Faction Faction, int Amount, string Because = null, StringBuilder PutMessage = null, string Type = null, bool Silent = false, bool Transient = false, bool SingleLine = false, bool Multiple = false)
	{
		return Modify(Faction, Amount, Because, PutMessage, Type, Silent, Transient, SingleLine, Multiple);
	}

	[Obsolete("Use Modify(), will be removed after Q2 2024")]
	public float modify(string Faction, int Amount, string Type = null, string Because = null, StringBuilder PutMessage = null, bool Silent = false, bool Transient = false)
	{
		return Modify(Faction, Amount, Type, Because, PutMessage, Silent, Transient);
	}

	[Obsolete("Use Modify(), will be removed after Q2 2024")]
	public float modify(string Faction, int Amount, bool DisplayMessage)
	{
		return Modify(Faction, Amount, DisplayMessage);
	}
}
