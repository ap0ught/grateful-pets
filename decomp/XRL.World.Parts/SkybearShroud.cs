using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class SkybearShroud : IPart
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public int GetCooldown()
	{
		return 100;
	}

	public int GetDuration()
	{
		return 10;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Duration", GetDuration());
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID, ParentObject.Equipped), GetCooldown());
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetMovementAbilityListEvent.ID && ID != AIGetOffensiveAbilityListEvent.ID && ID != EquippedEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetMovementAbilityListEvent E)
	{
		if (E.Actor == ParentObject.Equipped && E.Distance - E.StandoffDistance * 2 >= 10 && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("ActivateSkyshroud", 1, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Actor == ParentObject.Equipped && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("ActivateSkyshroud", 1, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (ParentObject.IsEquippedProperly())
		{
			E.Actor.RegisterPartEvent(this, "ActivateSkyshroud");
			ActivatedAbilityID = E.Actor.AddActivatedAbility("Activate Flume-Flier", "ActivateSkyshroud", "Items");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.RemoveEffect<Dashing>();
		E.Actor.UnregisterPartEvent(this, "ActivateSkyshroud");
		E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped != null && equipped.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			E.AddAction("Activate", "activate", "ActivateSkyshroud", null, 'a');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateSkyshroud")
		{
			ActivateSkyshroud();
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ActivateSkyshroud" && !ActivateSkyshroud())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	private bool ActivateSkyshroud()
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped == null)
		{
			return false;
		}
		if (!equipped.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			return false;
		}
		if (!equipped.ApplyEffect(new Dashing(GetDuration())))
		{
			return false;
		}
		IComponent<GameObject>.XDidY(equipped, "start", "dashing in a plume of flame and smoke", "!", null, null, equipped);
		equipped.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown());
		equipped.PlayWorldSound("Sounds/Interact/sfx_interact_jetpack_activate");
		return true;
	}
}
