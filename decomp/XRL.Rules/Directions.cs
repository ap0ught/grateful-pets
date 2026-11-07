using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.World;

namespace XRL.Rules;

public static class Directions
{
	public static string[] DirectionList = new string[8] { "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] CardinalDirectionList = new string[4] { "N", "E", "S", "W" };

	public static Dictionary<string, string[]> DirectionsAdjacent = new Dictionary<string, string[]>(9)
	{
		{
			"NW",
			new string[2] { "W", "N" }
		},
		{
			"N",
			new string[2] { "NW", "NE" }
		},
		{
			"NE",
			new string[2] { "N", "E" }
		},
		{
			"E",
			new string[2] { "NE", "SE" }
		},
		{
			"SE",
			new string[2] { "E", "S" }
		},
		{
			"S",
			new string[2] { "SE", "SW" }
		},
		{
			"SW",
			new string[2] { "S", "W" }
		},
		{
			"W",
			new string[2] { "SW", "NW" }
		},
		{ ".", DirectionList }
	};

	public static Dictionary<string, string[]> DirectionsOrthogonal = new Dictionary<string, string[]>(9)
	{
		{
			"NW",
			new string[2] { "W", "E" }
		},
		{
			"N",
			new string[2] { "W", "E" }
		},
		{
			"NE",
			new string[2] { "W", "S" }
		},
		{
			"E",
			new string[2] { "N", "S" }
		},
		{
			"SE",
			new string[2] { "N", "W" }
		},
		{
			"S",
			new string[2] { "N", "W" }
		},
		{
			"SW",
			new string[2] { "N", "E" }
		},
		{
			"W",
			new string[2] { "N", "S" }
		},
		{
			".",
			new string[2] { "W", "E" }
		}
	};

	public static bool IsActualDirection(string dir)
	{
		if (string.IsNullOrEmpty(dir))
		{
			return false;
		}
		if (Array.IndexOf(DirectionList, dir) != -1)
		{
			return true;
		}
		if (dir == "U" || dir == "D")
		{
			return true;
		}
		return false;
	}

	public static string GetRandomDirection()
	{
		return DirectionList.GetRandomElement();
	}

	public static string GetRandomCardinalDirection()
	{
		return CardinalDirectionList.GetRandomElement();
	}

	public static string GetUITextArrowForDirection(string D)
	{
		return D switch
		{
			"W" => "←", 
			"N" => "↑", 
			"E" => "→", 
			"S" => "↓", 
			"NW" => "↖", 
			"NE" => "↗", 
			"SE" => "↘", 
			"SW" => "↙", 
			"w" => "←", 
			"n" => "↑", 
			"e" => "→", 
			"s" => "↓", 
			"nw" => "↖", 
			"ne" => "↗", 
			"se" => "↘", 
			"sw" => "↙", 
			"." => "\a", 
			_ => "?", 
		};
	}

	public static string GetArrowForDirection(string D)
	{
		return D switch
		{
			"N" => "\u0018", 
			"S" => "\u0019", 
			"E" => "\u001a", 
			"W" => "\u001b", 
			"n" => "\u0018", 
			"s" => "\u0019", 
			"e" => "\u001a", 
			"w" => "\u001b", 
			"U" => "<", 
			"u" => "<", 
			"<" => "<", 
			"D" => ">", 
			"d" => ">", 
			">" => ">", 
			_ => "?", 
		};
	}

	public static string GetExpandedDirection(string D)
	{
		return D switch
		{
			"N" => "north", 
			"S" => "south", 
			"E" => "east", 
			"W" => "west", 
			"NW" => "northwest", 
			"NE" => "northeast", 
			"SW" => "southwest", 
			"SE" => "southeast", 
			"n" => "north", 
			"s" => "south", 
			"e" => "east", 
			"w" => "west", 
			"nw" => "northwest", 
			"ne" => "northeast", 
			"sw" => "southwest", 
			"se" => "southeast", 
			"U" => "up", 
			"u" => "up", 
			"<" => "up", 
			"D" => "down", 
			"d" => "down", 
			">" => "down", 
			"." => "here", 
			"?" => "somewhere", 
			_ => D, 
		};
	}

	public static string GetIndicativeDirection(string D)
	{
		return D switch
		{
			"N" => "northward", 
			"S" => "southward", 
			"E" => "eastward", 
			"W" => "westward", 
			"NW" => "northwestward", 
			"NE" => "northeastward", 
			"SW" => "southwestward", 
			"SE" => "southeastward", 
			"n" => "northward", 
			"s" => "southwarwd", 
			"e" => "eastward", 
			"w" => "westward", 
			"nw" => "northwestward", 
			"ne" => "northeastward", 
			"sw" => "southwestward", 
			"se" => "southeastward", 
			"U" => "upward", 
			"u" => "upward", 
			"<" => "upward", 
			"D" => "downward", 
			"d" => "downward", 
			">" => "downward", 
			"." => "here", 
			"?" => "somewhere", 
			_ => D, 
		};
	}

	public static string GetDirectionDescription(string Direction)
	{
		switch (Direction)
		{
		case "D":
		case "d":
		case ">":
			return "below";
		case "U":
		case "u":
		case "<":
			return "above";
		case ".":
			return "here";
		case "?":
			return "somewhere";
		default:
			return "to the " + GetExpandedDirection(Direction);
		}
	}

	public static string GetDirectionDescription(XRL.World.GameObject Actor, string Direction)
	{
		if (Actor == null || Actor.IsPlayer())
		{
			return GetDirectionDescription(Direction);
		}
		switch (Direction)
		{
		case "D":
		case "d":
		case ">":
			return "below " + Actor.them;
		case "U":
		case "u":
		case "<":
			return "above " + Actor.them;
		case ".":
			return "near " + Actor.them;
		default:
			return "to " + Actor.its + " " + GetExpandedDirection(Direction);
		}
	}

	public static string GetIncomingDirectionDescription(string Direction)
	{
		switch (Direction)
		{
		case "D":
		case "d":
		case ">":
			return "from below";
		case "U":
		case "u":
		case "<":
			return "from above";
		case ".":
			return "from nearby";
		case "?":
			return "from somewhere";
		default:
			return "from the " + GetExpandedDirection(Direction);
		}
	}

	public static string GetIncomingDirectionDescription(XRL.World.GameObject Actor, string Direction)
	{
		if (Actor == null || Actor.IsPlayer())
		{
			return GetIncomingDirectionDescription(Direction);
		}
		switch (Direction)
		{
		case "D":
		case "d":
		case ">":
			return "from below " + Actor.them;
		case "U":
		case "u":
		case "<":
			return "from above " + Actor.them;
		case ".":
			return "from near " + Actor.them;
		default:
			return "from " + Actor.its + " " + GetExpandedDirection(Direction);
		}
	}

	public static string GetDirectionShortDescription(string Direction)
	{
		switch (Direction)
		{
		case "D":
		case "d":
		case ">":
			return "D";
		case "U":
		case "u":
		case "<":
			return "U";
		case ".":
			return "here";
		case "?":
			return "somewhere";
		default:
			return Direction.ToUpper();
		}
	}

	public static string GetOppositeDirection(string Direction)
	{
		if (Direction == "N")
		{
			return "S";
		}
		if (Direction == "S")
		{
			return "N";
		}
		if (Direction == "E")
		{
			return "W";
		}
		if (Direction == "W")
		{
			return "E";
		}
		if (Direction == "NW")
		{
			return "SE";
		}
		if (Direction == "NE")
		{
			return "SW";
		}
		if (Direction == "SW")
		{
			return "NE";
		}
		if (Direction == "SE")
		{
			return "NW";
		}
		if (Direction == "n")
		{
			return "s";
		}
		if (Direction == "s")
		{
			return "n";
		}
		if (Direction == "e")
		{
			return "w";
		}
		if (Direction == "w")
		{
			return "e";
		}
		if (Direction == "nw")
		{
			return "se";
		}
		if (Direction == "ne")
		{
			return "sw";
		}
		if (Direction == "sw")
		{
			return "ne";
		}
		if (Direction == "se")
		{
			return "nw";
		}
		if (Direction == "D")
		{
			return "U";
		}
		if (Direction == "d")
		{
			return "u";
		}
		if (Direction == ">")
		{
			return "<";
		}
		if (Direction == "U")
		{
			return "D";
		}
		if (Direction == "u")
		{
			return "d";
		}
		if (Direction == "<")
		{
			return ">";
		}
		if (Direction == "north")
		{
			return "south";
		}
		if (Direction == "south")
		{
			return "north";
		}
		if (Direction == "east")
		{
			return "west";
		}
		if (Direction == "west")
		{
			return "east";
		}
		if (Direction == "north-east")
		{
			return "south-west";
		}
		if (Direction == "south-east")
		{
			return "north-west";
		}
		if (Direction == "north-west")
		{
			return "south-east";
		}
		if (Direction == "south-east")
		{
			return "south-west";
		}
		if (Direction == "northeast")
		{
			return "southwest";
		}
		if (Direction == "southeast")
		{
			return "northwest";
		}
		return Direction switch
		{
			"northwest" => "southeast", 
			"southeast" => "southwest", 
			"down" => "up", 
			"up" => "down", 
			_ => ".", 
		};
	}

	public static void ApplyDirection(string dir, ref int x, ref int y, int d = 1)
	{
		switch (dir)
		{
		case "N":
			y -= d;
			break;
		case "S":
			y += d;
			break;
		case "W":
			x -= d;
			break;
		case "E":
			x += d;
			break;
		case "NW":
			x -= d;
			y -= d;
			break;
		case "NE":
			x += d;
			y -= d;
			break;
		case "SW":
			x -= d;
			y += d;
			break;
		case "SE":
			x += d;
			y -= d;
			break;
		case "U":
		case "D":
			Debug.LogWarning("handling contextually unsupported direction " + dir);
			break;
		case null:
			Debug.LogWarning("handling null direction");
			break;
		default:
			Debug.LogWarning("handling unrecognized direction " + dir);
			break;
		case ".":
			break;
		}
	}

	public static void ApplyDirection(string dir, ref int x, ref int y, ref int z, int d = 1)
	{
		switch (dir)
		{
		case "N":
			y -= d;
			break;
		case "S":
			y += d;
			break;
		case "W":
			x -= d;
			break;
		case "E":
			x += d;
			break;
		case "NW":
			x -= d;
			y -= d;
			break;
		case "NE":
			x += d;
			y -= d;
			break;
		case "SW":
			x -= d;
			y += d;
			break;
		case "SE":
			x += d;
			y += d;
			break;
		case "U":
		case "<":
			z -= d;
			break;
		case "D":
		case ">":
			z += d;
			break;
		case null:
			Debug.LogWarning("handling null direction");
			break;
		default:
			Debug.LogWarning("handling unrecognized direction " + dir);
			break;
		case ".":
			break;
		}
	}

	public static void ApplyDirectionGlobal(string dir, ref int x, ref int y, ref int z, ref int wx, ref int wy, int d = 1)
	{
		ApplyDirection(dir, ref x, ref y, ref z, d);
		while (x < 0)
		{
			x += 3;
			wx--;
		}
		while (x > 2)
		{
			x -= 3;
			wx++;
		}
		while (y < 0)
		{
			y += 3;
			wy--;
		}
		while (y > 2)
		{
			y -= 3;
			wy++;
		}
	}

	public static string CombineDirections(string A, string B, int AD = 1, int BD = 1)
	{
		int x = 0;
		int y = 0;
		ApplyDirection(A, ref x, ref y, AD);
		ApplyDirection(B, ref x, ref y, BD);
		if (x > 0)
		{
			if (y > 0)
			{
				return "SE";
			}
			if (y < 0)
			{
				return "NE";
			}
			return "E";
		}
		if (x < 0)
		{
			if (y > 0)
			{
				return "SW";
			}
			if (y < 0)
			{
				return "NW";
			}
			return "W";
		}
		if (y > 0)
		{
			return "S";
		}
		if (y < 0)
		{
			return "N";
		}
		return ".";
	}

	public static string[] GetOrthogonalDirections(string Middle)
	{
		if (!DirectionsOrthogonal.ContainsKey(Middle))
		{
			return DirectionsOrthogonal["."];
		}
		return DirectionsOrthogonal[Middle];
	}

	public static string[] GetAdjacentDirections(string Dir)
	{
		if (!DirectionsAdjacent.ContainsKey(Dir))
		{
			return null;
		}
		return DirectionsAdjacent[Dir];
	}

	public static List<string> GetAdjacentDirections(string Middle, int Range)
	{
		int num = 0;
		int i = 0;
		for (int num2 = DirectionList.Length; i < num2; i++)
		{
			if (DirectionList[i].EqualsNoCase(Middle))
			{
				num = i;
				break;
			}
		}
		List<string> list = new List<string>();
		for (int j = num - Range; j <= num + Range; j++)
		{
			int num3 = j;
			if (num3 < 0)
			{
				num3 += DirectionList.Length;
			}
			if (num3 >= DirectionList.Length)
			{
				num3 -= DirectionList.Length;
			}
			list.Add(DirectionList[num3]);
		}
		return list;
	}

	public static RingSpanEnumerator<string> IterateAdjacentDirections(string Start, bool IncludeSelf = false, bool Invert = false)
	{
		return DirectionList.IterateRing(Start, IncludeSelf, Invert);
	}

	public static List<string> DirectionsFromAngle(int degrees, int steps)
	{
		float num = (float)degrees / 58f;
		float num2 = (float)Math.Sin(num);
		float num3 = (float)Math.Cos(num);
		List<string> list = new List<string>(steps);
		float num4 = 0f;
		float num5 = 0f;
		int num6 = 0;
		int num7 = 0;
		int i = 0;
		for (int num8 = steps * 100; i < num8; i++)
		{
			num4 += num2;
			num5 += num3;
			int num9 = (int)Math.Round(num4, MidpointRounding.AwayFromZero);
			int num10 = (int)Math.Round(num5, MidpointRounding.AwayFromZero);
			if (num9 != num6 || num10 != num7)
			{
				string text = "";
				if (num10 > num7)
				{
					text += "N";
				}
				else if (num10 < num7)
				{
					text += "S";
				}
				if (num9 > num6)
				{
					text += "E";
				}
				else if (num9 < num6)
				{
					text += "W";
				}
				list.Add(text);
				if (list.Count >= steps)
				{
					break;
				}
				num6 = num9;
				num7 = num10;
			}
		}
		return list;
	}
}
