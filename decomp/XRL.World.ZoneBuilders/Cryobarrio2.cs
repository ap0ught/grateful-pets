using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class Cryobarrio2
{
	public static void MakeCryochamber(int x, int y, Zone Z, string Contents = null, string Plaque = null)
	{
		Z.ClearBox(new Box(x - 2, y - 2, x + 2, y + 2));
		Z.GetCell(x + 2, y - 1).AddObject("VGlassWall");
		Z.GetCell(x + 2, y).AddObject("VGlassWall");
		Z.GetCell(x + 2, y + 1).AddObject("VGlassWall");
		Z.GetCell(x - 2, y - 1).AddObject("VGlassWall");
		Z.GetCell(x - 2, y).AddObject("VGlassWall");
		Z.GetCell(x - 2, y + 1).AddObject("VGlassWall");
		Z.GetCell(x - 1, y + 2).AddObject("HGlassWall");
		Z.GetCell(x + 1, y + 2).AddObject("HGlassWall");
		if (y < 12 && Plaque != null)
		{
			if (Contents != null)
			{
				Z.GetCell(x, y + 2).AddObject(Plaque);
			}
		}
		else
		{
			Z.GetCell(x, y + 2).AddObject("HGlassWall");
		}
		Z.GetCell(x - 1, y - 2).AddObject("HGlassWall");
		Z.GetCell(x + 1, y - 2).AddObject("HGlassWall");
		if (y >= 12 && Plaque != null)
		{
			if (Contents != null)
			{
				Z.GetCell(x, y - 2).AddObject(Plaque);
			}
		}
		else
		{
			Z.GetCell(x, y - 2).AddObject("HGlassWall");
		}
		if (Contents == "*Destroyed")
		{
			Z.GetCell(x, y).AddObject("CryoGas");
		}
		else
		{
			Z.GetCell(x, y).AddObject("StableCryoGas");
		}
		if (!string.IsNullOrEmpty(Contents) && Contents != "*Destroyed")
		{
			GameObject gameObject = null;
			if (Contents == "*Clone")
			{
				if (The.Player != null)
				{
					gameObject = Cloning.GenerateClone(The.Player, null, null, DuplicateGear: true, BecomesCompanion: false, Budded: false, "Cryotube", ItemNaming.GetBaseVersion);
				}
			}
			else
			{
				try
				{
					gameObject = GameObject.Create(Contents);
				}
				catch (Exception message)
				{
					MetricsManager.LogError(message);
				}
			}
			if (gameObject != null)
			{
				gameObject.AddPart(new NoDamageWhileFrozen());
				gameObject.AddPart(new OmniphaseWhileFrozen());
				gameObject.SetStringProperty("StartFrozen", "1");
				Z.GetCell(x, y).AddObject(gameObject);
				if (gameObject.Brain != null)
				{
					gameObject.MakeActive();
				}
			}
			else
			{
				MetricsManager.LogError("could not create contents for cryochamber: " + Contents);
			}
		}
		Z.FillHollowBox(new Box(x - 1, y - 1, x + 1, y + 1), "StableCryoGas");
		Z.GetCell(x - 2, y - 2).AddObject("CryochamberWallSE");
		Z.GetCell(x + 2, y - 2).AddObject("CryochamberWallSW");
		Z.GetCell(x - 2, y + 2).AddObject("CryochamberWallNE");
		Z.GetCell(x + 2, y + 2).AddObject("CryochamberWallNW");
	}

	public bool BuildZone(Zone Z)
	{
		Z.Fill("Marble");
		Z.FillBox(new Box(0, 0, Z.Width - 1, Z.Height - 1), "Fulcrete");
		Z.ClearBox(new Box(10, 2, 70, 21));
		Z.FillHollowBox(new Box(9, 2, 71, 22), "Marble");
		List<Point> list = new List<Point>();
		list.Add(new Point(20, 6));
		list.Add(new Point(40, 6));
		list.Add(new Point(60, 6));
		list.Add(new Point(20, 18));
		list.Add(new Point(40, 18));
		list.Add(new Point(60, 18));
		list = new List<Point>(Algorithms.RandomShuffle(list, Stat.Rand));
		for (int i = 1; i < 7; i++)
		{
			string plaque = null;
			string contents = null;
			if (i == 1 || i == 2)
			{
				plaque = "CryochamberPlaque";
			}
			if (i == 3)
			{
				plaque = "CryochamberPlaqueSpider";
			}
			if (i == 4)
			{
				plaque = "CryochamberPlaqueRhinox";
			}
			if (i == 5)
			{
				plaque = "CryochamberPlaqueSkybear";
			}
			if (i == 6)
			{
				plaque = "CryochamberPlaqueYempris";
			}
			if (i == 3)
			{
				contents = "Phase Spider Coward";
			}
			if (i == 4)
			{
				contents = "Rhinox";
			}
			if (i == 5)
			{
				contents = "Skybear";
			}
			if (i == 6)
			{
				contents = "Yempuris";
			}
			MakeCryochamber(list[i - 1].X, list[i - 1].Y, Z, contents, plaque);
		}
		Z.ClearBox(new Box(2, 11, 9, 13));
		Z.ClearBox(new Box(5, 6, 9, 18));
		Z.GetCell(2, 12).AddObject("OpenShaft");
		Z.GetCell(2, 12).AddObject("Platform");
		GameObject gameObject = GameObjectFactory.Factory.CreateObject("ElevatorSwitch");
		ElevatorSwitch part = gameObject.GetPart<ElevatorSwitch>();
		part.TopLevel = Z.Z;
		part.FloorLevel = Z.Z + 1;
		Z.GetCell(2, 11).AddObject(gameObject);
		return true;
	}
}
