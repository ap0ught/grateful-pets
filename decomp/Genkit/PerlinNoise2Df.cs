using System;
using System.Collections.Generic;

namespace Genkit;

public class PerlinNoise2Df
{
	private float[,] Noise;

	private float Amplitude = 1f;

	private int Frequency = 1;

	public PerlinNoise2Df(int freq, float _amp, Random rand)
	{
		Noise = new float[freq, freq];
		Amplitude = _amp;
		Frequency = freq;
		for (int i = 0; i < freq; i++)
		{
			for (int j = 0; j < freq; j++)
			{
				Noise[i, j] = (float)rand.NextDouble();
			}
		}
	}

	public static float getMax(float[,] Array)
	{
		float num = float.MinValue;
		foreach (float num2 in Array)
		{
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public static float getMax(List<float> DoubleCollection)
	{
		float num = float.MinValue;
		foreach (float item in DoubleCollection)
		{
			if (item > num)
			{
				num = item;
			}
		}
		return num;
	}

	public static float[,] sumNoiseFunctions(int width, int height, int sx, int sy, List<PerlinNoise2Df> noiseFunctions)
	{
		return sumNoiseFunctions(width, height, sx, sy, noiseFunctions, 1f);
	}

	public static float[,] sumNoiseFunctions(int width, int height, int sx, int sy, List<PerlinNoise2Df> noiseFunctions, float xmul)
	{
		float[,] array = new float[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				array[i, j] = 0f;
			}
		}
		for (int k = 0; k < noiseFunctions.Count; k++)
		{
			float num = (float)width * xmul / (float)noiseFunctions[k].Frequency;
			float num2 = (float)height / (float)noiseFunctions[k].Frequency;
			for (int l = 0; l < width; l++)
			{
				for (int m = 0; m < height; m++)
				{
					int num3 = (int)((float)l / num);
					int num4 = num3 + 1;
					int num5 = (int)((float)m / num2);
					int num6 = num5 + 1;
					float interpolatedPoint = noiseFunctions[k].getInterpolatedPoint(num3 + sx, num4 + sx, num5 + sy, num6 + sy, (float)l / num - (float)num3, (float)m / num2 - (float)num5);
					array[l, m] += interpolatedPoint * noiseFunctions[k].Amplitude;
				}
			}
		}
		float max = getMax(array);
		for (int n = 0; n < width; n++)
		{
			for (int num7 = 0; num7 < height; num7++)
			{
				array[n, num7] /= max;
			}
		}
		return array;
	}

	public float getInterpolatedPoint(int _xa, int _xb, int _ya, int _yb, float x, float y)
	{
		float a = interpolate(Noise[_xa % Frequency, _ya % Frequency], Noise[_xb % Frequency, _ya % Frequency], x);
		float b = interpolate(Noise[_xa % Frequency, _yb % Frequency], Noise[_xb % Frequency, _yb % Frequency], x);
		return interpolate(a, b, y);
	}

	private float interpolate(float a, float b, float x)
	{
		float num = x * MathF.PI;
		float num2 = (1f - (float)Math.Cos(num)) * 0.5f;
		return a * (1f - num2) + b * num2;
	}

	public static float[,] Smooth(float[,] Field, int Width, int Height, int FilterPasses)
	{
		int[,] array = new int[3, 3]
		{
			{ 2, 4, 2 },
			{ 4, 8, 4 },
			{ 2, 4, 2 }
		};
		for (int i = 0; i < FilterPasses; i++)
		{
			float[,] array2 = new float[Width, Height];
			for (int j = 0; j < Width; j++)
			{
				for (int k = 0; k < Height; k++)
				{
					int num = 0;
					array2[j, k] = 0f;
					for (int l = 0; l < 3; l++)
					{
						for (int m = 0; m < 3; m++)
						{
							if (j + (l - 1) >= 0 && j + (l - 1) < Width && k + (m - 1) >= 0 && k + (m - 1) < Height)
							{
								array2[j, k] += Field[j + (l - 1), k + (m - 1)] * (float)array[l, m];
								num += array[l, m];
							}
						}
					}
					array2[j, k] /= num;
				}
			}
			Field = array2;
		}
		return Field;
	}
}
