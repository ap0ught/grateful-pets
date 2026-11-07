using System;

namespace XRL.World.Parts;

[Serializable]
public class AIStopShootingImmuneTargets : AIBehaviorPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIWantUseWeaponEvent>.ID)
		{
			return ID == PooledEvent<NotifyTargetImmuneEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIWantUseWeaponEvent E)
	{
		if (E.Object == ParentObject && GameObject.Validate(E.Target) && E.Target.HasID && ParentObject.GetIntProperty(ImmunityKey(E.Target)) > 0)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NotifyTargetImmuneEvent E)
	{
		if (E.Weapon == ParentObject && GameObject.Validate(E.Target))
		{
			ParentObject.SetIntProperty(ImmunityKey(E.Target), 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private string ImmunityKey(GameObject Target)
	{
		return "TargetImmune" + Target.ID;
	}
}
