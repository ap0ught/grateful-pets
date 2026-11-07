using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;
using XRL.Core;
using XRL.World;

namespace ConsoleLib.Console;

[Serializable]
[GenerateSerializationPartial]
public class Renderable : IRenderable, IComposite
{
	public string Tile;

	public string RenderString = " ";

	public string ColorString = "";

	public string TileColor;

	public char DetailColor;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	public Renderable()
	{
	}

	public Renderable(Renderable Source)
	{
		Tile = Source.Tile;
		RenderString = Source.RenderString;
		ColorString = Source.ColorString;
		TileColor = Source.TileColor;
		DetailColor = Source.DetailColor;
	}

	public Renderable(Renderable Source, string Tile = null, string RenderString = null, string ColorString = null, string TileColor = null, char? DetailColor = null)
	{
		this.Tile = Tile ?? Source?.Tile;
		this.RenderString = RenderString ?? Source?.RenderString;
		this.ColorString = ColorString ?? Source?.ColorString;
		this.TileColor = TileColor ?? Source?.TileColor;
		this.DetailColor = DetailColor ?? Source?.DetailColor ?? '\0';
	}

	public Renderable(IRenderable Source)
	{
		Copy(Source);
	}

	public void Copy(IRenderable Source)
	{
		Tile = Source.getTile();
		RenderString = Source.getRenderString();
		ColorString = Source.getColorString();
		TileColor = Source.getTileColor();
		DetailColor = Source.getDetailColor();
	}

	public static Renderable UITile(string tilePath)
	{
		return UITile(tilePath, 'y', 'W');
	}

	public static Renderable UITile(string tilePath, char foregroundColorCode = 'y', char detailColorCode = 'w', string noTileAlt = " ", char noTileColor = '\0')
	{
		char c = ((noTileColor != '\0') ? noTileColor : foregroundColorCode);
		string colorString = $"&{c}";
		return new Renderable(tilePath, noTileAlt, colorString, "&" + foregroundColorCode, detailColorCode);
	}

	private static char ColorCodeFromString(string color)
	{
		if (string.IsNullOrEmpty(color))
		{
			return '\0';
		}
		if (color.Length == 1)
		{
			return color[0];
		}
		return color[1];
	}

	public static Renderable UITile(string tilePath, string foregroundColorCode = "y", string detailColorCode = "w", string noTileAlt = " ", string noTileColor = null)
	{
		char c = ColorCodeFromString(noTileColor);
		if (c == '\0')
		{
			c = ColorCodeFromString(foregroundColorCode);
		}
		string colorString = $"&{c}";
		return new Renderable(tilePath, noTileAlt, colorString, "&" + ColorCodeFromString(foregroundColorCode), ColorCodeFromString(detailColorCode));
	}

	public Renderable(IRenderable Source, string Tile = null, string RenderString = null, string ColorString = null, string TileColor = null, char? DetailColor = null)
	{
		this.Tile = Tile ?? Source.getTile();
		this.RenderString = RenderString ?? Source.getRenderString();
		this.ColorString = ColorString ?? Source.getColorString();
		this.TileColor = TileColor ?? Source.getTileColor();
		this.DetailColor = DetailColor ?? Source.getDetailColor();
	}

	public Renderable(string Tile, string RenderString = " ", string ColorString = "", string TileColor = null, char DetailColor = '\0')
		: this()
	{
		setTile(Tile);
		setRenderString(RenderString);
		setColorString(ColorString);
		setTileColor(TileColor);
		setDetailColor(DetailColor);
	}

	public Renderable(GameObjectBlueprint Blueprint)
	{
		Set(Blueprint);
	}

	public void Set(GameObjectBlueprint Blueprint)
	{
		GamePartBlueprint part = Blueprint.GetPart("Render");
		Tile = part.GetParameterString("Tile");
		RenderString = part.GetParameterString("RenderString", " ");
		TileColor = part.GetParameterString("TileColor");
		ColorString = part.GetParameterString("ColorString", "");
		DetailColor = part.GetParameterString("DetailColor", "\0")[0];
	}

	public Renderable setTile(string val)
	{
		Tile = val;
		return this;
	}

	public string getTile()
	{
		return Tile;
	}

	public Renderable setRenderString(string val)
	{
		RenderString = val;
		return this;
	}

	public string getRenderString()
	{
		return RenderString;
	}

	public Renderable setColorString(string val)
	{
		ColorString = val;
		return this;
	}

	public string getColorString()
	{
		return ColorString;
	}

	public Renderable setTileColor(string val)
	{
		TileColor = val;
		return this;
	}

	public string getTileColor()
	{
		return TileColor;
	}

	public Renderable setDetailColor(char val)
	{
		DetailColor = val;
		return this;
	}

	public Renderable setDetailColor(string val)
	{
		DetailColor = ((!string.IsNullOrEmpty(val)) ? val[0] : '\0');
		return this;
	}

	public char getDetailColor()
	{
		return DetailColor;
	}

	public char GetForegroundColor()
	{
		return ColorUtility.FindLastForeground(ResolveColorString()) ?? 'y';
	}

	public char GetBackgroundColor()
	{
		return ColorUtility.FindLastBackground(ResolveColorString()) ?? 'k';
	}

	public string ResolveColorString()
	{
		if (Globals.RenderMode == RenderModeType.Tiles)
		{
			string tileColor = getTileColor();
			if (!tileColor.IsNullOrEmpty())
			{
				return tileColor;
			}
		}
		return getColorString();
	}

	public ColorChars getColorChars()
	{
		char foreground = 'y';
		char background = 'k';
		string text = ResolveColorString();
		if (Globals.RenderMode == RenderModeType.Tiles)
		{
			text = getTileColor();
			if (text.IsNullOrEmpty())
			{
				text = getColorString();
			}
		}
		else
		{
			text = getColorString();
		}
		if (!text.IsNullOrEmpty())
		{
			int num = text.LastIndexOf(ColorChars.FOREGROUND_INDICATOR);
			int num2 = text.LastIndexOf(ColorChars.BACKGROUND_INDICATOR);
			if (num >= 0 && num < text.Length - 1)
			{
				foreground = text[num + 1];
			}
			if (num2 >= 0 && num2 < text.Length - 1)
			{
				background = text[num2 + 1];
			}
		}
		return new ColorChars
		{
			detail = getDetailColor(),
			foreground = foreground,
			background = background
		};
	}

	public virtual bool getHFlip()
	{
		return false;
	}

	public virtual bool getVFlip()
	{
		return false;
	}

	public string GetSpriteName()
	{
		if (Tile != null)
		{
			return Tile;
		}
		if (RenderString.Length == 0)
		{
			return "Text/" + 32 + ".bmp";
		}
		return "Text/" + (int)RenderString[0] + ".bmp";
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Tile);
		Writer.WriteOptimized(RenderString);
		Writer.WriteOptimized(ColorString);
		Writer.WriteOptimized(TileColor);
		Writer.Write(DetailColor);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		Tile = Reader.ReadOptimizedString();
		RenderString = Reader.ReadOptimizedString();
		ColorString = Reader.ReadOptimizedString();
		TileColor = Reader.ReadOptimizedString();
		DetailColor = Reader.ReadChar();
	}
}
