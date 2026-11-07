using System;
using System.Text;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is not by default, penetration bonus is increased by half of
/// the standard power load bonus, i.e. 1 for the standard overload
/// power load of 400.
/// </remarks>
[Serializable]
public class AfferentWeapon : IActivePart
{
	public int PenetrationBonus;

	public bool WorksWhenThrown = true;

	public AfferentWeapon()
	{
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
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<IsAdaptivePenetrationActiveEvent>.ID && (ID != PooledEvent<GetThrownWeaponPerformanceEvent>.ID || !WorksWhenThrown) && ID != GetWeaponMeleePenetrationEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsAdaptivePenetrationActiveEvent E)
	{
		int num = MyPowerLoadLevel();
		int? powerLoadLevel = num;
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			E.Symbol = "à";
			E.Bonus += PenetrationBonus + IComponent<GameObject>.PowerLoadBonus(num, 100, 300);
			E.Active = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetWeaponMeleePenetrationEvent E)
	{
		E.StatBonus = 0;
		int num = MyPowerLoadLevel();
		int? powerLoadLevel = num;
		if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			MeleeWeapon part = ParentObject.GetPart<MeleeWeapon>();
			if (part != null && E.Defender != null)
			{
				E.StatBonus = E.Defender.StatMod(part.Stat) + IComponent<GameObject>.PowerLoadBonus(num, 100, 300);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetThrownWeaponPerformanceEvent E)
	{
		E.PenetrationModifier = 0;
		int num = MyPowerLoadLevel();
		bool useCharge = !E.Prospective;
		int? powerLoadLevel = num;
		if (IsReady(useCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			MeleeWeapon part = ParentObject.GetPart<MeleeWeapon>();
			if (part != null && E.Defender != null)
			{
				E.PenetrationModifier = E.Defender.StatMod(part.Stat);
			}
			E.PenetrationBonus += PenetrationBonus + IComponent<GameObject>.PowerLoadBonus(num, 100, 300);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(AppendEffect);
		return base.HandleEvent(E);
	}

	public void AppendEffect(StringBuilder SB)
	{
		MeleeWeapon part = ParentObject.GetPart<MeleeWeapon>();
		if (part != null && !part.Stat.IsNullOrEmpty())
		{
			SB.Append("This weapon uses its victim's ").Append(Statistic.GetStatDisplayName(part.Stat)).Append(" modifier for penetration bonus.");
		}
	}
}
