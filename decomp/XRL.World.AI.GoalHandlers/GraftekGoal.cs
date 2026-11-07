using System;
using System.Collections.Generic;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class GraftekGoal : GoalHandler
{
	public GameObject Target;

	[NonSerialized]
	private int LastSeen;

	public GraftekGoal(GameObject Target)
	{
		this.Target = Target;
	}

	public override void Create()
	{
		Think("I'm trying to graft " + Target.an() + "!");
	}

	public override bool CanFight()
	{
		return false;
	}

	public override bool Finished()
	{
		return false;
	}

	public override void Push(Brain pBrain)
	{
		base.Push(pBrain);
	}

	public override void TakeAction()
	{
		if (Target == null)
		{
			Think("I don't have a target anymore!");
			FailToParent();
			return;
		}
		if (Target.IsInvalid())
		{
			Think("My target has been destroyed!");
			Target = null;
			FailToParent();
			return;
		}
		Cell currentCell = Target.CurrentCell;
		Cell currentCell2 = ParentObject.CurrentCell;
		int num = ((currentCell != null && currentCell2 != null) ? currentCell.PathDistanceTo(currentCell2) : (-1));
		if (currentCell == null || currentCell.ParentZone?.ZoneID == null || num > 80)
		{
			LastSeen++;
		}
		if (Target.HasPart<GraftekGraft>())
		{
			Think("My target is already grafted...");
			Target = null;
			FailToParent();
		}
		else if (LastSeen > 5)
		{
			Think("I can't find my target...");
			Target = null;
			FailToParent();
		}
		else if (currentCell == null || currentCell.ParentZone?.ZoneID == null)
		{
			Think("My target has no location!");
			Target = null;
			FailToParent();
		}
		else if (num <= 1)
		{
			Think("I'm going to graft my target!");
			Target.Sparksplatter();
			Target.AddPart(new GraftekGraft());
		}
		else if (ParentObject.IsMobile())
		{
			Think("I'm going to move towards my target.");
			bool pathGlobal = false;
			if (Target.IsPlayer())
			{
				pathGlobal = true;
			}
			if (currentCell.ParentZone.IsWorldMap())
			{
				Think("Target's on the world map, can't follow!");
				Target = null;
				FailToParent();
				return;
			}
			FindPath findPath = new FindPath(currentCell2.ParentZone.ZoneID, currentCell2.X, currentCell2.Y, currentCell.ParentZone.ZoneID, currentCell.X, currentCell.Y, pathGlobal, PathUnlimited: false, ParentBrain.ParentObject);
			if (findPath.Usable)
			{
				using (List<string>.Enumerator enumerator = findPath.Directions.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						PushChildGoal(new Step(current));
					}
					return;
				}
			}
			FailToParent();
		}
		else
		{
			ParentBrain.ParentObject.UseEnergy(1000);
			Think("My target is too far and I'm immobile.");
			FailToParent();
		}
	}
}
