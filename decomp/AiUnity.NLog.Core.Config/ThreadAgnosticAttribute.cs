using System;

namespace AiUnity.NLog.Core.Config;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ThreadAgnosticAttribute : Attribute
{
}
