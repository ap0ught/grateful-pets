using System;
using System.Diagnostics;
using AiUnity.Common.Log;
using AiUnity.NLog.Core.Internal;
using UnityEngine;

namespace AiUnity.NLog.Core;

public class NLogger : AiUnity.Common.Log.ILogger
{
	private readonly Type loggerType = typeof(NLogger);

	private volatile LoggerConfiguration configuration;

	private volatile bool isDebugEnabled;

	private volatile bool isErrorEnabled;

	private volatile bool isFatalEnabled;

	private volatile bool isInfoEnabled;

	private volatile bool isTraceEnabled;

	private volatile bool isWarnEnabled;

	public UnityEngine.Object Context { get; set; }

	public LogFactory Factory { get; private set; }

	public IFormatProvider FormatProvider { get; protected set; }

	public string Name { get; private set; }

	public bool IsTraceEnabled => isTraceEnabled;

	public bool IsDebugEnabled => isDebugEnabled;

	public bool IsInfoEnabled => isInfoEnabled;

	public bool IsWarnEnabled => isWarnEnabled;

	public bool IsErrorEnabled => isErrorEnabled;

	public bool IsFatalEnabled => isFatalEnabled;

	public bool IsAssertEnabled => IsEnabled(LogLevels.Assert);

	public event EventHandler<EventArgs> LoggerReconfigured;

	protected internal NLogger(UnityEngine.Object context = null, IFormatProvider formatProvider = null)
	{
		Context = context;
		FormatProvider = formatProvider;
	}

	internal void Initialize(string name, LoggerConfiguration loggerConfiguration, LogFactory factory)
	{
		Name = name;
		Factory = factory;
		SetConfiguration(loggerConfiguration);
	}

	internal void SetConfiguration(LoggerConfiguration newConfiguration)
	{
		configuration = newConfiguration;
		isTraceEnabled = newConfiguration.IsEnabled(LogLevels.Trace);
		isDebugEnabled = newConfiguration.IsEnabled(LogLevels.Debug);
		isInfoEnabled = newConfiguration.IsEnabled(LogLevels.Info);
		isWarnEnabled = newConfiguration.IsEnabled(LogLevels.Warn);
		isErrorEnabled = newConfiguration.IsEnabled(LogLevels.Error);
		isFatalEnabled = newConfiguration.IsEnabled(LogLevels.Fatal);
		LoggerReconfigured?.Invoke(this, new EventArgs());
	}

	internal void WriteToTargets(LogLevels level, UnityEngine.Object context, IFormatProvider formatProvider, string message, object[] args, Exception exception = null)
	{
		LogEventInfo logEventInfo = LogEventInfo.Create(level, Name, context, formatProvider, message, args, exception);
		WriteToTargets(logEventInfo);
	}

	internal void WriteToTargets(LogEventInfo logEventInfo)
	{
		LoggerEngine.Write(loggerType, GetTargetsForLevel(logEventInfo.Level), logEventInfo, Factory);
	}

	private TargetWithFilterChain GetTargetsForLevel(LogLevels level)
	{
		return configuration.GetTargetsForLevel(level);
	}

	public bool IsEnabled(LogLevels level)
	{
		return GetTargetsForLevel(level) != null;
	}

	public void Log(LogEventInfo logEvent)
	{
		if (IsEnabled(logEvent.Level))
		{
			WriteToTargets(logEvent);
		}
	}

	public void Log(LogLevels level, string message, params object[] args)
	{
		if (IsEnabled(level))
		{
			WriteToTargets(level, Context, FormatProvider, message, args);
		}
	}

	public void Log(LogLevels level, UnityEngine.Object context, string message, params object[] args)
	{
		if (IsEnabled(level))
		{
			WriteToTargets(level, context, FormatProvider, message, args);
		}
	}

	public void Log(LogLevels level, Exception exception, string message, params object[] args)
	{
		if (IsEnabled(level))
		{
			WriteToTargets(level, Context, FormatProvider, message, args, exception);
		}
	}

