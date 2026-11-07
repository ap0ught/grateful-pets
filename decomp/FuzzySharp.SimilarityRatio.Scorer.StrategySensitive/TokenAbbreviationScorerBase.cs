using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FuzzySharp.Utils;

namespace FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

public abstract class TokenAbbreviationScorerBase : StrategySensitiveScorerBase
{
	public override int Score(string input1, string input2)
	{
		string text;
		string text2;
		if (input1.Length < input2.Length)
		{
			text = input1;
			text2 = input2;
		}
		else
		{
			text = input2;
			text2 = input1;
		}
		if ((double)text2.Length / (double)text.Length < 1.5)
		{
			return 0;
		}
		string[] array = (from Match m in Regex.Matches(text2, "[a-zA-Z]+")
			select m.Value).ToArray();
		string[] array2 = (from Match m in Regex.Matches(text, "[a-zA-Z]+")
			select m.Value).ToArray();
		if (array2.Length > 4)
		{
			return 0;
		}
		string[] seed;
		string[] array3;
		if (array.Length > array2.Length)
		{
			seed = array;
			array3 = array2;
		}
		else
		{
			seed = array2;
			array3 = array;
		}
		List<List<string>> list = seed.PermutationsOfSize(array3.Length);
		List<int> list2 = new List<int>();
		foreach (List<string> item in list)
		{
			double num = 0.0;
			for (int num2 = 0; num2 < array3.Length; num2++)
			{
				string text3 = item[num2];
				string text4 = array3[num2];
				if (StringContainsInOrder(text3, text4))
				{
					int num3 = Scorer(text3, text4);
					num += (double)num3;
				}
			}
			list2.Add((int)(num / (double)array3.Length));
		}
		if (list2.Count != 0)
		{
			return list2.Max();
		}
		return 0;
	}

	private bool StringContainsInOrder(string s1, string s2)
	{
		if (s1.Length < s2.Length)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < s1.Length; i++)
		{
			if (s2[num] == s1[i])
			{
				num++;
			}
			if (num == s2.Length)
			{
				return true;
			}
			if (i + s2.Length - num == s1.Length)
			{
				return false;
			}
		}
		return false;
	}
}
