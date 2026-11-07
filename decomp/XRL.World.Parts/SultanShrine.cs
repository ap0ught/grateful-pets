#define NLOG_ALL
using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SultanShrine : IPart
{
	public long RevealedEventID = -1L;

	public int PresetLocation = -1;

	public int Period;

	public string Inscription = "";

	public string PresetGospelByProperty;

	public bool Initialized;

	public string RegionName;

	[NonSerialized]
	private HistoricEvent _RevealedEvent;

	public HistoricEvent RevealedEvent
	{
		get
		{
			if (_RevealedEvent == null && RevealedEventID != -1)
			{
				_RevealedEvent = The.Game.sultanHistory.GetEvent(RevealedEventID);
			}
			return _RevealedEvent;
		}
		set
		{
			_RevealedEvent = value;
			RevealedEventID = value?.id ?? (-1);
		}
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID && (ID != EnteredCellEvent.ID || Initialized))
		{
			return ID == PooledEvent<GetPointsOfInterestEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			string baseDisplayName = ParentObject.BaseDisplayName;
			string key = "Sultan Shrine " + baseDisplayName;
			string explanation = null;
			bool flag = true;
			PointOfInterest pointOfInterest = E.Find(key);
			if (pointOfInterest != null)
			{
				if (ParentObject.DistanceTo(E.Actor) < pointOfInterest.GetDistanceTo(E.Actor))
				{
					E.Remove(pointOfInterest);
					explanation = "nearest";
				}
				else
				{
					flag = false;
					pointOfInterest.Explanation = "nearest";
				}
			}
			if (flag)
			{
				E.Add(ParentObject, baseDisplayName, explanation, key, null, null, null, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public static string GetStatueForSultan(string SultanName, int Period)
	{
		string value = null;
		if (Period > 5 || SultanName == "Resheph")
		{
			value = "Terrain/sw_resheph_sultanstatue.bmp";
		}
		else
		{
			Dictionary<string, string> dictionary = The.Game.GetObjectGameState("SultanAssignedStatues") as Dictionary<string, string>;
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, string>();
				The.Game.SetObjectGameState("SultanAssignedStatues", dictionary);
			}
			if (!dictionary.TryGetValue(SultanName, out value))
			{
				int num = 0;
				do
				{
					value = PopulationManager.GenerateOne("SultanShrines_StatueTiles").Blueprint;
				}
				while (dictionary.ContainsValue(value) && num++ < 100);
				dictionary[SultanName] = value;
			}
		}
		return value;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!Initialized)
		{
			ParentObject.Render.Tile = "terrain/sw_tombstone_" + Stat.Random(1, 4) + ".bmp";
			ShrineInitialize();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!E.AutogetOnlyMode && E.Command != "Look" && HasUnrevealedSecret())
		{
			E.Command = "Look";
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterLookedAt");
		Registrar.Register("SpecialInit");
		base.Register(Object, Registrar);
	}

	public void ShrineInitialize()
	{
		if (Initialized)
		{
			return;
		}
		History history = null;
		HistoricEntityList historicEntityList = null;
		HistoricEntity historicEntity = null;
		HistoricEntitySnapshot historicEntitySnapshot = null;
		try
		{
			history = The.Game.sultanHistory;
			historicEntityList = history.GetEntitiesWherePropertyEquals("type", "sultan");
			if (HasPropertyOrTag("ForceSultan"))
			{
				historicEntityList.entities = new List<HistoricEntity> { history.GetEntity(GetPropertyOrTag("ForceSultan")) };
			}
			string text = null;
			if (PresetLocation != -1)
			{
				text = XRLCore.Core.Game.GetStringGameState("SultanDungeonPlacementOrder_" + PresetLocation);
			}
			if (text != null)
			{
				int num = 0;
				while (num < historicEntityList.entities.Count)
				{
					int num2 = 0;
					while (true)
					{
						if (num2 < historicEntityList.entities[num].events.Count)
						{
							if (historicEntityList.entities[num].events[num2].HasEventProperty("revealsRegion") && historicEntityList.entities[num].events[num2].GetEventProperty("revealsRegion") == text)
							{
								historicEntity = historicEntityList.entities[num];
								historicEntitySnapshot = historicEntity.GetCurrentSnapshot();
								RevealedEvent = historicEntityList.entities[num].events[num2];
								goto IL_02bb;
							}
							num2++;
							continue;
						}
						num++;
						break;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Logger.log.Error("SultanShrine initialization error:" + ex.ToString());
		}
		try
		{
			List<HistoricEntity> list = historicEntityList.entities.Shuffle();
			for (int i = 0; i < list.Count; i++)
			{
				if (Period != 0 && int.Parse(list[i].GetCurrentSnapshot().GetProperty("period")) != Period)
				{
					continue;
				}
				List<HistoricEvent> events = list[i].events;
				for (int j = 0; j < events.Count; j++)
				{
					historicEntity = list[i];
					historicEntitySnapshot = historicEntity.GetCurrentSnapshot();
					RevealedEvent = historicEntity.GetRandomEventWhereDelegate((HistoricEvent ev) => ev.HasEventProperty("gospel") && (PresetGospelByProperty == null || ev.HasEventProperty(PresetGospelByProperty)), Stat.Rnd);
					if (RevealedEvent != null)
					{
						break;
					}
				}
				if (RevealedEvent != null)
				{
					break;
				}
			}
			if (RevealedEvent == null)
			{
				HistoricEvent historicEvent = (RevealedEvent = historicEntity.GetRandomEventWhereDelegate((HistoricEvent ev) => ev.HasEventProperty("gospel"), Stat.Rnd));
			}
		}
		catch (Exception ex2)
		{
			Logger.log.Error("SultanShrine initialization error (2):" + ex2.ToString());
		}
		goto IL_02bb;
		IL_02bb:
		try
		{
			string text2 = null;
			if (ParentObject.HasTagOrProperty("HasPregeneratedName"))
			{
				text2 = ParentObject.GetPropertyOrTag("PregeneratedSultanName") ?? historicEntitySnapshot.GetProperty("name", "<unknown name>");
			}
			else
			{
				text2 = historicEntitySnapshot.GetProperty("name", "<unknown name>");
				ParentObject.DisplayName = "shrine to " + text2;
				string randomElementFromListProperty = historicEntitySnapshot.GetRandomElementFromListProperty("cognomen", null, Stat.Rand);
				if (randomElementFromListProperty != null)
				{
					Render render = ParentObject.Render;
					render.DisplayName = render.DisplayName + ", " + randomElementFromListProperty;
				}
			}
			if (!ParentObject.HasTagOrProperty("Worshippable") && !text2.IsNullOrEmpty() && text2[0] != '<')
			{
				ParentObject.SetStringProperty("Worshippable", "yes");
				ParentObject.SetStringProperty("WorshippedAs", text2);
				string property = historicEntitySnapshot.GetProperty("period");
				int result;
				if (text2 == "Resheph")
				{
					ParentObject.SetIntProperty("WorshipPower", 8);
				}
				else if (!property.IsNullOrEmpty() && int.TryParse(property, out result))
				{
					ParentObject.SetIntProperty("WorshipPower", 7 - result);
				}
				else
				{
					ParentObject.SetIntProperty("WorshipPower", 2);
				}
				if (text2 == "Resheph")
				{
					ParentObject.SetStringProperty("WorshipFaction", text2);
				}
				else
				{
					ParentObject.SetStringProperty("WorshipFaction", Faction.GetSultanFactionName(property));
				}
			}
			ParentObject.GetPart<Description>().Short = "The shrine depicts a significant event from the life of the ancient sultan " + text2 + ":\n\n" + RevealedEvent.GetEventProperty("gospel", "<unknown gospel>");
			if (historicEntitySnapshot != null)
			{
				string statueForSultan = GetStatueForSultan(text2, Period);
				if (!statueForSultan.IsNullOrEmpty())
				{
					ParentObject.Render.Tile = statueForSultan;
				}
				if (PresetLocation == 0)
				{
					foreach (JournalSultanNote sultanNote in JournalAPI.GetSultanNotes())
					{
						if (sultanNote.SultanID == historicEntitySnapshot.entity.id)
						{
							sultanNote.Attributes.Add("include:Joppa");
						}
					}
				}
			}
		}
		catch (Exception ex3)
		{
			Logger.log.Error("SultanShrine initialization error (3):" + ex3.ToString());
		}
		Initialized = true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SpecialInit")
		{
			ShrineInitialize();
		}
		else if (E.ID == "AfterLookedAt")
		{
			ShrineInitialize();
			RevealBasedOnHistoricalEvent(RevealedEvent, PresetLocation, PresetGospelByProperty, ParentObject.DisplayName);
		}
		return base.FireEvent(E);
	}

	public static void RevealBasedOnHistoricalEvent(HistoricEvent ev, int PresetLocation = -1, string PresetGospelByProperty = null, string LearnedFrom = null)
	{
		JournalSultanNote journalSultanNote = ev.Reveal(LearnedFrom);
		if ((PresetLocation == 0 || PresetGospelByProperty == "JoppaShrine") && journalSultanNote != null && !journalSultanNote.Has("nobuy:Joppa"))
		{
			journalSultanNote.Attributes.Add("nobuy:Joppa");
			Faction faction = Factions.Get("Joppa");
			if (faction.Visible)
			{
				journalSultanNote.History = journalSultanNote.History + " {{K|-known by " + faction.GetFormattedName() + "}}";
			}
		}
	}

	public bool HasUnrevealedSecret()
	{
		if (RevealedEvent != null)
		{
			return RevealedEvent.HasUnrevealedSecret();
		}
		return false;
	}
}
