using System;
using Genkit;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIWiring : AIBehaviorPart
{
	public string Blueprint = "Wire Strand";

	public int Radius = 5;

	[NonSerialized]
	private Pathfinder Pathfinder;

	[NonSerialized]
	private DelegateGoal Goal;

	[NonSerialized]
	private int Items = -1;

	[NonSerialized]
	private int Step;

	[NonSerialized]
	private int Wait;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID && ID != TookEvent.ID && ID != DroppedEvent.ID)
		{
			return ID == PooledEvent<IsConversationallyResponsiveEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (Wait-- <= 0)
		{
			Wait = 10;
			try
			{
				if (QueueAction())
				{
					return false;
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("AIWiring", x);
			}
		}
		return base.HandleEvent(E);
	}

	public bool QueueAction()
	{
		if (Items == -1)
		{
			Items = ParentObject.Inventory.Count(Blueprint);
		}
		if (Items == 0 || !ParentObject.IsMobile())
		{
			return false;
		}
		if (Goal != null && ParentObject.Brain.Goals.Contains(Goal))
		{
			return false;
		}
		ElectricalPowerTransmission electricalPowerTransmission = null;
		ElectricalPowerTransmission electricalPowerTransmission2 = null;
		foreach (Cell item in ParentObject.CurrentCell.IterateAdjacent(Radius, IncludeSelf: false, LocalOnly: true))
		{
			foreach (GameObject @object in item.Objects)
			{
				if (!@object.TryGetPart<ElectricalPowerTransmission>(out var Part))
				{
					continue;
				}
				if (Part.IsConsumer && !Part.AnyCharge(0L))
				{
					electricalPowerTransmission = Part;
					if (electricalPowerTransmission2 != null)
					{
						return FindPath(electricalPowerTransmission, electricalPowerTransmission2);
					}
				}
				else if (Part.IsProducer && Part.AnyCharge(0L))
				{
					electricalPowerTransmission2 = Part;
					if (electricalPowerTransmission != null)
					{
						return FindPath(electricalPowerTransmission, electricalPowerTransmission2);
					}
				}
			}
		}
		return false;
	}

	public bool FindPath(ElectricalPowerTransmission Consumer, ElectricalPowerTransmission Producer)
	{
		if (Consumer == null || Producer == null)
		{
			return false;
		}
		long gridBit = Consumer.GetGridBit();
		if (gridBit != 0L && Producer.GetGridBit() == gridBit)
		{
			return false;
		}
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		int num = int.MaxValue;
		foreach (GameObject item in Consumer.GetGrid())
		{
			foreach (GameObject item2 in Producer.GetGrid())
			{
				int num2 = item.DistanceTo(item2);
				if (num2 < num)
				{
					gameObject = item;
					gameObject2 = item2;
					num = num2;
				}
			}
		}
		if (gameObject == null || gameObject2 == null)
		{
			return false;
		}
		if (Pathfinder == null)
		{
			Pathfinder = new Pathfinder(80, 25);
		}
		if (Pathfinder.FindPath(gameObject.CurrentCell.Location, gameObject2.CurrentCell.Location, Display: false, CardinalDirectionsOnly: true, 50))
		{
			Step = 0;
			Pathfinder.Directions.Reverse();
			Pathfinder.Steps.Reverse();
			if (Goal == null)
			{
				Goal = new DelegateGoal(TakeAction, null, Failed);
			}
			ParentObject.Brain.PushGoal(Goal);
			return true;
		}
		return false;
	}

	public void TakeAction(GoalHandler Handler)
	{
		if (!ParentObject.IsMobile())
		{
			Handler.FailToParent();
			return;
		}
		int count = Pathfinder.Steps.Count;
		if (Step >= count)
		{
			Handler.Pop();
			return;
		}
		Cell cell = ParentObject.CurrentZone.GetCell(Pathfinder.Steps[Step].pos);
		if (cell.HasObject((GameObject x) => x.HasPart<ElectricalPowerTransmission>()))
		{
			Step++;
			return;
		}
		if (ParentObject.DistanceTo(cell) > 1)
		{
			Handler.PushChildGoal(new MoveTo(cell, careful: false, overridesCombat: false, 1));
			return;
		}
		GameObject gameObject = ParentObject.Inventory.FindObjectByBlueprint(Blueprint);
		if (gameObject == null || !gameObject.TryGetPart<DeployableInfrastructure>(out var Part) || !Part.DeployOne(ParentObject, cell, Message: true, Sound: true))
		{
			Handler.FailToParent();
			return;
		}
		Step++;
		ParentObject.ForfeitTurn();
	}

	public void Failed(GoalHandler Handler)
	{
		Handler.FailToParent();
	}

	public override bool HandleEvent(TookEvent E)
	{
		Items = -1;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		Items = -1;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (Goal != null && E.Speaker == ParentObject && E.Speaker.Brain.Goals.Contains(Goal))
		{
			E.Message = E.Speaker.Does("ignore") + " " + E.Actor.t() + ".";
			return false;
		}
		return base.HandleEvent(E);
	}
}
