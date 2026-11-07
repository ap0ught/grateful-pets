using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cascade = 17)]
public class PhysicalContactEvent : PooledEvent<PhysicalContactEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Object;

	public BodyPart ActorBodyPart;

	public BodyPart ObjectBodyPart;

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
		ActorBodyPart = null;
		ObjectBodyPart = null;
	}

	public static void Send(GameObject Actor, GameObject Object, BodyPart ActorBodyPart = null, BodyPart ObjectBodyPart = null)
	{
		PhysicalContactEvent physicalContactEvent = PooledEvent<PhysicalContactEvent>.FromPool();
		physicalContactEvent.Actor = Actor;
		physicalContactEvent.Object = Object;
		physicalContactEvent.ActorBodyPart = ActorBodyPart;
		physicalContactEvent.ObjectBodyPart = ObjectBodyPart;
		Actor.HandleEvent(physicalContactEvent);
		Object.HandleEvent(physicalContactEvent);
	}
}
