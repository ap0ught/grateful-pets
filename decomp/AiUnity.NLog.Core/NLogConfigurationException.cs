using System;

namespace AiUnity.NLog.Core;

[Serializable]
public class NLogConfigurationException : Exception
{
	public NLogConfigurationException()
	{
	}

	public NLogConfigurationException(string message)
		: base(message)
	{
	}

	public NLogConfigurationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
