using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;
using XRL.World;

namespace Qud.API;

[Serializable]
[GenerateSerializationPartial]
public class JournalGeneralNote : IBaseJournalEntry
{
	public long Time;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	[Obsolete]
	public long time
	{
		get
		{
			return Time;
		}
		set
		{
			Time = value;
		}
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Time);
		Writer.WriteOptimized(ID);
		Writer.WriteOptimized(History);
		Writer.WriteOptimized(Text);
		Writer.WriteOptimized(LearnedFrom);
		Writer.WriteOptimized(Weight);
		Writer.Write(Revealed);
		Writer.Write(Tradable);
		Writer.Write(Attributes);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(SerializationReader Reader)
	{
		Time = Reader.ReadOptimizedInt64();
		ID = Reader.ReadOptimizedString();
		History = Reader.ReadOptimizedString();
		Text = Reader.ReadOptimizedString();
		LearnedFrom = Reader.ReadOptimizedString();
		Weight = Reader.ReadOptimizedInt32();
		Revealed = Reader.ReadBoolean();
		Tradable = Reader.ReadBoolean();
		Attributes = Reader.ReadList<string>();
	}
}
