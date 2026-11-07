using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ErosTeleportation : BaseMutation
{
	public ErosTeleportation()
	{
		Type = "Mental";
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetMovementAbilityListEvent.ID && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetMovementAbilityListEvent E)
	{
		if (E.TargetCell != null && E.Distance > 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Actor.PartyLeader != null && E.Actor.PartyLeader.Target != null && E.Actor.PartyLeader.DistanceTo(E.Target) <= 1 && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, E.TargetCell, E.Actor, null, this))
		{
			E.Add("CommandLeaderTeleport");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance > 3 && GameObject.Validate(E.Target) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Actor.PartyLeader != null && E.Actor.PartyLeader.Target != null && E.Actor.PartyLeader.DistanceTo(E.Target) <= 1 && CheckMyRealityDistortionAdvisability())
		{
			E.Add("CommandLeaderTeleport");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandLeaderTeleport");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You teleport to a nearby location near your leader.";
	}

	public override string GetLevelText(int Level)
	{
		return "Cooldown: " + GetCooldown(Level) + " rounds";
	}

	public int GetCooldown(int Level)
	{
		int num = 125 - 10 * Level;
		if (num < 5)
		{
			num = 5;
		}
		return num;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public static bool Cast(ErosTeleportation mutation = null, string level = "5-6", Event E = null, Cell Destination = null)
	{
		if (mutation == null)
		{
			mutation = new ErosTeleportation();
			mutation.ParentObject = The.Player;
			mutation.Level = Stat.Roll(level);
		}
		Cell cell = null;
		GameObject parentObject = mutation.ParentObject;
		if (!parentObject.IsRealityDistortionUsable())
		{
			RealityStabilized.ShowGenericInterdictMessage(parentObject);
			return false;
		}
		if (parentObject.PartyLeader != null)
		{
			Cell cell2 = parentObject.PartyLeader.CurrentCell;
			if (cell2 != null)
			{
				cell = cell2.GetEmptyAdjacentCells().GetRandomElement();
			}
		}
		if (cell == null)
		{
			return false;
		}
		if (parentObject.IsPlayer())
		{
			if (!cell.IsExplored())
			{
				Popup.ShowFail("You can only teleport to a place you have seen before!");
				return false;
			}
			if (!cell.IsEmptyOfSolid())
			{
				Popup.ShowFail("You may only teleport into an empty square!");
				return false;
			}
		}
		Event e = Event.New("InitiateRealityDistortionTransit", "Object", parentObject, "Mutation", mutation, "Cell", cell);
		if (!parentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
		{
			return false;
		}
		parentObject.ParticleBlip("&C\u000f", 10, 0L);
		parentObject.TeleportTo(cell, 0);
		parentObject.TeleportSwirl(null, "&C", Voluntary: true);
		parentObject.ParticleBlip("&C\u000f", 10, 0L);
		mutation.UseEnergy(1000, "Mental Mutation E-Ros Teleportation");
		mutation.CooldownMyActivatedAbility(mutation.ActivatedAbilityID, Math.Max(125 - 10 * mutation.Level, 5));
		IComponent<GameObject>.EmitMessage(parentObject, "E-Ros yells, {{W|'I'm coming, " + parentObject.PartyLeader.BaseDisplayName + "!'}}");
		parentObject.ParticleText("I'm coming, " + parentObject.PartyLeader.BaseDisplayName + "!", 'W');
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandLeaderTeleport" && !Cast(this, null, E, E.GetParameter("TargetCell") as Cell))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Teleport", "CommandLeaderTeleport", "Mental Mutations", null, "\u001d", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
