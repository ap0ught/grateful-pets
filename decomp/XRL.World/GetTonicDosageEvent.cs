namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetTonicDosageEvent : PooledEvent<GetTonicDosageEvent>
{
	public GameObject Object;

	public GameObject Subject;

	public GameObject Actor;

	public int Dosage;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Subject = null;
		Actor = null;
		Dosage = 0;
	}

	public static int GetFor(GameObject Object, GameObject Subject = null, GameObject Actor = null, int BaseDosage = 1)
	{
		int num = BaseDosage;
		bool flag = true;
		bool flag2 = GameObject.Validate(ref Subject) && Subject.HasRegisteredEvent("GetTonicDosage");
		bool flag3 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetTonicDosage");
		bool flag4 = GameObject.Validate(ref Subject) && Subject.WantEvent(PooledEvent<GetTonicDosageEvent>.ID, MinEvent.CascadeLevel);
		bool flag5 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetTonicDosageEvent>.ID, MinEvent.CascadeLevel);
		if (flag && (flag2 | flag3))
		{
			Event obj = Event.New("GetTonicDosage");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Subject", Subject);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Dosage", num);
			if (flag & flag2)
			{
				flag = Subject.FireEvent(obj);
			}
			if (flag & flag3)
			{
				flag = Object.FireEvent(obj);
			}
			num = obj.GetIntParameter("Dosage");
		}
		if (flag && (flag4 | flag5))
		{
			GetTonicDosageEvent getTonicDosageEvent = PooledEvent<GetTonicDosageEvent>.FromPool();
			getTonicDosageEvent.Object = Object;
			getTonicDosageEvent.Subject = Subject;
			getTonicDosageEvent.Actor = Actor;
			getTonicDosageEvent.Dosage = num;
			if (flag & flag4)
			{
				flag = Subject.HandleEvent(getTonicDosageEvent);
			}
			if (flag & flag5)
			{
				flag = Object.HandleEvent(getTonicDosageEvent);
			}
			num = getTonicDosageEvent.Dosage;
		}
		return num;
	}
}
