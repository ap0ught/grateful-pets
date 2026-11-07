using System;

namespace XRL;

/// <summary>
/// Signals new game to reset something about this class when a game starts.
/// Will reset static fields decorated with <see cref="T:XRL.GameBasedStaticCacheAttribute" />
/// and invoke static methods decorated with <see cref="T:XRL.GameBasedCacheInitAttribute" />, in that order.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HasGameBasedStaticCacheAttribute : Attribute
{
}
