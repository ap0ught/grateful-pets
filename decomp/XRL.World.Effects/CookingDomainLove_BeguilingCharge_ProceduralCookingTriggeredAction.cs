using System;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainLove_BeguilingCharge_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public int Tier;

	public int AppliedBonus;

	public string TargetID;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(7, 8);
		TargetID = target?.ID;
		AppliedBonus = 0;
		base.Init(target);
	}

	public override string GetDescription()
	{
		return "@they beguile a creature as per Beguiling at rank 7-8 for the duration of this effect.";
	}

	public override string GetNotification()
	{
		return "@they feel the swell of love inside.";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetCompanionLimitEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCompanionLimitEvent E)
	{
		if (E.Means == "Beguiling")
		{
			GameObject actor = E.Actor;
			if (actor != null && actor.IDMatch(TargetID))
			{
				E.Limit += AppliedBonus;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer())
		{
			AppliedBonus++;
			if (!Beguiling.Cast(go, null, null, Tier))
			{
				AppliedBonus--;
			}
		}
	}

	public override void Remove(GameObject go)
	{
		AppliedBonus = 0;
		Beguiling.SyncTarget(go);
		base.Remove(go);
	}
}
