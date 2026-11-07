using System.Collections.Generic;
using XRL.Collections;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetTransitiveLocationEvent : PooledEvent<GetTransitiveLocationEvent>
{
	private struct Location(Cell Cell, GameObject Portal, int Priority)
	{
		public Cell Cell = Cell;

		public GameObject Portal = Portal;

		public int Priority = Priority;
	}

	public string Type = "";

	public Zone Zone;

	public Cell Origin;

	public GameObject Actor;

	public GameObject Source;

	private List<Location> Locations = new List<Location>();

	private bool? IngressType;

	private bool? EgressType;

	private bool? InteriorType;

	private bool? PullDownType;

	private bool? WorldMapType;

	public Cell Destination
	{
		get
		{
			if (Locations.Count <= 0)
			{
				return null;
			}
			return Locations[0].Cell;
		}
	}

	public GameObject Portal
	{
		get
		{
			if (Locations.Count <= 0)
			{
				return null;
			}
			return Locations[0].Portal;
		}
	}

	public bool IsIngress
	{
		get
		{
			bool valueOrDefault = IngressType == true;
			if (!IngressType.HasValue)
			{
				valueOrDefault = Type.HasDelimitedSubstring(',', "Ingress");
				IngressType = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool IsEgress
	{
		get
		{
			bool valueOrDefault = EgressType == true;
			if (!EgressType.HasValue)
			{
				valueOrDefault = Type.HasDelimitedSubstring(',', "Egress");
				EgressType = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool IsInterior
	{
		get
		{
			bool valueOrDefault = InteriorType == true;
			if (!InteriorType.HasValue)
			{
				valueOrDefault = Type.HasDelimitedSubstring(',', "Interior");
				InteriorType = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool IsPullDown
	{
		get
		{
			bool valueOrDefault = PullDownType == true;
			if (!PullDownType.HasValue)
			{
				valueOrDefault = Type.HasDelimitedSubstring(',', "PullDown");
				PullDownType = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool IsWorldMap
	{
		get
		{
			bool valueOrDefault = WorldMapType == true;
			if (!WorldMapType.HasValue)
			{
				valueOrDefault = Type.HasDelimitedSubstring(',', "WorldMap");
				WorldMapType = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Type = "";
		Zone = null;
		Origin = null;
		Actor = null;
		Source = null;
		Locations.Clear();
		IngressType = null;
		EgressType = null;
		InteriorType = null;
		PullDownType = null;
		WorldMapType = null;
	}

	public void AddLocation(Cell Cell, GameObject Portal = null, int Priority = 1000)
	{
		int num = Locations.Count;
		while (num > 0 && Locations[num - 1].Priority < Priority)
		{
			num--;
		}
		Locations.Insert(num, new Location(Cell, Portal, Priority));
	}

	public static void GetFor(Zone Zone, string Type, GameObject Actor, GameObject Source, Cell Origin, out Cell Destination, out GameObject Portal)
	{
		if (Zone == null)
		{
			Destination = null;
			Portal = null;
			return;
		}
		using ScopeDisposedList<GameObject> scopeDisposedList = ScopeDisposedList<GameObject>.GetFromPool();
		Zone.GetObjectsThatWantEvent(PooledEvent<GetTransitiveLocationEvent>.ID, MinEvent.CascadeLevel, scopeDisposedList);
		if (GameObject.Validate(ref Source) && Source.WantEvent(PooledEvent<GetTransitiveLocationEvent>.ID, MinEvent.CascadeLevel) && !scopeDisposedList.Contains(Source))
		{
			scopeDisposedList.Add(Source);
		}
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetTransitiveLocationEvent>.ID, MinEvent.CascadeLevel) && !scopeDisposedList.Contains(Actor))
		{
			scopeDisposedList.Add(Actor);
		}
		if (scopeDisposedList.IsNullOrEmpty())
		{
			Destination = null;
			Portal = null;
			return;
		}
		GetTransitiveLocationEvent getTransitiveLocationEvent = PooledEvent<GetTransitiveLocationEvent>.FromPool();
		getTransitiveLocationEvent.Type = Type ?? "";
		getTransitiveLocationEvent.Zone = Zone;
		getTransitiveLocationEvent.Origin = Origin;
		getTransitiveLocationEvent.Actor = Actor;
		getTransitiveLocationEvent.Source = Source;
		using (PooledContainer<GameObject>.Enumerator enumerator = scopeDisposedList.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.HandleEvent(getTransitiveLocationEvent))
			{
			}
		}
		Destination = getTransitiveLocationEvent.Destination;
		Portal = getTransitiveLocationEvent.Portal;
	}
}
