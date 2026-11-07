using AiUnity.Common.Log;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Targets;

namespace AiUnity.NLog.Core.Config;

public static class SimpleConfigurator
{
	public static void ConfigureForTargetLogging(Target target)
	{
		ConfigureForTargetLogging(target, LogLevels.Info);
	}

	public static void ConfigureForTargetLogging(Target target, LogLevels minLevel)
	{
		LoggingConfiguration loggingConfiguration = new LoggingConfiguration();
		LoggingRule item = new LoggingRule("*", "*", minLevel, target);
		loggingConfiguration.LoggingRules.Add(item);
		Singleton<NLogManager>.Instance.Configuration = loggingConfiguration;
	}

	public static void ConfigureForFileLogging(string fileName)
	{
		ConfigureForFileLogging(fileName, LogLevels.Info);
	}

	public static void ConfigureForFileLogging(string fileName, LogLevels minLevel)
	{
		ConfigureForTargetLogging(new FileTarget
		{
			FileName = fileName
		}, minLevel);
	}
}
