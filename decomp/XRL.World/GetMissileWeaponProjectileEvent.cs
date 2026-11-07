namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetMissileWeaponProjectileEvent : PooledEvent<GetMissileWeaponProjectileEvent>
{
	public GameObject Launcher;

	public GameObject Projectile;

	public string Blueprint;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Launcher = null;
		Projectile = null;
		Blueprint = null;
	}

	public static GetMissileWeaponProjectileEvent FromPool(GameObject Launcher)
	{
		GetMissileWeaponProjectileEvent getMissileWeaponProjectileEvent = PooledEvent<GetMissileWeaponProjectileEvent>.FromPool();
		getMissileWeaponProjectileEvent.Launcher = Launcher;
		return getMissileWeaponProjectileEvent;
	}

	public static bool GetFor(GameObject Launcher, ref GameObject Projectile, ref string Blueprint)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Launcher) && Launcher.WantEvent(PooledEvent<GetMissileWeaponProjectileEvent>.ID, MinEvent.CascadeLevel))
		{
			GetMissileWeaponProjectileEvent getMissileWeaponProjectileEvent = PooledEvent<GetMissileWeaponProjectileEvent>.FromPool();
			getMissileWeaponProjectileEvent.Launcher = Launcher;
			getMissileWeaponProjectileEvent.Projectile = Projectile;
			getMissileWeaponProjectileEvent.Blueprint = Blueprint;
			flag = Launcher.HandleEvent(getMissileWeaponProjectileEvent);
			Projectile = getMissileWeaponProjectileEvent.Projectile;
			Blueprint = getMissileWeaponProjectileEvent.Blueprint;
		}
		if (flag && GameObject.Validate(ref Launcher) && Launcher.HasRegisteredEvent("GetMissileWeaponProjectile"))
		{
			Event obj = Event.New("GetMissileWeaponProjectile");
			obj.SetParameter("Launcher", Launcher);
			obj.SetParameter("Projectile", Projectile);
			obj.SetParameter("Blueprint", Blueprint);
			flag = Launcher.FireEvent(obj);
			Projectile = obj.GetGameObjectParameter("Projectile");
			Blueprint = obj.GetStringParameter("Blueprint");
		}
		return flag;
	}
}
