using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genkit;

public class WaveCollapseFastModel : WaveCollapseFastModelBase
{
	private int N;

	private static byte[][] patterns;

	private List<Color32> colors;

	private ushort ground;

	public Color32[] results;

	public bool[] mask;

	public void UpdateSample(string templateName, int N, bool periodicInput, bool periodicOutput, int symmetry, int ground)
	{
		if (!WaveCollapseTools.waveTemplates.ContainsKey(templateName.ToLower()))
		{
			Debug.LogError("Unknown wave template: " + templateName.ToLower());
			return;
		}
		WaveTemplateEntry entry = WaveCollapseTools.waveTemplates[templateName];
		UpdateSample(entry, N, periodicInput, periodicOutput, symmetry, ground);
	}

	public void UpdateSample(WaveTemplateEntry entry, int N, bool periodicInput, bool periodicOutput, int symmetry, int ground)
	{
		this.N = N;
		periodic = periodicOutput;
		int SMX = entry.width;
		int SMY = entry.height;
		byte[,] sample = new byte[SMX, SMY];
		colors = new List<Color32>();
		for (int i = 0; i < SMY; i++)
		{
			for (int j = 0; j < SMX; j++)
			{
				Color color = entry.pixels[j + (SMY - 1 - i) * SMX];
				int num = 0;
				using (List<Color32>.Enumerator enumerator = colors.GetEnumerator())
				{
					while (enumerator.MoveNext() && !(enumerator.Current == color))
					{
						num++;
					}
				}
				if (num == colors.Count)
				{
					colors.Add(color);
				}
				sample[j, i] = (byte)num;
			}
		}
		int C = colors.Count;
		long W = WaveCollapseTools.Power(C, N * N);
		Func<Func<int, int, byte>, byte[]> pattern = (Func<int, int, byte> f) =>
		{
			byte[] array2 = new byte[N * N];
			for (int k = 0; k < N; k++)
			{
				for (int l = 0; l < N; l++)
				{
					array2[l + k * N] = f(l, k);
				}
			}
			return array2;
		};
		Func<int, int, byte[]> func = (int x, int y) => pattern((int dx, int dy) => sample[(x + dx) % SMX, (y + dy) % SMY]);
		Func<byte[], byte[]> func2 = (byte[] p) => pattern((int x, int y) => p[N - 1 - y + x * N]);
		Func<byte[], byte[]> func3 = (byte[] p) => pattern((int x, int y) => p[N - 1 - x + y * N]);
		Func<byte[], long> func4 = (byte[] p) =>
		{
			long num11 = 0L;
			long num12 = 1L;
			for (int k = 0; k < p.Length; k++)
			{
				num11 += p[p.Length - 1 - k] * num12;
				num12 *= C;
			}
			return num11;
		};
		Func<long, byte[]> func5 = (long ind) =>
		{
			long num11 = ind;
			long num12 = W;
			byte[] array2 = new byte[N * N];
			for (int k = 0; k < array2.Length; k++)
			{
				num12 /= C;
				int num13 = 0;
				while (num11 >= num12)
				{
					num11 -= num12;
					num13++;
				}
				array2[k] = (byte)num13;
			}
			return array2;
		};
		Dictionary<long, int> dictionary = new Dictionary<long, int>();
		List<long> list = new List<long>();
		for (int num2 = 0; num2 < (periodicInput ? SMY : (SMY - N + 1)); num2++)
		{
			for (int num3 = 0; num3 < (periodicInput ? SMX : (SMX - N + 1)); num3++)
			{
				byte[][] array = new byte[8][];
				array[0] = func(num3, num2);
				array[1] = func3(array[0]);
				array[2] = func2(array[0]);
				array[3] = func3(array[2]);
				array[4] = func2(array[2]);
				array[5] = func3(array[4]);
				array[6] = func2(array[4]);
				array[7] = func3(array[6]);
				for (int num4 = 0; num4 < symmetry; num4++)
				{
					long num5 = func4(array[num4]);
					if (dictionary.ContainsKey(num5))
					{
						dictionary[num5]++;
						continue;
					}
					dictionary.Add(num5, 1);
					list.Add(num5);
				}
			}
		}
		T = dictionary.Count;
		Debug.Log("*** T= " + T + " using sample: " + entry.ToString() + " pi:" + periodicInput + " po:" + periodicOutput + " s:" + symmetry + " g:" + ground);
		this.ground = (ushort)((ground + T) % T);
		if (patterns == null || patterns.Length < T)
		{
			patterns = new byte[T][];
		}
		weights = new double[T];
		int num6 = 0;
		foreach (long item in list)
		{
			patterns[num6] = func5(item);
			weights[num6] = dictionary[item];
			num6++;
		}
		List<ushort> list2 = new List<ushort>();
		if (WaveCollapseFastModelBase.propagator == null)
		{
			WaveCollapseFastModelBase.propagator = new List<ushort>[4][];
		}
		int num7 = Math.Max(WaveCollapseFastModelBase.INIT_SIZE, T);
		for (int num8 = 0; num8 < 4; num8++)
		{
			if (WaveCollapseFastModelBase.propagator[num8] == null || WaveCollapseFastModelBase.propagator[num8].Length < T)
			{
				WaveCollapseFastModelBase.propagator[num8] = new List<ushort>[num7];
			}
			for (int num9 = 0; num9 < T; num9++)
			{
				list2.Clear();
				for (ushort num10 = 0; num10 < T; num10++)
				{
					if (agrees(patterns[num9], patterns[num10], WaveCollapseFastModelBase.DX[num8], WaveCollapseFastModelBase.DY[num8]))
					{
						list2.Add(num10);
					}
				}
				if (WaveCollapseFastModelBase.propagator[num8][num9] == null)
				{
					WaveCollapseFastModelBase.propagator[num8][num9] = new List<ushort>(12);
				}
				WaveCollapseFastModelBase.propagator[num8][num9].Clear();
				WaveCollapseFastModelBase.propagator[num8][num9].AddRange(list2);
			}
		}
	}

