using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class PsychicMeridian : IActivePart
{
	public string NosebleedDamage = "1d2+4";

	public int NosebleedSave = 30;

	public string Cooldown;

	public int CurrentCooldown;

	public bool TargetWearer;

	public bool Guarded = true;

	public PsychicMeridian()
	{
		IsRealityDistortionBased = true;
		WorksOnWearer = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantTurnTick()
	{
		return Cooldown != null;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (CurrentCooldown > 0)
		{
			CurrentCooldown = Math.Max(0, CurrentCooldown - Amount);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterMentalDefendEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "DefenderAfterAttack");
		E.Actor.RegisterPartEvent(this, "DefenderProjectileHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "DefenderAfterAttack");
		E.Actor.UnregisterPartEvent(this, "DefenderProjectileHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterMentalDefendEvent E)
	{
		if ((!Guarded || E.Penetrations <= 0) && CurrentCooldown <= 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			AfflictNosebleed(E.Attacker);
		}
		return base.HandleEvent(E);
	}

	public void AfflictNosebleed(GameObject Attacker)
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		GameObject gameObject = (TargetWearer ? activePartFirstSubject : Attacker);
		if (!TargetWearer && (activePartFirstSubject.IsPlayer() || gameObject.IsPlayer()))
		{
			IComponent<GameObject>.XDidY(gameObject, "impale", gameObject.itself + " on " + activePartFirstSubject.poss("psychic barbs"), null, null, null, null, gameObject);
		}
		gameObject.ApplyEffect(new Nosebleed(NosebleedDamage, NosebleedSave, activePartFirstSubject));
		if (!string.IsNullOrEmpty(Cooldown))
		{
			CurrentCooldown = Cooldown.RollCached();
		}
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("stars", 1);
			E.Add("time", 1);
			E.Add("scholarship", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "DefenderAfterAttack" || E.ID == "DefenderProjectileHit") && CurrentCooldown <= 0)
		{
			if (Guarded && E.GetIntParameter("Penetrations") > 0)
			{
				return true;
			}
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && E.GetParameter("Damage") is Damage damage && damage.HasAttribute("Mental"))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				if (gameObjectParameter == null)
				{
					return true;
				}
				AfflictNosebleed(gameObjectParameter);
			}
		}
		return base.FireEvent(E);
	}
}
