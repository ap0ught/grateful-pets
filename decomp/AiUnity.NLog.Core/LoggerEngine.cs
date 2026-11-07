using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Filters;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Targets;

namespace AiUnity.NLog.Core;

internal static class LoggerEngine
{
	private const int StackTraceSkipMethods = 0;

	private static readonly Assembly mscorlibAssembly = typeof(string).Assembly;

	private static readonly Assembly systemAssembly = typeof(Debug).Assembly;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	internal static void Write(Type loggerType, TargetWithFilterChain targets, LogEventInfo logEvent, LogFactory factory)
	{
		if (targets == null)
		{
			return;
		}
		StackTraceUsage stackTraceUsage = targets.GetStackTraceUsage();
		if (stackTraceUsage != StackTraceUsage.None && !logEvent.HasStackTrace)
		{
			StackTrace stackTrace = new StackTrace(0, stackTraceUsage == StackTraceUsage.WithSource);
			int userStackFrame = FindCallingMethodOnStackTrace(stackTrace, loggerType);
			logEvent.SetStackTrace(stackTrace, userStackFrame);
		}
		int originalThreadId = Thread.CurrentThread.ManagedThreadId;
		AsyncContinuation onException = (Exception ex) =>
		{
			if (ex != null && factory.ThrowExceptions && Thread.CurrentThread.ManagedThreadId == originalThreadId)
			{
				throw new NLogRuntimeException("Exception occurred in NLog", ex);
			}
		};
		TargetWithFilterChain targetWithFilterChain = targets;
		while (targetWithFilterChain != null && WriteToTargetWithFilterChain(targetWithFilterChain, logEvent, onException))
		{
			targetWithFilterChain = targetWithFilterChain.NextInChain;
		}
	}

	private static int FindCallingMethodOnStackTrace(StackTrace stackTrace, Type loggerType)
	{
		int? num = null;
		if (loggerType != null)
		{
			for (int i = 0; i < stackTrace.FrameCount; i++)
			{
				MethodBase method = stackTrace.GetFrame(i).GetMethod();
				if (method.DeclaringType == loggerType || (method.DeclaringType != null && (SkipAssembly(method.DeclaringType.Assembly) || SkipNameSpace(method.DeclaringType.Namespace))))
				{
					num = i + 1;
				}
				else if (num.HasValue)
				{
					break;
				}
			}
		}
		if (num == stackTrace.FrameCount)
		{
			num = null;
		}
		if (!num.HasValue)
		{
			for (int j = 0; j < stackTrace.FrameCount; j++)
			{
				MethodBase method2 = stackTrace.GetFrame(j).GetMethod();
				Assembly assembly = null;
				if (method2.DeclaringType != null)
				{
					assembly = method2.DeclaringType.Assembly;
				}
				if (SkipAssembly(assembly))
				{
					num = j + 1;
				}
				else if (num != 0)
				{
					break;
				}
			}
		}
		return num.GetValueOrDefault();
	}

	private static bool SkipAssembly(Assembly assembly)
	{
		if (assembly != null && assembly.GetModules().Any((Module m) => m.Name.Equals("NLog.dll")))
		{
			return true;
		}
		if (assembly == mscorlibAssembly)
		{
			return true;
		}
		if (assembly == systemAssembly)
		{
			return true;
		}
		if (Singleton<NLogManager>.Instance.HiddenAssemblies.Contains(assembly))
		{
			return true;
		}
		return false;
	}

	private static bool SkipNameSpace(string nameSpace)
	{
		if (string.IsNullOrEmpty(nameSpace))
		{
			return false;
		}
		if (nameSpace.StartsWith("AiUnity.NLog.Core") || nameSpace.StartsWith("AiUnity.CLog.Core") || nameSpace.StartsWith("AiUnity.Common"))
		{
			return true;
		}
		if (nameSpace.StartsWith("UnityEngine"))
		{
			return true;
		}
		if (Singleton<NLogManager>.Instance.HiddenNameSpaces.Any((string n) => n.StartsWith(nameSpace)))
		{
			return true;
		}
		return false;
	}

	private static bool WriteToTargetWithFilterChain(TargetWithFilterChain targetListHead, LogEventInfo logEvent, AsyncContinuation onException)
	{
		Target target = targetListHead.Target;
		FilterResult filterResult = GetFilterResult(targetListHead.FilterChain, logEvent);
		if (filterResult == FilterResult.Ignore || filterResult == FilterResult.IgnoreFinal)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.Debug("{0}.{1} Rejecting message because of a filter.", logEvent.LoggerName, logEvent.Level);
			}
			if (filterResult == FilterResult.IgnoreFinal)
			{
				return false;
			}
			return true;
		}
		target.WriteAsyncLogEvent(logEvent.WithContinuation(onException));
		if (filterResult == FilterResult.LogFinal)
		{
			return false;
		}
		return true;
	}

	private static FilterResult GetFilterResult(IEnumerable<Filter> filterChain, LogEventInfo logEvent)
	{
		FilterResult filterResult = FilterResult.Neutral;
		try
		{
			foreach (Filter item in filterChain)
			{
				filterResult = item.GetFilterResult(logEvent);
				if (filterResult != FilterResult.Neutral)
				{
					break;
				}
			}
			return filterResult;
		}
		catch (Exception ex)
		{
			if (ex.MustBeRethrown())
			{
				throw;
			}
			Logger.Warn("Exception during filter evaluation: {0}", ex);
			return FilterResult.Ignore;
		}
	}
}
