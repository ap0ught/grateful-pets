using System;
using System.Collections.Generic;
using System.Threading;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[NLogConfigurationItem]
[Preserve]
public abstract class Target : ISupportsInitialize, IDisposable
{
	private object lockObject = new object();

	private List<Layout> allLayouts;

	private Exception initializeException;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public string Name { get; set; }

	protected object SyncRoot => lockObject;

	protected LoggingConfiguration LoggingConfiguration { get; private set; }

	protected bool IsInitialized { get; private set; }

	void ISupportsInitialize.Initialize(LoggingConfiguration configuration)
	{
		Initialize(configuration);
	}

	void ISupportsInitialize.Close()
	{
		Close();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void Flush(AsyncContinuation asyncContinuation)
	{
		if (asyncContinuation == null)
		{
			throw new ArgumentNullException("asyncContinuation");
		}
		lock (SyncRoot)
		{
			if (!IsInitialized)
			{
				asyncContinuation(null);
				return;
			}
			asyncContinuation = AsyncHelpers.PreventMultipleCalls(asyncContinuation);
			try
			{
				FlushAsync(asyncContinuation);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}
				asyncContinuation(exception);
			}
		}
	}

	public void PrecalculateVolatileLayouts(LogEventInfo logEvent)
	{
		lock (SyncRoot)
		{
			if (!IsInitialized)
			{
				return;
			}
			foreach (Layout allLayout in allLayouts)
			{
				allLayout.Precalculate(logEvent);
			}
		}
	}

	public override string ToString()
	{
		TargetAttribute targetAttribute = (TargetAttribute)Attribute.GetCustomAttribute(GetType(), typeof(TargetAttribute));
		if (targetAttribute != null)
		{
			return targetAttribute.DisplayName + " Target[" + (Name ?? "(unnamed)") + "]";
		}
		return GetType().Name;
	}

	public void WriteAsyncLogEvent(AsyncLogEventInfo logEvent)
	{
		lock (SyncRoot)
		{
			if (!IsInitialized)
			{
				logEvent.Continuation(null);
				return;
			}
			if (initializeException != null)
			{
				logEvent.Continuation(CreateInitException());
				return;
			}
			AsyncContinuation asyncContinuation = AsyncHelpers.PreventMultipleCalls(logEvent.Continuation);
			try
			{
				Write(logEvent.LogEvent.WithContinuation(asyncContinuation));
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}
				if (Singleton<NLogManager>.Instance.ThrowExceptions)
				{
					throw;
				}
				asyncContinuation(exception);
			}
		}
	}

	public void WriteAsyncLogEvents(params AsyncLogEventInfo[] logEvents)
	{
		if (logEvents == null || logEvents.Length == 0)
		{
			return;
		}
		lock (SyncRoot)
		{
			if (!IsInitialized)
			{
				AsyncLogEventInfo[] array = logEvents;
				foreach (AsyncLogEventInfo asyncLogEventInfo in array)
				{
					asyncLogEventInfo.Continuation(null);
				}
				return;
			}
			if (initializeException != null)
			{
				AsyncLogEventInfo[] array = logEvents;
				foreach (AsyncLogEventInfo asyncLogEventInfo2 in array)
				{
					asyncLogEventInfo2.Continuation(CreateInitException());
				}
				return;
			}
			AsyncLogEventInfo[] array2 = new AsyncLogEventInfo[logEvents.Length];
			for (int j = 0; j < logEvents.Length; j++)
			{
				array2[j] = logEvents[j].LogEvent.WithContinuation(AsyncHelpers.PreventMultipleCalls(logEvents[j].Continuation));
			}
			try
			{
				Write(array2);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}
				AsyncLogEventInfo[] array = array2;
				foreach (AsyncLogEventInfo asyncLogEventInfo3 in array)
				{
					asyncLogEventInfo3.Continuation(exception);
				}
			}
		}
	}

	internal void Initialize(LoggingConfiguration configuration)
	{
		lock (SyncRoot)
		{
			LoggingConfiguration = configuration;
			if (IsInitialized)
			{
				return;
			}
			PropertyHelper.CheckRequiredParameters(this);
			IsInitialized = true;
			try
			{
				InitializeTarget();
				initializeException = null;
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
				{
					throw;
				}
				initializeException = ex;
				Logger.Error("Error initializing target {0} {1}.", this, ex);
				throw;
			}
		}
	}

	internal void Close()
	{
		lock (SyncRoot)
		{
			LoggingConfiguration = null;
			if (!IsInitialized)
			{
				return;
			}
			IsInitialized = false;
			try
			{
				if (initializeException == null)
				{
					CloseTarget();
				}
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
				{
					throw;
				}
				Logger.Error("Error closing target {0} {1}.", this, ex);
				throw;
			}
		}
	}

	internal void WriteAsyncLogEvents(AsyncLogEventInfo[] logEventInfos, AsyncContinuation continuation)
	{
		if (logEventInfos.Length == 0)
		{
			continuation(null);
			return;
		}
		AsyncLogEventInfo[] array = new AsyncLogEventInfo[logEventInfos.Length];
		int remaining = logEventInfos.Length;
		for (int i = 0; i < logEventInfos.Length; i++)
		{
			AsyncContinuation originalContinuation = logEventInfos[i].Continuation;
			AsyncContinuation asyncContinuation = (Exception ex) =>
			{
				originalContinuation(ex);
				if (Interlocked.Decrement(ref remaining) == 0)
				{
					continuation(null);
				}
			};
			array[i] = logEventInfos[i].LogEvent.WithContinuation(asyncContinuation);
		}
		WriteAsyncLogEvents(array);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			CloseTarget();
		}
	}

	protected virtual void InitializeTarget()
	{
		GetAllLayouts();
	}

	protected virtual void CloseTarget()
	{
	}

	protected virtual void FlushAsync(AsyncContinuation asyncContinuation)
	{
		asyncContinuation(null);
	}

	protected virtual void Write(LogEventInfo logEvent)
	{
	}

	protected virtual void Write(AsyncLogEventInfo logEvent)
	{
		try
		{
			MergeEventProperties(logEvent.LogEvent);
			Write(logEvent.LogEvent);
			logEvent.Continuation(null);
		}
		catch (Exception exception)
		{
			if (exception.MustBeRethrown())
			{
				throw;
			}
			logEvent.Continuation(exception);
		}
	}

	protected virtual void Write(AsyncLogEventInfo[] logEvents)
	{
		for (int i = 0; i < logEvents.Length; i++)
		{
			Write(logEvents[i]);
		}
	}

	private Exception CreateInitException()
	{
		return new NLogRuntimeException("Target " + this?.ToString() + " failed to initialize.", initializeException);
	}

	private void GetAllLayouts()
	{
		allLayouts = new List<Layout>(ObjectGraphScanner.FindReachableObjects<Layout>(new object[1] { this }));
		Logger.Trace("{0} has {1} layouts", this, allLayouts.Count);
	}

	protected void MergeEventProperties(LogEventInfo logEvent)
	{
		if (logEvent.Parameters == null)
		{
			return;
		}
		object[] parameters = logEvent.Parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (!(parameters[i] is LogEventInfo logEventInfo))
			{
				continue;
			}
			foreach (object key in logEventInfo.Properties.Keys)
			{
				logEvent.Properties.Add(key, logEventInfo.Properties[key]);
			}
			logEventInfo.Properties.Clear();
		}
	}
}
