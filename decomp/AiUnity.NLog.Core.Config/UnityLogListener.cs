using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Time;
using UnityEngine;

namespace AiUnity.NLog.Core.Config;

[NLogConfigurationItem]
public class UnityLogListener : ISupportsInitialize
{
	public List<StackFrame> stackFrames;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	void ISupportsInitialize.Close()
	{
		Logger.Info("Removing UnityLogListener");
		Application.logMessageReceived -= HandleLog;
	}

	void ISupportsInitialize.Initialize(LoggingConfiguration configuration)
	{
		Logger.Info("Adding UnityLogListener");
		Application.logMessageReceived += HandleLog;
	}

	private void HandleLog(string message, string stackTrace, LogType type)
	{
		LogLevels logLevel = LogLevels.Debug;
		switch (type)
		{
		case LogType.Assert:
			logLevel = LogLevels.Assert;
			break;
		case LogType.Exception:
			logLevel = LogLevels.Assert;
			break;
		case LogType.Log:
			logLevel = LogLevels.Debug;
			break;
		case LogType.Error:
			logLevel = LogLevels.Error;
			break;
		case LogType.Warning:
			logLevel = LogLevels.Warn;
			break;
		}
		LogEventInfo logEventInfo = CreateLogEventInfo(logLevel, message, null);
		Singleton<NLogManager>.Instance.GetLogger(logEventInfo.LoggerName, logEventInfo.Context, logEventInfo.FormatProvider).Log(logEventInfo);
	}

	protected virtual LogEventInfo CreateLogEventInfo(LogLevels logLevel, string message, object[] arguments)
	{
		string loggerName = "Unity";
		try
		{
			StackFrame stackFrame = new StackTrace(fNeedFileInfo: true).GetFrames().SkipWhile((StackFrame s) => s.GetMethod().DeclaringType.Namespace.StartsWith("UnityEngine") || s.GetMethod().DeclaringType.Name.Equals("UnityLogListener")).FirstOrDefault();
			if (stackFrame != null)
			{
				Type declaringType = stackFrame.GetMethod().DeclaringType;
				if (declaringType.Namespace.StartsWith("AiUnity.NLog.Core") || declaringType.Namespace.StartsWith("AiUnity.CLog.Core") || declaringType.Namespace.StartsWith("AiUnity.Common"))
				{
					return null;
				}
				loggerName = declaringType.FullName;
			}
		}
		catch
		{
			loggerName = "Unity";
		}
		return new LogEventInfo(logLevel, loggerName, null, null, message, arguments)
		{
			TimeStamp = TimeSource.Current.Time,
			UnityLogListener = this,
			FromUnityLogListener = true
		};
	}
}
