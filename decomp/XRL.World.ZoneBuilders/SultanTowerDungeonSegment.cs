using Genkit;

namespace XRL.World.ZoneBuilders;

public class SultanTowerDungeonSegment(Location2D pos, int radius, int thickness) : SultanCircleDungeonSegment(pos, radius)
{
	private int thickness = 2;

	public override bool HasCustomColor(int x, int y)
	{
		Location2D location2D = Location2D.Get(x, y);
		Point2D point2D = new Point2D(location2D.X - center.X, location2D.Y - center.Y);
		point2D.x = (int)((float)point2D.x / 1.5f);
		if (point2D.Distance(new Point2D(0, 0)) >= radius - thickness)
		{
			return true;
		}
		return false;
	}
}
