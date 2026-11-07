using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainCold_CryokinesisCharge_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they freeze an area per Cryokinesis at level 5-6.";
	}

	public override string GetNotification()
	{
		return "@they freeze an area with cryokinesis.";
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer())
		{
			Cryokinesis.Cast();
		}
	}
}
