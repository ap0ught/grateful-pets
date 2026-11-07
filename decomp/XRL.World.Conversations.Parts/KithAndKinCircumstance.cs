using Qud.API;

namespace XRL.World.Conversations.Parts;

public class KithAndKinCircumstance : IKithAndKinPart
{
	public ConversationXMLBlueprint Blueprint;

	public override bool LoadChild(ConversationXMLBlueprint Blueprint)
	{
		this.Blueprint = Blueprint;
		return base.LoadChild(Blueprint);
	}

	public override void Awake()
	{
		Circumstances = KithAndKinGameState.Instance.getKnownFreeClues();
		foreach (JournalObservation circumstance in Circumstances)
		{
			Blueprint.ID = circumstance.ID;
			Blueprint.ReplaceText(circumstance.Text);
			ParentElement.LoadChild(Blueprint);
		}
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == LeftElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeftElementEvent E)
	{
		Circumstance = Circumstances.Find((JournalObservation x) => x.ID == IConversationPart.LastChoiceID);
		return base.HandleEvent(E);
	}
}
