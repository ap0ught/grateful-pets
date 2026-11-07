using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRL.Rules;

namespace Genkit;

public abstract class WaveCollapseFastModelBase
{
	public static readonly int INIT_SIZE = 350;

	protected static BitArray[] wave;

	protected static List<ushort>[][] propagator;

	private static ushort[] compatible;

	private static Tuple<int, ushort>[] stack;

	private int stacksize;

	protected System.Random random = Stat.Rnd5;

	public int FMX;

	public int FMY;

	public int T;

	private int MaxT;

	protected bool periodic;

	protected double[] weights;

	private static double[] weightLogWeights;

	protected double sumOfWeights;

	protected double sumOfWeightLogWeights;

	protected double startingEntropy;

	protected static ushort[] observed;

	private static ushort[] sumsOfOnes;

	protected static double[] sumsOfWeights;

	protected static double[] sumsOfWeightLogWeights;

	protected static double[] entropies;

	private static List<double> distribution = new List<double>(256);

	private static Stack<Tuple<int, ushort>> tuplePool = new Stack<Tuple<int, ushort>>();

	protected static int[] DX = new int[4] { -1, 0, 1, 0 };

	protected static int[] DY = new int[4] { 0, 1, 0, -1 };

	private static int[] opposite = new int[4] { 2, 3, 0, 1 };

	protected WaveCollapseFastModelBase(int width, int height)
	{
		FMX = width;
		FMY = height;
	}

	public abstract Color32[] GetResult();

	protected void Init()
	{
		Debug.Log("Init T=" + T);
		MaxT = Math.Max(T, INIT_SIZE);
		if (wave == null)
		{
			wave = new BitArray[18000];
		}
		if (compatible == null || compatible.Length < wave.Length * MaxT * 4)
		{
			compatible = new ushort[wave.Length * MaxT * 4];
		}
		for (int i = 0; i < wave.Length; i++)
		{
			if (wave[i] == null || wave[i].Length < T)
			{
				wave[i] = new BitArray(MaxT);
			}
		}
		if (weightLogWeights == null || weightLogWeights.Length < T)
		{
			Debug.Log("Reinitializing weightLogWeights to T=" + MaxT);
			weightLogWeights = new double[MaxT];
		}
		sumOfWeights = 0.0;
		sumOfWeightLogWeights = 0.0;
		for (int j = 0; j < T; j++)
		{
			weightLogWeights[j] = weights[j] * Math.Log(weights[j]);
			sumOfWeights += weights[j];
			sumOfWeightLogWeights += weightLogWeights[j];
		}
		startingEntropy = Math.Log(sumOfWeights) - sumOfWeightLogWeights / sumOfWeights;
		if (sumsOfOnes == null)
		{
			sumsOfOnes = new ushort[wave.Length];
		}
		if (sumsOfWeights == null)
		{
			sumsOfWeights = new double[wave.Length];
		}
		if (sumsOfWeightLogWeights == null)
		{
			sumsOfWeightLogWeights = new double[wave.Length];
		}
		if (entropies == null)
		{
			entropies = new double[wave.Length];
		}
		if (observed == null)
		{
			observed = new ushort[wave.Length];
		}
		if (stack == null || stack.Length < wave.Length * T)
		{
			Debug.Log("Reinitializing stack to T=" + MaxT);
			stack = new Tuple<int, ushort>[wave.Length * MaxT];
		}
		stacksize = 0;
	}

