using System;
using System.ComponentModel;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Filters;

[AttributeUsage(AttributeTargets.Class)]
[Preserve]
public sealed class FilterAttribute : DisplayNameAttribute
{
	public FilterAttribute(string name)
		: base(name)
	{
	}
}
