using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class RandomWeald : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		switch (Stat.Random(1, 4))
		{
		case 1:
			ParentObject.Render.ColorString = "&w";
			break;
		case 2:
			ParentObject.Render.ColorString = "&W";
			break;
		case 3:
			ParentObject.Render.ColorString = "&G";
			break;
		case 4:
			ParentObject.Render.ColorString = "&g";
			break;
		}
		if (Stat.Random(0, 1) == 0)
		{
			ParentObject.Render.ColorString = ParentObject.Render.ColorString.ToLower();
		}
		int num = Stat.Random(1, 10);
		if (num == 1)
		{
			ParentObject.Render.RenderString = ",";
		}
		if (num == 2)
		{
			ParentObject.Render.RenderString = ".";
		}
		if (num >= 3 && num <= 6)
		{
			ParentObject.Render.RenderString = "õ";
		}
		if (num > 6 && num <= 9)
		{
			ParentObject.Render.RenderString = "\u009d";
		}
		if (num == 10)
		{
			ParentObject.Render.RenderString = "\u009f";
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}
