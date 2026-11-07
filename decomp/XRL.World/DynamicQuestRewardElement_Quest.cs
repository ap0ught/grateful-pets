using System;
using XRL.Core;

namespace XRL.World;

[Serializable]
public class DynamicQuestRewardElement_Quest : DynamicQuestRewardElement
{
	public string questID;

	public DynamicQuestRewardElement_Quest()
	{
	}

	public DynamicQuestRewardElement_Quest(string questID)
	{
		this.questID = questID;
	}

	public override string getRewardConversationType()
	{
		return null;
	}

	public override string getRewardAcceptQuestText()
	{
		return null;
	}

	public override void award()
	{
		XRLCore.Core.Game.StartQuest(questID);
	}
}
