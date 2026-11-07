using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World.Parts;

[Serializable]
[GenerateSerializationPartial]
public class EncounterEntry : IComposite
{
	public string Text = "You notice some strange ruins nearby. Do you want to investigate?";

	public string Zone = "";

	public string secretID = "";

	public string ReplacementText = "";

	public bool Optional = true;

	public bool Enabled = true;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Text);
		Writer.WriteOptimized(Zone);
		Writer.WriteOptimized(secretID);
		Writer.WriteOptimized(ReplacementText);
		Writer.Write(Optional);
		Writer.Write(Enabled);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		Text = Reader.ReadOptimizedString();
		Zone = Reader.ReadOptimizedString();
		secretID = Reader.ReadOptimizedString();
		ReplacementText = Reader.ReadOptimizedString();
		Optional = Reader.ReadBoolean();
		Enabled = Reader.ReadBoolean();
	}

	public EncounterEntry()
	{
	}

	public EncounterEntry(string Text, string Zone, string Replacement, string Secret, bool Optional)
	{
		this.Text = Text;
		this.Zone = Zone;
		ReplacementText = Replacement;
		this.Optional = Optional;
		secretID = Secret;
	}
}
