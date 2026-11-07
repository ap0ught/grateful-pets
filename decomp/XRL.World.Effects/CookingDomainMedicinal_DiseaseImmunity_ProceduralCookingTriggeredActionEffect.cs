using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainMedicinal_DiseaseImmunity_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingEffect
{
	public CookingDomainMedicinal_DiseaseImmunity_ProceduralCookingTriggeredActionEffect()
	{
		Duration = 300;
	}

	public override string GetDetails()
	{
		return "Immune to disease onset.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ApplyDiseaseOnset");
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
		Object.UnregisterEffectEvent(this, "ApplyDiseaseOnset");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyDiseaseOnset")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
