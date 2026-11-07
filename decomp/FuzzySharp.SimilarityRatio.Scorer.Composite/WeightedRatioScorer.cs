using System;
using System.Linq;

namespace FuzzySharp.SimilarityRatio.Scorer.Composite;

public class WeightedRatioScorer : ScorerBase
{
	private static double UNBASE_SCALE = 0.95;

	private static double PARTIAL_SCALE = 0.9;

	private static bool TRY_PARTIALS = true;

	public override int Score(string input1, string input2)
	{
		int length = input1.Length;
		int length2 = input2.Length;
		if (length == 0 || length2 == 0)
		{
			return 0;
		}
		bool flag = TRY_PARTIALS;
		double uNBASE_SCALE = UNBASE_SCALE;
		double num = PARTIAL_SCALE;
		int num2 = Fuzz.Ratio(input1, input2);
		double num3 = (double)Math.Max(length, length2) / (double)Math.Min(length, length2);
		if (num3 < 1.5)
		{
			flag = false;
		}
		if (num3 > 8.0)
		{
			num = 0.6;
		}
		if (flag)
		{
			double num4 = (double)Fuzz.PartialRatio(input1, input2) * num;
			double num5 = (double)Fuzz.TokenSortRatio(input1, input2) * uNBASE_SCALE * num;
			double num6 = (double)Fuzz.TokenSetRatio(input1, input2) * uNBASE_SCALE * num;
			return (int)Math.Round(new double[4] { num2, num4, num5, num6 }.Max());
		}
		double num7 = (double)Fuzz.TokenSortRatio(input1, input2) * uNBASE_SCALE;
		double num8 = (double)Fuzz.TokenSetRatio(input1, input2) * uNBASE_SCALE;
		return (int)Math.Round(new double[3] { num2, num7, num8 }.Max());
	}
}
