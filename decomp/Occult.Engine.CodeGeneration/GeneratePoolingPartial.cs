using System;

namespace Occult.Engine.CodeGeneration;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GeneratePoolingPartial : Attribute
{
	public string Reset;

	public string Pool;

	public Type PoolType;

	public int Capacity;
}
