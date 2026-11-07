using System;

namespace XRL.World.Parts;

[Serializable]
public class ModPiping : IModification
{
	public string Liquid = "water";

	public ModPiping()
	{
	}

	public ModPiping(int Tier)
		: base(Tier)
	{
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModPiping).Liquid != Liquid)
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		HydraulicPowerTransmission part = Object.GetPart<HydraulicPowerTransmission>();
		if (part == null)
		{
			part = new HydraulicPowerTransmission();
			if (Tier > 1)
			{
				part.ChargeRate = 1500 + Tier * 500;
			}
			Object.AddPart(part);
		}
		else if (part.ChargeRate < 1500 + Tier * 500)
		{
			part.ChargeRate = 1500 + Tier * 500;
		}
		LiquidVolume liquidVolume = Object.LiquidVolume;
		if (liquidVolume == null)
		{
			liquidVolume = new LiquidVolume(Liquid, 8, 8);
			Object.AddPart(liquidVolume);
		}
		else if (liquidVolume.Volume < liquidVolume.MaxVolume)
		{
			liquidVolume.Volume = liquidVolume.MaxVolume;
		}
		liquidVolume.Sealed = true;
		if (Object.GetIntProperty("ThermalInsulation") < 900)
		{
			Object.SetIntProperty("ThermalInsulation", 900);
		}
		if (Object.Physics != null && Object.Physics.FlameTemperature < 1200)
		{
			Object.Physics.FlameTemperature = 1200;
		}
		IncreaseDifficulty(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GenericRatingEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GenericRatingEvent E)
	{
		if (E.Type == "WallDigNavigationWeight" && E.Object == ParentObject)
		{
			E.Rating += 20;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddWithClause("piping");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Piping: Has hydraulic pipes installed.";
	}
}
