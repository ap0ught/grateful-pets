using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Kobold;
using UnityEngine;

[ExecuteAlways]
public class MossyImposter : MonoBehaviour, IConfigurableExtra
{
	public SpriteRenderer spriteRenderer;

	public int count;

	public Texture2D texture;

	public Texture2D moss;

	public string lastConfig;

	private static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

	public bool Test;

	public void Configure(string configurationString)
	{
		if (configurationString == lastConfig)
		{
			return;
		}
		lastConfig = configurationString;
		if (sprites.ContainsKey(configurationString))
		{
			spriteRenderer.sprite = sprites[configurationString];
			return;
		}
		string[] array = configurationString.Split('~');
		int seed = Convert.ToInt32(array[1]);
		texture = SpriteManager.GetUnitySprite(array[0]).texture;
		if (moss == null)
		{
			moss = new Texture2D(16, 24, TextureFormat.RGBA32, mipChain: false);
		}
		System.Random random = new System.Random(seed);
		int num = random.Next(-2, 2);
		for (int i = 0; i < texture.width; i++)
		{
			for (int j = 0; j < texture.height; j++)
			{
				if (texture.GetPixel(i, j).a != 0f && random.Next(j, 6 + j) + num < 12)
				{
					moss.SetPixel(i, j, ConsoleLib.Console.ColorUtility.colorFromChar('g'));
				}
				else
				{
					moss.SetPixel(i, j, Color.clear);
				}
			}
		}
		moss.Apply();
		moss.filterMode = UnityEngine.FilterMode.Point;
		spriteRenderer.sprite = Sprite.Create(moss, new Rect(0f, 0f, moss.width, moss.height), new Vector2(0.5f, 0.5f));
		sprites.Add(configurationString, spriteRenderer.sprite);
	}

	public void Update()
	{
		lastConfig = null;
		if (Test)
		{
			Test = false;
			Configure("Creatures/sw_bearman5.bmp~5");
		}
	}
}
