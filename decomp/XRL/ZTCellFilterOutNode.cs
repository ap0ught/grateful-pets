using System.Collections.Generic;
using System.Linq;
using Genkit;
using UnityEngine;
using XRL.UI;

namespace XRL;

public class ZTCellFilterOutNode : ZoneTemplateNode
{
	public override bool Execute(ZoneTemplateGenerationContext Context)
	{
		InfluenceMapRegion influenceMapRegion = Context.Regions.Regions[Context.CurrentRegion];
		InfluenceMapRegion influenceMapRegion2 = influenceMapRegion.deepCopy();
		List<Location2D> list = new List<Location2D>();
		string[] array = Filter.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Location2D loc in influenceMapRegion.Cells)
			{
				Context.Z.GetCell(loc);
				string text = array[i].ToLower();
				bool flag = false;
				if (text[0] == '!')
				{
					flag = true;
					text = text.Substring(1);
				}
				List<InfluenceMapRegion> regions = Context.Regions.Regions;
				if (text.StartsWith("regionhassemantictag:"))
				{
					string[] tags = text.Split(':')[1].Split(';');
					if (flag)
					{
						if (!Context.currentSector.Cells.Any((Location2D p) => tags.Any((string tag) => Context.Z.GetCell(p).HasSemanticTag(tag))))
						{
							list.Add(loc);
						}
					}
					else if (Context.currentSector.Cells.Any((Location2D p) => tags.Any((string tag) => Context.Z.GetCell(p).HasSemanticTag(tag))))
					{
						list.Add(loc);
					}
					continue;
				}
				switch (text)
				{
				case "reachable":
					if (flag)
					{
						if (!Context.Z.GetCell(loc).IsReachable())
						{
							list.Add(loc);
						}
					}
					else if (Context.Z.GetCell(loc).IsReachable())
					{
						list.Add(loc);
					}
					break;
				case "liquid":
					if (flag)
					{
						if (!Context.Z.GetCell(loc).HasOpenLiquidVolume())
						{
							list.Add(loc);
						}
					}
					else if (Context.Z.GetCell(loc).HasOpenLiquidVolume())
					{
						list.Add(loc);
					}
					break;
				case "furthest":
					if (!Context.Regions.Regions[0].Contains(loc))
					{
						list.Add(loc);
					}
					break;
				case "isolated":
					if (flag)
					{
						if (!regions.Any((InfluenceMapRegion R) => !R.ConnectsToTag("connection") && !R.HasTag("connected") && R.Contains(loc)))
						{
							list.Add(loc);
						}
					}
					else if (!regions.Any((InfluenceMapRegion R) => (R.HasTag("connected") || R.ConnectsToTag("connection")) && R.Contains(loc)))
					{
						list.Add(loc);
					}
					break;
				case "pocket":
					if (flag)
					{
						if (regions.Any((InfluenceMapRegion R) => R.AdjacentRegions.Count > 0 && R.Contains(loc)))
						{
							list.Add(loc);
						}
					}
					else if (regions.Any((InfluenceMapRegion R) => R.AdjacentRegions.Count == 0 && R.Contains(loc)))
					{
						list.Add(loc);
					}
					break;
				case "connection":
					if (flag)
					{
						if (!regions.Any((InfluenceMapRegion R) => !R.HasTag("connection") && R.Contains(loc)))
						{
							list.Add(loc);
						}
					}
					else if (!regions.Any((InfluenceMapRegion R) => R.HasTag("connection") && R.Contains(loc)))
					{
						list.Add(loc);
					}
					break;
				case "deadend":
					if (flag)
					{
						if (!regions.Any((InfluenceMapRegion R) => R.AdjacentRegions.Count != 1 && R.Contains(loc)))
						{
							list.Add(loc);
						}
					}
					else if (!regions.Any((InfluenceMapRegion R) => R.AdjacentRegions.Count == 1 && R.Contains(loc)))
					{
						list.Add(loc);
					}
					break;
				default:
					Debug.LogWarning("Unknown criteria: " + text);
					return false;
				}
			}
		}
		foreach (Location2D item in list)
		{
			influenceMapRegion2.removeCell(item);
		}
		Context.Regions.Regions[Context.CurrentRegion] = influenceMapRegion2;
		if (Options.DrawInfluenceMaps)
		{
			influenceMapRegion2.draw();
		}
		for (int num = 0; num < Children.Count; num++)
		{
			if (!Children[num].TestCriteria(Context))
			{
				continue;
			}
			int num2 = Children[num].CheckChance(Context);
			for (int num3 = 0; num3 < num2; num3++)
			{
				if (!Children[num].Execute(Context))
				{
					Context.Regions.Regions[Context.CurrentRegion] = influenceMapRegion;
					return false;
				}
			}
		}
		Context.Regions.Regions[Context.CurrentRegion] = influenceMapRegion;
		return true;
	}
}
