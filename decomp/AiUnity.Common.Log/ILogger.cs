using System;
using UnityEngine;

namespace AiUnity.Common.Log;

public interface ILogger
{
	string Name { get; }

	void Log(LogLevels level, UnityEngine.Object context, Exception exception, string message, params object[] args);
}
