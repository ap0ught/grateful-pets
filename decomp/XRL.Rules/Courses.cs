namespace XRL.Rules;

public static class Courses
{
	public static class Utilities
	{
		public sealed class FromDirectionIndexer
		{
			private const char FIRST = 'E';

			private const char LAST = 'w';

			private const int LENGTH = 51;

			private static readonly int[] Array;

			public int this[char Direction] => Array[Direction - 69];

			public int this[string Direction]
			{
				get
				{
					if (Direction.Length == 2)
					{
						return Array[Direction[0] - 69] | Array[Direction[1] - 69];
					}
					return Array[Direction[0] - 69];
				}
			}

			static FromDirectionIndexer()
			{
				Array = new int[51];
				Array[9] = 1;
				Array[0] = 2;
				Array[14] = 4;
				Array[18] = 8;
				Array[41] = 1;
				Array[32] = 2;
				Array[46] = 4;
				Array[50] = 8;
			}
		}
	}

	public const int NONE = 0;

	public const int NORTH = 1;

	public const int EAST = 2;

	public const int SOUTH = 4;

	public const int WEST = 8;

	public const int NORTHEAST = 3;

	public const int SOUTHEAST = 6;

	public const int SOUTHWEST = 12;

	public const int NORTHWEST = 9;

	public const int OMNI = 15;

	public const int COUNT = 8;

	public const int LENGTH = 16;

	public static readonly Utilities.FromDirectionIndexer FromDirection;

	public static readonly string[] ToDirection;

	public static readonly int[] ToIndex;

	public static readonly Pivot[] ToPivot;

	public static readonly Pivot[][] ToExpandPivot;

	public static readonly int[] Opposite;

	public static readonly int[][] Expand;

	public static readonly int[] All;

	public static readonly int[] Cardinal;

	public static readonly int[] Ordinal;

	static Courses()
	{
		FromDirection = new Utilities.FromDirectionIndexer();
		ToDirection = new string[16];
		ToIndex = new int[16];
		ToPivot = new Pivot[16];
		ToExpandPivot = new Pivot[16][];
		Opposite = new int[16];
		Expand = new int[16][];
		All = new int[8] { 9, 1, 3, 2, 6, 4, 12, 8 };
		Cardinal = new int[4] { 1, 2, 4, 8 };
		Ordinal = new int[4] { 9, 3, 6, 12 };
		string[] directionList = Directions.DirectionList;
		int[] toIndex = ToIndex;
		string[] toDirection = ToDirection;
		Pivot[] toPivot = ToPivot;
		Pivot[][] toExpandPivot = ToExpandPivot;
		int[] opposite = Opposite;
		int[][] expand = Expand;
		int[] all = All;
		int i = 0;
		for (int num = all.Length; i < num; i++)
		{
			int num2 = all[i];
			int x = 0;
			int y = 0;
			if ((num2 & 1) != 0)
			{
				y = -1;
			}
			else if ((num2 & 4) != 0)
			{
				y = 1;
			}
			if ((num2 & 2) != 0)
			{
				x = 1;
			}
			else if ((num2 & 8) != 0)
			{
				x = -1;
			}
			toIndex[num2] = i;
			toDirection[num2] = directionList[i];
			toPivot[num2] = new Pivot(x, y);
			opposite[num2] = ((num2 << 2) | (num2 >> 2)) & 0xF;
		}
		int j = 0;
		for (int num3 = all.Length; j < num3; j++)
		{
			int num4 = all[j];
			int num5 = 0;
			expand[num4] = new int[8];
			toExpandPivot[num4] = new Pivot[8];
			RingSpanEnumerator<int> enumerator = IterateAdjacent(num4, IncludeSelf: true).GetEnumerator();
			while (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				expand[num4][num5] = current;
				toExpandPivot[num4][num5++] = toPivot[current];
			}
		}
		toDirection[0] = ".";
		toDirection[15] = "?";
	}

	public static RingSpanEnumerator<int> IterateAdjacent(int Course, bool IncludeSelf = false, bool Invert = false)
	{
		return All.IterateRingAt(ToIndex[Course], IncludeSelf, Invert);
	}
}
