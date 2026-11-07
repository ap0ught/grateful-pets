using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialWater : IPart
{
	public int nFrameOffset;

	public bool Rushing;

	public bool Fresh;

	public bool Acid;

	public bool Bloody;

	public AnimatedMaterialWater()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
		Rushing = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (ParentObject.Physics.IsFreezing())
		{
			E.RenderString = "~";
			if (E.ColorsVisible)
			{
				E.ColorString = "&c";
				E.DetailColor = "C";
			}
		}
		else if (Acid)
		{
			Render render = ParentObject.Render;
			int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (ParentObject.Physics.CurrentCell.ParentZone.Z == 10 && Stat.RandomCosmetic(1, 600) == 1)
			{
				E.RenderString = "÷";
				E.TileVariantColors("&Y^g", "&Y", "g");
			}
			if (Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num < 15)
				{
					render.RenderString = "÷";
					if (E.ColorsVisible)
					{
						render.ColorString = "&g^G";
						render.TileColor = "&g";
						render.DetailColor = "G";
					}
				}
				else if (num < 30)
				{
					render.RenderString = " ";
					if (E.ColorsVisible)
					{
						render.ColorString = "&Y^g";
						render.TileColor = "&Y";
						render.DetailColor = "g";
					}
				}
				else if (num < 45)
				{
					render.RenderString = "÷";
					if (E.ColorsVisible)
					{
						render.ColorString = "&g^G";
						render.TileColor = "&g";
						render.DetailColor = "G";
					}
				}
				else
				{
					render.RenderString = "~";
					if (E.ColorsVisible)
					{
						render.ColorString = "&y^G";
						render.TileColor = "&y";
						render.DetailColor = "G";
					}
				}
			}
		}
		else if (Bloody)
		{
			Render render2 = ParentObject.Render;
			int num2 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (ParentObject.Physics.CurrentCell.ParentZone.Z == 10 && Stat.RandomCosmetic(1, 600) == 1)
			{
				E.RenderString = "÷";
				E.TileVariantColors("&Y^r", "&Y", "r");
			}
			if (Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num2 < 15)
				{
					render2.RenderString = "÷";
					render2.ColorString = "&b^R";
					render2.TileColor = "&b";
					render2.DetailColor = "R";
				}
				else if (num2 < 30)
				{
					render2.RenderString = " ";
					render2.ColorString = "&Y^R";
					render2.TileColor = "&Y";
					render2.DetailColor = "R";
				}
				else if (num2 < 45)
				{
					render2.RenderString = "÷";
					render2.ColorString = "&b^R";
					render2.TileColor = "&b";
					render2.DetailColor = "R";
				}
				else
				{
					render2.RenderString = "~";
					render2.ColorString = "&y^r";
					render2.TileColor = "&y";
					render2.DetailColor = "r";
				}
			}
		}
		else if (Fresh)
		{
			Render render3 = ParentObject.Render;
			int num3 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (ParentObject.Physics.CurrentCell.ParentZone.Z == 10 && Stat.RandomCosmetic(1, 600) == 1)
			{
				E.RenderString = "÷";
				E.TileVariantColors("&Y^B", "&Y", "B");
			}
			if (Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num3 < 15)
				{
					render3.RenderString = "÷";
					render3.ColorString = "&b^B";
					render3.TileColor = "&b";
					render3.DetailColor = "B";
				}
				else if (num3 < 30)
				{
					render3.RenderString = " ";
					render3.ColorString = "&Y^B";
					render3.TileColor = "&Y";
					render3.DetailColor = "B";
				}
				else if (num3 < 45)
				{
					render3.RenderString = "÷";
					render3.ColorString = "&b^B";
					render3.TileColor = "&b";
					render3.DetailColor = "B";
				}
				else
				{
					render3.RenderString = "~";
					render3.ColorString = "&y^B";
					render3.TileColor = "&y";
					render3.DetailColor = "B";
				}
			}
		}
		else if (Rushing)
		{
			Render render4 = ParentObject.Render;
			int num4 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (num4 < 15)
			{
				E.RenderString = "~";
				E.TileVariantColors("&B^b", "&B", "b");
			}
			else if (num4 < 30)
			{
				E.RenderString = render4.RenderString;
				E.TileVariantColors("&Y^b", "&Y", "b");
			}
			else if (num4 < 45)
			{
				E.RenderString = "~";
				E.TileVariantColors("&B^b", "&B", "b");
			}
			else
			{
				E.RenderString = render4.RenderString;
				E.TileVariantColors("&B^b", "&B", "b");
			}
		}
		else
		{
			Render render5 = ParentObject.Render;
			int num5 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (ParentObject.Physics.CurrentCell.ParentZone.Z == 10 && Stat.RandomCosmetic(1, 600) == 1)
			{
				E.RenderString = "~";
				E.TileVariantColors("&Y^b", "&Y", "b");
			}
			if (Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num5 < 15)
				{
					render5.RenderString = "÷";
					render5.ColorString = "&B^b";
					render5.TileColor = "&B";
					render5.DetailColor = "b";
				}
				else if (num5 < 30)
				{
					render5.RenderString = "~";
					render5.ColorString = "&B^b";
					render5.TileColor = "&B";
					render5.DetailColor = "b";
				}
				else if (num5 < 45)
				{
					render5.RenderString = " ";
					render5.ColorString = "&B^b";
					render5.TileColor = "&B";
					render5.DetailColor = "b";
				}
				else
				{
					render5.RenderString = "~";
					render5.ColorString = "&B^b";
					render5.TileColor = "&B";
					render5.DetailColor = "b";
				}
			}
		}
		return base.Render(E);
	}
}
