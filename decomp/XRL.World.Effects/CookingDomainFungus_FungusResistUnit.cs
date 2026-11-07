using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFungus_FungusResistUnit : ProceduralCookingEffectUnit
{
	public override void Init(GameObject target)
	{
	}

	public override string GetDescription()
	{
		return "75% chance that itchy skin doesn't develop into a fungal infection";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "BeforeApplyFungalInfection");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "BeforeApplyFungalInfection");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyFungalInfection" && 75.in100())
		{
			E.AddParameter("Cancelled", true);
		}
	}
}
