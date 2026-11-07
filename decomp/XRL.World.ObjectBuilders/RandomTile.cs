using System;
using System.Collections.Generic;

namespace XRL.World.ObjectBuilders;

[Serializable]
public class RandomTile : IObjectBuilder
{
	public string Tiles;

	public override void Initialize()
	{
		Tiles = "";
	}

	public override void Apply(GameObject Object, string Context)
	{
		List<string> list = Tiles.CachedCommaExpansion();
		if (list.Count > 0)
		{
			Object.Render.Tile = list.GetRandomElement();
		}
	}
}
