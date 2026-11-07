using System;
using XRL.Serialization;

namespace Occult.Engine.CodeGeneration;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GeneratedBootPartialAttribute : GeneratedPartial
{
	public static ulong GetHashCode(Type Type)
	{
		return FastSerialization.GetFieldSaveHash(Type);
	}
}
