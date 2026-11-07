using System;

namespace AiUnity.NLog.Core.Config;

public class LoggingConfigurationChangedEventArgs : EventArgs
{
	public LoggingConfiguration OldConfiguration { get; private set; }

	public LoggingConfiguration NewConfiguration { get; private set; }

	internal LoggingConfigurationChangedEventArgs(LoggingConfiguration oldConfiguration, LoggingConfiguration newConfiguration)
	{
		OldConfiguration = oldConfiguration;
		NewConfiguration = newConfiguration;
	}
}
