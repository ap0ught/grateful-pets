using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class Memorial : IPart
{
	public string Eulogies;

	public string UseDefaultEulogyIfPrimaryFaction;

	public bool OnlyIfNamed;

	public bool GenerateNameIfNotNamed;

	public override bool SameAs(IPart Part)
	{
		Memorial memorial = Part as Memorial;
		if (memorial.Eulogies != Eulogies)
		{
			return false;
		}
		if (memorial.UseDefaultEulogyIfPrimaryFaction != UseDefaultEulogyIfPrimaryFaction)
		{
			return false;
		}
		if (memorial.OnlyIfNamed != OnlyIfNamed)
		{
			return false;
		}
		if (memorial.GenerateNameIfNotNamed != GenerateNameIfNotNamed)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == OnDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(OnDeathRemovalEvent E)
	{
		if (!OnlyIfNamed || ParentObject.HasProperName)
		{
			string text = ParentObject.GetPrimaryFaction();
			if (!Eulogies.IsNullOrEmpty())
			{
				foreach (KeyValuePair<string, string> item in Eulogies.CachedDictionaryExpansion())
				{
					Faction ifExists = Factions.GetIfExists(item.Key);
					if (ifExists != null)
					{
						if (ifExists.QueueMemorial(ParentObject, GenerateNameIfNotNamed, item.Value, E.Reason, E.ThirdPersonReason) != null && ifExists.Name == text)
						{
							text = null;
						}
					}
					else
					{
						MetricsManager.LogError("Memorial had nonexistent faction " + item.Key);
					}
				}
			}
			if (!text.IsNullOrEmpty() && UseDefaultEulogyIfPrimaryFaction == text)
			{
				Factions.GetIfExists(text).QueueMemorial(ParentObject, GenerateNameIfNotNamed, null, E.Reason, E.ThirdPersonReason);
			}
		}
		return base.HandleEvent(E);
	}

	public void AddEulogy(string Faction, string Eulogy)
	{
		if (Faction.IsNullOrEmpty())
		{
			MetricsManager.LogError("faction for Memorial.AddEulogy() was null/empty");
			return;
		}
		if (Factions.GetIfExists(Faction) == null)
		{
			MetricsManager.LogError("no such faction: " + Faction);
			return;
		}
		if (Eulogy.Contains(";;"))
		{
			MetricsManager.LogError("cannot track eulogy containing double semicolon, replacing with emdash");
			Eulogy = Eulogy.Replace(";;", "--");
		}
		if (Eulogy.Contains("::"))
		{
			MetricsManager.LogError("cannot track eulogy containing double colon, replacing with ellipsis");
			Eulogy = Eulogy.Replace("::", "...");
		}
		if (Eulogies.IsNullOrEmpty())
		{
			Eulogies = Faction + "::" + Eulogy;
			return;
		}
		Dictionary<string, string> dictionary = Eulogies.CachedDictionaryExpansion();
		if (dictionary.TryGetValue(Faction, out var _))
		{
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>(dictionary);
			dictionary2[Faction] = Eulogy;
			Eulogies = dictionary2.ToStringForCachedDictionaryExpansion();
			return;
		}
		Eulogies = Eulogies + ";;" + Faction + "::" + Eulogy;
	}
}
