using System;

namespace XRL.World.Parts;

[Serializable]
public class AIHealingTonicUse : AIBehaviorPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveItemListEvent.ID)
		{
			return ID == AIGetPassiveItemListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		AIUseHealingTonic(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetPassiveItemListEvent E)
	{
		AIUseHealingTonic(E);
		return base.HandleEvent(E);
	}

	public void AIUseHealingTonic(IAICommandListEvent E)
	{
		if (!ParentObject.IsBroken() && !ParentObject.IsRusted() && !ParentObject.IsImportant() && E.Actor.FireEvent("CanApplyTonic") && E.Actor.GetTonicEffectCount() < E.Actor.GetTonicCapacity() && GetUtilityScoreEvent.GetFor(E.Actor, ParentObject) > 0)
		{
			E.Add("Apply", 100, ParentObject, Inv: true, Self: true);
		}
	}
}
