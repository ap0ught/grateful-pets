using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class Smartgun : IPoweredPart
{
	public int Level = 1;

	public string EquipperPropertyEnables = "TechScannerEquipped";

	public string EquipperEventEnables = "HandleSmartData";

	public long UseTurn;

	[NonSerialized]
	public static Event eHandleSmartData = new ImmutableEvent("HandleSmartData");

	public Smartgun()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		Smartgun smartgun = p as Smartgun;
		if (smartgun.Level != Level)
		{
			return false;
		}
		if (smartgun.EquipperPropertyEnables != EquipperPropertyEnables)
		{
			return false;
		}
		if (smartgun.EquipperEventEnables != EquipperEventEnables)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetToHitModifierEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetToHitModifierEvent E)
	{
		if (E.Checking == "Actor" && IsObjectActivePartSubject(E.Actor) && E.Target == E.AimedAt && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && !E.Prospective && UseTurn < XRLCore.Core.Game.Turns)
		{
			UseTurn = XRLCore.Core.Game.Turns;
			ConsumeCharge();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ModifyAimVariance");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ModifyAimVariance" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (UseTurn < XRLCore.Core.Game.Turns)
			{
				UseTurn = XRLCore.Core.Game.Turns;
				ConsumeCharge();
			}
			E.SetParameter("Amount", E.GetIntParameter("Amount") - Level);
		}
		return base.FireEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool IsDataReady()
	{
		GameObject gameObject = ParentObject.Equipped ?? ParentObject.Implantee;
		if (gameObject == null)
		{
			return false;
		}
		if (!EquipperPropertyEnables.IsNullOrEmpty() && gameObject.GetIntProperty(EquipperPropertyEnables) > 0)
		{
			return true;
		}
		if (!EquipperEventEnables.IsNullOrEmpty())
		{
			if (EquipperEventEnables == eHandleSmartData.ID)
			{
				if (!gameObject.FireEvent(eHandleSmartData))
				{
					return true;
				}
			}
			else if (!gameObject.FireEvent(EquipperEventEnables))
			{
				return true;
			}
		}
		return false;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return !IsDataReady();
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "DataProviderNotFound";
	}
}
