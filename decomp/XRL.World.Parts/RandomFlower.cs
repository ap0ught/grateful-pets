using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class RandomFlower : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectCreated");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectCreated")
		{
			Render render = ParentObject.Render;
			render.Tile = "terrain/tile_flowers" + Stat.Random(1, 2) + ".bmp";
			switch (Stat.Random(1, 7))
			{
			case 1:
				render.ColorString = "&R";
				break;
			case 2:
				render.ColorString = "&M";
				break;
			case 3:
				render.ColorString = "&B";
				break;
			case 4:
				render.ColorString = "&C";
				break;
			case 5:
				render.ColorString = "&Y";
				break;
			case 6:
				render.ColorString = "&G";
				break;
			case 7:
				render.ColorString = "&W";
				break;
			}
			if (Stat.Random(0, 1) == 0)
			{
				render.ColorString = render.ColorString.ToLower();
			}
			int num = Stat.Random(1, 5);
			if (num == 1)
			{
				render.RenderString = ",";
			}
			if (num == 2)
			{
				render.RenderString = ".";
			}
			if (num == 3)
			{
				render.RenderString = "ù";
			}
			if (num == 4)
			{
				render.RenderString = "ú";
			}
			ParentObject.RemovePart(this);
		}
		return base.FireEvent(E);
	}
}
