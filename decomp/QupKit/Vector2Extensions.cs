using UnityEngine;

namespace QupKit;

public static class Vector2Extensions
{
	public static Vector2i ToIntVector2(this Vector2 vector2)
	{
		int[] array = new int[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = Mathf.RoundToInt(vector2[i]);
		}
		return new Vector2i(array);
	}
}
