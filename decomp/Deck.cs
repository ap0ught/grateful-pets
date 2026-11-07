using System;
using System.Collections.Generic;

public static class Deck
{
	public static IEnumerable<long> deal(long handSize, long deckSize, Random rng = null)
	{
		if (rng == null)
		{
			rng = new Random();
		}
		long j = -1L;
		long qu1 = -handSize + 1 + deckSize;
		long negalphainv = -13L;
		long threshold = -negalphainv * handSize;
		double nreal = handSize;
		double Nreal = deckSize;
		double num = 1.0 / (double)handSize;
		_ = 1.0 / (double)(handSize - 1);
		double Vprime = Math.Exp(Math.Log(rng.NextDouble()) * num);
		double qu1real = 0.0 - nreal + 1.0 + Nreal;
		while (handSize > 1 && threshold < deckSize)
		{
			double nmin1inv = 1.0 / (-1.0 + nreal);
			long S;
			double negSreal;
			while (true)
			{
				double num2 = Nreal * (0.0 - Vprime + 1.0);
				S = (long)Math.Floor(num2);
				if (S >= qu1)
				{
					Vprime = Math.Exp(Math.Log(rng.NextDouble()) * num);
					continue;
				}
				double num3 = rng.NextDouble();
				negSreal = -S;
				double num4 = Math.Exp(Math.Log(num3 * Nreal / qu1real) * nmin1inv);
				Vprime = num4 * ((0.0 - num2) / Nreal + 1.0) * (qu1real / (negSreal + qu1real));
				if (Vprime <= 1.0)
				{
					break;
				}
				double num5 = 1.0;
				double num6 = -1.0 + Nreal;
				double num7;
				long num8;
				if (-1 + handSize > S)
				{
					num7 = 0.0 - nreal + Nreal;
					num8 = -S + deckSize;
				}
				else
				{
					num7 = -1.0 + negSreal + Nreal;
					num8 = qu1;
				}
				for (long num9 = deckSize - 1; num9 >= num8; num9--)
				{
					num5 = num5 * num6 / num7;
					num6--;
					num7--;
				}
				if (Nreal / (0.0 - num2 + Nreal) >= num4 * Math.Exp(Math.Log(num5) * nmin1inv))
				{
					Vprime = Math.Exp(Math.Log(rng.NextDouble()) * nmin1inv);
					break;
				}
				Vprime = Math.Exp(Math.Log(rng.NextDouble()) * num);
			}
			j += S + 1;
			yield return j;
			deckSize = -S + (-1 + deckSize);
			Nreal = negSreal + (-1.0 + Nreal);
			handSize--;
			nreal--;
			num = nmin1inv;
			qu1 = -S + qu1;
			qu1real = negSreal + qu1real;
			threshold += negalphainv;
		}
		if (handSize > 1)
		{
			foreach (long item in vitter_a(handSize, deckSize, j, rng))
			{
				yield return item;
			}
		}
		else
		{
			long S = (long)Math.Floor((double)deckSize * Vprime);
			yield return j + (S + 1);
		}
	}

	public static IEnumerable<long> vitter_a(long n, long N, long j, Random rng)
	{
		double top = N - n;
		double Nreal = N;
		long num2;
		while (n >= 2)
		{
			double num = rng.NextDouble();
			num2 = 0L;
			for (double num3 = top / Nreal; num3 > num; num3 = num3 * top / Nreal)
			{
				num2++;
				top--;
				Nreal--;
			}
			j += num2 + 1;
			yield return j;
			Nreal--;
			n--;
		}
		num2 = (long)Math.Floor(Math.Round(Nreal) * rng.NextDouble());
		j += num2 + 1;
		yield return j;
	}
}
