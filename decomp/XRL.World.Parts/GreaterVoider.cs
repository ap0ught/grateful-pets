using System;
using System.Collections.Generic;
using Genkit;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class GreaterVoider : IPart
{
	public const int LOOT_CORPSE_CHANCE = 10;

	public static string LastLairZone = null;

	public static Rect2D LastLairRect = Rect2D.invalid;

	public int RealityStabilizationPenetration = 60;

	public bool createdLair;

	public string lairZone;

	public Rect2D lairRect = Rect2D.invalid;

	private static readonly Dictionary<string, string> SurfaceWalls = new Dictionary<string, string> { { "DeepJungle", "PlantWall" } };

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("GreaterVoiderBiteHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GreaterVoiderBiteHit")
		{
			if (lairRect == Rect2D.invalid)
			{
				return true;
			}
			if (lairZone == null)
			{
				return true;
			}
			if (ParentObject.InZone(lairZone) && lairRect.Contains(ParentObject.CurrentCell.Pos2D))
			{
				return true;
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (gameObjectParameter != null)
			{
				List<Cell> emptyCells = The.ZoneManager.GetZone(lairZone).GetEmptyCells(lairRect);
				Cell cell = emptyCells.RemoveRandomElement();
				Cell cell2 = emptyCells.RemoveRandomElement();
				if (cell == null || cell2 == null)
				{
					return true;
				}
				Event obj = Event.New("InitiateRealityDistortionTransit");
				obj.SetParameter("Object", gameObjectParameter);
				obj.SetParameter("Cell", cell);
				obj.SetParameter("Mutation", this);
				obj.SetParameter("RealityStabilizationPenetration", RealityStabilizationPenetration);
				Event obj2 = Event.New("InitiateRealityDistortionTransit");
				obj2.SetParameter("Object", gameObjectParameter);
				obj2.SetParameter("Cell", cell);
				obj2.SetParameter("Mutation", this);
				obj2.SetParameter("RealityStabilizationPenetration", RealityStabilizationPenetration);
				if (gameObjectParameter.FireEvent(obj) && cell.FireEvent(obj2) && ParentObject.FireEvent(obj2) && cell2.FireEvent(obj2))
				{
					DidXToY("teleport", gameObjectParameter, "to " + ParentObject.its + " lair", "!", null, null, null, gameObjectParameter);
					if (gameObjectParameter.TeleportTo(cell, 0))
					{
						gameObjectParameter.TeleportSwirl(null, "&C", Voluntary: false, null, 'ù', IsOut: true);
						if (ParentObject.TeleportTo(cell2, 0))
						{
							ParentObject.TeleportSwirl();
						}
					}
				}
			}
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!createdLair)
		{
			try
			{
				CreateLair();
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Voider lair creation", x);
			}
		}
		return base.HandleEvent(E);
	}

	public FastNoise NoiseSetup(Zone Z)
	{
		FastNoise fastNoise = new FastNoise(The.Game.GetWorldSeed(Z.ZoneID));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.25f);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(2);
		fastNoise.SetFractalLacunarity(1.7f);
		fastNoise.SetFractalGain(0.5f);
		return fastNoise;
	}

	public void CreateLair()
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (LastLairZone == currentZone.ZoneID && LastLairRect.Area > 1)
		{
			lairRect = LastLairRect;
			lairZone = LastLairZone;
			createdLair = true;
			return;
		}
		int[,] unreachableOrWallGrid = currentZone.GetUnreachableOrWallGrid();
		Rect2D rect2D = GridTools.MaxRectByArea(unreachableOrWallGrid, bDraw: false, 4, 4);
		bool flag = false;
		if (rect2D.Area <= 1)
		{
			for (int i = 0; i < currentZone.Height; i++)
			{
				for (int j = 0; j < currentZone.Width; j++)
				{
					unreachableOrWallGrid[j, i] = (currentZone.Map[j][i].HasObject(IsBlocker) ? 1 : 0);
				}
			}
			flag = true;
			rect2D = GridTools.MaxRectByArea(unreachableOrWallGrid, bDraw: false, 4, 4);
			if (rect2D.Area <= 1)
			{
				return;
			}
		}
		if (rect2D.Width > 7)
		{
			int num = (rect2D.Width - 7) / 2;
			rect2D.x1 += num;
			rect2D.x2 -= num + (rect2D.Width - 7) % 2;
		}
		if (rect2D.Height > 7)
		{
			int num2 = (rect2D.Height - 7) / 2;
			rect2D.y1 += num2;
			rect2D.y2 -= num2 + (rect2D.Height - 7) % 2;
		}
		string wall = GetWall(currentZone, rect2D);
		currentZone.ClearBox(rect2D);
		currentZone.FillHollowBox(rect2D, wall);
		FastNoise fastNoise = NoiseSetup(currentZone);
		Rect2D rect2D2 = (flag ? rect2D.ReduceBy(-1, -1) : rect2D);
		Rect2D rect2D3 = rect2D.ReduceBy(2, 2);
		for (int k = rect2D2.y1; k <= rect2D2.y2; k++)
		{
			for (int l = rect2D2.x1; l <= rect2D2.x2; l++)
			{
				if (!rect2D3.Contains(l, k))
				{
					Cell cell = currentZone.GetCell(l, k);
					if (cell != null && !cell.HasObject(IsBlocker) && fastNoise.GetNoise(l, k, currentZone.Z) > 1f - (float)rect2D.Area / 80f)
					{
						cell.ClearAndAddObject(wall);
					}
				}
			}
		}
		LastLairZone = (lairZone = ParentObject.Physics.CurrentCell.ParentZone.ZoneID);
		LastLairRect = (lairRect = rect2D.ReduceBy(1, 1));
		createdLair = true;
		List<Cell> emptyCells = currentZone.GetEmptyCells(rect2D);
		if (10.in100())
		{
			emptyCells.RemoveRandomElement().AddPopulation("DeadHumanoidWithGear");
		}
		foreach (Cell item in emptyCells)
		{
			if (Stat.Random(1, 100) <= 25)
			{
				item.AddObject("Web");
			}
			if (Stat.Random(1, 100) <= 25)
			{
				item.AddObject("Bones");
			}
		}
	}

	public string GetWall(Zone Z, Box R)
	{
		string value = null;
		GameObject terrainObject = Z.GetTerrainObject();
		if (Z.Z == 10 && SurfaceWalls.TryGetValue(terrainObject.GetPropertyOrTag("Terrain", ""), out value))
		{
			return value;
		}
		Location2D center = R.center;
		int num = 9999999;
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				int num2 = center.ManhattanDistance(j, i);
				if (num2 >= num)
				{
					continue;
				}
				int k = 0;
				for (int count = Z.Map[j][i].Objects.Count; k < count; k++)
				{
					if (Z.Map[j][i].Objects[k].IsWall())
					{
						value = Z.Map[j][i].Objects[k].Blueprint;
						num = num2;
						break;
					}
				}
			}
		}
		return value ?? terrainObject.GetPropertyOrTag("TerrainWall", "Shale");
	}

	private bool IsBlocker(GameObject Object)
	{
		if (Object.CanClear())
		{
			if (!Object.IsWall())
			{
				return !Object.HasTag("Plant");
			}
			return false;
		}
		return true;
	}
}
