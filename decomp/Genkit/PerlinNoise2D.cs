using System;
using System.Collections.Generic;

namespace Genkit;

public class PerlinNoise2D
{
	private double[,] Noise;

	private float Amplitude = 1f;

	private int Frequency = 1;

	public PerlinNoise2D(int freq, float _amp, Random rand)
	{
		Noise = new double[freq, freq];
		Amplitude = _amp;
		Frequency = freq;
		for (int i = 0; i < freq; i++)
		{
			for (int j = 0; j < freq; j++)
			{
				Noise[i, j] = rand.NextDouble();
			}
		}
	}

	public static double getMax(double[,] Array)
	{
		double num = double.MinValue;
		foreach (double num2 in Array)
		{
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public static double getMax(List<double> DoubleCollection)
	{
		double num = double.MinValue;
		foreach (double item in DoubleCollection)
		{
			if (item > num)
			{
				num = item;
			}
		}
		return num;
	}

	public static double[,] sumNoiseFunctions(int width, int height, int sx, int sy, List<PerlinNoise2D> noiseFunctions)
	{
		return sumNoiseFunctions(width, height, sx, sy, noiseFunctions, 1f);
	}

	public static double[,] sumNoiseFunctions(int width, int height, int sx, int sy, List<PerlinNoise2D> noiseFunctions, float xmul)
	{
		double[,] array = new double[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				array[i, j] = 0.0;
			}
		}
		for (int k = 0; k < noiseFunctions.Count; k++)
		{
			double num = (float)width * xmul / (float)noiseFunctions[k].Frequency;
			double num2 = (float)height / (float)noiseFunctions[k].Frequency;
			for (int l = 0; l < width; l++)
			{
				for (int m = 0; m < height; m++)
				{
					int num3 = (int)((double)l / num);
					int num4 = num3 + 1;
					int num5 = (int)((double)m / num2);
					int num6 = num5 + 1;
					double interpolatedPoint = noiseFunctions[k].getInterpolatedPoint(num3 + sx, num4 + sx, num5 + sy, num6 + sy, (double)l / num - (double)num3, (double)m / num2 - (double)num5);
					array[l, m] += interpolatedPoint * (double)noiseFunctions[k].Amplitude;
				}
			}
		}
		double max = getMax(array);
		for (int n = 0; n < width; n++)
		{
			for (int num7 = 0; num7 < height; num7++)
			{
				array[n, num7] /= max;
			}
		}
		return array;
	}

	public double getInterpolatedPoint(int _xa, int _xb, int _ya, int _yb, double x, double y)
	{
		double a = interpolate(Noise[_xa % Frequency, _ya % Frequency], Noise[_xb % Frequency, _ya % Frequency], x);
		double b = interpolate(Noise[_xa % Frequency, _yb % Frequency], Noise[_xb % Frequency, _yb % Frequency], x);
		return interpolate(a, b, y);
	}

	private double interpolate(double a, double b, double x)
	{
		double d = x * Math.PI;
		double num = (1.0 - Math.Cos(d)) * 0.5;
		return a * (1.0 - num) + b * num;
	}

	public static double[,] Smooth(double[,] Field, int Width, int Height, int FilterPasses)
	{
		int[,] array = new int[3, 3]
		{
			{ 2, 4, 2 },
			{ 4, 8, 4 },
			{ 2, 4, 2 }
		};
		for (int i = 0; i < FilterPasses; i++)
		{
			double[,] array2 = new double[Width, Height];
			for (int j = 0; j < Width; j++)
			{
				for (int k = 0; k < Height; k++)
				{
					int num = 0;
					array2[j, k] = 0.0;
					for (int l = 0; l < 3; l++)
					{
						for (int m = 0; m < 3; m++)
						{
							if (j + (l - 1) >= 0 && j + (l - 1) < Width && k + (m - 1) >= 0 && k + (m - 1) < Height)
							{
								array2[j, k] += Field[j + (l - 1), k + (m - 1)] * (double)array[l, m];
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
