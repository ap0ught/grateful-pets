using System.Collections.Generic;

namespace XRL.World;

public class CellBlueprint
{
	public string Name;

	public string Inherits;

	public string ApplyTo;

	public string LandingZone;

	public string AmbientBed;

	public bool Mutable = true;

	public ZoneBlueprint[,,] LevelBlueprint = new ZoneBlueprint[Definitions.Width, Definitions.Height, Definitions.Layers];

	public Dictionary<string, object> Properties;

	public void CopyFrom(CellBlueprint ParentBlueprint)
	{
		AmbientBed = ParentBlueprint.AmbientBed;
		for (int i = 0; i < Definitions.Height; i++)
		{
			for (int j = 0; j < Definitions.Width; j++)
			{
				for (int k = 0; k < Definitions.Layers; k++)
				{
					LevelBlueprint[j, i, k] = ParentBlueprint.LevelBlueprint[j, i, k];
				}
			}
		}
	}

	public void SetProperty(string Key, object Value)
	{
		if (Properties == null)
		{
			Properties = new Dictionary<string, object>();
		}
		Properties[Key] = Value;
	}
}
