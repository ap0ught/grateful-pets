using System.Collections.Generic;
using UnityEngine;
using ex2D.Detail;

public class exTextureInfo : ScriptableObject
{
	public enum DiceType
	{
		Empty,
		Max,
		Trimmed
	}

	public struct Dice
	{
		public DiceType sizeType;

		public int offset_x;

		public int offset_y;

		public int width;

		public int height;

		public int x;

		public int y;

		public bool rotated;

		public int rotatedWidth
		{
			get
			{
				if (!rotated)
				{
					return width;
				}
				return height;
			}
		}

		public int rotatedHeight
		{
			get
			{
				if (!rotated)
				{
					return height;
				}
				return width;
			}
		}
	}

	public string rawTextureGUID = "";

	public string rawAtlasGUID = "";

	public Texture2D texture;

	public bool rotated;

	public bool trim;

	public int trimThreshold = 1;

	public int trim_x;

	public int trim_y;

	public int rawWidth = 1;

	public int rawHeight = 1;

	public int x;

	public int y;

	public int width = 1;

	public int height = 1;

	public int borderLeft;

	public int borderRight;

	public int borderTop;

	public int borderBottom;

	public int ShaderMode;

	[SerializeField]
	private List<int> diceData = new List<int>();

	public int rotatedWidth
	{
		get
		{
			if (rotated)
			{
				return height;
			}
			return width;
		}
	}

	public int rotatedHeight
	{
		get
		{
			if (rotated)
			{
				return width;
			}
			return height;
		}
	}

	public bool hasBorder
	{
		get
		{
			if (borderLeft == 0 && borderRight == 0 && borderTop == 0)
			{
				return borderBottom != 0;
			}
			return true;
		}
	}

	public int diceUnitWidth
	{
		get
		{
			if (diceData != null && diceData.Count > 0)
			{
				return diceData[0];
			}
			return 0;
		}
	}

	public int diceUnitHeight
	{
		get
		{
			if (diceData != null && diceData.Count > 0)
			{
				return diceData[1];
			}
			return 0;
		}
	}

	public bool isDiced
	{
		get
		{
			if (diceData != null && diceData.Count > 0)
			{
				if (diceData[0] <= 0 || diceData[0] >= width)
				{
					if (diceData[1] > 0)
					{
						return diceData[1] < height;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public DiceEnumerator dices => new DiceEnumerator(diceData, this);

	public static exTextureInfo Create(Texture2D _texture)
	{
		exTextureInfo exTextureInfo2 = ScriptableObject.CreateInstance<exTextureInfo>();
		Rect rect = new Rect(0f, 0f, _texture.width, _texture.height);
		exTextureInfo2.texture = _texture;
		exTextureInfo2.rotated = false;
		exTextureInfo2.trim = false;
		exTextureInfo2.trim_x = (int)rect.x;
		exTextureInfo2.trim_y = (int)rect.y;
		exTextureInfo2.width = (int)rect.width;
		exTextureInfo2.height = (int)rect.height;
		exTextureInfo2.x = (int)rect.x;
		exTextureInfo2.y = (int)rect.y;
		exTextureInfo2.rawWidth = _texture.width;
		exTextureInfo2.rawHeight = _texture.height;
		return exTextureInfo2;
	}
}
