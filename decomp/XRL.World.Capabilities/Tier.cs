using System;

namespace XRL.World.Capabilities;

public static class Tier
{
	public const int MINIMUM = 1;

	public const int MAXIMUM = 8;

	public static int Constrain(int Tier)
	{
		return Math.Min(Math.Max(Tier, 1), 8);
	}

	public static void Constrain(ref int Tier)
	{
		Tier = Constrain(Tier);
	}

	public static int Fuzz(int Tier, int Chance = 5)
	{
		while (Chance.in100())
		{
			Tier--;
		}
		Constrain(ref Tier);
		while (Chance.in100())
		{
			Tier++;
		}
		Constrain(ref Tier);
		return Tier;
	}

	public static void Fuzz(ref int Tier, int Chance = 5)
	{
		Tier = Fuzz(Tier);
	}
}
