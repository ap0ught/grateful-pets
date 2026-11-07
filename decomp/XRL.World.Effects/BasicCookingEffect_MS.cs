using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class BasicCookingEffect_MS : BasicCookingEffect
{
	public string stat;

	public int amount;

	public BasicCookingEffect_MS()
	{
	}

	public BasicCookingEffect_MS(string tastyMessage)
		: this()
	{
		wellFedMessage = tastyMessage;
	}

	public override string GetDetails()
	{
		if (amount == 0)
		{
			return "+6% Move Speed";
		}
		return amount.Signed() + " Move Speed";
	}

	public override void ApplyEffect(GameObject Object)
	{
		amount = (int)Math.Max(Math.Round((double)(200 - Object.BaseStat("MoveSpeed")) * 0.06, MidpointRounding.AwayFromZero), 1.0);
		StatShifter.SetStatShift("MoveSpeed", -amount);
		if (Object.IsPlayer())
		{
			Popup.Show(wellFedMessage + "\n\n{{W|" + amount + " move speed for the rest of the day}}");
		}
	}

	public override void RemoveEffect(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		amount = 0;
	}
}
