using System;

namespace XRL.World;

[Serializable]
public class DynamicQuestRewardElement_VillageZeroLoot : DynamicQuestRewardElement
{
	public override string getRewardConversationType()
	{
		return null;
	}

	public override string getRewardAcceptQuestText()
	{
		return "Accept Quest - reward: random";
	}

	public override void award()
	{
	}
}
