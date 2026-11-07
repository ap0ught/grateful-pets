using System;

namespace XRL.World.Parts;

[Serializable]
public class Pair
{
	public int x;

	public int y;

	public Pair()
	{
		x = 0;
		y = 0;
	}

	public Pair(int xp, int yp)
	{
		x = xp;
		y = yp;
	}
}
