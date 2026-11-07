using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;
using Qud.API;

namespace XRL.World;

[Serializable]
[GenerateSerializationPartial]
public sealed class HindrenClueRumor : IComposite
{
	public string villagerCategory;

	public string text;

	public string secret;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(villagerCategory);
		Writer.WriteOptimized(text);
		Writer.WriteOptimized(secret);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public void Read(SerializationReader Reader)
	{
		villagerCategory = Reader.ReadOptimizedString();
		text = Reader.ReadOptimizedString();
		secret = Reader.ReadOptimizedString();
	}

	public void trigger()
	{
		JournalAPI.RevealObservation(secret, onlyIfNotRevealed: true);
	}
}
