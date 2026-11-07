using System;

namespace XRL.World.Effects;

[Serializable]
public class Incommunicado : Effect
{
	public Incommunicado()
	{
		DisplayName = "incommunicado";
		Duration = 1;
	}

	public override string GetDetails()
	{
		return "Doesn't know how to rejoin leader.";
	}

	public override int GetEffectType()
	{
		return 1;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == PooledEvent<JoinPartyLeaderPossibleEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject partyLeader = Object.PartyLeader;
		if (partyLeader == null || Object.InSameZone(partyLeader))
		{
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(JoinPartyLeaderPossibleEvent E)
	{
		return E.Result = false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			return false;
		}
		if (Object.HasEffect<Incommunicado>())
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_bewilderment");
		return true;
	}

	public override void Remove(GameObject Object)
	{
	}
}
