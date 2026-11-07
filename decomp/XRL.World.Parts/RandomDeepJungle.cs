using System;
using Genkit;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class RandomDeepJungle : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		SetupDeepJungle();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneLoaded")
		{
			SetupDeepJungle();
		}
		return base.FireEvent(E);
	}

	private void SetupDeepJungle()
	{
		Render render = ParentObject.Render;
		if (Stat.Random(1, 10) == 1)
		{
			render.ColorString = "&G";
		}
		else
		{
			render.ColorString = "&g";
		}
		int num = Stat.Random(1, 10);
		if (num < 3)
		{
			render.RenderString = "á";
		}
		else if (num < 6)
		{
			render.RenderString = "\u009d";
		}
		else if (num < 9)
		{
			render.RenderString = "ç";
		}
		else if (num < 11)
		{
			render.RenderString = "æ";
		}
		render.Tile = "Terrain/sw_deepjungle_" + Stat.Random(1, 4) + ".bmp";
		render.DetailColor = (50.in100() ? "G" : "g");
		if (ParentObject.Physics.CurrentCell.X > 0 && ParentObject.Physics.CurrentCell.X < 79 && ParentObject.Physics.CurrentCell.Y > 0 && ParentObject.Physics.CurrentCell.Y < 24)
		{
			bool flag = ParentObject.Physics.CurrentCell.X % 2 == 0;
			bool flag2 = GetSeededRange(ParentObject.Physics.CurrentCell.X / 2 + "," + ParentObject.Physics.CurrentCell.Y, 1, 4) == 1;
			string text = ((GetSeededRange(ParentObject.Physics.CurrentCell.Y + "," + ParentObject.Physics.CurrentCell.X / 2, 1, 3) == 1) ? "a" : "b");
			if (flag2)
			{
				Cell cellFromDirection = ParentObject.Physics.CurrentCell.GetCellFromDirection("E");
				Cell cellFromDirection2 = ParentObject.Physics.CurrentCell.GetCellFromDirection("W");
				if (cellFromDirection2 != null && cellFromDirection != null && ((flag && cellFromDirection.HasObjectWithBlueprint("TerrainDeepJungle")) || (!flag && cellFromDirection2.HasObjectWithBlueprint("TerrainDeepJungle"))))
				{
					if (flag)
					{
						render.Tile = "Terrain/sw_deepjungle_" + text + "_left.bmp";
					}
					else
					{
						render.Tile = "Terrain/sw_deepjungle_" + text + "_right.bmp";
					}
				}
			}
		}
		ParentObject.RemovePart(this);
	}

	public static int GetSeededRange(string Seed, int Low, int High)
	{
		return new Random(Hash.String(Seed)).Next(Low, High);
	}
}
