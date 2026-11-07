using System;
using Genkit;
using XRL.World.AI.Pathfinding;

namespace XRL.World.Parts;

/// <remarks>Ideally this would be possible with a separate action queue/non-zone world map, maybe for dlc.</remarks>
public class AIWorldMapTravel : AIBehaviorPart
{
	public int ParasangX;

	public int ParasangY;

	public int ZoneX = -1;

	public int ZoneY = -1;

	public int Segments;

	public bool Cardinal;

	public bool Pinned;

	[NonSerialized]
	private FindPath Pathfinder;

	[NonSerialized]
	private int TravelSegments = -1;

	public bool SetZoneID(string Value)
	{
		string World;
		int ZoneZ;
		return ZoneID.Parse(Value, out World, out ParasangX, out ParasangY, out ZoneX, out ZoneY, out ZoneZ);
	}

	public bool SetTerrain(string Blueprint)
	{
		return SetTerrain(ParentObject.CurrentZone, Blueprint);
	}

	public bool SetTerrain(Zone Zone, string Blueprint)
	{
		GameObject gameObject = Zone?.FindObject(Blueprint);
		if (gameObject != null)
		{
			ParasangX = gameObject.CurrentCell.X;
			ParasangY = gameObject.CurrentCell.Y;
			ZoneX = -1;
			ZoneY = -1;
			return true;
		}
		return false;
	}

	public void ResetPathing()
	{
		if (Pathfinder != null)
		{
			Pathfinder.Found = false;
		}
	}

	public override void Initialize()
	{
		CheckZone(ParentObject.CurrentZone);
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (ID != EnteredCellEvent.ID)
		{
			if (ID == GetZoneSuspendabilityEvent.ID)
			{
				return Pinned;
			}
			return false;
		}
		return true;
	}

	public void TakeAction()
	{
		Cell cell = ParentObject.CurrentCell;
		Zone parentZone = cell.ParentZone;
		if (!parentZone.IsWorldMap() || ParentObject.IsPlayer())
		{
			ParentObject.RemovePart(this);
			return;
		}
		if (cell.X == ParasangX && cell.Y == ParasangY)
		{
			int num = ZoneX;
			int num2 = ZoneY;
			int z = 10;
			if (num == -1 || num2 == -1)
			{
				Point3D landingLocation = The.ZoneManager.GetLandingLocation(parentZone.ZoneWorld, ParasangX, ParasangY);
				num = landingLocation.x;
				num2 = landingLocation.y;
				z = landingLocation.z;
			}
			Cell pullDownLocation = The.ZoneManager.GetZone(parentZone.ZoneWorld, ParasangX, ParasangY, num, num2, z).GetPullDownLocation(ParentObject);
			ParentObject.SystemMoveTo(pullDownLocation);
			return;
		}
		if (Pathfinder == null)
		{
			Pathfinder = new FindPath();
		}
		if (!Pathfinder.Found)
		{
			Pathfinder.PerformPathfind(parentZone, cell.X, cell.Y, parentZone, ParasangX, ParasangY, PathGlobal: false, ParentObject, Unlimited: false, AddNoise: false, Cardinal, null, 100, ExploredOnly: false, Autoexploring: false, Juggernaut: false, IgnoreCreatures: true);
			if (!Pathfinder.Found)
			{
				ParentObject.RemovePart(this);
				return;
			}
		}
		int num3 = Pathfinder.Steps.IndexOf(cell);
		if (num3 != -1 && num3 + 1 < Pathfinder.Steps.Count)
		{
			Cell target = Pathfinder.Steps[num3 + 1];
			string directionFromCell = cell.GetDirectionFromCell(target);
			if (ParentObject.Move(directionFromCell, Forced: true, System: true, IgnoreGravity: true, NoStack: false, AllowDashing: true, DoConfirmations: true, null, null, NearestAvailable: false, 1000, null, null, Peaceful: true))
			{
				TravelSegments = -1;
				return;
			}
		}
		Pathfinder.Found = false;
	}

	public int GetTravelSegments()
	{
		if (TravelSegments == -1)
		{
			GameObject gameObject = ParentObject.CurrentCell?.GetFirstObjectWithPart(typeof(TerrainTravel));
			if (gameObject != null && gameObject.TryGetPart<TerrainTravel>(out var Part))
			{
				TravelSegments = Part.GetTravelSegments(ParentObject);
			}
		}
		return 1000;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		Segments += 10 * Amount;
		int travelSegments = GetTravelSegments();
		if (Segments >= travelSegments)
		{
			TakeAction();
			Segments -= travelSegments;
		}
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		CheckZone(E.Cell.ParentZone);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetZoneSuspendabilityEvent E)
	{
		if (Pinned)
		{
			E.Suspendability = Suspendability.Pinned;
			return false;
		}
		return base.HandleEvent(E);
	}

	public void CheckZone(Zone Zone)
	{
		if (Zone != null)
		{
			if (!Zone.IsWorldMap())
			{
				ParentObject.RemovePart(this);
			}
			else if (Pinned && Zone.Suspended && !Zone.Stale)
			{
				The.ZoneManager.SetCachedZone(Zone);
			}
		}
	}
}
