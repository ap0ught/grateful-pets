using System;
using System.Collections.Generic;

namespace XRL.World.Capabilities;

public static class EnergyStorage
{
	private static readonly string[] electricalDescriptions = new string[6] { "Drained", "Very Low", "Low", "Used", "Fresh", "Full" };

	private static List<string> electricalStatuses = new List<string> { null, null, null, null, null, null };

	private static readonly string[] clockworkDescriptions = new string[6] { "Run Down", "Very Run Down", "Fairly Run Down", "Somewhat Run Down", "Well-Wound", "Fully Wound" };

	private static List<string> clockworkStatuses = new List<string> { null, null, null, null, null, null };

	private static readonly string[] kineticDescriptions = new string[6] { "Stopped", "Very Slow", "Somewhat Slow", "Fairly Fast", "Nearly Full Speed", "Full Speed" };

	private static List<string> kineticStatuses = new List<string> { null, null, null, null, null, null };

	private static readonly string[] tensionDescriptions = new string[6] { "Slack", "Very Slack", "Fairly Slack", "Somewhat Slack", "Tense", "Fully Tensed" };

	private static List<string> tensionStatuses = new List<string> { null, null, null, null, null, null };

	private static readonly string[] glowDescriptions = new string[6] { "Dark", "Very Dim", "Somewhat Dim", "Somewhat Bright", "Fairly Bright", "Bright" };

	private static List<string> glowStatuses = new List<string> { null, null, null, null, null, null };

	private static readonly string[] darkDescriptions = new string[6] { "Gray", "Dark Gray", "Murky Black", "Black", "Deep Black", "Pure Black" };

	private static List<string> darkStatuses = new List<string> { null, null, null, null, null, null };

	private static string[] bioDescriptions = new string[6] { "Exhausted", "Flagging", "Enervated", "Fatigued", "Lively", "Vigorous" };

	private static List<string> bioStatuses = new List<string> { null, null, null, null, null, null };

	private static readonly string[] roughpercentageDescriptions = new string[6] { "0%", "~10%", "~25%", "~50%", "~75%", "~100%" };

	private static List<string> roughpercentageStatuses = new List<string> { null, null, null, null, null, null };

	private static List<string> percentageStatuses = new List<string>(101);

	private static readonly Dictionary<string, string> SlotTypeNames = new Dictionary<string, string>
	{
		{ "EnergyCell", "energy cell" },
		{ "PowerCore", "power core" }
	};

	private static readonly Dictionary<string, string> SlotTypeShortNames = new Dictionary<string, string>
	{
		{ "EnergyCell", "cell" },
		{ "PowerCore", "core" }
	};

	public static string GetSlotTypeName(string SlotType)
	{
		if (SlotType != null && SlotTypeNames.TryGetValue(SlotType, out var value))
		{
			return value;
		}
		return "energy cell";
	}

	public static string GetSlotTypeShortName(string SlotType)
	{
		if (SlotType != null && SlotTypeShortNames.TryGetValue(SlotType, out var value))
		{
			return value;
		}
		return "cell";
	}

	public static int GetChargePercentage(int Charge, int MaxCharge)
	{
		if (MaxCharge == 0)
		{
			return 100;
		}
		int num = Charge * 100 / MaxCharge;
		if (num < 0)
		{
			num = 0;
		}
		else if (num > 100)
		{
			num = 100;
		}
		if (num == 0 && Charge > 0)
		{
			num = 1;
		}
		else if (num == 100 && Charge < MaxCharge)
		{
			num = 99;
		}
		return num;
	}

	public static int GetChargeLevel(int Charge, int MaxCharge)
	{
		float num = GetChargePercentage(Charge, MaxCharge);
		if (num == 0f)
		{
			return 0;
		}
		if (num <= 10f)
		{
			return 1;
		}
		if (num <= 25f)
		{
			return 2;
		}
		if (num <= 50f)
		{
			return 3;
		}
		if (num <= 75f)
		{
			return 4;
		}
		return 5;
	}

	public static string GetChargeLevelColor(int Level)
	{
		return Level switch
		{
			0 => "K", 
			1 => "r", 
			2 => "R", 
			3 => "W", 
			4 => "g", 
			5 => "G", 
			_ => throw new Exception("invalid charge level " + Level), 
		};
	}

