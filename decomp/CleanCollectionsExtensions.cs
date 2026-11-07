using System.Collections.Generic;

public static class CleanCollectionsExtensions
{
	public static bool CleanContains<T>(this List<T> list, T item)
	{
		if (item == null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == null)
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] != null && list[j].Equals(item))
				{
					return true;
				}
			}
		}
		return false;
	}
}
