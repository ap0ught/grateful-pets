using System;
using System.Text;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class RepellingForce : BaseMutation
{
	public const string ABL_CMD = "CommandRepellingForce";

	public const int ABL_CLD = 30;

	public RepellingForce()
	{
		Type = "Mental";
	}

	public override string GetDescription()
	{
		return "You invoke a repelling force in the surrounding area, throwing enemies back.";
	}

	public override string GetLevelText(int Level)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Compound("Area: 7x7", '\n');
		if (Level == base.Level)
		{
			stringBuilder.Compound("Creatures are pushed away from center of blast.", '\n');
		}
		else
		{
			stringBuilder.Compound("{{rules|Increased push force}}", '\n');
		}
		stringBuilder.Compound("Cooldown: ", '\n').Append(30).Append(" rounds");
		return stringBuilder.ToString();
	}

	public override bool Mutate(GameObject Object, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility(GetDisplayName(), "CommandRepellingForce", "Mental Mutations", null, "#");
		return base.Mutate(Object, Level);
	}

	public override bool Unmutate(GameObject Object)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(Object);
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Area", "7x7");
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 30);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 2 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandRepellingForce");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("might", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandRepellingForce");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandRepellingForce")
		{
			if (ParentObject.OnWorldMap())
			{
				return ParentObject.ShowFailure("You cannot use " + GetDisplayName(WithAnnotations: false) + " on the world map.");
			}
			DidX("flip", "polarity");
			StunningForce.Concussion(ParentObject.CurrentCell, ParentObject, Level, 3, ParentObject.GetPhase(), null, Stun: false, Damage: false);
			UseEnergy(1000);
			CooldownMyActivatedAbility(ActivatedAbilityID, 30);
		}
		return base.FireEvent(E);
	}
}
