using System;
using Genkit;
using Qud.API;

namespace XRL.World.WorldBuilders;

[Serializable]
public class GeneratedLocationInfo : IComposite
{
	public string name;

	public string ownerID;

	public string secretID;

	public string attribute;

	[NonSerialized]
	public Location2D zoneLocation;

	public string targetZone;

	public int distanceTo(Location2D location)
	{
		return zoneLocation.Distance(location);
	}

	public bool isUndiscovered()
	{
		if (string.IsNullOrEmpty(secretID))
		{
			return false;
		}
		return !JournalAPI.IsMapOrVillageNoteRevealed(secretID);
	}

	public void Write(SerializationWriter Writer)
	{
		Writer.Write(zoneLocation);
	}

	public void Read(SerializationReader Reader)
	{
		zoneLocation = Reader.ReadLocation2D();
	}
}
