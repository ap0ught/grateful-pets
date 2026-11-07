using System;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class RuinPowerGrid : IPart
{
	public string DamageChance = "0-3x10";

	public int MissingChance = 30;

	public string MissingProducers = "1d3";

	public string MissingConsumers = "1d6";

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
			PowerGrid powerGrid = new PowerGrid();
			powerGrid.DamageChance = DamageChance;
			if (MissingChance.in100())
			{
				powerGrid.MissingProducers = MissingProducers;
				powerGrid.MissingConsumers = MissingConsumers;
			}
			powerGrid.PreferWalls = PreferWalls;
			powerGrid.AvoidWalls = AvoidWalls;
			powerGrid.BuildZone(currentZone);
		}
		ParentObject.Obliterate();
		return base.HandleEvent(E);
	}
}
