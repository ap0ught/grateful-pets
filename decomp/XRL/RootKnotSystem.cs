using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Rules;
using XRL.World;
using XRL.World.AI;

namespace XRL;

[Serializable]
public class RootKnotSystem : IGameSystem
{
	[NonSerialized]
	public List<GameObject> inventory = new List<GameObject>();

	public bool first = true;

	public override void Write(SerializationWriter writer)
	{
		writer.WriteGameObjectList(inventory);
	}

	public override void Read(SerializationReader reader)
	{
		inventory = new List<GameObject>();
		reader.ReadGameObjectList(inventory);
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(ZoneBuiltEvent.ID);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		PlaceSummoner(E.Zone);
		return base.HandleEvent(E);
	}

	public void PlaceSummoner(Zone Zone)
	{
		if (Zone.IsWorldMap())
		{
			return;
		}
		int num = 3;
		if (Zone.IsCheckpoint())
		{
			num = 40;
		}
		if (first)
		{
			num = 100;
		}
		if (Stat.Roll(1, 100) > num)
		{
			return;
		}
		first = false;
		if (Zone.ZoneID == "JoppaWorld.11.22.1.1.10")
		{
			Zone.GetCell(24, 21).AddObject("RootPetSummoner");
		}
		else
		{
			Cell cell = Zone.GetCellWithEmptyBorder(1);
			if (cell == null)
			{
				cell = Zone.GetEmptyCells().FirstOrDefault();
			}
			if (cell == null)
			{
				cell = Zone.GetCells().FirstOrDefault();
			}
			Cell cell2 = cell.GetCellFromDirection("NW");
			if (cell2 == null)
			{
				cell2 = cell;
			}
			cell2.AddObject("RootPetSummoner");
		}
		foreach (GameObject @object in Zone.GetObjects("RootKnot"))
		{
			@object.Brain.AddOpinion<OpinionMollify>(The.Player, 100f);
			@object.Inventory.Objects = inventory;
		}
	}
}
