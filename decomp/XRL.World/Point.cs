using System;
using Genkit;

namespace XRL.World;

[Serializable]
public class Point
{
	public int X;

	public int Y;

	public int Direction;

	public char DisplayChar;

	public Location2D location => Location2D.Get(X, Y);

	public Point(int x, int y)
	{
		X = x;
		Y = y;
		Direction = 0;
		DisplayChar = ' ';
	}

	public Point(int x, int y, int direction, char displaychar)
	{
		X = x;
		Y = y;
		Direction = direction;
		DisplayChar = displaychar;
	}
}
