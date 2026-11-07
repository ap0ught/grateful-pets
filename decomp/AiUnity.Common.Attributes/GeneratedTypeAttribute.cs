using System;

namespace AiUnity.Common.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class GeneratedTypeAttribute : Attribute
{
	public readonly string CreationDate;

	public GeneratedTypeAttribute(string creationDate)
	{
		CreationDate = creationDate;
	}
}
