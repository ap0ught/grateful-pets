using System;
using HistoryKit;
using Qud.API;
using XRL.Core;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class VillageTerrain : IPart
{
	public string VillageEntityID;

	public string secretId;

	public bool revealed;

	[NonSerialized]
	private HistoricEntity _Village;

	public HistoricEntity Village
	{
		get
		{
			if (_Village == null && !VillageEntityID.IsNullOrEmpty())
			{
				_Village = The.Game.sultanHistory.GetEntity(VillageEntityID);
			}
			return _Village;
		}
		set
		{
			_Village = value;
			VillageEntityID = value?.id;
		}
	}

	public VillageTerrain()
	{
	}

	public VillageTerrain(HistoricEntity village)
	{
		Village = village;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Object.SetIntProperty("ForceMutableSave", 1);
		Registrar.Register("VillageReveal");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageReveal")
		{
			if (revealed)
			{
				return false;
			}
			revealed = true;
			History sultanHistory = XRLCore.Core.Game.sultanHistory;
			HistoricEntitySnapshot currentSnapshot = Village.GetCurrentSnapshot();
			string tag = ParentObject.GetTag("AlternateTerrainName", ParentObject.GetTag("Terrain"));
			string text = HistoricStringExpander.ExpandString("<spice.history.regions.terrain." + tag + ".over.!random>", currentSnapshot, sultanHistory);
			string text2;
			do
			{
				text2 = HistoricStringExpander.ExpandString("<spice.history.regions.terrain." + tag + ".over.!random>", currentSnapshot, sultanHistory);
			}
			while (text.Equals(text2) && !string.IsNullOrEmpty(text));
			if (If.d100(30))
			{
				text = text + " and " + text2;
			}
			string newValue = ((currentSnapshot.GetList("sacredThings").Count > 0) ? currentSnapshot.GetList("sacredThings").GetRandomElement() : currentSnapshot.GetProperty("defaultSacredThing"));
			string newValue2 = ((currentSnapshot.GetList("profaneThings").Count > 0) ? currentSnapshot.GetList("profaneThings").GetRandomElement() : currentSnapshot.GetProperty("defaultProfaneThing"));
			ParentObject.GetPart<Description>().Short = Grammar.InitCap(HistoricStringExpander.ExpandString("<spice.villages.description.!random>.").Replace("*terrainFragment*", text).Replace("*sacredThing*", newValue)
				.Replace("*profaneThing*", newValue2)
				.Replace("*faction*", Faction.GetFormattedName(currentSnapshot.GetProperty("baseFaction"))));
			ParentObject.Render.Tile = "Terrain/sw_joppa.bmp";
			ParentObject.Render.ColorString = "&" + Crayons.GetRandomColorAll(new Random(currentSnapshot.GetProperty("name").GetHashCode()));
			ParentObject.GiveProperName(Grammar.MakeTitleCase(currentSnapshot.GetProperty("name")), Force: true);
			ParentObject.SetStringProperty("IndefiniteArticle", "");
			ParentObject.SetStringProperty("DefiniteArticle", "");
			ParentObject.SetStringProperty("OverrideIArticle", "");
			ParentObject.SetStringProperty("Gender", "nonspecific");
			ParentObject.SetGender("nonspecific");
			Random r = new Random(currentSnapshot.GetProperty("name").GetHashCode() + 1);
			do
			{
				ParentObject.Render.DetailColor = Crayons.GetRandomColorAll(r);
			}
			while (ParentObject.Render.DetailColor == ParentObject.Render.ColorString.Substring(1));
			ParentObject.Render.RenderString = "#";
			ParentObject.GetPart<TerrainTravel>()?.ClearEncounters();
			ParentObject.SetStringProperty("OverlayColor", "&W");
			if (secretId != null)
			{
				JournalMapNote mapNote = JournalAPI.GetMapNote(secretId);
				if (!mapNote.Revealed)
				{
					mapNote.Reveal();
				}
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
