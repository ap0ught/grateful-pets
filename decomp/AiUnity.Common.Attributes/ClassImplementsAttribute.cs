using System;

namespace AiUnity.Common.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ClassImplementsAttribute : ClassTypeConstraintAttribute
{
	public Type InterfaceType { get; private set; }

	public ClassImplementsAttribute(string labelName = null)
		: base(labelName)
	{
	}

	public ClassImplementsAttribute(Type interfaceType, string labelName = null)
		: base(labelName)
	{
		InterfaceType = interfaceType;
	}

	public override bool IsConstraintSatisfied(Type type)
	{
		if (base.IsConstraintSatisfied(type))
		{
			Type[] interfaces = type.GetInterfaces();
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (interfaces[i] == InterfaceType)
				{
					return true;
				}
			}
		}
		return false;
	}
}
