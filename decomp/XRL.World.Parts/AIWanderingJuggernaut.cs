using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIWanderingJuggernaut : AIBehaviorPart
{
	public int Angle;

	public bool Active = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeforeBeginTakeActionEvent>.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		if (Active)
		{
			WanderJuggernautStyle();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		Angle = Stat.Random(0, 359);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("VillageInit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit")
		{
			Active = false;
		}
		return base.FireEvent(E);
	}

	private void WanderJuggernautStyle()
	{
		if (ParentObject.IsPlayer() || ParentObject.Brain == null || ParentObject.PartyLeader != null || !ParentObject.FireEvent("CanAIDoIndependentBehavior") || ParentObject.IsFleeing())
		{
			return;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return;
		}
		Cell cell2 = ParentObject.MovingTo();
		GameObject target = ParentObject.Target;
		if (target != null)
		{
			if (!ParentObject.HasPart<Consumer>())
			{
				return;
			}
			Cell cell3 = target.CurrentCell;
			if (cell3 == null)
			{
				return;
			}
			if (cell == cell3)
			{
				ParentObject.Brain.PushGoal(new FleeLocation(cell, 1));
			}
			else if (cell2 != cell3 && cell.IsAdjacentTo(cell3))
			{
				string directionFromCell = cell.GetDirectionFromCell(cell3);
				if (directionFromCell != null)
				{
					ParentObject.Target = null;
					ParentObject.Target = target;
					ParentObject.Brain.PushGoal(new Step(directionFromCell, careful: false, overridesCombat: true, wandering: false, juggernaut: true, target));
				}
			}
		}
		else if (cell2 == null)
		{
			List<string> list = Directions.DirectionsFromAngle(Angle, 64);
			int num = 0;
			while (cell.GetCellFromDirection(list[0], BuiltOnly: false) == null && ++num < 100)
			{
				Angle = Stat.Random(0, 359);
				list = Directions.DirectionsFromAngle(Angle, 64);
			}
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				ParentObject?.Brain?.PushGoal(new Step(list[num2], careful: false, overridesCombat: false, wandering: true, juggernaut: true, null, allowUnbuilt: true));
			}
		}
	}
}
