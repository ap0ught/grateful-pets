namespace XRL.World;

[GameEvent]
public class ConfigureMissileVisualEffectEvent : PooledEvent<ConfigureMissileVisualEffectEvent>
{
	public MissileWeaponVFXConfiguration Configuration;

	public MissileWeaponVFXConfiguration.MissileVFXPathDefinition Path;

	public GameObject Actor;

	public GameObject Launcher;

	public GameObject Projectile;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Configuration = null;
		Path = null;
		Actor = null;
		Launcher = null;
		Projectile = null;
	}

	public static void Send(MissileWeaponVFXConfiguration Configuration, MissileWeaponVFXConfiguration.MissileVFXPathDefinition Path, GameObject Actor, GameObject Launcher, GameObject Projectile)
	{
		ConfigureMissileVisualEffectEvent E = PooledEvent<ConfigureMissileVisualEffectEvent>.FromPool();
		E.Configuration = Configuration;
		E.Path = Path;
		E.Actor = Actor;
		E.Launcher = Launcher;
		E.Projectile = Projectile;
		if (Actor != null && Actor != Launcher)
		{
			Actor.HandleEvent(E);
		}
		if (Launcher != null && Launcher != Projectile)
		{
			Launcher.HandleEvent(E);
		}
		Projectile?.HandleEvent(E);
		PooledEvent<ConfigureMissileVisualEffectEvent>.ResetTo(ref E);
	}
}
