#define NLOG_ALL
using System;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core;
using AiUnity.NLog.Core.Config;
using Platform.IO;
using UnityEngine;

public static class Logger
{
	public static NLogger log;

	public static NLogger traceLog;

	public static NLogger gameLog;

	public static SimpleFileLogger buildLog;

	public static void Exception(string context, Exception ex)
	{
		gameLog.Error(context + " - " + ex.ToString());
	}

	public static void Exception(Exception ex)
	{
		gameLog.Error(ex.ToString());
	}

	static Logger()
	{
		new LoggingConfiguration();
		gameLog = Singleton<NLogManager>.Instance.GetLogger("GameLog", new UnityEngine.Object());
		buildLog = SimpleFileLogger.Create("build_log.txt").AwaitResult();
		traceLog = Singleton<NLogManager>.Instance.GetLogger("TraceLog", new UnityEngine.Object());
		log = gameLog;
		gameLog.Info("GameLogger initialized...");
		buildLog.Info("BuildLogger initialized...");
		traceLog.Info("TraceLogger initialized...");
	}
}
