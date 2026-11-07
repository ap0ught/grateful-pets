using System;
using System.Collections.Generic;
using System.Linq;

namespace XRL.World;

public class ZoneBlueprint
{
	public string Name;

	public string Level;

	public int Tier;

	public string x;

	public string y;

	public string GroundLiquid;

	public bool disableForcedConnections;

	public bool ProperName;

	public string NameContext;

	public string IndefiniteArticle;

	public string DefiniteArticle;

	public string AmbientBed;

	public string AmbientSounds;

	public int AmbientVolume = -1;

	public bool IncludeContextInZoneDisplay = true;

	public bool IncludeStratumInZoneDisplay = true;

	public bool HasWeather;

	public bool HasBiomes = true;

	public string WindSpeed;

	public string WindDirections;

	public string WindDuration;

	public CellBlueprint Cell;

	public ZoneBuilderCollection Builders;

	public ZonePartCollection Parts;

	public Dictionary<string, object> Properties;

	public ZoneBlueprint(ZoneBlueprint Parent)
	{
		if (Parent != null)
		{
			Cell = Parent.Cell;
			Level = "-";
			Tier = Parent.Tier;
			AmbientBed = Parent.AmbientBed;
			AmbientSounds = Parent.AmbientSounds;
			AmbientVolume = Parent.AmbientVolume;
			Builders = new ZoneBuilderCollection(Parent.Builders);
			Parts = ((Parent.Parts == null) ? null : new ZonePartCollection(Parent.Parts));
			x = "-";
			y = "-";
			if (!Parent.Name.IsNullOrEmpty())
			{
				Name = Parent.Name;
			}
		}
		else
		{
			Builders = new ZoneBuilderCollection();
		}
	}

	public bool AnyBuilder(Func<ZoneBuilderBlueprint, bool> p)
	{
		return Builders.Any(p);
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
