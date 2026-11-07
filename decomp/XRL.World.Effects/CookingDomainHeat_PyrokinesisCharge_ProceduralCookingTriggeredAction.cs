using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHeat_PyrokinesisCharge_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they toast an area per Pyrokinesis at level 5-6.";
	}

	public override string GetNotification()
	{
		return "@they toast an area with pyrokinesis.";
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer())
		{
			Pyrokinesis.Cast();
		}
	}
}
