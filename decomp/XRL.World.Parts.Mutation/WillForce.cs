using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class WillForce : BaseMutation
{
	public Guid StrengthActivatedAbilityID = Guid.Empty;

	public Guid AgilityActivatedAbilityID = Guid.Empty;

	public Guid ToughnessActivatedAbilityID = Guid.Empty;

	public WillForce()
	{
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(StrengthActivatedAbilityID, CollectStats);
		DescribeMyActivatedAbility(AgilityActivatedAbilityID, CollectStats);
		DescribeMyActivatedAbility(ToughnessActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 4)
		{
			if (IsMyActivatedAbilityAIUsable(StrengthActivatedAbilityID))
			{
				E.Add("CommandWillForceStrength", (E.Actor.BaseStat("Strength") <= E.Actor.BaseStat("Agility") || E.Actor.BaseStat("Strength") <= E.Actor.BaseStat("Toughness")) ? 1 : 3);
			}
			if (IsMyActivatedAbilityAIUsable(AgilityActivatedAbilityID))
			{
				E.Add("CommandWillForceAgility", (E.Actor.BaseStat("Agility") <= E.Actor.BaseStat("Strength") || E.Actor.BaseStat("Agility") <= E.Actor.BaseStat("Toughness")) ? 1 : 3);
			}
			if (IsMyActivatedAbilityAIUsable(ToughnessActivatedAbilityID))
			{
				E.Add("CommandWillForceToughness");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("jewels", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandWillForceAgility");
		Registrar.Register("CommandWillForceStrength");
		Registrar.Register("CommandWillForceToughness");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "Through sheer force of will, you perform uncanny physical feats.";
	}

	public int GetLowDuration(int Level)
	{
		return 16 + 2 * Level;
	}

	public int GetHighDuration(int Level)
	{
		return 20 + 2 * Level;
	}

	public override string GetLevelText(int Level)
	{
		string text = "Augments one physical attribute by an amount equal to twice your Ego bonus\n";
		text = text + "Duration: {{rules|" + GetLowDuration(Level) + "-" + GetHighDuration(Level) + "}} rounds\n";
		return text + "Cooldown: 200 rounds";
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Bonus", Math.Max(ParentObject.StatMod("Ego") * 2, 1));
		stats.Set("Duration", GetLowDuration(Level) + "-" + GetHighDuration(Level));
		stats.CollectCooldownTurns(MyActivatedAbility(StrengthActivatedAbilityID), 200);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandWillForceStrength")
		{
			if (!ActivateWillForce("Strength"))
			{
				return false;
			}
		}
		else if (E.ID == "CommandWillForceAgility")
		{
			if (!ActivateWillForce("Agility"))
			{
				return false;
			}
		}
		else if (E.ID == "CommandWillForceToughness" && !ActivateWillForce("Toughness"))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool ActivateWillForce(string Stat)
	{
		ParentObject.UseEnergy(1000, "Mental Mutation EgoProjection");
		CooldownMyActivatedAbility(StrengthActivatedAbilityID, 200);
		CooldownMyActivatedAbility(AgilityActivatedAbilityID, 200);
		CooldownMyActivatedAbility(ToughnessActivatedAbilityID, 200);
		int num = Math.Max(ParentObject.StatMod("Ego") * 2, 1);
		int num2 = XRL.Rules.Stat.Random(GetLowDuration(Level), GetHighDuration(Level));
		foreach (Effect effect in ParentObject.Effects)
		{
			if (effect is BoostStatistic boostStatistic && boostStatistic.Statistic == Stat)
			{
				if (boostStatistic.Bonus < num)
				{
					boostStatistic.Duration = 0;
					ParentObject.CleanEffects();
					break;
				}
				if (boostStatistic.Duration < num2)
				{
					boostStatistic.Duration = num2;
				}
				return true;
			}
		}
		ParentObject.ApplyEffect(new BoostStatistic(num2, Stat, num));
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		StrengthActivatedAbilityID = AddMyActivatedAbility("Boost Strength", "CommandWillForceStrength", "Mental Mutations", null, "¾");
		AgilityActivatedAbilityID = AddMyActivatedAbility("Boost Agility", "CommandWillForceAgility", "Mental Mutations", null, "\u00af");
		ToughnessActivatedAbilityID = AddMyActivatedAbility("Boost Toughness", "CommandWillForceToughness", "Mental Mutations", null, "\u0003");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref StrengthActivatedAbilityID);
		RemoveMyActivatedAbility(ref AgilityActivatedAbilityID);
		RemoveMyActivatedAbility(ref ToughnessActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
