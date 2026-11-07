using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsNocturnalApex : IPart
{
	public bool Used;

	public string commandId = "";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		ActivatedAbilityEntry ability = MyActivatedAbility(ActivatedAbilityID, ParentObject?.Implantee);
		int num = stats.CollectComputePowerAdjustUp(ability, "Agility bonus", 6);
		stats.Set("AgilityBonus", num, num != 6, num - 6);
		int num2 = stats.CollectComputePowerAdjustUp(ability, "Move speed bonus", 10);
		stats.Set("MovespeedBonus", num2, num2 != 10, num2 - 10);
		int num3 = stats.CollectComputePowerAdjustUp(ability, "Duration", 100);
		stats.Set("Duration", num3, num3 != 100, num3 - 100);
		stats.Set("Cooldown", "once per night");
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID)
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
		if (!Used && IsNight() && !commandId.IsNullOrEmpty() && GameObject.Validate(E.Target) && GameObject.Validate(E.Actor) && E.Actor == ParentObject.Implantee && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && (E.Actor.HasTagOrProperty("AIAbilityIgnoreDamage") || !E.Target.isDamaged(0.5, inclusive: true)) && Stat.Random(1, 10) >= E.Distance && (E.Actor.HasTagOrProperty("AIAbilityIgnoreCon") || Stat.Random(0, 10) <= E.Target.Con(E.Actor, IgnoreHideCon: true)))
		{
			E.Add(commandId);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddDynamicCommand(out commandId, "CommandToggleNocturnalApex", "Prowl", "Cybernetics", "You gain +6 agility and +10 movespeed for 100 turns. Can only be activated at night.");
		E.Implantee.RegisterPartEvent(this, commandId);
		E.Implantee.RegisterPartEvent(this, "Regenerating2");
		E.Implantee.DisableActivatedAbility(ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, commandId);
		E.Implantee.UnregisterPartEvent(this, "Regenerating2");
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == commandId && E.Actor == ParentObject.Implantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee?.CurrentCell != null)
			{
				int num = 100;
				int num2 = -10;
				int num3 = 6;
				int num4 = GetAvailableComputePowerEvent.GetFor(implantee);
				if (num4 != 0)
				{
					num = num * (100 + num4) / 100;
					num2 = num2 * (100 + num4) / 100;
					num3 = num3 * (100 + num4) / 100;
				}
				implantee?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
				implantee.ApplyEffect(new NocturnalApexed(num, num2, num3, ParentObject));
				Used = true;
				implantee.DisableActivatedAbility(ActivatedAbilityID);
				implantee.CooldownActivatedAbility(ActivatedAbilityID, 1200);
				implantee.SetActivatedAbilityDisabledMessage(ActivatedAbilityID, "You've already prowled tonight.");
				ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		GameObject implantee = ParentObject.Implantee;
		if (E.Object == implantee)
		{
			if (!Used && IsNight())
			{
				implantee?.EnableActivatedAbility(ActivatedAbilityID);
			}
			else
			{
				if (Used && IsDay())
				{
					Used = false;
				}
				implantee?.DisableActivatedAbility(ActivatedAbilityID);
				if (!Used)
				{
					implantee?.SetActivatedAbilityDisabledMessage(ActivatedAbilityID, "You can only prowl at night.");
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Regenerating2" && IsDay())
		{
			float num = GetAvailableComputePowerEvent.AdjustUp(ParentObject.Implantee, 1.1f);
			E.SetParameter("Amount", (int)((float)E.GetIntParameter("Amount") * num));
		}
		return base.FireEvent(E);
	}
}
