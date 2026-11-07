using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace XRL.World;

[Serializable]
public class WorldBlueprint
{
	public string DisplayName;

	public string Name;

	public string Map;

	public string AmbientBed;

	public string ZoneFactory;

	public string ZoneFactoryRegex;

	public string Plane;

	public string Protocol;

	public string CustomClock;

	public int? PsychicHunterChance;

	[NonSerialized]
	private IZoneFactory _ZoneFactoryInstance;

	[NonSerialized]
	private Regex Compiled;

	[NonSerialized]
	private List<ZoneBlueprint> _Blueprints = new List<ZoneBlueprint>();

	public List<ZoneBuilderBlueprint> Builders = new List<ZoneBuilderBlueprint>();

	public Dictionary<string, CellBlueprint> CellBlueprintsByApplication = new Dictionary<string, CellBlueprint>();

	public Dictionary<string, CellBlueprint> CellBlueprintsByName = new Dictionary<string, CellBlueprint>();

	public IZoneFactory ZoneFactoryInstance
	{
		get
		{
			if (_ZoneFactoryInstance == null && !ZoneFactory.IsNullOrEmpty())
			{
				Type type = ModManager.ResolveType("XRL.World.ZoneFactories." + ZoneFactory);
				_ZoneFactoryInstance = Activator.CreateInstance(type) as IZoneFactory;
				if (_ZoneFactoryInstance != null)
				{
					_ZoneFactoryInstance.Blueprint = this;
					_ZoneFactoryInstance.Initialize();
				}
			}
			return _ZoneFactoryInstance;
		}
	}

	public bool IsMatch(string ZoneID)
	{
		if (ZoneID.IsNullOrEmpty())
		{
			return false;
		}
		if (ZoneFactoryRegex.IsNullOrEmpty())
		{
			return false;
		}
		try
		{
			if (Compiled == null)
			{
				Compiled = new Regex(ZoneFactoryRegex, RegexOptions.Compiled);
			}
			return Compiled.IsMatch(ZoneID);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("World::ZoneFactoryRegex", x);
			return false;
		}
	}

	public ZoneBlueprint GetBlueprintFor(ZoneRequest Request)
	{
		if (Request.Blueprints == null)
		{
			_Blueprints.Clear();
			Request.Blueprints = _Blueprints;
		}
		ZoneFactoryInstance?.AddBlueprintsFor(Request);
		return Request.Blueprints.FirstOrDefault();
	}

	public List<ZoneBlueprint> GetBlueprintsFor(ZoneRequest Request)
	{
		ref List<ZoneBlueprint> blueprints = ref Request.Blueprints;
		if (blueprints == null)
		{
			blueprints = new List<ZoneBlueprint>();
		}
		ZoneFactoryInstance?.AddBlueprintsFor(Request);
		return Request.Blueprints;
	}

	public Zone GenerateZone(ZoneRequest Request, int Width, int Height)
	{
		return ZoneFactoryInstance?.GenerateZone(Request, Width, Height) ?? new Zone(Width, Height);
	}
}
