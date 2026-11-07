using System;
using HistoryKit;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class VillageSurface : IPart
{
	public string VillageName;

	public string RevealString;

	public string RevealSecret;

	public string RevealKey;

	public bool IsVillageZero;

	public Vector2i RevealLocation;

	public int region = -1;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != ZoneActivatedEvent.ID)
		{
			return ID == ZoneThawedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		The.Player?.RegisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		The.Player?.RegisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		The.Player?.RegisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckReveal();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			The.Player?.UnregisterPartEvent(this, "EnteredCell");
			CheckReveal();
		}
		return base.FireEvent(E);
	}

	public void CheckReveal()
	{
		if (The.Player == null)
		{
			return;
		}
		Zone currentZone = ParentObject.GetCurrentZone();
		if (!The.Player.InZone(currentZone))
		{
			return;
		}
		string text = "Visit " + VillageName;
		string state = "Visited_" + VillageName;
		if (The.Game.HasQuest(text))
		{
			The.Game.CompleteQuest(text);
			if (The.Game.GetIntGameState(state) != 1)
			{
				if (!IsVillageZero)
				{
					Achievement.VILLAGES_100.Progress.Increment();
				}
				The.Game.SetIntGameState(state, 1);
				if (The.Player != null && !IsVillageZero)
				{
					JournalAPI.AddAccomplishment("You visited the village of " + VillageName + ".", HistoricStringExpander.ExpandString("In the month of " + Calendar.GetMonth() + " of " + Calendar.GetYear() + " AR, =name= founded the village of " + VillageName + " to <spice.history.gospels.HumblePractice.LateSultanate.!random>."), "Acting against the prohibition on the practice of <spice.elements." + The.Player.GetMythicDomain() + ".practices.!random>, =name= led an army to the gates of " + VillageName + ". =name= <spice.commonPhrases.liberated.!random> its citizens, and in " + The.Player.GetPronounProvider().PossessiveAdjective + " honor they <spice.history.gospels.Celebration.LateSultanate.!random>.", null, "general", MuralCategory.BecomesLoved, MuralWeight.Medium, null, -1L);
				}
			}
		}
		else if (The.Game.GetIntGameState(state) != 1)
		{
			if (!IsVillageZero)
			{
				The.Player?.AwardXP(currentZone.NewTier * 250, -1, 0, int.MaxValue, null, null, null, null, currentZone.ZoneID);
				Achievement.VILLAGES_100.Progress.Increment();
			}
			The.Game.SetIntGameState(state, 1);
			if (The.Player != null && !IsVillageZero)
			{
				JournalAPI.AddAccomplishment("You visited the village of " + VillageName + ".", "In the month of " + Calendar.GetMonth() + " of " + Calendar.GetYear() + " AR, =name= founded the village of " + VillageName + " to <spice.history.gospels.HumblePractice.LateSultanate.!random>.", "Acting against the prohibition on the practice of <spice.elements." + The.Player.GetMythicDomain() + ".practices.!random>, =name= led an army to the gates of " + VillageName + ". =name= <spice.commonPhrases.liberated.!random> its citizens, and in " + The.Player.GetPronounProvider().PossessiveAdjective + " honor they <spice.history.gospels.Celebration.LateSultanate.!random>.", null, "general", MuralCategory.BecomesLoved, MuralWeight.Medium, null, -1L);
			}
		}
		if (!The.Game.HasIntGameState(RevealKey))
		{
			The.Game.SetIntGameState(RevealKey, 1);
			if (RevealLocation != null && The.Game.ZoneManager.GetZone("JoppaWorld").GetCell(RevealLocation.x, RevealLocation.y).FireEvent("VillageReveal") && RevealString != null && !IsVillageZero && The.Player != null)
			{
				Popup.Show(RevealString, null, "sfx_newLocation_discovered");
			}
			JournalMapNote mapNote = JournalAPI.GetMapNote(RevealSecret);
			if (mapNote != null && !mapNote.Revealed)
			{
				JournalAPI.RevealMapNote(mapNote);
			}
		}
		ParentObject.CurrentZone?.GetWorldTerrainObject().SetStringProperty("MinimapColor", "g");
		The.Player?.UnregisterPartEvent(this, "EnteredCell");
		ParentObject.Obliterate();
	}
}
