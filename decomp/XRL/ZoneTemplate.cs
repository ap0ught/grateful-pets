using System.Collections.Generic;
using Genkit;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.ZoneBuilders;

namespace XRL;

public class ZoneTemplate
{
	public int RegionSize = 100;

	public string Name;

	public ZoneTemplateNode Root = new ZTGroupNode();

	public ZoneTemplateNode GlobalRoot = new ZTGroupNode();

	public ZoneTemplateNode SingleRoot = new ZTGroupNode();

	public bool Execute(Zone Z, InfluenceMap influenceMap = null)
	{
		ZoneTemplateGenerationContext zoneTemplateGenerationContext = new ZoneTemplateGenerationContext();
		zoneTemplateGenerationContext.CurrentRegion = 0;
		zoneTemplateGenerationContext.Z = Z;
		if (influenceMap != null)
		{
			zoneTemplateGenerationContext.Regions = influenceMap;
		}
		else
		{
			zoneTemplateGenerationContext.Regions = GenerateInfluenceMap(Z, new List<Point>(), InfluenceMapSeedStrategy.RandomPointFurtherThan4, RegionSize);
		}
		foreach (InfluenceMapRegion region in zoneTemplateGenerationContext.Regions.Regions)
		{
			bool flag = false;
			for (int i = 0; i < zoneTemplateGenerationContext.Z.ZoneConnectionCache.Count; i++)
			{
				if (region.PointIn(zoneTemplateGenerationContext.Z.ZoneConnectionCache[i].Pos2D))
				{
					region.Tags.Add("connection");
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			List<ZoneConnection> zoneConnections = XRLCore.Core.Game.ZoneManager.GetZoneConnections(zoneTemplateGenerationContext.Z.ZoneID);
			for (int j = 0; j < zoneConnections.Count; j++)
			{
				if (region.PointIn(zoneConnections[j].Pos2D))
				{
					region.Tags.Add("connection");
					break;
				}
			}
		}
		zoneTemplateGenerationContext.Variables = new Dictionary<string, string>();
		zoneTemplateGenerationContext.Variables.Add("{zonetier}", Z.NewTier.ToString());
		zoneTemplateGenerationContext.Variables.Add("{faction0}", PopulationManager.RollOneFrom("RandomZoneTemplateFaction").Blueprint);
		zoneTemplateGenerationContext.Variables.Add("{faction1}", PopulationManager.RollOneFrom("RandomZoneTemplateFaction").Blueprint);
		zoneTemplateGenerationContext.Variables.Add("{faction2}", PopulationManager.RollOneFrom("RandomZoneTemplateFaction").Blueprint);
		List<int> list = new List<int>();
		for (int k = 0; k < zoneTemplateGenerationContext.Regions.Regions.Count; k++)
		{
			list.Add(k);
		}
		Algorithms.RandomShuffleInPlace(list);
		int currentRegion = zoneTemplateGenerationContext.Regions.AddGlobalRegion();
		zoneTemplateGenerationContext.CurrentRegion = currentRegion;
		if (!GlobalRoot.Execute(zoneTemplateGenerationContext))
		{
			return true;
		}
		zoneTemplateGenerationContext.CurrentRegion = list.GetRandomElement();
		if (!SingleRoot.Execute(zoneTemplateGenerationContext))
		{
			return true;
		}
		for (int l = 0; l < list.Count; l++)
		{
			zoneTemplateGenerationContext.CurrentRegion = list[l];
			Execute(zoneTemplateGenerationContext);
			if (!zoneTemplateGenerationContext.PopulatedRegions.Contains(zoneTemplateGenerationContext.CurrentRegion))
			{
				zoneTemplateGenerationContext.PopulatedRegions.Add(zoneTemplateGenerationContext.CurrentRegion);
			}
		}
		return true;
	}

	private bool Execute(ZoneTemplateGenerationContext Context)
	{
		if (!Root.Execute(Context))
		{
			return false;
		}
		return true;
	}

	public static InfluenceMap GenerateInfluenceMap(Zone Z, List<Point> AdditionalSeeds, InfluenceMapSeedStrategy SeedStrategy, int MaxRegionSize, List<Point2D> AdditionalWalls = null, bool bDraw = false)
	{
		InfluenceMap influenceMap = new InfluenceMap(Z.Width, Z.Height);
		influenceMap.bDraw = bDraw;
		Z.SetInfluenceMapWalls(influenceMap.Walls);
		if (AdditionalWalls != null)
		{
			foreach (Point2D AdditionalWall in AdditionalWalls)
			{
				influenceMap.Walls[AdditionalWall.x, AdditionalWall.y] = 1;
			}
		}
		if (AdditionalSeeds != null)
		{
			foreach (Point AdditionalSeed in AdditionalSeeds)
			{
				influenceMap.AddSeed(AdditionalSeed.X, AdditionalSeed.Y, bRecalculate: false);
			}
		}
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (!Z.GetCell(zoneConnection.X, zoneConnection.Y).IsSolid() && !Z.GetCell(zoneConnection.X, zoneConnection.Y).HasObjectWithTag("InfluenceMapBlocker"))
			{
				influenceMap.AddSeed(zoneConnection.X, zoneConnection.Y, bRecalculate: false);
			}
		}
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-" && !Z.GetCell(item.X, item.Y).IsSolid() && !Z.GetCell(item.X, item.Y).HasObjectWithTag("InfluenceMapBlocker"))
			{
				influenceMap.AddSeed(item.X, item.Y, bRecalculate: false);
			}
		}
		if (influenceMap.Seeds.Count > 0)
		{
			influenceMap.Recalculate();
		}
		List<Cell> cells = Z.GetCells();
		Algorithms.RandomShuffleInPlace(cells, Stat.Rand);
		for (int i = 0; i < cells.Count; i++)
		{
			if (influenceMap.GetSeedAt(Location2D.Get(cells[i].X, cells[i].Y)) == -1 && cells[i].IsEmpty() && !cells[i].HasObjectWithTag("InfluenceMapBlocker"))
			{
				influenceMap.AddSeed(cells[i].X, cells[i].Y);
				influenceMap.Recalculate();
			}
		}
		influenceMap.bDraw = bDraw;
		int num = 50;
		while (num >= 0 && (influenceMap.Seeds.Count == 0 || influenceMap.LargestSize() > MaxRegionSize))
		{
			if (SeedStrategy == InfluenceMapSeedStrategy.LargestRegion)
			{
				influenceMap.AddSeedAtMaximaInLargestSeed();
			}
			if (SeedStrategy == InfluenceMapSeedStrategy.FurthestPoint)
			{
				influenceMap.AddSeedAtMaxima();
			}
			if (SeedStrategy == InfluenceMapSeedStrategy.RandomPointFurtherThan4)
			{
				influenceMap.AddSeedAtRandom(5);
			}
			if (SeedStrategy == InfluenceMapSeedStrategy.RandomPointFurtherThan1)
			{
				influenceMap.AddSeedAtRandom(0);
			}
			num--;
		}
		if (influenceMap.Seeds.Count == 0)
		{
			Z.ClearWalkableBorders();
			Z.SetInfluenceMapWalls(influenceMap.Walls);
			influenceMap.AddSeedAtRandom(0);
			if (influenceMap.Seeds.Count == 0)
			{
				Z.GetCell(0, 0).Clear();
				Z.SetInfluenceMapWalls(influenceMap.Walls);
				influenceMap.AddSeedAtRandom(0);
			}
		}
		influenceMap.bDraw = bDraw;
		influenceMap.Recalculate();
		if (bDraw || Options.DrawInfluenceMaps)
		{
			influenceMap.Draw(bDrawConnectionMap: false);
		}
		return influenceMap;
	}
}
