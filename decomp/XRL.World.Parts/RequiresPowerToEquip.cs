using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class RequiresPowerToEquip : IPoweredPart
{
	public RequiresPowerToEquip()
	{
		WorksOnEquipper = true;
		IsBootSensitive = false;
		ChargeUse = 0;
		ChargeMinimum = 1;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EffectAppliedEvent.ID)
		{
			return ID == EffectRemovedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckEquip();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckEquip();
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		CheckEquip();
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginBeingEquipped");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginBeingEquipped" && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			string text = ParentObject.The + ParentObject.DisplayNameOnly + ParentObject.GetVerb("do") + " not seem operational.";
			if (E.GetIntParameter("AutoEquipTry") > 0)
			{
				E.SetParameter("FailureMessage", text);
			}
			else if (E.GetGameObjectParameter("Equipper").IsPlayer())
			{
				Popup.Show(text);
			}
			return false;
		}
		return base.FireEvent(E);
	}

	public void CheckEquip()
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped != null && !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ParentObject.ForceUnequip(Silent: true);
			if (ParentObject.Equipped == null && equipped.IsPlayer())
			{
				Popup.Show(equipped.Poss(ParentObject) + ParentObject.GetVerb("stop") + " operating; you unequip " + ParentObject.them + ".");
			}
		}
	}
}
