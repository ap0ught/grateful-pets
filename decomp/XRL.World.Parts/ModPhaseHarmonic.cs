using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ModPhaseHarmonic : IModification
{
	public ModPhaseHarmonic()
	{
	}

	public ModPhaseHarmonic(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		ChargeUse = 100;
		WorksOnSelf = true;
		IsTechScannable = true;
		NameForStatus = "PhaseRegulator";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.HasTag("NoModPhaseHarmonic"))
		{
			return true;
		}
		if (!GenericQueryEvent.Check(Object, "PhaseHarmonicEligible"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (Object.HasPart<AmmoGrenade>())
		{
			ChargeUse = 0;
		}
		else
		{
			Object.RequirePart<EnergyCellSocket>();
		}
		IncreaseDifficultyAndComplexity(1, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeDetonateEvent>.ID && ID != PooledEvent<GetActivationPhaseEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDetonateEvent E)
	{
		if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ParentObject.ForceApplyEffect(new Omniphase(1));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{phase-harmonic|phase-harmonic}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(GetPhysicalDescription());
		E.Postfix.AppendRules(GetDescription(Tier), GetEventSensitiveAddStatusSummary(E));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetActivationPhaseEvent E)
	{
		if ((E.Phase == 1 || E.Phase == 2) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Phase = 3;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileSetup");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileSetup")
		{
			GameObject Object = E.GetGameObjectParameter("Projectile");
			if (GameObject.Validate(ref Object) && !Object.HasEffect<Omniphase>() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				Object.ForceApplyEffect(new Omniphase("ModPhaseHarmonic"));
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
		return "Phase-Harmonic: This weapon can affect both in-phase and out-of-phase objects.";
	}

	public static bool IsProjectileCompatible(string BlueprintName)
	{
		if (!BlueprintName.IsNullOrEmpty())
		{
			GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(BlueprintName);
			if (blueprintIfExists != null && blueprintIfExists.HasPart("OmniphaseProjectile"))
			{
				return false;
			}
		}
		return true;
	}
}
