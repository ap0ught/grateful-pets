using System;
using Genkit;

namespace XRL.World.Parts;

[Serializable]
public class RandomFungal : IPart
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
		SetupFungal();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneLoaded")
		{
			SetupFungal();
		}
		return base.FireEvent(E);
	}

	private void SetupFungal()
	{
		Render render = ParentObject.Render;
		if (ParentObject.Physics.CurrentCell.X > 0 && ParentObject.Physics.CurrentCell.X < 79 && ParentObject.Physics.CurrentCell.Y > 0 && ParentObject.Physics.CurrentCell.Y < 24)
		{
			bool flag = ParentObject.Physics.CurrentCell.X % 2 == 0;
			bool flag2 = GetSeededRange(ParentObject.Physics.CurrentCell.X / 2 + "," + ParentObject.Physics.CurrentCell.Y, 1, 4) == 1;
			string text = ((GetSeededRange(ParentObject.Physics.CurrentCell.Y + "," + ParentObject.Physics.CurrentCell.X / 2, 1, 3) == 1) ? "a" : "b");
			if (flag2)
			{
				Cell cellFromDirection = ParentObject.Physics.CurrentCell.GetCellFromDirection("E");
				Cell cellFromDirection2 = ParentObject.Physics.CurrentCell.GetCellFromDirection("W");
				if (cellFromDirection2 != null && cellFromDirection != null && ((flag && cellFromDirection.HasObjectWithPropertyOrTagEqualToValue("Terrain", "Fungal")) || (!flag && cellFromDirection2.HasObjectWithPropertyOrTagEqualToValue("Terrain", "Fungal"))))
				{
					if (flag)
					{
						render.Tile = "Terrain/sw_worldmap_rainbowwood_left_" + text + ".bmp";
					}
					else
					{
						render.Tile = "Terrain/sw_worldmap_rainbowwood_right_" + text + ".bmp";
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
