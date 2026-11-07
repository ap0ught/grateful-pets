using System;
using ConsoleLib.Console;
using Genkit;

namespace XRL;

/// This class is not used in the base game.
[Serializable]
public class SifrahRenderable : Renderable
{
	public int X;

	public int Y;

	public virtual string GetAltColorString(string ColorString)
	{
		return ColorString;
	}

	public virtual string GetAltTileColor(string ColorString)
	{
		return ColorString;
	}

	public virtual char GetAltDetailColor(char DetailColor)
	{
		return DetailColor;
	}

	public virtual string GetHighlightColorString()
	{
		if (ColorString.IsNullOrEmpty())
		{
			return ColorString + "^Y";
		}
		return "^Y";
	}

	public virtual string GetHighlightTileColor()
	{
		string text = TileColor;
		if (text != null)
		{
			text += "^Y";
		}
		return text;
	}

	public virtual char GetHighlightDetailColor()
	{
		return DetailColor;
	}

	public void WriteTo(ScreenBuffer Buffer)
	{
		Buffer.Write(this);
	}

	public void WriteTo(ScreenBuffer Buffer, string ColorString, char DetailColor)
	{
		Buffer.Write(this, null, null, GetAltColorString(ColorString), GetAltTileColor(ColorString), GetAltDetailColor(DetailColor));
	}

	public void WriteTo(ScreenBuffer Buffer, string ColorString)
	{
		Buffer.Write(this, null, null, GetAltColorString(ColorString), GetAltTileColor(ColorString));
	}

	public void WriteHighlightedTo(ScreenBuffer Buffer)
	{
		Buffer.Write(this, null, null, GetHighlightColorString(), GetHighlightTileColor(), GetHighlightDetailColor());
	}

	public Renderable GetAlternate(string ColorString, char DetailColor)
	{
		return new Renderable(this, null, null, GetAltColorString(ColorString), GetAltTileColor(ColorString), GetAltDetailColor(DetailColor));
	}

	public Renderable GetAlternate(string ColorString)
	{
		return new Renderable(this, null, null, GetAltColorString(ColorString), GetAltTileColor(ColorString));
	}

	public Location2D GetLocation(int DX = 0, int DY = 0)
	{
		return Location2D.Get(X + DX, Y + DY);
	}
}
