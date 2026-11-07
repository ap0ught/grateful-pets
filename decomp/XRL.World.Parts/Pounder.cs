using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class Pounder : IActivePart
{
	public int TurnsPerBonus = 1;

	public int TurnsNextToTarget;

	public Pounder()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetToHitModifierEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckTargetAdjacency();
	}

	public override bool HandleEvent(GetToHitModifierEvent E)
	{
		if (E.Checking == "Actor" && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Modifier += GetPounderBonus();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int pounderBonus = GetPounderBonus();
		E.Postfix.AppendRules("Pounder: Receives +1 to its to-hit and penetration rolls for every " + TurnsPerBonus.Things("turn") + " " + (ParentObject.IsCreature ? ParentObject.itis : (ParentObject.its + " wielder is")) + " next to " + (ParentObject.IsCreature ? ParentObject.its : "their") + " target. (currently " + pounderBonus.Signed() + ")", GetEventSensitiveAddStatusSummary(E));
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerGetWeaponPenModifier");
		Registrar.Register("GetWeaponPenModifier");
		base.Register(Object, Registrar);
	}

	public int GetPounderBonus()
	{
		return TurnsNextToTarget / TurnsPerBonus;
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "GetWeaponPenModifier" || E.ID == "AttackerGetWeaponPenModifier") && WasReady())
		{
			E.SetParameter("Penetrations", E.GetIntParameter("Penetrations") + GetPounderBonus());
		}
		return base.FireEvent(E);
	}

	public void CheckTargetAdjacency()
	{
		GameObject gameObject = ParentObject.Equipped ?? ParentObject;
		GameObject target = gameObject.Target;
		if (target == null || gameObject.DistanceTo(target) > 1 || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			TurnsNextToTarget = 0;
		}
		else
		{
			TurnsNextToTarget++;
		}
	}
}
