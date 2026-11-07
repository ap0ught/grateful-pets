using System;
using System.Threading;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Internal;

internal class TimeoutContinuation : IDisposable
{
	private AsyncContinuation asyncContinuation;

	private Timer timeoutTimer;

	public TimeoutContinuation(AsyncContinuation asyncContinuation, TimeSpan timeout)
	{
		this.asyncContinuation = asyncContinuation;
		timeoutTimer = new Timer(TimerElapsed, null, timeout, TimeSpan.FromMilliseconds(-1.0));
	}

	public void Function(Exception exception)
	{
		try
		{
			StopTimer();
			Interlocked.Exchange(ref asyncContinuation, null)?.Invoke(exception);
		}
		catch (Exception exception2)
		{
			if (exception2.MustBeRethrown())
			{
				throw;
			}
			ReportExceptionInHandler(exception2);
		}
	}

	public void Dispose()
	{
		StopTimer();
		GC.SuppressFinalize(this);
	}

	private static void ReportExceptionInHandler(Exception exception)
	{
		Singleton<NLogInternalLogger>.Instance.Error("Exception in asynchronous handler {0}", exception);
	}

	private void StopTimer()
	{
		lock (this)
		{
			if (timeoutTimer != null)
			{
				timeoutTimer.Dispose();
				timeoutTimer = null;
			}
		}
	}

	private void TimerElapsed(object state)
	{
		Function(new TimeoutException("Timeout."));
	}
}
