using System;

namespace AiUnity.Common.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ClassExtendsAttribute : ClassTypeConstraintAttribute
{
	public Type BaseType { get; private set; }

	public override bool IsConstraintSatisfied(Type type)
	{
		if (base.IsConstraintSatisfied(type) && BaseType.IsAssignableFrom(type))
		{
			return type != BaseType;
		}
		return false;
	}

	public ClassExtendsAttribute(Type baseType, string labelName = null, string tooltip = null)
		: base(labelName, tooltip)
	{
		BaseType = baseType;
	}
}
