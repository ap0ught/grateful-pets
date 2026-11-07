using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Internal;

namespace AiUnity.NLog.Core.Common;

public static class AsyncHelpers
{
	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public static void ForEachItemSequentially<T>(IEnumerable<T> items, AsyncContinuation asyncContinuation, AsynchronousAction<T> action)
	{
		action = ExceptionGuard(action);
		AsyncContinuation invokeNext = null;
		IEnumerator<T> enumerator = items.GetEnumerator();
		invokeNext = (Exception ex) =>
		{
			if (ex != null)
			{
				asyncContinuation(ex);
			}
			else if (!enumerator.MoveNext())
			{
				asyncContinuation(null);
			}
			else
			{
				action(enumerator.Current, PreventMultipleCalls(invokeNext));
			}
		};
		invokeNext(null);
	}

	public static void Repeat(int repeatCount, AsyncContinuation asyncContinuation, AsynchronousAction action)
	{
		action = ExceptionGuard(action);
		AsyncContinuation invokeNext = null;
		int remaining = repeatCount;
		invokeNext = (Exception ex) =>
		{
			if (ex != null)
			{
				asyncContinuation(ex);
			}
			else if (remaining-- <= 0)
			{
				asyncContinuation(null);
			}
			else
			{
				action(PreventMultipleCalls(invokeNext));
			}
		};
		invokeNext(null);
	}

	public static AsyncContinuation PrecededBy(AsyncContinuation asyncContinuation, AsynchronousAction action)
	{
		action = ExceptionGuard(action);
		return (Exception ex) =>
		{
			if (ex != null)
			{
				asyncContinuation(ex);
			}
			else
			{
				action(PreventMultipleCalls(asyncContinuation));
			}
		};
	}

	public static AsyncContinuation WithTimeout(AsyncContinuation asyncContinuation, TimeSpan timeout)
	{
		return new TimeoutContinuation(asyncContinuation, timeout).Function;
	}

	public static void ForEachItemInParallel<T>(IEnumerable<T> values, AsyncContinuation asyncContinuation, AsynchronousAction<T> action)
	{
		action = ExceptionGuard(action);
		List<T> list = new List<T>(values);
		int remaining = list.Count;
		List<Exception> exceptions = new List<Exception>();
		Logger.Trace("ForEachItemInParallel() {0} items", list.Count);
		if (remaining == 0)
		{
			asyncContinuation(null);
			return;
		}
		AsyncContinuation continuation = (Exception ex) =>
		{
			Logger.Trace("Continuation invoked: {0}", ex);
			if (ex != null)
			{
				lock (exceptions)
				{
					exceptions.Add(ex);
				}
			}
			int num = Interlocked.Decrement(ref remaining);
			Logger.Trace("Parallel task completed. {0} items remaining", num);
			if (num == 0)
			{
				asyncContinuation(GetCombinedException(exceptions));
			}
		};
		foreach (T item in list)
		{
			T itemCopy = item;
			ThreadPool.QueueUserWorkItem((object s) =>
			{
				action(itemCopy, PreventMultipleCalls(continuation));
			});
		}
	}

	public static void RunSynchronously(AsynchronousAction action)
	{
		ManualResetEvent ev = new ManualResetEvent(initialState: false);
		Exception lastException = null;
		action(PreventMultipleCalls((Exception ex) =>
		{
			lastException = ex;
			ev.Set();
		}));
		ev.WaitOne();
		if (lastException != null)
		{
			throw new NLogRuntimeException("Asynchronous exception has occurred.", lastException);
		}
	}

	public static AsyncContinuation PreventMultipleCalls(AsyncContinuation asyncContinuation)
	{
		if (asyncContinuation.Target is SingleCallContinuation)
		{
			return asyncContinuation;
		}
		return new SingleCallContinuation(asyncContinuation).Function;
	}

	public static Exception GetCombinedException(IList<Exception> exceptions)
	{
		if (exceptions.Count == 0)
		{
			return null;
		}
		if (exceptions.Count == 1)
		{
			return exceptions[0];
		}
		StringBuilder stringBuilder = new StringBuilder();
		string value = string.Empty;
		string newLine = Environment.NewLine;
		foreach (Exception exception in exceptions)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(exception.ToString());
			stringBuilder.Append(newLine);
			value = newLine;
		}
		return new NLogRuntimeException("Got multiple exceptions:" + Environment.NewLine + stringBuilder);
	}

	private static AsynchronousAction ExceptionGuard(AsynchronousAction action)
	{
		return (AsyncContinuation cont) =>
		{
			try
			{
				action(cont);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}
				cont(exception);
			}
		};
	}

	private static AsynchronousAction<T> ExceptionGuard<T>(AsynchronousAction<T> action)
	{
		return (T argument, AsyncContinuation cont) =>
		{
			try
			{
				action(argument, cont);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}
				cont(exception);
			}
		};
	}
}
