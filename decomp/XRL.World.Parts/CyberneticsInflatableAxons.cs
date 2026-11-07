using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsInflatableAxons : IPart
{
	public string commandId = "";

	public int Bonus = 40;

	public int Duration = 10;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		ActivatedAbilityEntry ability = MyActivatedAbility(ActivatedAbilityID, ParentObject?.Implantee);
		int num = stats.CollectComputePowerAdjustUp(ability, "Quickness bonus", Bonus);
		stats.Set("QuicknessBonus", num, num != Bonus, num - Bonus);
		int num2 = stats.CollectComputePowerAdjustUp(ability, "Duration", Duration);
		stats.Set("QuicknessDuration", num2, num2 != Duration, num2 - Duration);
		stats.CollectCooldownTurns(ability, GetCooldown());
	}

	public int GetCooldown()
	{
		return 100;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject?.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (!commandId.IsNullOrEmpty() && GameObject.Validate(E.Target) && GameObject.Validate(E.Actor) && E.Distance <= 1 && E.Actor == ParentObject.Implantee && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && (E.Actor.HasTagOrProperty("AIAbilityIgnoreDamage") || !E.Target.isDamaged(0.2, inclusive: true)) && (E.Actor.HasTagOrProperty("AIAbilityIgnoreCon") || Stat.Random(-10, 10) <= E.Target.Con(E.Actor, IgnoreHideCon: true)))
		{
			E.Add(commandId);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddDynamicCommand(out commandId, "CommandInflateAxons", "Inflate Axons", "Cybernetics", "You gain +" + Bonus + " quickness for " + Duration + " rounds, then you become sluggish for 10 rounds (-10 quickness).");
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
			int num = Duration;
			int num2 = Bonus;
			int num3 = GetAvailableComputePowerEvent.GetFor(E.Actor);
			if (num3 != 0)
			{
				num = num * (100 + num3) / 100;
				num2 = num2 * (100 + num3) / 100;
			}
			E.Actor.ApplyEffect(new AxonsInflated(num, num2, ParentObject));
			E.Actor.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown());
			ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
