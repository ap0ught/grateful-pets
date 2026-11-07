using System;

namespace AiUnity.Common.Log;

public interface IGameConsoleController
{
	void AddMessage(int logLevels, string message, string loggerName = null, DateTime dateTime = default(DateTime));

	void SetConsoleActive(bool consoleActive);

	void SetFontSize(int fontSize, bool updateControl = true, bool updateMessage = true);

	void SetIconEnable(bool iconEnable);

	void SetLogLevelFilter(LogLevels level, bool updateControl = true, bool updateMessage = true);
}