	public void Log(LogLevels level, UnityEngine.Object context, Exception exception, string message, params object[] args)
	{
		if (IsEnabled(level))
		{
			WriteToTargets(level, context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_TRACE")]
	[Conditional("UNITY_EDITOR")]
	public void Trace(string message, params object[] args)
	{
		if (IsTraceEnabled)
		{
			WriteToTargets(LogLevels.Trace, Context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_TRACE")]
	[Conditional("UNITY_EDITOR")]
	public void Trace(UnityEngine.Object context, string message, params object[] args)
	{
		if (IsTraceEnabled)
		{
			WriteToTargets(LogLevels.Trace, context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_TRACE")]
	[Conditional("UNITY_EDITOR")]
	public void Trace(Exception exception, string message, params object[] args)
	{
		if (IsTraceEnabled)
		{
			WriteToTargets(LogLevels.Trace, Context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_TRACE")]
	[Conditional("UNITY_EDITOR")]
	public void Trace(Exception exception, UnityEngine.Object context, string message, params object[] args)
	{
		if (IsTraceEnabled)
		{
			WriteToTargets(LogLevels.Trace, context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public void Debug(string message, params object[] args)
	{
		if (IsDebugEnabled)
		{
			WriteToTargets(LogLevels.Debug, Context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public void Debug(UnityEngine.Object context, string message, params object[] args)
	{
		if (IsDebugEnabled)
		{
			WriteToTargets(LogLevels.Debug, context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public void Debug(Exception exception, string message, params object[] args)
	{
		if (IsDebugEnabled)
		{
			WriteToTargets(LogLevels.Debug, Context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public void Debug(Exception exception, UnityEngine.Object context, string message, params object[] args)
	{
		if (IsDebugEnabled)
		{
			WriteToTargets(LogLevels.Debug, context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_INFO")]
	[Conditional("UNITY_EDITOR")]
	public void Info(string message, params object[] args)
	{
		if (IsInfoEnabled)
		{
			WriteToTargets(LogLevels.Info, Context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_INFO")]
	[Conditional("UNITY_EDITOR")]
	public void Info(UnityEngine.Object context, string message, params object[] args)
	{
		if (IsInfoEnabled)
		{
			WriteToTargets(LogLevels.Info, context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_INFO")]
	[Conditional("UNITY_EDITOR")]
	public void Info(Exception exception, string message, params object[] args)
	{
		if (IsInfoEnabled)
		{
			WriteToTargets(LogLevels.Info, Context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_INFO")]
	[Conditional("UNITY_EDITOR")]
	public void Info(Exception exception, UnityEngine.Object context, string message, params object[] args)
	{
		if (IsInfoEnabled)
		{
			WriteToTargets(LogLevels.Info, context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_WARN")]
	[Conditional("UNITY_EDITOR")]
	public void Warn(string message, params object[] args)
	{
		if (IsWarnEnabled)
		{
			WriteToTargets(LogLevels.Warn, Context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_WARN")]
	[Conditional("UNITY_EDITOR")]
	public void Warn(UnityEngine.Object context, string message, params object[] args)
	{
		if (IsWarnEnabled)
		{
			WriteToTargets(LogLevels.Warn, context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_WARN")]
	[Conditional("UNITY_EDITOR")]
	public void Warn(Exception exception, string message, params object[] args)
	{
		if (IsWarnEnabled)
		{
			WriteToTargets(LogLevels.Warn, Context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_WARN")]
	[Conditional("UNITY_EDITOR")]
	public void Warn(Exception exception, UnityEngine.Object context, string message, params object[] args)
	{
		if (IsWarnEnabled)
		{
			WriteToTargets(LogLevels.Warn, context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_ERROR")]
	[Conditional("UNITY_EDITOR")]
	public void Error(string message, params object[] args)
	{
		if (IsErrorEnabled)
		{
			WriteToTargets(LogLevels.Error, Context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_ERROR")]
	[Conditional("UNITY_EDITOR")]
	public void Error(UnityEngine.Object context, string message, params object[] args)
	{
		if (IsErrorEnabled)
		{
			WriteToTargets(LogLevels.Error, context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_ERROR")]
	[Conditional("UNITY_EDITOR")]
	public void Error(Exception exception, string message, params object[] args)
	{
		if (IsErrorEnabled)
		{
			WriteToTargets(LogLevels.Error, Context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_ERROR")]
	[Conditional("UNITY_EDITOR")]
	public void Error(Exception exception, UnityEngine.Object context, string message, params object[] args)
	{
		if (IsErrorEnabled)
		{
			WriteToTargets(LogLevels.Error, context, FormatProvider, message, args, exception);
		}
	}

	public void Fatal(string message, params object[] args)
	{
		if (IsFatalEnabled)
		{
			WriteToTargets(LogLevels.Fatal, Context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_FATAL")]
	[Conditional("UNITY_EDITOR")]
	public void Fatal(UnityEngine.Object context, string message, params object[] args)
	{
		if (IsFatalEnabled)
		{
			WriteToTargets(LogLevels.Fatal, context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_FATAL")]
	[Conditional("UNITY_EDITOR")]
	public void Fatal(Exception exception, string message, params object[] args)
	{
		if (IsFatalEnabled)
		{
			WriteToTargets(LogLevels.Fatal, Context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_FATAL")]
	[Conditional("UNITY_EDITOR")]
	public void Fatal(Exception exception, UnityEngine.Object context, string message, params object[] args)
	{
		if (IsFatalEnabled)
		{
			WriteToTargets(LogLevels.Fatal, context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_ASSERT")]
	[Conditional("UNITY_EDITOR")]
	public void Assert(bool test, string message, params object[] args)
	{
		if (IsAssertEnabled && !test)
		{
			WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_ASSERT")]
	[Conditional("UNITY_EDITOR")]
	public void Assert(bool test, UnityEngine.Object context, string message, params object[] args)
	{
		if (IsAssertEnabled && !test)
		{
			WriteToTargets(LogLevels.Assert, context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_ASSERT")]
	[Conditional("UNITY_EDITOR")]
	public void Assert(Exception exception, string message, params object[] args)
	{
		if (IsAssertEnabled)
		{
			WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_ASSERT")]
	[Conditional("UNITY_EDITOR")]
	public void Assert(Exception exception, UnityEngine.Object context, string message, params object[] args)
	{
		if (IsAssertEnabled)
		{
			WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args, exception);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_ASSERT")]
	[Conditional("UNITY_EDITOR")]
	public void Assert(Func<bool> test, string message, params object[] args)
	{
		if (IsAssertEnabled && !test())
		{
			WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args);
		}
	}

	[Conditional("NLOG_ALL")]
	[Conditional("NLOG_ASSERT")]
	[Conditional("UNITY_EDITOR")]
	public void Assert(Func<bool> test, UnityEngine.Object context, string message, params object[] args)
	{
		if (IsAssertEnabled && !test())
		{
			WriteToTargets(LogLevels.Assert, context, FormatProvider, message, args);
		}
	}
}
