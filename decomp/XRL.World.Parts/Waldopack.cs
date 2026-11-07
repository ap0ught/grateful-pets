using System;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class Waldopack : IPoweredPart
{
	public string ManagerID => ParentObject.ID + "::Waldopack";

	public Waldopack()
	{
		WorksOnEquipper = true;
		NameForStatus = "ServoSystems";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeDismemberEvent>.ID && ID != EquippedEvent.ID && ID != GetShortDescriptionEvent.ID && ID != GetTinkeringBonusEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == PooledEvent<GetMeleeAttackChanceEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDismemberEvent E)
	{
		if (E.IsObject && E.Part?.Manager != null && E.Part.Manager == ManagerID)
		{
			ParentObject.ApplyEffect(new Broken());
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			AddArm(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		RemoveArm(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (E.Intrinsic && !E.Primary && E.BodyPart?.Manager != null && E.BodyPart.Manager == ManagerID)
		{
			E.Chance -= 7;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if (E.ForSifrah && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Bonus++;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Options.AnySifrah)
		{
			E.Postfix.AppendRules("Adds a bonus turn in tinkering Sifrah games.");
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public void AddArm(GameObject Subject = null)
	{
		if (Subject == null)
		{
			Subject = ParentObject.Equipped;
		}
		Body body = Subject?.Body;
		if (body != null)
		{
			BodyPart body2 = body.GetBody();
			string managerID = ManagerID;
			bool? extrinsic = true;
			string[] orInsertBefore = new string[4] { "Hands", "Feet", "Roots", "Thrown Weapon" };
			body2.AddPartAt("Servo-Arm", 0, null, null, null, null, managerID, null, null, null, null, null, null, null, extrinsic, null, null, null, null, null, "Arm", orInsertBefore).AddPart("Servo-Claw", 0, null, null, null, null, Extrinsic: true, Manager: ManagerID);
			Subject.WantToReequip();
		}
	}

	public void RemoveArm(GameObject Subject = null)
	{
		if (Subject == null)
		{
			Subject = ParentObject.Equipped;
		}
		Subject?.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		Subject?.WantToReequip();
	}
}
