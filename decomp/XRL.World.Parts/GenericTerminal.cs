using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class GenericTerminal : IPoweredPart
{
	public string StartingScreen = "";

	public string StartingScreenTrueKin = "";

	public string StartingScreenMutant = "";

	public GenericTerminal()
	{
		ChargeUse = 100;
		WorksOnSelf = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.AddAction("Interface", "interface", "InterfaceWithTerminal", null, 'i', FireOnActor: false, 100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "InterfaceWithTerminal" && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			GameObject actor = E.Actor;
			if ((actor == null || actor.IsPlayer()) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (actor != null && actor.IsTrueKin() && !string.IsNullOrEmpty(StartingScreenTrueKin))
				{
					XRL.UI.GenericTerminal.ShowTerminal(ParentObject, actor, ModManager.CreateInstance<GenericTerminalScreen>(StartingScreenTrueKin));
				}
				else if (actor != null && !actor.IsTrueKin() && !string.IsNullOrEmpty(StartingScreenMutant))
				{
					XRL.UI.GenericTerminal.ShowTerminal(ParentObject, actor, ModManager.CreateInstance<GenericTerminalScreen>(StartingScreenTrueKin));
				}
				else
				{
					XRL.UI.GenericTerminal.ShowTerminal(ParentObject, actor, ModManager.CreateInstance<GenericTerminalScreen>(StartingScreen));
				}
			}
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanSmartUse");
		Registrar.Register("CommandSmartUse");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			if (E.GetGameObjectParameter("User").IsPlayer() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
		}
		else if (E.ID == "CommandSmartUse")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("User");
			if (gameObjectParameter.IsPlayer() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (gameObjectParameter.IsTrueKin() && !string.IsNullOrEmpty(StartingScreenTrueKin))
				{
					XRL.UI.GenericTerminal.ShowTerminal(ParentObject, gameObjectParameter, ModManager.CreateInstance<GenericTerminalScreen>(StartingScreenTrueKin));
				}
				else if (!gameObjectParameter.IsTrueKin() && !string.IsNullOrEmpty(StartingScreenMutant))
				{
					XRL.UI.GenericTerminal.ShowTerminal(ParentObject, gameObjectParameter, ModManager.CreateInstance<GenericTerminalScreen>(StartingScreenTrueKin));
				}
				else
				{
					XRL.UI.GenericTerminal.ShowTerminal(ParentObject, gameObjectParameter, ModManager.CreateInstance<GenericTerminalScreen>(StartingScreen));
				}
			}
		}
		return base.FireEvent(E);
	}
}
