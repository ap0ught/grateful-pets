using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class BleedingOnHit : IActivePart
{
	public string Amount = "1d2";

	public int SaveTarget = 20;

	public string RequireDamageAttribute;

	public bool Holographic;

	public bool Stack;

	public BleedingOnHit()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		BleedingOnHit bleedingOnHit = p as BleedingOnHit;
		if (bleedingOnHit.Amount != Amount)
		{
			return false;
		}
		if (bleedingOnHit.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (bleedingOnHit.RequireDamageAttribute != RequireDamageAttribute)
		{
			return false;
		}
		if (bleedingOnHit.Holographic != Holographic)
		{
			return false;
		}
		if (bleedingOnHit.Stack != Stack)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != ImplantedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "WieldedWeaponHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "WieldedWeaponHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterPartEvent(this, "WieldedWeaponHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "WieldedWeaponHit");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public bool CheckApply(Event E)
	{
		if (!RequireDamageAttribute.IsNullOrEmpty() && (!(E.GetParameter("Damage") is Damage damage) || !damage.HasAttribute(RequireDamageAttribute)))
		{
			return false;
		}
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
		return E.GetGameObjectParameter("Defender").ApplyEffect(Holographic ? new HolographicBleeding(Amount, SaveTarget, gameObjectParameter, Stack) : new Bleeding(Amount, SaveTarget, gameObjectParameter, Stack));
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WieldedWeaponHit")
		{
			if (IsObjectActivePartSubject(E.GetGameObjectParameter("Attacker")))
			{
				CheckApply(E);
			}
		}
		else if (E.ID == "WeaponHit" && IsObjectActivePartSubject(E.GetGameObjectParameter("Weapon")))
		{
			CheckApply(E);
		}
		return base.FireEvent(E);
	}
}
