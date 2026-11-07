using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
[HasGameBasedStaticCache]
public class VehicleRecord : IComposite
{
	[NonSerialized]
	[GameBasedStaticCache(true, false, CreateInstance = false)]
	public static Dictionary<string, VehicleRecord> _All;

	public string ID;

	public string OwnerID;

	public string Blueprint;

	public string Type;

	public GlobalLocation Location;

	public static Dictionary<string, VehicleRecord> All
	{
		get
		{
			if (_All == null)
			{
				_All = The.Game.GetObjectGameState("Vehicle.Records") as Dictionary<string, VehicleRecord>;
				if (_All == null)
				{
					_All = new Dictionary<string, VehicleRecord>();
					The.Game.SetObjectGameState("Vehicle.Records", _All);
				}
			}
			return _All;
		}
	}

	public static List<VehicleRecord> GetRecordsFor(GameObject Owner = null, string Blueprint = null, string Type = null)
	{
		List<VehicleRecord> list = new List<VehicleRecord>();
		foreach (KeyValuePair<string, VehicleRecord> item in All)
		{
			if ((!Blueprint.IsNullOrEmpty() && item.Value.Blueprint != Blueprint) || (!Type.IsNullOrEmpty() && item.Value.Type != Type))
			{
				continue;
			}
			if (Owner != null)
			{
				if (Owner.IsPlayer())
				{
					if (item.Value.OwnerID != "Player")
					{
						continue;
					}
				}
				else if (!Owner.IDMatch(item.Value.OwnerID))
				{
					continue;
				}
			}
			list.Add(item.Value);
		}
		return list;
	}

	public static List<GameObject> ResolveRecordsFor(GameObject Owner = null, string Blueprint = null, string Type = null)
	{
		List<VehicleRecord> recordsFor = GetRecordsFor(Owner, Blueprint, Type);
		List<GameObject> list = new List<GameObject>(recordsFor.Count);
		foreach (VehicleRecord item in recordsFor)
		{
			if (The.ZoneManager.CachedObjects.TryGetValue(item.ID, out var value))
			{
				list.Add(value);
				continue;
			}
			Cell cell = item.Location.ResolveCell();
			if (cell == null)
			{
				continue;
			}
			value = cell.FindObjectByID(item.ID) ?? cell.ParentZone.FindObjectByID(item.ID);
			if (value != null)
			{
				if (value.IsTemporary)
				{
					All.Remove(item.ID);
				}
				else
				{
					list.Add(value);
				}
			}
		}
		return list;
	}
}
