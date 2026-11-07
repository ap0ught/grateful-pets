using System;
using System.CodeDom.Compiler;
using System.Text;
using ConsoleLib.Console;
using Kobold;
using Newtonsoft.Json;
using Occult.Engine.CodeGeneration;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
[GeneratePoolingPartial(Capacity = 128)]
[GenerateSerializationPartial]
public class Render : IPart, IRenderable
{
	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	private static readonly IPartPool RenderPool = new IPartPool(128);

	public const int RENDER_FLAG_OCCLUDING = 1;

	public const int RENDER_FLAG_IF_DARK = 2;

	public const int RENDER_FLAG_VISIBLE = 4;

	public const int RENDER_FLAG_CUSTOM = 8;

	public const int RENDER_FLAG_HFLIP = 16;

	public const int RENDER_FLAG_VFLIP = 32;

	public const int RENDER_FLAG_IGNORE_COLOR_FOR_STACK = 64;

	public const int RENDER_FLAG_IGNORE_TILE_FOR_STACK = 128;

	public const int RENDER_FLAG_NEVER = 256;

	public const int RENDER_FLAG_PARTY_FLIP = 512;

	public string DisplayName;

	public string RenderString = "?";

	public string ColorString = "&y";

	public string DetailColor = "";

	public string TileColor = "";

	public int RenderLayer;

	public int Flags = 4;

	public string Tile;

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override IPartPool Pool => RenderPool;

	[JsonIgnore]
	public bool Occluding
	{
		get
		{
			return (Flags & 1) == 1;
		}
		set
		{
			Flags = (value ? (Flags | 1) : (Flags & -2));
		}
	}

	[JsonIgnore]
	public bool RenderIfDark
	{
		get
		{
			return (Flags & 2) == 2;
		}
		set
		{
			Flags = (value ? (Flags | 2) : (Flags & -3));
		}
	}

	[JsonIgnore]
	public new bool Visible
	{
		get
		{
			return (Flags & 4) == 4;
		}
		set
		{
			Flags = (value ? (Flags | 4) : (Flags & -5));
		}
	}

	[JsonIgnore]
	public bool CustomRender
	{
		get
		{
			return (Flags & 8) == 8;
		}
		set
		{
			Flags = (value ? (Flags | 8) : (Flags & -9));
		}
	}

	[JsonIgnore]
	public bool HFlip
	{
		get
		{
			return (Flags & 0x10) == 16;
		}
		set
		{
			Flags = (value ? (Flags | 0x10) : (Flags & -17));
		}
	}

	[JsonIgnore]
	public bool VFlip
	{
		get
		{
			return (Flags & 0x20) == 32;
		}
		set
		{
			Flags = (value ? (Flags | 0x20) : (Flags & -33));
		}
	}

	[JsonIgnore]
	public bool IgnoreColorForStack
	{
		get
		{
			return (Flags & 0x40) == 64;
		}
		set
		{
			Flags = (value ? (Flags | 0x40) : (Flags & -65));
		}
	}

	[JsonIgnore]
	public bool IgnoreTileForStack
	{
		get
		{
			return (Flags & 0x80) == 128;
		}
		set
		{
			Flags = (value ? (Flags | 0x80) : (Flags & -129));
		}
	}

	[JsonIgnore]
	public bool Never
	{
		get
		{
			return (Flags & 0x100) == 256;
		}
		set
		{
			Flags = (value ? (Flags | 0x100) : (Flags & -257));
		}
	}

	[JsonIgnore]
	public bool PartyFlip
	{
		get
		{
			return (Flags & 0x200) == 512;
		}
		set
		{
			Flags = (value ? (Flags | 0x200) : (Flags & -513));
		}
	}

