using System;
using Genkit;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class TombPatrolBehavior : IPart
{
	public Point2D GetNextWaypoint()
	{
		return new Point2D(0, 0);
	}

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
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return base.HandleEvent(E);
		}
		if (ParentObject.IsPlayerControlled())
		{
			return base.HandleEvent(E);
		}
		Zone parentZone = ParentObject.Physics.CurrentCell.ParentZone;
		if (parentZone != null)
		{
			int wX = parentZone.wX;
			int wY = parentZone.wY;
			int z = parentZone.Z;
			int num = parentZone.X;
			int num2 = parentZone.Y;
			if (num == 0 && num2 == 0)
			{
				num = 1;
			}
			else if (num == 1 && num2 == 0)
			{
				num = 2;
			}
			else if (num == 2 && num2 == 0)
			{
				num2 = 1;
			}
			else if (num == 2 && num2 == 1)
			{
				num2 = 2;
			}
			else if (num == 2 && num2 == 2)
			{
				num = 1;
			}
			else if (num == 1 && num2 == 2)
			{
				num = 0;
			}
			else if (num == 0 && num2 == 2)
			{
				num2 = 1;
			}
			else if (num == 0 && num2 == 1)
			{
				num2 = 0;
			}
			ParentObject.Brain.PushGoal(new TombPatrolGoal("JoppaWorld." + wX + "." + wY + "." + num + "." + num2 + "." + z));
		}
		return false;
	}

	public override bool HandleEvent(GetFeelingEvent E)
	{
		if (E.TargetLeader == null)
		{
			if (E.Target.HasMarkOfDeath())
			{
				E.Feeling = 0;
				return false;
			}
			if (E.Target.Blueprint != ParentObject.Blueprint)
			{
				E.Feeling = -100;
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(PooledEvent<GetFeelingEvent>.ID);
	}
}
