using System;
using XRL.Messages;
using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Capabilities;

public static class Flight
{
	public static readonly string SWOOP_ATTACK_COMMAND_NAME = "CommandSwoopAttack";

	[NonSerialized]
	private static int BestFlyingLevel;

	public static bool IsFlying(GameObject obj)
	{
		return obj?.IsFlying ?? false;
	}

	private static void CollectBestFlyingLevel(Effect RFX)
	{
		if (RFX is Flying flying && flying.Level > BestFlyingLevel)
		{
			BestFlyingLevel = flying.Level;
		}
	}

	public static int GetBestFlyingLevel(GameObject obj)
	{
		BestFlyingLevel = 0;
		obj.ForeachEffect(CollectBestFlyingLevel);
		return BestFlyingLevel;
	}

	public static void SuspendFlight(GameObject obj)
	{
		obj.ModIntProperty("SuspendFlight", 1);
	}

	public static void DesuspendFlight(GameObject obj)
	{
		obj.ModIntProperty("SuspendFlight", -1, RemoveIfZero: true);
	}

	public static bool EnvironmentAllowsFlight(Zone Z)
	{
		if (Z != null)
		{
			if (!Z.IsWorldMap())
			{
				return Z.GetZoneZ() <= 10;
			}
			return true;
		}
		return false;
	}

	public static bool EnvironmentAllowsFlight(Cell C)
	{
		if (C != null)
		{
			if (!C.HasObjectWithTag("FlyingWhitelistArea"))
			{
				return EnvironmentAllowsFlight(C.ParentZone);
			}
			return true;
		}
		return false;
	}

	public static bool EnvironmentAllowsFlight(GameObject GO)
	{
		return EnvironmentAllowsFlight(GO?.CurrentCell);
	}

	private static IFlightSource FlightSource(GameObject Source)
	{
		return Source?.GetFirstFlightSourcePart();
	}

	private static IFlightSource FlightSource(GameObject Source, Predicate<IFlightSource> Filter)
	{
		return Source?.GetFirstFlightSourcePart(Filter);
	}

	private static bool HasActivatedAbilityID(IFlightSource FS)
	{
		return FS.FlightActivatedAbilityID != Guid.Empty;
	}

	public static bool IsAbilityUsable(IFlightSource FS, GameObject Object)
	{
		if (FS == null)
		{
			return false;
		}
		return Object.IsActivatedAbilityUsable(FS.FlightActivatedAbilityID);
	}

	public static bool IsAbilityUsable(GameObject Source, GameObject Object)
	{
		return IsAbilityUsable(FlightSource(Source, HasActivatedAbilityID), Object);
	}

	public static bool IsAbilityAIUsable(IFlightSource FS, GameObject Object)
	{
		if (FS == null)
		{
			return false;
		}
		return Object.IsActivatedAbilityAIUsable(FS.FlightActivatedAbilityID);
	}

	public static bool IsAbilityAIUsable(GameObject Source, GameObject Object)
	{
		return IsAbilityAIUsable(FlightSource(Source, HasActivatedAbilityID), Object);
	}

