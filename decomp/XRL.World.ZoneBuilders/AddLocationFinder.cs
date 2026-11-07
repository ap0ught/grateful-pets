using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class AddLocationFinder
{
	public string SecretID;

	public int Value;

	public bool BuildZone(Zone Z)
	{
		GameObject gameObject = GameObject.Create("LocationFinder");
		if (gameObject.TryGetPart<LocationFinder>(out var Part))
		{
			Part.ID = SecretID;
			Part.Value = Value;
			Z.GetCell(0, 0)?.AddObject(gameObject);
		}
		return true;
	}
}
