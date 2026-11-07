using System;
using Genkit;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class RandomJungle : IPart
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
		SetupJungle();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneLoaded")
		{
			SetupJungle();
		}
		return base.FireEvent(E);
	}

	private void SetupJungle()
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
		if (num == 1)
		{
			render.RenderString = "\u0005";
		}
		else if (num < 5)
		{
			render.RenderString = "\u009d";
		}
		else if (num < 8)
		{
			render.RenderString = "ç";
		}
		else if (num < 11)
		{
			render.RenderString = "æ";
		}
		string text = "";
		int num2 = Stat.Random(1, 3);
		if (num2 == 1)
		{
			text = "a";
		}
		if (num2 == 2)
		{
			text = "b";
		}
		if (num2 == 3)
		{
			text = "c";
		}
		render.Tile = "terrain/tile_jungle" + Stat.Random(1, 3) + text + ".png";
		render.DetailColor = "G";
		if (ParentObject.Physics.CurrentCell.X > 0 && ParentObject.Physics.CurrentCell.X < 79 && ParentObject.Physics.CurrentCell.Y > 0 && ParentObject.Physics.CurrentCell.Y < 24)
		{
			bool flag = ParentObject.Physics.CurrentCell.X % 2 == 0;
			bool flag2 = GetSeededRange(ParentObject.Physics.CurrentCell.X / 2 + "," + ParentObject.Physics.CurrentCell.Y, 1, 4) == 1;
			int seededRange = GetSeededRange(ParentObject.Physics.CurrentCell.Y + "," + ParentObject.Physics.CurrentCell.X / 2, 1, 3);
			if (flag2)
			{
				Cell cellFromDirection = ParentObject.Physics.CurrentCell.GetCellFromDirection("E");
				Cell cellFromDirection2 = ParentObject.Physics.CurrentCell.GetCellFromDirection("W");
				if (cellFromDirection2 != null && cellFromDirection != null && ((flag && cellFromDirection.HasObjectWithBlueprint("TerrainJungle")) || (!flag && cellFromDirection2.HasObjectWithBlueprint("TerrainJungle"))))
				{
					if (flag)
					{
						render.Tile = "terrain/tile_jungleleft" + seededRange + text + ".png";
					}
					else
					{
						render.Tile = "terrain/tile_jungleright" + seededRange + text + ".png";
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
