namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetBandagePerformanceEvent : PooledEvent<GetBandagePerformanceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public static readonly int PASSES = 3;

	public GameObject Object;

	public GameObject Actor;

	public GameObject Subject;

	public string Checking;

	public int BasePerformance;

	public int Performance;

	public int Pass;

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
		Actor = null;
		Subject = null;
		Checking = null;
		BasePerformance = 0;
		Performance = 0;
		Pass = 0;
	}

	public static int GetFor(GameObject Object, GameObject Actor, GameObject Subject, int BasePerformance)
	{
		int num = BasePerformance;
		bool flag = true;
		Event obj = null;
		GetBandagePerformanceEvent getBandagePerformanceEvent = null;
		bool flag2 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetBandagePerformance");
		bool flag3 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetBandagePerformance");
		bool flag4 = GameObject.Validate(ref Subject) && Subject.HasRegisteredEvent("GetBandagePerformance");
		bool flag5 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetBandagePerformanceEvent>.ID, CascadeLevel);
		bool flag6 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetBandagePerformanceEvent>.ID, CascadeLevel);
		bool flag7 = GameObject.Validate(ref Subject) && Subject.WantEvent(PooledEvent<GetBandagePerformanceEvent>.ID, CascadeLevel);
		if (flag2 | flag3 | flag4 | flag5 | flag6 | flag7)
		{
			bool flag8 = true;
			int num2 = 1;
			while ((flag & flag8) && num2 <= PASSES)
			{
				flag8 = false;
				if (flag && (flag2 | flag3 | flag4))
				{
					if (obj == null)
					{
						obj = Event.New("GetBandagePerformance");
						obj.SetParameter("Object", Object);
						obj.SetParameter("Actor", Actor);
						obj.SetParameter("Subject", Subject);
						obj.SetParameter("BasePerformance", BasePerformance);
						obj.SetParameter("Pass", num2);
					}
					obj.SetParameter("Performance", num);
					if ((flag & flag2) && GameObject.Validate(ref Object))
					{
						obj.SetParameter("Checking", "Object");
						flag = Object.FireEvent(obj);
						flag8 = true;
					}
					if ((flag & flag3) && GameObject.Validate(ref Actor))
					{
						obj.SetParameter("Checking", "Actor");
						flag = Actor.FireEvent(obj);
						flag8 = true;
					}
					if ((flag & flag4) && GameObject.Validate(ref Subject))
					{
						obj.SetParameter("Checking", "Subject");
						flag = Subject.FireEvent(obj);
						flag8 = true;
					}
					num = obj.GetIntParameter("Performance");
				}
				if (flag && (flag5 | flag6 | flag7))
				{
					if (getBandagePerformanceEvent == null)
					{
						getBandagePerformanceEvent = PooledEvent<GetBandagePerformanceEvent>.FromPool();
					}
					getBandagePerformanceEvent.Object = Object;
					getBandagePerformanceEvent.Actor = Actor;
					getBandagePerformanceEvent.Subject = Subject;
					getBandagePerformanceEvent.BasePerformance = BasePerformance;
					getBandagePerformanceEvent.Performance = num;
					getBandagePerformanceEvent.Pass = num2;
					if ((flag & flag5) && GameObject.Validate(ref Object))
					{
						getBandagePerformanceEvent.Checking = "Object";
						flag = Object.HandleEvent(getBandagePerformanceEvent);
						flag8 = true;
					}
					if ((flag & flag6) && GameObject.Validate(ref Actor))
					{
						getBandagePerformanceEvent.Checking = "Actor";
						flag = Actor.HandleEvent(getBandagePerformanceEvent);
						flag8 = true;
					}
					if ((flag & flag7) && GameObject.Validate(ref Subject))
					{
						getBandagePerformanceEvent.Checking = "Subject";
						flag = Subject.HandleEvent(getBandagePerformanceEvent);
						flag8 = true;
					}
					num = getBandagePerformanceEvent.Performance;
				}
				num2++;
			}
		}
		return num;
	}
}
