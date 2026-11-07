using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialStasisfield : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 50;

	public int FrameOffset;

	public bool Rushing;

	public AnimatedMaterialStasisfield()
	{
		FrameOffset = Stat.RandomCosmetic(0, 60);
		Rushing = true;
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (Visible() && E.ColorsVisible)
		{
			string text = null;
			string text2 = null;
			if (Rushing)
			{
				int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
				if (!Options.DisableTextAnimationEffects)
				{
					FrameOffset += 3;
				}
				if (Stat.RandomCosmetic(1, 120) == 1)
				{
					Rushing = false;
				}
				if (num < 45)
				{
					text = "^m";
					text2 = "m";
				}
				else
				{
					text = "^C";
					text2 = "C";
				}
			}
			else if ((XRLCore.CurrentFrame + FrameOffset) % 60 < 45)
			{
				text = "^m";
				text2 = "m";
			}
			else
			{
				text = "^C";
				text2 = "C";
			}
			if (!text.IsNullOrEmpty() || !text2.IsNullOrEmpty())
			{
				E.ApplyColors(null, text, text2, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY);
			}
		}
		return base.FinalRender(E);
	}

	public override bool Render(RenderEvent E)
	{
		string text = null;
		string text2 = null;
		if (Rushing)
		{
			int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
			if (!Options.DisableTextAnimationEffects)
			{
				FrameOffset += 3;
			}
			if (Stat.RandomCosmetic(1, 120) == 1)
			{
				Rushing = false;
			}
			if (num < 15)
			{
				E.RenderString = "°";
				text = "&C";
				text2 = "k";
			}
			else if (num < 30)
			{
				E.RenderString = "°";
				text = "&c";
				text2 = "m";
			}
			else if (num < 45)
			{
				E.RenderString = "°";
				text = "&M";
				text2 = "c";
			}
			else
			{
				text = "&m";
				text2 = "C";
			}
		}
		else
		{
			int num2 = (XRLCore.CurrentFrame + FrameOffset) % 60;
			if (num2 < 15)
			{
				E.RenderString = "°";
				text = "&C";
				text2 = "k";
			}
			else if (num2 < 30)
			{
				E.RenderString = "°";
				text = "&c";
				text2 = "M";
			}
			else if (num2 < 45)
			{
				E.RenderString = "°";
				text = "&C";
				text2 = "m";
			}
			else
			{
				text = "&m";
				text2 = "C";
			}
		}
		if (!text.IsNullOrEmpty() || !text2.IsNullOrEmpty())
		{
			E.ApplyColors(text, text2, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY);
		}
		return base.Render(E);
	}
}
