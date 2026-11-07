using ConsoleLib.Console;
using Kobold;
using QupKit;
using UnityEngine;
using XRL.UI;
using XRL.World;

public class QudItemListElement : ObjectPool<QudItemListElement>
{
	public string displayName = "<unknown>";

	public string rightText = "";

	public XRL.World.GameObject go;

	public Color foregroundColor;

	public Color backgroundColor;

	public Color detailColor;

	public string tile;

	public string category;

	public exTextureInfo spriteInfo;

	public int weight;

	public void PoolReset()
	{
		weight = 0;
		go = null;
		rightText = null;
		spriteInfo = null;
		tile = null;
		displayName = null;
		category = null;
	}

	public void InitFrom(XRL.World.GameObject go)
	{
		if (go == null)
		{
			tile = "Items/bit1.bmp";
			displayName = Sidebar.FormatToRTF("&knothing");
			rightText = "";
			foregroundColor = new Color(0f, 0f, 0f, 0f);
			detailColor = new Color(0f, 0f, 0f, 0f);
			backgroundColor = new Color(0f, 0f, 0f, 0f);
			weight = 0;
			return;
		}
		this.go = go;
		displayName = go.DisplayName;
		if (displayName.Contains("&") || displayName.Contains("^") || displayName.Contains("{{"))
		{
			displayName = Sidebar.FormatToRTF(displayName);
		}
		if (go.Physics != null)
		{
			weight = go.Physics.Weight;
			rightText = weight + " lbs";
		}
		if (go.Render == null || go.Render.Tile == null)
		{
			return;
		}
		tile = go.Render.Tile;
		foregroundColor = Color.white;
		detailColor = Color.black;
		backgroundColor = new Color(0f, 0f, 0f, 0f);
		if (!string.IsNullOrEmpty(go.Render.DetailColor))
		{
			detailColor = ConsoleLib.Console.ColorUtility.ColorMap[go.Render.DetailColor[0]];
		}
		if (!string.IsNullOrEmpty(go.Render.TileColor))
		{
			for (int i = 0; i < go.Render.TileColor.Length; i++)
			{
				if (go.Render.TileColor[i] == '&' && i < go.Render.TileColor.Length - 1)
				{
					if (go.Render.TileColor[i + 1] == '&')
					{
						i++;
					}
					else
					{
						foregroundColor = ConsoleLib.Console.ColorUtility.ColorMap[go.Render.TileColor[i + 1]];
					}
				}
				if (go.Render.TileColor[i] == '^' && i < go.Render.TileColor.Length - 1)
				{
					if (go.Render.TileColor[i + 1] == '^')
					{
						i++;
					}
					else
					{
						backgroundColor = ConsoleLib.Console.ColorUtility.ColorMap[go.Render.TileColor[i + 1]];
					}
				}
			}
		}
		else
		{
			if (string.IsNullOrEmpty(go.Render.ColorString))
			{
				return;
			}
			for (int j = 0; j < go.Render.ColorString.Length; j++)
			{
				if (go.Render.ColorString[j] == '&' && j < go.Render.ColorString.Length - 1)
				{
					if (go.Render.ColorString[j + 1] == '&')
					{
						j++;
					}
					else
					{
						foregroundColor = ConsoleLib.Console.ColorUtility.ColorMap[go.Render.ColorString[j + 1]];
					}
				}
				if (go.Render.ColorString[j] == '^' && j < go.Render.ColorString.Length - 1)
				{
					if (go.Render.ColorString[j + 1] == '^')
					{
						j++;
					}
					else
					{
						backgroundColor = ConsoleLib.Console.ColorUtility.ColorMap[go.Render.ColorString[j + 1]];
					}
				}
			}
		}
	}

	public Sprite GenerateSprite()
	{
		if (tile == null)
		{
			return null;
		}
		if (spriteInfo == null)
		{
			spriteInfo = SpriteManager.GetTextureInfo(tile);
		}
		return SpriteManager.GetUnitySprite(spriteInfo);
	}
}
