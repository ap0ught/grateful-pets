using XRL.Language;

namespace XRL.World.Conversations.Parts;

public class TextFilter : IConversationPart
{
	public string FilterID;

	public string Extras;

	public bool FormattingProtect = true;

	public TextFilter()
	{
		Priority = -1000;
	}

	public TextFilter(string ID, string Extras = null)
		: this()
	{
		FilterID = ID;
		this.Extras = Extras;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == PrepareTextLateEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextLateEvent E)
	{
		if (!FilterID.IsNullOrEmpty())
		{
			E.Text = TextFilters.Filter(E.Text, FilterID, Extras, FormattingProtect);
		}
		return base.HandleEvent(E);
	}
}
