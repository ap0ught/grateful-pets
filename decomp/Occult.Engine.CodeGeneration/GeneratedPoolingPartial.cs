using System;
using System.Reflection;

namespace Occult.Engine.CodeGeneration;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GeneratedPoolingPartial : GeneratedPartial
{
	public static ulong GetHashCode(Type Type)
	{
		return Type.GetStableHashCode64(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, FieldAttributes.Static | FieldAttributes.Literal);
	}
}
