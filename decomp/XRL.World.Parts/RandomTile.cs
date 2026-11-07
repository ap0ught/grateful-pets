using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class RandomTile : IPart
{
	public string Tiles = "";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectCreatedEvent.ID)
		{
			return ID == SingletonEvent<RefreshTileEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		RandomizeTile();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RefreshTileEvent E)
	{
		RandomizeTile();
		return base.HandleEvent(E);
	}

	public void RandomizeTile()
	{
		List<string> list = Tiles.CachedCommaExpansion();
		if (list.Count > 0)
		{
			ParentObject.Render.Tile = list.GetRandomElement();
		}
	}

	public bool IsTileInRandomSet(string Tile)
	{
		return Tiles.CachedCommaExpansion().Contains(Tile);
	}
}
