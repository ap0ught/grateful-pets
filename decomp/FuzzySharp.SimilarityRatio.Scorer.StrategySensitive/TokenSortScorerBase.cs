using System.Linq;
using System.Text.RegularExpressions;

namespace FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

public abstract class TokenSortScorerBase : StrategySensitiveScorerBase
{
	public override int Score(string input1, string input2)
	{
		string arg = string.Join(" ", from s in Regex.Split(input1, "\\s+")
			where s.Any()
			orderby s
			select s).Trim();
		string arg2 = string.Join(" ", from s in Regex.Split(input2, "\\s+")
			where s.Any()
			orderby s
			select s).Trim();
		return Scorer(arg, arg2);
	}
}
