using System;

namespace XRL.World.Parts;

[Serializable]
public class AITonicUse : AIBehaviorPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetDefensiveItemListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		AIUseTonic(E);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void AIUseTonic(IAICommandListEvent E)
	{
		if (!ParentObject.IsBroken() && !ParentObject.IsRusted() && !ParentObject.IsImportant() && E.Actor.FireEvent("CanApplyTonic") && E.Actor.GetTonicEffectCount() < E.Actor.GetTonicCapacity())
		{
			int num = GetUtilityScoreEvent.GetFor(E.Actor, ParentObject);
			if (num > 0)
			{
				E.Add("Apply", Math.Max(num / 10, 1), ParentObject, Inv: true, Self: true);
			}
		}
	}
}
