using Genkit;
using UnityEngine;

namespace XRL.World.ZoneBuilders;

public class ISultanDungeonSegment
{
	public string mutator;

	public virtual int x1 => 0;

	public virtual int y1 => 0;

	public virtual int x2 => 0;

	public virtual int y2 => 0;

	public virtual int width()
	{
		return 0;
	}

	public virtual int height()
	{
		return 0;
	}

	public virtual bool contains(int x, int y)
	{
		return false;
	}

	public virtual bool contains(Location2D pos)
	{
		return false;
	}

	public virtual bool HasCustomColor(int x, int y)
	{
		return false;
	}

	public virtual Color32 GetCustomColor(int x, int y)
	{
		return new Color32(0, 0, 0, 0);
	}
}
