using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.ZoneBuilders.Utility;

public class NoiseMap
{
	public int Width;

	public int Height;

	public int CutoffDepth;

	public int[,] Seeds;

	public int[,] Noise;

	public int[,] Areas;

	public int nAreas;

	public Dictionary<int, List<NoiseMapNode>> AreaNodes;

	public List<Location2D> PlacedSeeds = new List<Location2D>();

	public NoiseMap Copy(int x1, int y1, int x2, int y2)
	{
		NoiseMap noiseMap = new NoiseMap();
		noiseMap.Width = x2 - x1 + 1;
		noiseMap.Height = y2 - y1 + 1;
		noiseMap.Seeds = new int[noiseMap.Width, noiseMap.Height];
		noiseMap.Areas = new int[noiseMap.Width, noiseMap.Height];
		if (Noise != null)
		{
			noiseMap.Noise = new int[noiseMap.Width, noiseMap.Height];
			for (int i = x1; i <= x2; i++)
			{
				for (int j = y1; j <= y2; j++)
				{
					noiseMap.Noise[i - x1, j - y1] = Noise[x1, y1];
					noiseMap.Areas[i - x1, j - y1] = Areas[x1, y1];
				}
			}
		}
		return noiseMap;
	}

	public NoiseMap()
	{
	}

	public NoiseMap(int _Width, int _Height, int MaxDepth, int SectorsWide, int SectorsHigh, string SeedsPerSector, int MinSeedDepth, int MaxSeedDepth, int BaseNoise, int FilterPasses, int BorderWidth, int _CutoffDepth, List<NoiseMapNode> ExtraNodes)
	{
		CreateNoiseMap(_Width, _Height, MaxDepth, SectorsWide, SectorsHigh, SeedsPerSector, MinSeedDepth, MaxSeedDepth, BaseNoise, FilterPasses, BorderWidth, _CutoffDepth, ExtraNodes, -1);
	}

	public NoiseMap(int _Width, int _Height, int MaxDepth, int SectorsWide, int SectorsHigh, int SeedsPerSector, int MinSeedDepth, int MaxSeedDepth, int BaseNoise, int FilterPasses, int BorderWidth, int _CutoffDepth, List<NoiseMapNode> ExtraNodes)
	{
		CreateNoiseMap(_Width, _Height, MaxDepth, SectorsWide, SectorsHigh, SeedsPerSector.ToString(), MinSeedDepth, MaxSeedDepth, BaseNoise, FilterPasses, BorderWidth, _CutoffDepth, ExtraNodes, -1);
	}

	public NoiseMap(int _Width, int _Height, int MaxDepth, int SectorsWide, int SectorsHigh, int SeedsPerSector, int MinSeedDepth, int MaxSeedDepth, int BaseNoise, int FilterPasses, int BorderWidth, int _CutoffDepth, List<NoiseMapNode> ExtraNodes, int iFilterBorder)
	{
		CreateNoiseMap(_Width, _Height, MaxDepth, SectorsWide, SectorsHigh, SeedsPerSector.ToString(), MinSeedDepth, MaxSeedDepth, BaseNoise, FilterPasses, BorderWidth, _CutoffDepth, ExtraNodes, iFilterBorder);
	}

	public NoiseMap(int _Width, int _Height, int MaxDepth, int SectorsWide, int SectorsHigh, int SeedsPerSector, int MinSeedDepth, int MaxSeedDepth, int BaseNoise, int FilterPasses, int BorderWidth, int _CutoffDepth, List<NoiseMapNode> ExtraNodes, int iFilterBorder, int MaximumSeeds)
	{
		CreateNoiseMap(_Width, _Height, MaxDepth, SectorsWide, SectorsHigh, SeedsPerSector.ToString(), MinSeedDepth, MaxSeedDepth, BaseNoise, FilterPasses, BorderWidth, _CutoffDepth, ExtraNodes, iFilterBorder, MaximumSeeds);
	}

