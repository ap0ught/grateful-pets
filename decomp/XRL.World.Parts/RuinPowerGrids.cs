using System;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class RuinPowerGrids : IPart
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

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeZoneBuiltEvent E)
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone != null)
		{
			XRL.World.ZoneBuilders.RuinPowerGrids ruinPowerGrids = new XRL.World.ZoneBuilders.RuinPowerGrids();
			ruinPowerGrids.DamageChance = DamageChance;
			ruinPowerGrids.MissingElectricalChance = MissingElectricalChance;
			ruinPowerGrids.MissingElectricalProducers = MissingElectricalProducers;
			ruinPowerGrids.MissingElectricalConsumers = MissingElectricalConsumers;
			ruinPowerGrids.MissingHydraulicsChance = MissingHydraulicsChance;
			ruinPowerGrids.MissingHydraulicsProducers = MissingHydraulicsProducers;
			ruinPowerGrids.MissingHydraulicsConsumers = MissingHydraulicsConsumers;
			ruinPowerGrids.HydraulicLiquid = HydraulicLiquid;
			ruinPowerGrids.HydraulicLiquidTable = HydraulicLiquidTable;
			ruinPowerGrids.MissingMechanicalChance = MissingMechanicalChance;
			ruinPowerGrids.MissingMechanicalProducers = MissingMechanicalProducers;
			ruinPowerGrids.MissingMechanicalConsumers = MissingMechanicalConsumers;
			ruinPowerGrids.PreferWalls = PreferWalls;
			ruinPowerGrids.AvoidWalls = AvoidWalls;
			ruinPowerGrids.BuildZone(currentZone);
		}
		ParentObject.Obliterate();
		return base.HandleEvent(E);
	}
}
