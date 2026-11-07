using System;
using System.ComponentModel;

namespace AiUnity.NLog.Core.Conditions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ConditionMethodAttribute : DisplayNameAttribute
{
	public ConditionMethodAttribute(string name)
		: base(name)
	{
	}
}
