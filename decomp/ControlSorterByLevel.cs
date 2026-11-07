using System.Collections.Generic;

public class ControlSorterByLevel : IComparer<exUIControl>
{
	public int Compare(exUIControl _a, exUIControl _b)
	{
		exUIControl exUIControl2 = null;
		int num = 0;
		int num2 = 0;
		exUIControl2 = _a.parent;
		while ((bool)exUIControl2)
		{
			num++;
			exUIControl2 = exUIControl2.parent;
		}
		exUIControl2 = _b.parent;
		while ((bool)exUIControl2)
		{
			num2++;
			exUIControl2 = exUIControl2.parent;
		}
		return num - num2;
	}
}
