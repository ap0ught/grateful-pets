using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class InsertRandomBookLine : IConversationPart
{
	public string BookID;

	public bool Prepend;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		if (BookUI.Books.TryGetValue(BookID, out var value) && value.Count > 0)
		{
			string randomElement = value[0].Lines.GetRandomElement();
			if (!randomElement.IsNullOrEmpty())
			{
				if (Prepend)
				{
					E.Text.Insert(0, "\n\n").Insert(0, randomElement);
				}
				else
				{
					E.Text.Append("\n\n").Append(randomElement);
				}
			}
		}
		return base.HandleEvent(E);
	}
}
