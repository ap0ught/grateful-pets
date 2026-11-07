using System;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingEffectUnit_10Regeneration : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "+10% to natural healing rate";
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
			int value = (int)Math.Ceiling((float)E.GetIntParameter("Amount") * 0.9f);
			E.SetParameter("Amount", value);
		}
	}
}
