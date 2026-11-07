using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XRL.World;

[Serializable]
[HasModSensitiveStaticCache]
public class Damage
{
	public int _Amount;

	public List<string> Attributes = new List<string>();

	public bool SuppressionMessageDone;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, string> AttributeSounds;

	public int Amount
	{
		get
		{
			return _Amount;
		}
		set
		{
			_Amount = Math.Max(value, 0);
		}
	}

	public static void SetAttributeSound(string attribute, string sounds)
	{
		if (AttributeSounds == null)
		{
			AttributeSounds = new Dictionary<string, string>();
		}
		AttributeSounds.Set(attribute, sounds);
	}

	[ModSensitiveCacheInit]
	private static void Init()
	{
		SetAttributeSound("Fire", "Sounds/Damage/sfx_damage_elemental_heat");
		SetAttributeSound("Heat", "Sounds/Damage/sfx_damage_elemental_heat");
		SetAttributeSound("Acid", "Sounds/Damage/sfx_damage_elemental_acid");
		SetAttributeSound("Cold", "Sounds/Damage/sfx_damage_elemental_cold");
		SetAttributeSound("Electric", "Sounds/Damage/sfx_damage_elemental_electrical");
		SetAttributeSound("Electricity", "Sounds/Damage/sfx_damage_elemental_electrical");
		SetAttributeSound("Electrical", "Sounds/Damage/sfx_damage_elemental_electrical");
		SetAttributeSound("Shock", "Sounds/Damage/sfx_damage_elemental_electrical");
		SetAttributeSound("Lightning", "Sounds/Damage/sfx_damage_elemental_electrical");
		SetAttributeSound("Light", "Sounds/Damage/sfx_damage_elemental_light");
		SetAttributeSound("Laser", "Sounds/Damage/sfx_damage_elemental_light");
		SetAttributeSound("Poison", "Sounds/Damage/sfx_damage_elemental_poison");
		SetAttributeSound("Asphyxiation", "Sounds/Damage/sfx_damage_elemental_poison");
		SetAttributeSound("Bleeding", "Sounds/Damage/sfx_damage_elemental_bleeding");
		SetAttributeSound("Mental", "Sounds/Damage/sfx_damage_elemental_mental");
		SetAttributeSound("Psionic", "Sounds/Damage/sfx_damage_elemental_mental");
		SetAttributeSound("Explosion", "Sounds/Damage/sfx_damage_explosive");
		SetAttributeSound("Explosive", "Sounds/Damage/sfx_damage_explosive");
		SetAttributeSound("Explosives", "Sounds/Damage/sfx_damage_explosive");
	}

	public void PlaySound(GameObject target)
	{
		foreach (string item in (from _ in AttributeSounds
			where HasAttribute(_.Key)
			select _.Value).Distinct())
		{
			target.PlayWorldSound(item);
		}
	}

	public Damage(int Amount)
	{
		this.Amount = Amount;
	}

