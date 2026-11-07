using System;

internal static class Twister
{
	private const uint N = 624u;

	private const uint M = 397u;

	private const uint K = 2567483615u;

	private static uint[] state = new uint[625];

	private static uint nextRand;

	private static int left = -1;

	private static uint HighestBit(uint u)
	{
		return u & 0x80000000u;
	}

	private static uint LowestBit(uint u)
	{
		return u & 1;
	}

	private static uint LowestBits(uint u)
	{
		return u & 0x7FFFFFFF;
	}

	private static uint MixBits(uint u, uint v)
	{
		return HighestBit(u) | LowestBits(v);
	}

	public static void Seed(uint seed)
	{
		uint num = (seed | 1) & 0xFFFFFFFFu;
		uint[] array = state;
		int num2 = 0;
		left = 0;
		array[num2++] = num;
		int num3 = 624;
		while (Convert.ToBoolean(--num3))
		{
			array[num2++] = (num *= 69069) & 0xFFFFFFFFu;
		}
	}

	private static uint Reload()
	{
		uint num = 0u;
		uint num2 = 2u;
		uint num3 = 397u;
		if (left < -1)
		{
			Seed(4357u);
		}
		left = 623;
		nextRand = state[1];
		uint u = state[0];
		uint num4 = state[1];
		int num5 = 228;
		while (Convert.ToBoolean(--num5))
		{
			state[num++] = state[num3++] ^ (MixBits(u, num4) >> 1) ^ (uint)(Convert.ToBoolean(LowestBit(num4)) ? (-1727483681) : 0);
			u = num4;
			num4 = state[num2++];
		}
		num3 = 0u;
		num5 = 397;
		while (Convert.ToBoolean(--num5))
		{
			state[num++] = state[num3++] ^ (MixBits(u, num4) >> 1) ^ (uint)(Convert.ToBoolean(LowestBit(num4)) ? (-1727483681) : 0);
			u = num4;
			num4 = state[num2++];
		}
		num4 = state[0];
		state[num] = state[num3] ^ (MixBits(u, num4) >> 1) ^ (uint)(Convert.ToBoolean(LowestBit(num4)) ? (-1727483681) : 0);
		num4 ^= num4 >> 11;
		num4 ^= (num4 << 7) & 0x9D2C5680u;
		num4 ^= (num4 << 15) & 0xEFC60000u;
		return num4 ^ (num4 >> 18);
	}

	public static float NextDouble()
	{
		return (float)Random() / 4.2949673E+09f;
	}

	public static uint Random()
	{
		if (--left < 0)
		{
			return Reload();
		}
		uint num = nextRand++;
		uint num2 = num ^ (num >> 11);
		uint num3 = num2 ^ ((num2 << 7) & 0x9D2C5680u);
		uint num4 = num3 ^ ((num3 << 15) & 0xEFC60000u);
		return num4 ^ (num4 >> 18);
	}
}
