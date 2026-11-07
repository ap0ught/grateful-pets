using System;
using System.ComponentModel;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[Preserve]
public sealed class AmbientPropertyAttribute : DisplayNameAttribute
{
	public AmbientPropertyAttribute(string name)
		: base(name)
	{
	}
}
