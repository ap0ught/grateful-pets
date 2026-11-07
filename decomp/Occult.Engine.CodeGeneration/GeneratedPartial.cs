using System;
using System.Diagnostics;

namespace Occult.Engine.CodeGeneration;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[Conditional("UNITY_EDITOR")]
public class GeneratedPartial : Attribute
{
	public ulong Hash;
}
