using System;
using UnityEngine;
using XRL.Rules;

namespace Genkit;

public static class GridExtensions
{
	public static Texture2D toTexture2D(this Grid<Color> grid)
	{
		Texture2D texture2D = new Texture2D(240, 75);
		texture2D.filterMode = FilterMode.Point;
		for (int i = 0; i < grid.width; i++)
		{
			for (int j = 0; j < grid.height; j++)
			{
				texture2D.SetPixel(i, grid.height - j - 1, grid.get(i, j));
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D toTexture2D(this Grid<Color4> grid)
	{
		Texture2D texture2D = new Texture2D(240, 75);
		texture2D.filterMode = FilterMode.Point;
		for (int i = 0; i < grid.width; i++)
		{
			for (int j = 0; j < grid.height; j++)
			{
				texture2D.SetPixel(i, grid.height - j - 1, grid.get(i, j).toUnityColor);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public static Sprite toSprite(this Grid<Color> grid)
	{
		return Sprite.Create(grid.toTexture2D(), new Rect(0f, 0f, grid.width, grid.height), new Vector2(0f, 0f));
	}

	public static Sprite toSprite(this Grid<Color4> grid)
	{
		return Sprite.Create(grid.toTexture2D(), new Rect(0f, 0f, grid.width, grid.height), new Vector2(0f, 0f));
	}

	public static void fromWFCTemplate(this Grid<Color> grid, string templateName, int n = 3, int seed = int.MaxValue)
	{
		WaveCollapseTools.LoadTemplates();
		WaveCollapseFastModel waveCollapseFastModel = new WaveCollapseFastModel(templateName, n, grid.width, grid.height, periodicInput: true, periodicOutput: false, 8, 0);
		if (seed == int.MaxValue)
		{
			seed = new System.Random().Next();
		}
		waveCollapseFastModel.Run(seed, 0);
		Color32[] result = waveCollapseFastModel.GetResult();
		for (int i = 0; i < grid.width; i++)
		{
			for (int j = 0; j < grid.height; j++)
			{
				grid.set(i, j, new Color((int)result[i + j * grid.width].r, (int)result[i + j * grid.width].g, (int)result[i + j * grid.width].b));
			}
		}
	}

	public static void fromWFCTemplate(this Grid<Color4> grid, string templateName, int n = 3, int seed = int.MaxValue)
	{
		WaveCollapseTools.LoadTemplates();
		WaveCollapseFastModel waveCollapseFastModel = new WaveCollapseFastModel(templateName, n, grid.width, grid.height, periodicInput: true, periodicOutput: false, 8, 0);
		if (seed == int.MaxValue)
		{
			seed = Stat.Random(0, 2147483646);
		}
		waveCollapseFastModel.Run(seed, 0);
		Color32[] result = waveCollapseFastModel.GetResult();
		for (int i = 0; i < grid.width; i++)
		{
			for (int j = 0; j < grid.height; j++)
			{
				grid.set(i, j, new Color4((int)result[i + j * grid.width].r, (int)result[i + j * grid.width].g, (int)result[i + j * grid.width].b));
			}
		}
	}
}
