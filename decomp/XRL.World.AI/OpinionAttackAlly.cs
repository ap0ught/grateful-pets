using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World.AI;

[GenerateSerializationPartial]
public class OpinionAttackAlly : IOpinionCombat
{
	public string Ally;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	public override int BaseValue => -75;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Ally);
		Writer.Write(Magnitude);
		Writer.WriteOptimized(Time);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(SerializationReader Reader)
	{
		Ally = Reader.ReadOptimizedString();
		Magnitude = Reader.ReadSingle();
		Time = Reader.ReadOptimizedInt64();
	}

	public override void Initialize(GameObject Actor, GameObject Subject, GameObject Object)
	{
		if (Object.HasProperName)
		{
			Ally = Object.Render.DisplayName;
		}
		else
		{
			Ally = null;
		}
	}

	public override string GetText(GameObject Actor)
	{
		return "Attacked " + (Ally ?? "my ally") + ".";
	}
}
