using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainAgility_Shank_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	private bool didFire;

	public override string GetDescription()
	{
		return "@they shank @their opponent.";
	}

	public override string GetNotification()
	{
		if (didFire)
		{
			return "@they perform@s an act of nimble violence.";
		}
		return null;
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer())
		{
			ShortBlades_Shank.Cast(go);
			didFire = true;
		}
	}
}
