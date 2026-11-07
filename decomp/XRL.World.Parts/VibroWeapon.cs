using System;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, penetration bonus is increased by half of
/// the standard power load bonus, i.e. 1 for the standard overload
/// power load of 400.
/// </remarks>
[Serializable]
public class VibroWeapon : IPoweredPart
{
	public int PenetrationBonus;

	public bool WorksWhenThrown = true;

	public VibroWeapon()
	{
		ChargeUse = 0;
		IsPowerLoadSensitive = true;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		VibroWeapon vibroWeapon = p as VibroWeapon;
		if (vibroWeapon.PenetrationBonus != PenetrationBonus)
		{
			return false;
		}
		if (vibroWeapon.WorksWhenThrown != WorksWhenThrown)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<IsAdaptivePenetrationActiveEvent>.ID && (ID != PooledEvent<GetThrownWeaponPerformanceEvent>.ID || !WorksWhenThrown))
		{
			return ID == GetWeaponMeleePenetrationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsAdaptivePenetrationActiveEvent E)
	{
		int num = MyPowerLoadLevel();
		int? powerLoadLevel = num;
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			E.Bonus += PenetrationBonus + IComponent<GameObject>.PowerLoadBonus(num, 100, 300);
			E.Active = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetWeaponMeleePenetrationEvent E)
	{
		int num = MyPowerLoadLevel();
		int? powerLoadLevel = num;
		if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			E.MaxStatBonus = (E.StatBonus = E.AV + PenetrationBonus + IComponent<GameObject>.PowerLoadBonus(num, 100, 300));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetThrownWeaponPerformanceEvent E)
	{
		int num = MyPowerLoadLevel();
		bool useCharge = !E.Prospective;
		int? powerLoadLevel = num;
		if (IsReady(useCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			E.PenetrationBonus += PenetrationBonus + IComponent<GameObject>.PowerLoadBonus(num, 100, 300);
			E.Vorpal = true;
		}
		return base.HandleEvent(E);
	}
}
