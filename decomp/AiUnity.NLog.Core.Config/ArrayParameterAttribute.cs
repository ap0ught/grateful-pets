using System;

namespace AiUnity.NLog.Core.Config;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ArrayParameterAttribute : Attribute
{
	public Type ItemType { get; private set; }

	public string ElementName { get; private set; }

	public ArrayParameterAttribute(Type itemType, string elementName)
	{
		ItemType = itemType;
		ElementName = elementName;
	}
}
