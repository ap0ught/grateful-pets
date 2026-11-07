using System;

namespace XRL.World.Effects;

[Serializable]
public class BasicTriggeredCookingStatEffect : BasicTriggeredCookingEffect
{
	public string stat;

	public int amount;

	public override string GetDetails()
	{
		return amount.Signed() + " " + Statistic.GetStatDisplayName(stat);
	}

	public BasicTriggeredCookingStatEffect(string stat, string amount, int duration)
	{
		Duration = duration;
		DisplayName = null;
		this.stat = stat;
		this.amount = amount.RollCached();
	}

	public override void ApplyEffect(GameObject Object)
	{
		StatShifter.SetStatShift(stat, amount);
		base.ApplyEffect(Object);
	}

	public override void RemoveEffect(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		amount = 0;
		base.RemoveEffect(Object);
	}
}
