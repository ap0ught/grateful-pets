using System;

namespace XRL.World;

[GameEvent(Cascade = 0)]
public class PartRemovedEvent : PooledEvent<PartRemovedEvent>
{
	public new static readonly int CascadeLevel;

	public GameObject Actor;

	public IPart Part;

	public Type Type;

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
		Part = null;
		Type = null;
	}

	public static void Send(GameObject Actor, IPart Part, Type Type)
	{
		using PartRemovedEvent partRemovedEvent = PooledEvent<PartRemovedEvent>.FromPool();
		partRemovedEvent.Actor = Actor;
		partRemovedEvent.Part = Part;
		partRemovedEvent.Type = Type;
		Actor.HandleEvent(partRemovedEvent);
	}
}
