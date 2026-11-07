namespace XRL.World.ZoneBuilders;

public class JungleRuins : ZoneBuilderSandbox
{
	public int RuinLevel = 100;

	public int Chance = 100;

	public string ZonesWide = "1d3";

	public string ZonesHigh = "1d2";

	public bool BuildZone(Zone Z)
	{
		if (Chance.in100())
		{
			Ruins ruins = new Ruins();
			ruins.RuinLevel = RuinLevel;
			ruins.ZonesWide = ZonesWide;
			ruins.ZonesHigh = ZonesHigh;
			ruins.BuildZone(Z);
			ZoneTemplateManager.Templates["SecretRuins"].Execute(Z);
			ZoneTemplateManager.Templates["SurfaceRuinsTechInfrastructure"].Execute(Z);
		}
		return true;
	}
}
