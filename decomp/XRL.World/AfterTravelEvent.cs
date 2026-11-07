using XRL.Collections;

namespace XRL.World;

[GameEvent(Cascade = 17)]
public class AfterTravelEvent : PooledEvent<AfterTravelEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Terrain;

	public int Segments;

	private Rack<IEventHandler> Handlers = new Rack<IEventHandler>();

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
		Terrain = null;
		Segments = 0;
		Handlers.Clear();
	}

	public static void Send(GameObject Actor, GameObject Terrain, int Segments)
	{
		using AfterTravelEvent afterTravelEvent = PooledEvent<AfterTravelEvent>.FromPool();
		afterTravelEvent.Actor = Actor;
		afterTravelEvent.Terrain = Terrain;
		afterTravelEvent.Segments = Segments;
		afterTravelEvent.Handlers.Add(The.Game);
		Zone currentZone = Terrain.CurrentZone;
		currentZone.GetContentWantEventHandlers(PooledEvent<AfterTravelEvent>.ID, CascadeLevel, afterTravelEvent.Handlers);
		if (currentZone != Actor.CurrentZone)
		{
			afterTravelEvent.Handlers.Add(Actor);
		}
		IEventHandler[] array = afterTravelEvent.Handlers.GetArray();
		int i = 0;
		for (int count = afterTravelEvent.Handlers.Count; i < count; i++)
		{
			array[i].HandleEvent(afterTravelEvent);
		}
	}
}
