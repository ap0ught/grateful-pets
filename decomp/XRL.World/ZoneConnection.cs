using System;
using Genkit;

namespace XRL.World;

[Serializable]
public class ZoneConnection
{
	public string Type;

	public int X;

	public int Y;

	public string Object;

	public Location2D Loc2D => Location2D.Get(X, Y);

	public Point2D Pos2D => new Point2D(X, Y);

	public override string ToString()
	{
		if (Object == null)
		{
			return Type + " <no object>";
		}
		return Type + " " + Object;
	}
}
