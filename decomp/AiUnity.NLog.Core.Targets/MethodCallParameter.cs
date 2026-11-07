using System;
using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[NLogConfigurationItem]
[Preserve]
public class MethodCallParameter
{
	public string Name { get; set; }

	public Type Type { get; set; }

	[RequiredParameter]
	public Layout Layout { get; set; }

	public MethodCallParameter()
	{
		Type = typeof(string);
	}

	public MethodCallParameter(Layout layout)
	{
		Type = typeof(string);
		Layout = layout;
	}

	public MethodCallParameter(string parameterName, Layout layout)
	{
		Type = typeof(string);
		Name = parameterName;
		Layout = layout;
	}

	public MethodCallParameter(string name, Layout layout, Type type)
	{
		Type = type;
		Name = name;
		Layout = layout;
	}

	internal object GetValue(LogEventInfo logEvent)
	{
		return Convert.ChangeType(Layout.Render(logEvent), Type);
	}
}
