using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class BasicCookingEffect_Hitpoints : BasicCookingEffect
{
	public string stat;

	public int amount;

	public BasicCookingEffect_Hitpoints()
	{
	}

	public BasicCookingEffect_Hitpoints(string tastyMessage)
		: this()
	{
		wellFedMessage = tastyMessage;
	}

	public override string GetDetails()
	{
		if (amount == 0)
		{
			return "+10% hit points";
		}
		return amount.Signed() + " hit points";
	}

	public override void ApplyEffect(GameObject Object)
	{
		amount = (int)Math.Max(Math.Round((double)Object.BaseStat("Hitpoints") * 0.1, MidpointRounding.AwayFromZero), 1.0);
		StatShifter.SetStatShift("Hitpoints", amount);
		if (Object.IsPlayer())
		{
			Popup.Show(wellFedMessage + "\n\n{{W|+" + amount + " hit " + ((amount == 1) ? "point" : "points") + " for the rest of the day}}");
		}
	}

	public override void RemoveEffect(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		amount = 0;
	}
}