	public static bool AbilitySetup(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return false;
			}
		}
		string text = "Fly";
		if (FS.FlightSourceDescription != null)
		{
			text = text + " (" + FS.FlightSourceDescription + ")";
		}
		FS.FlightActivatedAbilityID = Object.AddActivatedAbility(text, FS.FlightEvent, FS.FlightActivatedAbilityClass, null, "\u009d", null, Toggleable: true, FS.FlightFlying, ActiveToggle: true, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, "CommandFlyToggle");
		return true;
	}

	public static bool AbilityTeardown(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		IFlightSource flightSource = FS ?? FlightSource(Source, HasActivatedAbilityID);
		if (flightSource != null)
		{
			Guid ID = flightSource.FlightActivatedAbilityID;
			Object.RemoveActivatedAbility(ref ID);
			flightSource.FlightActivatedAbilityID = ID;
		}
		return true;
	}

	public static int GetMoveFallChance(GameObject Object, IFlightSource FS)
	{
		int num = ((FS == null) ? int.MaxValue : (FS.FlightBaseFallChance - FS.FlightLevel));
		foreach (Flying item in Object.YieldEffects<Flying>())
		{
			if (item is Flying { Source: not null } flying)
			{
				IFlightSource flightSource = FlightSource(flying.Source);
				int num2 = flightSource.FlightBaseFallChance - flightSource.FlightLevel;
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return Math.Max(0, (num != int.MaxValue) ? num : 0);
	}

	public static int GetMoveFallChance(GameObject Object, GameObject Source = null)
	{
		return GetMoveFallChance(Object, (Source == null) ? null : FlightSource(Source));
	}

	public static int GetSwoopFallChance(GameObject Object, IFlightSource FS)
	{
		int num = ((FS == null) ? int.MaxValue : (FS.FlightBaseFallChance * 4 - FS.FlightLevel));
		foreach (Flying item in Object.YieldEffects<Flying>())
		{
			if (item is Flying { Source: not null } flying)
			{
				IFlightSource flightSource = FlightSource(flying.Source);
				int num2 = flightSource.FlightBaseFallChance * 4 - flightSource.FlightLevel;
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return Math.Max(0, (num != int.MaxValue) ? num : 0);
	}

	public static int GetSwoopFallChance(GameObject Object, GameObject Source = null)
	{
		return GetSwoopFallChance(Object, (Source == null) ? null : FlightSource(Source));
	}

	public static bool MaintainFlight(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
		}
		if (FS == null)
		{
			return FailFlying(Source, Object);
		}
		if (FS.FlightFlying)
		{
			if (FS.FlightRequiresOngoingEffort)
			{
				if (!Object.CanMoveExtremities("Fly"))
				{
					return FailFlying(Source, Object);
				}
				PowerSwitch part = Object.GetPart<PowerSwitch>();
				if (part != null && !part.Active)
				{
					return FailFlying(Source, Object);
				}
			}
			if (!Object.OnWorldMap())
			{
				int num = FS.FlightBaseFallChance - FS.FlightLevel;
				if (num > 0 && Stat.Random(1, 200) <= num)
				{
					return FailFlying(Source, Object, FS);
				}
			}
		}
		return true;
	}

	public static bool CheckFlight(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return StopFlying(Source, Object);
			}
		}
		if (FS.FlightFlying && !Object.OnWorldMap() && !EnvironmentAllowsFlight(Object) && !Object.CurrentCell.HasObjectWithPart("StairsDown"))
		{
			return StopFlying(Source, Object, FS);
		}
		return true;
	}

	public static Flying GetFlyingEffectFromSource(GameObject Source, GameObject Object)
	{
		if (GameObject.Validate(ref Source) && GameObject.Validate(ref Object))
		{
			foreach (Flying item in Object.YieldEffects<Flying>())
			{
				if (item is Flying flying && flying.Source == Source)
				{
					return flying;
				}
			}
		}
		return null;
	}

	public static bool StartFlying(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return false;
			}
		}
		if (FS.FlightFlying)
		{
			return false;
		}
		if (!EnvironmentAllowsFlight(Object))
		{
			return Object.Fail("You can't fly underground!");
		}
		if (!Object.CanChangeMovementMode("Flying", ShowMessage: true))
		{
			return false;
		}
		Object.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_wings_fly_move");
		if (Object.GetEffectCount(typeof(Flying)) == 0)
		{
			if (Object.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("You begin flying!");
			}
			else if (Object.IsVisible())
			{
				MessageQueue.AddPlayerMessage(Object.Does("begin") + " flying.");
			}
			Object.AddActivatedAbility("Swoop", SWOOP_ATTACK_COMMAND_NAME, "Maneuvers", null, "û", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, The.Game.HasBooleanGameState(SWOOP_ATTACK_COMMAND_NAME));
			The.Game.SetBooleanGameState(SWOOP_ATTACK_COMMAND_NAME, Value: true);
			Object.MovementModeChanged("Flying");
		}
		else if (Object.IsPlayer())
		{
			MessageQueue.AddPlayerMessage("You begin using an additional flight capability.");
		}
		FS.FlightFlying = true;
		Object.ApplyEffect(new Flying(FS.FlightLevel, Source));
		Object.RemoveEffect<Prone>();
		Object.ToggleActivatedAbility(FS.FlightActivatedAbilityID);
		Object.FireEvent("FlightStarted");
		ObjectStartedFlyingEvent.SendFor(Object);
		return true;
	}

	private static void NoLongerFlying(GameObject Object)
	{
		Object.Abilities?.RemoveAbilityByCommand(SWOOP_ATTACK_COMMAND_NAME);
	}

	public static bool StopFlying(GameObject Source, GameObject Object, IFlightSource FS = null, bool Silent = false, bool FromFail = false)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return false;
			}
		}
		if (!FS.FlightFlying)
		{
			return false;
		}
		if (Object == null)
		{
			FS.FlightFlying = false;
			return false;
		}
		int effectCount = Object.GetEffectCount(typeof(Flying));
		Object.GetCurrentCell();
		if (!Silent)
		{
			if (effectCount <= 1)
			{
				if (Object.IsPlayer())
				{
					MessageQueue.AddPlayerMessage("You return to the ground.");
				}
				else if (Object.IsVisible())
				{
					MessageQueue.AddPlayerMessage(Object.Does("return") + " to the ground.");
				}
			}
			else if (Object.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("You cease using one of your flight capabilities.");
			}
		}
		FS.FlightFlying = false;
		if (!Object.RemoveEffect(typeof(Flying), (Effect FX) => (FX as Flying).Source == Source))
		{
			Object.RemoveEffect<Flying>();
		}
		Object.ToggleActivatedAbility(FS.FlightActivatedAbilityID);
		Object.FireEvent("FlightStoppedFromOneSource");
		if (effectCount <= 1)
		{
			Object.FireEvent("FlightStopped");
			if (!FromFail)
			{
				NoLongerFlying(Object);
				Object.MovementModeChanged("NotFlying");
			}
			ObjectStoppedFlyingEvent.SendFor(Object);
		}
		Object.Gravitate();
		return true;
	}

	private static bool HasSource(Flying FX)
	{
		return FX.Source != null;
	}

	public static void Land(GameObject Object, bool Silent = false)
	{
		if (!Object.HasEffect<Flying>(HasSource))
		{
			return;
		}
		if (!Silent)
		{
			if (Object.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("You return to the ground.");
			}
			else if (Object.IsVisible())
			{
				MessageQueue.AddPlayerMessage(Object.Does("return") + " to the ground.");
			}
		}
		foreach (Flying item in Object.YieldEffects<Flying>(HasSource))
		{
			StopFlying(item.Source, Object, null, Silent: true);
		}
	}

	public static bool FailFlying(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return false;
			}
		}
		if (!FS.FlightFlying)
		{
			return false;
		}
		if (Object == null)
		{
			FS.FlightFlying = false;
			return false;
		}
		Object.StopMoving();
		int effectCount = Object.GetEffectCount(typeof(Flying));
		if (effectCount <= 1)
		{
			Object.PlayWorldSound("fly_generic_fall");
			if (Object.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("You fall to the ground!", 'R');
			}
			else if (Object.IsVisible())
			{
				MessageQueue.AddPlayerMessage(Object.Does("fall") + " to the ground.");
			}
		}
		else if (Object.IsPlayer())
		{
			MessageQueue.AddPlayerMessage("One of your flight capabilities fails.", 'R');
		}
		bool result = StopFlying(Source, Object, FS, Silent: true, FromFail: true);
		Object.CooldownActivatedAbility(FS.FlightActivatedAbilityID, 5, null, Involuntary: true);
		Object.FireEvent("FlightFailedFromOneSource");
		if (effectCount <= 1)
		{
			Object.FireEvent("FlightFailed");
			NoLongerFlying(Object);
			Object.MovementModeChanged("NotFlying", Involuntary: true);
			ObjectStoppedFlyingEvent.SendFor(Object);
			Object.ApplyEffect(new Prone());
		}
		return result;
	}

	public static void Fall(GameObject Object)
	{
		foreach (Flying item in Object.YieldEffects<Flying>())
		{
			Flying flying = item as Flying;
			if (flying?.Source != null)
			{
				FailFlying(flying.Source, Object);
			}
		}
	}

	public static void SyncFlying(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (Source == null || Object == null)
		{
			return;
		}
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return;
			}
		}
		bool flag = false;
		if (FS.FlightFlying)
		{
			flag = true;
			bool flag2 = false;
			foreach (Flying item in Object.YieldEffects<Flying>())
			{
				if ((item as Flying)?.Source == Source)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				FS.FlightFlying = false;
				if (Object.IsActivatedAbilityToggledOn(FS.FlightActivatedAbilityID))
				{
					Object.ToggleActivatedAbility(FS.FlightActivatedAbilityID);
				}
			}
		}
		else
		{
			foreach (Flying item2 in Object.YieldEffects<Flying>())
			{
				flag = true;
				Flying flying = item2 as Flying;
				if (flying?.Source == Source)
				{
					Object.RemoveEffect(flying);
				}
			}
		}
		int effectCount = Object.GetEffectCount(typeof(Flying));
		if (flag && effectCount <= 0)
		{
			NoLongerFlying(Object);
		}
	}
}
