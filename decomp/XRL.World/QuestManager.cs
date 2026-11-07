using System;

namespace XRL.World;

[Serializable]
public class QuestManager : IPart
{
	public string MyQuestID;

	public virtual void AfterQuestAdded()
	{
	}

	public virtual void OnQuestAdded()
	{
	}

	public virtual void OnStepComplete(string StepName)
	{
	}

	public virtual void OnQuestComplete()
	{
	}

	public virtual GameObject GetQuestInfluencer()
	{
		return null;
	}

	public virtual string GetQuestZoneID()
	{
		return null;
	}
}
