using System;
using System.Text;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, the percentage of extra damage done to
/// diggable targets (base value of which is 100 / HitsRequired, so
/// 25% by default), will increased by ((power load - 100) / 30), i.e.
/// 10% for the standard overload power load of 400, and the effective
/// PenetrationBonus will be increased by (power load - 100)%, i.e.
/// 30% for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class Drill : IPoweredPart
{
	public int PenetrationBonus = 24;

	public int HitsRequired = 4;

	public string Sound;

	public Drill()
	{
		WorksOnEquipper = true;
		IsPowerLoadSensitive = true;
	}

	public override bool SameAs(IPart p)
	{
		Drill drill = p as Drill;
		if (drill.PenetrationBonus != PenetrationBonus)
		{
			return false;
		}
		if (drill.HitsRequired != HitsRequired)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == PooledEvent<PathAsBurrowerEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(PathAsBurrowerEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int effectivePenetrationBonus = GetEffectivePenetrationBonus();
		if (effectivePenetrationBonus != 0)
		{
			E.Postfix.AppendRules(((ChargeUse > 0) ? "When powered, " : "") + effectivePenetrationBonus.Signed() + " penetration vs. walls.", (ChargeUse > 0) ? new Action<StringBuilder>(AddStatusSummary) : null);
		}
		int wallHitsRequired = GetWallHitsRequired();
		if (wallHitsRequired > 0)
		{
			E.Postfix.AppendRules(((ChargeUse > 0) ? "When powered, destroys " : "Destroys ") + " walls after " + wallHitsRequired.Things("penetrating hit") + ".", (ChargeUse > 0) ? new Action<StringBuilder>(AddStatusSummary) : null);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponDealDamage");
		Registrar.Register("GetWeaponHitDice");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetWeaponHitDice")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null && gameObjectParameter.IsDiggable() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				E.SetParameter("PenetrationBonus", E.GetIntParameter("PenetrationBonus") + GetEffectivePenetrationBonus());
			}
		}
		else if (E.ID == "WeaponDealDamage")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter2 != null && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (!Sound.IsNullOrEmpty())
				{
					gameObjectParameter2.PlayWorldSound(Sound);
				}
				if (gameObjectParameter2.IsDiggable())
				{
					double wallBonusPercentage = GetWallBonusPercentage();
					if (wallBonusPercentage > 0.0)
					{
						Damage damage = E.GetParameter("Damage") as Damage;
						int num = (int)Math.Floor((double)gameObjectParameter2.baseHitpoints * wallBonusPercentage / 100.0);
						if (num >= damage.Amount)
						{
							damage.Amount = num;
							CombatJuice.playPrefabAnimation(gameObjectParameter2, "Impacts/ImpactVFXDig");
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public double GetWallBonusPercentage()
	{
		double num = 100.0 / (double)HitsRequired + (double)MyPowerLoadBonus(int.MinValue, 100, 30);
		if (ParentObject.IsGiganticEquipment)
		{
			num *= 2.0;
		}
		return num;
	}

	public int GetWallHitsRequired()
	{
		return GetWallHitsRequired(GetWallBonusPercentage());
	}

	public static int GetWallHitsRequired(double Percentage)
	{
		if (Percentage <= 0.0)
		{
			return 0;
		}
		return Math.Max((int)Math.Ceiling(100.0 / Percentage), 1);
	}

	public static int GetWallHitsRequired(int Percentage)
	{
		return GetWallHitsRequired((float)Percentage);
	}

	public int GetEffectivePenetrationBonus()
	{
		int num = PenetrationBonus;
		int num2 = MyPowerLoadBonus(int.MinValue, 100, 10);
		if (num2 != 0)
		{
			num = (int)Math.Round((double)num * (100.0 + (double)num2) / 100.0);
		}
		return num;
	}
}
