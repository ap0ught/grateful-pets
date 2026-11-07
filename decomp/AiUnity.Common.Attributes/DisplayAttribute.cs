using System;
using System.ComponentModel;

namespace AiUnity.Common.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DisplayAttribute : DisplayNameAttribute
{
	public bool Advanced { get; private set; }

	public int Order { get; private set; }

	public string Tooltip { get; private set; }

	public DisplayAttribute(string name, string toolTip, bool advanced = false, int order = 0)
		: base(name)
	{
		Advanced = advanced;
		Tooltip = toolTip;
		Order = order;
	}
}
