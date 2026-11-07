using System.Text;

namespace XRL.World.Conversations.Parts;

/// <summary>Appends or prepends a text that can be either spoken or unspoken to the element.</summary>
public class TextInsert : IPredicatePart
{
	public string Text;

	public bool Prepend;

	public bool Spoken = true;

	public int NewLines;

	public bool IsKeyboardHotkey;

	/// <summary>Require one predicate to match rather than all.</summary>
	public bool AnyPredicate;

	public bool Append
	{
		get
		{
			return !Prepend;
		}
		set
		{
			Prepend = !value;
		}
	}

	public void Insert(StringBuilder SB)
	{
		if (Text.IsNullOrEmpty() || (IsKeyboardHotkey && !CapabilityManager.AllowKeyboardHotkeys) || !Check(AnyPredicate))
		{
			return;
		}
		if (IsKeyboardHotkey)
		{
			Text = Text.Replace("{CmdStartTrade}", ControlManager.getCommandInputFormatted("CmdStartTrade"));
		}
		Text = Text.GetRandomSubstring('~');
		if (Prepend)
		{
			if (!Text.EndsWith("\n"))
			{
				SB.Insert(0, "\n", NewLines);
			}
			SB.Insert(0, Text);
		}
		else
		{
			if (!SB.EndsWith('\n'))
			{
				SB.Append('\n', NewLines);
			}
			SB.Append(Text);
		}
	}

	public override void LoadText(string Text)
	{
		this.Text = Text;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && (ID != PrepareTextEvent.ID || !Spoken))
		{
			if (ID == DisplayTextEvent.ID)
			{
				return !Spoken;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		Insert(E.Text);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DisplayTextEvent E)
	{
		Insert(E.Text);
		return base.HandleEvent(E);
	}
}
