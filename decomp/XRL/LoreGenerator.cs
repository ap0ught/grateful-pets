using System;
using System.Collections.Generic;
using System.Text;
using XRL.Rules;
using XRL.World;

namespace XRL;

public static class LoreGenerator
{
	private static StringBuilder SB = new StringBuilder();

	private static Dictionary<string, int[]> landmarks = new Dictionary<string, int[]>
	{
		{
			"Joppa",
			new int[2] { 11, 22 }
		},
		{
			"Red Rock",
			new int[2] { 11, 20 }
		},
		{
			"Grit Gate",
			new int[2] { 22, 14 }
		},
		{
			"the Six Day Stilt",
			new int[2] { 5, 2 }
		},
		{
			"Golgotha",
			new int[2] { 23, 9 }
		},
		{
			"Bethesda Susa",
			new int[2] { 25, 3 }
		},
		{
			"Kyakukya",
			new int[2] { 27, 20 }
		},
		{
			"Omonporch",
			new int[2] { 53, 4 }
		},
		{
			"the Yd Freehold",
			new int[2] { 67, 17 }
		}
	};

	private static Dictionary<string, int[]> landmarks_altStart = new Dictionary<string, int[]>
	{
		{
			"the ruins of Joppa",
			new int[2] { 11, 22 }
		},
		{
			"Red Rock",
			new int[2] { 11, 20 }
		},
		{
			"Grit Gate",
			new int[2] { 22, 14 }
		},
		{
			"the Six Day Stilt",
			new int[2] { 5, 2 }
		},
		{
			"Golgotha",
			new int[2] { 23, 9 }
		},
		{
			"Bethesda Susa",
			new int[2] { 25, 3 }
		},
		{
			"Kyakukya",
			new int[2] { 27, 20 }
		},
		{
			"Omonporch",
			new int[2] { 53, 4 }
		},
		{
			"the Yd Freehold",
			new int[2] { 67, 17 }
		}
	};

	public static string RuinOfHouseIsnerLore(int X, int Y)
	{
		KeyValuePair<string, int[]> keyValuePair = (The.Game.AlternateStart ? landmarks_altStart.GetRandomElement() : landmarks.GetRandomElement());
		string key = keyValuePair.Key;
		int num = X - keyValuePair.Value[0];
		int num2 = Y - keyValuePair.Value[1];
		string text = ((num < 0) ? "west" : "east");
		string text2 = ((num2 < 0) ? "north" : "south");
		num = Math.Abs(num);
		num2 = Math.Abs(num2);
		int num3;
		string text3;
		if (Stat.Random(0, 1) == 0)
		{
			num3 = num;
			text3 = text;
		}
		else
		{
			num3 = num2;
			text3 = text2;
		}
		int num4 = Stat.Random(0, 40);
		string text4;
		if (num4 < 10)
		{
			text4 = "of the ";
		}
		else if (num4 < 20)
		{
			text4 = "to the ";
		}
		else
		{
			text4 = ((num4 >= 30) ? "in the " : "with the ");
		}
		num4 = Stat.Random(0, 70);
		string text5;
		if (num4 < 10)
		{
			text5 = "{Ruin of House Isner, ";
		}
		else if (num4 < 20)
		{
			text5 = "{masterwork pistol Ruin of House Isner, ";
		}
		else if (num4 < 30)
		{
			text5 = "{chest containing the Ruin of House Isner, ";
		}
		else if (num4 < 40)
		{
			text5 = "{chest holding the Ruin of House Isner, ";
		}
		else if (num4 < 50)
		{
			text5 = "{famous revolver Ruin of House Isner, ";
		}
		else
		{
			text5 = ((num4 >= 60) ? "{lost masterwork pistol, " : "{hiding place of the Ruin of House Isner, ");
		}
		num4 = Stat.Random(0, 60);
		string text6;
		if (num4 < 10)
		{
			text6 = ((num3 == 0) ? ("said to be stored somewhere under one parasang " + text3 + " of " + key + ".}") : ("said to be stored somewhere " + num3 + " parasangs " + text3 + " of " + key + ".}"));
		}
		else if (num4 < 20)
		{
			text6 = ((num3 == 0) ? ("rumored to be stored somewhere under one parasangs " + text3 + " of " + key + ".}") : ("rumored to be stored somewhere " + num3 + " parasangs " + text3 + " of " + key + ".}"));
		}
		else if (num4 < 30)
		{
			text6 = ((num3 == 0) ? ("located somewhere under one parasang " + text3 + " of " + key + ", as the story goes.}") : ("located somewhere " + num3 + " parasangs " + text3 + " of " + key + ", as the story goes.}"));
		}
		else if (num4 < 40)
		{
			text6 = ((num3 == 0) ? ("located somewhere near " + key + " less than one parasang " + text3 + ".}") : ("located somewhere near " + key + " some " + num3 + " parasangs " + text3 + ".}"));
		}
		else if (num4 >= 50)
		{
			text6 = ((num3 == 0) ? ("where I stored it somewhere under one parasang " + text3 + " of " + key + ".}") : ("where I stored it somewhere " + num3 + " parasangs " + text3 + " of " + key + ".}"));
		}
		else
		{
			text6 = ((num3 == 0) ? ("which I hid somewhere " + text3 + " of " + key + ", less than one parasang.}") : ("which I hid somewhere " + text3 + " of " + key + ", about " + num3 + " parasangs.}"));
		}
		return text4 + text5 + text6;
	}

