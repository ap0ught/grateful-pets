using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Wait : GoalHandler
{
	public int TicksLeft;

	public string Reason;

	public Wait(int Duration)
	{
		TicksLeft = Duration;
	}

	public Wait(int Duration, string Reason)
		: this(Duration)
	{
		this.Reason = Reason;
	}

	public override void Create()
	{
		if (!string.IsNullOrEmpty(Reason))
		{
			Think("I'll wait " + TicksLeft + " ticks because " + Reason + ".");
		}
		else
		{
			Think("I'll wait " + TicksLeft + " ticks.");
		}
	}

	public override bool IsBusy()
	{
		return false;
	}

	public override bool Finished()
	{
		return TicksLeft <= 0;
	}

	public override void TakeAction()
	{
		ParentObject.UseEnergy(1000);
		TicksLeft--;
		if (TicksLeft <= 0)
		{
			Pop();
		}
	}
}
