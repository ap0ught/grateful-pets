using System;
using System.Collections.Generic;
using System.Text;
using XRL.Messages;
using XRL.World.AI.GoalHandlers;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;

namespace XRL.World.AI;

[Serializable]
public class GoalHandler
{
	public int Age;

	public Brain ParentBrain;

	public GoalHandler ParentHandler;

	[NonSerialized]
	private static StringBuilder DescBuilder = new StringBuilder();

	public GameObject ParentObject => ParentBrain?.ParentObject;

	public Zone CurrentZone => ParentObject?.CurrentZone;

	public Cell CurrentCell => ParentObject?.CurrentCell;

	public virtual bool CanFight()
	{
		return true;
	}

	public virtual bool IsBusy()
	{
		return true;
	}

	public virtual bool IsNonAggressive()
	{
		return false;
	}

	public virtual bool IsFleeing()
	{
		return false;
	}

	public virtual void Failed()
	{
	}

	public virtual string GetDetails()
	{
		return null;
	}

	public virtual string GetDescription()
	{
		DescBuilder.Length = 0;
		DescBuilder.Append(GetType().Name);
		string details = GetDetails();
		if (!string.IsNullOrEmpty(details))
		{
			DescBuilder.Append(": ").Append(details);
		}
		return DescBuilder.ToString();
	}

	public void FailToParent()
	{
		while (ParentBrain.Goals.Count > 0 && ParentBrain.Goals.Peek() != ParentHandler)
		{
			ParentBrain.Goals.Pop();
		}
		if (ParentBrain.Goals.Count > 0)
		{
			ParentBrain.Goals.Peek().Failed();
		}
	}

	public virtual void PushGoal(GoalHandler Goal)
	{
		Goal.Push(ParentBrain);
	}

	public virtual void ForcePushGoal(GoalHandler Goal)
	{
		Goal.ParentBrain = ParentBrain;
		ParentBrain.Goals.Push(this);
		Goal.Create();
	}

	public virtual void PushChildGoal(GoalHandler Child)
	{
		Child.ParentHandler = this;
		Child.Push(ParentBrain);
	}

	public void PushChildGoal(GoalHandler Child, GoalHandler Parent)
	{
		Child.ParentHandler = Parent;
		Child.Push(ParentBrain);
	}

	public virtual void Push(Brain Brain)
	{
		ParentBrain = Brain;
		Brain.Goals.Push(this);
		Create();
	}

	public virtual void InsertGoalAfter(GoalHandler Goal)
	{
		Goal.InsertAfter(this, ParentBrain);
	}

	public virtual void InsertGoalAfter(GoalHandler After, GoalHandler Goal)
	{
		Goal.InsertAfter(After, ParentBrain);
	}

	public virtual void ForceInsertGoalAfter(GoalHandler After, GoalHandler Goal)
	{
		Goal.ParentBrain = ParentBrain;
		ParentBrain.Goals.InsertUnder(After, Goal);
		Goal.Create();
	}

	public virtual void InsertChildGoalAfter(GoalHandler After, GoalHandler Child)
	{
		Child.ParentHandler = this;
		Child.InsertAfter(After, ParentBrain);
	}

	public virtual void InsertChildGoalAfter(GoalHandler After, GoalHandler Child, GoalHandler Parent)
	{
		Child.ParentHandler = Parent;
		Child.InsertAfter(After, ParentBrain);
	}

	public virtual void InsertAfter(GoalHandler After, Brain Brain)
	{
		ParentBrain = Brain;
		Brain.Goals.InsertUnder(After, this);
		Create();
	}

	public virtual void InsertGoalAfter(string After, GoalHandler Goal)
	{
		Goal.InsertAfter(After, ParentBrain);
	}

	public virtual void ForceInsertGoalAfter(string After, GoalHandler Goal)
	{
		Goal.ParentBrain = ParentBrain;
		ParentBrain.Goals.InsertUnder(ParentBrain.FindGoal(After), Goal);
		Goal.Create();
	}

	public virtual void InsertChildGoalAfter(string After, GoalHandler Child)
	{
		Child.ParentHandler = this;
		Child.InsertAfter(After, ParentBrain);
	}

	public virtual void InsertChildGoalAfter(string After, GoalHandler Child, GoalHandler Parent)
	{
		Child.ParentHandler = Parent;
		Child.InsertAfter(After, ParentBrain);
	}

