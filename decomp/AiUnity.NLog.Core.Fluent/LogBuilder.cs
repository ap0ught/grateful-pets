using System;
using System.Diagnostics;
using AiUnity.Common.Log;

namespace AiUnity.NLog.Core.Fluent;

public class LogBuilder
{
	private readonly LogEventInfo _logEvent;

	private readonly NLogger _logger;

	public LogEventInfo LogEventInfo => _logEvent;

	public LogBuilder(NLogger logger)
		: this(logger, LogLevels.Debug)
	{
	}

	public LogBuilder(NLogger logger, LogLevels logLevel)
	{
		if (logger == null)
		{
			throw new ArgumentNullException("logger");
		}
		_logger = logger;
		_logEvent = new LogEventInfo
		{
			Level = logLevel,
			LoggerName = logger.Name,
			TimeStamp = DateTime.Now
		};
	}

	public LogBuilder Exception(Exception exception)
	{
		_logEvent.Exception = exception;
		return this;
	}

	public LogBuilder Level(LogLevels logLevel)
	{
		_logEvent.Level = logLevel;
		return this;
	}

	public LogBuilder LoggerName(string loggerName)
	{
		_logEvent.LoggerName = loggerName;
		return this;
	}

	public LogBuilder Message(string message)
	{
		_logEvent.Message = message;
		return this;
	}

	public LogBuilder Message(string format, object arg0)
	{
		_logEvent.Message = format;
		_logEvent.Parameters = new object[1] { arg0 };
		return this;
	}

	public LogBuilder Message(string format, object arg0, object arg1)
	{
		_logEvent.Message = format;
		_logEvent.Parameters = new object[2] { arg0, arg1 };
		return this;
	}

	public LogBuilder Message(string format, object arg0, object arg1, object arg2)
	{
		_logEvent.Message = format;
		_logEvent.Parameters = new object[3] { arg0, arg1, arg2 };
		return this;
	}

	public LogBuilder Message(string format, object arg0, object arg1, object arg2, object arg3)
	{
		_logEvent.Message = format;
		_logEvent.Parameters = new object[4] { arg0, arg1, arg2, arg3 };
		return this;
	}

	public LogBuilder Message(string format, params object[] args)
	{
		_logEvent.Message = format;
		_logEvent.Parameters = args;
		return this;
	}

	public LogBuilder Message(IFormatProvider provider, string format, params object[] args)
	{
		_logEvent.FormatProvider = provider;
		_logEvent.Message = format;
		_logEvent.Parameters = args;
		return this;
	}

	public LogBuilder Property(object name, object value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		_logEvent.Properties[name] = value;
		return this;
	}

	public LogBuilder TimeStamp(DateTime timeStamp)
	{
		_logEvent.TimeStamp = timeStamp;
		return this;
	}

	public LogBuilder StackTrace(StackTrace stackTrace, int userStackFrame)
	{
		_logEvent.SetStackTrace(stackTrace, userStackFrame);
		return this;
	}

	public void Write()
	{
		_logger.Log(_logEvent);
	}

	public void WriteIf(Func<bool> condition)
	{
		if (condition != null && condition())
		{
			_logger.Log(_logEvent);
		}
	}

	public void WriteIf(bool condition)
	{
		if (condition)
		{
			_logger.Log(_logEvent);
		}
	}
}
