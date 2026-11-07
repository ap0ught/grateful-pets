using System;
using Genkit;
using UnityEngine;

namespace XRL.World.ZoneBuilders;

public class SultanCircleDungeonSegment : ISultanDungeonSegment
{
	protected Location2D center;

	protected int radius;

	public override int x1 => Math.Min(79, Math.Max(0, center.X - (int)((double)radius * 1.5)));

	public override int y1 => Math.Min(24, Math.Max(0, center.Y - radius));

	public override int x2 => Math.Max(0, Math.Min(79, center.X + (int)((double)radius * 1.5)));

	public override int y2 => Math.Max(0, Math.Min(24, center.Y + radius));

	public SultanCircleDungeonSegment(Location2D pos, int radius)
	{
		center = pos;
		this.radius = radius;
	}

	public override int width()
	{
		return (int)(2.0 * ((double)radius * 1.5)) + 1;
	}

	public override int height()
	{
		return 2 * radius + 1;
	}

	public override bool HasCustomColor(int x, int y)
	{
		return false;
	}

	public override Color32 GetCustomColor(int x, int y)
	{
		return ColorOutputMap.BLACK;
	}

	public override bool contains(int x, int y)
	{
		return contains(Location2D.Get(x, y));
	}

	public override bool contains(Location2D location)
	{
		double num = (double)radius * 1.5;
		double num2 = radius;
		if (num <= 0.0 || num2 <= 0.0)
		{
			return false;
		}
		Point point = new Point(location.X - center.X, location.Y - center.Y);
		return (double)(point.X * point.X) / (num * num) + (double)(point.Y * point.Y) / (num2 * num2) <= 1.0;
	}
}
