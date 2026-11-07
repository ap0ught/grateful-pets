using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class BasicCookingStatEffect : BasicCookingEffect
{
	public string stat;

	public int amount;

	public BasicCookingStatEffect(string stat, string amount, int duration)
	{
		Duration = duration;
		DisplayName = null;
		this.stat = stat;
		this.amount = Stat.Roll(amount);
	}

	public override void ApplyEffect(GameObject Object)
	{
		Object.Statistics[stat].Bonus += amount;
		base.ApplyEffect(Object);
	}

	public override void RemoveEffect(GameObject Object)
	{
		Object.Statistics[stat].Bonus -= amount;
		amount = 0;
		base.RemoveEffect(Object);
	}
}
