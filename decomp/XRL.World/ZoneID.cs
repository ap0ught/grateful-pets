using System;
using System.Text;
using Genkit;
using UnityEngine;

namespace XRL.World;

public static class ZoneID
{
	private static StringBuilder SB = new StringBuilder();

	public static string Assemble(string World, Location2D Location, int ZoneZ = 10)
	{
		return SB.Clear().Append(World).Append('.')
			.Append(Location.X / 3)
			.Append('.')
			.Append(Location.Y / 3)
			.Append('.')
			.Append(Location.X % 3)
			.Append('.')
			.Append(Location.Y % 3)
			.Append('.')
			.Append(ZoneZ)
			.ToString();
	}

	public static string Assemble(string World, int ParasangX, int ParasangY, int ZoneX, int ZoneY, int ZoneZ)
	{
		return SB.Clear().Append(World).Append('.')
			.Append(ParasangX)
			.Append('.')
			.Append(ParasangY)
			.Append('.')
			.Append(ZoneX)
			.Append('.')
			.Append(ZoneY)
			.Append('.')
			.Append(ZoneZ)
			.ToString();
	}

	public static string Assemble(string World, int ParasangX, int ParasangY, string ZoneX, string ZoneY, int ZoneZ)
	{
		return SB.Clear().Append(World).Append('.')
			.Append(ParasangX)
			.Append('.')
			.Append(ParasangY)
			.Append('.')
			.Append(ZoneX)
			.Append('.')
			.Append(ZoneY)
			.Append('.')
			.Append(ZoneZ)
			.ToString();
	}

	private static int Parse(string ID, out string World, out string Blueprint, out string Instance)
	{
		int num = ID.IndexOf('.');
		int num2 = ((num == -1) ? ID.Length : ID.IndexOf('.'));
		int num3 = ID.IndexOf('@', 0, num2);
		if (num3 == -1)
		{
			World = ID.Substring(0, num2);
			Blueprint = null;
			Instance = null;
			return num;
		}
		World = ID.Substring(0, num3);
		int num4 = ID.IndexOf('@', num3 + 1);
		if (num4 == -1)
		{
			Blueprint = ID.Substring(num3 + 1, num2 - num3 - 1);
			Instance = null;
			return num;
		}
		Blueprint = ID.Substring(num3 + 1, num4 - num3 - 1);
		Instance = ID.Substring(num4 + 1, num2 - num4 - 1);
		return num;
	}

	public static bool Parse(string ID, out string World, out string Blueprint, out string Instance, out int ParasangX, out int ParasangY, out int ZoneX, out int ZoneY, out int ZoneZ)
	{
		ParasangX = -1;
		ParasangY = -1;
		ZoneX = -1;
		ZoneY = -1;
		ZoneZ = -1;
		int num = Parse(ID, out World, out Blueprint, out Instance);
		if (num == -1)
		{
			return false;
		}
		int num2 = ID.IndexOf('.', num + 1);
		if (num2 == -1)
		{
			return false;
		}
		int num3 = ID.IndexOf('.', num2 + 1);
		if (num3 == -1)
		{
			return false;
		}
		int num4 = ID.IndexOf('.', num3 + 1);
		if (num4 == -1)
		{
			return false;
		}
		int num5 = ID.IndexOf('.', num4 + 1);
		if (num5 == -1)
		{
			return false;
		}
		try
		{
			ParasangX = int.Parse(ID.AsSpan(num + 1, num2 - num - 1));
			ParasangY = int.Parse(ID.AsSpan(num2 + 1, num3 - num2 - 1));
			ZoneX = int.Parse(ID.AsSpan(num3 + 1, num4 - num3 - 1));
			ZoneY = int.Parse(ID.AsSpan(num4 + 1, num5 - num4 - 1));
			ZoneZ = int.Parse(ID.AsSpan(num5 + 1, ID.Length - num5 - 1));
		}
		catch (Exception x)
		{
			MetricsManager.LogError("error parsing \"" + ID + "\"", x);
		}
		return true;
	}

	public static bool Parse(string ID, out string World, out int ParasangX, out int ParasangY, out int ZoneX, out int ZoneY, out int ZoneZ)
	{
		string Blueprint;
		string Instance;
		return Parse(ID, out World, out Blueprint, out Instance, out ParasangX, out ParasangY, out ZoneX, out ZoneY, out ZoneZ);
	}

	public static bool Parse(string ID, out string World, out int ParasangX, out int ParasangY)
	{
		int ZoneX;
		int ZoneY;
		int ZoneZ;
		return Parse(ID, out World, out ParasangX, out ParasangY, out ZoneX, out ZoneY, out ZoneZ);
	}

	public static bool Parse(string ID, out int ParasangX, out int ParasangY, out int ZoneX, out int ZoneY, out int ZoneZ)
	{
		string World;
		return Parse(ID, out World, out ParasangX, out ParasangY, out ZoneX, out ZoneY, out ZoneZ);
	}

	public static bool Parse(string ID, out int ParasangX, out int ParasangY, out int ZoneX, out int ZoneY)
	{
		string World;
		int ZoneZ;
		return Parse(ID, out World, out ParasangX, out ParasangY, out ZoneX, out ZoneY, out ZoneZ);
	}

	/// <summary>Check match level of two Zone IDs.</summary>
	/// <returns>-1 for no match, 0 for world match, 1 for parasang match, 2 for zone match.</returns>
	public static int Match(string ID, string Other)
	{
		int i = 0;
		int num = 0;
		int length = ID.Length;
		int length2 = Other.Length;
		for (int num2 = Mathf.Min(length, length2); i < num2 && ID[i] == Other[i]; i++)
		{
			if (ID[i] == '.')
			{
				num++;
			}
		}
		if (i == length && i == length2)
		{
			num++;
		}
		if (num <= 0)
		{
			return -1;
		}
		if (num <= 2)
		{
			return 0;
		}
		if (num <= 5)
		{
			return 1;
		}
		return 2;
	}

	public static string GetWorldID(string ID)
	{
		Parse(ID, out var World, out var _, out var _, out var _, out var _, out var _);
		return World;
	}
}
