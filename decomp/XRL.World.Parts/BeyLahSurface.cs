using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class BeyLahSurface : IPart
{
	public string RegionName = "Bey Lah";

	public string RevealString;

	public string RevealSecret = "$beylah";

	public string RevealKey = "BeyLah_LocationKnown";

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
		if (E.ID == "EnteredCell" && !The.Game.HasIntGameState(RevealKey))
		{
			The.Game.SetIntGameState(RevealKey, 1);
			ZoneManager.instance.GetZone("JoppaWorld").BroadcastEvent("BeyLahReveal");
			JournalMapNote mapNote = JournalAPI.GetMapNote(RevealSecret);
			if (mapNote != null && !mapNote.Revealed)
			{
				JournalAPI.AddAccomplishment("You discovered the hidden village of Bey Lah.", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered Bey Lah, once thought lost to the sands of time.", "<spice.commonPhrases.allThroughout.!random.capitalize> =year=, =name= <spice.commonPhrases.ravaged.!random> the flower fields and brought turmoil to the troubled village of Bey Lah. " + The.Player.GetPronounProvider().CapitalizedSubjective + " became known as the Hindren <spice.commonPhrases.scourge.!random.capitalize>.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Medium, null, -1L);
				JournalAPI.RevealMapNote(mapNote);
			}
		}
		return base.FireEvent(E);
	}
}
