using System.Collections.Generic;
using AiUnity.Common.Log;

namespace AiUnity.NLog.Core.Internal;

internal class LoggerConfiguration
{
	private readonly Dictionary<LogLevels, TargetWithFilterChain> targetsByLevel;

	public LoggerConfiguration(Dictionary<LogLevels, TargetWithFilterChain> targetsByLevel)
	{
		this.targetsByLevel = targetsByLevel;
	}

	public TargetWithFilterChain GetTargetsForLevel(LogLevels level)
	{
		targetsByLevel.TryGetValue(level, out var value);
		return value;
	}

	public bool IsEnabled(LogLevels level)
	{
		return targetsByLevel.ContainsKey(level);
	}
}
