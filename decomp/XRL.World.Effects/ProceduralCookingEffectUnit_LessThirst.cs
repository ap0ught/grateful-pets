using System;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingEffectUnit_LessThirst : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "@thisCreature thirst@s at half rate.";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "CalculatingThirst");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "CalculatingThirst");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "CalculatingThirst")
		{
			int num = (int)Math.Ceiling((float)E.GetIntParameter("Amount") * 0.5f);
			if (num < 1)
			{
				num = 1;
			}
			E.SetParameter("Amount", num);
		}
	}
}
