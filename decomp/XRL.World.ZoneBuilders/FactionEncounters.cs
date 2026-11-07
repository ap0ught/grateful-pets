using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

[HasWishCommand]
public class FactionEncounters : ZoneBuilderSandbox
{
	public string Population = "GenericFactionPopulation";

	public int Chance = 2;

	public int Rolls = 2;

	public static bool BuildFactionEncounter(string Faction, Zone Z, int ZoneLevel, int ZoneTier)
	{
		List<GameObject> list = Event.NewGameObjectList();
		GameObject gameObject = null;
		int num = ((!Data.TryGetText("FactionEncounterNumber_" + Faction, out var Result)) ? Data.GetText("FactionEncounterNumber_*Default").RollCached() : Result.RollCached());
		if (Faction == "Pariahs")
		{
			list.Add(PariahSpawner.GeneratePariah(Math.Max(1, Stat.Random(ZoneLevel - 4, ZoneLevel + 4)), AlterName: false));
			for (int i = 1; i < num; i++)
			{
				list.Add(PariahSpawner.GeneratePariah(Math.Max(1, Stat.Random(ZoneLevel - 4, ZoneLevel + 4))));
			}
			gameObject = list[0];
			if (The.Game.GetStringGameState("SlynthSettlementFaction") == Faction)
			{
				int j = 0;
				for (int num2 = Stat.Random(1, 3); j < num2; j++)
				{
					GameObject pariah = GameObject.Create("BaseSlynth");
					list.Add(PariahSpawner.GeneratePariah(pariah, AlterName: false));
				}
			}
		}
		else
		{
			BallBag<string> ballBag = new BallBag<string>();
			foreach (GameObjectBlueprint factionMember in GameObjectFactory.Factory.GetFactionMembers(Faction))
			{
				if (Math.Abs(ZoneLevel - ZoneBuilderSandbox.GetLevelOfObject(factionMember.Name)) <= 10)
				{
					ballBag.Add(factionMember.Name, Math.Max(5, 25 - Math.Abs(ZoneLevel - ZoneBuilderSandbox.GetLevelOfObject(factionMember.Name))));
				}
			}
			if (ballBag.Count == 0)
			{
				return false;
			}
			for (int k = 0; k < num; k++)
			{
				string blueprint = ballBag.PeekOne();
				list.Add(GameObject.Create(blueprint));
				if (k == 0)
				{
					gameObject = list[k];
				}
			}
		}
		string populationName = ZoneBuilderSandbox.PopulationOr("FactionEncounterPartyObjects_" + Faction, "FactionEncounterPartyObjects_*Default");
		string populationName2 = ZoneBuilderSandbox.PopulationOr("FactionEncounterZoneObjects_" + Faction, "FactionEncounterZoneObjects_*Default");
		string table = ZoneBuilderSandbox.PopulationOr("FactionEncounterMemberInventory_" + Faction, "FactionEncounterMemberInventory_*Default");
		string table2 = ZoneBuilderSandbox.PopulationOr("FactionEncounterLeaderInventory_" + Faction, "FactionEncounterLeaderInventory_*Default");
		if (Faction.Equals("Templar") && gameObject.Blueprint == "Templar Squire")
		{
			gameObject = GameObject.Create("Knight Templar");
			list.Add(gameObject);
		}
		HeroMaker.MakeHero(gameObject, "BaseFactionHeroTemplate_" + Faction, "SpecialFactionHeroTemplate_" + Faction);
		if (Faction == "Barathrumites")
		{
			gameObject.AddSkill("Tinkering");
			gameObject.AddSkill("Tinkering_Repair");
			gameObject.AddSkill("Tinkering_Tinker1");
			gameObject.AddSkill("Tinkering_Tinker2");
		}
		foreach (GameObject item in list)
		{
			if (item != gameObject)
			{
				item.SetAlliedLeader<AllyRetinue>(gameObject);
				item.EquipFromPopulationTable(table, ZoneTier);
			}
			else
			{
				item.EquipFromPopulationTable(table2, ZoneTier);
			}
		}
		foreach (PopulationResult item2 in PopulationManager.Generate(populationName, "zonetier", ZoneTier.ToString()))
		{
			for (int l = 0; l < item2.Number; l++)
			{
				GameObject gameObject2 = GameObject.Create(item2.Blueprint);
				if (!string.IsNullOrEmpty(item2.Hint))
				{
					gameObject2.SetStringProperty("PlacementHint", item2.Hint);
				}
				list.Add(gameObject2);
			}
		}
		ZoneBuilderSandbox.PlaceParty(list, Z);
		foreach (PopulationResult item3 in PopulationManager.Generate(populationName2, "zonetier", ZoneTier.ToString()))
		{
			for (int m = 0; m < item3.Number; m++)
			{
				ZoneBuilderSandbox.PlaceObjectInArea(Z, Z.area, GameObject.Create(item3.Blueprint), 0, 0, item3.Hint);
			}
		}
		return true;
	}

	public bool BuildZone(Zone Z)
	{
		int num = 0;
		for (int i = 0; i < Rolls; i++)
		{
			if (Chance.in100())
			{
				num++;
			}
		}
		if (num > 0)
		{
			int level = Z.Level;
			int newTier = Z.NewTier;
			for (int j = 0; j < num; j++)
			{
				BuildFactionEncounter(PopulationManager.RollOneFrom(Population).Blueprint, Z, level, newTier);
			}
		}
		return true;
	}

	[WishCommand(null, null, Regex = "^factionencounter:\\s*(.*?)(?::(\\d+)){0,2}$")]
	public static bool HandleFactionEncounterWish(Match match)
	{
		string value = match.Groups[1].Value;
		Zone currentZone = The.Player.CurrentZone;
		int zoneLevel = currentZone.Level;
		int num = currentZone.NewTier;
		if (match.Groups[2].Success)
		{
			try
			{
				num = Convert.ToInt32(match.Groups[2].Value);
				zoneLevel = num * 5;
			}
			catch
			{
			}
		}
		if (match.Groups[3].Success)
		{
			try
			{
				zoneLevel = Convert.ToInt32(match.Groups[3].Value);
			}
			catch
			{
			}
		}
		if (!BuildFactionEncounter(value, currentZone, zoneLevel, num))
		{
			Popup.Show("No members found for '" + value + "'.");
		}
		return true;
	}
}
