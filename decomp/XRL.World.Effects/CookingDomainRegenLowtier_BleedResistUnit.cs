using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainRegenLowtier_BleedResistUnit : ProceduralCookingEffectUnit
{
	public int Tier;

	public override string GetDescription()
	{
		return Tier.Signed() + " to saves vs. bleeding";
	}

	public override string GetTemplatedDescription()
	{
		return "+8-12 to saves vs. bleeding";
	}

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(8, 12);
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "ModifyDefendingSave");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "ModifyDefendingSave");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "ModifyDefendingSave" && E.GetStringParameter("Vs").Contains("Bleeding"))
		{
			E.SetParameter("Roll", E.GetIntParameter("Roll") + Tier);
		}
	}
}
