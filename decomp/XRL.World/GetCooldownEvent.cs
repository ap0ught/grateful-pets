using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class GetCooldownEvent : PooledEvent<GetCooldownEvent>
{
	public struct CooldownCalculation
	{
		public int PercentageReduction;

		public int LinearReduction;

		public string Reason;
	}

	public new static readonly int CascadeLevel = 1;

	public GameObject Actor;

	public ActivatedAbilityEntry Ability;

	public int Base;

	public int PercentageReduction;

	public int LinearReduction;

	public List<CooldownCalculation> Calculations = new List<CooldownCalculation>();

	public bool StoreCalculations;

	public int ResultUncapped => Base * (100 - PercentageReduction) / 100 - LinearReduction;

	public int Result => Math.Max(ResultUncapped, ActivatedAbilities.MinimumValueForCooldown(Base));

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Ability = null;
		Base = 0;
		PercentageReduction = 0;
		LinearReduction = 0;
		Calculations.Clear();
		StoreCalculations = false;
	}

	public void AddCalculation(string Reason, int PercentageReduction, int LinearReduction)
	{
		if (StoreCalculations)
		{
			Calculations.Add(new CooldownCalculation
			{
				PercentageReduction = PercentageReduction,
				LinearReduction = LinearReduction,
				Reason = Reason
			});
		}
	}

	public static GetCooldownEvent FromPool(GameObject Actor, ActivatedAbilityEntry Ability, int Base, int PercentageReduction = 0, int LinearReduction = 0, bool StoreCalculations = false)
	{
		GetCooldownEvent getCooldownEvent = PooledEvent<GetCooldownEvent>.FromPool();
		getCooldownEvent.Actor = Actor;
		getCooldownEvent.Ability = Ability;
		getCooldownEvent.Base = Base;
		getCooldownEvent.PercentageReduction = PercentageReduction;
		getCooldownEvent.LinearReduction = LinearReduction;
		getCooldownEvent.StoreCalculations = StoreCalculations;
		getCooldownEvent.Calculations.Clear();
		return getCooldownEvent;
	}

	public static int GetFor(GameObject Actor, ActivatedAbilityEntry Ability, int Base)
	{
		return TryCalculateFor(Actor, Ability, Base, 0, 0, StoreCalculations: false)?.Result ?? Base;
	}

	public static GetCooldownEvent TryCalculateFor(GameObject Actor, ActivatedAbilityEntry Ability, int Base, int PercentageReduction = 0, int LinearReduction = 0, bool StoreCalculations = true)
	{
		GetCooldownEvent getCooldownEvent = FromPool(Actor, Ability, Base, PercentageReduction, LinearReduction, StoreCalculations);
		if (Ability != null && Ability.ParentObject != null && Ability.AffectedByWillpower && Ability.ParentObject.HasStat("Willpower"))
		{
			int num = Math.Min(80, (Ability.ParentObject.Stat("Willpower") - 16) * 5);
			getCooldownEvent.PercentageReduction += num;
			if (StoreCalculations)
			{
				string text = ((num > 0) ? "high" : "low");
				getCooldownEvent.AddCalculation(text + " Willpower", num, 0);
			}
		}
		if (Actor != null && Actor.WantEvent(PooledEvent<GetCooldownEvent>.ID, CascadeLevel))
		{
			Actor.HandleEvent(getCooldownEvent);
		}
		return getCooldownEvent;
	}
}
