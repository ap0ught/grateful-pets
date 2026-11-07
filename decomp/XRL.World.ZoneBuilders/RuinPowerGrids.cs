namespace XRL.World.ZoneBuilders;

public class RuinPowerGrids : ZoneBuilderSandbox
{
	public string DamageChance = "0-3x10";

	public int MissingElectricalChance = 30;

	public string MissingElectricalProducers = "1d3";

	public string MissingElectricalConsumers = "1d6";

	public int MissingHydraulicsChance = 5;

	public string MissingHydraulicsProducers = "1d2";

	public string MissingHydraulicsConsumers = "1d6";

	public string HydraulicLiquid;

	public string HydraulicLiquidTable = "HydraulicFluid";

	public int MissingMechanicalChance = 5;

	public string MissingMechanicalProducers = "1d2";

	public string MissingMechanicalConsumers = "1d4";

	public bool PreferWalls = true;

	public bool AvoidWalls;

	public bool BuildZone(Zone Z)
	{
		string damageChance = DamageChance.RollCached().ToString();
		PowerGrid powerGrid = new PowerGrid();
		powerGrid.DamageChance = damageChance;
		if (MissingElectricalChance.in100())
		{
			powerGrid.MissingProducers = MissingElectricalProducers;
			powerGrid.MissingConsumers = MissingElectricalConsumers;
		}
		powerGrid.PreferWalls = PreferWalls;
		powerGrid.AvoidWalls = AvoidWalls;
		powerGrid.BuildZone(Z);
		Hydraulics hydraulics = new Hydraulics();
		hydraulics.DamageChance = damageChance;
		hydraulics.Liquid = HydraulicLiquid;
		hydraulics.LiquidTable = HydraulicLiquidTable;
		if (MissingHydraulicsChance.in100())
		{
			hydraulics.MissingProducers = MissingHydraulicsProducers;
			hydraulics.MissingConsumers = MissingHydraulicsConsumers;
		}
		hydraulics.PreferWalls = PreferWalls;
		hydraulics.AvoidWalls = AvoidWalls;
		hydraulics.BuildZone(Z);
		MechanicalPower mechanicalPower = new MechanicalPower();
		mechanicalPower.DamageChance = damageChance;
		if (MissingMechanicalChance.in100())
		{
			mechanicalPower.MissingProducers = MissingMechanicalProducers;
			mechanicalPower.MissingConsumers = MissingMechanicalConsumers;
		}
		mechanicalPower.PreferWalls = PreferWalls;
		mechanicalPower.AvoidWalls = AvoidWalls;
		mechanicalPower.BuildZone(Z);
		Z.RebuildReachableMap();
		return true;
	}
}
