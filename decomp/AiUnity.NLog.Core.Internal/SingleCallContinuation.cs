using System;
using System.Threading;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Internal;

internal class SingleCallContinuation
{
	private AsyncContinuation asyncContinuation;

	public SingleCallContinuation(AsyncContinuation asyncContinuation)
	{
		this.asyncContinuation = asyncContinuation;
	}

	public void Function(Exception exception)
	{
		try
		{
			Interlocked.Exchange(ref asyncContinuation, null)?.Invoke(exception);
		}
		catch (Exception exception2)
		{
			if (exception2.MustBeRethrown())
			{
				throw;
			}
			if (Singleton<NLogManager>.Instance.ThrowExceptions)
			{
				throw;
			}
			ReportExceptionInHandler(exception2);
		}
	}

	private static void ReportExceptionInHandler(Exception exception)
	{
		Singleton<NLogInternalLogger>.Instance.Error("Exception in asynchronous handler {0}", exception);
	}
}
