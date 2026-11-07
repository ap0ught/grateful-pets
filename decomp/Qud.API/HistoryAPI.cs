using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HistoryKit;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace Qud.API;

public static class HistoryAPI
{
	public static HistoricEntitySnapshot GetEntityCurrentSnapshot(string id)
	{
		return XRLCore.Core?.Game?.sultanHistory?.GetEntity(id)?.GetCurrentSnapshot();
	}

	public static string GetEntityName(string id)
	{
		if (string.Equals(id, "Joppa"))
		{
			return "Joppa";
		}
		if (string.Equals(id, "The Yd Freehold"))
		{
			return "the Yd Freehold";
		}
		if (string.Equals(id, "Kyakukya"))
		{
			return "Kyakukya";
		}
		return XRLCore.Core?.Game?.sultanHistory?.GetEntity(id)?.GetCurrentSnapshot()?.GetProperty("name");
	}

	public static HistoricEvent GetEvent(long id)
	{
		return XRLCore.Core?.Game?.sultanHistory?.GetEvent(id);
	}

	public static List<HistoricEntity> GetSultans()
	{
		History history = XRLCore.Core?.Game?.sultanHistory;
		if (history == null)
		{
			return new List<HistoricEntity>();
		}
		HistoricEntityList entitiesWithProperty = history.GetEntitiesWithProperty("isCandidate");
		if (entitiesWithProperty == null)
		{
			return new List<HistoricEntity>();
		}
		return entitiesWithProperty.entities;
	}

	public static List<string> GetSultanHatedFactions(HistoricEntity sultan)
	{
		return sultan.GetCurrentSnapshot().GetList("hatedFactions");
	}

	public static List<string> GetSultanLikedFactions(HistoricEntity sultan)
	{
		return sultan.GetCurrentSnapshot().GetList("likedFactions");
	}

	public static List<HistoricEntity> GetVillages()
	{
		return new List<HistoricEntity>(XRLCore.Core.Game.sultanHistory.GetEntitiesWherePropertyEquals("type", "village").entities);
	}

	public static List<HistoricEntity> GetKnownVillages()
	{
		List<HistoricEntity> list = new List<HistoricEntity>();
		foreach (HistoricEntity entity in GetVillages())
		{
			if (JournalAPI.VillageNotes.Any((JournalVillageNote note) => note.Revealed && note.VillageID == entity.id))
			{
				list.Add(entity);
			}
		}
		return list;
	}

	public static HistoricEntitySnapshot GetVillageSnapshot(string Faction)
	{
		History sultanHistory = The.Game.sultanHistory;
		if (Factions.TryGet(Faction, out var Faction2) && !Faction2.EntityID.IsNullOrEmpty() && sultanHistory.TryGetEntity(Faction2.EntityID, out var Entity))
		{
			return Entity.GetCurrentSnapshot();
		}
		return null;
	}

