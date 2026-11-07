using System;
using System.ComponentModel;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[AttributeUsage(AttributeTargets.Class)]
[Preserve]
public sealed class LayoutRendererAttribute : DisplayNameAttribute
{
	public bool IsWrapper { get; set; }

	public LayoutRendererAttribute(string name, bool isWrapper = false)
		: base(name)
	{
		IsWrapper = isWrapper;
	}
}
