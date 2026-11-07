using System;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialForcefield : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 50;

	public int FrameOffset;

	public bool Rushing;

	public string Color = "Normal";

	[NonSerialized]
	private StringBuilder tileBuilder = new StringBuilder();

	[NonSerialized]
	private int lastN = 1;

	[NonSerialized]
	private long accumulator;

	public AnimatedMaterialForcefield()
	{
		FrameOffset = Stat.RandomCosmetic(0, 60);
		Rushing = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (!Visible() || !E.ColorsVisible)
		{
			return true;
		}
		string text = null;
		string text2 = null;
		string text3 = null;
		if (E.Tile == null)
		{
			string text4 = "^c";
			string text5 = "^C";
			if (Color == "Red")
			{
				text4 = "^r";
				text5 = "^R";
			}
			if (Color == "Magenta")
			{
				text4 = "^m";
				text5 = "^M";
			}
			if (Color == "Blue")
			{
				text4 = "^b";
				text5 = "^B";
			}
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
					text = "&r";
					text2 = text4;
				}
				else
				{
					text = "&R";
					text2 = text5;
				}
			}
			else if ((XRLCore.CurrentFrame + FrameOffset) % 60 < 45)
			{
				text = "&r";
				text2 = text4;
			}
			else
			{
				text = "&R";
				text2 = text5;
			}
		}
		else
		{
			string text6 = "^k";
			string text7 = "^K";
			string text8 = "^c";
			string text9 = "^C";
			string text10 = "&c";
			string text11 = "&C";
			if (Color == "Red")
			{
				text6 = "^k";
				text7 = "^r";
				text8 = "^r";
				text9 = "^r";
				text10 = "&r";
				text11 = "&R";
			}
			if (Color == "Magenta")
			{
				text6 = "^k";
				text7 = "^m";
				text8 = "^m";
				text9 = "^m";
				text10 = "&m";
				text11 = "&M";
			}
			if (Color == "Blue")
			{
				text6 = "^k";
				text7 = "^K";
				text8 = "^b";
				text9 = "^B";
				text10 = "&b";
				text11 = "&B";
			}
			if (Rushing)
			{
				int num2 = (XRLCore.CurrentFrame + FrameOffset) % 60;
				if (!Options.DisableTextAnimationEffects)
				{
					FrameOffset += 3;
				}
				if (Stat.RandomCosmetic(1, 120) == 1)
				{
					Rushing = false;
				}
				if (num2 < 15)
				{
					text = text11;
					text2 = text6;
					text3 = text6[1].ToString();
				}
				else if (num2 < 30)
				{
					text = text10;
					text2 = text6;
					text3 = text7[1].ToString();
				}
				else if (num2 < 45)
				{
					text = text11;
					text2 = text6;
					text3 = text8[1].ToString();
				}
				else
				{
					text = text10;
					text2 = text6;
					text3 = text9[1].ToString();
				}
			}
			else
			{
				int num3 = (XRLCore.CurrentFrame + FrameOffset) % 60;
				if (num3 < 15)
				{
					text = text11;
					text2 = text6;
					text3 = text6[1].ToString();
				}
				else if (num3 < 30)
				{
					text = text10;
					text2 = text6;
					text3 = text6[1].ToString();
				}
				else if (num3 < 45)
				{
					text = text11;
					text2 = text6;
					text3 = text8[1].ToString();
				}
				else
				{
					text = text10;
					text2 = text6;
					text3 = text9[1].ToString();
				}
			}
		}
		if (!text.IsNullOrEmpty() || !text2.IsNullOrEmpty() || !text3.IsNullOrEmpty())
		{
			E.ApplyColors(text, text2, text3, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY);
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (E.Tile != null)
		{
			if (XRLCore.FrameTimer.ElapsedMilliseconds - accumulator > 500)
			{
				string value = E.Tile.Substring(E.Tile.LastIndexOf('_') + 1);
				accumulator = XRLCore.FrameTimer.ElapsedMilliseconds;
				if (++lastN > 4)
				{
					lastN = 1;
				}
				tileBuilder.Length = 0;
				tileBuilder.Append("Assets_Content_Textures_Tiles2_force_field_").Append(lastN).Append('_')
					.Append(value);
				ParentObject.Render.Tile = tileBuilder.ToString();
			}
		}
		else
		{
			int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
			if (!Options.DisableTextAnimationEffects)
			{
				FrameOffset += 3;
			}
			if (num < 45)
			{
				E.RenderString = "°";
			}
			else
			{
				E.RenderString = ParentObject.Render.RenderString;
			}
		}
		return true;
	}
}
