using System;

namespace XRL;

/// <summary>Will look for a [ModSensitiveCacheInit] static void Method(), and reset any [ModSensitiveStaticCache] fields</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class HasModSensitiveStaticCacheAttribute : Attribute
{
}
