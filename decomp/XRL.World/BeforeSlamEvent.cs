namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class BeforeSlamEvent : PooledEvent<BeforeSlamEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Target;

	public Cell TargetCell;

	public int SlamMultiplier;

	public int SlamPower;

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
		Target = null;
		TargetCell = null;
		SlamMultiplier = 0;
		SlamPower = 0;
	}

	public static bool Check(GameObject Actor, GameObject Target, Cell TargetCell, ref int SlamMultiplier, ref int SlamPower)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("BeforeSlam"))
		{
			Event obj = Event.New("BeforeSlam");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			obj.SetParameter("TargetCell", TargetCell);
			obj.SetParameter("SlamMultiplier", SlamMultiplier);
			obj.SetParameter("SlamPower", SlamPower);
			flag = Actor.FireEvent(obj);
			SlamMultiplier = obj.GetIntParameter("SlamMultiplier");
			SlamPower = obj.GetIntParameter("SlamPower");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<BeforeSlamEvent>.ID, CascadeLevel))
		{
			BeforeSlamEvent beforeSlamEvent = PooledEvent<BeforeSlamEvent>.FromPool();
			beforeSlamEvent.Actor = Actor;
			beforeSlamEvent.Target = Target;
			beforeSlamEvent.TargetCell = TargetCell;
			beforeSlamEvent.SlamMultiplier = SlamMultiplier;
			beforeSlamEvent.SlamPower = SlamPower;
			flag = Actor.HandleEvent(beforeSlamEvent);
			SlamMultiplier = beforeSlamEvent.SlamMultiplier;
			SlamPower = beforeSlamEvent.SlamPower;
		}
		return flag;
	}
}
