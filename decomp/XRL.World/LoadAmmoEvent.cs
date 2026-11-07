namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class LoadAmmoEvent : PooledEvent<LoadAmmoEvent>
{
	public GameObject Object;

	public GameObject Actor;

	public GameObject Projectile;

	public GameObject LoadedAmmo;

	public string Message;

	public bool ActivePartsIgnoreSubject;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Actor = null;
		Projectile = null;
		LoadedAmmo = null;
		Message = null;
		ActivePartsIgnoreSubject = false;
	}

	public static bool Check(GameObject Object, GameObject Actor, out GameObject Projectile, out GameObject LoadedAmmo, out string Message, bool ActivePartsIgnoreSubject = false)
	{
		Message = null;
		Projectile = null;
		LoadedAmmo = null;
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("LoadAmmo"))
		{
			Event obj = Event.New("LoadAmmo");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Loader", Actor);
			obj.SetParameter("Ammo", Projectile);
			obj.SetParameter("LoadedAmmo", LoadedAmmo);
			obj.SetParameter("Message", Message);
			obj.SetFlag("ActivePartsIgnoreSubject", ActivePartsIgnoreSubject);
			flag = Object.FireEvent(obj);
			Projectile = obj.GetGameObjectParameter("Ammo");
			LoadedAmmo = obj.GetGameObjectParameter("LoadedAmmo");
			Message = obj.GetStringParameter("Message");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<LoadAmmoEvent>.ID, MinEvent.CascadeLevel))
		{
			LoadAmmoEvent loadAmmoEvent = PooledEvent<LoadAmmoEvent>.FromPool();
			loadAmmoEvent.Object = Object;
			loadAmmoEvent.Actor = Actor;
			loadAmmoEvent.Projectile = Projectile;
			loadAmmoEvent.LoadedAmmo = LoadedAmmo;
			loadAmmoEvent.Message = Message;
			loadAmmoEvent.ActivePartsIgnoreSubject = ActivePartsIgnoreSubject;
			flag = Object.HandleEvent(loadAmmoEvent);
			Projectile = loadAmmoEvent.Projectile;
			LoadedAmmo = loadAmmoEvent.LoadedAmmo;
			Message = loadAmmoEvent.Message;
		}
		return flag;
	}

	public static bool Check(GameObject Object, GameObject Actor, out GameObject Projectile, bool ActivePartsIgnoreSubject = false)
	{
		GameObject LoadedAmmo;
		string Message;
		return Check(Object, Actor, out Projectile, out LoadedAmmo, out Message, ActivePartsIgnoreSubject);
	}
}
