using System;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIShopper : AIBehaviorPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<AIBoredEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (ShouldGoShopping() && 25.in100())
		{
			ParentObject.Brain.PushGoal(new GoOnAShoppingSpree());
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool ShouldGoShopping()
	{
		if (ParentObject.PartyLeader != null)
		{
			return false;
		}
		if (ParentObject.HasProperName && (ParentObject.CurrentZone == null || !GoOnAShoppingSpree.TargetZones.Contains(ParentObject.CurrentZone.ZoneID)))
		{
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		return true;
	}
}
