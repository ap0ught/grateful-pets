using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class CentralitySorter : Comparer<GameObject>
{
	public int CenterX = 40;

	public int CenterY = 12;

	public CentralitySorter(int _CenterX, int _CenterY)
	{
		CenterX = _CenterX;
		CenterY = _CenterY;
	}

	public CentralitySorter(Zone Z)
		: this(Z.Width / 2, Z.Height / 2)
	{
	}

	public override int Compare(GameObject a, GameObject b)
	{
		int num = XRL.Rules.Geometry.Distance(CenterX, CenterY, a);
		int num2 = XRL.Rules.Geometry.Distance(CenterX, CenterY, b);
		if (num > num2)
		{
			return 1;
		}
		if (num2 > num)
		{
			return -1;
		}
		return 0;
	}
}
