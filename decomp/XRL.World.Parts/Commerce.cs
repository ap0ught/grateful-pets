using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World.Parts;

[Serializable]
[GeneratePoolingPartial]
[GenerateSerializationPartial]
public class Commerce : IPart
{
	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	private static readonly IPartPool CommercePool = new IPartPool();

	public double Value = 1.0;

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override IPartPool Pool => CommercePool;

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override void Reset()
	{
		base.Reset();
		Value = 1.0;
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.Write(Value);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Value = Reader.ReadDouble();
	}
}
