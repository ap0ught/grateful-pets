using System;
using XRL.Core;

namespace QupKit;

public class ThreadTaskQueueEntry
{
	public int Delay;

	public string TaskID;

	private Action actionToTake;

	public ThreadTaskQueueEntry(Action a)
	{
		actionToTake = a;
	}

	public void SetAction(Action a)
	{
		actionToTake = a;
	}

	public void Clear()
	{
		actionToTake = null;
		TaskID = null;
	}

	public void Execute(ThreadTaskQueue parentQueue)
	{
		if (actionToTake != null)
		{
			try
			{
				actionToTake();
			}
			catch (Exception ex)
			{
				XRLCore.LogError(ex);
			}
		}
		Clear();
		parentQueue.poolEntry(this);
	}
}
