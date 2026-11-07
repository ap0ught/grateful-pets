namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ShouldAttackToReachTargetEvent : PooledEvent<ShouldAttackToReachTargetEvent>
{
	public GameObject Actor;

	public GameObject Object;

	public GameObject Target;

	public bool ShouldAttack;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Object = null;
		Target = null;
		ShouldAttack = false;
	}

	public static bool Check(GameObject Actor, GameObject Object, GameObject Target = null, bool ShouldAttack = false)
	{
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("ShouldAttackToReachTarget");
			bool flag3 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("ShouldAttackToReachTarget");
			if (flag2 | flag3)
			{
				Event obj = Event.New("ShouldAttackToReachTarget");
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Object", Object);
				obj.SetParameter("Target", Target);
				obj.SetFlag("ShouldAttack", ShouldAttack);
				flag = flag && (!flag2 || Actor.FireEvent(obj)) && (!flag3 || Object.FireEvent(obj));
				ShouldAttack = obj.HasFlag("ShouldAttack");
			}
		}
		if (flag)
		{
			bool flag4 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<ShouldAttackToReachTargetEvent>.ID, MinEvent.CascadeLevel);
			bool flag5 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<ShouldAttackToReachTargetEvent>.ID, MinEvent.CascadeLevel);
			if (flag4 | flag5)
			{
				ShouldAttackToReachTargetEvent shouldAttackToReachTargetEvent = PooledEvent<ShouldAttackToReachTargetEvent>.FromPool();
				shouldAttackToReachTargetEvent.Actor = Actor;
				shouldAttackToReachTargetEvent.Object = Object;
				shouldAttackToReachTargetEvent.Target = Target;
				shouldAttackToReachTargetEvent.ShouldAttack = ShouldAttack;
				flag = flag && (!flag4 || Actor.HandleEvent(shouldAttackToReachTargetEvent)) && (!flag5 || Object.HandleEvent(shouldAttackToReachTargetEvent));
				ShouldAttack = shouldAttackToReachTargetEvent.ShouldAttack;
			}
		}
		return ShouldAttack;
	}
}
