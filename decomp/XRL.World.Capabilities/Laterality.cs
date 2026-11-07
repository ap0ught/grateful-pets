using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Anatomy;

namespace XRL.World.Capabilities;

public static class Laterality
{
	public const int NONE = 0;

	public const int LEFT = 1;

	public const int RIGHT = 2;

	public const int UPPER = 4;

	public const int LOWER = 8;

	public const int FORE = 16;

	public const int MID = 32;

	public const int HIND = 64;

	public const int INSIDE = 128;

	public const int OUTSIDE = 256;

	public const int INNER = 512;

	public const int OUTER = 1024;

	public const int ANY = 65535;

	public const int AXIS_LATERAL = 1;

	public const int AXIS_VERTICAL = 2;

	public const int AXIS_LONGITUDINAL = 4;

	public const int AXIS_SUPERFICIAL = 8;

	public const int AXIS_STRATAL = 16;

	public static Dictionary<int, int[]> Axes = new Dictionary<int, int[]>
	{
		{
			1,
			new int[3] { 3, 1, 2 }
		},
		{
			2,
			new int[3] { 12, 4, 8 }
		},
		{
			4,
			new int[4] { 112, 16, 64, 32 }
		},
		{
			8,
			new int[3] { 384, 128, 256 }
		},
		{
			16,
			new int[3] { 1536, 512, 1024 }
		}
	};

