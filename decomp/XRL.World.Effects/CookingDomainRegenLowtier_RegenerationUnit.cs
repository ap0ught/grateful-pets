using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainRegenLowtier_RegenerationUnit : ProceduralCookingEffectUnit
{
	public int Tier;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(10, 15);
	}

	public override string GetDescription()
	{
		return Tier.Signed() + "% to natural healing rate";
	}

	public override string GetTemplatedDescription()
	{
		return "+10-15% to natural healing rate";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "Regenerating2");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "Regenerating2");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "Regenerating2")
		{
			float num = 1f + (float)Tier * 0.01f;
			int value = (int)Math.Ceiling((float)E.GetIntParameter("Amount") * num);
			E.SetParameter("Amount", value);
		}
	}
}
