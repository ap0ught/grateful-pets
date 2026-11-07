using System;
using AiUnity.Common.Log;

namespace AiUnity.Common.LogUI.Scripts;

public class LogMessage
{
	public string LoggerName { get; set; }

	public LogLevels LogLevels { get; set; }

	public string Message { get; set; }

	public DateTime TimeStamp { get; set; }

	public LogMessage(string message, int logLevel, string loggerName, DateTime timeStamp)
	{
		Message = message;
		LogLevels = (LogLevels)logLevel;
		LoggerName = loggerName;
		TimeStamp = timeStamp;
	}
}
