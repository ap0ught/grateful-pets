using System.Diagnostics.CodeAnalysis;
using Genkit;

namespace XRL.World;

[GameEvent(Cascade = 17)]
public class BeforeTravelDownEvent : PooledEvent<BeforeTravelDownEvent>
{
	public new static readonly int CascadeLevel = 17;

	[NotNull]
	public GameObject Actor;

	public string World;

	[NotNull]
	public Cell Cell;

	public GameObject Terrain;

	public Point3D Landing;

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
		World = null;
		Cell = null;
		Terrain = null;
		Landing = default;
	}

	public static bool Check(GameObject Actor, string World, Cell Cell, GameObject Terrain, Point3D? Landing)
	{
		using BeforeTravelDownEvent beforeTravelDownEvent = PooledEvent<BeforeTravelDownEvent>.FromPool();
		beforeTravelDownEvent.Actor = Actor;
		beforeTravelDownEvent.World = World;
		beforeTravelDownEvent.Cell = Cell;
		beforeTravelDownEvent.Terrain = Terrain;
		beforeTravelDownEvent.Landing = Landing ?? new Point3D(1, 1, 10);
		if (!Actor.HandleEvent(beforeTravelDownEvent))
		{
			return false;
		}
		if (Terrain != null && !Terrain.HandleEvent(beforeTravelDownEvent))
		{
			return false;
		}
		return The.Game.HandleEvent(beforeTravelDownEvent);
	}
}
