using System;

namespace XRL.World.Parts;

[Serializable]
public class WallColor : IPart
{
	public bool AsBackground;

	public int ColorPriority = 100;

	public override bool Render(RenderEvent E)
	{
		GameObject gameObject = ParentObject?.CurrentCell?.GetFirstWall();
		if (gameObject != null)
		{
			string foregroundColor = gameObject.Render.GetForegroundColor();
			if (AsBackground)
			{
				E.ApplyColors("&" + VaryColor(E.GetForegroundColorChar().ToString(), foregroundColor), "^" + foregroundColor, VaryColor(E.GetDetailColorChar().ToString(), foregroundColor), ColorPriority, ColorPriority, ColorPriority);
			}
			else
			{
				E.ApplyColors("&" + foregroundColor, VaryColor(E.GetDetailColorChar().ToString(), foregroundColor), ColorPriority, ColorPriority);
			}
		}
		return base.Render(E);
	}

	public string VaryColor(string Color, string RelativeTo)
	{
		if (Color == RelativeTo)
		{
			if (Color[0] >= 'A' && Color[0] <= 'Z')
			{
				return Color.ToLower();
			}
			return Color.ToUpper();
		}
		return Color;
	}
}
