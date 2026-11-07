using System;

namespace XRL.World;

[Serializable]
public class DynamicQuestRewardElement_Reputation : DynamicQuestRewardElement
{
	public string faction;

	public int amount;

	public DynamicQuestRewardElement_Reputation()
	{
	}

	public DynamicQuestRewardElement_Reputation(string faction, int amount)
		: this()
	{
		this.faction = faction;
		this.amount = amount;
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
		The.Game.PlayerReputation.Modify(faction, amount, "Quest");
	}
}
