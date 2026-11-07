using System;
using System.CodeDom.Compiler;
using HistoryKit;
using Occult.Engine.CodeGeneration;
using XRL;
using XRL.UI;
using XRL.World;

namespace Qud.API;

[Serializable]
[GenerateSerializationPartial]
public class JournalSultanNote : IBaseJournalEntry
{
	public string SultanID;

	public long EventID;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	[Obsolete]
	public string sultan
	{
		get
		{
			return SultanID;
		}
		set
		{
			SultanID = value;
		}
	}

	[Obsolete]
	public long linkId
	{
		get
		{
			return EventID;
		}
		set
		{
			EventID = value;
		}
	}

	[Obsolete]
	public long eventId
	{
		get
		{
			return EventID;
		}
		set
		{
			EventID = value;
		}
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(SultanID);
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
		SultanID = Reader.ReadOptimizedString();
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

	public override bool Forgettable()
	{
		HistoricEvent historicEvent = HistoryAPI.GetEvent(EventID);
		if (historicEvent == null)
		{
			return true;
		}
		if (historicEvent.HasEventProperty("revealsRegion"))
		{
			return false;
		}
		if (historicEvent.HasEventProperty("revealsItem"))
		{
			return false;
		}
		if (historicEvent.HasEventProperty("revealsItemLocation"))
		{
			return false;
		}
		return true;
	}

	public override void Reveal(string LearnedFrom = null, bool Silent = false)
	{
		if (Revealed)
		{
			return;
		}
		base.Reveal(LearnedFrom, Silent);
		Updated();
		if (!Silent)
		{
			IBaseJournalEntry.DisplayMessage("You note this piece of information in the {{W|" + JournalScreen.GetSultansDisplayName() + " > " + HistoryAPI.GetEntityName(SultanID) + "}} section of your journal.");
		}
		bool flag = true;
		foreach (JournalSultanNote sultanNote in JournalAPI.SultanNotes)
		{
			if (sultanNote.SultanID == SultanID && !sultanNote.Revealed)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			Achievement.LEARN_ONE_SULTAN_HISTORY.Unlock();
		}
	}
}
