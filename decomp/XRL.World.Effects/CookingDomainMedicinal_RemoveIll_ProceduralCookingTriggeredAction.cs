using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainMedicinal_RemoveIll_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they are cured of the Ill status effect.";
	}

	public override string GetNotification()
	{
		return "@they feel better.";
	}

	public override void Apply(GameObject go)
	{
		go.RemoveEffect<Ill>();
	}
}
