using System.Collections.Generic;
using UnityEngine;
using XRL.Rules;

namespace Genkit;

public class ColorOutputMap
{
	public static readonly Color32 BLACK = new Color32(0, 0, 0, byte.MaxValue);

	public static readonly Color32 RED = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	public static readonly Color32 MAGENTA = new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);

	public static readonly Color32 GREEN = new Color32(0, byte.MaxValue, 0, byte.MaxValue);

	public static readonly Color32 BLUE = new Color32(0, 0, byte.MaxValue, byte.MaxValue);

	public static readonly Color32 WHITE = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static readonly Color32 YELLOW = new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);

	public Color32[] pixels;

	public int width;

	public int height;

	public int extrawidth;

	public int extraheight;

	public ColorOutputMap(int width, int height)
	{
		this.width = width;
		this.height = height;
		pixels = new Color32[width * height];
		for (int i = 0; i < width * height; i++)
		{
			pixels[i] = WHITE;
		}
	}

	public ColorOutputMap(WaveCollapseModelBase model)
	{
		pixels = model.GetResult();
		width = model.FMX;
		height = model.FMY;
	}

	public ColorOutputMap(WaveCollapseFastModelBase model)
	{
		pixels = model.GetResult();
		width = model.FMX;
		height = model.FMY;
	}

	public ColorOutputMap(Color32[] pixels, int width, int height)
	{
		this.pixels = pixels;
		this.width = width;
		this.height = height;
	}

	public void HMirror()
	{
		for (int i = 0; i < width / 2; i++)
		{
			for (int j = 0; j < height; j++)
			{
				setPixel(width - i - 1, j, getPixel(i, j));
			}
		}
	}

	public void VMirror()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height / 2; j++)
			{
				setPixel(i, height - j - 1, getPixel(i, j));
			}
		}
	}

	public void Trim(Color32 colorToTrim, Color32 colorToTrimTo)
	{
		int num = 1;
		while (num != 0)
		{
			num = 0;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					if (getPixel(i, j).Equals(colorToTrim))
					{
						int num2 = 0;
						if (i > 0 && getPixel(i - 1, j).Equals(colorToTrim))
						{
							num2++;
						}
						if (i < width - 1 && getPixel(i + 1, j).Equals(colorToTrim))
						{
							num2++;
						}
						if (j > 0 && getPixel(i, j - 1).Equals(colorToTrim))
						{
							num2++;
						}
						if (j < height - 1 && getPixel(i, j + 1).Equals(colorToTrim))
						{
							num2++;
						}
						if (num2 == 1)
						{
							num++;
							setPixel(i, j, colorToTrimTo);
						}
					}
				}
			}
		}
	}

	public int CountColor(Color32 colorToCount)
	{
		int num = 0;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (getPixel(i, j).Equals(colorToCount))
				{
					num++;
				}
			}
		}
		return num;
	}

	public List<ColorOutputMap> CarveSubmaps(List<Color32> colorsToCombine)
	{
		return CarveSubmaps(colorsToCombine, WHITE);
	}

	public List<ColorOutputMap> CarveSubmaps(List<Color32> colorsToCombine, Color32 visitedColor)
	{
		List<ColorOutputMap> list = new List<ColorOutputMap>();
		ColorOutputMap colorOutputMap = Copy();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (!colorsToCombine.Contains(colorOutputMap.getPixel(i, j)))
				{
					continue;
				}
				List<Location2D> list2 = new List<Location2D>();
				List<Location2D> list3 = new List<Location2D>();
				Queue<Location2D> queue = new Queue<Location2D>();
				queue.Enqueue(Location2D.Get(i, j));
				int num = int.MaxValue;
				int num2 = int.MaxValue;
				int num3 = int.MinValue;
				int num4 = int.MinValue;
				int num5 = int.MaxValue;
				int num6 = int.MaxValue;
				int num7 = int.MinValue;
				int num8 = int.MinValue;
				while (queue.Count > 0)
				{
					Location2D location2D = queue.Dequeue();
					list2.Add(location2D);
					if (!colorsToCombine.Contains(colorOutputMap.getPixel(location2D.X, location2D.Y)))
					{
						continue;
					}
					if (location2D.X < num)
					{
						if (colorOutputMap.getPixel(location2D.X, location2D.Y).Equals(BLACK))
						{
							num5 = location2D.X;
						}
						num = location2D.X;
					}
					if (location2D.X > num3)
					{
						if (colorOutputMap.getPixel(location2D.X, location2D.Y).Equals(BLACK))
						{
							num7 = location2D.X;
						}
						num3 = location2D.X;
					}
					if (location2D.Y < num2)
					{
						if (colorOutputMap.getPixel(location2D.X, location2D.Y).Equals(BLACK))
						{
							num6 = location2D.Y;
						}
						num2 = location2D.Y;
					}
					if (location2D.Y > num4)
					{
						if (colorOutputMap.getPixel(location2D.X, location2D.Y).Equals(BLACK))
						{
							num8 = location2D.Y;
						}
						num4 = location2D.Y;
					}
					list3.Add(location2D);
					colorOutputMap.setPixel(location2D.X, location2D.Y, visitedColor);
					if (location2D.X - 1 >= 0 && !list2.Contains(Location2D.Get(location2D.X - 1, location2D.Y)))
					{
						queue.Enqueue(Location2D.Get(location2D.X - 1, location2D.Y));
					}
					if (location2D.X + 1 < width && !list2.Contains(Location2D.Get(location2D.X + 1, location2D.Y)))
					{
						queue.Enqueue(Location2D.Get(location2D.X + 1, location2D.Y));
					}
					if (location2D.Y - 1 >= 0 && !list2.Contains(Location2D.Get(location2D.X, location2D.Y - 1)))
					{
						queue.Enqueue(Location2D.Get(location2D.X, location2D.Y - 1));
					}
					if (location2D.Y - 1 < height && !list2.Contains(Location2D.Get(location2D.X, location2D.Y + 1)))
					{
						queue.Enqueue(Location2D.Get(location2D.X, location2D.Y + 1));
					}
				}
				int num9 = num3 - num + 1;
				int num10 = num4 - num2 + 1;
				ColorOutputMap colorOutputMap2 = new ColorOutputMap(num9, num10);
				foreach (Location2D item in list3)
				{
					colorOutputMap2.setPixel(item.X - num, item.Y - num2, getPixel(item.X, item.Y));
				}
				colorOutputMap2.extrawidth = 0;
				colorOutputMap2.extraheight = 0;
				if (num5 != int.MaxValue && num7 != int.MinValue)
				{
					colorOutputMap2.extrawidth = num7 - num5 + 1;
				}
				if (num6 != int.MaxValue && num8 != int.MinValue)
				{
					colorOutputMap2.extraheight = num8 - num6 + 1;
				}
				list.Add(colorOutputMap2);
			}
		}
		return list;
	}

	public ColorOutputMap Copy()
	{
		ColorOutputMap colorOutputMap = new ColorOutputMap(width, height);
		for (int i = 0; i < pixels.Length; i++)
		{
			colorOutputMap.pixels[i] = pixels[i];
		}
		return colorOutputMap;
	}

	public void Paste(ColorOutputMap map, int xpos, int ypos)
	{
		for (int i = 0; i < map.width && xpos + i < width; i++)
		{
			for (int j = 0; j < map.height && ypos + j < height; j++)
			{
				setPixel(xpos + i, ypos + j, map.getPixel(i, j));
			}
		}
	}

	public Point2D findPixelWithColor(Color32 c)
	{
		List<Point2D> list = new List<Point2D>();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (pixels[i + j * width].Equals(c))
				{
					list.Add(new Point2D(i, j));
				}
			}
		}
		if (list.Count > 0)
		{
			return list[Stat.Random(0, list.Count - 1)];
		}
		return Point2D.invalid;
	}

	public Point2D getFirstPixelWithColor(Color32 c)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (pixels[i + j * width].Equals(c))
				{
					return new Point2D(i, j);
				}
			}
		}
		return Point2D.invalid;
	}

	public Color32 getPixel(int x, int y)
	{
		if (x >= 0 && y >= 0 && x < width && y < height)
		{
			return pixels[x + y * width];
		}
		return Color.magenta;
	}

	public void setPixel(int x, int y, Color32 c)
	{
		if (x >= 0 && y >= 0 && x < width && y < height)
		{
			pixels[x + y * width] = c;
		}
	}

	public void ReplaceBorders(Color32 oldColor, Color32 newColor)
	{
		for (int i = 0; i < width; i++)
		{
			if (WaveCollapseTools.equals(getPixel(i, 0), oldColor))
			{
				setPixel(i, 0, newColor);
			}
			if (WaveCollapseTools.equals(getPixel(i, height - 1), oldColor))
			{
				setPixel(i, height - 1, newColor);
			}
		}
		for (int j = 1; j < height - 1; j++)
		{
			if (WaveCollapseTools.equals(getPixel(0, j), oldColor))
			{
				setPixel(0, j, newColor);
			}
			if (WaveCollapseTools.equals(getPixel(width - 1, j), oldColor))
			{
				setPixel(width - 1, j, newColor);
			}
		}
	}
}
