using System;

namespace XRL;

/// <summary> Valid on either [HasModSensitiveCache] or [HasGameBasedStaticCache] types </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PreGameCacheInitAttribute : Attribute
{
}
