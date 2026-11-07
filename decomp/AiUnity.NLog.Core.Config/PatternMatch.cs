using System;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Config;

public class PatternMatch
{
	internal enum MatchMode
	{
		All,
		None,
		Equals,
		StartsWith,
		EndsWith,
		Contains
	}

	private string loggerNamePattern;

	private MatchMode loggerNameMatchMode;

	private string loggerNameMatchArgument;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public string LoggerNamePattern
	{
		get
		{
			return loggerNamePattern;
		}
		set
		{
			loggerNamePattern = value;
			int num = loggerNamePattern.IndexOf('*');
			int num2 = loggerNamePattern.LastIndexOf('*');
			if (num < 0)
			{
				loggerNameMatchMode = MatchMode.Equals;
				loggerNameMatchArgument = value;
			}
			else if (num == num2)
			{
				string text = LoggerNamePattern.Substring(0, num);
				string text2 = LoggerNamePattern.Substring(num + 1);
				if (text.Length > 0)
				{
					loggerNameMatchMode = MatchMode.StartsWith;
					loggerNameMatchArgument = text;
				}
				else if (text2.Length > 0)
				{
					loggerNameMatchMode = MatchMode.EndsWith;
					loggerNameMatchArgument = text2;
				}
			}
			else if (num == 0 && num2 == LoggerNamePattern.Length - 1)
			{
				string text3 = LoggerNamePattern.Substring(1, LoggerNamePattern.Length - 2);
				loggerNameMatchMode = MatchMode.Contains;
				loggerNameMatchArgument = text3;
			}
			else
			{
				loggerNameMatchMode = MatchMode.None;
				loggerNameMatchArgument = string.Empty;
			}
		}
	}

	public PatternMatch(string loggerNamePattern = "*")
	{
		LoggerNamePattern = loggerNamePattern;
	}

	public bool NameMatches(string loggerName)
	{
		Logger.Debug("Matching rule = {0} (Pattern={1}) LoggerName={2}", loggerNameMatchArgument, loggerNamePattern, loggerName);
		return loggerNameMatchMode switch
		{
			MatchMode.All => true, 
			MatchMode.Equals => loggerName.Equals(loggerNameMatchArgument, StringComparison.Ordinal), 
			MatchMode.StartsWith => loggerName.StartsWith(loggerNameMatchArgument, StringComparison.Ordinal), 
			MatchMode.EndsWith => loggerName.EndsWith(loggerNameMatchArgument, StringComparison.Ordinal), 
			MatchMode.Contains => loggerName.IndexOf(loggerNameMatchArgument, StringComparison.Ordinal) >= 0, 
			_ => false, 
		};
	}

	public override string ToString()
	{
		return $"logNamePattern: ({loggerNameMatchArgument}:{loggerNameMatchMode})";
	}
}
