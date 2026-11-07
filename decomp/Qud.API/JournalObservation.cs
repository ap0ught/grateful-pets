using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;
using XRL.UI;
using XRL.World;

namespace Qud.API;

[Serializable]
[GenerateSerializationPartial]
public class JournalObservation : IBaseJournalEntry
{
	public long Time;

	public string Category;

	public string RevealText;

	public bool Rumor;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	[Obsolete]
	public string category
	{
		get
		{
			return Category;
		}
		set
		{
			Category = value;
		}
	}

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

	[Obsolete]
	public string id
	{
		get
		{
			return ID;
		}
		set
		{
			ID = value;
		}
	}

	[Obsolete]
	public bool initCapAsFragment
	{
		get
		{
			return Rumor;
		}
		set
		{
			Rumor = value;
		}
	}

	[Obsolete]
	public string additionalRevealText
	{
		get
		{
			return RevealText;
		}
		set
		{
			RevealText = value;
		}
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Time);
		Writer.WriteOptimized(Category);
		Writer.WriteOptimized(RevealText);
		Writer.Write(Rumor);
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
		Category = Reader.ReadOptimizedString();
		RevealText = Reader.ReadOptimizedString();
		Rumor = Reader.ReadBoolean();
		ID = Reader.ReadOptimizedString();
		History = Reader.ReadOptimizedString();
		Text = Reader.ReadOptimizedString();
		LearnedFrom = Reader.ReadOptimizedString();
		Weight = Reader.ReadOptimizedInt32();
		Revealed = Reader.ReadBoolean();
		Tradable = Reader.ReadBoolean();
		Attributes = Reader.ReadList<string>();
	}

	public override void Reveal(string LearnedFrom = null, bool Silent = false)
	{
		if (!Revealed)
		{
			base.Reveal(LearnedFrom, Silent);
			Updated();
			IBaseJournalEntry.SB.Clear().Append("{{W|").Append(RevealText.IsNullOrEmpty() ? Text : RevealText)
				.Append("}}\n\nYou note this piece of information in the {{W|")
				.Append(JournalScreen.STR_OBSERVATIONS)
				.Append("}} section of your journal.");
			IBaseJournalEntry.DisplayMessage(IBaseJournalEntry.SB.ToString());
		}
	}
}
