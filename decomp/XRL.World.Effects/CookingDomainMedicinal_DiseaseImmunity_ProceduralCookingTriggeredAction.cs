using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainMedicinal_DiseaseImmunity_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they can't acquire new diseases for 6 hours.";
	}

	public override string GetNotification()
	{
		return "@they become impervious to disease.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainMedicinal_DiseaseImmunity_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainMedicinal_DiseaseImmunity_ProceduralCookingTriggeredActionEffect());
		}
	}
}