	public static string GenerateLandmarkDirectionsTo(Zone Z, bool bAltStarts = false)
	{
		return GenerateLandmarkDirectionsTo(Z.ZoneID, bAltStarts);
	}

	public static string GenerateLandmarkDirectionsTo(string ZoneID)
	{
		return GenerateLandmarkDirectionsTo(ZoneID, The.Game.AlternateStart);
	}

	public static string GenerateLandmarkDirectionsTo(string ZoneID, bool bAltStarts)
	{
		SB.Clear();
		XRL.World.ZoneID.Parse(ZoneID, out var ParasangX, out var ParasangY, out var _, out var _, out var ZoneZ);
		string text = "";
		string text2 = "";
		int num = 0;
		int num2 = 0;
		num = 0;
		num2 = 0;
		int num3 = int.MaxValue;
		int num4 = int.MaxValue;
		foreach (KeyValuePair<string, int[]> item in bAltStarts ? landmarks_altStart : landmarks)
		{
			if (Math.Abs(ParasangX - item.Value[0]) + Math.Abs(ParasangY - item.Value[1]) < num3)
			{
				int num5 = Math.Abs(ParasangX - item.Value[0]);
				num4 = Math.Abs(ParasangY - item.Value[1]);
				num3 = num5 + num4;
				num = ParasangX - item.Value[0];
				num2 = ParasangY - item.Value[1];
				text = item.Key;
				text2 = item.Key;
			}
		}
		if (num == 0 && num2 == 0)
		{
			return "near " + text;
		}
		if (text == text2)
		{
			if (num < 0)
			{
				if (num == -1)
				{
					SB.Append(Math.Abs(num)).Append(" parasang west");
				}
				else
				{
					SB.Append(Math.Abs(num)).Append(" parasangs west");
				}
			}
			else if (num > 0)
			{
				if (num == 1)
				{
					SB.Append(Math.Abs(num)).Append(" parasang east");
				}
				else
				{
					SB.Append(Math.Abs(num)).Append(" parasangs east");
				}
			}
			if (num2 < 0)
			{
				if (SB.Length > 0)
				{
					SB.Append(" and ");
				}
				if (num2 == -1)
				{
					SB.Append(Math.Abs(num2)).Append(" parasang north");
				}
				else
				{
					SB.Append(Math.Abs(num2)).Append(" parasangs north");
				}
			}
			else if (num2 > 0)
			{
				if (SB.Length > 0)
				{
					SB.Append(" and ");
				}
				if (num2 == 1)
				{
					SB.Append(Math.Abs(num2)).Append(" parasang south");
				}
				else
				{
					SB.Append(Math.Abs(num2)).Append(" parasangs south");
				}
			}
			SB.Append(" of ").Append(text);
		}
		else
		{
			if (num < 0)
			{
				if (num == -1)
				{
					SB.Append(Math.Abs(num)).Append(" parasang west of ").Append(text);
				}
				else
				{
					SB.Append(Math.Abs(num)).Append(" parasangs west of ").Append(text);
				}
			}
			else if (num > 0)
			{
				if (num == 1)
				{
					SB.Append(Math.Abs(num)).Append(" parasang east of ").Append(text);
				}
				else
				{
					SB.Append(Math.Abs(num)).Append(" parasangs east of ").Append(text);
				}
			}
			if (num2 < 0)
			{
				if (SB.Length > 0)
				{
					SB.Append(", ");
				}
				if (num2 == -1)
				{
					SB.Append(Math.Abs(num2)).Append(" parasang north of ").Append(text2);
				}
				else
				{
					SB.Append(Math.Abs(num2)).Append(" parasangs north of ").Append(text2);
				}
			}
			else if (num2 > 0)
			{
				if (SB.Length > 0)
				{
					SB.Append(", ");
				}
				if (num2 == 1)
				{
					SB.Append(Math.Abs(num2)).Append(" parasang south of ").Append(text2);
				}
				else
				{
					SB.Append(Math.Abs(num2)).Append(" parasangs south of ").Append(text2);
				}
			}
		}
		if (ZoneZ > 10)
		{
			SB.Append(", " + (ZoneZ - 10) + " strata deep");
		}
		return SB.ToString();
	}
}
