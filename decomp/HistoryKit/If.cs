using System;
using XRL.Rules;

namespace HistoryKit;

public class If
{
	public static bool CoinFlip(Action OnTrue = null, Action OnFalse = null)
	{
		if (Stat.Random(1, 100) <= 50)
		{
			OnTrue?.Invoke();
			return true;
		}
		OnFalse?.Invoke();
		return false;
	}

	public static bool Chance(string chance, Action OnTrue = null, Action OnFalse = null)
	{
		if (Stat.Chance(chance))
		{
			OnTrue?.Invoke();
			return true;
		}
		OnFalse?.Invoke();
		return false;
	}

	public static bool Chance(int chance, Action OnTrue = null, Action OnFalse = null)
	{
		if (Stat.Chance(chance))
		{
			OnTrue?.Invoke();
			return true;
		}
		OnFalse?.Invoke();
		return false;
	}

	public static bool OneIn(int chance, Action OnTrue = null, Action OnFalse = null)
	{
		if (Stat.Random(1, chance) == 1)
		{
			OnTrue?.Invoke();
			return true;
		}
		OnFalse?.Invoke();
		return false;
	}

	public static bool d100(int amount, Action OnTrue = null, Action OnFalse = null)
	{
		if (Stat.Random(1, 100) <= amount)
		{
			OnTrue?.Invoke();
			return true;
		}
		OnFalse?.Invoke();
		return false;
	}
}
