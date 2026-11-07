using System;

namespace AiUnity.NLog.Core;

[Serializable]
public class NLogRuntimeException : Exception
{
	public NLogRuntimeException()
	{
	}

	public NLogRuntimeException(string message)
		: base(message)
	{
	}

	public NLogRuntimeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
