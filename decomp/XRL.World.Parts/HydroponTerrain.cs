using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class HydroponTerrain : IPart
{
	public string secretId = "$hydropon";

	public bool revealed;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Object.SetIntProperty("ForceMutableSave", 1);
		Registrar.Register("HydroponReveal");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "HydroponReveal")
		{
			ParentObject.Render.Tile = "Terrain/sw_joppa.bmp";
			ParentObject.Render.ColorString = "&B";
			ParentObject.Render.DisplayName = "Hydropon";
			ParentObject.Render.DetailColor = "r";
			ParentObject.Render.RenderString = "#";
			ParentObject.HasProperName = true;
			ParentObject.GetPart<Description>().Short = "It's the hydropon.";
			ParentObject.GetPart<TerrainTravel>()?.ClearEncounters();
			ParentObject.SetStringProperty("OverlayColor", "&B");
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
