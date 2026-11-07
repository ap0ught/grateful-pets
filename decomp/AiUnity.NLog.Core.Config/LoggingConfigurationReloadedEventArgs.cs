using System;

namespace AiUnity.NLog.Core.Config;

public class LoggingConfigurationReloadedEventArgs : EventArgs
{
	public bool Succeeded { get; private set; }

	public Exception Exception { get; private set; }

	internal LoggingConfigurationReloadedEventArgs(bool succeeded, Exception exception)
	{
		Succeeded = succeeded;
		Exception = exception;
	}
}
