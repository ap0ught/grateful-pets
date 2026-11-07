namespace XRL.EditorFormats.Map;

public struct MapFileCellReference(MapFileRegion region, int x, int y, MapFileCell cell)
{
	public readonly MapFileRegion region = region;

	public readonly int x = x;

	public readonly int y = y;

	public readonly MapFileCell cell = cell;
}
