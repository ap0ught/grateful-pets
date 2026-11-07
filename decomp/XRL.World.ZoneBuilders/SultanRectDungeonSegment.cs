using Genkit;
using UnityEngine;

namespace XRL.World.ZoneBuilders;

public class SultanRectDungeonSegment : ISultanDungeonSegment
{
	private Rect2D rect;

	public override int x1 => rect.x1;

	public override int y1 => rect.y1;

	public override int x2 => rect.x2;

	public override int y2 => rect.y2;

	public SultanRectDungeonSegment(Rect2D r)
	{
		rect = r;
	}

	public override int width()
	{
		return rect.Width;
	}

	public override int height()
	{
		return rect.Height;
	}

	public override bool HasCustomColor(int x, int y)
	{
		return false;
	}

	public override Color32 GetCustomColor(int x, int y)
	{
		return new Color32(0, 0, 0, 0);
	}

	public override bool contains(int x, int y)
	{
		return contains(Location2D.Get(x, y));
	}

	public override bool contains(Location2D pos)
	{
		return rect.Contains(pos.X, pos.Y);
	}
}
