using Qud.API;

namespace XRL.World.Conversations.Parts;

public class KithAndKinIfExcluded : IKithAndKinPart
{
	public string Target;

	public bool IsExcluded(JournalObservation Motive)
	{
		if (!Motive.TryGetAttribute("motive:", out var Value))
		{
			Value = "keh";
		}
		if (Eliminated.Contains(Value))
		{
			return true;
		}
		IConversationElement element = ParentElement.Parent.GetElement("KithAndKinExclusion", Circumstance?.ID);
		if (element == null || !element.Attributes.TryGetValue("Thief", out var value))
		{
			return false;
		}
		return IKithAndKinPart.KeyOf(Value) == value;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == GetTargetElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTargetElementEvent E)
	{
		JournalObservation journalObservation = Motives.Find((JournalObservation x) => x.ID == E.Element.ID);
		if (journalObservation != null && IsExcluded(journalObservation))
		{
			E.Target = Target;
		}
		return base.HandleEvent(E);
	}
}
