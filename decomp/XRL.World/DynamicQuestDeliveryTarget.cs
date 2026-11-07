using System;
using System.Collections.Generic;
using Genkit;

namespace XRL.World;

[Serializable]
public class DynamicQuestDeliveryTarget : IComposite
{
	public string type;

	public string zoneId;

	[NonSerialized]
	public Location2D location;

	public string displayName;

	public string secretId;

	public List<string> attributes;

	public List<string> factions;

	public void Write(SerializationWriter Writer)
	{
		Writer.Write(location);
	}

	public void Read(SerializationReader Reader)
	{
		location = Reader.ReadLocation2D();
	}
}
