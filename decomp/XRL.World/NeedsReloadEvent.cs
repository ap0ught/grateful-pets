namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Singleton)]
public class NeedsReloadEvent : SingletonEvent<NeedsReloadEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Weapon;

	public IComponent<GameObject> Skip;

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
		Weapon = null;
		Skip = null;
	}

	public static bool Check(GameObject Actor, IComponent<GameObject> Skip = null)
	{
		if (Actor.WantEvent(SingletonEvent<NeedsReloadEvent>.ID, CascadeLevel))
		{
			SingletonEvent<NeedsReloadEvent>.Instance.Reset();
			SingletonEvent<NeedsReloadEvent>.Instance.Actor = Actor;
			SingletonEvent<NeedsReloadEvent>.Instance.Skip = Skip;
			if (!Actor.HandleEvent(SingletonEvent<NeedsReloadEvent>.Instance))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Check(GameObject Actor, GameObject Weapon, IComponent<GameObject> Skip = null)
	{
		if (Weapon.WantEvent(SingletonEvent<NeedsReloadEvent>.ID, CascadeLevel))
		{
			SingletonEvent<NeedsReloadEvent>.Instance.Reset();
			SingletonEvent<NeedsReloadEvent>.Instance.Actor = Actor;
			SingletonEvent<NeedsReloadEvent>.Instance.Weapon = Weapon;
			SingletonEvent<NeedsReloadEvent>.Instance.Skip = Skip;
			if (!Weapon.HandleEvent(SingletonEvent<NeedsReloadEvent>.Instance))
			{
				return true;
			}
		}
		return false;
	}
}
