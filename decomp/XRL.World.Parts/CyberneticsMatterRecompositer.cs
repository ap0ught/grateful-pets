using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsMatterRecompositer : IPart
{
	public string commandId = "";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		ActivatedAbilityEntry ability = MyActivatedAbility(ActivatedAbilityID, ParentObject?.Implantee);
		int num = stats.CollectComputePowerAdjustDown(ability, "Cooldown", GetBaseCooldown());
		stats.CollectCooldownTurns(ability, num, num - GetBaseCooldown());
	}

	public int GetBaseCooldown()
	{
		return 100;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetMovementAbilityListEvent.ID && ID != AIGetOffensiveAbilityListEvent.ID && ID != AIGetRetreatAbilityListEvent.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != GetMovementCapabilitiesEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		E.Add("Matter Recompositer", commandId, 11500, MyActivatedAbility(ActivatedAbilityID));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject.Implantee))
		{
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject?.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetMovementAbilityListEvent E)
	{
		if (E.TargetCell != null && !commandId.IsNullOrEmpty() && E.Distance >= 40 && GameObject.Validate(E.Actor) && E.Actor == ParentObject.Implantee && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && 10.in100() && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, null, E.Actor, ParentObject))
		{
			E.Add(commandId);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetRetreatAbilityListEvent E)
	{
		if (!commandId.IsNullOrEmpty() && (E.TargetCell == null || E.Distance >= 5) && GameObject.Validate(E.Actor) && E.Actor == ParentObject.Implantee && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, null, E.Actor, ParentObject))
		{
			E.Add(commandId);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (!commandId.IsNullOrEmpty() && E.Distance >= 40 && GameObject.Validate(E.Actor) && E.Actor == ParentObject.Implantee && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && 5.in100() && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, null, E.Actor, ParentObject))
		{
			E.Add(commandId);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddDynamicCommand(out commandId, "CommandEmergencyRecomposite", "Emergency Recomposite", "Cybernetics", null, "\a", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == commandId && E.Actor != null && E.Actor == ParentObject.Implantee && E.Actor.CurrentCell != null)
		{
			if (E.Actor.OnWorldMap())
			{
				return E.Actor.Fail("You cannot do that on the world map.");
			}
			List<Cell> cells = E.Actor.CurrentZone.GetCells((Cell c) => c.Explored && c.IsEmptyOfSolidFor(E.Actor));
			if (cells.Count <= 0)
			{
				return E.Actor.Fail("There are no places to escape to safely!");
			}
			Cell randomElement = cells.GetRandomElement();
			Event e = Event.New("InitiateRealityDistortionTransit", "Object", E.Actor, "Device", ParentObject, "Cell", randomElement);
			if (!ParentObject.FireEvent(e, E) || !randomElement.FireEvent(e, E))
			{
				return false;
			}
			E.Actor.TechTeleportSwirlOut();
			if (E.Actor.TeleportTo(randomElement, 0))
			{
				E.Actor.TechTeleportSwirlIn();
			}
			IComponent<GameObject>.XDidY(E.Actor, "teleport", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
			int turns = GetAvailableComputePowerEvent.AdjustDown(E.Actor, GetBaseCooldown());
			E.Actor.CooldownActivatedAbility(ActivatedAbilityID, turns);
			ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice reduces this item's cooldown.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
