using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MotedAir : IPart
{
	private string render;

	private string color;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (!Options.UseTiles && !string.IsNullOrEmpty(render))
		{
			if (Stat.RandomCosmetic(1, 1000) == 1)
			{
				E.ColorString = "&" + Crayons.GetRandomColorAll() + "^k";
			}
			if (Stat.RandomCosmetic(1, 1000) == 1)
			{
				int num = Stat.RandomCosmetic(1, 100);
				if (num < 2)
				{
					E.RenderString = "ø";
				}
				else if (num < 5)
				{
					E.RenderString = "ù";
				}
				else if (num < 8)
				{
					E.RenderString = "\a";
				}
				else if (num < 40)
				{
					E.RenderString = "ú";
				}
				else if (num < 45)
				{
					E.RenderString = ".";
				}
				else
				{
					E.RenderString = " ";
				}
			}
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (!Options.UseTiles && render == null)
		{
			int num = Stat.RandomCosmetic(1, 100);
			color = Crayons.GetRandomColorAll();
			if (num < 2)
			{
				render = "ø";
			}
			else if (num < 5)
			{
				render = "ù";
			}
			else if (num < 8)
			{
				render = "\a";
			}
			else if (num < 40)
			{
				render = "ú";
			}
			else if (num < 45)
			{
				render = ".";
			}
			else
			{
				render = " ";
			}
			ParentObject.Render.RenderString = render;
			ParentObject.Render.ColorString = "&" + color + "^k";
		}
		return base.HandleEvent(E);
	}
}
