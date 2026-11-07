using System;

namespace AiUnity.Common.Log;

[Flags]
public enum LogLevels
{
	Trace = 0x40,
	Debug = 0x20,
	Info = 0x10,
	Warn = 8,
	Error = 4,
	Fatal = 2,
	Assert = 1,
	Everything = -1
}
