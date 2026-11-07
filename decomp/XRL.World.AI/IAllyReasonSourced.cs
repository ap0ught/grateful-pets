using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World.AI;

[GenerateSerializationPartial]
public abstract class IAllyReasonSourced : IAllyReason
{
	public string Name;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Name);
		Writer.WriteOptimized(Time);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(SerializationReader Reader)
	{
		Name = Reader.ReadOptimizedString();
		Time = Reader.ReadOptimizedInt64();
	}

	public override void Initialize(GameObject Actor, GameObject Source, AllegianceSet Set)
	{
		Name = Source?.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: false, Short: true, BaseOnly: true, IndicateHidden: false, SecondPerson: false);
	}
}
