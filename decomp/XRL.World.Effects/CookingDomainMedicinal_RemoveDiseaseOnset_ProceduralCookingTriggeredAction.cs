using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainMedicinal_RemoveDiseaseOnset_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they are cured of an incubating disease with a 25% chance.";
	}

	public override string GetNotification()
	{
		return "@they feel like you might be fighting off any ailments you have.";
	}

	public override void Apply(GameObject go)
	{
		if (Stat.Random(1, 100) <= 25)
		{
			go.RemoveEffect<GlotrotOnset>();
			go.RemoveEffect<IronshankOnset>();
		}
	}
}
