using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AiUnity.Common.Extensions;
using AiUnity.Common.Log;
using AiUnity.Common.Types;
using AiUnity.NLog.Core.Filters;
using AiUnity.NLog.Core.Targets;
using UnityEngine;

namespace AiUnity.NLog.Core.Config;

[NLogConfigurationItem]
public class LoggingRule
{
	public LogLevels logLevels;

	public PatternMatch namePatternMatch;

	public PatternMatch namespacePatternMatch;

	public IList<Target> Targets { get; private set; }

	public IList<LoggingRule> ChildRules { get; private set; }

	public IList<Filter> Filters { get; private set; }

	public bool Final { get; set; }

	public PlatformEnumFlagWrapper<RuntimePlatform> TargetPlatforms { get; set; }

	public LoggingRule()
	{
		Filters = new List<Filter>();
		ChildRules = new List<LoggingRule>();
		Targets = new List<Target>();
		namePatternMatch = new PatternMatch();
		namespacePatternMatch = new PatternMatch();
	}

	public LoggingRule(string loggerNamePattern, string loggerNamespacePattern, LogLevels minLevel, Target target)
	{
		Filters = new List<Filter>();
		ChildRules = new List<LoggingRule>();
		Targets = new List<Target>();
		namePatternMatch = new PatternMatch(loggerNamePattern);
		namespacePatternMatch = new PatternMatch(loggerNamespacePattern);
		Targets.Add(target);
		logLevels = LogLevels.Everything & (minLevel - 1);
	}

	public LoggingRule(string loggerNamePattern, string loggerNamespacePattern, Target target)
	{
		Filters = new List<Filter>();
		ChildRules = new List<LoggingRule>();
		Targets = new List<Target>();
		namePatternMatch = new PatternMatch(loggerNamePattern);
		namespacePatternMatch = new PatternMatch(loggerNamespacePattern);
		Targets.Add(target);
	}

	public void EnableLoggingForLevel(LogLevels level)
	{
		logLevels |= level;
	}

	public void DisableLoggingForLevel(LogLevels level)
	{
		logLevels &= ~level;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, namePatternMatch.ToString());
		stringBuilder.Append(" levels: [ ");
		stringBuilder.Append(logLevels);
		stringBuilder.Append("] appendTo: [ ");
		foreach (Target target in Targets)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0} ", target.Name);
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public bool IsLoggingEnabledForLevel(LogLevels level)
	{
		return logLevels.Has(level);
	}

	public bool NameMatches(string loggerFullName)
	{
		int num = loggerFullName.LastIndexOf('.');
		string loggerName = loggerFullName.Substring(num + 1);
		string loggerName2 = loggerFullName.Substring(0, Math.Max(0, num));
		if (namePatternMatch.NameMatches(loggerName))
		{
			return namespacePatternMatch.NameMatches(loggerName2);
		}
		return false;
	}

	internal bool isPlatformMatch()
	{
		return TargetPlatforms.Has(Application.platform);
	}
}
