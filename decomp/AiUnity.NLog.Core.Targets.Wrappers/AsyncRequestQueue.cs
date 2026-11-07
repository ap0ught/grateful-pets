using System.Collections.Generic;
using System.Threading;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Preserve]
internal class AsyncRequestQueue
{
	private readonly Queue<AsyncLogEventInfo> logEventInfoQueue = new Queue<AsyncLogEventInfo>();

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public int RequestLimit { get; set; }

	public AsyncTargetWrapperOverflowAction OnOverflow { get; set; }

	public int RequestCount => logEventInfoQueue.Count;

	public AsyncRequestQueue(int requestLimit, AsyncTargetWrapperOverflowAction overflowAction)
	{
		RequestLimit = requestLimit;
		OnOverflow = overflowAction;
	}

	public void Enqueue(AsyncLogEventInfo logEventInfo)
	{
		lock (this)
		{
			if (logEventInfoQueue.Count >= RequestLimit)
			{
				Logger.Debug("Async queue is full");
				switch (OnOverflow)
				{
				case AsyncTargetWrapperOverflowAction.Discard:
					Logger.Debug("Discarding one element from queue");
					logEventInfoQueue.Dequeue();
					break;
				case AsyncTargetWrapperOverflowAction.Grow:
					Logger.Debug("The overflow action is Grow, adding element anyway");
					break;
				case AsyncTargetWrapperOverflowAction.Block:
					while (logEventInfoQueue.Count >= RequestLimit)
					{
						Logger.Debug("Blocking because the overflow action is Block...");
						Monitor.Wait(this);
						Logger.Trace("Entered critical section.");
					}
					Logger.Trace("Limit ok.");
					break;
				}
			}
			logEventInfoQueue.Enqueue(logEventInfo);
		}
	}

	public AsyncLogEventInfo[] DequeueBatch(int count)
	{
		List<AsyncLogEventInfo> list = new List<AsyncLogEventInfo>();
		lock (this)
		{
			for (int i = 0; i < count; i++)
			{
				if (logEventInfoQueue.Count <= 0)
				{
					break;
				}
				list.Add(logEventInfoQueue.Dequeue());
			}
			if (OnOverflow == AsyncTargetWrapperOverflowAction.Block)
			{
				Monitor.PulseAll(this);
			}
		}
		return list.ToArray();
	}

	public void Clear()
	{
		lock (this)
		{
			logEventInfoQueue.Clear();
		}
	}
}
