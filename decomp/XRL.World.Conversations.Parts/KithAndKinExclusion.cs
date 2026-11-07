namespace XRL.World.Conversations.Parts;

public class KithAndKinExclusion : IKithAndKinPart
{
	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != EnteredElementEvent.ID && ID != LeftElementEvent.ID && ID != GetTextElementEvent.ID)
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeftElementEvent E)
	{
		if (!Eliminated.Contains(Thief))
		{
			Eliminated.Add(Thief);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTextElementEvent E)
	{
		if (!Eliminated.Contains(Thief))
		{
			E.Selected = E.Texts.Find((ConversationText x) => x.ID == Circumstance?.ID) ?? E.Selected;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		E.Text.Replace("=thief.name=", ThiefName);
		return base.HandleEvent(E);
	}
}
