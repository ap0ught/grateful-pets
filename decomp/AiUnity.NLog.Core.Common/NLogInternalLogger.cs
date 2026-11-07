using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;

namespace AiUnity.NLog.Core.Common;

public class NLogInternalLogger : InternalLogger<NLogInternalLogger>
{
	static NLogInternalLogger()
	{
		Singleton<NLogInternalLogger>.Instance.Assert(true, "This log statement is executed prior to unity editor serialization due to InitializeOnLoad attribute.  The allows NLog logger to work in all phases of Unity Editor compile (ie. serialization).");
		Singleton<CommonInternalLogger>.Instance.Assert(true, "This log statement is executed prior to unity editor serialization due to InitializeOnLoad attribute.  The allows Common logger to work in all phases of Unity Editor compile (ie. serialization).");
	}
}
