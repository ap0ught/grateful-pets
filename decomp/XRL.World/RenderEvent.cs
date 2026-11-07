using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Core;

namespace XRL.World;

public class RenderEvent : IRenderable, IDisposable
{
	public static class Pool
	{
		private class Entry : RenderEvent
		{
			public bool Disposed;

			public override void Dispose()
			{
				if (!Disposed)
				{
					Reset();
					Disposed = true;
					Bag.Add(this);
				}
			}
		}

		private static ConcurrentBag<Entry> Bag = new ConcurrentBag<Entry>();

		public static RenderEvent Get()
		{
			if (!Bag.TryTake(out var result))
			{
				result = new Entry();
			}
			result.Disposed = false;
			return result;
		}
	}

	public static readonly int ID = MinEvent.RegisterEvent(typeof(RenderEvent), "ComponentRenderEvent");

	public static readonly int OverlayID = MinEvent.RegisterEvent(typeof(RenderEvent), "OverlayRenderEvent");

	public static readonly int FinalID = MinEvent.RegisterEvent(typeof(RenderEvent), "FinalRenderEvent");

	public int x;

	public int y;

	public bool WantsToPaint;

	public bool DisableFullscreenColorEffects;

	public bool Alt;

	public bool CustomDraw;

	public string WantsBackdrop;

	public bool BackdropBleedthrough;

	public bool AsIfKnown;

	public string _Tile;

	public StringBuilder Builder = new StringBuilder();

	public string Final;

	public string ColorString;

	public string RenderString;

	public string BackgroundString;

	public string DetailColor;

	public string Context;

	public int ColorStringPriority;

	public int DetailColorPriority;

	public int BackgroundStringPriority;

	public bool Visible;

	public bool NoWake;

	public bool UI;

	public bool HFlip;

	public bool VFlip;

	public List<ImposterExtra.ImposterInfo> Imposters = new List<ImposterExtra.ImposterInfo>();

	public int HighestLayer = -1;

	public LightLevel Lit = LightLevel.None;

	public string Tile
	{
		get
		{
			return _Tile;
		}
		set
		{
			if (value == null)
			{
				HFlip = false;
				VFlip = false;
			}
			_Tile = value;
		}
	}

	public bool ColorsVisible
	{
		get
		{
			if (Zone.ColorsVisible(Lit))
			{
				return !Alt;
			}
			return false;
		}
	}

	string IRenderable.getTile()
	{
		return Tile;
	}

	public string getColorString()
	{
		return ColorString;
	}

	public string getRenderString()
	{
		return RenderString;
	}

	public char getDetailColor()
	{
		if (string.IsNullOrEmpty(DetailColor))
		{
			return '\0';
		}
		return DetailColor[0];
	}

	public RenderEvent GreyOutForUI()
	{
		DetailColor = "K";
		ColorString = "&K";
		return this;
	}

