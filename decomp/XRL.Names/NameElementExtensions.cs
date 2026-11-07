using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.Names;

public static class NameElementExtensions
{
	public static string GetRandomNameElement<T>(this List<T> List, Random R = null) where T : NameElement
	{
		switch (List.Count)
		{
		case 0:
			return null;
		case 1:
			if (List[0].Weight <= 0)
			{
				return null;
			}
			return List[0].Name;
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			int num = 0;
			int i = 0;
			for (int count = List.Count; i < count; i++)
			{
				if (List[i].Weight > 0)
				{
					num += List[i].Weight;
				}
			}
			if (num <= 0)
			{
				return null;
			}
			int num2 = R.Next(0, num);
			int num3 = 0;
			int j = 0;
			for (int count2 = List.Count; j < count2; j++)
			{
				if (List[j].Weight > 0)
				{
					num3 += List[j].Weight;
					if (num2 < num3)
					{
						return List[j].Name;
					}
				}
			}
			return null;
		}
		}
	}

	public static T Find<T>(this List<T> List, string Name) where T : NameElement
	{
		foreach (T item in List)
		{
			if (item.Name == Name)
			{
				return item;
			}
		}
		return null;
	}

	public static bool Has<T>(this List<T> List, string Name) where T : NameElement
	{
		foreach (T item in List)
		{
			if (item.Name == Name)
			{
				return true;
			}
		}
		return false;
	}
}
