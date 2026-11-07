namespace XRL.World.ZoneBuilders;

public class BasicLair : ZoneBuilderSandbox
{
	public string Table = "";

	public string Adjectives = "";

	public string Stairs = "";

	public bool BuildZone(Zone Z)
	{
		return new SultanDungeon().BuildRandomZoneWithArgs(Z, 0, bBuildSurface: true, Adjectives.Split(','), Stairs, Table, "Lairs_");
	}
}
