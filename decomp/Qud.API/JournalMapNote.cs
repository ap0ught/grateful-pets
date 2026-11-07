using System;
using System.CodeDom.Compiler;
using Genkit;
using Occult.Engine.CodeGeneration;
using XRL;
using XRL.Language;
using XRL.UI;
using XRL.World;

namespace Qud.API;

[Serializable]
[GenerateSerializationPartial]
public class JournalMapNote : IBaseJournalEntry
{
	public string WorldID;

	public int ParasangX;

	public int ParasangY;

	public int ZoneX;

	public int ZoneY;

	public int ZoneZ;

	public string Category;

	public long Time;

	public bool Tracked;

	[NonSerialized]
	private string _ZoneID;

	private string _displayText;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	public string ZoneID
	{
		get
		{
			if (_ZoneID == null)
			{
				IBaseJournalEntry.SB.Clear().Append(WorldID ?? "NULL").Append('.')
					.Append(ParasangX)
					.Append('.')
					.Append(ParasangY)
					.Append('.')
					.Append(ZoneX)
					.Append('.')
					.Append(ZoneY)
					.Append('.')
					.Append(ZoneZ);
				_ZoneID = IBaseJournalEntry.SB.ToString();
			}
			return _ZoneID;
		}
		set
		{
			_ZoneID = value;
			XRL.World.ZoneID.Parse(value, out WorldID, out ParasangX, out ParasangY, out ZoneX, out ZoneY, out ZoneZ);
		}
	}

	public Location2D Location => Location2D.Get(ParasangX, ParasangY);

	public Location2D ResolvedLocation => Location2D.Get(ResolvedX, ResolvedY);

	public int ResolvedX => ParasangX * 3 + ZoneX;

	public int ResolvedY => ParasangY * 3 + ZoneY;

	public bool Visited
	{
		get
		{
			if (ZoneID != null)
			{
				return The.ZoneManager.VisitedTime.ContainsKey(ZoneID);
			}
			return false;
		}
	}

	public long LastVisit
	{
		get
		{
			if (ZoneID == null || !The.ZoneManager.VisitedTime.TryGetValue(ZoneID, out var value))
			{
				return -1L;
			}
			return value;
		}
	}

	[Obsolete]
	public Location2D location => Location2D.Get(ResolvedX, ResolvedY);

	[Obsolete]
	public int x => ParasangX * 3 + ZoneX;

	[Obsolete]
	public int y => ParasangY * 3 + ZoneY;

	[Obsolete("use ZoneID")]
	public string zoneid
	{
		get
		{
			return ZoneID;
		}
		set
		{
			ZoneID = value;
		}
	}

	[Obsolete("use WorldID")]
	public string w
	{
		get
		{
			return WorldID;
		}
		set
		{
			WorldID = value;
		}
	}

	[Obsolete("use ParasangX")]
	public int wx
	{
		get
		{
			return ParasangX;
		}
		set
		{
			ParasangX = value;
		}
	}

	[Obsolete("use ParasangY")]
	public int wy
	{
		get
		{
			return ParasangY;
		}
		set
		{
			ParasangY = value;
		}
	}

	[Obsolete("use ZoneX")]
	public int cx
	{
		get
		{
			return ZoneX;
		}
		set
		{
			ZoneX = value;
		}
	}

	[Obsolete("use ZoneY")]
	public int cy
	{
		get
		{
			return ZoneY;
		}
		set
		{
			ZoneY = value;
		}
	}

	[Obsolete("use ZoneZ")]
	public int cz
	{
		get
		{
			return ZoneZ;
		}
		set
		{
			ZoneZ = value;
		}
	}

	[Obsolete("use Category")]
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

