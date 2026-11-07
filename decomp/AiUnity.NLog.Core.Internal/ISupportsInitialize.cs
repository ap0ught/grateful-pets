using AiUnity.NLog.Core.Config;

namespace AiUnity.NLog.Core.Internal;

public interface ISupportsInitialize
{
	void Initialize(LoggingConfiguration configuration);

	void Close();
}
