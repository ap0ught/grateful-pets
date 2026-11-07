using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using AiUnity.Common.Extensions;
using AiUnity.Common.Log;
using AiUnity.Common.Patterns;
using UnityEngine;

namespace AiUnity.Common.InternalLog;

public abstract class InternalLogger<T> : Singleton<T>, IInternalLogger where T : new()
{
	private LogLevels internalLogLevel;

	private bool isDarkTheme;

	private string logLevelPlayerPref;

	public LogLevels InternalLogLevel
	{
		get
		{
			return internalLogLevel;
		}
		set
		{
			if (internalLogLevel != value)
			{
				Logger.Info("Setting {0} (PlayerPref=\"{1}\") to LogLevel={2}", GetType().Name, logLevelPlayerPref, value);
				PlayerPrefs.SetInt(logLevelPlayerPref, (int)value);
				internalLogLevel = value;
				if (Logger != this)
				{
					Logger.InternalLogLevel = value;
				}
			}
		}
	}

	public bool IsAssertEnabled => IsEnabled(LogLevels.Assert);

	public bool IsDebugEnabled => IsEnabled(LogLevels.Debug);

	public bool IsErrorEnabled => IsEnabled(LogLevels.Error);

	public bool IsFatalEnabled => IsEnabled(LogLevels.Fatal);

	public bool IsInfoEnabled => IsEnabled(LogLevels.Info);

	public bool IsTraceEnabled => IsEnabled(LogLevels.Trace);

	public bool IsWarnEnabled => IsEnabled(LogLevels.Warn);

	protected bool IncludeTimestamp { get; set; }

	protected LogLevels InternalLogLevelsDefault { get; set; }

	private static IInternalLogger Logger => Singleton<CommonInternalLogger>.Instance;

	public InternalLogger()
	{
		logLevelPlayerPref = "AiUnity" + GetType().Name + "LogLevel";
		internalLogLevel = PlayerPrefs.GetInt(logLevelPlayerPref, (int)InternalLogLevelsDefault).ToEnum<LogLevels>();
		isDarkTheme = Application.isEditor && Convert.ToBoolean(PlayerPrefs.GetInt("AiUnityIsProSkin", 0));
		InternalLogLevelsDefault = LogLevels.Info | LogLevels.Warn | LogLevels.Error | LogLevels.Fatal;
		IncludeTimestamp = false;
	}

	public void Assert(string message, params object[] args)
	{
		if (IsAssertEnabled)
		{
			Write(LogLevels.Assert, message, args);
		}
	}

	public void Assert(bool condition, string message, params object[] args)
	{
		if (IsAssertEnabled && !condition)
		{
			Write(LogLevels.Assert, message, args);
		}
	}

	public void Assert(Exception e, string message, params object[] args)
	{
		if (IsAssertEnabled && e != null)
		{
			string message2 = string.Format("{0}/n<color=#ff0000ff>{1}</color>", message, (e != null) ? e.Message : "Missing exception message");
			Write(LogLevels.Assert, message2, args);
		}
	}

	public void Debug(string message, params object[] args)
	{
		if (IsDebugEnabled)
		{
			Write(LogLevels.Debug, message, args);
		}
	}

	public void Error(string message, params object[] args)
	{
		if (IsErrorEnabled)
		{
			Write(LogLevels.Error, message, args);
		}
	}

	public void Error(Exception e, string message, params object[] args)
	{
		if (IsErrorEnabled)
		{
			string arg = ((e != null) ? e.Message : "Missing exception message");
			string message2 = $"{message}/n<color=#ff0000ff>{arg}</color>";
			Write(LogLevels.Error, message2, args);
		}
	}

	public void Fatal(string message, params object[] args)
	{
		if (IsFatalEnabled)
		{
			Write(LogLevels.Fatal, message, args);
		}
	}

	public void Fatal(Exception e, string message, params object[] args)
	{
		if (IsFatalEnabled)
		{
			string arg = ((e != null) ? e.Message : "Missing exception message");
			string message2 = $"{message}/n<color=#ff0000ff>{arg}</color>";
			Write(LogLevels.Fatal, message2, args);
		}
	}

	public void Info(string message, params object[] args)
	{
		if (IsInfoEnabled)
		{
			Write(LogLevels.Info, message, args);
		}
	}

	public void Log(LogLevels levels, string message, params object[] args)
	{
		if (IsEnabled(levels))
		{
			Write(levels, message, args);
		}
	}

	public void Trace(string message, params object[] args)
	{
		if (IsTraceEnabled)
		{
			Write(LogLevels.Trace, message, args);
		}
	}

	public void Warn(string message, params object[] args)
	{
		if (IsWarnEnabled)
		{
			Write(LogLevels.Warn, message, args);
		}
	}

	public void Warn(Exception e, string message, params object[] args)
	{
		if (IsErrorEnabled)
		{
			string arg = ((e != null) ? e.Message : "Missing exception message");
			string message2 = $"{message}/n<color=#ff0000ff>{arg}</color>";
			Write(LogLevels.Warn, message2, args);
		}
	}

	protected virtual string CreateMessage(LogLevels levels, string formattedMessage)
	{
		string arg;
		try
		{
			StackFrame stackFrame = new StackTrace(fNeedFileInfo: true).GetFrames().SkipWhile((StackFrame f) => typeof(IInternalLogger).IsAssignableFrom(f.GetMethod().DeclaringType)).FirstOrDefault();
			arg = $"{stackFrame.GetMethod().DeclaringType.FullName}.{stackFrame.GetMethod().Name}";
		}
		catch
		{
			arg = $"AiUnity Internal Message ({GetType().Name})";
		}
		string text = $"[{levels} (ScriptBuilder Internal)] {arg}";
		if (Application.isEditor)
		{
			string arg2 = (isDarkTheme ? "orange" : "magenta");
			text = $"<color={arg2}>{text}</color>";
		}
		return text + Environment.NewLine + formattedMessage;
	}

	protected void Write(LogLevels levels, string message, object[] args)
	{
		string formattedMessage = ((args != null && args.Length != 0) ? string.Format(CultureInfo.InvariantCulture, message, args) : message);
		string message2 = CreateMessage(levels, formattedMessage);
		if (levels.Has(LogLevels.Info) || levels.Has(LogLevels.Debug) || levels.Has(LogLevels.Trace))
		{
			UnityEngine.Debug.Log(message2);
		}
		else if (levels.Has(LogLevels.Warn))
		{
			UnityEngine.Debug.LogWarning(message2);
		}
		else if (levels.Has(LogLevels.Error) || levels.Has(LogLevels.Fatal))
		{
			UnityEngine.Debug.LogError(message2);
		}
		else
		{
			levels.Has(LogLevels.Assert);
		}
	}

	private bool IsEnabled(LogLevels level)
	{
		return InternalLogLevel.Has(level);
	}
}
