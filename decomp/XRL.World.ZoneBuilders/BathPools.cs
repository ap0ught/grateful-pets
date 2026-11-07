using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class BathPools
{
	public bool BuildZone(Zone Z)
	{
		int num = 10;
		int num2 = 3;
		int num3 = 60;
		int num4 = 18;
		Z.FillBox(new Box(0, 0, Z.Width - 1, Z.Height - 1), "Marble");
		Z.ClearBox(new Box(num, num2, num + num3, num2 + num4));
		NoiseMap noiseMap = new NoiseMap(num3, num4, 10, 1, 1, Stat.Random(40, 50), Stat.Random(50, 70), Stat.Random(125, 135), 0, 10, 0, 1, null, 5);
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				int num5 = i;
				int num6 = j;
				if (i >= num3 / 2)
				{
					num5 = num3 - i;
				}
				if (j >= num4 / 2)
				{
					num6 = num4 - j;
				}
				if (!((float)noiseMap.Noise[num5, num6] > 0.5f))
				{
					continue;
				}
				bool flag = true;
				for (int k = -1; k <= 1; k++)
				{
					int num7 = -1;
					while (num7 <= 1)
					{
						if (num5 + k <= 0 || num5 + k >= 80 || num6 + num7 <= 0 || num6 + num7 >= 24 || (num7 == 0 && k == 0) || !((float)noiseMap.Noise[num5 + k, num6 + num7] <= 0.5f))
						{
							num7++;
							continue;
						}
						goto IL_0116;
					}
					continue;
					IL_0116:
					flag = false;
					break;
				}
				if (flag)
				{
					Z.GetCell(i + num, j + num2).Clear();
					Z.GetCell(i + num, j + num2).AddObject("ConvalessenceDeepPool");
				}
				else
				{
					Z.GetCell(i + num, j + num2).Clear();
					Z.GetCell(i + num, j + num2).AddObject("CyanMarbleWalkway");
				}
			}
		}
		int l = 0;
		int num8 = 0;
		for (; l < 1000; l++)
		{
			if (num8 >= 5)
			{
				break;
			}
			int num9 = Stat.Random(4, num3 / 2 - 1);
			int num10 = Stat.Random(3, num4 / 2 - 1);
			bool flag2 = false;
			for (int m = -1; m <= 1; m++)
			{
				int num11 = -1;
				while (num11 <= 1)
				{
					if (num9 + m <= 0 || num9 + m >= num3 || num10 + num11 <= 0 || num10 + num11 >= num4 || !((float)noiseMap.Noise[num9 + m, num10 + num11] > 0.5f))
					{
						num11++;
						continue;
					}
					goto IL_022d;
				}
				continue;
				IL_022d:
				flag2 = true;
				break;
			}
			if (!flag2)
			{
				num8++;
				Z.GetCell(num9 + num, num10 + num2).AddObject("Marble");
				Z.GetCell(num9 + num, num4 - num10 + num2).AddObject("Marble");
				Z.GetCell(num3 - num9 + num, num10 + num2).AddObject("Marble");
				Z.GetCell(num3 - num9 + num, num4 - num10 + num2).AddObject("Marble");
			}
		}
		Box box = new Box(num - 1, num2 + num4 - 3, num + 3, num2 + num4 + 1);
		Box box2 = new Box(num + num3 - 3, num2 + num4 - 3, num + num3 + 1, num2 + num4 + 1);
		if (Z.Z % 2 == 0)
		{
			Z.ClearBox(box);
		}
		else
		{
			Z.ClearBox(box2);
		}
		Point p = new Point(num + 3, num2 + num4 - 1);
		Point p2 = new Point(num + num3 - 3, num2 + num4 - 1);
		Point p3 = new Point(num + 1, num2 + num4 - 1);
		Point p4 = new Point(num + num3 - 1, num2 + num4 - 1);
		Z.GetCell(p).Clear();
		Z.GetCell(p2).Clear();
		for (int n = box.y1; n <= box.y2; n++)
		{
			for (int num12 = box.x1; num12 <= box.x2; num12++)
			{
				if ((num12 + n) % 2 == 0)
				{
					Z.GetCell(num12, n).AddObject("WhiteTile");
				}
				else
				{
					Z.GetCell(num12, n).AddObject("GreenTile");
				}
			}
		}
		for (int num13 = box2.y1; num13 <= box2.y2; num13++)
		{
			for (int num14 = box2.x1; num14 <= box2.x2; num14++)
			{
				if ((num14 + num13) % 2 == 0)
				{
					Z.GetCell(num14, num13).AddObject("WhiteTile");
				}
				else
				{
					Z.GetCell(num14, num13).AddObject("GreenTile");
				}
			}
		}
		if (Z.Z % 2 == 0)
		{
			Z.FillHollowBox(box, "Marble");
		}
		if (Z.Z % 2 != 0)
		{
			Z.FillHollowBox(box2, "Marble");
		}
		string text = "Door";
		string text2 = "Door";
		if (Z.Z == 12)
		{
			text2 = "Troll Door 1";
		}
		if (Z.Z == 13)
		{
			text = "Troll Door 2";
		}
		if (Z.Z == 14)
		{
			text2 = "Troll Door 3";
		}
		if (text2 != "Door")
		{
			Z.GetCell(p).Clear();
			Z.GetCell(p).AddObject(text2);
		}
		if (text != "Door")
		{
			Z.GetCell(p2).Clear();
			Z.GetCell(p2).AddObject(text);
		}
		if (Z.Z % 2 == 0)
		{
			Z.GetCell(p3).AddObject("StairsDown");
			Z.GetCell(p4).AddObject("StairsUp");
		}
		else
		{
			Z.GetCell(p3).AddObject("StairsUp");
			Z.GetCell(p4).AddObject("StairsDown");
		}
		Z.ClearReachableMap();
		for (int num15 = 10; num15 < 20; num15++)
		{
			int num16 = 10;
			while (num16 < 20)
			{
				if (!Z.GetCell(num15, num16).IsEmpty())
				{
					num16++;
					continue;
				}
				goto IL_05e8;
			}
			continue;
			IL_05e8:
			Z.BuildReachableMap(num15, num16);
			break;
		}
		return true;
	}
}