	public NoiseMap(int _Width, int _Height, int MaxDepth, int SectorsWide, int SectorsHigh, string SeedsPerSector, int MinSeedDepth, int MaxSeedDepth, int BaseNoise, int FilterPasses, int BorderWidth, int _CutoffDepth, List<NoiseMapNode> ExtraNodes, int iFilterBorder, int MaximumSeeds)
	{
		CreateNoiseMap(_Width, _Height, MaxDepth, SectorsWide, SectorsHigh, SeedsPerSector, MinSeedDepth, MaxSeedDepth, BaseNoise, FilterPasses, BorderWidth, _CutoffDepth, ExtraNodes, iFilterBorder, MaximumSeeds);
	}

	public void FastQuadPlasma(int Width, int Height, int MinStartHeight, int MaxStartHeight, int RoughnessMin, int RoughnessMax)
	{
		if (Width % 2 == 0)
		{
			Width++;
		}
		if (Height % 2 == 0)
		{
			Height++;
		}
		if (Width < Height)
		{
			Height = Width;
		}
		if (Height < Width)
		{
			Height = Width;
		}
		Noise = new int[Width, Height];
		Areas = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Noise[i, j] = int.MaxValue;
			}
		}
		Stack<Box> stack = new Stack<Box>();
		Noise[0, 0] = Stat.Random(MinStartHeight, MaxStartHeight);
		Noise[Width - 1, 0] = Stat.Random(MinStartHeight, MaxStartHeight);
		Noise[0, Height - 1] = Stat.Random(MinStartHeight, MaxStartHeight);
		Noise[Width - 1, Height - 1] = Stat.Random(MinStartHeight, MaxStartHeight);
		stack.Push(new Box(0, 0, Width - 1, Height - 1));
		while (stack.Count > 0)
		{
			Box box = stack.Pop();
			if (box.Width > 1)
			{
				if (Noise[box.MidX, box.y1] == int.MaxValue)
				{
					Noise[box.MidX, box.y1] = (Noise[box.x1, box.y1] + Noise[box.x2, box.y1]) / 2 + Stat.Random(RoughnessMin, RoughnessMax);
				}
				if (Noise[box.MidX, box.y2] == int.MaxValue)
				{
					Noise[box.MidX, box.y2] = (Noise[box.x1, box.y2] + Noise[box.x2, box.y2]) / 2 + Stat.Random(RoughnessMin, RoughnessMax);
				}
				if (Noise[box.x1, box.MidY] == int.MaxValue)
				{
					Noise[box.x1, box.MidY] = (Noise[box.x1, box.y1] + Noise[box.x1, box.y2]) / 2 + Stat.Random(RoughnessMin, RoughnessMax);
				}
				if (Noise[box.x2, box.MidY] == int.MaxValue)
				{
					Noise[box.x2, box.MidY] = (Noise[box.x2, box.y1] + Noise[box.x2, box.y2]) / 2 + Stat.Random(RoughnessMin, RoughnessMax);
				}
				if (Noise[box.MidX, box.MidY] == int.MaxValue)
				{
					Noise[box.MidX, box.MidY] = (Noise[box.x2, box.y1] + Noise[box.x2, box.y2] + Noise[box.x1, box.y1] + Noise[box.x1, box.y2]) / 4 + Stat.Random(RoughnessMin, RoughnessMax);
				}
				if (box.Width > 2)
				{
					stack.Push(new Box(box.x1, box.y1, box.MidX, box.MidY));
					stack.Push(new Box(box.MidX, box.y1, box.x2, box.MidY));
					stack.Push(new Box(box.x1, box.MidY, box.MidX, box.y2));
					stack.Push(new Box(box.MidX, box.MidY, box.x2, box.y2));
				}
			}
		}
	}

	public void CreateNoiseMap(int _Width, int _Height, int MaxDepth, int SectorsWide, int SectorsHigh, string SeedsPerSector, int MinSeedDepth, int MaxSeedDepth, int BaseNoise, int FilterPasses, int BorderWidth, int _CutoffDepth, List<NoiseMapNode> ExtraNodes, int FilterBorder, int MaximumSeeds = int.MaxValue)
	{
		if (MinSeedDepth > MaxSeedDepth)
		{
			MaxSeedDepth = MinSeedDepth;
		}
		CutoffDepth = _CutoffDepth;
		Width = _Width;
		Height = _Height;
		Noise = new int[Width, Height];
		Seeds = new int[Width, Height];
		Areas = new int[Width, Height];
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Noise[j, i] = 0;
				Seeds[j, i] = 0;
			}
		}
		for (int k = BorderWidth + FilterPasses; k < Width - BorderWidth - FilterPasses; k++)
		{
			for (int l = BorderWidth + FilterPasses; l < Height - BorderWidth - FilterPasses; l++)
			{
				Noise[k, l] = Stat.Random(0, BaseNoise);
				Seeds[k, l] = 0;
			}
		}
		List<Rect2D> list = new List<Rect2D>();
		for (int m = 0; m < SectorsHigh; m++)
		{
			for (int n = 0; n < SectorsWide; n++)
			{
				int x = n * (Width / SectorsWide);
				int x2 = (n + 1) * (Width / SectorsWide);
				int y = m * (Height / SectorsHigh);
				int y2 = (m + 1) * (Height / SectorsHigh);
				list.Add(new Rect2D
				{
					x1 = x,
					x2 = x2,
					y1 = y,
					y2 = y2
				});
			}
		}
		list.ShuffleInPlace();
		foreach (Rect2D item in list)
		{
			int num = item.x1;
			int num2 = item.x2;
			int num3 = item.y1;
			int num4 = item.y2;
			if (FilterBorder == -1)
			{
				if (num < BorderWidth + FilterPasses)
				{
					num = BorderWidth + FilterPasses;
				}
				if (num3 < BorderWidth + FilterPasses)
				{
					num3 = BorderWidth + FilterPasses;
				}
				if (num2 > Width - 1 - BorderWidth - FilterPasses)
				{
					num2 = Width - 1 - BorderWidth - FilterPasses;
				}
				if (num4 > Height - 1 - BorderWidth - FilterPasses)
				{
					num4 = Height - 1 - BorderWidth - FilterPasses;
				}
			}
			else
			{
				if (num < BorderWidth + FilterBorder)
				{
					num = BorderWidth + FilterBorder;
				}
				if (num3 < BorderWidth + FilterBorder)
				{
					num3 = BorderWidth + FilterBorder;
				}
				if (num2 > Width - 1 - BorderWidth - FilterBorder)
				{
					num2 = Width - 1 - BorderWidth - FilterBorder;
				}
				if (num4 > Height - 1 - BorderWidth - FilterBorder)
				{
					num4 = Height - 1 - BorderWidth - FilterBorder;
				}
			}
			int num5 = Stat.Roll(SeedsPerSector);
			for (int num6 = 0; num6 < num5; num6++)
			{
				int num7 = Stat.Random(num, num2);
				int num8 = Stat.Random(num3, num4);
				PlacedSeeds.Add(Location2D.Get(num7, num8));
				Noise[num7, num8] = Stat.Random(MinSeedDepth, MaxSeedDepth);
				Seeds[num7, num8]++;
				MaximumSeeds--;
				if (MaximumSeeds <= 0)
				{
					break;
				}
			}
			if (MaximumSeeds <= 0)
			{
				break;
			}
		}
		if (ExtraNodes != null)
		{
			foreach (NoiseMapNode ExtraNode in ExtraNodes)
			{
				if (ExtraNode.x > 0 && ExtraNode.y > 0 && ExtraNode.x < Width - 1 && ExtraNode.y < Height - 1)
				{
					if (ExtraNode.depth == -1)
					{
						Noise[ExtraNode.x, ExtraNode.y] = MaxSeedDepth;
					}
					else
					{
						Noise[ExtraNode.x, ExtraNode.y] = ExtraNode.depth;
					}
					Seeds[ExtraNode.x, ExtraNode.y]++;
				}
			}
		}
		int[,] array = new int[3, 3]
		{
			{ 1, 3, 1 },
			{ 3, 6, 3 },
			{ 1, 3, 1 }
		};
		for (int num9 = 0; num9 < FilterPasses; num9++)
		{
			int[,] array2 = new int[Width, Height];
			for (int num10 = 0; num10 < Height; num10++)
			{
				for (int num11 = 0; num11 < Width; num11++)
				{
					int num12 = 0;
					array2[num11, num10] = 0;
					for (int num13 = 0; num13 < 3; num13++)
					{
						for (int num14 = 0; num14 < 3; num14++)
						{
							if (num11 + (num14 - 1) >= 0 && num11 + (num14 - 1) < Width && num10 + (num13 - 1) >= 0 && num10 + (num13 - 1) < Height)
							{
								array2[num11, num10] += Noise[num11 + (num14 - 1), num10 + (num13 - 1)] * array[num14, num13];
								num12 += array[num14, num13];
							}
						}
					}
					array2[num11, num10] /= num12;
				}
			}
			Noise = array2;
		}
		AreaNodes = new Dictionary<int, List<NoiseMapNode>>();
		try
		{
			FillAreas();
		}
		catch (Exception x3)
		{
			MetricsManager.LogException("NoiseMap::FillAreas", x3);
		}
	}

	private void FillAreas()
	{
		nAreas = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Areas[j, i] = int.MinValue;
			}
		}
		for (int k = 0; k < Height; k++)
		{
			for (int l = 0; l < Width; l++)
			{
				if (Noise[l, k] > 0 && Areas[l, k] == int.MinValue)
				{
					FillArea(l, k, nAreas);
					nAreas++;
				}
			}
		}
		for (int m = 0; m < nAreas; m++)
		{
			AreaNodes[m] = new List<NoiseMapNode>();
		}
		for (int n = 0; n < Height; n++)
		{
			for (int num = 0; num < Width; num++)
			{
				if (Areas[num, n] != int.MinValue)
				{
					AreaNodes[Areas[num, n]].Add(new NoiseMapNode(num, n, Noise[num, n]));
				}
			}
		}
	}

	private void FillArea(int x, int y, int a)
	{
		if (x >= 0 && x < Width && y >= 0 && y < Height && Areas[x, y] <= int.MinValue && Noise[x, y] > CutoffDepth)
		{
			Areas[x, y] = a;
			FillArea(x - 1, y - 1, a);
			FillArea(x, y - 1, a);
			FillArea(x + 1, y - 1, a);
			FillArea(x - 1, y, a);
			FillArea(x + 1, y, a);
			FillArea(x - 1, y + 1, a);
			FillArea(x, y + 1, a);
			FillArea(x + 1, y + 1, a);
		}
	}

	public void Draw()
	{
		int i = 0;
		for (int num = Noise.GetUpperBound(1) + 1; i < num; i++)
		{
			int j = 0;
			for (int num2 = Noise.GetUpperBound(0) + 1; j < num2; j++)
			{
				if (Noise[j, i] > CutoffDepth)
				{
					Popup._ScreenBuffer.Goto(j, i);
					Popup._ScreenBuffer.Write(49);
				}
				else
				{
					Popup._ScreenBuffer.Goto(j, i);
					Popup._ScreenBuffer.Write(32);
				}
				if (Seeds[j, i] > 0)
				{
					Popup._ScreenBuffer.Goto(j, i);
					Popup._ScreenBuffer.Write("&W" + (char)(48 + Seeds[j, i]));
				}
			}
		}
		Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
		Keyboard.getch();
	}
}