	private bool? Observe()
	{
		double num = 1000.0;
		int num2 = -1;
		for (int i = 0; i < wave.Length; i++)
		{
			if (OnBoundary(i % FMX, i / FMX))
			{
				continue;
			}
			int num3 = sumsOfOnes[i];
			if (num3 == 0)
			{
				return false;
			}
			double num4 = entropies[i];
			if (num3 > 1 && num4 <= num)
			{
				double num5 = 1E-06 * random.NextDouble();
				if (num4 + num5 < num)
				{
					num = num4 + num5;
					num2 = i;
				}
			}
		}
		if (num2 == -1)
		{
			for (int j = 0; j < wave.Length; j++)
			{
				for (ushort num6 = 0; num6 < T; num6++)
				{
					if (wave[j][num6])
					{
						observed[j] = num6;
						break;
					}
				}
			}
			return true;
		}
		distribution.Clear();
		for (int k = 0; k < T; k++)
		{
			distribution.Add(wave[num2][k] ? weights[k] : 0.0);
		}
		int num7 = distribution.Random(random.NextDouble());
		BitArray bitArray = wave[num2];
		for (ushort num8 = 0; num8 < T; num8++)
		{
			if (bitArray[num8] != (num8 == num7))
			{
				Ban(num2, num8);
			}
		}
		return null;
	}

	protected void Propagate()
	{
		while (stacksize > 0)
		{
			Tuple<int, ushort> tuple = stack[stacksize - 1];
			stack[stacksize - 1] = null;
			stacksize--;
			int item = tuple.Item1;
			int num = item % FMX;
			int num2 = item / FMX;
			for (int i = 0; i < 4; i++)
			{
				int num3 = DX[i];
				int num4 = DY[i];
				int num5 = num + num3;
				int num6 = num2 + num4;
				if (OnBoundary(num5, num6))
				{
					continue;
				}
				if (num5 < 0)
				{
					num5 += FMX;
				}
				else if (num5 >= FMX)
				{
					num5 -= FMX;
				}
				if (num6 < 0)
				{
					num6 += FMY;
				}
				else if (num6 >= FMY)
				{
					num6 -= FMY;
				}
				int num7 = num5 + num6 * FMX;
				List<ushort> list = propagator[i][tuple.Item2];
				int num8 = num7 * MaxT * 4;
				for (int j = 0; j < list.Count; j++)
				{
					ushort num9 = list[j];
					compatible[num8 + num9 * 4 + i]--;
					if (compatible[num8 + num9 * 4 + i] == 0)
					{
						Ban(num7, num9);
					}
				}
			}
			tuplePool.Push(tuple);
		}
	}

	public virtual bool Run(int seed, int limit)
	{
		Init();
		Clear();
		random = new System.Random(seed);
		for (int i = 0; i < limit || limit == 0; i++)
		{
			bool? flag = Observe();
			if (flag.HasValue)
			{
				return flag.Value;
			}
			Propagate();
		}
		return true;
	}

	protected void Ban(int i, ushort t)
	{
		wave[i][t] = false;
		int num = i * MaxT * 4 + t * 4;
		for (int j = 0; j < 4; j++)
		{
			compatible[num + j] = 0;
		}
		if (tuplePool.Count > 0)
		{
			stack[stacksize] = tuplePool.Pop();
			stack[stacksize] = new Tuple<int, ushort>(i, t);
		}
		else
		{
			stack[stacksize] = new Tuple<int, ushort>(i, t);
		}
		stacksize++;
		double num2 = sumsOfWeights[i];
		entropies[i] += sumsOfWeightLogWeights[i] / num2 - Math.Log(num2);
		sumsOfOnes[i]--;
		sumsOfWeights[i] -= weights[t];
		sumsOfWeightLogWeights[i] -= weightLogWeights[t];
		num2 = sumsOfWeights[i];
		entropies[i] -= sumsOfWeightLogWeights[i] / num2 - Math.Log(num2);
	}

	protected virtual void Clear()
	{
		for (int i = 0; i < wave.Length; i++)
		{
			for (int j = 0; j < T; j++)
			{
				wave[i][j] = true;
				for (int k = 0; k < 4; k++)
				{
					compatible[i * MaxT * 4 + j * 4 + k] = (ushort)propagator[opposite[k]][j].Count;
				}
			}
			sumsOfOnes[i] = (ushort)weights.Length;
			sumsOfWeights[i] = sumOfWeights;
			sumsOfWeightLogWeights[i] = sumOfWeightLogWeights;
			entropies[i] = startingEntropy;
			observed[i] = 0;
		}
	}

	protected abstract bool OnBoundary(int x, int y);
}
