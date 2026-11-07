using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class BasicCookingEffect_Quickness : BasicCookingEffect
{
	public string stat;

	public int amount;

	public BasicCookingEffect_Quickness()
	{
	}

	public BasicCookingEffect_Quickness(string tastyMessage)
		: this()
	{
		wellFedMessage = tastyMessage;
	}

	public override string GetDetails()
	{
		if (amount == 0)
		{
			return "+3% Quickness";
		}
		return amount.Signed() + " Quickness";
	}

	public override void ApplyEffect(GameObject Object)
	{
		amount = (int)Math.Max(Math.Round((double)Object.BaseStat("Speed") * 0.03, MidpointRounding.AwayFromZero), 1.0);
		StatShifter.SetStatShift("Speed", amount);
		if (Object.IsPlayer())
		{
			Popup.Show(wellFedMessage + "\n\n{{W|+" + amount + " quickness for the rest of the day}}");
		}
	}

	public override void RemoveEffect(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		amount = 0;
	}
}
