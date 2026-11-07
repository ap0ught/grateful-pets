using AiUnity.Common.Patterns;

namespace AiUnity.Common.InternalLog;

public class CommonInternalLogger : InternalLogger<CommonInternalLogger>
{
	static CommonInternalLogger()
	{
		Singleton<CommonInternalLogger>.Instance.Assert(true, "This log statement is executed prior to unity editor serialization due to InitializeOnLoad attribute.  The allows Common logger to work in all phases of Unity Editor compile (ie. serialization).");
	}
}
