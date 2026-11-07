namespace XRL.World.ZoneBuilders;

public class Ruins : ZoneBuilderSandbox
{
	public int RuinLevel = 100;

	public string ZonesWide = "1d3";

	public string ZonesHigh = "1d2";

	public bool BuildZone(Zone Z)
	{
		ZoneManager zoneManager = The.ZoneManager;
		BuildingZoneTemplate buildingZoneTemplate = zoneManager.GetZoneColumnProperty(Z.ZoneID, "Builder.Ruins.BuildingZoneTemplate") as BuildingZoneTemplate;
		if (buildingZoneTemplate == null)
		{
			buildingZoneTemplate = new BuildingZoneTemplate();
			buildingZoneTemplate.New(Z.Width, Z.Height, ZonesWide.RollCached(), ZonesHigh.RollCached());
			zoneManager.SetZoneColumnProperty(Z.ZoneID, "Builder.Ruins.BuildingZoneTemplate", buildingZoneTemplate);
			string text = "";
			string populationName = ((!string.Equals(Z.GetTerrainObject()?.Blueprint, "TerrainRuins") && !string.Equals(Z.GetTerrainObject()?.Blueprint, "TerrainBaroqueRuins")) ? "DefaultRuinsSemantics" : "WorldRuinsSemantics");
			foreach (PopulationResult item in PopulationManager.Generate(populationName))
			{
				if (text != "")
				{
					text += ",";
				}
				text += item.Blueprint;
			}
			if (text == "")
			{
				text = "*Default";
			}
			zoneManager.SetZoneColumnProperty(Z.ZoneID, "Builder.Ruins.SemanticTags", text);
		}
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				Cell cell = Z.GetCell(j, i);
				BuildingTemplateTile buildingTemplateTile = buildingZoneTemplate.Template.Map[j, i];
				if (buildingTemplateTile == BuildingTemplateTile.Inside)
				{
					cell.AddSemanticTag("Inside");
					cell.AddSemanticTag("Room");
					cell.AddSemanticTag("Perimeter");
				}
				if (buildingTemplateTile == BuildingTemplateTile.Wall)
				{
					cell.AddSemanticTag("Wall");
					cell.AddSemanticTag("Inner");
					cell.AddSemanticTag("Perimeter");
				}
				if (buildingTemplateTile == BuildingTemplateTile.Door)
				{
					cell.AddSemanticTag("Door");
					cell.AddSemanticTag("Perimeter");
				}
				if (buildingTemplateTile == BuildingTemplateTile.OutsideWall)
				{
					cell.AddSemanticTag("Wall");
					cell.AddSemanticTag("Outer");
				}
				if (buildingTemplateTile == BuildingTemplateTile.StairsUp)
				{
					cell.AddSemanticTag("Connection");
					cell.AddSemanticTag("Stairs");
					cell.AddSemanticTag("Up");
				}
				if (buildingTemplateTile == BuildingTemplateTile.StairsDown)
				{
					cell.AddSemanticTag("Connection");
					cell.AddSemanticTag("Stairs");
					cell.AddSemanticTag("Up");
				}
				if (buildingTemplateTile == BuildingTemplateTile.Outside)
				{
					cell.AddSemanticTag("Outside");
				}
				if (buildingTemplateTile == BuildingTemplateTile.Void)
				{
					cell.AddSemanticTag("Inside");
					cell.AddSemanticTag("Isolated");
				}
			}
		}
		Z.DebugDrawSemantics();
		string[] array = zoneManager.GetZoneColumnProperty(Z.ZoneID, "Builder.Ruins.SemanticTags", "*Default").ToString().Split(',');
		foreach (string text2 in array)
		{
			string populationName2 = text2 + "Replace";
			if (PopulationManager.HasTable(populationName2))
			{
				string blueprint = PopulationManager.RollOneFrom(populationName2).Blueprint;
				if (!string.Equals(blueprint, "*None"))
				{
					if (string.Equals(blueprint, "Extra"))
					{
						Z.AddSemanticTag("Extra" + text2);
					}
					else
					{
						Z.AddSemanticTag(text2);
					}
				}
			}
			else
			{
				Z.AddSemanticTag(text2);
			}
		}
		Ruiner ruiner = new Ruiner();
		bool bUnderground = Z.GetZoneZ() > 10;
		buildingZoneTemplate.BuildZone(Z, bUnderground);
		if (RuinLevel > 0)
		{
			ruiner.RuinZone(Z, RuinLevel, bUnderground, 100000, 100000);
		}
		Z.RebuildReachableMap();
		return true;
	}
}
