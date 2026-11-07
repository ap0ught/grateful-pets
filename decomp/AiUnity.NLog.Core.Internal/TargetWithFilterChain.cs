using System.Collections.Generic;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Filters;
using AiUnity.NLog.Core.Targets;

namespace AiUnity.NLog.Core.Internal;

[NLogConfigurationItem]
internal class TargetWithFilterChain
{
	private StackTraceUsage stackTraceUsage;

	public Target Target { get; private set; }

	public IList<Filter> FilterChain { get; private set; }

	public TargetWithFilterChain NextInChain { get; set; }

	public TargetWithFilterChain(Target target, IList<Filter> filterChain)
	{
		Target = target;
		FilterChain = filterChain;
		stackTraceUsage = StackTraceUsage.None;
	}

	public StackTraceUsage GetStackTraceUsage()
	{
		return stackTraceUsage;
	}

	internal void PrecalculateStackTraceUsage()
	{
		this.stackTraceUsage = StackTraceUsage.None;
		IUsesStackTrace[] array = ObjectGraphScanner.FindReachableObjects<IUsesStackTrace>(new object[1] { this });
		for (int i = 0; i < array.Length; i++)
		{
			StackTraceUsage stackTraceUsage = array[i].StackTraceUsage;
			if (stackTraceUsage > this.stackTraceUsage)
			{
				this.stackTraceUsage = stackTraceUsage;
				if (this.stackTraceUsage >= StackTraceUsage.WithSource)
				{
					break;
				}
			}
		}
	}
}
