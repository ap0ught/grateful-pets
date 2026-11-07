using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GivesRep : IPart
{
	public bool wasParleyed;

	public int repValue = 100;

	[NonSerialized]
	public List<FriendorFoe> relatedFactions = new List<FriendorFoe>();

	private bool KillRepDone;

	private static List<Faction> LovedCache = new List<Faction>();

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		GivesRep givesRep = (GivesRep)base.DeepCopy(Parent, MapInv);
		givesRep.relatedFactions = new List<FriendorFoe>(relatedFactions.Count);
		foreach (FriendorFoe relatedFaction in relatedFactions)
		{
			givesRep.relatedFactions.Add(new FriendorFoe(relatedFaction));
		}
		return givesRep;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.Write(relatedFactions.Count);
		foreach (FriendorFoe relatedFaction in relatedFactions)
		{
			Writer.Write(relatedFaction.faction);
			Writer.Write(relatedFaction.status);
			Writer.Write(relatedFaction.reason);
		}
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string faction = Reader.ReadString();
			string status = Reader.ReadString();
			string reason = Reader.ReadString();
			relatedFactions.Add(new FriendorFoe(faction, status, reason));
		}
		base.Read(Basis, Reader);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != CanGiveDirectionsEvent.ID && ID != PooledEvent<GetPointsOfInterestEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			E.Add(ParentObject, ParentObject.GetReferenceDisplayName());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanGiveDirectionsEvent E)
	{
		if (E.SpeakingWith == ParentObject && !E.PlayerCompanion && !ParentObject.HasEffect<Lost>() && !ParentObject.HasEffect<Confused>() && !ParentObject.HasEffect<FuriouslyConfused>())
		{
			E.CanGiveDirections = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		AppendReputationDescription(E.Postfix);
		if (wasParleyed)
		{
			E.Postfix.Compound("You are water-bonded with " + ParentObject.GetPronounProvider().Objective + ".", "\n");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (E.Killer != null && E.Killer.IsPlayerControlled() && !KillRepDone)
		{
			KillRepDone = true;
			if (wasParleyed && E.Killer != ParentObject && (!E.Accidental || ParentObject.IsHostileTowards(E.Killer)))
			{
				Achievement.VIOLATE_WATER_RITUAL.Unlock();
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("You violated the covenant of the water ritual and killed your bonded kith. You are cursed.\n\n");
				foreach (Faction item in Factions.Loop())
				{
					if (item.Visible && !item.HatesPlayer)
					{
						The.Game.PlayerReputation.Modify(item, VaryRep(-100), null, stringBuilder, "WaterRitualCurse", Silent: false, Transient: false, SingleLine: true, Multiple: true);
					}
				}
				The.Game.PlayerReputation.FinishModify();
				Popup.Show(stringBuilder.ToString());
			}
			foreach (Faction lovedFaction in GetLovedFactions(25, VisibleOnly: true, WithoutReason: true))
			{
				The.Game.PlayerReputation.Modify(lovedFaction, VaryRep(-repValue * 2), null, null, "LegendaryFactionAward");
			}
			foreach (FriendorFoe relatedFaction in relatedFactions)
			{
				if (Factions.Get(relatedFaction.faction).Visible)
				{
					if (relatedFaction.status == "love")
					{
						The.Game.PlayerReputation.Modify(relatedFaction.faction, VaryRep(-repValue * 2), "LegendaryFactionAward");
					}
					else if (relatedFaction.status == "friend")
					{
						The.Game.PlayerReputation.Modify(relatedFaction.faction, VaryRep(-repValue * 2), "LegendaryFriendAward");
					}
					else if (relatedFaction.status == "dislike")
					{
						The.Game.PlayerReputation.Modify(relatedFaction.faction, VaryRep(repValue), "LegendaryDislikeAward");
					}
					else if (relatedFaction.status == "hate")
					{
						The.Game.PlayerReputation.Modify(relatedFaction.faction, VaryRep(repValue * 2), "LegendaryHateAward");
					}
					else
					{
						Debug.LogError("Unknown status " + relatedFaction.status + " for " + relatedFaction.faction);
					}
				}
			}
			string referenceDisplayName = ParentObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: true);
			if (wasParleyed)
			{
				JournalAPI.AddAccomplishment("You slew your bonded kith " + referenceDisplayName + ", violating the covenant of the water ritual and earning the emnity of all.", "Blasphemously, the traitor " + referenceDisplayName + " attacked =name=, " + ParentObject.its + " water-sib, and =name= was forced to slay " + ParentObject.them + ". Deep in grief, =name= wept for one year.", $"In the month of {Calendar.GetMonth()} of {Calendar.GetYear()}, =name= was challenged by <spice.commonPhrases.pretender.!random.article> to a duel over the rights of {Factions.GetMostLikedFormattedName()}. =name= won and murdered the pretender before tragically realizing <spice.pronouns.subject.!random> was {The.Player.GetPronounProvider().PossessiveAdjective} water-sib.", null, "general", MuralCategory.Slays, MuralWeight.High, null, -1L);
			}
			else
			{
				JournalAPI.AddAccomplishment("You slew " + referenceDisplayName + ".", HistoricStringExpander.ExpandString("In the month of " + Calendar.GetMonth() + " of " + Calendar.GetYear() + ", brave =name= slew <spice.commonPhrases.odious.!random> " + referenceDisplayName + " in single combat."), $"In the month of {Calendar.GetMonth()} of {Calendar.GetYear()}, =name= was challenged by <spice.commonPhrases.pretender.!random.article> to a duel over the rights of {Factions.GetMostLikedFormattedName()}. =name= won and murdered the pretender <spice.elements.{The.Player.GetMythicDomain()}.murdermethods.!random>.", null, "general", MuralCategory.Slays, MuralWeight.Low, null, -1L);
				ItemNaming.Opportunity(E.Killer, ParentObject, null, null, "Kill", 6, 0, 0, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("FactionsAdded");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "FactionsAdded")
		{
			FillInRelatedFactions(Initial: true);
		}
		return base.FireEvent(E);
	}

	public List<Faction> GetLovedFactions(int MinimumMembership = 25, bool VisibleOnly = false, bool WithoutReason = false)
	{
		List<Faction> lovedCache = LovedCache;
		lovedCache.Clear();
		foreach (KeyValuePair<string, int> item in ParentObject.Brain.GetBaseAllegiance())
		{
			Faction ifExists = Factions.GetIfExists(item.Key);
			if (ifExists != null && item.Value >= MinimumMembership && (!VisibleOnly || ifExists.Visible))
			{
				lovedCache.Add(ifExists);
			}
		}
		string value = ParentObject.GetxTag("Reputation", "LovedBy");
		if (value.IsNullOrEmpty())
		{
			return lovedCache;
		}
		DelimitedEnumeratorChar enumerator2 = value.DelimitedBy(',').GetEnumerator();
		while (enumerator2.MoveNext())
		{
			enumerator2.Current.Split(':', out var First, out var Second);
			if (!WithoutReason || Second.IsEmpty)
			{
				Faction ifExists2 = Factions.GetIfExists(new string(First));
				if (ifExists2 != null && (!VisibleOnly || ifExists2.Visible))
				{
					lovedCache.Add(ifExists2);
				}
			}
		}
		return lovedCache;
	}

	public void DumpRelatedFactions(string When)
	{
	}

	public void ResetRelatedFactions()
	{
		Brain brain = ParentObject.Brain;
		foreach (FriendorFoe relatedFaction in relatedFactions)
		{
			int num = 0;
			if (relatedFaction.status == "friend" || relatedFaction.status == "love")
			{
				num = 100;
			}
			else if (relatedFaction.status == "dislike")
			{
				num = -50;
			}
			else if (relatedFaction.status == "hate")
			{
				num = -100;
			}
			if (brain.FactionFeelings.ContainsKey(relatedFaction.faction))
			{
				if (brain.FactionFeelings[relatedFaction.faction] == num)
				{
					brain.FactionFeelings.Remove(relatedFaction.faction);
				}
				else
				{
					brain.FactionFeelings[relatedFaction.faction] -= num;
				}
			}
		}
		relatedFactions.Clear();
	}

	public int FillInRelatedFactions(bool Initial = false)
	{
		string dice = ParentObject.GetxTag("Reputation", "RelatedAmount", "1d3");
		return FillInRelatedFactions(dice.RollCached(), Initial);
	}

	public int FillInRelatedFactions(int FactionCount, bool Initial = false)
	{
		int num = 0;
		List<string[]> list = new List<string[]>(3);
		if (HasPropertyOrTag("staticFaction1"))
		{
			list.Add(GetPropertyOrTag("staticFaction1").Split(','));
			num++;
		}
		if (HasPropertyOrTag("staticFaction2"))
		{
			list.Add(GetPropertyOrTag("staticFaction2").Split(','));
			num++;
		}
		if (HasPropertyOrTag("staticFaction3"))
		{
			list.Add(GetPropertyOrTag("staticFaction3").Split(','));
			num++;
		}
		num += AddStaticXTagFactions(list, "LovedBy", "love");
		num += AddStaticXTagFactions(list, "LikedBy", "friend");
		num += AddStaticXTagFactions(list, "DislikedBy", "dislike");
		num += AddStaticXTagFactions(list, "HatedBy", "hate");
		while (list.Count < 3)
		{
			list.Add(new string[3]);
		}
		string text = ParentObject.GetPropertyOrTag("NoHateFactions") ?? ParentObject.GetxTag("Reputation", "NotDislikedBy");
		string text2 = ParentObject.GetPropertyOrTag("NoFriendFactions") ?? ParentObject.GetxTag("Reputation", "NotLikedBy");
		if (FactionCount < num)
		{
			FactionCount = num;
		}
		Brain brain = ParentObject.Brain;
		for (int i = 1; i <= FactionCount; i++)
		{
			FriendorFoe friendorFoe = ((relatedFactions.Count >= i) ? relatedFactions[i - 1] : new FriendorFoe());
			string[] array = list[i - 1];
			string text3;
			if (friendorFoe.faction.IsNullOrEmpty())
			{
				text3 = ((!array[0].IsNullOrEmpty()) ? (friendorFoe.faction = array[0]) : (friendorFoe.faction = GenerateFriendOrFoe.getRandomFaction(ParentObject)));
			}
			else
			{
				text3 = friendorFoe.faction;
			}
			string text4;
			if (!friendorFoe.status.IsNullOrEmpty())
			{
				text4 = friendorFoe.status;
			}
			else if (!array[1].IsNullOrEmpty())
			{
				text4 = (friendorFoe.status = array[1]);
			}
			else
			{
				int num2 = Stat.Random(1, 100);
				ref string status = ref friendorFoe.status;
				string text5;
				if (num2 <= 10)
				{
					text5 = "friend";
				}
				else
				{
					text5 = ((num2 > 55) ? "hate" : "dislike");
				}
				text4 = (status = text5);
			}
			if (text4 != "love" && text4 != "friend" && text4 != "dislike" && text4 != "hate")
			{
				Debug.LogWarning("had unknown status '" + text4 + "', using dislike");
				text4 = (friendorFoe.status = "dislike");
			}
			if (text != null && (text4 == "dislike" || text4 == "hate") && text.Contains(text3))
			{
				text4 = (friendorFoe.status = "friend");
			}
			if (text2 != null && (text4 == "friend" || text4 == "love") && text2.Contains(text3))
			{
				text4 = (friendorFoe.status = "dislike");
			}
			string value = null;
			if (!friendorFoe.reason.IsNullOrEmpty())
			{
				value = friendorFoe.reason;
			}
			else if (!array[2].IsNullOrEmpty())
			{
				value = (friendorFoe.reason = array[2]);
			}
			switch (text4)
			{
			case "friend":
				if (brain.FactionFeelings.ContainsKey(text3))
				{
					if (Initial)
					{
						brain.FactionFeelings[text3] += 100;
					}
				}
				else
				{
					brain.FactionFeelings.Add(text3, 100);
				}
				if (value.IsNullOrEmpty())
				{
					if (ParentObject.HasTag("StaticLikeReason"))
					{
						value = ParentObject.GetTag("StaticLikeReason");
					}
					else
					{
						value = ((!ParentObject.BelongsToFaction("Entropic")) ? GenerateFriendOrFoe.getLikeReason() : GenerateFriendOrFoe_HEB.getLikeReason());
					}
					friendorFoe.reason = value;
				}
				break;
			case "love":
				if (brain.FactionFeelings.ContainsKey(text3))
				{
					if (Initial)
					{
						brain.FactionFeelings[text3] += 100;
					}
				}
				else
				{
					brain.FactionFeelings.Add(text3, 100);
				}
				break;
			case "dislike":
				if (brain.FactionFeelings.ContainsKey(text3))
				{
					if (Initial)
					{
						brain.FactionFeelings[text3] -= 50;
					}
				}
				else
				{
					brain.FactionFeelings.Add(text3, -50);
				}
				if (value.IsNullOrEmpty())
				{
					if (ParentObject.HasTag("StaticHateReason"))
					{
						value = ParentObject.GetTag("StaticHateReason");
					}
					else
					{
						value = ((!ParentObject.BelongsToFaction("Entropic")) ? GenerateFriendOrFoe.getHateReason() : GenerateFriendOrFoe_HEB.getHateReason());
					}
					friendorFoe.reason = value;
				}
				break;
			case "hate":
				if (brain.FactionFeelings.ContainsKey(text3))
				{
					if (Initial)
					{
						brain.FactionFeelings[text3] -= 100;
					}
				}
				else
				{
					brain.FactionFeelings.Add(text3, -100);
				}
				if (value.IsNullOrEmpty())
				{
					if (ParentObject.HasTag("StaticHateReason"))
					{
						value = ParentObject.GetTag("StaticHateReason");
					}
					else
					{
						value = ((!ParentObject.BelongsToFaction("Entropic")) ? GenerateFriendOrFoe.getHateReason() : GenerateFriendOrFoe_HEB.getHateReason());
					}
					friendorFoe.reason = value;
				}
				break;
			default:
				throw new Exception("internal inconsistency");
			}
			if (relatedFactions.Count < i)
			{
				relatedFactions.Add(friendorFoe);
			}
		}
		return FactionCount;
	}

	private int AddStaticXTagFactions(List<string[]> Factions, string Tag, string Status)
	{
		int num = 0;
		string value = ParentObject.GetxTag("Reputation", Tag);
		if (!value.IsNullOrEmpty())
		{
			DelimitedEnumeratorChar enumerator = value.DelimitedBy(',').GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReadOnlySpan<char> current = enumerator.Current;
				string[] array = new string[3] { null, Status, null };
				current.Split(':', out var First, out var Second);
				array[0] = ((First.Length == 0) ? null : new string(First));
				array[2] = ((Second.Length == 0) ? null : new string(Second));
				Factions.Add(array);
				num++;
			}
		}
		return num;
	}

	public static int VaryRep(int Rep)
	{
		float num = ((Math.Abs(Rep) >= 100) ? Stat.Random(-5, 5) : Stat.Random(-25, 25));
		float num2 = Rep;
		num2 += 0.1f * num2 * num / 5f;
		num2 = 5 * (int)Math.Round(num2 / 5f);
		return (int)num2;
	}

	[Obsolete("use VaryRep(), will be removed after Q2 2024")]
	public static int varyRep(int Rep)
	{
		return VaryRep(Rep);
	}

	private int AddStaticXTagFactions(List<string[]> Factions, string Tag, string Status, bool RequireReason = false)
	{
		int num = 0;
		string value = ParentObject.GetxTag("Reputation", Tag);
		if (!value.IsNullOrEmpty())
		{
			DelimitedEnumeratorChar enumerator = value.DelimitedBy(',').GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Split(':', out var First, out var Second);
				if (!RequireReason || !Second.IsEmpty)
				{
					string[] array = new string[3] { null, Status, null };
					array[0] = ((First.Length == 0) ? null : new string(First));
					array[2] = ((Second.Length == 0) ? null : new string(Second));
					Factions.Add(array);
					num++;
				}
			}
		}
		return num;
	}

	public static int VaryRepUp(int Rep)
	{
		float num = Stat.Random(0, 10);
		float num2 = Rep;
		num2 += 0.1f * num2 * num / 10f;
		num2 = 10 * (int)Math.Round(num2 / 10f);
		return (int)num2;
	}

	[Obsolete("use VaryRepUp(), will be removed after Q2 2024")]
	public static int varyRepUp(int Rep)
	{
		return VaryRepUp(Rep);
	}

	public void AppendReputationDescription(StringBuilder SB)
	{
		if (SB.Length > 0 && SB[SB.Length - 1] != '\n')
		{
			SB.Append('\n');
		}
		List<string> list = new List<string>(2);
		foreach (Faction lovedFaction in GetLovedFactions(25, VisibleOnly: true, WithoutReason: true))
		{
			string text = Markup.Color("C", lovedFaction.GetFormattedName());
			if (Options.GivesRepShowsCurrentRep)
			{
				string text2 = Markup.Color("K", $"({Faction.PlayerReputation.Get(lovedFaction.Name)})");
				list.Add(text + text2);
			}
			else
			{
				list.Add(text);
			}
		}
		bool flag = false;
		if (list.Count > 0)
		{
			SB.Append("{{C|-----}}");
			flag = true;
			SB.Append("\nLoved by ").Append(Grammar.MakeAndList(list)).Append(".\n");
		}
		foreach (FriendorFoe relatedFaction in relatedFactions)
		{
			Faction ifExists = Factions.GetIfExists(relatedFaction.faction);
			if (ifExists != null && ifExists.Visible && !relatedFaction.reason.IsNullOrEmpty())
			{
				if (!flag)
				{
					SB.Append("{{C|-----}}");
					flag = true;
				}
				SB.Append('\n');
				if (relatedFaction.status == "love")
				{
					SB.Append("Loved");
				}
				if (relatedFaction.status == "friend")
				{
					SB.Append("Admired");
				}
				else if (relatedFaction.status == "dislike")
				{
					SB.Append("Disliked");
				}
				else if (relatedFaction.status == "hate")
				{
					SB.Append("Hated");
				}
				SB.Append(" by {{C|").Append(Faction.GetFormattedName(relatedFaction.faction)).Append("}}");
				if (Options.GivesRepShowsCurrentRep)
				{
					SB.Append(Markup.Color("K", $"({Faction.PlayerReputation.Get(relatedFaction.faction)})"));
				}
				SB.Append(" for ").Append(relatedFaction.reason).Append(".\n");
			}
		}
	}
}
