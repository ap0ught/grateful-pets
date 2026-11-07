using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class RandomRenderString : IPart
{
	public string Strings = "";

	public override bool SameAs(IPart p)
	{
		return true;
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
		RandomizeTile();
		return base.HandleEvent(E);
	}

	public void RandomizeTile()
	{
		List<string> list = Strings.CachedCommaExpansion();
		if (list.Count > 0)
		{
			ParentObject.Render.RenderString = ((char)int.Parse(list.GetRandomElement())).ToString();
		}
	}

	public bool IsTileInRandomSet(string Tile)
	{
		return Strings.CachedCommaExpansion().Contains(Tile);
	}
}
