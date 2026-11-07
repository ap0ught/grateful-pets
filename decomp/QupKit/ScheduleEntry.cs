using System;

namespace QupKit;

public class ScheduleEntry
{
	public float ScheduledTime;

	public float Time;

	public Action OnExecute;

	public ScheduleEntry(float T, Action A, float ScheduledTime)
	{
		Time = T;
		OnExecute = A;
	}

	public virtual void Execute()
	{
		if (OnExecute != null)
		{
			OnExecute();
			OnExecute = null;
		}
	}
}
