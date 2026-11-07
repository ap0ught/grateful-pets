using XRL.Rules;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetThrownWeaponRangeEvent : PooledEvent<GetThrownWeaponRangeEvent>
{
	public GameObject Object;

	public GameObject Actor;

	public int MaxRange;

	public int MidRange;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Actor = null;
		MaxRange = 0;
		MidRange = 0;
	}

	public static void GetFor(out int MaxRange, out int MidRange, GameObject Object, GameObject Actor, int DefaultMaxRange = 9999, int DefaultMidRange = -1)
	{
		MaxRange = DefaultMaxRange;
		MidRange = DefaultMidRange;
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetThrownWeaponRange"))
		{
			Event obj = Event.New("GetThrownWeaponRange");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("MaxRange", MaxRange);
			obj.SetParameter("MidRange", MidRange);
			flag = Object.FireEvent(obj);
			MaxRange = obj.GetIntParameter("MaxRange");
			MidRange = obj.GetIntParameter("MidRange");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetThrownWeaponRangeEvent>.ID, MinEvent.CascadeLevel))
		{
			GetThrownWeaponRangeEvent getThrownWeaponRangeEvent = PooledEvent<GetThrownWeaponRangeEvent>.FromPool();
			getThrownWeaponRangeEvent.Object = Object;
			getThrownWeaponRangeEvent.Actor = Actor;
			getThrownWeaponRangeEvent.MaxRange = MaxRange;
			getThrownWeaponRangeEvent.MidRange = MidRange;
			flag = Object.HandleEvent(getThrownWeaponRangeEvent);
			MaxRange = getThrownWeaponRangeEvent.MaxRange;
			MidRange = getThrownWeaponRangeEvent.MidRange;
		}
		if (flag && MidRange == DefaultMidRange)
		{
			GetThrowProfileEvent.Process(out MidRange, out var _, out var _, out var _, Actor, Object);
			if (MaxRange == DefaultMaxRange)
			{
				MaxRange = MidRange + RuleSettings.THROW_RANGE_RANDOM_VARIANCE_MAX;
			}
			MidRange += RuleSettings.THROW_RANGE_RANDOM_VARIANCE_MIN;
		}
	}
}
