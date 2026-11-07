using AiUnity.Common.Log;

namespace AiUnity.NLog.Core.Fluent;

public static class LoggerExtensions
{
	public static LogBuilder Log(this NLogger logger, LogLevels logLevel)
	{
		return new LogBuilder(logger, logLevel);
	}

	public static LogBuilder Trace(this NLogger logger)
	{
		return new LogBuilder(logger, LogLevels.Trace);
	}

	public static LogBuilder Debug(this NLogger logger)
	{
		return new LogBuilder(logger, LogLevels.Debug);
	}

	public static LogBuilder Info(this NLogger logger)
	{
		return new LogBuilder(logger, LogLevels.Info);
	}

	public static LogBuilder Warn(this NLogger logger)
	{
		return new LogBuilder(logger, LogLevels.Warn);
	}

	public static LogBuilder Error(this NLogger logger)
	{
		return new LogBuilder(logger, LogLevels.Error);
	}

	public static LogBuilder Fatal(this NLogger logger)
	{
		return new LogBuilder(logger, LogLevels.Fatal);
	}
}
