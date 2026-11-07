namespace XRL.World.ZoneBuilders;

public class SnapjawStockadeMaker
{
	public bool BuildZone(Zone Z)
	{
		return new StockadeMaker().BuildZone(Z, ClearCombatObjectsFirst: true, "BrinestalkStakes", "SnapjawFortGlobals", null);
	}
}
