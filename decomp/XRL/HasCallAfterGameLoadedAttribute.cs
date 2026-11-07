using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HasCallAfterGameLoadedAttribute : Attribute
{
}
