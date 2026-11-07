using AiUnity.NLog.Core.Config;

namespace AiUnity.NLog.Core.Internal;

public interface IUsesStackTrace
{
	StackTraceUsage StackTraceUsage { get; }
}