	public WaveCollapseFastModel(WaveTemplateEntry entry, int N, int width, int height, bool periodicInput, bool periodicOutput, int symmetry, int ground)
		: base(width, height)
	{
		UpdateSample(entry, N, periodicInput, periodicOutput, symmetry, ground);
	}

	public WaveCollapseFastModel(string templateName, int N, int width, int height, bool periodicInput, bool periodicOutput, int symmetry, int ground)
		: base(width, height)
	{
		UpdateSample(templateName, N, periodicInput, periodicOutput, symmetry, ground);
	}

	private bool agrees(byte[] p1, byte[] p2, int dx, int dy)
	{
		int num = ((dx >= 0) ? dx : 0);
		int num2 = ((dx < 0) ? (dx + N) : N);
		int num3 = ((dy >= 0) ? dy : 0);
		int num4 = ((dy < 0) ? (dy + N) : N);
		for (int i = num3; i < num4; i++)
		{
			for (int j = num; j < num2; j++)
			{
				if (p1[j + N * i] != p2[j - dx + N * (i - dy)])
				{
					return false;
				}
			}
		}
		return true;
	}

	public override Color32[] GetResult()
	{
		return results;
	}

	public void ClearColors(string clearColors, string clearstyle = "border1")
	{
		if (WaveCollapseFastModelBase.wave == null)
		{
			return;
		}
		if (mask == null)
		{
			mask = new bool[FMX * FMY];
		}
		Color32[] result = GetResult();
		List<Color32> list = new List<Color32>();
		for (int i = 0; i < clearColors.Length; i++)
		{
			if (clearColors[i] == 'W')
			{
				list.Add(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			}
			if (clearColors[i] == 'w')
			{
				list.Add(new Color32(128, 128, 128, byte.MaxValue));
			}
			if (clearColors[i] == 'R')
			{
				list.Add(new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
			}
			if (clearColors[i] == 'K')
			{
				list.Add(new Color32(0, 0, 0, byte.MaxValue));
			}
			if (clearColors[i] == 'B')
			{
				list.Add(new Color32(0, 0, byte.MaxValue, byte.MaxValue));
			}
			if (clearColors[i] == 'G')
			{
				list.Add(new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			}
			if (clearColors[i] == 'M')
			{
				list.Add(new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue));
			}
			if (clearColors[i] == 'Y')
			{
				list.Add(new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
			}
		}
		for (int j = 0; j < FMY; j++)
		{
			for (int k = 0; k < FMX; k++)
			{
				Color32 color = result[k + j * FMX];
				mask[k + j * FMX] = true;
				if (clearstyle == "border1")
				{
					if (list.Contains(color))
					{
						if (k != FMX - 1)
						{
							_ = k + 1 + j * FMX;
							_ = result.Length;
						}
						if ((k == 0 || WaveCollapseTools.equals(result[k - 1 + j * FMX], color)) && (k == FMX - 1 || WaveCollapseTools.equals(result[k + 1 + j * FMX], color)) && (j == 0 || WaveCollapseTools.equals(result[k + (j - 1) * FMX], color)) && (j == FMY - 1 || WaveCollapseTools.equals(result[k + (j + 1) * FMX], color)))
						{
							mask[k + j * FMX] = false;
						}
					}
				}
				else if (clearstyle == "border1.5")
				{
					if (list.Contains(color) && (k == 0 || WaveCollapseTools.equals(result[k - 1 + j * FMX], color)) && (k == FMX - 1 || WaveCollapseTools.equals(result[k + 1 + j * FMX], color)) && (j == 0 || WaveCollapseTools.equals(result[k + (j - 1) * FMX], color)) && (j == FMY - 1 || WaveCollapseTools.equals(result[k + (j + 1) * FMX], color)) && (k <= 0 || j <= 0 || WaveCollapseTools.equals(result[k - 1 + (j - 1) * FMX], color)) && (k <= 0 || j >= FMY - 1 || WaveCollapseTools.equals(result[k - 1 + (j + 1) * FMX], color)) && (k >= FMX - 1 || j <= 0 || WaveCollapseTools.equals(result[k + 1 + (j - 1) * FMX], color)) && (k >= FMX - 1 || j >= FMY - 1 || WaveCollapseTools.equals(result[k + 1 + (j + 1) * FMX], color)))
					{
						mask[k + j * FMX] = false;
					}
				}
				else if (list.Contains(color))
				{
					mask[k + j * FMX] = false;
				}
			}
		}
	}

	public override bool Run(int seed, int limit)
	{
		bool result = base.Run(seed, limit);
		GenResult();
		return result;
	}

	private void GenResult()
	{
		if (results == null)
		{
			results = new Color32[FMX * FMY];
		}
		if (WaveCollapseFastModelBase.observed != null)
		{
			for (int i = 0; i < FMY; i++)
			{
				int num = ((i >= FMY - N + 1) ? (N - 1) : 0);
				for (int j = 0; j < FMX; j++)
				{
					int num2 = ((j >= FMX - N + 1) ? (N - 1) : 0);
					int num3 = j - num2 + (i - num) * FMX;
					int num4 = num2 + num * N;
					Color32 color = new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue);
					try
					{
						color = colors[patterns[WaveCollapseFastModelBase.observed[num3]][num4]];
					}
					catch
					{
					}
					if (mask == null || !mask[j + i * FMX])
					{
						results[j + i * FMX] = new Color32(color.r, color.g, color.b, byte.MaxValue);
					}
					else
					{
						results[j + i * FMX] = results[j + i * FMX];
					}
				}
			}
			return;
		}
		for (int k = 0; k < WaveCollapseFastModelBase.wave.Length; k++)
		{
			int num5 = 0;
			float num6 = 0f;
			float num7 = 0f;
			float num8 = 0f;
			int num9 = k % FMX;
			int num10 = k / FMX;
			for (int l = 0; l < N; l++)
			{
				for (int m = 0; m < N; m++)
				{
					int num11 = num9 - m;
					if (num11 < 0)
					{
						num11 += FMX;
					}
					int num12 = num10 - l;
					if (num12 < 0)
					{
						num12 += FMY;
					}
					int num13 = num11 + num12 * FMX;
					if (OnBoundary(num11, num12))
					{
						continue;
					}
					for (int n = 0; n < T; n++)
					{
						if (WaveCollapseFastModelBase.wave[num13][n])
						{
							num5++;
							Color color2 = colors[patterns[n][m + l * N]];
							num6 += color2.r;
							num7 += color2.g;
							num8 += color2.b;
						}
					}
				}
			}
			if (mask == null || !mask[num9 + num10 * FMX])
			{
				results[num9 + num10 * FMX] = new Color32((byte)(num6 / (float)num5 * 255f), (byte)(num7 / (float)num5 * 255f), (byte)(num8 / (float)num5 * 255f), byte.MaxValue);
			}
			else
			{
				results[num9 + num10 * FMX] = results[num9 + num10 * FMX];
			}
		}
	}

	protected override bool OnBoundary(int x, int y)
	{
		if (!periodic)
		{
			if (x + N <= FMX && y + N <= FMY && x >= 0)
			{
				return y < 0;
			}
			return true;
		}
		return false;
	}

	protected override void Clear()
	{
		base.Clear();
		if (ground == 0)
		{
			return;
		}
		for (int i = 0; i < FMX; i++)
		{
			for (ushort num = 0; num < T; num++)
			{
				if (num != ground)
				{
					Ban(i + (FMY - 1) * FMX, num);
				}
			}
			for (int j = 0; j < FMY - 1; j++)
			{
				Ban(i + j * FMX, ground);
			}
		}
		Propagate();
	}
}
