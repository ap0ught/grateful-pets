using System;
using System.Collections.Generic;
using XRL.World;

namespace XRL.EditorFormats.Map;

public class MapFileObjectBlueprint
{
	public string Name;

	public string Owner;

	public string Part;

	public Dictionary<string, string> Properties;

	public Dictionary<string, int> IntProperties;

	public MapFileObjectBlueprint(string Name, string Owner = null, string Part = null)
	{
		this.Name = Name;
		this.Owner = Owner;
		this.Part = Part;
	}

	public MapFileObjectBlueprint(MapFileObjectBlueprint bp)
	{
		Name = bp.Name;
		Owner = bp.Owner;
		Part = bp.Part;
		if (!bp.Properties.IsNullOrEmpty())
		{
			Properties = new Dictionary<string, string>(bp.Properties);
		}
		if (!bp.IntProperties.IsNullOrEmpty())
		{
			IntProperties = new Dictionary<string, int>(bp.IntProperties);
		}
	}

	public static bool operator ==(MapFileObjectBlueprint a, MapFileObjectBlueprint b)
	{
		return object.Equals(a, b);
	}

	public static bool operator !=(MapFileObjectBlueprint a, MapFileObjectBlueprint b)
	{
		return !object.Equals(a, b);
	}

	public override int GetHashCode()
	{
		int num = 37;
		num = num * 53 + Name.GetHashCode();
		if (!Owner.IsNullOrEmpty())
		{
			num = num * 53 + Owner.GetHashCode();
		}
		if (!Part.IsNullOrEmpty())
		{
			num = num * 53 + Part.GetHashCode();
		}
		if (!Properties.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, string> property in Properties)
			{
				num = num * 53 + property.Key.GetHashCode();
				num = num * 53 + property.Value.GetHashCode();
			}
		}
		if (!IntProperties.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, int> intProperty in IntProperties)
			{
				num = num * 53 + intProperty.Key.GetHashCode();
				num = num * 53 + intProperty.Value.GetHashCode();
			}
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (obj is MapFileObjectBlueprint mapFileObjectBlueprint)
		{
			if (Name == mapFileObjectBlueprint.Name && (Owner == mapFileObjectBlueprint.Owner || (Owner.IsNullOrEmpty() && mapFileObjectBlueprint.Owner.IsNullOrEmpty())) && (Part == mapFileObjectBlueprint.Part || (Part.IsNullOrEmpty() && mapFileObjectBlueprint.Part.IsNullOrEmpty())) && Properties.PairEquals(mapFileObjectBlueprint.Properties))
			{
				return IntProperties.PairEquals(mapFileObjectBlueprint.IntProperties);
			}
			return false;
		}
		return false;
	}

	public bool HasProperty(string Key)
	{
		if (Properties == null || !Properties.ContainsKey(Key))
		{
			if (IntProperties != null)
			{
				return IntProperties.ContainsKey(Key);
			}
			return false;
		}
		return true;
	}

	public GameObject Create(Action<GameObject> BeforeObjectCreated = null)
	{
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(Name, BeforeObjectCreated);
		bool flag = false;
		if (!Owner.IsNullOrEmpty() && gameObject.Physics != null)
		{
			gameObject.Physics.Owner = Owner;
			flag = true;
		}
		if (!Part.IsNullOrEmpty())
		{
			Type type = ModManager.ResolveType("XRL.World.Parts." + Part);
			gameObject.AddPart(Activator.CreateInstance(type) as IPart);
			flag = true;
		}
		if (!Properties.IsNullOrEmpty())
		{
			flag = true;
			foreach (KeyValuePair<string, string> property in Properties)
			{
				gameObject.Property[property.Key] = property.Value;
			}
		}
		if (!IntProperties.IsNullOrEmpty())
		{
			flag = true;
			foreach (KeyValuePair<string, int> intProperty in IntProperties)
			{
				gameObject.IntProperty[intProperty.Key] = intProperty.Value;
			}
		}
		if (flag && (gameObject.HasTag("Immutable") || gameObject.HasTag("ImmutableWhenUnexplored")))
		{
			gameObject.SetIntProperty("ForceMutableSave", 1);
		}
		return gameObject;
	}
}
