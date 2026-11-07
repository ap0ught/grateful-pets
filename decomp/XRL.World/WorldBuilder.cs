using System;
using XRL.Core;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public abstract class WorldBuilder
{
	public ZoneManager ZM;

	public XRLGame game => XRLCore.Core.Game;

	public abstract bool BuildWorld(string worldName);

	public void MarkCell(string World, int x, int y, string str)
	{
		GameObject gameObject = ((The.ZoneManager?.GetZone(World))?.GetCell(x, y) ?? null)?.GetObjectInCell(0);
		if (gameObject != null && gameObject.TryGetPart<Render>(out var Part))
		{
			Part.RenderString = str;
			Part.Tile = null;
		}
	}
}
