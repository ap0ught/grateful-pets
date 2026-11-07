using System;

namespace XRL.World.Parts;

[Serializable]
public class RemotePowerSwitch : IPoweredPart
{
	public string FrequencyCode;

	public RemotePowerSwitch()
	{
		ChargeUse = 0;
		IsPowerSwitchSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as RemotePowerSwitch).FrequencyCode != FrequencyCode)
		{
			return false;
		}
		return base.SameAs(p);
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
		PowerSwitch powerSwitch = Connected();
		if (powerSwitch != null && ParentObject.Understood() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (powerSwitch.Active)
			{
				E.AddAction("Deactivate", "deactivate", "RemotePowerSwitchOff", null, 'a', FireOnActor: false, powerSwitch.DeactivateActionPriority, 0, Override: false, WorksAtDistance: false, powerSwitch.FlippableKinetically);
			}
			else
			{
				E.AddAction("Activate", "activate", "RemotePowerSwitchOn", null, 'a', FireOnActor: false, powerSwitch.ActivateActionPriority, 0, Override: false, WorksAtDistance: false, powerSwitch.FlippableKinetically);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "RemotePowerSwitchOn")
		{
			PowerSwitch powerSwitch = Connected();
			if (powerSwitch != null && E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				IComponent<GameObject>.XDidYToZ(E.Actor, powerSwitch.ActivateVerb, powerSwitch.ActivatePreposition, ParentObject, powerSwitch.ActivateExtra, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
				if (powerSwitch.KeylessActivation || powerSwitch.AccessCheck(E.Actor))
				{
					string text = GameText.VariableReplace(powerSwitch.ActivateSuccessMessage, ParentObject, (GameObject)null, E.Actor.IsPlayer());
					string text2 = GameText.VariableReplace(powerSwitch.ActivateFailureMessage, ParentObject, (GameObject)null, E.Actor.IsPlayer());
					if (ParentObject.FireEvent(Event.New("RemotePowerSwitchActivate", "Actor", E.Actor)))
					{
						if (!string.IsNullOrEmpty(text))
						{
							IComponent<GameObject>.EmitMessage(E.Actor, text, ' ', FromDialog: true);
						}
					}
					else if (!string.IsNullOrEmpty(text2))
					{
						IComponent<GameObject>.EmitMessage(E.Actor, text2, ' ', FromDialog: true);
					}
				}
				E.Actor.UseEnergy(1000, "Item Activate");
				E.RequestInterfaceExit();
			}
		}
		else if (E.Command == "RemotePowerSwitchOff")
		{
			PowerSwitch powerSwitch2 = Connected();
			if (powerSwitch2 != null && E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				IComponent<GameObject>.XDidYToZ(E.Actor, powerSwitch2.DeactivateVerb, powerSwitch2.DeactivatePreposition, ParentObject, powerSwitch2.DeactivateExtra, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
				if (powerSwitch2.KeylessDeactivation || powerSwitch2.AccessCheck(E.Actor))
				{
					string text3 = GameText.VariableReplace(powerSwitch2.DeactivateSuccessMessage, ParentObject, (GameObject)null, E.Actor.IsPlayer());
					string text4 = GameText.VariableReplace(powerSwitch2.DeactivateFailureMessage, ParentObject, (GameObject)null, E.Actor.IsPlayer());
					if (ParentObject.FireEvent(Event.New("RemotePowerSwitchDeactivate", "Actor", E.Actor)))
					{
						if (!string.IsNullOrEmpty(text3))
						{
							IComponent<GameObject>.EmitMessage(E.Actor, text3, ' ', FromDialog: true);
						}
					}
					else if (!string.IsNullOrEmpty(text4))
					{
						IComponent<GameObject>.EmitMessage(E.Actor, text4, ' ', FromDialog: true);
					}
				}
				E.Actor.UseEnergy(1000, "Item Deactivate");
				E.RequestInterfaceExit();
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanSmartUse");
		Registrar.Register("CommandSmartUseEarly");
		Registrar.Register("RemotePowerSwitchActivate");
		Registrar.Register("RemotePowerSwitchDeactivate");
		base.Register(Object, Registrar);
	}

	public PowerSwitch Connected()
	{
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return null;
		}
		Zone parentZone = cell.ParentZone;
		if (parentZone == null)
		{
			return null;
		}
		for (int i = 0; i < parentZone.Height; i++)
		{
			for (int j = 0; j < parentZone.Width; j++)
			{
				Cell cell2 = parentZone.GetCell(j, i);
				if (cell2 == null)
				{
					continue;
				}
				int k = 0;
				for (int count = cell2.Objects.Count; k < count; k++)
				{
					if (cell2.Objects[k].TryGetPart<PowerSwitch>(out var Part) && Part.FrequencyCode == FrequencyCode)
					{
						return Part;
					}
				}
			}
		}
		return null;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			if (Connected() != null && E.GetGameObjectParameter("User").IsPlayer() && ParentObject.Understood())
			{
				return false;
			}
		}
		else if (E.ID == "CommandSmartUseEarly")
		{
			if (Connected() != null && E.GetGameObjectParameter("User").IsPlayer() && ParentObject.Understood())
			{
				ParentObject.Twiddle();
				return false;
			}
		}
		else if (E.ID == "RemotePowerSwitchActivate")
		{
			PowerSwitch powerSwitch = Connected();
			if (powerSwitch == null)
			{
				return false;
			}
			if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
			Event obj = Event.New("PowerSwitchActivate");
			obj.SetParameter("Actor", E.GetGameObjectParameter("Actor"));
			obj.SetFlag("Forced", E.HasFlag("Forced"));
			if (!powerSwitch.FireEvent(obj))
			{
				return false;
			}
		}
		else if (E.ID == "RemotePowerSwitchDeactivate")
		{
			PowerSwitch powerSwitch2 = Connected();
			if (powerSwitch2 == null)
			{
				return false;
			}
			if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
			Event obj2 = Event.New("PowerSwitchDectivate");
			obj2.SetParameter("Actor", E.GetGameObjectParameter("Actor"));
			obj2.SetFlag("Forced", E.HasFlag("Forced"));
			if (!powerSwitch2.FireEvent(obj2))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
