using System;
using XRL.Rules;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class AIBarathrumShuttle : AIBehaviorPart
{
	public const string ID_MOVER_CENTRAL = "NorthSheva.39.11.2.2.8";

	public const string ID_SHIP_QUAY = "NorthSheva.66.13.0.1.10";

	public const string ID_SHIP_CELL = "ShevaStarship";

	public const string TERRAIN_QUAY = "TerrainStarfarersQuayStop";

	public const int STAGE_ASCENDING = -1;

	public const int STAGE_MOVER_CENTRAL = 0;

	public const int STAGE_TRAVELING_QUAY = 1;

	public const int STAGE_SHIP_QUAY = 2;

	public const int STAGE_SHIP_LAUNCH = 3;

	public int Stage = -1;

	public int Timer;

	[NonSerialized]
	private DelegateGoal Goal;

	public override bool WantEvent(int ID, int Cascade)
	{
		if (ID != PooledEvent<AIBoredEvent>.ID && ID != GetZoneSuspendabilityEvent.ID)
		{
			return ID == BeginMentalDefendEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (PushGoal(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetZoneSuspendabilityEvent E)
	{
		E.Suspendability = Suspendability.Pinned;
		return false;
	}

	public override bool HandleEvent(BeginMentalDefendEvent E)
	{
		if (E.Defender == ParentObject && E.Command == "Proselytize")
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool PushGoal(GameObject Actor)
	{
		if (Actor == ParentObject)
		{
			if (Stage >= 0 && Actor.PartyLeader != null)
			{
				return false;
			}
			if (Goal == null || !Actor.Brain.Goals.Contains(Goal))
			{
				if (Goal == null)
				{
					Goal = new DelegateGoal(Action);
				}
				Actor.Brain.PushGoal(Goal);
				return true;
			}
		}
		return false;
	}

	public void Action(GoalHandler Handler)
	{
		if (Stage == -1)
		{
			ActionIdle(Handler);
		}
		else if (Stage == 0)
		{
			ActionMoverCentral(Handler);
		}
		else if (Stage == 1)
		{
			ActionTravelQuay(Handler);
		}
		else if (Stage == 2)
		{
			ActionShipQuay(Handler);
		}
		else if (Stage == 3)
		{
			ActionShipLaunch(Handler);
		}
	}

	public void ActionIdle(GoalHandler Handler)
	{
		if (ParentObject.TryGetEffect<Sitting>(out var Effect))
		{
			if (!GameObject.Validate(Effect.SittingOn) || Effect.SittingOn.DistanceTo(ParentObject) >= 1)
			{
				ParentObject.RemoveEffect(Effect);
			}
			else
			{
				ParentObject.UseEnergy(1000);
			}
			return;
		}
		GameObject gameObject = ParentObject.CurrentZone.FindClosestObject(ParentObject, IsValidSeat, ExploredOnly: false, VisibleOnly: true, IncludeSelf: false);
		if (gameObject != null)
		{
			if (ParentObject.DistanceTo(gameObject) <= 1)
			{
				gameObject.GetPart<Chair>().SitDown(ParentObject);
			}
			else
			{
				Handler.PushChildGoal(new MoveTo(gameObject, careful: false, overridesCombat: false, 1));
			}
		}
		else
		{
			Handler.PushChildGoal(new WanderRandomly(5));
			Handler.PushChildGoal(new Wait(5));
		}
	}

	public void ActionMoverCentral(GoalHandler Handler)
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone is InteriorZone { ParentObject: var parentObject } interiorZone)
		{
			GameObject gameObject = interiorZone.FindObject((GameObject x) => x.HasPart<VehicleControl>());
			if (parentObject == null || gameObject == null || parentObject.Blueprint != "Mover" || !gameObject.TryGetPart<VehicleControl>(out var Part) || !parentObject.TryGetPart<Vehicle>(out var Part2) || (Part2.PilotID != null && Part2.PilotID != ParentObject.ID))
			{
				Handler.PushChildGoal(new MoveToExterior());
				return;
			}
			if (ParentObject.DistanceTo(gameObject) > 1)
			{
				Handler.PushChildGoal(new MoveTo(gameObject, careful: false, overridesCombat: false, 1));
				return;
			}
			if (Part2.PilotID == ParentObject.ID)
			{
				Handler.PushChildGoal(new Wait(10));
				return;
			}
			if (Part.AttemptPilot(ParentObject))
			{
				Stage = 1;
				StartTravel(parentObject);
				return;
			}
		}
		else
		{
			GameObject gameObject2 = currentZone.FindObject(IsFreeMover);
			if (gameObject2 != null)
			{
				Handler.PushChildGoal(new MoveToInterior(gameObject2));
				return;
			}
			if (currentZone.ZoneID != "NorthSheva.39.11.2.2.8")
			{
				Handler.PushChildGoal(new MoveToZone("NorthSheva.39.11.2.2.8"));
				return;
			}
		}
		Handler.PushChildGoal(new Wait(Stat.Random(5, 25)));
	}

	public void ActionTravelQuay(GoalHandler Handler)
	{
		if (ParentObject.HasEffect<Piloting>())
		{
			Handler.PushChildGoal(new Wait(Stat.Random(5, 25)));
		}
		else
		{
			ActionIdle(Handler);
		}
	}

	public void ActionShipQuay(GoalHandler Handler)
	{
		Zone parentZone = ParentObject.CurrentCell.ParentZone;
		if (parentZone is InteriorZone)
		{
			if (parentZone.ZoneID.Contains("ShevaStarship"))
			{
				Stage = 3;
			}
			else
			{
				Handler.PushChildGoal(new MoveToExterior());
			}
			return;
		}
		if (parentZone.ZoneID != "NorthSheva.66.13.0.1.10")
		{
			Handler.PushChildGoal(new MoveToZone("NorthSheva.66.13.0.1.10"));
			return;
		}
		GameObject gameObject = parentZone.FindFirstObject((GameObject x) => x.TryGetPart<Interior>(out var Part) && Part.Cell == "ShevaStarship");
		if (gameObject != null)
		{
			Handler.PushChildGoal(new MoveToInterior(gameObject));
		}
		else
		{
			Handler.PushChildGoal(new Wait(Stat.Random(5, 25)));
		}
	}

	public void ActionShipLaunch(GoalHandler Handler)
	{
		if (!(ParentObject.CurrentCell.ParentZone is InteriorZone interiorZone) || !interiorZone.ZoneID.Contains("ShevaStarship"))
		{
			Stage = 2;
			return;
		}
		GameObject gameObject = interiorZone.FindFirstObject((GameObject x) => x.HasPart(typeof(ShevaStarshipControl)));
		if (gameObject == null)
		{
			Handler.PushGoal(new Wait(5));
			return;
		}
		if (gameObject.DistanceTo(ParentObject) > 1)
		{
			Handler.PushGoal(new MoveTo(gameObject, careful: false, overridesCombat: false, 1));
			return;
		}
		ParentObject.UseEnergy(1000);
		if (gameObject.TryGetPart<ShevaStarshipControl>(out var Part) && !Part.IsStarted && !Part.IsFinished)
		{
			DidXToY("start", "interfacing with", gameObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: true);
			if (!Part.AttemptLaunch(ParentObject))
			{
				Handler.PushGoal(new Wait(20));
			}
		}
	}

	public void StartTravel(GameObject Object)
	{
		Zone currentZone = Object.CurrentZone;
		if (currentZone.IsWorldMap())
		{
			return;
		}
		Cell worldCell = currentZone.GetWorldCell();
		if (worldCell == null || !(currentZone.GetTerrainObject()?.Blueprint != "TerrainStarfarersQuayStop") || worldCell.ParentZone.Stale)
		{
			return;
		}
		Object.SystemMoveTo(worldCell);
		if (Object.HasPart<AIWorldMapTravel>())
		{
			return;
		}
		AIWorldMapTravel aIWorldMapTravel = new AIWorldMapTravel();
		if (!aIWorldMapTravel.SetTerrain(worldCell.ParentZone, "TerrainStarfarersQuayStop"))
		{
			return;
		}
		aIWorldMapTravel.Cardinal = true;
		aIWorldMapTravel.Pinned = true;
		Object.AddPart(aIWorldMapTravel);
		Cell cell = (The.Player.OnWorldMap() ? The.Player.CurrentCell : The.ActiveZone.GetTerrainObject()?.CurrentCell);
		if (cell != null)
		{
			int num = 0;
			while (cell.ManhattanDistanceTo(Object.CurrentCell) < 2 && num++ < 20)
			{
				aIWorldMapTravel.TakeAction();
			}
		}
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (Stage != 1 || !(ParentObject.CurrentZone is InteriorZone { ParentObject: var parentObject }))
		{
			return;
		}
		Zone zone = parentObject?.CurrentZone;
		if (zone != null && !zone.IsWorldMap() && zone.GetTerrainObject()?.Blueprint == "TerrainStarfarersQuayStop")
		{
			if (ParentObject.TryGetEffect<Piloting>(out var Effect))
			{
				Effect.Stop();
			}
			Stage = 2;
		}
	}

	public static bool IsFreeMover(GameObject Object)
	{
		if (Object.Blueprint == "Mover" && Object.TryGetPart<Vehicle>(out var Part) && Part.PilotID == null && Object.TryGetPart<Interior>(out var Part2))
		{
			if (Part2.IsZoneLive)
			{
				return !Part2.Zone.IsActive();
			}
			return true;
		}
		return false;
	}

	public bool IsValidSeat(GameObject Object)
	{
		if (Object.HasPart(typeof(Chair)))
		{
			return Object.CurrentCell.IsPassable(ParentObject);
		}
		return false;
	}

	[WishCommand("launchbarathrum", null)]
	public static void WishBarathrumLaunch()
	{
		GameObject gameObject = GameObject.Create("Starship 1 Platform N");
		GameObject gameObject2 = GameObject.Create("Barathrum");
		The.ActiveZone.GetCell(0, 0).AddObject(gameObject);
		The.ActiveZone.GetCell(0, 0).AddObject(gameObject2);
		Interior part = gameObject.GetPart<Interior>();
		part.TryEnter(The.Player, Force: true);
		part.TryEnter(gameObject2, Force: true);
		gameObject2.RequirePart<AIBarathrumShuttle>().Stage = 3;
	}
}
