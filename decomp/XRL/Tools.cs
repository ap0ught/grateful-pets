using System;
using System.Collections.Generic;
using XRL.Collections;
using XRL.Rules;
using XRL.World;

namespace XRL;

public class Tools
{
	[ThreadStatic]
	private static bool[,] FillVisited;

	[ThreadStatic]
	private static RingDeque<(int, int, int, int)> FillQueue;

	public static bool BoxOverlap(Box Box1, Box Box2)
	{
		if (Box1.x1 > Box2.x2)
		{
			return false;
		}
		if (Box1.x2 < Box2.x1)
		{
			return false;
		}
		if (Box1.y1 > Box2.y2)
		{
			return false;
		}
		if (Box1.y2 < Box2.y1)
		{
			return false;
		}
		return true;
	}

	public static List<Box> GenerateBoxes(BoxGenerateOverlap Overlap, Range NumberOfBoxees, Range Width, Range Height, Range Volume, Range XRange = null, Range YRange = null)
	{
		return GenerateBoxes(new List<Box>(), Overlap, NumberOfBoxees, Width, Height, Volume, (XRange == null) ? new Range(0, 79) : XRange, (YRange == null) ? new Range(0, 24) : YRange);
	}

	public static List<Box> GenerateBoxes(Box OutOfBounds, BoxGenerateOverlap Overlap, Range NumberOfBoxees, Range Width, Range Height, Range Volume)
	{
		return GenerateBoxes(new List<Box> { OutOfBounds }, Overlap, NumberOfBoxees, Width, Height, Volume, new Range(0, 79), new Range(0, 24));
	}

	public static List<Box> GenerateBoxes(List<Box> OutOfBounds, BoxGenerateOverlap Overlap, Range NumberOfBoxees, Range Width, Range Height, Range Volume, Range XRange, Range YRange)
	{
		List<Box> list = new List<Box>();
		while (true)
		{
			int num = Stat.Random(NumberOfBoxees.Min, NumberOfBoxees.Max);
			list.Clear();
			for (int i = 0; i < num; i++)
			{
				int num2 = 5000;
				while (num2 > 0)
				{
					Box box;
					while (true)
					{
						IL_0030:
						num2--;
						box = new Box(Stat.Random(XRange.Min, XRange.Max), Stat.Random(YRange.Min, YRange.Max), Stat.Random(XRange.Min, XRange.Max), Stat.Random(YRange.Min, YRange.Max));
						if (box.Width < Width.Min || box.Width > Width.Max || box.Height < Height.Min || box.Height > Height.Max || (Volume != null && (box.Width * box.Height < Volume.Min || box.Width * box.Height > Volume.Max)))
						{
							break;
						}
						foreach (Box OutOfBound in OutOfBounds)
						{
							if (BoxOverlap(OutOfBound, box))
							{
								goto IL_0030;
							}
						}
						goto IL_013a;
					}
					continue;
					IL_013a:
					list.Add(box);
					break;
				}
			}
			if (Overlap == BoxGenerateOverlap.Irrelevant)
			{
				break;
			}
			using List<Box>.Enumerator enumerator = list.GetEnumerator();
			while (true)
			{
				if (enumerator.MoveNext())
				{
					Box current = enumerator.Current;
					foreach (Box item in list)
					{
						if (current != item && ((Overlap == BoxGenerateOverlap.AlwaysOverlap && !BoxOverlap(current, item)) || (Overlap == BoxGenerateOverlap.NeverOverlap && BoxOverlap(current, item))))
						{
							goto end_IL_01c8;
						}
					}
					continue;
				}
				return list;
				continue;
				end_IL_01c8:
				break;
			}
		}
		return list;
	}

	public static void Box(Zone Z, Box B, string Blueprint, int Chance)
	{
		for (int i = B.y1; i <= B.y2; i++)
		{
			for (int j = B.x1; j <= B.x2; j++)
			{
				if ((j == B.x1 || j == B.x2 || i == B.y1 || i == B.y2) && Stat.Random(1, 100) <= Chance)
				{
					Z.GetCell(j, i).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
				}
			}
		}
	}

	public static void FillBox(Zone Z, Box B, string Blueprint)
	{
		for (int i = B.y1; i <= B.y2; i++)
		{
			for (int j = B.x1; j <= B.x2; j++)
			{
				Z.GetCell(j, i).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			}
		}
	}

