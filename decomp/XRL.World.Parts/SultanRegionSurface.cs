using System;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SultanRegionSurface : IPart
{
	public string RegionName;

	public string RevealString;

	public string RevealSecret;

	public string RevealKey;

	public Vector2i RevealLocation;

	public int region = -1;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (The.Game.HasQuest("Visit " + RegionName))
			{
				The.Game.CompleteQuest("Visit " + RegionName);
				if (The.Game.GetIntGameState("Visited_" + RegionName) != 1)
				{
					JournalAPI.AddAccomplishment("You visited the historic site of " + RegionName + ".", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered " + region + ", once thought lost to the sands of time.", "Acting against the enfranchisement of " + Factions.GetMostHatedFormattedName() + ", =name= led an army to the historic site of " + RegionName + ". =name= <spice.commonPhrases.liberated.!random> its citizens, and in " + The.Player.GetPronounProvider().PossessiveAdjective + " honor they <spice.history.gospels.Celebration.LateSultanate.!random>.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Medium, null, -1L);
				}
				The.Game.SetIntGameState("Visited_" + RegionName, 1);
			}
			else if (The.Game.GetIntGameState("Visited_" + RegionName) != 1)
			{
				int newTier = ParentObject.CurrentZone.NewTier;
				The.Player?.AwardXP(250 * newTier, -1, 0, int.MaxValue, null, null, null, null, ParentObject.GetCurrentZone()?.ZoneID);
				The.Game.SetIntGameState("Visited_" + RegionName, 1);
				JournalAPI.AddAccomplishment("You visited the historic site of " + RegionName + ".", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered " + region + ", once thought lost to the sands of time.", "Acting against the enfranchisement of " + Factions.GetMostHatedFormattedName() + ", =name= led an army to the historic site of " + RegionName + ". =name= <spice.commonPhrases.liberated.!random> its citizens, and in " + The.Player.GetPronounProvider().PossessiveAdjective + " honor they <spice.history.gospels.Celebration.LateSultanate.!random>.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Medium, null, -1L);
			}
			if (The.Game.HasIntGameState(RevealKey))
			{
				return true;
			}
			The.Game.SetIntGameState(RevealKey, 1);
			if (RevealLocation != null && The.ZoneManager.GetZone("JoppaWorld").GetCell(RevealLocation.x, RevealLocation.y).FireEvent("SultanReveal") && RevealString != null)
			{
				Popup.Show(RevealString);
			}
			JournalMapNote mapNote = JournalAPI.GetMapNote(RevealSecret);
			if (mapNote != null && !mapNote.Revealed)
			{
				JournalAPI.RevealMapNote(mapNote);
			}
			ParentObject.CurrentZone?.GetWorldTerrainObject().SetStringProperty("MinimapColor", "g");
		}
		return base.FireEvent(E);
	}
}
