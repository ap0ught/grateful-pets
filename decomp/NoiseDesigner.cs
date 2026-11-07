using System;
using System.Diagnostics;
using Genkit.SimplexNoise;
using UnityEngine;
using UnityEngine.UI;

public class NoiseDesigner : MonoBehaviour
{
	private int width = 240;

	private int height = 75;

	private int depth = 30;

	private string LastRender = "Image";

	private float[,,] NoiseData;

	public int Layers
	{
		get
		{
			try
			{
				return Convert.ToInt32(GameObject.Find("LayersText").GetComponent<Text>().text);
			}
			catch (Exception)
			{
				return 4;
			}
		}
	}

	public float OctaveStrength
	{
		get
		{
			try
			{
				return Convert.ToSingle(GameObject.Find("OctaveText").GetComponent<Text>().text);
			}
			catch (Exception)
			{
				return 4f;
			}
		}
	}

	public float BaseStride
	{
		get
		{
			try
			{
				return Convert.ToSingle(GameObject.Find("BaseStrideText").GetComponent<Text>().text);
			}
			catch (Exception)
			{
				return 0.001f;
			}
		}
	}

	public float Cutoff
	{
		get
		{
			try
			{
				return Convert.ToSingle(GameObject.Find("CutoffText").GetComponent<Text>().text);
			}
			catch (Exception)
			{
				return 0f;
			}
		}
	}

	public int Bands
	{
		get
		{
			try
			{
				return Convert.ToInt32(GameObject.Find("BandsText").GetComponent<Text>().text);
			}
			catch (Exception)
			{
				return 3;
			}
		}
	}

	public int ZLevel
	{
		get
		{
			try
			{
				int num = (int)(GameObject.Find("LayerScroll").GetComponent<Scrollbar>().value * (float)depth);
				if (num < 0)
				{
					num = 0;
				}
				if (num > depth)
				{
					num = depth;
				}
				return num;
			}
			catch (Exception)
			{
				return 0;
			}
		}
	}

	public void UpdateSlide()
	{
		if (LastRender == "Image")
		{
			UpdateImage();
		}
		if (LastRender == "Bands")
		{
			DrawBands();
		}
	}

	public void UpdateImage()
	{
		LastRender = "Image";
		Color[] array = new Color[width * height];
		int zLevel = ZLevel;
		float num = 0f;
		float cutoff = Cutoff;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				float num2 = NoiseData[i, j, zLevel];
				num2 = ((!(num2 < cutoff)) ? ((num2 - cutoff) * (1f / (1f - cutoff))) : 0f);
				array[i + j * width] = new Color(num2 * 0.6f, num2 * 0.6f, num2 * 1f);
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		RawImage component = GameObject.Find("NoiseMap").GetComponent<RawImage>();
		if (component.texture == null)
		{
			component.texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
			component.texture.filterMode = FilterMode.Point;
		}
		((Texture2D)component.texture).SetPixels(array);
		((Texture2D)component.texture).Apply();
	}

	public void DrawBands()
	{
		LastRender = "Bands";
		Color[] array = new Color[width * height];
		float num = 0f;
		float num2 = 1f;
		int zLevel = ZLevel;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				float num3 = NoiseData[i, j, zLevel];
				num3 = ((!(num3 < Cutoff)) ? ((num3 - Cutoff) * (1f / (1f - Cutoff))) : 0f);
				array[i + j * width] = new Color(num3 * 0.6f, num3 * 0.6f, num3 * 1f);
				if (num3 > num)
				{
					num = num3;
				}
				if (num3 < num2)
				{
					num2 = num3;
				}
			}
		}
		UnityEngine.Debug.Log("Cel=" + num);
		UnityEngine.Debug.Log("Flo=" + num2);
		int bands = Bands;
		for (int k = 0; k < width; k++)
		{
			for (int l = 0; l < height; l++)
			{
				_ = NoiseData[k, l, zLevel];
				int num4 = (int)((array[k + l * width].b - num2) / ((num - num2) / (float)bands));
				if (num4 < 0)
				{
					num4 = 0;
				}
				if (num4 > bands - 1)
				{
					num4 = bands - 1;
				}
				if (num4 == 0)
				{
					array[k + l * width] = Color.red;
				}
				if (num4 == 1)
				{
					array[k + l * width] = Color.yellow;
				}
				if (num4 == 2)
				{
					array[k + l * width] = Color.green;
				}
				if (num4 == 3)
				{
					array[k + l * width] = Color.blue;
				}
				if (num4 == 4)
				{
					array[k + l * width] = Color.magenta;
				}
				if (num4 == 5)
				{
					array[k + l * width] = Color.cyan;
				}
			}
		}
		RawImage component = GameObject.Find("NoiseMap").GetComponent<RawImage>();
		if (component.texture == null)
		{
			component.texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
			component.texture.filterMode = FilterMode.Point;
		}
		((Texture2D)component.texture).SetPixels(array);
		((Texture2D)component.texture).Apply();
	}

	public void GenerateNoise(string Type)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		LayeredNoise layeredNoise = null;
		if (Type == "Linear")
		{
			layeredNoise = LayeredNoise.CreateLinearOctiveLayers(Layers, OctaveStrength, BaseStride);
		}
		if (Type == "Crazy")
		{
			layeredNoise = LayeredNoise.CreateCrazyOctiveLayers(Layers, OctaveStrength, BaseStride);
		}
		layeredNoise.BaseSampleMultiple = 0.05f;
		NoiseData = layeredNoise.Generate3D(width, height, depth);
		stopwatch.Stop();
		UpdateImage();
		UnityEngine.Debug.Log($"Time taken to generate noise: {stopwatch.ElapsedMilliseconds} ms");
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
