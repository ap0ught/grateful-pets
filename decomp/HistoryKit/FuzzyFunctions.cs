using System;
using XRL.Rules;

namespace HistoryKit;

public class FuzzyFunctions
{
	public static Func<T> DoThisButRarelyDoThat<T>(Func<T> Primary, Func<T> Secondary, string Chance)
	{
		if (If.Chance(Chance))
		{
			return Secondary;
		}
		return Primary;
	}

	public static Func<T> DoThisButRarelyDoThat<T>(Func<T> Primary, Func<T> Secondary, int ChanceOneIn)
	{
		if (Stat.Random(1, ChanceOneIn) == 1)
		{
			return Secondary;
		}
		return Primary;
	}

	public static void DoThisAndThatInRandomOrder(Action This, Action That)
	{
		If.CoinFlip(() =>
		{
			This();
			That();
		}, () =>
		{
			This();
			That();
		});
	}
}
