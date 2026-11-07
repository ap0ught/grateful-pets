using System;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsTerminal2 : IPoweredPart, IHackingSifrahHandler
{
	public static readonly string COMMAND_NAME = "InterfaceWithBecomingNook";

	public CyberneticsTerminal2()
	{
		ChargeUse = 100;
		WorksOnSelf = true;
		NameForStatus = "BecomingNookInterface";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetPointsOfInterestEvent>.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("circuitry", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			E.Add(ParentObject, ParentObject.GetReferenceDisplayName(), null, null, null, null, null, 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (!AttemptInterface(E.Actor, E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Interface", "interface", COMMAND_NAME, null, 'i', FireOnActor: false, 100);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == COMMAND_NAME && !AttemptInterface(E.Actor, E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void HackingResultSuccess(GameObject Actor, GameObject Object, HackingSifrah Game)
	{
		if (Object == ParentObject)
		{
			int num = 1;
			while (30.in100())
			{
				num++;
			}
			ParentObject.ModIntProperty("HackLevel", num);
			if (ParentObject.GetIntProperty("SecurityAlertLevel") >= ParentObject.GetIntProperty("HackLevel"))
			{
				ParentObject.SetIntProperty("HackLevel", ParentObject.GetIntProperty("SecurityAlertLevel") + 1);
			}
			AskLowLevelHack(Actor);
		}
	}

	public void HackingResultExceptionalSuccess(GameObject Actor, GameObject Object, HackingSifrah Game)
	{
		if (Object == ParentObject)
		{
			int num = 2;
			while (50.in100())
			{
				num++;
			}
			ParentObject.ModIntProperty("HackLevel", num);
			if (ParentObject.GetIntProperty("SecurityAlertLevel") >= ParentObject.GetIntProperty("HackLevel"))
			{
				ParentObject.SetIntProperty("HackLevel", ParentObject.GetIntProperty("SecurityAlertLevel") + 1);
			}
			int num2 = 1;
			while (30.in100())
			{
				num2++;
			}
			if (Actor.IsPlayer())
			{
				Popup.Show("In the course of the hack, you are able to insert instructions into " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " granting you an extra " + ((num2 == 1) ? "cybernetics license point" : num2.Things("cybernetics license point")) + "!");
			}
			Actor.ModIntProperty("CyberneticsLicenses", num2);
			AskLowLevelHack(Actor);
		}
	}

	public void HackingResultPartialSuccess(GameObject Actor, GameObject Object, HackingSifrah Game)
	{
		if (Object == ParentObject && Actor.IsPlayer())
		{
			Popup.Show("The hack fails, but you manage to cover your tracks before any security measures kick in.");
		}
	}

	public void HackingResultFailure(GameObject Actor, GameObject Object, HackingSifrah Game)
	{
		if (Object == ParentObject)
		{
			int num = 1;
			while (30.in100())
			{
				num++;
			}
			ParentObject.ModIntProperty("SecurityAlertLevel", num);
			if (Actor.IsPlayer())
			{
				Popup.Show("The hack fails, and alert lights on " + ParentObject.t() + " begin pulsing rhythmically...");
			}
		}
	}

	public void HackingResultCriticalFailure(GameObject Actor, GameObject Object, HackingSifrah Game)
	{
		if (Object != ParentObject)
		{
			return;
		}
		if (Actor.HasPart<Dystechnia>())
		{
			FusionReactor part = ParentObject.GetPart<FusionReactor>();
			if (part == null || !part.Explode())
			{
				ParentObject.Explode(10000);
			}
			Game.RequestInterfaceExit();
			return;
		}
		int num = 2;
		while (50.in100())
		{
			num++;
		}
		ParentObject.ModIntProperty("SecurityAlertLevel", num);
		if (Actor.IsPlayer())
		{
			Popup.Show("The hack fails, and alert lights on " + ParentObject.t() + " begin pulsing urgently...");
		}
	}

	public bool AttemptInterface(GameObject Actor, IEvent FromEvent = null)
	{
		if (!GameObject.Validate(Actor))
		{
			return false;
		}
		if (!Actor.IsPlayer())
		{
			return false;
		}
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return Actor.Fail(ParentObject.Does("are") + " " + GetStatusPhrase() + ".");
		}
		CyberneticsTerminal.ShowTerminal(ParentObject, Actor);
		FromEvent?.RequestInterfaceExit();
		return true;
	}

	private void AskLowLevelHack(GameObject Actor)
	{
		if (!GameObject.Validate(ref Actor) || !Actor.IsPlayer())
		{
			return;
		}
		switch (Options.SifrahHackingLowLevel)
		{
		case "Ask":
			if (Popup.ShowYesNo("Do you want to use a low-level hack? Low-level hacks make it more difficult to read the terminal output but reduce the chance of triggering security alerts.", "Sounds/UI/ui_notification", AllowEscape: false) == DialogResult.Yes)
			{
				ParentObject.SetIntProperty("LowLevelHack", 1);
			}
			else
			{
				ParentObject.RemoveIntProperty("LowLevelHack");
			}
			break;
		case "Always":
			ParentObject.SetIntProperty("LowLevelHack", 1);
			break;
		case "Never":
			ParentObject.RemoveIntProperty("LowLevelHack");
			break;
		}
	}
}
