using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;
using XRL.UI;
using XRL.World;

namespace Qud.API;

[Serializable]
[GenerateSerializationPartial]
public class JournalVillageNote : IBaseJournalEntry
{
	public string VillageID;

	public long EventID;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	[Obsolete]
	public string villageID
	{
		get
		{
			return VillageID;
		}
		set
		{
			VillageID = value;
		}
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(VillageID);
		Writer.WriteOptimized(EventID);
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
		VillageID = Reader.ReadOptimizedString();
		EventID = Reader.ReadOptimizedInt64();
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
			if (!Silent)
			{
				IBaseJournalEntry.DisplayMessage("You note this piece of information in the {{W|" + JournalScreen.STR_VILLAGES + " > " + HistoryAPI.GetEntityName(VillageID) + "}} section of your journal.");
			}
		}
	}

	public override string GetShortText()
	{
		int num = Text.LastIndexOf('|');
		if (num == -1)
		{
			return Text;
		}
		return Text.Substring(0, num);
	}

	public override string GetDisplayText()
	{
		string shortText = GetShortText();
		if (History.Length > 0)
		{
			return shortText + "\n" + History;
		}
		return shortText;
	}

	[Obsolete]
	public long getEventId()
	{
		return EventID;
	}
}
