using System;
using System.Collections;
using System.Collections.Generic;

namespace Genkit;

[Serializable]
public class Grid<T> : IEnumerable<T>, IEnumerable
{
	public int width;

	public int height;

	public T[,] cells;

	public Grid(int width, int height)
	{
		this.width = width;
		this.height = height;
		cells = new T[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				cells[i, j] = default;
			}
		}
	}

	public void fill(T fill)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				cells[i, j] = fill;
			}
		}
	}

	public Grid<T> mirrorHorizontal()
	{
		Grid<T> grid = new Grid<T>(width, height);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (i < width / 2)
				{
					grid.cells[i, j] = cells[i, j];
				}
				else
				{
					grid.cells[i, j] = cells[width - i - 1, j];
				}
			}
		}
		return grid;
	}

	public Grid<T> mirrorVertical()
	{
		Grid<T> grid = new Grid<T>(width, height);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (j < height / 2)
				{
					grid.cells[i, j] = cells[i, j];
				}
				else
				{
					grid.cells[i, j] = cells[i, height - j - 1];
				}
			}
		}
		return grid;
	}

	public T get(int x, int y)
	{
		return cells[x, y];
	}

	public void setRect(int x1, int y1, int x2, int y2, T value)
	{
		for (int i = x1; i <= x2; i++)
		{
			for (int j = y1; j <= y2; j++)
			{
				set(i, j, value);
			}
		}
	}

	public void set(int x, int y, T value)
	{
		cells[x, y] = value;
	}

	public T[,] copy()
	{
		return transformCopy((T element) => element);
	}

	public InfluenceMap regionalize(Func<int, int, T, int> wallTest, bool costOnly = true)
	{
		InfluenceMap regionMap = new InfluenceMap(width, height);
		forEach((int x, int y, T c) =>
		{
			regionMap.Walls[x, y] = wallTest(x, y, c);
		});
		regionMap.SeedAllUnseeded();
		if (costOnly)
		{
			regionMap.RecalculateCostOnly();
		}
		else
		{
			regionMap.Recalculate();
		}
		return regionMap;
	}

	public void forEach(Action<int, int, T> action)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				action(i, j, get(i, j));
			}
		}
	}

	public void box(Rect2D rect, Func<T> value)
	{
		box(rect.x1, rect.y1, rect.Width, rect.Height, value);
	}

	public void box(int xp, int yp, int width, int height, Func<T> value)
	{
		for (int i = xp; i < xp + width; i++)
		{
			for (int j = yp; j < yp + height; j++)
			{
				set(i, j, value());
			}
		}
	}

	public void box(int xp, int yp, int width, int height, Func<int, int, T> value)
	{
		for (int i = xp; i < xp + width; i++)
		{
			for (int j = yp; j < yp + height; j++)
			{
				set(i, j, value(i, j));
			}
		}
	}

	public U[,] transformCopy<U>(Func<T, U> transform)
	{
		U[,] array = new U[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				array[i, j] = transform(cells[i, j]);
			}
		}
		return array;
	}

	public void transform(Func<T, T> transform)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				cells[i, j] = transform(get(i, j));
			}
		}
	}

	public void transform(Func<int, int, T, T> transform)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				cells[i, j] = transform(i, j, get(i, j));
			}
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				yield return get(x, y);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				yield return get(x, y);
			}
		}
	}
}
