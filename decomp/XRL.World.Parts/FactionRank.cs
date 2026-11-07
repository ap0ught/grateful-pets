using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class FactionRank : IPart
{
	public string Ranks;

	public override bool SameAs(IPart Part)
	{
		if ((Part as FactionRank).Ranks != Ranks)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetFactionRankEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetFactionRankEvent E)
	{
		if (!Ranks.IsNullOrEmpty() && GameObject.Validate(E.Object) && !E.Object.IsPlayer() && Ranks.CachedDictionaryExpansion().TryGetValue(E.Faction, out var value) && (E.Rank == null || Faction.GetRankStanding(E.Faction, value) > Faction.GetRankStanding(E.Faction, E.Rank)))
		{
			E.Rank = value;
		}
		return base.HandleEvent(E);
	}

	public bool PromoteIfBelow(string Faction, string Rank, bool Message = false, bool IgnoreVisibility = false, bool Capitalize = true)
	{
		if (Rank.IsNullOrEmpty())
		{
			MetricsManager.LogError("rank was empty");
			return false;
		}
		if (Rank.Contains("::"))
		{
			MetricsManager.LogError("cannot track rank containing double colon, had " + Rank);
			return false;
		}
		if (Rank.Contains(";;"))
		{
			MetricsManager.LogError("cannot track rank containing double semicolon, had " + Rank);
			return false;
		}
		bool flag = false;
		Faction faction = null;
		if (Ranks.IsNullOrEmpty())
		{
			Ranks = Faction + "::" + Rank;
			flag = true;
		}
		else
		{
			Dictionary<string, string> dictionary = Ranks.CachedDictionaryExpansion();
			if (dictionary.TryGetValue(Faction, out var value))
			{
				if (value != Rank)
				{
					if (faction == null)
					{
						faction = Factions.Get(Faction);
					}
					if (faction.GetRankStanding(value) < faction.GetRankStanding(Rank))
					{
						Dictionary<string, string> dictionary2 = new Dictionary<string, string>(dictionary);
						dictionary2[Faction] = Rank;
						Ranks = dictionary2.ToStringForCachedDictionaryExpansion();
						flag = true;
					}
				}
			}
			else
			{
				Ranks = Ranks + ";;" + Faction + "::" + Rank;
				flag = true;
			}
		}
		if ((flag & Message) && (IgnoreVisibility || Visible()))
		{
			if (faction == null)
			{
				faction = Factions.Get(Faction);
			}
			DidX("are", "promoted to the " + faction.GetRankTerm() + " of " + (Capitalize ? Rank.Capitalize() : Rank), null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: true);
		}
		return flag;
	}
}
