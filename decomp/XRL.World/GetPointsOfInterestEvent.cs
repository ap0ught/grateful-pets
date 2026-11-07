using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 256, Cache = Cache.Pool)]
public class GetPointsOfInterestEvent : PooledEvent<GetPointsOfInterestEvent>
{
	public new static readonly int CascadeLevel = 256;

	public GameObject Actor;

	public Zone Zone;

	public List<PointOfInterest> List = new List<PointOfInterest>();

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
		Zone = null;
		List.Clear();
	}

	public PointOfInterest Find(GameObject Object)
	{
		foreach (PointOfInterest item in List)
		{
			if (item.Object == Object)
			{
				return item;
			}
		}
		return null;
	}

	public PointOfInterest Find(string Key)
	{
		foreach (PointOfInterest item in List)
		{
			if (item.Key == Key)
			{
				return item;
			}
		}
		return null;
	}

	public void Remove(PointOfInterest p)
	{
		List.Remove(p);
	}

	public PointOfInterest Add(GameObject Object = null, string DisplayName = null, string Explanation = null, string Key = null, string Preposition = null, Location2D Location = null, Cell Cell = null, int Radius = -1, IRenderable Icon = null, int Order = 0)
	{
		PointOfInterest pointOfInterest = new PointOfInterest();
		pointOfInterest.Object = Object;
		pointOfInterest.DisplayName = DisplayName;
		pointOfInterest.Explanation = Explanation;
		pointOfInterest.Key = Key;
		if (!string.IsNullOrEmpty(Preposition))
		{
			pointOfInterest.Preposition = Preposition;
		}
		pointOfInterest.Location = Location ?? Cell?.Location;
		pointOfInterest.Radius = Radius;
		pointOfInterest.Icon = Icon;
		pointOfInterest.Order = Order;
		List.Add(pointOfInterest);
		return pointOfInterest;
	}

	public bool StandardChecks(IPart Part = null, GameObject Actor = null, GameObject Object = null)
	{
		GameObject Object2 = Object ?? Part?.ParentObject;
		if (!GameObject.Validate(ref Object2))
		{
			return false;
		}
		GameObject gameObject = Actor ?? this.Actor;
		if (Object2 == gameObject)
		{
			return false;
		}
		if (Part?.Name != "Interesting" && Object2.HasPart<Interesting>())
		{
			return false;
		}
		if (Object2.HasPart<FungalVision>() && FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		Render render = Object2.Render;
		if (render == null || !render.Visible)
		{
			return false;
		}
		Cell currentCell = Object2.CurrentCell;
		if (currentCell == null || !currentCell.IsExplored())
		{
			return false;
		}
		if (Find(Object2) != null)
		{
			return false;
		}
		if (Object2.IsHostileTowards(gameObject))
		{
			return false;
		}
		if (Object2.IsLedBy(gameObject))
		{
			return false;
		}
		if (!Object2.Understood())
		{
			return false;
		}
		return true;
	}

	public static List<PointOfInterest> GetFor(GameObject Actor, Zone Z = null)
	{
		if (Z == null)
		{
			Z = Actor?.CurrentZone;
		}
		List<PointOfInterest> result = null;
		if (Z != null)
		{
			GetPointsOfInterestEvent getPointsOfInterestEvent = PooledEvent<GetPointsOfInterestEvent>.FromPool();
			getPointsOfInterestEvent.Actor = Actor;
			getPointsOfInterestEvent.Zone = Z;
			getPointsOfInterestEvent.List.Clear();
			Z.HandleEvent(getPointsOfInterestEvent);
			if (getPointsOfInterestEvent.List.Count > 0)
			{
				result = new List<PointOfInterest>(getPointsOfInterestEvent.List);
			}
		}
		return result;
	}

	public static PointOfInterest GetOne(GameObject Actor, string Key, Zone Z = null)
	{
		if (Z == null)
		{
			Z = Actor?.CurrentZone;
		}
		if (Z != null)
		{
			GetPointsOfInterestEvent getPointsOfInterestEvent = PooledEvent<GetPointsOfInterestEvent>.FromPool();
			getPointsOfInterestEvent.Actor = Actor;
			getPointsOfInterestEvent.Zone = Z;
			getPointsOfInterestEvent.List.Clear();
			Z.HandleEvent(getPointsOfInterestEvent);
			return getPointsOfInterestEvent.Find(Key);
		}
		return null;
	}

	public static bool Check(GameObject Object, GameObject Actor, Zone Z = null)
	{
		if (GameObject.Validate(ref Object))
		{
			if (Z == null)
			{
				Z = Actor?.CurrentZone;
			}
			if (Z != null)
			{
				GetPointsOfInterestEvent getPointsOfInterestEvent = PooledEvent<GetPointsOfInterestEvent>.FromPool();
				getPointsOfInterestEvent.Actor = Actor;
				getPointsOfInterestEvent.Zone = Z;
				getPointsOfInterestEvent.List.Clear();
				Object.HandleEvent(getPointsOfInterestEvent);
				if (getPointsOfInterestEvent.Find(Object) != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool AnyFor(GameObject Actor, Zone Z = null)
	{
		bool result = false;
		if (Z == null)
		{
			Z = Actor?.CurrentZone;
		}
		if (Z != null)
		{
			GetPointsOfInterestEvent getPointsOfInterestEvent = PooledEvent<GetPointsOfInterestEvent>.FromPool();
			getPointsOfInterestEvent.Actor = Actor;
			getPointsOfInterestEvent.Zone = Z;
			getPointsOfInterestEvent.List.Clear();
			Z.HandleEvent(getPointsOfInterestEvent);
			if (getPointsOfInterestEvent.List.Count > 0)
			{
				result = true;
			}
		}
		return result;
	}
}
