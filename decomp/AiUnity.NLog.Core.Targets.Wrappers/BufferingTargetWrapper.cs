using System;
using System.ComponentModel;
using System.Threading;
using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Target("BufferingWrapper", IsWrapper = true)]
[Preserve]
public class BufferingTargetWrapper : WrapperTargetBase
{
	private LogEventInfoBuffer buffer;

	private Timer flushTimer;

	[DefaultValue(100)]
	[Display("Buffer size", "Buffer size to fill before write to inner target occurs.", false, 0)]
	public int BufferSize { get; set; }

	[DefaultValue(-1)]
	[Display("Timeout", "Timeout (milliseconds) after which buffer contents is flushed.  Disabled with a value of -1.", false, 0)]
	public int FlushTimeout { get; set; }

	[DefaultValue(true)]
	[Display("Slide Timeout", "When enabled timer reset on each write.  If disabled timer will start uninterrupted upon first write.", false, 0)]
	public bool SlidingTimeout { get; set; }

	public BufferingTargetWrapper()
		: this(null)
	{
	}

	public BufferingTargetWrapper(Target wrappedTarget)
		: this(wrappedTarget, 100)
	{
	}

	public BufferingTargetWrapper(Target wrappedTarget, int bufferSize)
		: this(wrappedTarget, bufferSize, -1)
	{
	}

	public BufferingTargetWrapper(Target wrappedTarget, int bufferSize, int flushTimeout)
	{
		WrappedTarget = wrappedTarget;
		BufferSize = bufferSize;
		FlushTimeout = flushTimeout;
		SlidingTimeout = true;
	}

	protected override void FlushAsync(AsyncContinuation asyncContinuation)
	{
		AsyncLogEventInfo[] eventsAndClear = buffer.GetEventsAndClear();
		if (eventsAndClear.Length == 0)
		{
			WrappedTarget.Flush(asyncContinuation);
			return;
		}
		WrappedTarget.WriteAsyncLogEvents(eventsAndClear, (Exception ex) =>
		{
			WrappedTarget.Flush(asyncContinuation);
		});
	}

	protected override void InitializeTarget()
	{
		base.InitializeTarget();
		buffer = new LogEventInfoBuffer(BufferSize, growAsNeeded: false, 0);
		flushTimer = new Timer(FlushCallback, null, -1, -1);
	}

	protected override void CloseTarget()
	{
		base.CloseTarget();
		if (flushTimer != null)
		{
			flushTimer.Dispose();
			flushTimer = null;
		}
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		WrappedTarget.PrecalculateVolatileLayouts(logEvent.LogEvent);
		int num = buffer.Append(logEvent);
		if (num >= BufferSize)
		{
			AsyncLogEventInfo[] eventsAndClear = buffer.GetEventsAndClear();
			WrappedTarget.WriteAsyncLogEvents(eventsAndClear);
		}
		else if (FlushTimeout > 0 && (SlidingTimeout || num == 1))
		{
			flushTimer.Change(FlushTimeout, -1);
		}
	}

	private void FlushCallback(object state)
	{
		lock (SyncRoot)
		{
			if (IsInitialized)
			{
				AsyncLogEventInfo[] eventsAndClear = buffer.GetEventsAndClear();
				if (eventsAndClear.Length != 0)
				{
					WrappedTarget.WriteAsyncLogEvents(eventsAndClear);
				}
			}
		}
	}
}
