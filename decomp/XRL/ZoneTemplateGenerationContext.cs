using System.Collections.Generic;
using Genkit;
using XRL.World;

namespace XRL;

public class ZoneTemplateGenerationContext
{
	public Zone Z;

	public InfluenceMap Regions;

	public Dictionary<string, string> Variables = new Dictionary<string, string>();

	public List<int> PopulatedRegions = new List<int>();

	public int CurrentRegion;

	public InfluenceMapRegion currentSector => Regions.Regions[CurrentRegion];
}
