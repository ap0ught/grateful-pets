using System;

namespace XRL.World.Parts;

[Serializable]
public abstract class IIconColorPart : IPart
{
	public string TextForeground;

	public string TileForeground;

	public string TileDetail;

	public string Background;

	public int TextForegroundPriority;

	public int TileForegroundPriority;

	public int TileDetailPriority;

	public int BackgroundPriority;

	public override bool SameAs(IPart p)
	{
		IIconColorPart iconColorPart = p as IIconColorPart;
		if (iconColorPart.TextForeground != TextForeground)
		{
			return false;
		}
		if (iconColorPart.TileForeground != TileForeground)
		{
			return false;
		}
		if (iconColorPart.TileDetail != TileDetail)
		{
			return false;
		}
		if (iconColorPart.Background != Background)
		{
			return false;
		}
		if (iconColorPart.TextForegroundPriority != TextForegroundPriority)
		{
			return false;
		}
		if (iconColorPart.TileForegroundPriority != TileForegroundPriority)
		{
			return false;
		}
		if (iconColorPart.TileDetailPriority != TileDetailPriority)
		{
			return false;
		}
		if (iconColorPart.BackgroundPriority != BackgroundPriority)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<GetDebugInternalsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "TextForeground", TextForeground);
		E.AddEntry(this, "TileForeground", TileForeground);
		E.AddEntry(this, "TileDetail", TileDetail);
		E.AddEntry(this, "Background", Background);
		E.AddEntry(this, "TextForegroundPriority", TextForegroundPriority);
		E.AddEntry(this, "TileForegroundPriority", TileForegroundPriority);
		E.AddEntry(this, "TileDetailPriority", TileDetailPriority);
		E.AddEntry(this, "BackgroundPriority", BackgroundPriority);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		E.ApplyColors(TextForeground, TileForeground, TileDetail, Background, TextForegroundPriority, TileForegroundPriority, TileDetailPriority, BackgroundPriority);
		return base.Render(E);
	}

	public void Configure(string TextForeground = null, string TileForeground = null, string TileDetail = null, string Background = null, string Foreground = null, int? TextForegroundPriority = null, int? TileForegroundPriority = null, int? TileDetailPriority = null, int? BackgroundPriority = null, int? Priority = null)
	{
		this.TextForeground = TextForeground ?? Foreground ?? this.TextForeground;
		this.TileForeground = TileForeground ?? Foreground ?? this.TileForeground;
		this.TileDetail = TileDetail ?? this.TileDetail;
		this.Background = Background ?? this.Background;
		this.TextForegroundPriority = TextForegroundPriority ?? Priority ?? this.TextForegroundPriority;
		this.TileForegroundPriority = TileForegroundPriority ?? Priority ?? this.TileForegroundPriority;
		this.TileDetailPriority = TileDetailPriority ?? Priority ?? this.TileDetailPriority;
		this.BackgroundPriority = BackgroundPriority ?? Priority ?? this.BackgroundPriority;
	}
}
