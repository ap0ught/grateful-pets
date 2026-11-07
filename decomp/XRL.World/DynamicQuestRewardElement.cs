using System;

namespace XRL.World;

[Serializable]
public abstract class DynamicQuestRewardElement : IComposite
{
	public abstract void award();

	public abstract string getRewardConversationType();

	public abstract string getRewardAcceptQuestText();
}
