using System;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class RuinHydraulics : IPart
{
	public string DamageChance = "0-3x10";

	public int MissingChance = 30;

	public string MissingProducers = "1d2";

	public string MissingConsumers = "1d6";

	public string Liquid;

	public string LiquidTable = "HydraulicFluid";

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
			Hydraulics hydraulics = new Hydraulics();
			hydraulics.DamageChance = DamageChance;
			hydraulics.Liquid = Liquid;
			hydraulics.LiquidTable = LiquidTable;
			if (MissingChance.in100())
			{
				hydraulics.MissingProducers = MissingProducers;
				hydraulics.MissingConsumers = MissingConsumers;
			}
			hydraulics.PreferWalls = PreferWalls;
			hydraulics.AvoidWalls = AvoidWalls;
			hydraulics.BuildZone(currentZone);
		}
		ParentObject.Obliterate();
		return base.HandleEvent(E);
	}
}
