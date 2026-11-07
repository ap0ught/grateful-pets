using HistoryKit;
using Qud.API;
using XRL.World.Quests;

namespace XRL.World.Conversations.Parts;

public class ThahDynamicCommentary : IConversationPart
{
	public ConversationXMLBlueprint ChoiceBlueprint;

	public ConversationXMLBlueprint ChoiceTextBlueprint;

	public ConversationXMLBlueprint TextBlueprint;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != EnterElementEvent.ID)
		{
			return ID == IsElementVisibleEvent.ID;
		}
		return true;
	}

	public override bool LoadChild(ConversationXMLBlueprint Blueprint)
	{
		if (Blueprint.Name == "Choice")
		{
			ChoiceBlueprint = Blueprint;
			ChoiceTextBlueprint = Blueprint.GetChild("Text");
		}
		else if (Blueprint.Name == "Text")
		{
			TextBlueprint = Blueprint;
		}
		return base.LoadChild(Blueprint);
	}

	public override void Awake()
	{
		LandingPadsSystem system = The.Game.GetSystem<LandingPadsSystem>();
		if (system == null)
		{
			return;
		}
		foreach (string candidateFaction in system.candidateFactions)
		{
			if (ParentElement.GetElementByID(candidateFaction) == null)
			{
				HistoricEntitySnapshot villageSnapshot = HistoryAPI.GetVillageSnapshot(candidateFaction);
				if (villageSnapshot != null)
				{
					string text = TextBlueprint.Text;
					TextBlueprint.ID = candidateFaction;
					TextBlueprint.Text = HistoryAPI.ExpandVillageText(text, null, villageSnapshot);
					TextBlueprint.Attributes["IfLastChoice"] = candidateFaction;
					ParentElement.LoadChild(TextBlueprint);
					TextBlueprint.Text = text;
					text = ChoiceTextBlueprint.Text;
					ChoiceBlueprint.ID = candidateFaction;
					ChoiceTextBlueprint.Text = HistoryAPI.ExpandVillageText(text, null, villageSnapshot);
					ParentElement.LoadChild(ChoiceBlueprint);
					ChoiceTextBlueprint.Text = text;
				}
			}
		}
	}
}
