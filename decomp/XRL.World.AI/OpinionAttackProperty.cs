using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World.AI;

[GenerateSerializationPartial]
public class OpinionAttackProperty : IOpinionCombat
{
	public string What;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	public override int BaseValue => -25;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(What);
		Writer.Write(Magnitude);
		Writer.WriteOptimized(Time);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(SerializationReader Reader)
	{
		What = Reader.ReadOptimizedString();
		Magnitude = Reader.ReadSingle();
		Time = Reader.ReadOptimizedInt64();
	}

	public override void Initialize(GameObject Actor, GameObject Subject, GameObject Object)
	{
		What = Object?.Render?.DisplayName;
	}

	public override string GetText(GameObject Actor)
	{
		return "Attacked my " + (What ?? "things") + ".";
	}
}