	public static void FillBox(Zone Z, Box B, string Blueprint, int Chance)
	{
		for (int i = B.y1; i <= B.y2; i++)
		{
			for (int j = B.x1; j <= B.x2; j++)
			{
				if (Z.GetCell(j, i).IsEmpty() && Stat.Random(1, 100) <= Chance)
				{
					Z.GetCell(j, i).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
				}
			}
		}
	}

	public static void ClearBox(Zone Z, Box B)
	{
		for (int i = B.x1; i <= B.x2; i++)
		{
			for (int j = B.y1; j <= B.y2; j++)
			{
				Z.GetCell(i, j).Clear();
			}
		}
	}

	public static void ScanFill(int StartX, int StartY, int Width, int Height, Func<int, int, bool> Get, Action<int, int> Set, bool Diagonal = false)
	{
		if (!Get(StartX, StartY))
		{
			return;
		}
		bool[,] array = FillVisited;
		RingDeque<(int, int, int, int)> ringDeque = FillQueue;
		if (array == null)
		{
			array = (FillVisited = new bool[Width, Height]);
			ringDeque = (FillQueue = new RingDeque<(int, int, int, int)>(16));
		}
		else
		{
			ringDeque.Clear();
			int upperBound = array.GetUpperBound(0);
			int upperBound2 = array.GetUpperBound(1);
			if (upperBound < Width || upperBound2 < Height)
			{
				array = (FillVisited = new bool[Math.Max(upperBound, Width), Math.Max(upperBound2, Height)]);
			}
			else
			{
				Array.Clear(array, 0, array.Length);
			}
		}
		ringDeque.Enqueue((StartX, StartX, StartY, 1));
		ringDeque.Enqueue((StartX, StartX, StartY - 1, -1));
		int num = Width - 1;
		int num2 = Height - 1;
		int num3 = ((!Diagonal) ? 1 : 0);
		(int, int, int, int) Value;
		while (ringDeque.TryDequeue(out Value))
		{
			int num6;
			int num7;
			int num4;
			int num5;
			(num4, num5, num6, num7) = Value;
			if (num6 < 0 || num6 > num2)
			{
				continue;
			}
			if (Diagonal)
			{
				if (num4 > 0)
				{
					num4--;
				}
				if (num5 < num)
				{
					num5++;
				}
			}
			int num8 = num4;
			while (num8 >= 0 && !array[num8, num6])
			{
				array[num8, num6] = true;
				if (!Get(num8, num6))
				{
					break;
				}
				Set(num8, num6);
				num8--;
			}
			num8++;
			num4++;
			if (num8 < num4)
			{
				int num9 = num8;
				if (num9 < num4 - num3)
				{
					ringDeque.Enqueue((num9, num4 - 1, num6 - num7, -num7));
				}
				for (num8 = num4; num8 <= num && !array[num8, num6]; num8++)
				{
					array[num8, num6] = true;
					if (!Get(num8, num6))
					{
						break;
					}
					Set(num8, num6);
				}
				ringDeque.Enqueue((num9, num8 - 1, num6 + num7, num7));
				num4 = num8 + 1;
				if (num8 - num3 > num5)
				{
					ringDeque.Enqueue((num5, num8 - 1, num6 - num7, -num7));
				}
			}
			for (num8 = num5; num8 <= num && !array[num8, num6]; num8++)
			{
				array[num8, num6] = true;
				if (!Get(num8, num6))
				{
					break;
				}
				Set(num8, num6);
			}
			num8--;
			num5--;
			if (num8 > num5)
			{
				int num9 = num8;
				if (num9 > num5 + num3)
				{
					ringDeque.Enqueue((num5 + 1, num9, num6 - num7, -num7));
				}
				num8 = num5;
				while (num8 >= num4 && !array[num8, num6])
				{
					array[num8, num6] = true;
					if (!Get(num8, num6))
					{
						break;
					}
					Set(num8, num6);
					num8--;
				}
				if (num9 > num8)
				{
					ringDeque.Enqueue((num8 + 1, num9, num6 + num7, num7));
				}
				num5 = num8 - 1;
			}
			while (num4 <= num5 && num8 <= num)
			{
				for (num8 = num4; num8 <= num5 && !array[num8, num6]; num8++)
				{
					array[num8, num6] = true;
					if (!Get(num8, num6))
					{
						break;
					}
					Set(num8, num6);
				}
				if (num8 > num4)
				{
					ringDeque.Enqueue((num4, num8 - 1, num6 + num7, num7));
				}
				num4 = num8 + 1;
			}
		}
	}
}
