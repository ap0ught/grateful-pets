using System;

namespace Genkit.SimplexNoise;

public class LayeredNoise
{
	public float GenerateMin;

	public float GenerateMax = 1f;

	public bool GenerateNormalized = true;

	public float BaseSampleMultiple = 1f;

	public float OctaveStrength = 2f;

	private int Layers;

	private int[] LayerSeeds;

	private NoiseLayer[] LayerNoise;

	private static int LongRandom(Random rand)
	{
		byte[] array = new byte[4];
		rand.NextBytes(array);
		return BitConverter.ToInt32(array, 0);
	}

	public static LayeredNoise CreateLinearOctiveLayers(int _Layers, float _OctaveStrength, float _BaseStride, string Seed = null)
	{
		LayeredNoise layeredNoise = new LayeredNoise();
		layeredNoise.GenerateNormalized = true;
		layeredNoise.OctaveStrength = _OctaveStrength;
		layeredNoise.Layers = _Layers;
		Random rand = ((Seed == null) ? new Random() : new Random(Hash.String(Seed)));
		layeredNoise.LayerSeeds = new int[layeredNoise.Layers];
		layeredNoise.LayerNoise = new NoiseLayer[layeredNoise.Layers];
		for (int i = 0; i < layeredNoise.Layers; i++)
		{
			layeredNoise.LayerSeeds[i] = LongRandom(rand);
			layeredNoise.LayerNoise[i] = new NoiseLayer(layeredNoise.LayerSeeds[i]);
			layeredNoise.LayerNoise[i].SampleFunction = new LinearSampler(_BaseStride * (float)Math.Pow(_OctaveStrength, i));
		}
		return layeredNoise;
	}

	public static LayeredNoise CreateCrazyOctiveLayers(int _Layers, float _OctaveStrength, float _BaseStride, string Seed = null)
	{
		LayeredNoise layeredNoise = new LayeredNoise();
		layeredNoise.OctaveStrength = _OctaveStrength;
		layeredNoise.Layers = _Layers;
		Random rand = ((Seed == null) ? new Random() : new Random(Hash.String(Seed)));
		layeredNoise.LayerSeeds = new int[layeredNoise.Layers];
		layeredNoise.LayerNoise = new NoiseLayer[layeredNoise.Layers];
		for (int i = 0; i < layeredNoise.Layers; i++)
		{
			layeredNoise.LayerSeeds[i] = LongRandom(rand);
			layeredNoise.LayerNoise[i] = new NoiseLayer(layeredNoise.LayerSeeds[i]);
			layeredNoise.LayerNoise[i].SampleFunction = new CrazySampler(_BaseStride * (float)Math.Pow(_OctaveStrength, i));
		}
		return layeredNoise;
	}

	public float[] Generate1D(int Width)
	{
		float[] array = new float[Width];
		for (int i = 0; i < Width; i++)
		{
			array[i] = Generate(i);
		}
		if (GenerateNormalized)
		{
			float num = Mathx.Max(array, float.MinValue);
			float num2 = Mathx.Min(array, float.MaxValue);
			for (int j = 0; j < Width; j++)
			{
				array[j] = (array[j] - num2) / (num - num2);
			}
			for (int k = 0; k < Width; k++)
			{
				array[k] = array[k] * (GenerateMax - GenerateMin) + GenerateMin;
			}
		}
		return array;
	}

	public float[,] Generate2D(int Width, int Height)
	{
		float[,] array = new float[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				array[i, j] = Generate(i, j);
			}
		}
		if (GenerateNormalized)
		{
			float num = Mathx.Max(array, float.MinValue);
			float num2 = Mathx.Min(array, float.MaxValue);
			for (int k = 0; k < Width; k++)
			{
				for (int l = 0; l < Height; l++)
				{
					array[k, l] = (array[k, l] - num2) / (num - num2);
					array[k, l] = array[k, l] * (GenerateMax - GenerateMin) + GenerateMin;
				}
			}
		}
		return array;
	}

	public float[,] Generate3DSlice(int Width, int Height, int Depth)
	{
		float[,] array = new float[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				for (int k = 0; k < Depth; k++)
				{
					array[i, j] = Generate(i, j, k);
				}
			}
		}
		if (GenerateNormalized)
		{
			float num = Mathx.Max(array, float.MinValue);
			float num2 = Mathx.Min(array, float.MaxValue);
			for (int l = 0; l < Width; l++)
			{
				for (int m = 0; m < Height; m++)
				{
					array[l, m] = (array[l, m] - num2) / (num - num2);
					array[l, m] = array[l, m] * (GenerateMax - GenerateMin) + GenerateMin;
				}
			}
		}
		return array;
	}

	public float[,,] Generate3D(int Width, int Height, int Depth)
	{
		float[,,] array = new float[Width, Height, Depth];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				for (int k = 0; k < Depth; k++)
				{
					array[i, j, k] = Generate(i, j, k);
				}
			}
		}
		if (GenerateNormalized)
		{
			float num = Mathx.Max(array, float.MinValue);
			float num2 = Mathx.Min(array, float.MaxValue);
			for (int l = 0; l < Width; l++)
			{
				for (int m = 0; m < Height; m++)
				{
					for (int n = 0; n < Depth; n++)
					{
						array[l, m, n] = (array[l, m, n] - num2) / (num - num2);
						array[l, m, n] = array[l, m, n] * (GenerateMax - GenerateMin) + GenerateMin;
					}
				}
			}
		}
		return array;
	}

	public float Generate(float x)
	{
		float num = 0f;
		for (int i = 0; i < Layers; i++)
		{
			float x2 = x * BaseSampleMultiple;
			num += LayerNoise[i].Generate(x2);
		}
		return num / (float)Layers;
	}

	public float Generate(float x, float y)
	{
		float num = 0f;
		for (int i = 0; i < Layers; i++)
		{
			float x2 = x * BaseSampleMultiple;
			float y2 = y * BaseSampleMultiple;
			num += LayerNoise[i].Generate(x2, y2);
		}
		return num / (float)Layers;
	}

	public float Generate(float x, float y, float z)
	{
		float num = 0f;
		for (int i = 0; i < Layers; i++)
		{
			num += LayerNoise[i].Generate(x, y, z);
		}
		return num / (float)Layers;
	}
}
