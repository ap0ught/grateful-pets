using System;
using System.ComponentModel;

namespace AiUnity.NLog.Core.Common;

[AttributeUsage(AttributeTargets.Class)]
public sealed class TargetAttribute : DisplayNameAttribute
{
	public bool IsWrapper { get; set; }

	public bool IsCompound { get; set; }

	public TargetAttribute(string name)
		: base(name)
	{
	}
}
