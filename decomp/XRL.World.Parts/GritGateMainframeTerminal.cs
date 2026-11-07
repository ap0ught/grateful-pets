using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class GritGateMainframeTerminal : IPoweredPart
{
	public GritGateMainframeTerminal()
	{
		ChargeUse = 100;
		WorksOnSelf = true;
		NameForStatus = "GritGateMainframeTerminalInterface";
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
			E.AddAction("Interface", "interface", "InterfaceWithGritGateMainframe", null, 'i', FireOnActor: false, 100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "InterfaceWithGritGateMainframe")
		{
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				AccessTerminal(E.Actor);
				E.RequestInterfaceExit();
			}
			else
			{
				Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " unresponsive.");
			}
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
				AccessTerminal(gameObjectParameter);
			}
		}
		return base.FireEvent(E);
	}

	private void AccessTerminal(GameObject Actor)
	{
		if (The.Game.HasQuest("Grave Thoughts") && !The.Game.HasFinishedQuest("A Call to Arms"))
		{
			CyberneticsTerminal.ShowTerminal(ParentObject, Actor, null, new GritGateTerminalScreenRoot());
		}
		else if (The.Game.HasFinishedQuest("Decoding the Signal"))
		{
			CyberneticsTerminal.ShowTerminal(ParentObject, Actor, null, new GritGateTerminalScreenBasicAccess());
		}
		else
		{
			CyberneticsTerminal.ShowTerminal(ParentObject, Actor, null, new GritGateTerminalScreenGoodbye());
		}
	}
}
