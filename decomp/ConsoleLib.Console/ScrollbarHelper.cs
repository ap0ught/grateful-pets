using System;

namespace ConsoleLib.Console;

public class ScrollbarHelper
{
	public enum Orientation
	{
		Horizontal = 1,
		Vertical
	}

	public class ScrollbarVisual
	{
		public ushort BackgroundChar = 177;

		public ushort ActiveChar = 219;

		public ushort BackgroundColor = ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Black), TextColor.Black);

		public ushort ActiveColor = ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Grey), TextColor.Black);
	}

	public const int SameAsSize = -999;

	public static ScrollbarVisual DefaultVisual = new ScrollbarVisual();

	public static void Paint(ScreenBuffer buffer, int top, int left, int size, Orientation orient = Orientation.Vertical, int handleMin = 0, int handleMax = -999, int handleStart = 0, int handleEnd = 10, ScrollbarVisual visual = null)
	{
		visual = visual ?? DefaultVisual;
		if (handleMax == -999)
		{
			handleMax = size;
		}
		if (handleMax == 0)
		{
			handleMax = -1;
		}
		int num = (int)Math.Floor((float)size * (float)(handleStart - handleMin) / (float)(handleMax - handleMin));
		int val = (int)Math.Floor((float)size * (float)(handleEnd - handleMin) / (float)(handleMax - handleMin));
		val = Math.Min(val, size);
		switch (orient)
		{
		case Orientation.Vertical:
			buffer.Fill(left, top, left, top + size, visual.BackgroundChar, visual.BackgroundColor);
			if (handleMax > 0)
			{
				buffer.Fill(left, top + num, left, top + val, visual.ActiveChar, visual.ActiveColor);
			}
			break;
		case Orientation.Horizontal:
			buffer.Fill(left, top, left + size, top, visual.BackgroundChar, visual.BackgroundColor);
			if (handleMax > 0)
			{
				buffer.Fill(left + num, top, left + val, top, visual.ActiveChar, visual.ActiveColor);
			}
			break;
		}
	}
}
