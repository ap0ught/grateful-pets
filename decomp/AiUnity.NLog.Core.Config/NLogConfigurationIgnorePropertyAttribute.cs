using System;

namespace AiUnity.NLog.Core.Config;

[AttributeUsage(AttributeTargets.Property)]
public sealed class NLogConfigurationIgnorePropertyAttribute : Attribute
{
}
