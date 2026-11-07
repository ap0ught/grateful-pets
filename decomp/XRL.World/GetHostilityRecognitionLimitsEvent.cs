namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetHostilityRecognitionLimitsEvent : PooledEvent<GetHostilityRecognitionLimitsEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Subject;

	public int IgnoreEasierThan;

	public int IgnoreFartherThan;

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
		Subject = null;
		IgnoreEasierThan = 0;
		IgnoreFartherThan = 0;
	}

	public void MinDifficulty(int Value)
	{
		if (IgnoreEasierThan < Value)
		{
			IgnoreEasierThan = Value;
		}
	}

	public void MaxDistance(int Value)
	{
		if (IgnoreFartherThan > Value)
		{
			IgnoreFartherThan = Value;
		}
	}

	public static bool GetFor(GameObject Actor, GameObject Subject, ref int IgnoreEasierThan, ref int IgnoreFartherThan)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Subject) && Subject.HasRegisteredEvent("GetHostilityRecognitionLimits"))
		{
			Event obj = Event.New("GetHostilityRecognitionLimits");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Subject", Subject);
			obj.SetParameter("IgnoreEasierThan", IgnoreEasierThan);
			obj.SetParameter("IgnoreFartherThan", IgnoreFartherThan);
			flag = Subject.FireEvent(obj);
			IgnoreEasierThan = obj.GetIntParameter("IgnoreEasierThan");
			IgnoreFartherThan = obj.GetIntParameter("IgnoreFartherThan");
		}
		if (flag && GameObject.Validate(ref Subject) && Subject.WantEvent(PooledEvent<GetHostilityRecognitionLimitsEvent>.ID, CascadeLevel))
		{
			GetHostilityRecognitionLimitsEvent getHostilityRecognitionLimitsEvent = PooledEvent<GetHostilityRecognitionLimitsEvent>.FromPool();
			getHostilityRecognitionLimitsEvent.Actor = Actor;
			getHostilityRecognitionLimitsEvent.Subject = Subject;
			getHostilityRecognitionLimitsEvent.IgnoreEasierThan = IgnoreEasierThan;
			getHostilityRecognitionLimitsEvent.IgnoreFartherThan = IgnoreFartherThan;
			flag = Subject.HandleEvent(getHostilityRecognitionLimitsEvent);
			IgnoreEasierThan = getHostilityRecognitionLimitsEvent.IgnoreEasierThan;
			IgnoreFartherThan = getHostilityRecognitionLimitsEvent.IgnoreFartherThan;
		}
		return flag;
	}
}
