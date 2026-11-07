using System;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public class NorthShevaSystem : IPlayerSystem
{
	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(PooledEvent<CanTravelEvent>.ID);
		Registrar.Register(PooledEvent<BeforeTravelDownEvent>.ID);
		Registrar.Register(PooledEvent<GenericQueryEvent>.ID);
	}

	public override bool HandleEvent(CanTravelEvent E)
	{
		if (E.Object.CurrentZone?.ZoneWorld != "NorthSheva")
		{
			return true;
		}
		if (!E.Object.TryGetPart<Vehicle>(out var Part) || Part.Type != "Mover")
		{
			return E.Object.ShowFailure("You can't travel in this environment.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeTravelDownEvent E)
	{
		if (E.World == "NorthSheva")
		{
			GameObject firstObject = E.Cell.GetFirstObject((GameObject x) => x.GetBlueprint().DescendsFrom("TerrainMoverStop"));
			if (firstObject == null)
			{
				return E.Actor.ShowFailure("You can't land here.");
			}
			if (firstObject.GetBlueprint().DescendsFrom("TerrainMoverStopRuined"))
			{
				return E.Actor.ShowFailure("That stop is too ruined for the mover to land.");
			}
		}
		return true;
	}

	public override bool HandleEvent(GenericQueryEvent E)
	{
		if (E.Query == "ActivateSpiralBorer" && E.Subject?.CurrentZone?.ZoneWorld == "NorthSheva")
		{
			return E.Result = E.Subject.ShowFailure("You cannot do that here.");
		}
		return true;
	}
}
