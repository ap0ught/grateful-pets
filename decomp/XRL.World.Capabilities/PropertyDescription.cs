using System;
using System.Collections.Generic;
using System.Text;

namespace XRL.World.Capabilities;

public static class PropertyDescription
{
	[NonSerialized]
	public static List<string> InverseBenefitProperties = new List<string>();

	[NonSerialized]
	public static List<string> BooleanProperties = new List<string> { "Analgesia", "BioScannerEquipped", "ForcefieldNullifier", "Horrifying", "Serene", "StructureScannerEquipped", "TechScannerEquipped" };

	[NonSerialized]
	public static Dictionary<string, string> PropertyDisplayNames = new Dictionary<string, string>
	{
		{ "Analgesia", "analgesia" },
		{ "BioScannerEquipped", "bioscanning" },
		{ "ButcheryToolEquipped", "butchery" },
		{ "CarryBonus", "carrying capacity" },
		{ "CyberneticsLicenses", "cybernetics license tier" },
		{ "DisassembleBonus", "artifact disassembly" },
		{ "ForcefieldNullifier", "forcefield nullification" },
		{ "Horrifying", "horrifying presence" },
		{ "InspectorEquipped", "artifact inspection" },
		{ "MentalMutationForceShift", "mental mutations regardless of level" },
		{ "MentalMutationShift", "mental mutations" },
		{ "PhysicalMutationForceShift", "physical mutations regardless of level" },
		{ "PhysicalMutationShift", "physical mutations" },
		{ "Serene", "meditative serenity" },
		{ "StructureScannerEquipped", "structural scanning" },
		{ "TechScannerEquipped", "techscanning" }
	};

	[NonSerialized]
	public static Dictionary<string, int> PropertyEffectTypes = new Dictionary<string, int>
	{
		{ "Analgesia", 8192 },
		{ "BioScannerEquipped", 128 },
		{ "ButcheryToolEquipped", 128 },
		{ "CarryBonus", 128 },
		{ "CyberneticsLicenses", 128 },
		{ "DisassembleBonus", 128 },
		{ "ForcefieldNullifier", 64 },
		{ "Horrifying", 128 },
		{ "InspectorEquipped", 64 },
		{ "MentalMutationForceShift", 2 },
		{ "MentalMutationShift", 2 },
		{ "PhysicalMutationForceShift", 4 },
		{ "PhysicalMutationShift", 4 },
		{ "Serene", 2 },
		{ "StructureScannerEquipped", 128 },
		{ "TechScannerEquipped", 128 }
	};

	public static bool IsInverseBenefit(string Property)
	{
		return InverseBenefitProperties.Contains(Property);
	}

	public static bool IsBoolean(string Property)
	{
		return BooleanProperties.Contains(Property);
	}

	public static string GetPropertyDisplayName(string Property)
	{
		if (!PropertyDisplayNames.TryGetValue(Property, out var value))
		{
			return Property;
		}
		return value;
	}

	public static int GetPropertyEffectType(string Property)
	{
		if (!PropertyEffectTypes.TryGetValue(Property, out var value))
		{
			return 0;
		}
		return value;
	}

	public static void GetPropertyDescription(StringBuilder SB, string Property)
	{
		if (IsBoolean(Property))
		{
			SB.Append("provides ");
		}
		else
		{
			SB.Append("affects ");
		}
		SB.Append(GetPropertyDisplayName(Property));
	}

	public static string GetPropertyDescription(string Property)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		GetPropertyDescription(stringBuilder, Property);
		return stringBuilder.ToString();
	}

	public static void GetPropertyAdjustDescription(StringBuilder SB, string Property, int Adjust)
	{
		if (IsInverseBenefit(Property))
		{
			Adjust = -Adjust;
		}
		if (IsBoolean(Property))
		{
			SB.Append((Adjust > 0) ? "provides " : "inhibits ");
		}
		else
		{
			if (Adjust > 0)
			{
				SB.Append('+');
			}
			SB.Append(Adjust).Append(" to ");
		}
		SB.Append(GetPropertyDisplayName(Property));
	}

	public static string GetPropertyAdjustDescription(string Property, int Adjust)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		GetPropertyAdjustDescription(stringBuilder, Property, Adjust);
		return stringBuilder.ToString();
	}
}
