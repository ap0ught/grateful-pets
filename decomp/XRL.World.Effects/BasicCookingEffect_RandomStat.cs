using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class BasicCookingEffect_RandomStat : BasicCookingEffect
{
	public string stat;

	public int amount;

	public BasicCookingEffect_RandomStat()
	{
		DisplayName = "{{W|well fed}}";
	}

	public BasicCookingEffect_RandomStat(string tastyMessage)
		: this()
	{
		wellFedMessage = tastyMessage;
	}

	public override string GetDetails()
	{
		return amount.Signed() + " " + stat;
	}

	public override void ApplyEffect(GameObject Object)
	{
		stat = new List<string> { "Strength", "Intelligence", "Willpower", "Agility", "Toughness", "Ego" }.GetRandomElement();
		amount = 1;
		Object.Statistics[stat].Bonus += amount;
		if (Object.IsPlayer())
		{
			Popup.Show(wellFedMessage + "\n\n{{W|" + amount + " " + stat + " for the rest of the day}}");
		}
	}

	public override void RemoveEffect(GameObject Object)
	{
		Object.Statistics[stat].Bonus -= amount;
		amount = 0;
	}
}