	public bool HasAnyAttribute(List<string> Names)
	{
		if (Names == null)
		{
			return false;
		}
		if (Attributes == null)
		{
			return false;
		}
		foreach (string attribute in Attributes)
		{
			if (Names.Contains(attribute))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAttribute(string Name)
	{
		if (Attributes == null)
		{
			return false;
		}
		if (Attributes.Contains(Name))
		{
			return true;
		}
		return false;
	}

	public void AddAttribute(string Name)
	{
		Attributes.Add(Name);
	}

	public void AddAttributes(string List)
	{
		if (List.IsNullOrEmpty())
		{
			return;
		}
		if (List.Contains(" "))
		{
			string[] array = List.Split(' ');
			foreach (string name in array)
			{
				AddAttribute(name);
			}
		}
		else
		{
			AddAttribute(List);
		}
	}

	public bool IsColdDamage()
	{
		if (!HasAttribute("Cold") && !HasAttribute("Ice"))
		{
			return HasAttribute("Freeze");
		}
		return true;
	}

	public bool IsHeatDamage()
	{
		if (!HasAttribute("Fire"))
		{
			return HasAttribute("Heat");
		}
		return true;
	}

	public bool IsElectricDamage()
	{
		if (!HasAttribute("Electric") && !HasAttribute("Shock") && !HasAttribute("Lightning"))
		{
			return HasAttribute("Electricity");
		}
		return true;
	}

	public bool IsBludgeoningDamage()
	{
		if (!HasAttribute("Cudgel"))
		{
			return HasAttribute("Bludgeoning");
		}
		return true;
	}

	public bool IsAcidDamage()
	{
		return HasAttribute("Acid");
	}

	public bool IsLightDamage()
	{
		if (!HasAttribute("Light"))
		{
			return HasAttribute("Laser");
		}
		return true;
	}

	public bool IsDisintegrationDamage()
	{
		if (!HasAttribute("Disintegrate"))
		{
			return HasAttribute("Disintegration");
		}
		return true;
	}

	public static bool IsColdDamage(string Type)
	{
		if (!(Type == "Cold") && !(Type == "Ice"))
		{
			return Type == "Freeze";
		}
		return true;
	}

	public static bool IsHeatDamage(string Type)
	{
		if (!(Type == "Fire"))
		{
			return Type == "Heat";
		}
		return true;
	}

	public static bool IsElectricDamage(string Type)
	{
		switch (Type)
		{
		default:
			return Type == "Electrical";
		case "Electric":
		case "Shock":
		case "Lightning":
		case "Electricity":
			return true;
		}
	}

	public static bool IsAcidDamage(string Type)
	{
		return Type == "Acid";
	}

	public static bool IsLightDamage(string Type)
	{
		if (!(Type == "Light"))
		{
			return Type == "Laser";
		}
		return true;
	}

	public static bool IsDisintegrationDamage(string Type)
	{
		if (!(Type == "Disintegrate"))
		{
			return Type == "Disintegration";
		}
		return true;
	}

	public static bool ContainsColdDamage(string Type)
	{
		if (!Type.HasDelimitedSubstring(' ', "Cold") && !Type.HasDelimitedSubstring(' ', "Ice"))
		{
			return Type.HasDelimitedSubstring(' ', "Freeze");
		}
		return true;
	}

	public static bool ContainsHeatDamage(string Type)
	{
		if (!Type.HasDelimitedSubstring(' ', "Fire"))
		{
			return Type.HasDelimitedSubstring(' ', "Heat");
		}
		return true;
	}

	public static bool ContainsElectricDamage(string Type)
	{
		if (!Type.HasDelimitedSubstring(' ', "Electric") && !Type.HasDelimitedSubstring(' ', "Shock") && !Type.HasDelimitedSubstring(' ', "Lightning"))
		{
			return Type.HasDelimitedSubstring(' ', "Electricity");
		}
		return true;
	}

	public static bool ContainsAcidDamage(string Type)
	{
		return Type.HasDelimitedSubstring(' ', "Acid");
	}

	public static bool ContainsLightDamage(string Type)
	{
		if (!Type.HasDelimitedSubstring(' ', "Light"))
		{
			return Type.HasDelimitedSubstring(' ', "Laser");
		}
		return true;
	}

	public static bool ContainsDisintegrationDamage(string Type)
	{
		if (!Type.HasDelimitedSubstring(' ', "Disintegrate"))
		{
			return Type.HasDelimitedSubstring(' ', "Disintegration");
		}
		return true;
	}

	public static string GetDamageColor(string Attributes)
	{
		return "r";
	}

	public string GetDebugInfo()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(Amount);
		if (Attributes.Count > 0)
		{
			bool flag = true;
			foreach (string attribute in Attributes)
			{
				stringBuilder.Append(flag ? ':' : '/').Append(attribute);
			}
		}
		if (SuppressionMessageDone)
		{
			stringBuilder.Append("(SMD)");
		}
		return stringBuilder.ToString();
	}
}
