namespace XRL.World.ZoneBuilders;

public class RegionPopulator : ZoneBuilderSandbox
{
	public int RegionSize = 100;

	public bool BuildZone(Zone Z)
	{
		ZoneBuilderSandbox.GenerateInfluenceMap(Z, null, InfluenceMapSeedStrategy.FurthestPoint, RegionSize, null, bDraw: true);
		return true;
	}
}
