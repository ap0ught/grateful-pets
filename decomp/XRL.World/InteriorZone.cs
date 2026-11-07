using System;
using System.Collections.Generic;

namespace XRL.World;

[Serializable]
[HasGameBasedStaticCache]
public class InteriorZone : Zone
{
	[GameBasedStaticCache(true, false)]
	public static InteriorZone Active;

	public string Schema;

	public string Instance;

	public GlobalLocation Location = new GlobalLocation();

	[NonSerialized]
	private bool Failed;

	[NonSerialized]
	private GameObject _ParentObject;

	public override string ZoneID
	{
		get
		{
			return _ZoneID;
		}
		set
		{
			_ZoneID = value;
			XRL.World.ZoneID.Parse(value, out ZoneWorld, out Schema, out Instance, out wX, out wY, out X, out Y, out Z);
		}
	}

	public Cell ParentCell => ParentObject?.CurrentCell;

	public Zone ParentZone => ParentObject?.CurrentZone;

	/// <summary>The containing object of this interior zone.</summary>
	public GameObject ParentObject
	{
		get
		{
			if (GameObject.Validate(ref _ParentObject))
			{
				Location.SetCell(_ParentObject.CurrentCell);
			}
			else
			{
				if (Failed)
				{
					return null;
				}
				if (!Instance.IsNullOrEmpty())
				{
					Cell cell = Location.ResolveCell();
					_ParentObject = cell?.FindObjectByID(Instance) ?? cell?.ParentZone.FindObjectByID(Instance) ?? The.ZoneManager.FindObjectByID(Instance);
					if (_ParentObject == null)
					{
						MetricsManager.LogError("Unable to find interior zone host by ID: " + Instance);
					}
					Failed = _ParentObject == null;
				}
			}
			return _ParentObject;
		}
		set
		{
			_ParentObject = value;
			Instance = value.ID;
			Location.SetCell(_ParentObject.CurrentCell);
		}
	}

	public InteriorZone()
	{
	}

	public InteriorZone(int Width, int Height)
		: base(Width, Height)
	{
	}

	public override string ResolveZoneWorld()
	{
		return Location.World;
	}

	public Cell GetEscapeCell(GameObject For = null)
	{
		Cell cell = ParentObject?.CurrentCell;
		if (cell == null || cell.ParentZone == this)
		{
			long num = -1L;
			string zoneID = "JoppaWorld.11.22.1.1.10";
			foreach (KeyValuePair<string, long> item in The.ZoneManager.VisitedTime)
			{
				if (item.Value > num && !(ZoneID == item.Key) && item.Key.IndexOf('.') != -1)
				{
					num = item.Value;
					zoneID = item.Key;
				}
			}
			cell = The.ZoneManager.GetZone(zoneID).GetPullDownLocation(For);
		}
		if (For != null && !cell.IsPassable(For))
		{
			cell = cell.getClosestPassableCellFor(For);
		}
		return cell;
	}

	public Zone ResolveParentZone()
	{
		return ParentObject?.GetCurrentCell()?.ParentZone;
	}

	public Cell ResolveParentCell()
	{
		return ParentObject?.GetCurrentCell();
	}

	public Zone ResolveBasisZone()
	{
		return ResolveBasisCell()?.ParentZone;
	}

	public Cell ResolveBasisCell()
	{
		Cell cell = ResolveParentCell();
		InteriorZone interiorZone = cell?.ParentZone as InteriorZone;
		for (int i = 0; i < 100; i++)
		{
			if (interiorZone == null)
			{
				break;
			}
			if (interiorZone == this)
			{
				break;
			}
			Cell cell2 = interiorZone.ResolveParentCell();
			if (cell2 == null)
			{
				break;
			}
			cell = cell2;
			interiorZone = cell2.ParentZone as InteriorZone;
		}
		return cell;
	}

	public override void Activated()
	{
		Active = this;
		base.Activated();
	}

	public override void Deactivated()
	{
		if (Active == this)
		{
			Active = null;
		}
		base.Deactivated();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (ID != ZoneBuiltEvent.ID)
		{
			return base.WantEvent(ID, cascade);
		}
		return true;
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (E.ID == ZoneBuiltEvent.ID)
		{
			InteriorZoneBuiltEvent.Send(ParentObject, this);
		}
		return base.HandleEvent(E);
	}

	public override string GetCheckpointKey()
	{
		return ResolveBasisZone()?.GetCheckpointKey();
	}

	public override bool IsCheckpoint()
	{
		return ResolveBasisZone()?.IsCheckpoint() ?? false;
	}

	public override int GetTransitionIntervalTo(Zone Z)
	{
		Zone zone = ResolveBasisZone();
		if (zone == Z)
		{
			return 0;
		}
		if (Z is InteriorZone interiorZone && interiorZone.ResolveBasisZone() == zone)
		{
			return 0;
		}
		return base.GetTransitionIntervalTo(Z);
	}
}
