using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Layouts;
using AiUnity.NLog.Core.Time;
using UnityEngine;

namespace AiUnity.NLog.Core;

public class LogEventInfo
{
	public static readonly DateTime ZeroDate = DateTime.UtcNow;

	private static int globalSequenceId;

	private readonly object layoutCacheLock = new object();

	private string formattedMessage;

	private IDictionary<Layout, string> layoutCache;

	private IDictionary<object, object> properties;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public int SequenceID { get; private set; }

	public DateTime TimeStamp { get; set; }

	public LogLevels Level { get; set; }

	public bool HasStackTrace => StackTrace != null;

	public StackFrame UserStackFrame
	{
		get
		{
			if (StackTrace == null)
			{
				return null;
			}
			return StackTrace.GetFrame(UserStackFrameNumber);
		}
	}

	public int UserStackFrameNumber { get; private set; }

	public StackTrace StackTrace { get; private set; }

	public Exception Exception { get; set; }

	public string LoggerName { get; set; }

	public string Message { get; set; }

	public object[] Parameters { get; set; }

	public IFormatProvider FormatProvider { get; set; }

	public string FormattedMessage
	{
		get
		{
			if (formattedMessage == null)
			{
				CalcFormattedMessage();
			}
			return formattedMessage;
		}
	}

	public IDictionary<object, object> Properties
	{
		get
		{
			if (properties == null)
			{
				InitEventContext();
			}
			return properties;
		}
	}

	public UnityEngine.Object Context { get; private set; }

	public bool FromUnityLogListener { get; set; }

	internal ISupportsInitialize UnityLogListener { get; set; }

	public LogEventInfo()
	{
	}

	public LogEventInfo(LogLevels level, string loggerName, UnityEngine.Object context, IFormatProvider formatProvider, string message, object[] parameters = null, Exception exception = null)
	{
		TimeStamp = TimeSource.Current.Time;
		Level = level;
		LoggerName = loggerName;
		Message = message;
		Parameters = parameters;
		FormatProvider = formatProvider;
		Exception = exception;
		SequenceID = Interlocked.Increment(ref globalSequenceId);
		Context = context;
		if (NeedToPreformatMessage(parameters))
		{
			CalcFormattedMessage();
		}
	}

	public static LogEventInfo CreateNullEvent()
	{
		return new LogEventInfo((LogLevels)0, string.Empty, null, null, string.Empty);
	}

	public static LogEventInfo Create(LogLevels logLevel, string loggerName, UnityEngine.Object context, IFormatProvider formatProvider, string message, object[] parameters = null, Exception exception = null)
	{
		return new LogEventInfo(logLevel, loggerName, context, formatProvider, message, parameters, exception);
	}

	public AsyncLogEventInfo WithContinuation(AsyncContinuation asyncContinuation)
	{
		return new AsyncLogEventInfo(this, asyncContinuation);
	}

	public override string ToString()
	{
		return "Log Event: Logger='" + LoggerName + "' Level=" + Level.ToString() + " Message='" + FormattedMessage + "' SequenceID=" + SequenceID;
	}

	public void SetStackTrace(StackTrace stackTrace, int userStackFrame)
	{
		StackTrace = stackTrace;
		UserStackFrameNumber = userStackFrame;
	}

	internal string AddCachedLayoutValue(Layout layout, string value)
	{
		lock (layoutCacheLock)
		{
			if (layoutCache == null)
			{
				layoutCache = new Dictionary<Layout, string>();
			}
			layoutCache[layout] = value;
			return value;
		}
	}

	internal bool TryGetCachedLayoutValue(Layout layout, out string value)
	{
		lock (layoutCacheLock)
		{
			if (layoutCache == null)
			{
				value = null;
				return false;
			}
			return layoutCache.TryGetValue(layout, out value);
		}
	}

	private static bool NeedToPreformatMessage(object[] parameters)
	{
		if (parameters == null || parameters.Length == 0)
		{
			return false;
		}
		if (parameters.Length > 3)
		{
			return true;
		}
		if (!IsSafeToDeferFormatting(parameters[0]))
		{
			return true;
		}
		if (parameters.Length >= 2 && !IsSafeToDeferFormatting(parameters[1]))
		{
			return true;
		}
		if (parameters.Length >= 3 && !IsSafeToDeferFormatting(parameters[2]))
		{
			return true;
		}
		return false;
	}

	private static bool IsSafeToDeferFormatting(object value)
	{
		if (value == null)
		{
			return true;
		}
		if (!value.GetType().IsPrimitive)
		{
			return value is string;
		}
		return true;
	}

	private void CalcFormattedMessage()
	{
		if (Parameters == null || Parameters.Length == 0)
		{
			formattedMessage = Message;
			return;
		}
		try
		{
			formattedMessage = string.Format(FormatProvider ?? Singleton<NLogManager>.Instance.DefaultCultureInfo(), Message, Parameters);
		}
		catch (Exception ex)
		{
			formattedMessage = Message;
			if (ex.MustBeRethrown())
			{
				throw;
			}
			Logger.Warn("Error when formatting a message: {0}", ex);
		}
	}

	private void InitEventContext()
	{
		properties = new Dictionary<object, object>();
	}
}
