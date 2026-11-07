using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ModPhaseConjugate : IModification
{
	public ModPhaseConjugate()
	{
	}

	public ModPhaseConjugate(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasTagOrProperty("Grenade"))
		{
			return false;
		}
		if (Object.HasPart<PhaseGrenade>())
		{
			return false;
		}
		if (Object.HasPart<ModPhaseHarmonic>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexity(1, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeDetonateEvent>.ID && ID != PooledEvent<GenericQueryEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDetonateEvent E)
	{
		if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Phased effect = ParentObject.GetEffect<Phased>();
			if (effect == null)
			{
				ParentObject.ForceApplyEffect(new Phased(1));
			}
			else
			{
				ParentObject.RemoveEffect(effect);
				if (ParentObject.GetIntProperty("ProjectilePhaseAdded") > 0)
				{
					ParentObject.ModIntProperty("ProjectilePhaseAdded", -1, RemoveIfZero: true);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericQueryEvent E)
	{
		if (E.Query == "PhaseHarmonicEligible")
		{
			E.Result = false;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{K|phase-conjugate}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(GetPhysicalDescription());
		E.Postfix.AppendRules(GetDescription(Tier), GetEventSensitiveAddStatusSummary(E));
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public string GetPhysicalDescription()
	{
		return "";
	}

	public static string GetDescription(int Tier)
	{
		return "Phase-conjugate: This explosive shifts phase immediately before detonating.";
	}
}
