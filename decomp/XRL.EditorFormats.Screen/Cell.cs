using System;
using ConsoleLib.Console;
using UnityEngine;

namespace XRL.EditorFormats.Screen;

[Serializable]
public class Cell
{
	public static System.Random r = new System.Random();

	public char Char;

	public char Foreground;

	public char Background;

	public Cell()
	{
		Char = ' ';
		Foreground = 'K';
		Background = 'k';
	}

	public static Color GetColor(char c)
	{
		Color value = Color.grey;
		if (ConsoleLib.Console.ColorUtility.ColorMap.TryGetValue(c, out value))
		{
			return value;
		}
		return Color.grey;
	}
}
