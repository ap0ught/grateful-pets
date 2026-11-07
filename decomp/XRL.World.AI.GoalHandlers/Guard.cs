using System;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Guard : GoalHandler
{
	public override void Create()
	{
		Think("I'm guarding this place.");
	}

	public override bool Finished()
	{
		return false;
	}

	public override void TakeAction()
	{
		Cell currentCell = ParentObject.CurrentCell;
		if (currentCell == null || The.Player?.CurrentCell == null)
		{
			return;
		}
		Think("I'm guarding this place.");
		if (ParentBrain.PartyLeader != null)
		{
			if (ParentBrain.GoToPartyLeader())
			{
				return;
			}
			if (ParentBrain.Target == null && currentCell.ParentZone.IsActive())
			{
				GameObject gameObject = ParentBrain.FindProspectiveTarget(currentCell);
				if (gameObject != null)
				{
					ParentBrain.WantToKill(gameObject, "to guard this place for my leader");
					if (ParentBrain.Target == null)
					{
						return;
					}
				}
			}
			if (ParentObject.IsMobile())
			{
				if (ParentBrain.Staying)
				{
					if (!ParentBrain.Wanders && !ParentBrain.WandersRandomly && ParentBrain.StartingCell != null && !ParentBrain.StartingCell.Equals(ParentObject.CurrentCell))
					{
						PushChildGoal(new MoveTo(ParentBrain.StartingCell, careful: true));
					}
					else
					{
						ParentBrain.ParentObject.UseEnergy(1000);
					}
				}
				else
				{
					int num = 5;
					if (ParentBrain.PartyLeader.IsPlayer())
					{
						num = 1;
					}
					if (ParentBrain.ParentObject.DistanceTo(ParentBrain.PartyLeader) > num)
					{
						Cell currentCell2 = ParentObject.CurrentCell;
						Cell currentCell3 = ParentBrain.PartyLeader.CurrentCell;
						FindPath findPath = new FindPath(currentCell2, currentCell3, PathGlobal: false, PathUnlimited: true, ParentBrain.ParentObject, 95);
						if (findPath.Usable)
						{
							int num2 = Stat.Random(1, 5);
							foreach (string direction in findPath.Directions)
							{
								PushChildGoal(new Step(direction));
								num2--;
								if (num2 <= 0)
								{
									return;
								}
							}
						}
					}
				}
			}
			ParentBrain.ParentObject.UseEnergy(1000);
			return;
		}
		if (currentCell.ParentZone.IsActive() && ParentBrain.Target == null)
		{
			GameObject gameObject2 = ParentBrain.FindProspectiveTarget(currentCell);
			if (gameObject2 != null)
			{
				ParentBrain.WantToKill(gameObject2, "to guard this place");
				if (ParentBrain.Target != null)
				{
					return;
				}
			}
		}
		ParentBrain.ParentObject.UseEnergy(1000);
	}
}
