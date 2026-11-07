using System;
using System.ComponentModel;

namespace AiUnity.NLog.Core.Layouts;

[AttributeUsage(AttributeTargets.Class)]
public sealed class LayoutAttribute : DisplayNameAttribute
{
	public LayoutAttribute(string name)
		: base(name)
	{
	}
}
