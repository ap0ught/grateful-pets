using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class BeyLahTerrain : IPart
{
	public string secretId = "$beylah";

	public bool revealed;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Object.SetIntProperty("ForceMutableSave", 1);
		Registrar.Register("BeyLahReveal");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeyLahReveal")
		{
			ParentObject.Render.Tile = "Terrain/sw_joppa.bmp";
			ParentObject.Render.ColorString = "&W";
			ParentObject.Render.DisplayName = "Bey Lah";
			ParentObject.Render.DetailColor = "r";
			ParentObject.Render.RenderString = "#";
			ParentObject.HasProperName = true;
			ParentObject.GetPart<Description>().Short = "At the center of a particularly thick copse, the vegetation clears. Flower-bedecked huts huddle in the clearing within, surrounded by phalanxes of tidy watervine rows and carefully-tended lah.";
			ParentObject.GetPart<TerrainTravel>()?.ClearEncounters();
			ParentObject.SetStringProperty("OverlayColor", "&W");
			if (secretId != null)
			{
				JournalMapNote mapNote = JournalAPI.GetMapNote(secretId);
				if (mapNote != null && !mapNote.Revealed)
				{
					mapNote.Reveal();
				}
			}
		}
		return base.FireEvent(E);
	}
}
