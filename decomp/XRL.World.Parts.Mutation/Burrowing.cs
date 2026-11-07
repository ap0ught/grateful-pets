using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Burrowing : BaseMutation
{
	public GameObject Target;

	public bool bBurrowWhenBored = true;

	public Brain _pBrain;

	public Brain pBrain
	{
		get
		{
			if (_pBrain == null)
			{
				_pBrain = ParentObject.Brain;
			}
			return _pBrain;
		}
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID && ID != AIGetDefensiveAbilityListEvent.ID && ID != SingletonEvent<BeforeTakeActionEvent>.ID && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetDefensiveAbilityListEvent E)
	{
		if (!E.Actor.HasEffect<Burrowed>() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandBurrow");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (bBurrowWhenBored && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && (ParentObject.GetIntProperty("Librarian") <= 0 || ParentObject.Target != null))
		{
			CommandEvent.Send(E.Actor, "CommandBurrow");
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeTakeActionEvent E)
	{
		if (ParentObject.HasEffect<Burrowed>())
		{
			DisableMyActivatedAbility(ActivatedAbilityID);
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
		}
		else
		{
			EnableMyActivatedAbility(ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandBurrow")
		{
			if (ParentObject.HasEffect<Burrowed>())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You are already burrowed.");
				}
				return false;
			}
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot burrow on the world map.");
				}
				return false;
			}
			if (!ParentObject.CanChangeBodyPosition("Burrowing", ShowMessage: true))
			{
				return false;
			}
			if (!ParentObject.CanChangeMovementMode("Burrowing", ShowMessage: true))
			{
				return false;
			}
			ParentObject.BodyPositionChanged();
			ParentObject.MovementModeChanged();
			ParentObject.ApplyEffect(new Burrowed(GetMSPenalty(Level)));
			ParentObject.UseEnergy(1000, "Physical Mutation");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
			if (ParentObject.HasEffect<Burrowed>())
			{
				DisableMyActivatedAbility(ActivatedAbilityID);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("salt", 1);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AIBurrow");
		Registrar.Register("BeginAttack");
		Registrar.Register("VillageInit");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You can travel underground by burrowing.";
	}

	public override string GetLevelText(int Level)
	{
		return "Cooldown: " + GetCooldown(Level) + " rounds\n";
	}

	public int GetCooldown(int Level)
	{
		return Math.Max(15 - Level, 5);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit")
		{
			bBurrowWhenBored = false;
		}
		return base.FireEvent(E);
	}

	public int GetMSPenalty(int Level)
	{
		return 100 - 10 * Level;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Burrow", "CommandBurrow", "Physical Mutations", null, "\u0019");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
