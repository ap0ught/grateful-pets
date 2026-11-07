using XRL.Rules;

namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class GetThrowProfileEvent : PooledEvent<GetThrowProfileEvent>
{
	public new static readonly int CascadeLevel = 1;

	public GameObject Actor;

	public GameObject Object;

	public GameObject ApparentTarget;

	public Cell TargetCell;

	public int Distance;

	public int Range;

	public int Strength;

	public int AimVariance;

	public bool Telekinetic;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Object = null;
		ApparentTarget = null;
		TargetCell = null;
		Distance = 0;
		Range = 0;
		Strength = 0;
		AimVariance = 0;
		Telekinetic = false;
	}

	public static bool Process(out int Range, out int Strength, out int AimVariance, out bool Telekinetic, GameObject Actor, GameObject Object, GameObject ApparentTarget = null, Cell TargetCell = null, int Distance = 0)
	{
		GameObject.Validate(ref Object);
		GameObject.Validate(ref Actor);
		Range = 4;
		Strength = 0;
		AimVariance = 0;
		Telekinetic = false;
		if (Actor != null)
		{
			Range += Actor.GetIntProperty("ThrowRangeBonus") + Object.GetIntProperty("ThrowRangeSkillBonus");
			Strength = Actor.Stat("Strength");
		}
		if (Object != null)
		{
			Range += Object.GetIntProperty("ThrowRangeBonus");
		}
		AimVariance = Stat.Random(0, 20) - 10;
		if (Actor != null)
		{
			int num = Actor.StatMod("Agility");
			if (num != 0)
			{
				if (AimVariance == 0)
				{
					if (num < 0)
					{
						AimVariance += (50.in100() ? num : (-num));
					}
				}
				else if (AimVariance > 0)
				{
					AimVariance -= num;
					if (AimVariance < 0)
					{
						AimVariance = 0;
					}
				}
				else
				{
					AimVariance += num;
					if (AimVariance > 0)
					{
						AimVariance = 0;
					}
				}
			}
		}
		GetThrowProfileEvent getThrowProfileEvent = PooledEvent<GetThrowProfileEvent>.FromPool();
		getThrowProfileEvent.Actor = Actor;
		getThrowProfileEvent.Object = Object;
		getThrowProfileEvent.ApparentTarget = ApparentTarget;
		getThrowProfileEvent.TargetCell = TargetCell;
		getThrowProfileEvent.Distance = Distance;
		getThrowProfileEvent.Range = Range;
		getThrowProfileEvent.Strength = Strength;
		getThrowProfileEvent.AimVariance = AimVariance;
		getThrowProfileEvent.Telekinetic = Telekinetic;
		try
		{
			if (Actor != null && Actor.WantEvent(PooledEvent<GetThrowProfileEvent>.ID, CascadeLevel) && !Actor.HandleEvent(getThrowProfileEvent))
			{
				return false;
			}
			if (Object != null && Actor.WantEvent(PooledEvent<GetThrowProfileEvent>.ID, CascadeLevel) && !Object.HandleEvent(getThrowProfileEvent))
			{
				return false;
			}
			return true;
		}
		finally
		{
			Strength = getThrowProfileEvent.Strength;
			Range = getThrowProfileEvent.Range + Stat.GetScoreModifier(Strength);
			AimVariance = getThrowProfileEvent.AimVariance;
			Telekinetic = getThrowProfileEvent.Telekinetic;
		}
	}
}
