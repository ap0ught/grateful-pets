using System.Collections.Generic;
using UnityEngine;

public class ControlSorterByZ : IComparer<exUIControl>
{
	public int Compare(exUIControl _a, exUIControl _b)
	{
		int num = Mathf.CeilToInt(_a.transform.position.z - _b.transform.position.z);
		if (num != 0)
		{
			return num;
		}
		return _a.GetInstanceID() - _b.GetInstanceID();
	}
}
