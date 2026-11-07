using System;

namespace XRL.World.ZoneBuilders;

[Serializable]
public enum InfluenceMapSeedStrategy
{
	FurthestPoint,
	LargestRegion,
	RandomPointFurtherThan4,
	RandomPointFurtherThan1,
	RandomPoint
}
