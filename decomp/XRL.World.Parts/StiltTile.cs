using System;

namespace XRL.World.Parts;

[Serializable]
public class StiltTile : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (ParentObject.CurrentCell == null)
			{
				return true;
			}
			Render render = ParentObject.Render;
			if ((ParentObject.CurrentCell.X + ParentObject.CurrentCell.Y) % 2 == 0)
			{
				render.ColorString = "&K";
				render.RenderString = "ù";
				render.Tile = "terrain/sw_cathedral1.bmp";
				ParentObject.Physics.CurrentCell.PaintTile = "terrain/sw_cathedral1.bmp";
				ParentObject.Physics.CurrentCell.PaintTileColor = "&y";
			}
			else
			{
				render.ColorString = "&K";
				render.RenderString = "ú";
				render.Tile = "terrain/sw_cathedral2.bmp";
				ParentObject.Physics.CurrentCell.PaintTile = "terrain/sw_cathedral2.bmp";
				ParentObject.Physics.CurrentCell.PaintTileColor = "&y";
			}
			ParentObject.RemovePart(this);
		}
		return base.FireEvent(E);
	}
}
