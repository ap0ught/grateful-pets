using System;
using HistoryKit;
using Qud.API;
using XRL.Annals;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SultanRegion : IPart
{
	public string RegionEntityID;

	public string secretId;

	public bool revealed;

	[NonSerialized]
	private HistoricEntity _Region;

	public HistoricEntity Region
	{
		get
		{
			if (_Region == null && !RegionEntityID.IsNullOrEmpty())
			{
				_Region = The.Game.sultanHistory.GetEntity(RegionEntityID);
			}
			return _Region;
		}
		set
		{
			_Region = value;
			RegionEntityID = value?.id;
		}
	}

	public SultanRegion()
	{
	}

	public SultanRegion(HistoricEntity region)
	{
		Region = region;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("SultanReveal");
		Object.SetIntProperty("ForceMutableSave", 1);
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SultanReveal")
		{
			if (revealed)
			{
				return false;
			}
			revealed = true;
			History sultanHistory = The.Game.sultanHistory;
			HistoricEntitySnapshot currentSnapshot = Region.GetCurrentSnapshot();
			string property = QudHistoryHelpers.GetRegionalizationParametersSnapshot(sultanHistory).GetProperty("organizingPrinciple");
			string tag = ParentObject.GetTag("AlternateTerrainName", ParentObject.GetTag("Terrain"));
			if (50.in100())
			{
				string text = HistoricStringExpander.ExpandString("<spice.history.regions.terrain." + tag + ".over.!random>", currentSnapshot, sultanHistory);
				string text2;
				do
				{
					text2 = HistoricStringExpander.ExpandString("<spice.history.regions.terrain." + tag + ".over.!random>", currentSnapshot, sultanHistory);
				}
				while (text.Equals(text2) && !string.IsNullOrEmpty(text));
				string text3 = HistoricStringExpander.ExpandString("<spice.history.regions.organizingPrinciple." + property + "." + currentSnapshot.GetProperty("organizingPrincipleType") + ".thingsTheyDid.!random>", currentSnapshot, sultanHistory);
				ParentObject.GetPart<Description>().Short = Grammar.InitCap(HistoricStringExpander.ExpandString(text + " and " + text2 + ", here <spice.commonPhrases.stretches.!random> the ancient " + currentSnapshot.GetProperty("governmentType") + " where " + text3 + ".", currentSnapshot, sultanHistory));
			}
			else
			{
				string text4 = HistoricStringExpander.ExpandString("<spice.history.regions.terrain." + tag + ".over.!random>", currentSnapshot, sultanHistory);
				string text5 = HistoricStringExpander.ExpandString("<spice.history.regions.organizingPrinciple." + property + "." + currentSnapshot.GetProperty("organizingPrincipleType") + ".thingsTheyDid.!random>", currentSnapshot, sultanHistory);
				ParentObject.GetPart<Description>().Short = Grammar.InitCap(HistoricStringExpander.ExpandString(text5 + " in the <spice.commonPhrases.lost.!random> " + currentSnapshot.GetProperty("governmentType") + " whose ruins lie " + text4 + ".", currentSnapshot, sultanHistory));
			}
			string text6 = null;
			string property2 = currentSnapshot.GetProperty("organizingPrincipleType");
			switch (property2)
			{
			case "poor":
				switch (Stat.Random(0, 1))
				{
				case 0:
					text6 = "HistoricHuts";
					break;
				case 1:
					text6 = "HistoricHills";
					break;
				}
				break;
			case "middle-class":
				switch (Stat.Random(0, 2))
				{
				case 0:
					text6 = "HistoricHouse";
					break;
				case 1:
					text6 = "HistoricHouses";
					break;
				case 2:
					text6 = "HistoricGrid";
					break;
				}
				break;
			case "rich":
				switch (Stat.Random(0, 2))
				{
				case 0:
					text6 = "HistoricPantheon";
					break;
				case 1:
					text6 = "HistoricBuilding";
					break;
				case 2:
					text6 = "HistoricCastle";
					break;
				}
				break;
			default:
				if (property == "religion" || property == "profession")
				{
					switch (Stat.Random(0, 15))
					{
					case 0:
						text6 = "HistoricPantheon";
						break;
					case 1:
						text6 = "HistoricBuilding";
						break;
					case 2:
						text6 = "HistoricCastle";
						break;
					case 3:
						text6 = "HistoricHuts";
						break;
					case 4:
						text6 = "HistoricHills";
						break;
					case 5:
						text6 = "HistoricHouse";
						break;
					case 6:
						text6 = "HistoricHouses";
						break;
					case 7:
						text6 = "HistoricGrid";
						break;
					case 8:
						text6 = "HistoricBuilding2";
						break;
					case 9:
						text6 = "HistoricBuilding3";
						break;
					case 10:
						text6 = "HistoricArch";
						break;
					case 11:
						text6 = "HistoricPond";
						break;
					case 12:
						text6 = "HistoricPools";
						break;
					case 13:
						text6 = "HistoricTower";
						break;
					case 14:
						text6 = "HistoricArms";
						break;
					case 15:
						ParentObject.Render.Tile = "HistoricRing";
						break;
					}
				}
				else
				{
					switch (property2)
					{
					case "gardens":
						text6 = "HistoricGardens";
						break;
					case "pools":
						text6 = "HistoricPools";
						break;
					case "museums":
						text6 = "HistoricPond";
						break;
					case "college":
						text6 = "HistoricPantheon";
						break;
					case "waste":
						text6 = "HistoricPools";
						break;
					case "consulate":
						text6 = "HistoricBuilding";
						break;
					case "market":
						text6 = "HistoricRing";
						break;
					case "forums":
						text6 = "HistoricArch";
						break;
					case "residential":
						text6 = "HistoricHouses";
						break;
					case "theaters":
						text6 = "HistoricRing";
						break;
					case "food storage":
						text6 = "HistoricBuilding2";
						break;
					case "pipe hub":
						text6 = "HistoricArms";
						break;
					case "prison":
						text6 = "HistoricTower";
						break;
					case "barracks":
						text6 = "HistoricGrid";
						break;
					case "temple":
						text6 = "HistoricPantheon";
						break;
					case "tavern":
						text6 = "HistoricBuilding3";
						break;
					}
				}
				break;
			}
			GameObjectBlueprint gameObjectBlueprint = ((text6 != null) ? GameObjectFactory.Factory.Blueprints[text6] : null);
			if (gameObjectBlueprint != null)
			{
				ParentObject.Render.Tile = gameObjectBlueprint.GetPartParameter<string>("Render", "Tile");
				ParentObject.Render.ColorString = "&" + Crayons.GetRandomColorAll();
			}
			else
			{
				ParentObject.Render.Tile = "Terrain/sw_historic_pantheon.bmp";
				ParentObject.Render.ColorString = "&" + Crayons.GetRandomColorAll();
			}
			do
			{
				ParentObject.Render.DetailColor = Crayons.GetRandomColorAll();
			}
			while (ParentObject.Render.DetailColor == ParentObject.Render.ColorString.Substring(1));
			ParentObject.Render.RenderString = "#";
			ParentObject.SetStringProperty("OverlayColor", "&W");
			_ = "JoppaWorld." + ParentObject.CurrentCell.X + "." + ParentObject.CurrentCell.Y + ".1.1.10";
			string text7 = Grammar.MakeTitleCase(currentSnapshot.GetProperty("newName"));
			string value = "";
			if (text7.StartsWith("the ") || text7.StartsWith("The "))
			{
				text7 = text7.Substring(4);
				value = "the";
			}
			ParentObject.GiveProperName(text7, Force: true);
			ParentObject.SetStringProperty("IndefiniteArticle", value);
			ParentObject.SetStringProperty("DefiniteArticle", value);
			ParentObject.SetStringProperty("OverrideIArticle", value);
			ParentObject.SetStringProperty("Gender", "nonspecific");
			ParentObject.SetGender("nonspecific");
			ParentObject.GetPart<TerrainTravel>()?.ClearEncounters();
			if (secretId != null)
			{
				JournalMapNote mapNote = JournalAPI.GetMapNote(secretId);
				if (!mapNote.Revealed)
				{
					mapNote.Reveal();
				}
			}
		}
		return base.FireEvent(E);
	}
}