	public static StringBuilder BuildLateralityAdjective(int Laterality, out bool Conjoin, bool Capitalized = false)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int num = Laterality;
		Conjoin = false;
		if ((Laterality & 0x200) != 0)
		{
			stringBuilder.Compound(Capitalized ? "Inner" : "inner");
			Laterality &= -513;
		}
		else if ((Laterality & 0x400) != 0)
		{
			stringBuilder.Compound(Capitalized ? "Outer" : "outer");
			Laterality &= -1025;
		}
		if ((Laterality & 4) != 0)
		{
			stringBuilder.Compound(Capitalized ? "Upper" : "upper");
			Laterality &= -5;
		}
		else if ((Laterality & 8) != 0)
		{
			stringBuilder.Compound(Capitalized ? "Lower" : "lower");
			Laterality &= -9;
		}
		if ((Laterality & 2) != 0)
		{
			stringBuilder.Compound(Capitalized ? "Right" : "right");
			Laterality &= -3;
		}
		else if ((Laterality & 1) != 0)
		{
			stringBuilder.Compound(Capitalized ? "Left" : "left");
			Laterality &= -2;
		}
		if ((Laterality & 0x80) != 0)
		{
			stringBuilder.Compound(Capitalized ? "Inside" : "inside");
			Laterality &= -129;
		}
		else if ((Laterality & 0x100) != 0)
		{
			stringBuilder.Compound(Capitalized ? "Outside" : "outside");
			Laterality &= -257;
		}
		if ((Laterality & 0x20) != 0)
		{
			if ((Laterality & 0x10) != 0)
			{
				stringBuilder.Compound(Capitalized ? "Mid-Fore" : "mid-fore");
				Laterality &= -17;
			}
			else if ((Laterality & 0x40) != 0)
			{
				stringBuilder.Compound(Capitalized ? "Mid-Hind" : "mid-hind");
				Laterality &= -65;
			}
			else
			{
				stringBuilder.Compound(Capitalized ? "Mid" : "mid");
			}
			Laterality &= -33;
			Conjoin = true;
		}
		else if ((Laterality & 0x10) != 0)
		{
			stringBuilder.Compound(Capitalized ? "Fore" : "fore");
			Laterality &= -17;
			Conjoin = true;
		}
		else if ((Laterality & 0x40) != 0)
		{
			stringBuilder.Compound(Capitalized ? "Hind" : "hind");
			Laterality &= -65;
			Conjoin = true;
		}
		if (Laterality != 0)
		{
			throw new Exception("invalid concrete laterality value " + num);
		}
		return stringBuilder;
	}

	public static string WithLateralityAdjective(string Base, int Laterality, out bool Conjoin, bool Capitalized = false)
	{
		StringBuilder stringBuilder = BuildLateralityAdjective(Laterality, out Conjoin, Capitalized);
		if (stringBuilder.Length == 0)
		{
			return Base;
		}
		if (Base == null)
		{
			return stringBuilder.ToString();
		}
		if (Conjoin && (Base.Contains(" ") || Base.Contains("-")))
		{
			Conjoin = false;
		}
		if (Conjoin)
		{
			stringBuilder.Append(Base.ToLower());
		}
		else
		{
			stringBuilder.Compound(Base);
		}
		return stringBuilder.ToString();
	}

	public static string WithLateralityAdjective(string Base, int Laterality, bool Capitalized = false)
	{
		bool Conjoin;
		return WithLateralityAdjective(Base, Laterality, out Conjoin, Capitalized);
	}

	public static string LateralityAdjective(int Laterality, out bool Conjoin, bool Capitalized = false)
	{
		return WithLateralityAdjective(null, Laterality, out Conjoin, Capitalized);
	}

	public static string LateralityAdjective(int Laterality, bool Capitalized = false)
	{
		return WithLateralityAdjective(null, Laterality, Capitalized);
	}

	public static string StripLateralityAdjective(string Text, int Laterality, bool Capitalized = false)
	{
		if (Text == null)
		{
			return null;
		}
		StringBuilder stringBuilder = BuildLateralityAdjective(Laterality, out var Conjoin, Capitalized);
		if (stringBuilder.Length == 0)
		{
			return Text;
		}
		string text = stringBuilder.ToString();
		if (Text.StartsWith(text) && Text.Length > text.Length)
		{
			if (Conjoin && (Text.LastIndexOf(' ') >= text.Length || Text.LastIndexOf('-') > text.Length))
			{
				Conjoin = false;
			}
			Text = (Conjoin ? ((!Capitalized) ? Text.Substring(text.Length) : ((Text.Length <= text.Length + 1) ? (char.ToUpper(Text[text.Length]).ToString() ?? "") : (char.ToUpper(Text[text.Length]) + Text.Substring(text.Length + 1)))) : ((Text.Length <= text.Length + 1 || Text[text.Length] != ' ') ? Text.Substring(text.Length) : Text.Substring(text.Length + 1)));
		}
		return Text;
	}

	public static int GetCodeFromAdjective(ref string Text)
	{
		if (!Text.Contains(" "))
		{
			return 65535;
		}
		List<string> list = new List<string>(Text.Split(' '));
		int num = 0;
		for (; list.Count > 1; list.RemoveAt(0))
		{
			switch (list[0])
			{
			case "Right":
			case "right":
				num |= 2;
				continue;
			case "Left":
			case "left":
				num |= 1;
				continue;
			case "Upper":
			case "upper":
				num |= 4;
				continue;
			case "Lower":
			case "lower":
				num |= 8;
				continue;
			case "Fore":
			case "fore":
				num |= 0x10;
				continue;
			case "Mid":
			case "mid":
				num |= 0x20;
				continue;
			case "Hind":
			case "hind":
				num |= 0x40;
				continue;
			case "Inside":
			case "inside":
				num |= 0x80;
				continue;
			case "Outside":
			case "outside":
				num |= 0x100;
				continue;
			case "Inner":
			case "inner":
				num |= 0x200;
				continue;
			case "Outer":
			case "outer":
				num |= 0x400;
				continue;
			}
			break;
		}
		if (num == 0)
		{
			num = 65535;
		}
		Text = string.Join(" ", list.ToArray());
		return num;
	}

	public static int GetCodeFromAdjective(string Text)
	{
		return GetCodeFromAdjective(ref Text);
	}

	public static int GetCode(string text)
	{
		if (text.Contains(" "))
		{
			int num = 0;
			string[] array = text.Split(' ');
			foreach (string text2 in array)
			{
				num |= GetCode(text2);
			}
			return num;
		}
		return text switch
		{
			"None" => 0, 
			"Right" => 2, 
			"Left" => 1, 
			"Upper" => 4, 
			"Lower" => 8, 
			"Fore" => 16, 
			"Mid" => 32, 
			"Hind" => 64, 
			"Inside" => 128, 
			"Outside" => 256, 
			"Inner" => 512, 
			"Outer" => 1024, 
			"Any" => 65535, 
			_ => throw new Exception("invalid laterality '" + text + "'"), 
		};
	}

	public static int GetAxisCode(string text)
	{
		return text switch
		{
			"Lateral" => 1, 
			"Vertical" => 2, 
			"Longitudinal" => 4, 
			"Superficial" => 8, 
			"Stratal" => 16, 
			_ => throw new Exception("invalid laterality axis '" + text + "'"), 
		};
	}

	public static string GetAxisName(int code)
	{
		return code switch
		{
			1 => "Lateral", 
			2 => "Vertical", 
			4 => "Longitudinal", 
			8 => "Superficial", 
			16 => "Stratal", 
			_ => throw new Exception("invalid laterality code " + code), 
		};
	}

	public static int GetLateralityAxes(int code)
	{
		int num = 0;
		foreach (KeyValuePair<int, int[]> axis in Axes)
		{
			if ((code & axis.Value[0]) != 0)
			{
				num |= axis.Key;
			}
		}
		return num;
	}

	public static bool Match(int Laterality, int RequiredLaterality)
	{
		if (RequiredLaterality == 0)
		{
			if (Laterality == 0)
			{
				return true;
			}
		}
		else if ((Laterality & RequiredLaterality) == RequiredLaterality)
		{
			return true;
		}
		return false;
	}

	public static bool Match(BodyPart Part, int RequiredLaterality)
	{
		return Match(Part.Laterality, RequiredLaterality);
	}
}