	[JsonIgnore]
	public override int Priority => 90000;

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override void Reset()
	{
		base.Reset();
		DisplayName = null;
		RenderString = "?";
		ColorString = "&y";
		DetailColor = "";
		TileColor = "";
		RenderLayer = 0;
		Flags = 4;
		Tile = null;
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(DisplayName);
		Writer.WriteOptimized(RenderString);
		Writer.WriteOptimized(ColorString);
		Writer.WriteOptimized(DetailColor);
		Writer.WriteOptimized(TileColor);
		Writer.WriteOptimized(RenderLayer);
		Writer.WriteOptimized(Flags);
		Writer.WriteOptimized(Tile);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		DisplayName = Reader.ReadOptimizedString();
		RenderString = Reader.ReadOptimizedString();
		ColorString = Reader.ReadOptimizedString();
		DetailColor = Reader.ReadOptimizedString();
		TileColor = Reader.ReadOptimizedString();
		RenderLayer = Reader.ReadOptimizedInt32();
		Flags = Reader.ReadOptimizedInt32();
		Tile = Reader.ReadOptimizedString();
	}

	public override void Initialize()
	{
		RenderString = ProcessRenderString(RenderString);
	}

	public override void Attach()
	{
		ParentObject.Render = this;
	}

	public override void Remove()
	{
		if (ParentObject?.Render == this)
		{
			ParentObject.Render = null;
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		return ID == SingletonEvent<GetDebugInternalsEvent>.ID;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "DisplayName", DisplayName);
		E.AddEntry(this, "RenderString", RenderString);
		E.AddEntry(this, "ColorString", ColorString);
		E.AddEntry(this, "TileColor", TileColor);
		E.AddEntry(this, "DetailColor", DetailColor);
		E.AddEntry(this, "RenderLayer", RenderLayer);
		E.AddEntry(this, "Visible", Visible);
		E.AddEntry(this, "Occluding", Occluding);
		E.AddEntry(this, "RenderIfDark", RenderIfDark);
		E.AddEntry(this, "CustomRender", CustomRender);
		E.AddEntry(this, "Tile", Tile);
		return base.HandleEvent(E);
	}

	public string GetTileForegroundColor()
	{
		if (TileColor != null)
		{
			int num = TileColor.LastIndexOf('&');
			if (num >= 0)
			{
				return TileColor[num + 1].ToString();
			}
		}
		return GetForegroundColor();
	}

	public string GetForegroundColor()
	{
		if (ColorString != null)
		{
			int num = ColorString.LastIndexOf('&');
			if (num >= 0)
			{
				return ColorString[num + 1].ToString();
			}
		}
		return "y";
	}

	public char GetTileForegroundColorChar()
	{
		if (TileColor != null)
		{
			int num = TileColor.LastIndexOf('&');
			if (num >= 0)
			{
				return TileColor[num + 1];
			}
		}
		return GetForegroundColorChar();
	}

	public char GetForegroundColorChar()
	{
		if (ColorString != null)
		{
			int num = ColorString.LastIndexOf('&');
			if (num >= 0)
			{
				return ColorString[num + 1];
			}
		}
		return 'y';
	}

	public string GetBackgroundColor()
	{
		if (ColorString != null)
		{
			int num = ColorString.LastIndexOf('^');
			if (num >= 0)
			{
				return ColorString[num + 1].ToString();
			}
		}
		return "k";
	}

