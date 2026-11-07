using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainArmor_LargeAVBuff_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they gain@s +6 AV for 50 turns.";
	}

	public override string GetNotification()
	{
		return "@they stiffen@s.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainArmor_LargeAVBuff_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainArmor_LargeAVBuff_ProceduralCookingTriggeredActionEffect());
		}
	}
}
