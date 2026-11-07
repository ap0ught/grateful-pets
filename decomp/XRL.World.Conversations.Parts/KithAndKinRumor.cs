namespace XRL.World.Conversations.Parts;

public class KithAndKinRumor : IConversationPart
{
	public ConversationXMLBlueprint Blueprint;

	public HindrenClueRumor Rumor;

	public override bool LoadChild(ConversationXMLBlueprint Blueprint)
	{
		this.Blueprint = Blueprint;
		return base.LoadChild(Blueprint);
	}

	public override void Awake()
	{
		switch (The.Conversation.ID)
		{
		case "HindrenVillager":
			Rumor = KithAndKinGameState.Instance.getRumorForVillagerCategory("*villager");
			break;
		case "FaundrenVillager":
			Rumor = KithAndKinGameState.Instance.getRumorForVillagerCategory("*faundren");
			break;
		case "HindrenScout":
			Rumor = KithAndKinGameState.Instance.getRumorForVillagerCategory("*scout");
			break;
		}
		if (Rumor != null)
		{
			Blueprint.Text = Rumor.text;
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
		if (Rumor != null)
		{
			Rumor.trigger();
			KithAndKinGameState.Instance.foundClue();
		}
		return base.HandleEvent(E);
	}
}
