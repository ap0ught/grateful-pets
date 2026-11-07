using System;
using System.Linq;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class BoneWorm : IPart
{
	public string Duration = "1d3";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID)
		{
			return ID == PooledEvent<BeforeAITakingActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (!CheckTarget())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeAITakingActionEvent E)
	{
		CheckTarget();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AIBeginKill");
		base.Register(Object, Registrar);
	}

	public void enclose()
	{
		int r;
		for (r = 2; r < 4; r++)
		{
			Cell cell = (from c in ParentObject.Brain.Target.CurrentCell.GetAdjacentCells(r)
				where c.DistanceTo(ParentObject.Brain.Target) == r
				where c.IsPassable()
				orderby c.DistanceTo(ParentObject)
				select c).FirstOrDefault();
			if (cell != null)
			{
				ParentObject.Brain.PushGoal(new MoveTo(cell));
				break;
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIBeginKill" && ParentObject.HasEffect<Burrowed>() && ParentObject.Target != null)
		{
			enclose();
			return false;
		}
		return base.FireEvent(E);
	}

	public bool CheckTarget()
	{
		if (ParentObject.HasEffect<Burrowed>())
		{
			GameObject target = ParentObject.Target;
			if (target != null)
			{
				if (ParentObject.Brain.Goals.Count == 0 || (ParentObject.Brain.Goals.Peek().GetType() != typeof(MoveTo) && ParentObject.Brain.Goals.Peek().GetType() != typeof(Step)))
				{
					ParentObject.Brain.Goals.Clear();
					ParentObject.Target = target;
					enclose();
				}
				return false;
			}
		}
		return true;
	}
}
