using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class BeforeDismemberEvent : PooledEvent<BeforeDismemberEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Object;

	public GameObject Perspective;

	public BodyPart Part;

	public IInventory Where;

	public bool Silent;

	public bool Obliterate;

	public bool IsActor
	{
		get
		{
			if (Actor != null)
			{
				return Actor == Perspective;
			}
			return false;
		}
	}

	public bool IsObject
	{
		get
		{
			if (Object != null)
			{
				return Object == Perspective;
			}
			return false;
		}
	}

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
		Object = null;
		Perspective = null;
		Part = null;
		Where = null;
		Silent = false;
		Obliterate = false;
	}

	public static bool Check(GameObject Actor, GameObject Object, BodyPart Part, IInventory Where = null, bool Silent = false, bool Obliterate = false)
	{
		using BeforeDismemberEvent beforeDismemberEvent = PooledEvent<BeforeDismemberEvent>.FromPool();
		beforeDismemberEvent.Actor = Actor;
		beforeDismemberEvent.Object = Object;
		beforeDismemberEvent.Part = Part;
		beforeDismemberEvent.Where = Where;
		beforeDismemberEvent.Silent = Silent;
		beforeDismemberEvent.Obliterate = Obliterate;
		beforeDismemberEvent.Perspective = Actor;
		if (Actor != null && Actor != Object && !Actor.HandleEvent(beforeDismemberEvent))
		{
			return false;
		}
		beforeDismemberEvent.Perspective = Object;
		if (!Object.HandleEvent(beforeDismemberEvent))
		{
			return false;
		}
		beforeDismemberEvent.Perspective = null;
		return The.Game?.HandleEvent(beforeDismemberEvent) ?? true;
	}
}
