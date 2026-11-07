using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using AiUnity.Common.Attributes;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Internal;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Target("AsyncWrapper", IsWrapper = true)]
[Preserve]
public class AsyncTargetWrapper : WrapperTargetBase
{
	private readonly object lockObject = new object();

	private Timer lazyWriterTimer;

	private readonly Queue<AsyncContinuation> flushAllContinuations = new Queue<AsyncContinuation>();

	private readonly object continuationQueueLock = new object();

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	[DefaultValue(100)]
	[Display("Batch size", "Number of log events to process in a batch.", false, 0)]
	public int BatchSize { get; set; }

	[DefaultValue(50)]
	[Display("Batch timeout", "Process log events in batch after timeout interval.", false, 0)]
	public int TimeToSleepBetweenBatches { get; set; }

	[DefaultValue("Discard")]
	[Display("Overflow action", "Action to take if lazy writer queue limit is exceeded.", false, 0)]
	public AsyncTargetWrapperOverflowAction OverflowAction
	{
		get
		{
			return RequestQueue.OnOverflow;
		}
		set
		{
			RequestQueue.OnOverflow = value;
		}
	}

	[DefaultValue(10000)]
	[Display("Queue limit", "The maximum limit of the lazy writer thread request queue.", false, 0)]
	public int QueueLimit
	{
		get
		{
			return RequestQueue.RequestLimit;
		}
		set
		{
			RequestQueue.RequestLimit = value;
		}
	}

	internal AsyncRequestQueue RequestQueue { get; private set; }

	public AsyncTargetWrapper()
		: this(null)
	{
	}

	public AsyncTargetWrapper(Target wrappedTarget)
		: this(wrappedTarget, 10000, AsyncTargetWrapperOverflowAction.Discard)
	{
	}

	public AsyncTargetWrapper(Target wrappedTarget, int queueLimit, AsyncTargetWrapperOverflowAction overflowAction)
	{
		RequestQueue = new AsyncRequestQueue(10000, AsyncTargetWrapperOverflowAction.Discard);
		TimeToSleepBetweenBatches = 50;
		BatchSize = 100;
		WrappedTarget = wrappedTarget;
		QueueLimit = queueLimit;
		OverflowAction = overflowAction;
	}

	protected override void FlushAsync(AsyncContinuation asyncContinuation)
	{
		lock (continuationQueueLock)
		{
			flushAllContinuations.Enqueue(asyncContinuation);
		}
	}

	protected override void InitializeTarget()
	{
		base.InitializeTarget();
		RequestQueue.Clear();
		lazyWriterTimer = new Timer(ProcessPendingEvents, null, -1, -1);
		StartLazyWriterTimer();
	}

	protected override void CloseTarget()
	{
		StopLazyWriterThread();
		if (RequestQueue.RequestCount > 0)
		{
			ProcessPendingEvents(null);
		}
		base.CloseTarget();
	}

	protected virtual void StartLazyWriterTimer()
	{
		lock (lockObject)
		{
			if (lazyWriterTimer != null)
			{
				lazyWriterTimer.Change(TimeToSleepBetweenBatches, -1);
			}
		}
	}

	protected virtual void StopLazyWriterThread()
	{
		lock (lockObject)
		{
			if (lazyWriterTimer != null)
			{
				lazyWriterTimer.Change(-1, -1);
				lazyWriterTimer = null;
			}
		}
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		MergeEventProperties(logEvent.LogEvent);
		PrecalculateVolatileLayouts(logEvent.LogEvent);
		RequestQueue.Enqueue(logEvent);
	}

	private void ProcessPendingEvents(object state)
	{
		AsyncContinuation[] array;
		lock (continuationQueueLock)
		{
			array = ((flushAllContinuations.Count > 0) ? flushAllContinuations.ToArray() : new AsyncContinuation[1]);
			flushAllContinuations.Clear();
		}
		try
		{
			AsyncContinuation[] array2 = array;
			foreach (AsyncContinuation continuation in array2)
			{
				int num = BatchSize;
				if (continuation != null)
				{
					num = RequestQueue.RequestCount;
					Logger.Trace("Flushing {0} events.", num);
				}
				if (RequestQueue.RequestCount == 0 && continuation != null)
				{
					continuation(null);
				}
				AsyncLogEventInfo[] array3 = RequestQueue.DequeueBatch(num);
				if (continuation != null)
				{
					WrappedTarget.WriteAsyncLogEvents(array3, (Exception ex2) =>
					{
						WrappedTarget.Flush(continuation);
					});
				}
				else
				{
					WrappedTarget.WriteAsyncLogEvents(array3);
				}
			}
		}
		catch (Exception ex)
		{
			if (ex.MustBeRethrown())
			{
				throw;
			}
			Logger.Error("Error in lazy writer timer procedure: {0}", ex);
		}
		finally
		{
			StartLazyWriterTimer();
		}
	}
}
