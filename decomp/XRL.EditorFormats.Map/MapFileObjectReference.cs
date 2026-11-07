namespace XRL.EditorFormats.Map;

public struct MapFileObjectReference(MapFileRegion region, int x, int y, MapFileCell cell, MapFileObjectBlueprint blueprint)
{
	public readonly MapFileRegion region = region;

	public readonly int x = x;

	public readonly int y = y;

	public readonly MapFileCell cell = cell;

	public readonly MapFileObjectBlueprint blueprint = blueprint;
}
