using System.Collections.Generic;
using HistoryKit;
using Qud.API;

namespace XRL.World.Parts;

public static class Gossip
{
	public static string GenerateGossip_OneFaction(string faction)
	{
		History sultanHistory = The.Game.sultanHistory;
		if (50.in100())
		{
			return GenerateGossip_TwoFactions(faction, HistoricStringExpander.ExpandString("some <spice.commonPhrases.group.!random>", null, sultanHistory));
		}
		return GenerateGossip_TwoFactions(HistoricStringExpander.ExpandString("<spice.commonPhrases.someone.!random>", null, sultanHistory), faction);
	}

	public static string GenerateGossip_TwoFactions(string actor, string actee)
	{
		History sultanHistory = The.Game.sultanHistory;
		string text = HistoricStringExpander.ExpandString("<spice.gossip.twoFaction.!random>", null, sultanHistory);
		if (text.Contains("@item"))
		{
			GameObject gameObject = GameObject.Create(EncountersAPI.GetARandomDescendantOf("Item"));
			text = text.Replace("@item.a", gameObject.a).Replace("@item.name", gameObject.DisplayNameOnlyStripped);
		}
		List<string> factionNames = Factions.GetFactionNames();
		if (factionNames.Contains(actor))
		{
			if (40.in100())
			{
				List<GameObjectBlueprint> factionMembers = GameObjectFactory.Factory.GetFactionMembers(actor);
				actor = ((factionMembers.Count <= 0) ? ((!Faction.GetFormattedName(actor).IsNullOrEmpty()) ? Faction.GetFormattedName(actor) : actor) : GameObject.Create(factionMembers.GetRandomElement().Name).an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true));
			}
			else
			{
				string formattedName = Faction.GetFormattedName(actor);
				actor = ((!formattedName.IsNullOrEmpty()) ? formattedName : actor);
			}
		}
		if (factionNames.Contains(actee))
		{
			if (40.in100())
			{
				List<GameObjectBlueprint> factionMembers2 = GameObjectFactory.Factory.GetFactionMembers(actee);
				actee = ((factionMembers2.Count <= 0) ? ((!Faction.GetFormattedName(actee).IsNullOrEmpty()) ? Faction.GetFormattedName(actee) : actee) : GameObject.Create(factionMembers2.GetRandomElement().Name).an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true));
			}
			else
			{
				actee = ((!Faction.GetFormattedName(actee).IsNullOrEmpty()) ? Faction.GetFormattedName(actee) : actee);
			}
		}
		return text.Replace("*f1*", actor).Replace("*f2*", actee);
	}
}