	[Obsolete("always false")]
	public bool shown
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	[Obsolete]
	public bool tracked
	{
		get
		{
			return tracked;
		}
		set
		{
			tracked = value;
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

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(WorldID);
		Writer.WriteOptimized(ParasangX);
		Writer.WriteOptimized(ParasangY);
		Writer.WriteOptimized(ZoneX);
		Writer.WriteOptimized(ZoneY);
		Writer.WriteOptimized(ZoneZ);
		Writer.WriteOptimized(Category);
		Writer.WriteOptimized(Time);
		Writer.Write(Tracked);
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
		WorldID = Reader.ReadOptimizedString();
		ParasangX = Reader.ReadOptimizedInt32();
		ParasangY = Reader.ReadOptimizedInt32();
		ZoneX = Reader.ReadOptimizedInt32();
		ZoneY = Reader.ReadOptimizedInt32();
		ZoneZ = Reader.ReadOptimizedInt32();
		Category = Reader.ReadOptimizedString();
		Time = Reader.ReadOptimizedInt64();
		Tracked = Reader.ReadBoolean();
		ID = Reader.ReadOptimizedString();
		History = Reader.ReadOptimizedString();
		Text = Reader.ReadOptimizedString();
		LearnedFrom = Reader.ReadOptimizedString();
		Weight = Reader.ReadOptimizedInt32();
		Revealed = Reader.ReadBoolean();
		Tradable = Reader.ReadBoolean();
		Attributes = Reader.ReadList<string>();
	}

	public bool SameAs(JournalMapNote Note)
	{
		if (Note != null && WorldID == Note.WorldID && ParasangX == Note.ParasangX && ParasangY == Note.ParasangY && ZoneX == Note.ZoneX && ZoneY == Note.ZoneY && ZoneZ == Note.ZoneZ && Category == Note.Category && Text == Note.Text)
		{
			return ID == Note.ID;
		}
		return false;
	}

	public override bool Forgettable()
	{
		return false;
	}

	public override bool CanSell()
	{
		if (base.CanSell())
		{
			return The.ActiveZone.ZoneID != ZoneID;
		}
		return false;
	}

	public override string GetShareText()
	{
		return "The location of " + Grammar.LowerArticles(GetShortText());
	}

	public override string GetDisplayText()
	{
		if (_displayText == null)
		{
			IBaseJournalEntry.SB.Clear().Append(Text);
			IBaseJournalEntry.SB.Compound(LoreGenerator.GenerateLandmarkDirectionsTo(ZoneID), '\n');
			long lastVisit = LastVisit;
			if (lastVisit > 0)
			{
				IBaseJournalEntry.SB.Compound("Last visited on the ", '\n').Append(Calendar.GetDay(lastVisit)).Append(" of ")
					.Append(Calendar.GetMonth(lastVisit));
			}
			if (Options.DebugInternals)
			{
				IBaseJournalEntry.SB.Compound("\n{{internals|", '\n').Append('Ã').Append(' ');
				for (int i = 0; i < Attributes.Count; i++)
				{
					if (i != 0)
					{
						IBaseJournalEntry.SB.Append(", ");
					}
					IBaseJournalEntry.SB.Append(Attributes[i]);
				}
				if (History.Length > 0)
				{
					IBaseJournalEntry.SB.Compound(History, '\n');
				}
				IBaseJournalEntry.SB.Append("}}");
			}
			_displayText = IBaseJournalEntry.SB.ToString();
		}
		return _displayText;
	}

	public override void Reveal(string LearnedFrom = null, bool Silent = false)
	{
		if (!Revealed)
		{
			JournalAPI._mapNoteCategories = null;
			base.Reveal(LearnedFrom, Silent);
			string text = null;
			Time = Calendar.TotalTimeTicks;
			if (!JournalAPI.MapNotes.Contains(this))
			{
				JournalAPI.MapNotes.Add(this);
			}
			if (Category == "Artifacts")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(Text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Artifacts}} section of your journal.";
			}
			else if (Category == "Historic Sites")
			{
				text = "You note the location of " + Grammar.MakeTitleCaseWithArticle(Text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Historic Sites}} section of your journal.";
			}
			else if (Category == "Lairs")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(Text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Lairs}} section of your journal.";
			}
			else if (Category == "Merchants")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(Text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Merchants}} section of your journal.";
			}
			else if (Category == "Natural Features")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(Text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Natural Features}} section of your journal.";
			}
			else if (Category == "Oddities")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(Text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Oddities}} section of your journal.";
			}
			else if (Category == "Baetyls")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(Text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Baetyls}} section of your journal.";
			}
			else if (Category == "Ruins")
			{
				string text2 = ((Text == "some forgotten ruins") ? Grammar.InitLower(Text) : Grammar.MakeTitleCaseWithArticle(Text));
				text = "You note the location of " + text2 + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Ruins}} section of your journal.";
			}
			else if (Category == "Settlements")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(Text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Settlements}} section of your journal.";
			}
			else if (Category == "Named Locations")
			{
				text = "You note the location of " + Text + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Named Locations}} section of your journal.";
			}
			else if (Category == "Ruins with Becoming Nooks")
			{
				string text3 = ((Text == "some forgotten ruins") ? Grammar.InitLower(Text) : Grammar.MakeTitleCaseWithArticle(Text));
				text = "You note the location of " + text3 + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Ruins with Becoming Nooks}} section of your journal.";
			}
			else
			{
				text = "tell support@freeholdgames.com unknown location category: " + Category;
			}
			if (Category != "Miscellaneous" && !Silent)
			{
				IBaseJournalEntry.DisplayMessage(text);
			}
		}
	}
}
