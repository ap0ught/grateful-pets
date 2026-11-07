using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[Serializable]
[GenerateSerializationPartial]
public class WorshipTracking : IComposite
{
	public string Name;

	public string Faction;

	public bool Devoted;

	public int Times;

	public long First;

	public long Last;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Name);
		Writer.WriteOptimized(Faction);
		Writer.Write(Devoted);
		Writer.WriteOptimized(Times);
		Writer.WriteOptimized(First);
		Writer.WriteOptimized(Last);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		Name = Reader.ReadOptimizedString();
		Faction = Reader.ReadOptimizedString();
		Devoted = Reader.ReadBoolean();
		Times = Reader.ReadOptimizedInt32();
		First = Reader.ReadOptimizedInt64();
		Last = Reader.ReadOptimizedInt64();
	}

	public override string ToString()
	{
		return "Name: " + Name + "\nFaction: " + Faction + "\nDevoted: " + Devoted + "\nTimes: " + Times + "\nFirst: " + First + "\nLast: " + Last;
	}
}
