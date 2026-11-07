using Qud.API;

namespace XRL.World.Conversations.Parts;

public class SecretHandler : IConversationPart
{
	public string SecretID;

	public bool Require;

	public bool Override;

	public SecretHandler()
	{
	}

	public SecretHandler(string SecretID, bool Require = false, bool Override = false)
		: this()
	{
		this.SecretID = SecretID;
		this.Require = Require;
		this.Override = Override;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && (ID != IsElementVisibleEvent.ID || Require || Override) && (ID != EnterElementEvent.ID || !Require) && ID != ColorTextEvent.ID)
		{
			return ID == DisplayTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsElementVisibleEvent E)
	{
		if (!Require && !Override)
		{
			return JournalAPI.HasNote(SecretID);
		}
		return true;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		if (Require)
		{
			return JournalAPI.HasNote(SecretID);
		}
		return true;
	}

	public override bool HandleEvent(ColorTextEvent E)
	{
		if (Require && !JournalAPI.HasNote(SecretID))
		{
			E.Color = "K";
		}
		else if (E.Element is Choice { Visited: not false })
		{
			E.Color = "g";
		}
		else
		{
			E.Color = "M";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DisplayTextEvent E)
	{
		E.Text.Insert(0, "<!> ");
		return base.HandleEvent(E);
	}
}