	public static string ExpandVillageText(string Text, string Faction = null, HistoricEntitySnapshot Snapshot = null)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder(Text);
		ExpandVillageText(stringBuilder, Faction, Snapshot);
		return stringBuilder.ToString();
	}

	public static void ExpandVillageText(StringBuilder Text, string Faction = null, HistoricEntitySnapshot Snapshot = null)
	{
		if (Snapshot == null && !Faction.IsNullOrEmpty())
		{
			Snapshot = GetVillageSnapshot(Faction);
		}
		string value = "a village";
		string value2 = "the act of procreating";
		string value3 = "those who oppose arable land and the act of procreating";
		string value4 = "roaming around idly";
		if (Snapshot != null)
		{
			Random stableRandomGenerator = Stat.GetStableRandomGenerator(Snapshot.Name);
			GameObjectBlueprint randomElement = GameObjectFactory.Factory.GetFactionMembers(Snapshot.GetProperty("baseFaction")).GetRandomElement(stableRandomGenerator);
			value = Snapshot.Name;
			value2 = Snapshot.GetRandomElementFromListProperty("sacredThings", null, stableRandomGenerator) ?? Snapshot.GetProperty("defaultSacredThing");
			value3 = Snapshot.GetRandomElementFromListProperty("profaneThings", null, stableRandomGenerator) ?? Snapshot.GetProperty("defaultProfaneThing");
			value4 = randomElement.GetxTag_CommaDelimited("TextFragments", "Activity", null, stableRandomGenerator);
		}
		Text.StartReplace().AddReplacer("village.name", value).AddReplacer("village.sacred", value2)
			.AddReplacer("village.profane", value3)
			.AddReplacer("village.activity", value4)
			.Execute();
	}

	public static HistoricEntitySnapshot GetSultanForPeriod(int period)
	{
		List<HistoricEntity> sultans = GetSultans();
		if (sultans == null || sultans.Count == 0)
		{
			return null;
		}
		foreach (HistoricEntity item in sultans)
		{
			string entityProperty = item.GetEntityProperty("period", -1L);
			if (!entityProperty.IsNullOrEmpty() && int.TryParse(entityProperty, out var result) && period == result)
			{
				return item.GetCurrentSnapshot();
			}
		}
		return null;
	}

	public static List<string> GetLikedFactionsForSultan(int period)
	{
		HistoricEntitySnapshot sultanForPeriod = GetSultanForPeriod(period);
		if (sultanForPeriod == null)
		{
			return new List<string>();
		}
		return new List<string>(sultanForPeriod.GetList("likedFactions"));
	}

	public static List<string> GetDislikedFactionsForSultan(int period)
	{
		HistoricEntitySnapshot sultanForPeriod = GetSultanForPeriod(period);
		if (sultanForPeriod == null)
		{
			return new List<string>();
		}
		return new List<string>(sultanForPeriod.GetList("hatedFactions"));
	}

	public static List<string> GetDomainsForSultan(int period)
	{
		HistoricEntitySnapshot sultanForPeriod = GetSultanForPeriod(period);
		if (sultanForPeriod == null)
		{
			return new List<string>();
		}
		return new List<string>(sultanForPeriod.GetList("elements"));
	}

	public static void OnWaterRitualShuffleSecrets(WaterRitualRecord Record, BallBag<IBaseJournalEntry> Notes)
	{
	}

	public static void OnWaterRitualBuySecret(WaterRitualRecord Record, ref IBaseJournalEntry Note)
	{
	}

	public static void OnWaterRitualSellSecret(WaterRitualRecord Record, List<IBaseJournalEntry> Notes)
	{
	}

	public static List<HistoricEntity> GetKnownSultans()
	{
		History sultanHistory = XRLCore.Core.Game.sultanHistory;
		List<HistoricEntity> list = new List<HistoricEntity>();
		foreach (HistoricEntity entity in sultanHistory.GetEntitiesWithProperty("isCandidate").entities)
		{
			if (AnyKnownSultanEventsWithGospelsOrTombPropaganda(entity.id))
			{
				list.Add(entity);
			}
		}
		return list;
	}

	public static List<HistoricEvent> GetKnownSultanEventsWithGospels(string sultanId)
	{
		List<HistoricEvent> list = new List<HistoricEvent>();
		foreach (HistoricEvent @event in XRLCore.Core.Game.sultanHistory.GetEntity(sultanId).events)
		{
			if (@event.HasEventProperty("gospel") && JournalAPI.KnowsSultanEvent(@event.id))
			{
				list.Add(@event);
			}
		}
		return list;
	}

	public static bool AnyKnownSultanEventsWithGospels(string sultanId)
	{
		foreach (HistoricEvent @event in XRLCore.Core.Game.sultanHistory.GetEntity(sultanId).events)
		{
			if (@event.HasEventProperty("gospel") && JournalAPI.KnowsSultanEvent(@event.id))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyKnownSultanEventsWithGospelsOrTombPropaganda(string sultanId)
	{
		foreach (HistoricEvent @event in XRLCore.Core.Game.sultanHistory.GetEntity(sultanId).events)
		{
			if ((@event.HasEventProperty("gospel") || @event.HasEventProperty("tombInscription")) && JournalAPI.KnowsSultanEvent(@event.id))
			{
				return true;
			}
		}
		return false;
	}

	public static HistoricEntity GetResheph()
	{
		return XRLCore.Core.Game.sultanHistory.GetEntityWithProperty("Resheph");
	}

	public static int GetFlipYear()
	{
		return int.Parse(GetResheph().GetEntityProperty("flipYear", -1L));
	}

	public static string GetSultanTerm()
	{
		return The.Game?.GetStringGameState("SultanTerm", null) ?? "sultan";
	}

	public static List<HistoricEvent> GetSultanEventsWithGospels(string sultanId)
	{
		List<HistoricEvent> list = new List<HistoricEvent>();
		foreach (HistoricEvent @event in XRLCore.Core.Game.sultanHistory.GetEntity(sultanId).events)
		{
			if (@event.HasEventProperty("gospel"))
			{
				list.Add(@event);
			}
		}
		return list;
	}

	public static List<HistoricEvent> GetSultanEventsWithTombInscriptions(string sultanId)
	{
		List<HistoricEvent> list = new List<HistoricEvent>();
		foreach (HistoricEvent @event in XRLCore.Core.Game.sultanHistory.GetEntity(sultanId).events)
		{
			if (@event.HasEventProperty("tombInscription"))
			{
				list.Add(@event);
			}
		}
		return list;
	}
}
