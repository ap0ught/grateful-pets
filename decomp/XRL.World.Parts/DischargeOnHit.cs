using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: damage and voltage are increased by the standard
/// power load bonus, i.e. 2 for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class DischargeOnHit : IActivePart
{
	public string Voltage = "2d4";

	public string DamageRange = "1d4";

	public DischargeOnHit()
	{
		WorksOnSelf = true;
		NameForStatus = "DischargeGenerator";
	}

	public DischargeOnHit(string Voltage, string DamageRange)
		: this()
	{
		this.Voltage = Voltage;
		this.DamageRange = DamageRange;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool SameAs(IPart p)
	{
		DischargeOnHit dischargeOnHit = p as DischargeOnHit;
		if (dischargeOnHit.Voltage != Voltage)
		{
			return false;
		}
		if (dischargeOnHit.DamageRange != DamageRange)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AdjustWeaponScore");
		Registrar.Register("WeaponHit");
		Registrar.Register("ProjectileHit");
		Registrar.Register("WeaponThrowHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "ProjectileHit" || E.ID == "WeaponThrowHit")
		{
			int num = MyPowerLoadLevel();
			int? powerLoadLevel = num;
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
				int num2 = MyPowerLoadBonus(num);
				int voltage = Voltage.RollCached() + num2;
				string text = DamageRange;
				if (num2 != 0)
				{
					text = DieRoll.AdjustResult(text, num2);
				}
				if (E.ID == "WeaponHit")
				{
					GameObject target = gameObjectParameter2;
					gameObjectParameter.Discharge(null, voltage, 0, text, null, gameObjectParameter, ParentObject, target);
				}
				else
				{
					GameObject target = gameObjectParameter2;
					gameObjectParameter2.Discharge(null, voltage, 0, text, null, gameObjectParameter, ParentObject, target);
				}
			}
		}
		else if (E.ID == "AdjustWeaponScore" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int intParameter = E.GetIntParameter("Score");
			int num3 = MyPowerLoadBonus();
			int num4 = Math.Max(Voltage.RollMinCached() / 2 + Voltage.RollMaxCached() / 4 + num3 + (DamageRange.RollMinCached() + DamageRange.RollMaxCached() / 2 + num3), 1);
			E.SetParameter("Score", intParameter + num4);
		}
		return base.FireEvent(E);
	}
}
