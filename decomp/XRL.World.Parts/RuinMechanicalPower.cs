using System;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class RuinMechanicalPower : IPart
{
	public string DamageChance = "0-3x10";

	public int MissingChance = 30;

	public string MissingProducers = "1d2";

	public string MissingConsumers = "1d4";

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
			MechanicalPower mechanicalPower = new MechanicalPower();
			mechanicalPower.DamageChance = DamageChance;
			if (MissingChance.in100())
			{
				mechanicalPower.MissingProducers = MissingProducers;
				mechanicalPower.MissingConsumers = MissingConsumers;
			}
			mechanicalPower.PreferWalls = PreferWalls;
			mechanicalPower.AvoidWalls = AvoidWalls;
			mechanicalPower.BuildZone(currentZone);
		}
		ParentObject.Obliterate();
		return base.HandleEvent(E);
	}
}
