using System;

namespace XRL.World;

[Serializable]
public class DynamicQuestRewardElement_VillageZeroMainQuestHook : DynamicQuestRewardElement
{
	public override string getRewardConversationType()
	{
		return "VillageZeroMainQuest";
	}

	public override string getRewardAcceptQuestText()
	{
		return "Accept Quest - reward: recoiler and disk";
	}

	public override void award()
	{
	}
}
