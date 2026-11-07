using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class AfterDismemberEvent : PooledEvent<AfterDismemberEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Object;

	public GameObject Limb;

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

	public bool IsLimb
	{
		get
		{
			if (Limb != null)
			{
				return Limb == Perspective;
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
		Limb = null;
		Perspective = null;
		Part = null;
		Where = null;
		Silent = false;
		Obliterate = false;
	}

	public static void Send(GameObject Actor, GameObject Object, GameObject Limb, BodyPart Part, IInventory Where = null, bool Silent = false, bool Obliterate = false)
	{
		using AfterDismemberEvent afterDismemberEvent = PooledEvent<AfterDismemberEvent>.FromPool();
		afterDismemberEvent.Actor = Actor;
		afterDismemberEvent.Object = Object;
		afterDismemberEvent.Limb = Limb;
		afterDismemberEvent.Part = Part;
		afterDismemberEvent.Where = Where;
		afterDismemberEvent.Silent = Silent;
		afterDismemberEvent.Obliterate = Obliterate;
		afterDismemberEvent.Perspective = Actor;
		if (Actor != null && Actor != Object)
		{
			Actor.HandleEvent(afterDismemberEvent);
		}
		afterDismemberEvent.Perspective = Object;
		Object?.HandleEvent(afterDismemberEvent);
		afterDismemberEvent.Perspective = Limb;
		Limb?.HandleEvent(afterDismemberEvent);
		afterDismemberEvent.Perspective = null;
		The.Game?.HandleEvent(afterDismemberEvent);
	}
}
