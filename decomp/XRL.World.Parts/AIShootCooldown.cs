using System;

namespace XRL.World.Parts;

[Serializable]
public class AIShootCooldown : AIBehaviorPart
{
	public string Cooldown = "1d6";

	public int CurrentCooldown;

	public long LastFireTimeTick;

	public override bool SameAs(IPart p)
	{
		if ((p as AIShootCooldown).Cooldown != Cooldown)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIAfterMissileEvent>.ID)
		{
			return ID == PooledEvent<AIWantUseWeaponEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIWantUseWeaponEvent E)
	{
		if (CooldownActive())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIAfterMissileEvent E)
	{
		LastFireTimeTick = The.Game.TimeTicks;
		CurrentCooldown = Math.Max(CurrentCooldown, Cooldown.RollCached());
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool CooldownActive()
	{
		if (LastFireTimeTick != The.Game.TimeTicks)
		{
			return LastFireTimeTick + CurrentCooldown > The.Game.TimeTicks;
		}
		return false;
	}
}
