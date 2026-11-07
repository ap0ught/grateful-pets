namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class PointDefenseInterceptEvent : PooledEvent<PointDefenseInterceptEvent>
{
	public GameObject TargetProjectile;

	public GameObject InterceptProjectile;

	public GameObject PointDefenseSystem;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		TargetProjectile = null;
		InterceptProjectile = null;
		PointDefenseSystem = null;
	}

	public static bool Check(GameObject TargetProjectile, GameObject InterceptProjectile, GameObject PointDefenseSystem)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref TargetProjectile) && TargetProjectile.HasRegisteredEvent("PointDefenseIntercept"))
		{
			Event obj = Event.New("PointDefenseIntercept");
			obj.SetParameter("TargetProjectile", TargetProjectile);
			obj.SetParameter("InterceptProjectile", InterceptProjectile);
			obj.SetParameter("PointDefenseSystem", PointDefenseSystem);
			flag = TargetProjectile.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref TargetProjectile) && TargetProjectile.WantEvent(PooledEvent<PointDefenseInterceptEvent>.ID, MinEvent.CascadeLevel))
		{
			PointDefenseInterceptEvent pointDefenseInterceptEvent = PooledEvent<PointDefenseInterceptEvent>.FromPool();
			pointDefenseInterceptEvent.TargetProjectile = TargetProjectile;
			pointDefenseInterceptEvent.InterceptProjectile = InterceptProjectile;
			pointDefenseInterceptEvent.PointDefenseSystem = PointDefenseSystem;
			flag = TargetProjectile.HandleEvent(pointDefenseInterceptEvent);
		}
		return flag;
	}
}
