using System;

namespace Occult.Engine.CodeGeneration;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GenerateSerializationPartial : Attribute
{
	public string PreWrite;

	public string Write;

	public string PostWrite;

	public string PreRead;

	public string Read;

	public string PostRead;

	public bool? InvokeBase;
}
