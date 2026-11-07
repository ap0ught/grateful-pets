using System;

namespace XRL.World;

[GameEvent(Cascade = 0)]
public class PartAddedEvent : PooledEvent<PartAddedEvent>
{
	public new static readonly int CascadeLevel;

	public GameObject Actor;

	public IPart Part;

	public Type Type;

	public bool Registration;

	public bool Creation;

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
		Registration = false;
		Creation = false;
	}

	public static void Send(GameObject Actor, IPart Part, Type Type, bool Registration, bool Creation)
	{
		using PartAddedEvent partAddedEvent = PooledEvent<PartAddedEvent>.FromPool();
		partAddedEvent.Actor = Actor;
		partAddedEvent.Part = Part;
		partAddedEvent.Type = Type;
		partAddedEvent.Registration = Registration;
		partAddedEvent.Creation = Creation;
		Actor.HandleEvent(partAddedEvent);
		if (ComponentReflection.PartExtensions.TryGetValue(Type, out var value))
		{
			value.Add(Actor, Part, Registration, Creation);
		}
	}
}