	public void SetForegroundColor(char color)
	{
		if (string.IsNullOrEmpty(ColorString))
		{
			ColorString = "&" + color;
		}
		else if (ColorString.Contains("&"))
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append(ColorString);
			stringBuilder[ColorString.LastIndexOf('&') + 1] = color;
			ColorString = stringBuilder.ToString();
		}
		else
		{
			ColorString = "&" + color + ColorString;
		}
		if (!string.IsNullOrEmpty(TileColor))
		{
			if (TileColor.Contains("&"))
			{
				StringBuilder stringBuilder2 = Event.NewStringBuilder();
				stringBuilder2.Append(TileColor);
				stringBuilder2[TileColor.LastIndexOf('&') + 1] = color;
				TileColor = stringBuilder2.ToString();
			}
			else
			{
				TileColor = "&" + color + TileColor;
			}
		}
	}

	public void SetForegroundColor(string color)
	{
		if (!string.IsNullOrEmpty(color))
		{
			if (color[0] == '&' && color.Length > 1)
			{
				SetForegroundColor(color[1]);
			}
			else
			{
				SetForegroundColor(color[0]);
			}
		}
	}

	public void SetBackgroundColor(char color)
	{
		if (string.IsNullOrEmpty(ColorString))
		{
			ColorString = "^" + color;
		}
		else if (ColorString.Contains("^"))
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append(ColorString);
			stringBuilder[ColorString.LastIndexOf('^') + 1] = color;
			ColorString = stringBuilder.ToString();
		}
		else
		{
			ColorString = ColorString + "^" + color;
		}
		if (!string.IsNullOrEmpty(TileColor))
		{
			if (TileColor.Contains("^"))
			{
				StringBuilder stringBuilder2 = Event.NewStringBuilder();
				stringBuilder2.Append(TileColor);
				stringBuilder2[TileColor.LastIndexOf('^') + 1] = color;
				TileColor = stringBuilder2.ToString();
			}
			else
			{
				TileColor = TileColor + "^" + color;
			}
		}
	}

	public void SetBackgroundColor(string color)
	{
		if (!string.IsNullOrEmpty(color))
		{
			if (color[0] == '^' && color.Length > 1)
			{
				SetBackgroundColor(color[1]);
			}
			else
			{
				SetBackgroundColor(color[0]);
			}
		}
	}

	public bool SetRenderString(string s)
	{
		string text = ProcessRenderString(s);
		if (text != RenderString)
		{
			RenderString = text;
			return true;
		}
		return false;
	}

	public override bool SameAs(IPart p)
	{
		Render render = p as Render;
		if (render.DisplayName != DisplayName)
		{
			return false;
		}
		if (render.RenderString != RenderString)
		{
			return false;
		}
		if (render.ColorString != ColorString && !IgnoreColorForStack)
		{
			return false;
		}
		if (render.DetailColor != DetailColor && !IgnoreColorForStack)
		{
			return false;
		}
		if (render.TileColor != TileColor && !IgnoreColorForStack)
		{
			return false;
		}
		if (render.RenderLayer != RenderLayer)
		{
			return false;
		}
		if (render.Visible != Visible)
		{
			return false;
		}
		if (render.Occluding != Occluding)
		{
			return false;
		}
		if (render.RenderIfDark != RenderIfDark)
		{
			return false;
		}
		if (render.CustomRender != CustomRender)
		{
			return false;
		}
		if (render.Tile != Tile && !IgnoreTileForStack)
		{
			return false;
		}
		if (render.Flags != Flags)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public static string ProcessRenderString(string what)
	{
		if (what.Length > 1)
		{
			what = ((char)Convert.ToInt32(what)).ToString();
		}
		else if (what == "&")
		{
			what = "&&";
		}
		else if (what == "^")
		{
			what = "^^";
		}
		return what;
	}

	public bool getHFlip()
	{
		if (!PartyFlip)
		{
			return HFlip;
		}
		return !HFlip;
	}

	public bool getVFlip()
	{
		return VFlip;
	}

	public string getTile()
	{
		return Tile;
	}

	public string getRenderString()
	{
		return RenderString;
	}

	public string getColorString()
	{
		return ColorString;
	}

	public string getTileOrRenderColor()
	{
		if (!string.IsNullOrEmpty(TileColor))
		{
			return TileColor;
		}
		return GetForegroundColor();
	}

	public string getTileColor()
	{
		if (!string.IsNullOrEmpty(TileColor))
		{
			return TileColor;
		}
		return null;
	}

	public char getDetailColor()
	{
		if (!string.IsNullOrEmpty(DetailColor))
		{
			return DetailColor[0];
		}
		return '\0';
	}

	public string GetRenderColor()
	{
		if (Globals.RenderMode != RenderModeType.Tiles)
		{
			return ColorString;
		}
		if (!string.IsNullOrEmpty(TileColor))
		{
			return TileColor;
		}
		return ColorString;
	}

	public ColorChars getColorChars()
	{
		char foreground = 'y';
		char background = 'k';
		string text;
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
		if (!string.IsNullOrEmpty(text))
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

	public override void BasisError(GameObject Basis, SerializationReader Reader)
	{
		if (!Tile.IsNullOrEmpty() && !SpriteManager.HasTextureInfo(Tile))
		{
			string text = Basis.GetBlueprint(UseDefault: false)?.GetPartParameter("Render", "Tile", "");
			if (!text.IsNullOrEmpty() && SpriteManager.HasTextureInfo(Tile))
			{
				Tile = text;
			}
			else
			{
				Tile = null;
			}
		}
	}
}
