using UnityEngine;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class AddPresetAtLocation : ZoneBuilderSandbox
{
	public string Preset;

	public int X;

	public int Y;

	public bool BuildZone(Zone Z)
	{
		string preset = Preset;
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(preset);
		if (gameObject == null)
		{
			Debug.LogError("Bad preset blueprint: " + preset);
		}
		if (gameObject.HasPart<MapChunkPlacement>())
		{
			_ = gameObject.GetPart<MapChunkPlacement>().Width;
			_ = gameObject.GetPart<MapChunkPlacement>().Height;
		}
		if (gameObject.HasPart<MultiMapChunkPlacement>())
		{
			MultiMapChunkPlacement part = gameObject.GetPart<MultiMapChunkPlacement>();
			_ = part.MapsWide;
			_ = part.Width;
			_ = part.MapsHigh;
			_ = part.Height;
		}
		Z.GetCell(X, Y).AddObject(gameObject);
		new ForceConnections().BuildZone(Z);
		return true;
	}
}
