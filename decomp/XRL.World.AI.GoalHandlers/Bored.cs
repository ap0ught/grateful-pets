using System;
using XRL.Collections;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Bored : GoalHandler
{
	public static Rack<GameObject> IdleObjects = new Rack<GameObject>();

	public static Zone IdleZone;

	public static bool IdleDirty;

	public int ExtinguishSelfTries;

	public long NextLeaderPathFind;

	public long NextLeaderAltPathFind;

	public override bool Finished()
	{
		return false;
	}

	public override bool IsBusy()
	{
		return false;
	}

	public bool TryMovementAbilities(Cell TargetCell, int Distance = -1, int StandoffDistance = 0)
	{
		if (GameObject.Validate(ParentObject) && AICommandList.HandleCommandList(AIGetMovementAbilityListEvent.GetFor(ParentObject, null, TargetCell, Distance, StandoffDistance), ParentObject, null, TargetCell))
		{
			Think("Did a movement ability with a target cell");
			return true;
		}
		return false;
	}

	public bool TryMovementAbilities()
	{
		if (GameObject.Validate(ParentObject) && AICommandList.HandleCommandList(AIGetMovementAbilityListEvent.GetFor(ParentObject), ParentObject))
		{
			Think("Did a movement ability");
			return true;
		}
		return false;
	}

	public bool TryPassiveAbilities()
	{
		if (GameObject.Validate(ParentObject) && AICommandList.HandleCommandList(AIGetPassiveAbilityListEvent.GetFor(ParentObject), ParentObject))
		{
			Think("Did a passive ability");
			return true;
		}
		return false;
	}

	public bool TryPassiveItems()
	{
		if (GameObject.Validate(ParentObject) && AICommandList.HandleCommandList(AIGetPassiveItemListEvent.GetFor(ParentObject), ParentObject))
		{
			Think("Did a passive item");
			return true;
		}
		return false;
	}

	public void TakeActionWithPartyLeader()
	{
		if (ParentObject == null || ParentBrain == null)
		{
			return;
		}
		GameObject partyLeader = ParentBrain.PartyLeader;
		Cell currentCell = ParentObject.CurrentCell;
		if (currentCell == null || currentCell.ParentZone == null || ParentBrain.GoToPartyLeader())
		{
			return;
		}
		Zone parentZone = currentCell.ParentZone;
		if (parentZone == null || !parentZone.IsActive())
		{
			return;
		}
		if (ParentBrain.Target == null && ParentBrain.CanAcquireTarget())
		{
			if (partyLeader.IsPlayerControlled())
			{
				GameObject target = partyLeader.Target;
				if (target != null && ParentBrain.CheckPerceptionOf(target))
				{
					ParentBrain.WantToKill(target, "to boredly aid my leader");
					if (ParentBrain.Target != null)
					{
						return;
					}
				}
			}
			GameObject gameObject = ParentBrain.FindProspectiveTarget(currentCell);
			if (gameObject != null)
			{
				ParentBrain.WantToKill(gameObject, "out of bored hostility while having a party leader");
				if (ParentBrain.Target != null)
				{
					return;
				}
			}
			else
			{
				Think("I boredly looked for a target while having a party leader, but didn't find one.");
			}
		}
		if (TryMovementAbilities() || TryPassiveAbilities() || TryPassiveItems())
		{
			return;
		}
		if (!ParentObject.IsMobile())
		{
			ParentObject.UseEnergy(1000);
		}
		else if (ParentBrain.Staying)
		{
			bool flag = false;
			if (!ParentBrain.Wanders && !ParentBrain.WandersRandomly)
			{
				Cell cell = ParentBrain?.StartingCell?.ResolveCell();
				if (cell != null && currentCell != cell && cell.ParentZone == parentZone && ParentObject.FireEvent("CanAIDoIndependentBehavior"))
				{
					PushChildGoal(new MoveTo(parentZone.ZoneID, cell.X, cell.Y, careful: true));
					flag = true;
				}
			}
			if (!flag)
			{
				ParentObject.UseEnergy(1000);
			}
		}
		else if (The.Game.TimeTicks > NextLeaderPathFind)
		{
			Cell currentCell2 = partyLeader.CurrentCell;
			if (currentCell2 == null || currentCell2.ParentZone == null || currentCell2.OnWorldMap())
			{
				return;
			}
			int num = GetPartyLeaderFollowDistanceEvent.GetFor(ParentObject, partyLeader);
			int num2 = ParentObject.DistanceTo(currentCell2);
			if (num2 <= num || TryMovementAbilities(currentCell2, num2, num))
			{
				return;
			}
			FindPath findPath = new FindPath(currentCell, currentCell2, PathGlobal: false, PathUnlimited: true, ParentObject, 95);
			if (findPath.Usable)
			{
				NextLeaderPathFind = The.Game.TimeTicks;
				PathTowardLeader(findPath, num);
				return;
			}
			if (ParentObject.IsPlayerLed())
			{
				NextLeaderPathFind = The.Game.TimeTicks + 2;
			}
			else
			{
				NextLeaderPathFind = The.Game.TimeTicks + Stat.Random(50, 100);
			}
			if (The.Game.TimeTicks <= NextLeaderAltPathFind)
			{
				return;
			}
			Cell closestPassableCellFor = currentCell2.getClosestPassableCellFor(ParentObject);
			if (closestPassableCellFor != null && closestPassableCellFor != currentCell2)
			{
				FindPath findPath2 = new FindPath(currentCell, closestPassableCellFor, PathGlobal: false, PathUnlimited: true, ParentObject, 95);
				if (findPath2.Usable)
				{
					NextLeaderPathFind = The.Game.TimeTicks + 2;
					PathTowardLeader(findPath2, num);
				}
				else if (ParentObject.IsPlayerLed())
				{
					NextLeaderAltPathFind = The.Game.TimeTicks + 10;
				}
				else
				{
					NextLeaderAltPathFind = The.Game.TimeTicks + Stat.Random(100, 200);
				}
			}
		}
		else
		{
			Think("Waiting for party leader");
			ParentObject.UseEnergy(1000);
		}
	}

	private void PathTowardLeader(FindPath PathFinder, int Distance = 1)
	{
		for (int num = PathFinder.Steps.Count - Distance - 1; num >= 1; num--)
		{
			if (PathFinder.Steps[num].IsEmpty())
			{
				if (!TryMovementAbilities(PathFinder.Steps[num]))
				{
					break;
				}
				return;
			}
		}
		for (int num2 = Math.Min(Stat.Random(0, 4), PathFinder.Directions.Count - Distance - 1); num2 >= 0; num2--)
		{
			PushChildGoal(new Step(PathFinder.Directions[num2]));
		}
	}

	public override void TakeAction()
	{
		Cell cell = ParentObject?.CurrentCell;
		if (cell == null || The.Player?.CurrentCell == null)
		{
			return;
		}
		if (ParentObject.IsAflame() && ++ExtinguishSelfTries < 5)
		{
			PushChildGoal(new ExtinguishSelf());
			return;
		}
		Think("I'm bored.");
		if (!AIBoredEvent.Check(ParentObject) || ParentObject.Energy.Value < 1000)
		{
			return;
		}
		if (ParentBrain.PartyLeader != null)
		{
			TakeActionWithPartyLeader();
			return;
		}
		if (ParentBrain.Target == null)
		{
			GameObject gameObject = ParentBrain.FindProspectiveTarget(cell);
			if (gameObject != null)
			{
				ParentBrain.WantToKill(gameObject, "out of bored hostility");
				if (ParentBrain.Target != null)
				{
					return;
				}
			}
			else
			{
				Think("I boredly looked for a target, but didn't find one.");
			}
		}
		if (TryMovementAbilities() || TryPassiveAbilities() || TryPassiveItems() || ParentObject == null)
		{
			return;
		}
		Cell cell2 = ParentBrain?.StartingCell?.ResolveCell();
		if (cell2 != null && !ParentBrain.Wanders && !ParentBrain.WandersRandomly && cell != cell2 && ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			PushChildGoal(new MoveTo(cell2.ParentZone.ZoneID, cell2.X, cell2.Y, careful: true));
		}
		else if (!ParentObject.InSameZone(The.Player))
		{
			PushChildGoal(new Wait(Stat.Random(1, 20), "I'm not in the same zone as the player"));
		}
		else if (ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			if (ParentBrain.Wanders)
			{
				if ((ParentObject.HasTagOrProperty("Restless") && !ParentObject.HasTagOrProperty("Social")) || 10.in100())
				{
					PushChildGoal(new Wander());
					return;
				}
			}
			else if (ParentBrain.WandersRandomly && ((ParentObject.HasTagOrProperty("Restless") && !ParentObject.HasTagOrProperty("Social")) || 20.in100()))
			{
				PushChildGoal(new WanderRandomly(5));
				return;
			}
			if (ParentObject.HasTagOrProperty("AllowIdleBehavior") && !ParentObject.HasTagOrProperty("PreventIdleBehavior"))
			{
				Rack<GameObject> idleObjects = IdleObjects;
				Zone parentZone = cell.ParentZone;
				if (IdleDirty || IdleZone != parentZone)
				{
					idleObjects.Clear();
					Zone.ObjectEnumerator enumerator = parentZone.IterateObjects().GetEnumerator();
					while (enumerator.MoveNext())
					{
						GameObject current = enumerator.Current;
						if (current.WantEvent(PooledEvent<IdleQueryEvent>.ID, MinEvent.CascadeLevel))
						{
							IdleObjects.Add(current);
						}
					}
					IdleDirty = false;
				}
				if (!idleObjects.IsNullOrEmpty())
				{
					idleObjects.ShuffleInPlace();
					IdleQueryEvent e = IdleQueryEvent.FromPool(ParentBrain.ParentObject);
					for (int num = idleObjects.Count - 1; num >= 0; num--)
					{
						GameObject gameObject2 = idleObjects[num];
						if (!gameObject2.IsValid())
						{
							idleObjects.RemoveAt(num);
						}
						else if (!gameObject2.HandleEvent(e))
						{
							ParentObject.UseEnergy(1000);
							return;
						}
					}
				}
			}
			PushChildGoal(new Wait(Stat.Random(1, 10), "I couldn't find anything to do"));
		}
		else
		{
			PushChildGoal(new Wait(Stat.Random(1, 10), "I couldn't perform independent behavior"));
		}
	}
}
