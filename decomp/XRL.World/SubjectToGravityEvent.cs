namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class SubjectToGravityEvent : PooledEvent<SubjectToGravityEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public bool SubjectToGravity;

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
		Object = null;
		SubjectToGravity = false;
	}

	public static bool Check(GameObject Object)
	{
		bool flag = true;
		if (GameObject.Validate(ref Object))
		{
			if (!Object.IsReal)
			{
				flag = false;
			}
			else if (Object.HasPropertyOrTag("IgnoresGravity"))
			{
				flag = false;
			}
		}
		bool flag2 = true;
		if (flag2 && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("SubjectToGravity"))
		{
			Event obj = Event.New("SubjectToGravity");
			obj.SetParameter("Object", Object);
			obj.SetFlag("SubjectToGravity", flag);
			flag2 = Object.FireEvent(obj);
			flag = obj.HasFlag("SubjectToGravity");
		}
		if (flag2 && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<SubjectToGravityEvent>.ID, CascadeLevel))
		{
			SubjectToGravityEvent subjectToGravityEvent = PooledEvent<SubjectToGravityEvent>.FromPool();
			subjectToGravityEvent.Object = Object;
			subjectToGravityEvent.SubjectToGravity = flag;
			flag2 = Object.HandleEvent(subjectToGravityEvent);
			flag = subjectToGravityEvent.SubjectToGravity;
		}
		return flag;
	}
}
