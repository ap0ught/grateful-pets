using System;
using AiUnity.Common.Log;

namespace AiUnity.Common.InternalLog;

public interface IInternalLogger
{
	LogLevels InternalLogLevel { get; set; }

	bool IsAssertEnabled { get; }

	bool IsDebugEnabled { get; }

	bool IsErrorEnabled { get; }

	bool IsFatalEnabled { get; }

	bool IsInfoEnabled { get; }

	bool IsTraceEnabled { get; }

	bool IsWarnEnabled { get; }

	void Assert(string message, params object[] args);

	void Assert(Exception e, string message, params object[] args);

	void Assert(bool condition, string message, params object[] args);

	void Debug(string message, params object[] args);

	void Error(string message, params object[] args);

	void Error(Exception e, string message, params object[] args);

	void Fatal(string message, params object[] args);

	void Fatal(Exception e, string message, params object[] args);

	void Info(string message, params object[] args);

	void Log(LogLevels levels, string message, params object[] args);

	void Trace(string message, params object[] args);

	void Warn(string message, params object[] args);

	void Warn(Exception e, string message, params object[] args);
}
