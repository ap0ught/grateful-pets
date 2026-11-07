using System.Collections.Generic;

public class ControlSorterByPriority : IComparer<exUIControl>
{
	public int Compare(exUIControl _a, exUIControl _b)
	{
		int num = _a.priority;
		int num2 = _b.priority;
		if (_a == _b)
		{
			return 0;
		}
		if (!_a.gameObject.activeInHierarchy || !_a.activeInHierarchy)
		{
			num = -999;
		}
		if (!_b.gameObject.activeInHierarchy || !_b.activeInHierarchy)
		{
			num2 = -999;
		}
		if (num == num2)
		{
			return -1;
		}
		return num2 - num;
	}
}
