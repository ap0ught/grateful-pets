using System;
using System.ComponentModel;

namespace AiUnity.NLog.Core.Time;

[AttributeUsage(AttributeTargets.Class)]
public sealed class TimeSourceAttribute : DisplayNameAttribute
{
	public TimeSourceAttribute(string name)
		: base(name)
	{
	}
}
