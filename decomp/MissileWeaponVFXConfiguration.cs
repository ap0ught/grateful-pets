using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using UnityEngine;
using XRL.World;
using XRL.World.Parts;

public class MissileWeaponVFXConfiguration
{
	public class MissileVFXPathDefinition
	{
		public string projectileVFX;

		public Dictionary<string, string> projectileVFXConfiguration = new Dictionary<string, string>();

		public bool used;

		public Location2D first;

		public Location2D last;

		public List<Location2D> steps = new List<Location2D>(105);

		public string getConfigValue(string key, string defaultResult = null)
		{
			if (projectileVFXConfiguration == null)
			{
				return defaultResult;
			}
			if (projectileVFXConfiguration.TryGetValue(key, out var value))
			{
				return value;
			}
			return defaultResult;
		}

		public bool TryGetValue(string Key, out string Result)
		{
			if (projectileVFXConfiguration == null)
			{
				Result = null;
				return false;
			}
			return projectileVFXConfiguration.TryGetValue(Key, out Result);
		}

		public void addStep(Location2D step)
		{
			used = true;
			if (first == null)
			{
				first = step;
			}
			last = step;
			steps.Add(step);
		}

		public void reset()
		{
			used = false;
			first = null;
			last = null;
			projectileVFX = null;
			projectileVFXConfiguration.Clear();
			steps.Clear();
		}

		public void SetParameter(string Key, string Value)
		{
			projectileVFXConfiguration[Key] = Value;
		}

		public void SetProjectileVFX(Dictionary<string, string> Values)
		{
			used = true;
			if (Values.TryGetValue("Effect", out var value))
			{
				projectileVFX = value;
			}
			foreach (KeyValuePair<string, string> Value in Values)
			{
				projectileVFXConfiguration[Value.Key] = Value.Value;
			}
		}

		public void SetProjectileRender(XRL.World.GameObject Object)
		{
			Render render = Object.Render;
			if (render == null)
			{
				return;
			}
			used = true;
			projectileVFXConfiguration["RenderTile"] = render.Tile;
			ColorChars colorChars = render.getColorChars();
			projectileVFXConfiguration["RenderForeground"] = colorChars.foreground.ToString();
			projectileVFXConfiguration["RenderDetail"] = colorChars.detail.ToString();
			if (render.HFlip)
			{
				if (render.VFlip)
				{
					projectileVFXConfiguration["RenderFlip"] = "B";
				}
				else
				{
					projectileVFXConfiguration["RenderFlip"] = "H";
				}
			}
			else if (render.VFlip)
			{
				projectileVFXConfiguration["RenderFlip"] = "V";
			}
		}
	}

	private static Queue<MissileWeaponVFXConfiguration> pool = new Queue<MissileWeaponVFXConfiguration>();

	public Dictionary<int, MissileVFXPathDefinition> paths = new Dictionary<int, MissileVFXPathDefinition>();

	public float duration = 1.5f;

	public static MissileWeaponVFXConfiguration next()
	{
		MissileWeaponVFXConfiguration missileWeaponVFXConfiguration;
		lock (pool)
		{
			missileWeaponVFXConfiguration = ((pool.Count <= 0) ? new MissileWeaponVFXConfiguration() : pool.Dequeue());
		}
		missileWeaponVFXConfiguration.reset();
		return missileWeaponVFXConfiguration;
	}

	public static void repool(MissileWeaponVFXConfiguration configuration)
	{
		if (configuration == null)
		{
			return;
		}
		configuration.reset();
		lock (pool)
		{
			pool.Enqueue(configuration);
		}
	}

	public void reset()
	{
		foreach (KeyValuePair<int, MissileVFXPathDefinition> path in paths)
		{
			path.Value.reset();
		}
		duration = 1.5f;
	}

	public MissileVFXPathDefinition GetPath(int Index)
	{
		if (!paths.TryGetValue(Index, out var value))
		{
			value = new MissileVFXPathDefinition();
			paths.Add(Index, value);
		}
		return value;
	}

	public void addStep(int pathIndex, Location2D location)
	{
		GetPath(pathIndex)?.addStep(location);
	}

	public void setPathProjectileVFX(int pathIndex, string projectileVFX, string projectilVFXConfiguration)
	{
		MissileVFXPathDefinition path = GetPath(pathIndex);
		path.used = true;
		path.projectileVFX = projectileVFX;
		if (!projectilVFXConfiguration.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, string> item in projectilVFXConfiguration.CachedDictionaryExpansion())
			{
				path.projectileVFXConfiguration.Set(item.Key, item.Value);
			}
		}
		string configValue = path.getConfigValue("duration");
		if (configValue == null || !(configValue != ""))
		{
			return;
		}
		if (float.TryParse(configValue, out var result))
		{
			if (result != duration)
			{
				duration = result;
			}
		}
		else
		{
			Debug.LogError("invalid duration specification: " + configValue);
		}
	}

	public void setPathProjectileVFX(int pathIndex, Dictionary<string, string> Values)
	{
		GetPath(pathIndex).SetProjectileVFX(Values);
	}

	public void SetPathProjectileRender(int Index, XRL.World.GameObject Object)
	{
		GetPath(Index).SetProjectileRender(Object);
	}
}
