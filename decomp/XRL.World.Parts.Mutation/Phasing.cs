using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Phasing : BaseMutation
{
	public Guid PhaseOutActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveAbilityListEvent.ID && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(PhaseOutActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetDefensiveAbilityListEvent E)
	{
		if (IsMyActivatedAbilityAIUsable(PhaseOutActivatedAbilityID) && !E.Actor.HasEffect<Phased>() && E.Actor.isDamaged(0.25) && CheckMyRealityDistortionAdvisability())
		{
			E.Add("CommandPhaseOut");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance > 1 && E.Distance < 5 + Level && IsMyActivatedAbilityAIUsable(PhaseOutActivatedAbilityID) && !E.Actor.HasEffect<Phased>() && CheckMyRealityDistortionAdvisability())
		{
			E.Add("CommandPhaseOut");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("glass", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterPhaseIn");
		Registrar.Register("BeginTakeAction");
		Registrar.Register("CommandPhaseIn");
		Registrar.Register("CommandPhaseOut");
		Registrar.Register("CommandTogglePhase");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You may phase through solid objects for brief periods of time.";
	}

	public int GetDuration(int Level)
	{
		return 6 + Level;
	}

	public int GetBaseCooldown(int Level)
	{
		return 103 - 3 * Level;
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("Duration: {{rules|" + GetDuration(Level) + "}} rounds\n", "Cooldown: {{rules|", GetBaseCooldown(Level).ToString(), "}} rounds");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Duration", GetDuration(Level));
		stats.CollectCooldownTurns(MyActivatedAbility(PhaseOutActivatedAbilityID, ParentObject), GetBaseCooldown(Level));
		base.CollectStats(stats, Level);
	}

	public void SyncAbilities()
	{
		ActivatedAbilityEntry activatedAbility = ParentObject.GetActivatedAbility(PhaseOutActivatedAbilityID);
		activatedAbility.ToggleState = ParentObject.HasEffect<Phased>();
		activatedAbility.Visible = true;
	}

	public bool IsPhased()
	{
		return ParentObject?.HasEffect<Phased>() ?? false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterPhaseIn")
		{
			SyncAbilities();
		}
		else if (E.ID == "BeginTakeAction")
		{
			SyncAbilities();
		}
		else
		{
			if (E.ID == "CommandTogglePhase")
			{
				return ParentObject.FireEvent(Event.New(IsPhased() ? "CommandPhaseIn" : "CommandPhaseOut"));
			}
			if (E.ID == "CommandPhaseOut")
			{
				if (ParentObject.OnWorldMap())
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You cannot do that on the world map.");
					}
					return false;
				}
				Event e = Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this);
				if (!ParentObject.FireEvent(e, E))
				{
					return false;
				}
				ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_phase");
				ParentObject.ApplyEffect(new Phased(6 + Level + 1));
				CooldownMyActivatedAbility(PhaseOutActivatedAbilityID, 103 - 3 * Level);
				SyncAbilities();
			}
			else if (E.ID == "CommandPhaseIn")
			{
				ParentObject.RemoveEffect<Phased>();
				SyncAbilities();
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		PhaseOutActivatedAbilityID = AddMyActivatedAbility("Phase", "CommandTogglePhase", "Physical Mutations", null, "°", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: true, IsAttack: false, IsRealityDistortionBased: true);
		SyncAbilities();
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref PhaseOutActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
