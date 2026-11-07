using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class CanFireAllMissileWeaponsEvent : PooledEvent<CanFireAllMissileWeaponsEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public List<GameObject> MissileWeapons;

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
		MissileWeapons = null;
	}

	public static CanFireAllMissileWeaponsEvent FromPool(GameObject Actor)
	{
		CanFireAllMissileWeaponsEvent canFireAllMissileWeaponsEvent = PooledEvent<CanFireAllMissileWeaponsEvent>.FromPool();
		canFireAllMissileWeaponsEvent.Actor = Actor;
		return canFireAllMissileWeaponsEvent;
	}

	public static bool Check(GameObject Actor, List<GameObject> MissileWeapons = null)
	{
		if (MissileWeapons == null && Actor != null)
		{
			MissileWeapons = Actor.GetMissileWeapons();
		}
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("CanFireAllMissileWeapons"))
		{
			Event obj = Event.New("CanFireAllMissileWeapons");
			obj.SetParameter("Actor", Actor);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<CanFireAllMissileWeaponsEvent>.ID, CascadeLevel))
		{
			flag = Actor.HandleEvent(FromPool(Actor));
		}
		return !flag;
	}
}