	public void RenderEffectIndicator(string renderString, string tile, string colorString, string detailColor, int frameHint, int durationHint = 10)
	{
		if (!UI)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num >= frameHint && num < frameHint + durationHint)
			{
				Tile = tile;
				RenderString = renderString;
				ColorString = colorString;
				DetailColor = detailColor;
			}
		}
	}

	public bool ImpostersMatch(RenderEvent ev)
	{
		if (ev.Imposters.Count != Imposters.Count)
		{
			return false;
		}
		for (int i = 0; i < ev.Imposters.Count; i++)
		{
			if (Imposters[i].SameAs(ev.Imposters[i]))
			{
				return false;
			}
		}
		return true;
	}

	public void Reset()
	{
		Final = null;
		ColorString = "&y";
		BackgroundString = "";
		HighestLayer = -1;
		WantsToPaint = false;
		Alt = false;
		CustomDraw = false;
		AsIfKnown = false;
		_Tile = null;
		RenderString = " ";
		DetailColor = null;
		Lit = LightLevel.Light;
		Visible = false;
		NoWake = false;
		HFlip = false;
		VFlip = false;
		UI = false;
		Context = null;
		Imposters.Clear();
		if (Builder.Length > 0)
		{
			Builder.Clear();
		}
		ColorStringPriority = 0;
		DetailColorPriority = 0;
		WantsBackdrop = null;
		BackdropBleedthrough = false;
		BackgroundStringPriority = 0;
	}

	public bool getHFlip()
	{
		return HFlip;
	}

	public bool getVFlip()
	{
		return VFlip;
	}

	public ColorChars getColorChars()
	{
		char foreground = 'y';
		char background = 'k';
		string colorString = getColorString();
		if (!string.IsNullOrEmpty(colorString))
		{
			int num = colorString.LastIndexOf(ColorChars.FOREGROUND_INDICATOR);
			int num2 = colorString.LastIndexOf(ColorChars.BACKGROUND_INDICATOR);
			if (num >= 0 && num < colorString.Length - 1)
			{
				foreground = colorString[num + 1];
			}
			if (num2 >= 0 && num2 < colorString.Length - 1)
			{
				background = colorString[num2 + 1];
			}
		}
		return new ColorChars
		{
			detail = getDetailColor(),
			foreground = foreground,
			background = background
		};
	}

	public Color GetForegroundColor()
	{
		if (ColorString != null)
		{
			int num = ColorString.LastIndexOf('&');
			if (num != -1)
			{
				return ConsoleLib.Console.ColorUtility.FastColorMap[(uint)ColorString[num + 1]];
			}
		}
		return The.Color.Gray;
	}

	public char GetForegroundColorChar()
	{
		if (ColorString != null && ColorString.Length > 1)
		{
			return ColorString[1];
		}
		return 'y';
	}

	public Color GetDetailColor()
	{
		if (DetailColor != null && DetailColor.Length > 0)
		{
			return ConsoleLib.Console.ColorUtility.FastColorMap[(uint)DetailColor[0]];
		}
		return GetForegroundColor();
	}

	public char GetDetailColorChar()
	{
		if (DetailColor != null && DetailColor.Length > 0)
		{
			return DetailColor[0];
		}
		return 'w';
	}

	public char GetBackgroundColorChar()
	{
		if (BackgroundString != null)
		{
			int num = BackgroundString.LastIndexOf('^');
			if (num != -1)
			{
				return BackgroundString[num + 1];
			}
		}
		if (ColorString != null)
		{
			int num2 = ColorString.LastIndexOf('^');
			if (num2 != -1)
			{
				return ColorString[num2 + 1];
			}
		}
		return 'k';
	}

	public Color GetBackgroundColor()
	{
		if (BackgroundString != null)
		{
			int num = BackgroundString.LastIndexOf('^');
			if (num != -1)
			{
				return ConsoleLib.Console.ColorUtility.FastColorMap[(uint)BackgroundString[num + 1]];
			}
		}
		if (ColorString != null)
		{
			int num2 = ColorString.LastIndexOf('^');
			if (num2 != -1)
			{
				return ConsoleLib.Console.ColorUtility.FastColorMap[(uint)ColorString[num2 + 1]];
			}
		}
		return ConsoleLib.Console.ColorUtility.FastColorMap[107];
	}

	string IRenderable.getTileColor()
	{
		return null;
	}

	public void TileVariantColors(string SingleColor, string TileForeground, string TileDetail)
	{
		if (!ColorsVisible)
		{
			return;
		}
		if (!Tile.IsNullOrEmpty())
		{
			if (!TileForeground.IsNullOrEmpty())
			{
				ColorString = TileForeground;
			}
			if (!TileForeground.IsNullOrEmpty())
			{
				DetailColor = TileDetail;
			}
		}
		else if (!SingleColor.IsNullOrEmpty())
		{
			ColorString = SingleColor;
		}
	}

	public void ApplyColors(string TextForeground, string TileForeground, string TileDetail, string Background, int TextForegroundPriority, int TileForegroundPriority, int TileDetailPriority, int BackgroundPriority)
	{
		if (!ColorsVisible)
		{
			return;
		}
		if (!Tile.IsNullOrEmpty())
		{
			if (TileForegroundPriority > ColorStringPriority && !TileForeground.IsNullOrEmpty())
			{
				ColorString = TileForeground;
				ColorStringPriority = TileForegroundPriority;
			}
			if (TileDetailPriority > DetailColorPriority && !TileDetail.IsNullOrEmpty())
			{
				DetailColor = TileDetail;
				DetailColorPriority = TileDetailPriority;
			}
		}
		else if (TextForegroundPriority > ColorStringPriority && !TextForeground.IsNullOrEmpty())
		{
			ColorString = TextForeground;
			ColorStringPriority = TextForegroundPriority;
		}
		if (BackgroundPriority > BackgroundStringPriority && !Background.IsNullOrEmpty())
		{
			BackgroundString = Background;
			BackgroundStringPriority = BackgroundPriority;
		}
	}

	public void ApplyColors(string Foreground, string Background, string Detail, int ForegroundPriority, int BackgroundPriority, int DetailPriority)
	{
		if (ColorsVisible)
		{
			if (ForegroundPriority > ColorStringPriority && !Foreground.IsNullOrEmpty())
			{
				ColorString = Foreground;
				ColorStringPriority = ForegroundPriority;
			}
			if (BackgroundPriority > BackgroundStringPriority && !Background.IsNullOrEmpty())
			{
				BackgroundString = Background;
				BackgroundStringPriority = BackgroundPriority;
			}
			if (!Tile.IsNullOrEmpty() && DetailPriority > DetailColorPriority && !Detail.IsNullOrEmpty())
			{
				DetailColor = Detail;
				DetailColorPriority = DetailPriority;
			}
		}
	}

	public void ApplyColors(string Foreground, string Detail, int ForegroundPriority, int DetailPriority)
	{
		if (ColorsVisible)
		{
			if (ForegroundPriority > ColorStringPriority && !Foreground.IsNullOrEmpty())
			{
				ColorString = Foreground;
				ColorStringPriority = ForegroundPriority;
			}
			if (!Tile.IsNullOrEmpty() && DetailPriority > DetailColorPriority && !Detail.IsNullOrEmpty())
			{
				DetailColor = Detail;
				DetailColorPriority = DetailPriority;
			}
		}
	}

	public void ApplyBackgroundColor(string BackgroundString, int BackgroundStringPriority)
	{
		if (ColorsVisible && BackgroundStringPriority > this.BackgroundStringPriority && !BackgroundString.IsNullOrEmpty())
		{
			this.BackgroundString = BackgroundString;
			this.BackgroundStringPriority = BackgroundStringPriority;
		}
	}

	public void ApplyDetailColor(string Detail, int DetailPriority)
	{
		if (ColorsVisible && DetailPriority > ColorStringPriority && !Detail.IsNullOrEmpty())
		{
			DetailColor = Detail;
			DetailColorPriority = DetailPriority;
		}
	}

	public void ApplyColors(string Foreground, int ForegroundPriority)
	{
		if (ColorsVisible && ForegroundPriority > ColorStringPriority && !Foreground.IsNullOrEmpty())
		{
			ColorString = Foreground;
			ColorStringPriority = ForegroundPriority;
		}
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

	public virtual void Dispose()
	{
		if (Builder != null)
		{
			Builder = null;
		}
	}
}
