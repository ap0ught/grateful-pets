using System;
using System.Collections.Generic;
using System.Reflection;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Tinkering;

public static class ItemModding
{
	[NonSerialized]
	private static Dictionary<string, IModification> ModificationInstances = new Dictionary<string, IModification>(64);

	private static Type[] ModDescArgs2 = new Type[2]
	{
		typeof(int),
		typeof(GameObject)
	};

	private static Type[] ModDescArgs1 = new Type[1] { typeof(int) };

	public static bool CanMod(GameObject Object, GameObject Actor = null)
	{
		if (Actor == null)
		{
			Actor = The.Player;
		}
		if (Actor == null || !Actor.HasSkill("Tinkering"))
		{
			return false;
		}
		string text = ModKey(Object);
		if (text != null && (!Actor.IsPlayerControlled() || Object.Understood()))
		{
			foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
			{
				if (knownRecipe.Type == "Mod" && ModAppropriate(Object, knownRecipe, text))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string ModKey(GameObject Object)
	{
		string propertyOrTag = Object.GetPropertyOrTag("Mods");
		if (!propertyOrTag.IsNullOrEmpty() && propertyOrTag != "None" && Object.GetModificationSlotsUsed() < RuleSettings.MAXIMUM_ITEM_MODS)
		{
			return propertyOrTag;
		}
		return null;
	}

	public static bool ModAppropriate(GameObject Object, TinkerData ModData, string Key)
	{
		if (Key != null && ModData.CanMod(Key))
		{
			return ModificationApplicable(ModData.PartName, Object);
		}
		return false;
	}

	public static bool ModAppropriate(GameObject Object, TinkerData ModData)
	{
		return ModAppropriate(Object, ModData, ModKey(Object));
	}

	public static bool ModificationApplicable(string Name, GameObject Object, GameObject Actor = null, string Key = null)
	{
		if ((Key ?? ModKey(Object)) == null)
		{
			return false;
		}
		if (Object.HasPart(Name))
		{
			return false;
		}
		if (Actor != null && Actor.IsPlayerControlled() && !Object.Understood())
		{
			return false;
		}
		if (!CanBeModdedEvent.Check(Actor, Object, Name))
		{
			return false;
		}
		if (!ModificationInstances.TryGetValue(Name, out var value))
		{
			Type type = ModManager.ResolveType("XRL.World.Parts." + Name);
			if (type == null)
			{
				return false;
			}
			value = Activator.CreateInstance(type) as IModification;
			if (value == null)
			{
				return false;
			}
			ModificationInstances[Name] = value;
		}
		return value.ModificationApplicable(Object);
	}

	public static string GetModificationDescription(string Name, int Tier)
	{
		if (Name.Contains("[mod]"))
		{
			Name = Name.Replace("[mod]", "");
		}
		MethodInfo method = ModManager.ResolveType("XRL.World.Parts." + Name).GetMethod("GetDescription", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, ModDescArgs1, null);
		if (method != null)
		{
			return (string)method.Invoke(null, new object[1] { Tier });
		}
		return ModificationFactory.ModsByPart[Name].Description;
	}

	public static string GetModificationDescription(string Name, GameObject obj)
	{
		if (obj == null)
		{
			return GetModificationDescription(Name, 1);
		}
		if (Name.Contains("[mod]"))
		{
			Name = Name.Replace("[mod]", "");
		}
		Type type = ModManager.ResolveType("XRL.World.Parts." + Name);
		MethodInfo method = type.GetMethod("GetDescription", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, ModDescArgs2, null);
		if (method != null)
		{
			return (string)method.Invoke(null, new object[2]
			{
				Math.Max(obj.GetTier(), 1),
				obj
			});
		}
		MethodInfo method2 = type.GetMethod("GetDescription", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, ModDescArgs1, null);
		if (method2 != null)
		{
			return (string)method2.Invoke(null, new object[1] { Math.Max(obj.GetTier(), 1) });
		}
		return ModificationFactory.ModsByPart[Name].Description;
	}

	public static bool ApplyModification(GameObject Object, IModification ModPart, bool DoRegistration = true, GameObject Actor = null, bool Creation = false)
	{
		if (Actor != null && !ModPart.BeingAppliedBy(Object, Actor))
		{
			return false;
		}
		Object.AddPart(ModPart, DoRegistration, Creation);
		if (Object.HasPart<Commerce>() && ModificationFactory.ModsByPart.ContainsKey(ModPart.Name))
		{
			Object.GetPart<Commerce>().Value *= ModificationFactory.ModsByPart[ModPart.Name].Value;
		}
		ModPart.ApplyModification(Object);
		ModificationAppliedEvent.Send(Object, ModPart);
		TinkeringHelpers.CheckMakersMark(Object, Actor, ModPart);
		return true;
	}

	public static bool ApplyModification(GameObject Object, string ModPartName, out IModification ModPart, int Tier, bool DoRegistration = true, GameObject Actor = null, bool Creation = false)
	{
		Type type = ModManager.ResolveType("XRL.World.Parts." + ModPartName);
		if (type == null)
		{
			MetricsManager.LogError("ApplyModification", "Couldn't resolve unknown mod part: " + ModPartName);
			ModPart = null;
			return false;
		}
		XRL.World.Capabilities.Tier.Constrain(ref Tier);
		object[] args = new object[1] { Tier };
		ModPart = Activator.CreateInstance(type, args) as IModification;
		if (ModPart == null)
		{
			if (!(Activator.CreateInstance(type, args) is IPart))
			{
				MetricsManager.LogError("failed to load " + type);
			}
			else
			{
				MetricsManager.LogError(type?.ToString() + " is not an IModification");
			}
			return false;
		}
		return ApplyModification(Object, ModPart, DoRegistration, Actor, Creation);
	}

	public static bool ApplyModification(GameObject Object, string ModPartName, int Tier, bool DoRegistration = true, GameObject Actor = null, bool Creation = false)
	{
		IModification ModPart;
		return ApplyModification(Object, ModPartName, out ModPart, Tier, DoRegistration, Actor, Creation);
	}

	public static bool ApplyModification(GameObject Object, string ModPartName, out IModification ModPart, bool DoRegistration = true, GameObject Actor = null, bool Creation = false)
	{
		return ApplyModification(Object, ModPartName, out ModPart, Object.GetTier(), DoRegistration, Actor, Creation);
	}

	public static bool ApplyModification(GameObject Object, string ModPartName, bool DoRegistration = true, GameObject Actor = null, bool Creation = false)
	{
		IModification ModPart;
		return ApplyModification(Object, ModPartName, out ModPart, DoRegistration, Actor, Creation);
	}

	public static bool ApplyModificationFromPopulationTable(GameObject Object, string Table, int Tier, bool Creation = false)
	{
		string blueprint = PopulationManager.RollOneFrom(Table).Blueprint;
		return ApplyModification(Object, blueprint, Tier, Creation);
	}

	public static bool ApplyModificationFromPopulationTable(GameObject Object, string Table, bool Creation = false)
	{
		string blueprint = PopulationManager.RollOneFrom(Table).Blueprint;
		return ApplyModification(Object, blueprint, Creation);
	}
}
