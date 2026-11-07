using System;
using UnityEngine;
using XRL.Rules;

namespace Genkit;

public abstract class WaveCollapseModelBase
{
	protected bool[][][] wave;

	protected bool[][] changes;

	protected double[] stationary;

	protected int[][] observed;

	public int FMX;

	public int FMY;

	public int T;

	public int limit;

	protected bool periodic;

	private double[] logProb;

	private double logT;

	public bool bClear = true;

	private System.Random random = Stat.Rnd5;

	protected WaveCollapseModelBase(int width, int height)
	{
		FMX = width;
		FMY = height;
		wave = new bool[FMX][][];
		changes = new bool[FMX][];
		for (int i = 0; i < FMX; i++)
		{
			wave[i] = new bool[FMY][];
			changes[i] = new bool[FMY];
		}
	}

	protected abstract bool Propagate();

	private bool Observe()
	{
		double num = 1000.0;
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < FMX; i++)
		{
			for (int j = 0; j < FMY; j++)
			{
				if (OnBoundary(i, j))
				{
					continue;
				}
				bool[] array = wave[i][j];
				int num4 = 0;
				double num5 = 0.0;
				for (int k = 0; k < T; k++)
				{
					if (array[k])
					{
						num4++;
						num5 += stationary[k];
					}
				}
				if (num5 == 0.0)
				{
					continue;
				}
				double num6 = 1E-06 * random.NextDouble();
				double num7;
				if (num4 == 1)
				{
					num7 = 0.0;
				}
				else if (num4 == T)
				{
					num7 = logT;
				}
				else
				{
					double num8 = 0.0;
					double num9 = Math.Log(num5);
					for (int l = 0; l < T; l++)
					{
						if (array[l])
						{
							num8 += stationary[l] * logProb[l];
						}
					}
					num7 = num9 - num8 / num5;
				}
				if (num7 > 0.0 && num7 + num6 < num)
				{
					num = num7 + num6;
					num2 = i;
					num3 = j;
				}
			}
		}
		if (num2 == -1 && num3 == -1)
		{
			bool flag = false;
			if (observed == null)
			{
				flag = true;
			}
			if (flag)
			{
				observed = new int[FMX][];
			}
			for (int m = 0; m < FMX; m++)
			{
				if (flag)
				{
					observed[m] = new int[FMY];
				}
				for (int n = 0; n < FMY; n++)
				{
					for (int num10 = 0; num10 < T; num10++)
					{
						if (wave[m][n][num10])
						{
							observed[m][n] = num10;
							break;
						}
					}
				}
			}
			return true;
		}
		double[] array2 = new double[T];
		for (int num11 = 0; num11 < T; num11++)
		{
			array2[num11] = (wave[num2][num3][num11] ? stationary[num11] : 0.0);
		}
		int num12 = array2.Random(random.NextDouble());
		for (int num13 = 0; num13 < T; num13++)
		{
			wave[num2][num3][num13] = num13 == num12;
		}
		changes[num2][num3] = true;
		return false;
	}

	public bool Run(int seed, int limit)
	{
		logT = Math.Log(T);
		logProb = new double[T];
		for (int i = 0; i < T; i++)
		{
			logProb[i] = Math.Log(stationary[i]);
		}
		Clear();
		random = new System.Random(seed);
		for (int j = 0; j < limit || limit == 0; j++)
		{
			if (Observe())
			{
				return true;
			}
			while (Propagate())
			{
			}
		}
		return true;
	}

	protected virtual void Clear()
	{
		for (int i = 0; i < FMX; i++)
		{
			for (int j = 0; j < FMY; j++)
			{
				for (int k = 0; k < T; k++)
				{
					if (bClear)
					{
						wave[i][j][k] = true;
					}
				}
				changes[i][j] = false;
			}
		}
	}

	protected abstract bool OnBoundary(int x, int y);

	public abstract Color32[] GetResult();
}