	public virtual void InsertAfter(string After, Brain Brain)
	{
		ParentBrain = Brain;
		Brain.Goals.InsertUnder(ParentBrain.FindGoal(After), this);
		Create();
	}

	public virtual void InsertGoalAsParent(GoalHandler Goal)
	{
		ParentHandler = Goal;
		Goal.InsertAsParent(this, ParentBrain);
	}

	public virtual void InsertGoalAsParent(GoalHandler BecomesChild, GoalHandler Goal)
	{
		Goal.InsertAsParent(BecomesChild, ParentBrain);
	}

	public virtual void ForceInsertGoalAsParent(GoalHandler BecomesChild, GoalHandler Goal)
	{
		BecomesChild.ParentHandler = Goal;
		Goal.ParentBrain = ParentBrain;
		ParentBrain.Goals.InsertUnder(BecomesChild, Goal);
		Goal.Create();
	}

	public virtual void InsertAsParent(GoalHandler BecomesChild, Brain Brain)
	{
		BecomesChild.ParentHandler = this;
		ParentBrain = Brain;
		Brain.Goals.InsertUnder(BecomesChild, this);
		Create();
	}

	public virtual void InsertGoalAsParent(string BecomesChild, GoalHandler Goal)
	{
		Goal.InsertAsParent(BecomesChild, ParentBrain);
	}

	public virtual void ForceInsertGoalAsParent(string BecomesChild, GoalHandler Goal)
	{
		GoalHandler goalHandler = ParentBrain.FindGoal(BecomesChild);
		if (goalHandler != null)
		{
			goalHandler.ParentHandler = Goal;
		}
		Goal.ParentBrain = ParentBrain;
		ParentBrain.Goals.InsertUnder(goalHandler, Goal);
		Goal.Create();
	}

	public virtual void InsertAsParent(string BecomesChild, Brain Brain)
	{
		GoalHandler goalHandler = ParentBrain.FindGoal(BecomesChild);
		if (goalHandler != null)
		{
			goalHandler.ParentHandler = this;
		}
		ParentBrain = Brain;
		Brain.Goals.InsertUnder(goalHandler, this);
		Create();
	}

	public void Pop()
	{
		if (ParentBrain.Goals.Count > 0)
		{
			ParentBrain.Goals.Pop();
		}
	}

	public virtual void Create()
	{
	}

	public virtual void TakeAction()
	{
	}

	public virtual bool Finished()
	{
		return true;
	}

	public void Think(string Hrm)
	{
		ParentBrain.Think(Hrm);
	}

	public bool MoveTowards(Cell targetCell, bool Global = false, bool MoveAwayIfAt = true)
	{
		Think("I'm going to move towards my target.");
		if (targetCell.ParentZone.IsWorldMap())
		{
			Think("Target's on the world map, can't follow!");
			return false;
		}
		if (MoveAwayIfAt && targetCell == ParentObject.CurrentCell)
		{
			Cell randomLocalAdjacentCell = ParentObject.CurrentCell.GetRandomLocalAdjacentCell();
			if (randomLocalAdjacentCell != null)
			{
				PushChildGoal(new Step(ParentObject.CurrentCell.GetDirectionFromCell(randomLocalAdjacentCell)));
				return true;
			}
		}
		FindPath findPath = new FindPath(ParentObject.CurrentZone.ZoneID, ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, targetCell.ParentZone.ZoneID, targetCell.X, targetCell.Y, Global, PathUnlimited: false, ParentObject);
		if (findPath.Usable)
		{
			using (List<string>.Enumerator enumerator = findPath.Directions.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					PushChildGoal(new Step(current));
					return true;
				}
			}
			return true;
		}
		return false;
	}

	public static void AddPlayerMessage(string Message, string Color = null, bool Capitalize = true)
	{
		MessageQueue.AddPlayerMessage(Message, Color, Capitalize);
	}

	public static void AddPlayerMessage(string Message, char Color, bool Capitalize = true)
	{
		MessageQueue.AddPlayerMessage(Message, Color, Capitalize);
	}

	public static bool Visible(GameObject obj)
	{
		return obj?.IsVisible() ?? false;
	}

	public bool Visible()
	{
		return Visible(ParentObject);
	}
}
