using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainRubber_ReducedFallDamage : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "Falling damage @thisCreature take@s is reduced by 50%.";
	}

	public override string GetTemplatedDescription()
	{
		return "Falling damage @thisCreature take@s is reduced by 50%.";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "BeforeApplyDamage");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "BeforeApplyDamage");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage" && E.GetParameter("Damage") is Damage damage && damage.HasAttribute("Falling"))
		{
			damage.Amount /= 2;
		}
	}
}
