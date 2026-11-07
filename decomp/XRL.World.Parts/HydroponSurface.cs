using System;
using Qud.API;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class HydroponSurface : IPart
{
	public string RegionName = "Hydropon";

	public string RevealString;

	public string RevealSecret = "$hydropon";

	public string RevealKey = "Hydropon_LocationKnown";

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
		if (E.ID == "EnteredCell" && !XRLCore.Core.Game.HasIntGameState(RevealKey))
		{
			XRLCore.Core.Game.SetIntGameState(RevealKey, 1);
			ZoneManager.instance.GetZone("JoppaWorld").BroadcastEvent("HydroponReveal");
			JournalMapNote mapNote = JournalAPI.GetMapNote(RevealSecret);
			if (mapNote != null && !mapNote.Revealed)
			{
				JournalAPI.AddAccomplishment("You discovered the Hydropon.", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered the Hydropon.", "Acting against labor laws restricting the rights of newly sentient plant-people, =name= led an army to the gates of the Hydropon. " + The.Player.GetPronounProvider().CapitalizedSubjective + " <spice.commonPhrases.liberated.!random> its citizens, and in " + The.Player.GetPronounProvider().PossessiveAdjective + " honor they <spice.history.gospels.Celebration.LateSultanate.!random>.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Medium, null, -1L);
				JournalAPI.RevealMapNote(mapNote);
			}
		}
		return base.FireEvent(E);
	}
}