	public static string GetChargeColor(int Charge, int MaxCharge)
	{
		return GetChargeLevelColor(GetChargeLevel(Charge, MaxCharge));
	}

	public static string ColorByChargeLevel(string text, int chargeLevel)
	{
		string chargeLevelColor = GetChargeLevelColor(chargeLevel);
		if (!string.IsNullOrEmpty(chargeLevelColor))
		{
			text = Event.NewStringBuilder().Append("{{").Append(chargeLevelColor)
				.Append('|')
				.Append(text)
				.Append("}}")
				.ToString();
		}
		return text;
	}

	public static string ColorByChargeLevel(int number, int chargeLevel)
	{
		string chargeLevelColor = GetChargeLevelColor(chargeLevel);
		if (!string.IsNullOrEmpty(chargeLevelColor))
		{
			return Event.NewStringBuilder().Append("{{").Append(chargeLevelColor)
				.Append('|')
				.Append(number)
				.Append("}}")
				.ToString();
		}
		return number.ToString();
	}

	public static string GetChargeStatus(int Charge, int MaxCharge, string Style = "electrical")
	{
		if (Style == "amount")
		{
			return ColorByChargeLevel(Charge, 3);
		}
		int chargeLevel = GetChargeLevel(Charge, MaxCharge);
		switch (Style)
		{
		case "electrical":
			if (electricalStatuses[chargeLevel] == null)
			{
				electricalStatuses[chargeLevel] = ColorByChargeLevel(electricalDescriptions[chargeLevel], chargeLevel);
			}
			return electricalStatuses[chargeLevel];
		case "clockwork":
			if (clockworkStatuses[chargeLevel] == null)
			{
				clockworkStatuses[chargeLevel] = ColorByChargeLevel(clockworkDescriptions[chargeLevel], chargeLevel);
			}
			return clockworkStatuses[chargeLevel];
		case "kinetic":
			if (kineticStatuses[chargeLevel] == null)
			{
				kineticStatuses[chargeLevel] = ColorByChargeLevel(kineticDescriptions[chargeLevel], chargeLevel);
			}
			return kineticStatuses[chargeLevel];
		case "tension":
			if (tensionStatuses[chargeLevel] == null)
			{
				tensionStatuses[chargeLevel] = ColorByChargeLevel(tensionDescriptions[chargeLevel], chargeLevel);
			}
			return tensionStatuses[chargeLevel];
		case "glow":
			if (glowStatuses[chargeLevel] == null)
			{
				glowStatuses[chargeLevel] = ColorByChargeLevel(glowDescriptions[chargeLevel], chargeLevel);
			}
			return glowStatuses[chargeLevel];
		case "dark":
			if (darkStatuses[chargeLevel] == null)
			{
				darkStatuses[chargeLevel] = ColorByChargeLevel(darkDescriptions[chargeLevel], chargeLevel);
			}
			return darkStatuses[chargeLevel];
		case "bio":
			if (bioStatuses[chargeLevel] == null)
			{
				bioStatuses[chargeLevel] = ColorByChargeLevel(bioDescriptions[chargeLevel], chargeLevel);
			}
			return bioStatuses[chargeLevel];
		case "roughpercentage":
			if (roughpercentageStatuses[chargeLevel] == null)
			{
				roughpercentageStatuses[chargeLevel] = ColorByChargeLevel(roughpercentageDescriptions[chargeLevel], chargeLevel);
			}
			return roughpercentageStatuses[chargeLevel];
		case "percentage":
		{
			if (percentageStatuses.Count == 0)
			{
				for (int i = 0; i <= 100; i++)
				{
					percentageStatuses.Add(null);
				}
			}
			int chargePercentage = GetChargePercentage(Charge, MaxCharge);
			if (percentageStatuses[chargePercentage] == null)
			{
				percentageStatuses[chargePercentage] = ColorByChargeLevel(chargePercentage + "%", chargeLevel);
			}
			return percentageStatuses[chargePercentage];
		}
		default:
			throw new Exception("invalid charge style " + Style);
		}
	}
}
