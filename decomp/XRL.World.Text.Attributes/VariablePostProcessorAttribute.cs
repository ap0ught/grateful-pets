using System;

namespace XRL.World.Text.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class VariablePostProcessorAttribute : VariableReplacerAttribute
{
	public VariablePostProcessorAttribute()
	{
	}

	public VariablePostProcessorAttribute(params string[] Keys)
		: this()
	{
		base.Keys = Keys;
	}
}
