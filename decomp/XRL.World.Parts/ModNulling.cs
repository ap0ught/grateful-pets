using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, reality stabilization strength levels for
/// weapon applications are increased by ((power load - 100) / 30,
/// i.e. 10 for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class ModNulling : IModification
{
	public ModNulling()
	{
	}

	public ModNulling(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		IsPowerLoadSensitive = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<Armor>() && !Object.HasPart<Shield>() && !Object.HasPart<MeleeWeapon>() && !Object.HasPart<MissileWeapon>() && !Object.HasPart<ThrownWeapon>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
		RealityStabilization realityStabilization = Object.RequirePart<RealityStabilization>();
		if (Object.HasPart<Armor>() || Object.HasPart<Shield>())
		{
			if (realityStabilization.ChargeUse < 3)
			{
				realityStabilization.ChargeUse = 3;
			}
			if (realityStabilization.Visibility < 1)
			{
				realityStabilization.Visibility = 1;
			}
			if (realityStabilization.SelfVisibility < 1)
			{
				realityStabilization.SelfVisibility = 1;
			}
			if (realityStabilization.CellVisibilityOffset < 1)
			{
				realityStabilization.CellVisibilityOffset = 1;
			}
			realityStabilization.Projective = true;
			realityStabilization.WorksOn(AdjacentCellContents: false, Carrier: false, CellContents: true, Enclosed: false, Equipper: false, Holder: true, Implantee: false, Inventory: false, Self: true, Wearer: true);
			realityStabilization.HitpointsAffectPerformance = true;
			realityStabilization.GlimmerInterference = true;
			realityStabilization.IsPowerLoadSensitive = true;
			int num = Math.Max((Tier >= 5) ? (10 - Tier) : (Tier + 2), 2);
			BootSequence bootSequence = Object.GetPart<BootSequence>();
			if (bootSequence == null)
			{
				bootSequence = new BootSequence();
				bootSequence.BootTime = num;
				bootSequence.ChargeUse = 750;
				Object.AddPart(bootSequence);
			}
			else
			{
				if (bootSequence.BootTime < num)
				{
					bootSequence.BootTime = num;
				}
				if (bootSequence.ChargeUse < 750)
				{
					bootSequence.ChargeUse = 750;
				}
			}
			bootSequence.ReadoutInName = true;
			bootSequence.ReadoutInDescription = true;
			bootSequence.SyncWorksOn(realityStabilization);
			if (!Object.HasPart<PowerSwitch>())
			{
				PowerSwitch powerSwitch = new PowerSwitch();
				powerSwitch.EnergyCost = 100;
				powerSwitch.FlippableWithoutUnderstanding = true;
				powerSwitch.ActivateSuccessMessage = "";
				powerSwitch.ActivateFailureMessage = "";
				powerSwitch.DeactivateSuccessMessage = "";
				powerSwitch.DeactivateFailureMessage = "";
				Object.AddPart(powerSwitch);
			}
		}
		else
		{
			if (realityStabilization.ChargeUse < 150)
			{
				realityStabilization.ChargeUse = 150;
			}
			realityStabilization.ResetWorksOn();
			realityStabilization.IsPowerLoadSensitive = true;
			realityStabilization.HitpointsAffectPerformance = true;
			realityStabilization.GlimmerInterference = true;
		}
		IncreaseDifficultyAndComplexity(1, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CheckUsesChargeWhileEquippedEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckUsesChargeWhileEquippedEvent E)
	{
		if (!ParentObject.HasPart<Armor>() && !ParentObject.HasPart<Shield>())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{K|nulling}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(GetPhysicalDescription());
		E.Postfix.AppendRules(GetInstanceDescription(), GetEventSensitiveAddStatusSummary(E));
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("LauncherProjectileHit");
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponPseudoThrowHit");
		Registrar.Register("WeaponThrowHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "LauncherProjectileHit" || E.ID == "WeaponThrowHit" || E.ID == "WeaponPseudoThrowHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject Object = E.GetGameObjectParameter("Defender");
			RealityStabilization part = ParentObject.GetPart<RealityStabilization>();
			if (part != null && GameObject.Validate(ref Object))
			{
				int num = MyPowerLoadLevel();
				int? powerLoadLevel = num;
				if (part.IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
				{
					RealityStabilizeTransient(gameObjectParameter, Object, part, num);
				}
			}
		}
		return base.FireEvent(E);
	}

	public string GetPhysicalDescription()
	{
		return "";
	}

	public static string GetDescription(int Tier)
	{
		return "Nulling: When powered on a weapon, this item astrally burdens its target on hit. When powered on a piece of armor or shield, this item astrally burdens its wearer. Compute power on the local lattice increases the effectiveness of this effect.";
	}

	public string GetInstanceDescription()
	{
		if (ParentObject.HasPart<Armor>())
		{
			return "Nulling: When powered, this armor astrally burdens its wearer. Compute power on the local lattice increases the effectiveness of this effect.";
		}
		if (ParentObject.HasPart<Shield>())
		{
			return "Nulling: When powered, this shield astrally burdens its wielder. Compute power on the local lattice increases the effectiveness of this effect.";
		}
		return "Nulling: When powered, this weapon astrally burdens its target on hit. Compute power on the local lattice increases the effectiveness of this effect.";
	}

	public void RealityStabilizeTransient(GameObject who, GameObject obj, RealityStabilization rs = null, int PowerLoad = 100)
	{
		if (rs == null)
		{
			rs = ParentObject.GetPart<RealityStabilization>();
			if (rs == null)
			{
				return;
			}
		}
		rs.FlushEffectiveStrengthCache();
		rs.LastStatus = ActivePartStatus.Operational;
		rs.Strength = Stat.Random(1, 100) + Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad, 100, 30) + GetAvailableComputePowerEvent.GetFor(who);
		int effectiveStrength = rs.EffectiveStrength;
		if (effectiveStrength <= 0)
		{
			return;
		}
		int num = Stat.Random(2, 3);
		RealityStabilized realityStabilized = obj.GetEffect<RealityStabilized>();
		if (realityStabilized == null)
		{
			realityStabilized = new RealityStabilized();
			if (!obj.ForceApplyEffect(realityStabilized))
			{
				return;
			}
		}
		if (realityStabilized.IndependentStrength < effectiveStrength)
		{
			realityStabilized.IndependentStrength = effectiveStrength;
		}
		if (realityStabilized.Duration < num)
		{
			realityStabilized.Duration = num;
		}
		if (!GameObject.Validate(ref realityStabilized.Owner))
		{
			realityStabilized.Owner = who;
		}
		realityStabilized.Stabilize();
	}
}
