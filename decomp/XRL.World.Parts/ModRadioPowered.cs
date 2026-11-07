using System;

namespace XRL.World.Parts;

[Serializable]
public class ModRadioPowered : IModification
{
	public ModRadioPowered()
	{
	}

	public ModRadioPowered(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<EnergyCell>();
	}

	public override void ApplyModification(GameObject Object)
	{
		int num = Tier * 10;
		BroadcastPowerReceiver broadcastPowerReceiver = Object.RequirePart<BroadcastPowerReceiver>();
		if (broadcastPowerReceiver.ChargeRate < num)
		{
			broadcastPowerReceiver.ChargeRate = num;
		}
		int num2 = 11 + Tier;
		if (broadcastPowerReceiver.MaxSatellitePowerDepth < num2)
		{
			broadcastPowerReceiver.MaxSatellitePowerDepth = num2;
		}
		broadcastPowerReceiver.CanReceiveSatellitePower = true;
		broadcastPowerReceiver.Obvious = true;
		broadcastPowerReceiver.SatellitePowerOcclusionReadout = true;
		IntegralRecharger integralRecharger = Object.RequirePart<IntegralRecharger>();
		if (integralRecharger.ChargeRate != 0 && integralRecharger.ChargeRate < num)
		{
			integralRecharger.ChargeRate = num;
		}
		EnergyCell energyCell = Object.RequirePart<EnergyCell>();
		if (energyCell.ChargeRate < num)
		{
			energyCell.ChargeRate = num;
		}
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{C|radio-powered}}");
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
		return "Radio-powered: This item can be recharged via broadcast power.";
	}
}
