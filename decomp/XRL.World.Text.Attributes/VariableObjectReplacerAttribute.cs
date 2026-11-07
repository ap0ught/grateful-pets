using System;

namespace XRL.World.Text.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class VariableObjectReplacerAttribute : VariableReplacerAttribute
{
	public VariableObjectReplacerAttribute()
	{
		Capitalization = true;
	}

	public VariableObjectReplacerAttribute(params string[] Keys)
		: this()
	{
		base.Keys = Keys;
	}
}
