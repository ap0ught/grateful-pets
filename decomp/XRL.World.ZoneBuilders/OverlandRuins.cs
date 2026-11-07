namespace XRL.World.ZoneBuilders;

public class OverlandRuins : ZoneBuilderSandbox
{
	public int RuinLevel = 100;

	public string ZonesWide = "1d3";

	public string ZonesHigh = "1d2";

	public bool BuildZone(Zone Z)
	{
		Coach.StartSection("Build OverlandRuins");
		if (70.in100())
		{
			new SultanDungeon().BuildRandomZone(Z, 5);
			The.ZoneManager.AddZonePostBuilderIfNotAlreadyPresent(Z.ZoneID, "ForceConnections");
			return true;
		}
		Ruins ruins = new Ruins();
		ruins.RuinLevel = RuinLevel;
		ruins.ZonesWide = ZonesWide;
		ruins.ZonesHigh = ZonesHigh;
		ruins.BuildZone(Z);
		ZoneTemplateManager.Templates["SecretRuins"].Execute(Z);
		ZoneTemplateManager.Templates["SurfaceRuinsTechInfrastructure"].Execute(Z);
		return true;
	}
}
