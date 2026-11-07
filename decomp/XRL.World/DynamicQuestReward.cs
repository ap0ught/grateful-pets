using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[Serializable]
[GenerateSerializationPartial]
public class DynamicQuestReward : IComposite
{
	public string special;

	public int StepXP;

	public List<DynamicQuestRewardElement> rewards = new List<DynamicQuestRewardElement>();

	public List<DynamicQuestRewardElement> postrewards = new List<DynamicQuestRewardElement>();

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(special);
		Writer.WriteOptimized(StepXP);
		Writer.WriteComposite(rewards);
		Writer.WriteComposite(postrewards);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		special = Reader.ReadOptimizedString();
		StepXP = Reader.ReadOptimizedInt32();
		rewards = Reader.ReadCompositeList<DynamicQuestRewardElement>();
		postrewards = Reader.ReadCompositeList<DynamicQuestRewardElement>();
	}

	public void award()
	{
		foreach (DynamicQuestRewardElement reward in rewards)
		{
			reward.award();
		}
	}

	public void postaward()
	{
		foreach (DynamicQuestRewardElement postreward in postrewards)
		{
			postreward.award();
		}
	}

	public string getRewardConversationType()
	{
		foreach (DynamicQuestRewardElement reward in rewards)
		{
			string rewardConversationType = reward.getRewardConversationType();
			if (!string.IsNullOrEmpty(rewardConversationType))
			{
				return rewardConversationType;
			}
		}
		return "Choice";
	}

	public string getRewardAcceptQuestText()
	{
		foreach (DynamicQuestRewardElement reward in rewards)
		{
			string rewardAcceptQuestText = reward.getRewardAcceptQuestText();
			if (!string.IsNullOrEmpty(rewardAcceptQuestText))
			{
				return rewardAcceptQuestText;
			}
		}
		return null;
	}
}
