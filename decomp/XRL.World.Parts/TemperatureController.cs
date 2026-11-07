using System;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is set to
/// true, which it is not by default, temperature changes are increased
/// in magnitude by a percentage equal to ((power load - 100) / 10), i.e.
/// 30% for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class TemperatureController : IPoweredPart
{
	public static readonly int MIN_TEMPERATURE = -8000;

	public static readonly int MAX_TEMPERATURE = 8000;

	public static readonly int MIN_TARGET_REACHED_MESSAGE_TURNS = 2;

	public static readonly int MIN_TARGET_REACHED_MESSAGE_RESET_DIFFERENCE = 6;

	public int TemperatureAmountPercentage = 10;

	public int TemperatureAmountFloor = 5;

	public int TemperatureAmountChargeUsePercentage = 100;

	public int TemperatureTarget = 100;

	public int ConfigureEnergy = 100;

	public string BehaviorDescription;

	public string EquippedTargetReachedMessage = "=object.T= =object.verb:have= reached the target temperature.";

	public bool InactiveOnWorldMap;

	public bool TargetReached;

	public bool IgnoreResistance = true;

	public Guid ActivatedAbilityID;

	public static long LastTargetReachedMessage;

	public TemperatureController()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		TemperatureController temperatureController = p as TemperatureController;
		if (temperatureController.TemperatureAmountPercentage != TemperatureAmountPercentage)
		{
			return false;
		}
		if (temperatureController.TemperatureAmountFloor != TemperatureAmountFloor)
		{
			return false;
		}
		if (temperatureController.TemperatureAmountChargeUsePercentage != TemperatureAmountChargeUsePercentage)
		{
			return false;
		}
		if (temperatureController.TemperatureTarget != TemperatureTarget)
		{
			return false;
		}
		if (temperatureController.ConfigureEnergy != ConfigureEnergy)
		{
			return false;
		}
		if (temperatureController.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		if (temperatureController.EquippedTargetReachedMessage != EquippedTargetReachedMessage)
		{
			return false;
		}
		if (temperatureController.InactiveOnWorldMap != InactiveOnWorldMap)
		{
			return false;
		}
		if (temperatureController.TargetReached != TargetReached)
		{
			return false;
		}
		if (temperatureController.IgnoreResistance != IgnoreResistance)
		{
			return false;
		}
		if (temperatureController.ActivatedAbilityID != ActivatedAbilityID)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (InactiveOnWorldMap)
		{
			if (ActivePartHasMultipleSubjects())
			{
				foreach (GameObject activePartSubject in GetActivePartSubjects())
				{
					if (activePartSubject.OnWorldMap())
					{
						return true;
					}
				}
			}
			else
			{
				GameObject activePartFirstSubject = GetActivePartFirstSubject();
				if (activePartFirstSubject != null && activePartFirstSubject.OnWorldMap())
				{
					return true;
				}
			}
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "Inactive";
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("TemperatureChange", "roughly " + TemperatureAmountPercentage + "%");
		stats.Set("ChargeUse", "Temperature change x " + (float)TemperatureAmountChargeUsePercentage / 100f);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeTemperatureChangeEvent>.ID && ID != BootSequenceDoneEvent.ID && ID != PooledEvent<CanTemperatureReturnToAmbientEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != PooledEvent<ExamineSuccessEvent>.ID && ID != GetInventoryActionsEvent.ID && (ID != GetShortDescriptionEvent.ID || string.IsNullOrEmpty(BehaviorDescription)) && ID != InventoryActionEvent.ID && (ID != SingletonEvent<RadiatesHeatAdjacentEvent>.ID || !WorksOnAdjacentCellContents) && (ID != SingletonEvent<RadiatesHeatEvent>.ID || !WorksOnCellContents) && ID != EquippedEvent.ID && ID != UnequippedEvent.ID && (ID != ObjectCreatedEvent.ID || !WorksOnSelf))
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineSuccessEvent E)
	{
		if (E.Object == ParentObject && IsObjectActivePartSubject(ParentObject.Equipped))
		{
			SetUpActivatedAbility(ParentObject.Equipped);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "ConfigureTemperatureController")
		{
			ConfigureTemperatureController(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject != null && activePartFirstSubject.IsPlayer())
			{
				E.AddAction("ConfigureTemperatureController", "set target temperature", "ConfigureTemperatureController", null, 's', FireOnActor: false, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ConfigureTemperatureController")
		{
			ConfigureTemperatureController(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (WorksOnSelf)
		{
			SetUpActivatedAbility(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanTemperatureReturnToAmbientEvent E)
	{
		if (IsObjectActivePartSubject(E.Object) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, Math.Max(Math.Abs(E.Amount) * TemperatureAmountChargeUsePercentage / 100, 1), UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeTemperatureChangeEvent E)
	{
		if (E.Radiant && !E.IgnoreResistance && E.Object.Physics.Temperature == TemperatureTarget && Math.Abs(E.Amount) <= TemperatureAmountFloor && IsObjectActivePartSubject(E.Object) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, Math.Max(Math.Abs(E.Amount) * TemperatureAmountChargeUsePercentage / 100, 1), UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (ParentObject.Understood() && IsObjectActivePartSubject(E.Actor))
		{
			SetUpActivatedAbility(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(BehaviorDescription))
		{
			E.Postfix.AppendRules(BehaviorDescription, GetEventSensitiveAddStatusSummary(E));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatAdjacentEvent E)
	{
		if (WorksOnAdjacentCellContents && TemperatureAmountFloor > 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatEvent E)
	{
		if (WorksOnCellContents && TemperatureAmountFloor > 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		if (!ShouldResetMessaging())
		{
			TargetReached = true;
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		int num = MyPowerLoadLevel();
		int? powerLoadLevel = num;
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			return;
		}
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				AdjustTemperature(activePartSubject, num);
			}
			return;
		}
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject != null)
		{
			AdjustTemperature(activePartFirstSubject, num);
		}
	}

	public bool ShouldAdjustTemperature(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.Physics == null)
		{
			return false;
		}
		if (obj.Physics.SpecificHeat == 0f)
		{
			return false;
		}
		if (obj.Physics.Temperature == TemperatureTarget)
		{
			return false;
		}
		return true;
	}

	public bool ShouldAdjustTemperature()
	{
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				if (ShouldAdjustTemperature(activePartSubject))
				{
					return true;
				}
			}
			return false;
		}
		return ShouldAdjustTemperature(GetActivePartFirstSubject());
	}

	public bool ShouldResetMessaging(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.Physics == null)
		{
			return false;
		}
		if (obj.Physics.SpecificHeat == 0f)
		{
			return false;
		}
		if (obj.Physics.Temperature == TemperatureTarget)
		{
			return false;
		}
		if (Math.Abs(obj.Physics.Temperature - TemperatureTarget) < MIN_TARGET_REACHED_MESSAGE_RESET_DIFFERENCE)
		{
			return false;
		}
		return true;
	}

	public bool ShouldResetMessaging()
	{
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				if (ShouldResetMessaging(activePartSubject))
				{
					return true;
				}
			}
			return false;
		}
		return ShouldResetMessaging(GetActivePartFirstSubject());
	}

	public bool AdjustTemperature(GameObject obj, int PowerLoad = int.MinValue)
	{
		if (!ShouldAdjustTemperature(obj))
		{
			CheckMessaging(obj);
			return false;
		}
		if (ShouldResetMessaging(obj))
		{
			TargetReached = false;
		}
		int num = obj.Physics.Temperature - TemperatureTarget;
		int num2 = Math.Max(Math.Abs(num) * TemperatureAmountPercentage / 100, TemperatureAmountFloor);
		if (IsPowerLoadSensitive)
		{
			if (PowerLoad == int.MinValue)
			{
				PowerLoad = MyPowerLoadLevel();
			}
			int num3 = MyPowerLoadBonus(PowerLoad, 100, 10);
			if (num3 != 0)
			{
				num2 = num2 * (100 + num3) / 100;
			}
		}
		if (Math.Abs(num) < num2)
		{
			num2 = Math.Abs(num);
		}
		num2 = (int)((float)num2 * obj.Physics.SpecificHeat);
		num2 = Math.Max(1, num2);
		int value = Math.Max(1, num2 * TemperatureAmountChargeUsePercentage / 100);
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, value, UseChargeIfUnpowered: false, 0L, PowerLoad))
		{
			return false;
		}
		if (num > 0)
		{
			num2 = -num2;
		}
		obj.TemperatureChange(num2, obj.Equipped ?? obj.Implantee ?? obj, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance);
		CheckMessaging(obj);
		return true;
	}

	public void ConfigureTemperatureController(GameObject Actor, bool FreeAction = false)
	{
		SoundManager.PreloadClipSet("Sounds/Abilities/sfx_ability_dial_activated");
		if (Actor.IsPlayer())
		{
			int? num = Popup.AskNumber("Set target temperature.", "Sounds/UI/ui_notification", "", TemperatureTarget, MIN_TEMPERATURE, MAX_TEMPERATURE);
			if (num.HasValue && num != TemperatureTarget)
			{
				TemperatureTarget = num.Value;
				if (ConfigureEnergy > 0 && !FreeAction)
				{
					Actor.UseEnergy(ConfigureEnergy, "Item Temperature Controller");
				}
			}
			IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_interact_thermostat_change");
			int? num2 = num;
			IComponent<GameObject>.AddPlayerMessage("You set a target temperature of " + num2 + ".");
		}
		else
		{
			if (Actor == null)
			{
				return;
			}
			int val = GetIdealTemperatureEvent.GetFor(Actor);
			val = Math.Max(val, MIN_TEMPERATURE);
			val = Math.Min(val, MAX_TEMPERATURE);
			if (val != TemperatureTarget)
			{
				TemperatureTarget = val;
				if (ConfigureEnergy > 0 && !FreeAction)
				{
					Actor?.PlayWorldSound("Sounds/Abilities/sfx_ability_dial_activated");
					Actor.UseEnergy(ConfigureEnergy, "Item Temperature Controller");
				}
			}
		}
	}

	public void SetUpActivatedAbility(GameObject who)
	{
		if (who != null)
		{
			ActivatedAbilityID = who.AddActivatedAbility("Set Target Temperature", "ConfigureTemperatureController", (who == ParentObject) ? "Maneuvers" : "Items", null, "è");
			if (!who.IsPlayer() && !who.IsPlayerControlled())
			{
				ConfigureTemperatureController(who, FreeAction: true);
			}
		}
	}

	private void CheckMessaging(GameObject Object)
	{
		if (TargetReached || Object.Physics == null || Object.Physics.Temperature != TemperatureTarget)
		{
			return;
		}
		TargetReached = true;
		if (!EquippedTargetReachedMessage.IsNullOrEmpty() && Object.IsPlayer() && Object == ParentObject.Equipped && LastTargetReachedMessage <= XRLCore.CurrentTurn - MIN_TARGET_REACHED_MESSAGE_TURNS)
		{
			string text = GameText.VariableReplace(EquippedTargetReachedMessage, Object, ParentObject);
			if (!text.IsNullOrEmpty())
			{
				IComponent<GameObject>.AddPlayerMessage(text);
				LastTargetReachedMessage = XRLCore.CurrentTurn;
			}
		}
	}
}
