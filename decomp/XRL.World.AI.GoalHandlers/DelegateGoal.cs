using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class DelegateGoal : GoalHandler
{
	[NonSerialized]
	public Action<GoalHandler> OnAction;

	[NonSerialized]
	public Func<GoalHandler, bool> OnFinished;

	[NonSerialized]
	public Action<GoalHandler> OnFailed;

	[NonSerialized]
	public bool? SetCanFight;

	[NonSerialized]
	public bool? SetNonAggressive;

	public DelegateGoal()
	{
	}

	public DelegateGoal(Action<GoalHandler> Action, Func<GoalHandler, bool> Finished = null, Action<GoalHandler> Failed = null)
	{
		OnAction = Action;
		OnFinished = Finished;
		OnFailed = Failed;
	}

	public override bool Finished()
	{
		if (OnAction == null && OnFinished == null)
		{
			return true;
		}
		if (OnFinished != null)
		{
			return OnFinished(this);
		}
		return false;
	}

	public override void TakeAction()
	{
		if (OnAction != null)
		{
			OnAction(this);
		}
	}

	public override void Failed()
	{
		if (OnFailed != null)
		{
			OnFailed(this);
		}
	}

	public override bool CanFight()
	{
		return SetCanFight ?? base.CanFight();
	}

	public override bool IsNonAggressive()
	{
		return SetNonAggressive ?? base.IsNonAggressive();
	}
}
