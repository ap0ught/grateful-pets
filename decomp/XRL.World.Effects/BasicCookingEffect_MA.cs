using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class BasicCookingEffect_MA : BasicCookingEffect
{
	public string stat;

	public int amount;

	public BasicCookingEffect_MA()
	{
	}

	public BasicCookingEffect_MA(string tastyMessage)
		: this()
	{
		wellFedMessage = tastyMessage;
	}

	public override string GetDetails()
	{
		return "+1 MA";
	}

	public override void ApplyEffect(GameObject Object)
	{
		amount = 1;
		StatShifter.SetStatShift("MA", amount);
		if (Object.IsPlayer())
		{
			Popup.Show(wellFedMessage + "\n\n{{W|+1 MA for the rest of the day}}");
		}
	}

	public override void RemoveEffect(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		amount = 0;
	}
}
