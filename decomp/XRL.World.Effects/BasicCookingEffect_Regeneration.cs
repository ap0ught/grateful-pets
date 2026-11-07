using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class BasicCookingEffect_Regeneration : BasicCookingEffect
{
	public BasicCookingEffect_Regeneration()
	{
	}

	public BasicCookingEffect_Regeneration(string tastyMessage)
		: this()
	{
		wellFedMessage = tastyMessage;
	}

	public override string GetDetails()
	{
		return "+10% to natural healing rate";
	}

	public override void ApplyEffect(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "Regenerating2");
		if (Object.IsPlayer())
		{
			Popup.Show(wellFedMessage + "\n\n{{W|+10% to natural healing rate for the rest of the day}}");
		}
	}

	public override void RemoveEffect(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Regenerating2");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Regenerating2")
		{
			E.SetParameter("Amount", (int)Math.Round((double)E.GetIntParameter("Amount") * 1.1, MidpointRounding.AwayFromZero));
		}
		return base.FireEvent(E);
	}
}
