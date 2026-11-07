using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

public abstract class TokenSetScorerBase : StrategySensitiveScorerBase
{
	public override int Score(string input1, string input2)
	{
		HashSet<string> hashSet = new HashSet<string>(from s in Regex.Split(input1, "\\s+")
			where s.Any()
			select s);
		HashSet<string> hashSet2 = new HashSet<string>(from s in Regex.Split(input2, "\\s+")
			where s.Any()
			select s);
		string text = string.Join(" ", from s in hashSet.Intersect(hashSet2)
			orderby s
			select s).Trim();
		string text2 = (text + " " + string.Join(" ", from s in hashSet.Except(hashSet2)
			orderby s
			select s)).Trim();
		string arg = (text + " " + string.Join(" ", from s in hashSet2.Except(hashSet)
			orderby s
			select s)).Trim();
		return new int[3]
		{
			Scorer(text, text2),
			Scorer(text, arg),
			Scorer(text2, arg)
		}.Max();
	}
}
