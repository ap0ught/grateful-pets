using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPlant_BurgeoningHighTier_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public int Tier;

	public override void Init(GameObject target)
	{
		Tier = 10;
		base.Init(target);
	}

	public override string GetDescription()
	{
		return "@they cause@s plants to spontaneously grow in a random nearby area.";
	}

	public override string GetNotification()
	{
		return "Plants burgeon around @them!";
	}

	public override void Apply(GameObject go)
	{
		UnwelcomeGermination.Germinate(go, Tier, 8, friendly: true);
	}
}
