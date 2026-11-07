using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[Serializable]
[GenerateSerializationPartial]
public class MemorialTracking : IComposite
{
	public string Name;

	public string Faction;

	public string Eulogy;

	public string Reason;

	public string ThirdPersonReason;

	public string ID;

	public string Blueprint;

	public string Location;

	public long Queued;

	public long Performed;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Name);
		Writer.WriteOptimized(Faction);
		Writer.WriteOptimized(Eulogy);
		Writer.WriteOptimized(Reason);
		Writer.WriteOptimized(ThirdPersonReason);
		Writer.WriteOptimized(ID);
		Writer.WriteOptimized(Blueprint);
		Writer.WriteOptimized(Location);
		Writer.WriteOptimized(Queued);
		Writer.WriteOptimized(Performed);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		Name = Reader.ReadOptimizedString();
		Faction = Reader.ReadOptimizedString();
		Eulogy = Reader.ReadOptimizedString();
		Reason = Reader.ReadOptimizedString();
		ThirdPersonReason = Reader.ReadOptimizedString();
		ID = Reader.ReadOptimizedString();
		Blueprint = Reader.ReadOptimizedString();
		Location = Reader.ReadOptimizedString();
		Queued = Reader.ReadOptimizedInt64();
		Performed = Reader.ReadOptimizedInt64();
	}

	public override string ToString()
	{
		return "Name: " + Name + "\nFaction: " + Faction + "\nEulogy: " + Eulogy + "\nReason: " + Reason + "\nThirdPersonReason: " + ThirdPersonReason + "\nID: " + ID + "\nBlueprint: " + Blueprint + "\nLocation: " + Location + "\nQueued: " + Queued + "\nPerformed: " + Performed;
	}
}
