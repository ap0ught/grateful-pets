using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Class)]
public class HasOptionFlagUpdate : Attribute
{
	public string Prefix;

	/// <summary>Treat all public static fields on this class as flag fields.</summary>
	public bool FieldFlags;
}
