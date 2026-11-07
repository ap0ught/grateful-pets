using System;

namespace XRL.World.Parts;

[Serializable]
public class ColorShift : IPart
{
	public string ColorString;

	public string TileColor;

	public string DetailColor;

	public override bool SameAs(IPart p)
	{
		ColorShift colorShift = p as ColorShift;
		if (colorShift.ColorString != ColorString)
		{
			return false;
		}
		if (colorShift.TileColor != TileColor)
		{
			return false;
		}
		if (colorShift.DetailColor != DetailColor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public void Apply(string ColorString = null, string TileColor = null, string DetailColor = null)
	{
		if (ColorString != null && ParentObject.Render != null)
		{
			if (this.ColorString == null)
			{
				this.ColorString = ParentObject.Render.ColorString;
			}
			ParentObject.Render.ColorString = ColorString;
		}
		if (TileColor != null && ParentObject.Render != null)
		{
			if (this.TileColor == null)
			{
				this.TileColor = ParentObject.Render.TileColor;
			}
			ParentObject.Render.TileColor = TileColor;
		}
		if (DetailColor != null && ParentObject.Render != null)
		{
			if (this.DetailColor == null)
			{
				this.DetailColor = ParentObject.Render.DetailColor;
			}
			ParentObject.Render.DetailColor = DetailColor;
		}
	}

	public void Unapply()
	{
		if (ColorString != null && ParentObject.Render != null)
		{
			ParentObject.Render.ColorString = ColorString;
		}
		if (TileColor != null && ParentObject.Render != null)
		{
			ParentObject.Render.TileColor = TileColor;
		}
		if (DetailColor != null && ParentObject.Render != null)
		{
			ParentObject.Render.DetailColor = DetailColor;
		}
		ParentObject.RemovePart(this);
	}
}
