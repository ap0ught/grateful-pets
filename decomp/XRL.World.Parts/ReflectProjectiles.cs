using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ReflectProjectiles : IPoweredPart
{
	public int Chance = 100;

	public string RetroVariance = "0";

	public string Verb = "reflect";

	public bool Deactivated;

	public ReflectProjectiles()
	{
		ChargeUse = 0;
		IsEMPSensitive = true;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return Deactivated;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "Deactivated";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("ApplyEMP");
		base.Register(Object, Registrar);
	}

	public bool Check()
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (!ParentObject.HasEffect<ProjectileReflectionShield>())
			{
				ParentObject.ApplyEffect(new ProjectileReflectionShield(Chance, RetroVariance, Verb));
				DidX("activate", ParentObject.its + " reflective shield", null, null, null, ParentObject);
			}
			return true;
		}
		if (ParentObject.HasEffect<ProjectileReflectionShield>())
		{
			ParentObject.RemoveEffect<ProjectileReflectionShield>();
			DidX("have", ParentObject.its + " reflective shield deactivated", null, null, null, null, ParentObject);
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyEMP" && IsEMPSensitive && ParentObject.HasEffect<ProjectileReflectionShield>())
		{
			ParentObject.RemoveEffect<ProjectileReflectionShield>();
			DidX("have", ParentObject.its + " reflective shield deactivated", null, null, null, null, ParentObject);
		}
		if (E.ID == "BeginTakeAction")
		{
			Check();
		}
		return base.FireEvent(E);
	}
}
