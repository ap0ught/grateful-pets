using System.Collections.Generic;

public class ControlSorterByPriority2 : IComparer<exUIControl>
{
	public int Compare(exUIControl _a, exUIControl _b)
	{
		exUIControl exUIControl2 = null;
		int num = _a.priority;
		int num2 = _b.priority;
		exUIControl2 = _a.parent;
		while ((bool)exUIControl2)
		{
			num += exUIControl2.priority;
			exUIControl2 = exUIControl2.parent;
		}
		exUIControl2 = _b.parent;
		while ((bool)exUIControl2)
		{
			num2 += exUIControl2.priority;
			exUIControl2 = exUIControl2.parent;
		}
		return num - num2;
	}
}
