using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using XRL.Language;
using XRL.Rules;

namespace XRL;

public static class MarkovChain
{
	public static MarkovChainData BuildChain(string Corpus, int order)
	{
		MarkovChainData markovChainData = new MarkovChainData();
		markovChainData.order = order;
		string[] array = Corpus.Split(' ');
		bool flag = false;
		for (int i = 0; i < array.Length - order; i++)
		{
			string text = array[i];
			text = text.TrimStart(' ');
			text = text.TrimEnd(' ');
			string text2 = ((i != 0) ? array[i - 1] : " ");
			if (text2 == "")
			{
				text2 = " ";
			}
			if (text2[Math.Max(text2.Length - 1, 0)] == '.')
			{
				flag = true;
			}
			for (int j = 1; j < order; j++)
			{
				text = text + " " + array[i + j];
			}
			if (flag)
			{
				markovChainData.OpeningWords.Add(text);
				flag = false;
			}
			if (markovChainData.Chain.ContainsKey(text))
			{
				markovChainData.Chain[text].Add(array[i + order]);
				continue;
			}
			markovChainData.Chain[text] = new List<string> { array[i + order] };
		}
		return markovChainData;
	}

	public static MarkovChainData AppendCorpus(MarkovChainData Data, string Corpus, bool addOpeningWords = true)
	{
		string[] array = Corpus.Split(' ');
		bool flag = false;
		for (int i = 0; i < array.Length - Data.order; i++)
		{
			string text = array[i];
			text = text.TrimStart(' ');
			text = text.TrimEnd(' ');
			string text2 = ((i != 0) ? array[i - 1] : " ");
			if (text2 == "")
			{
				text2 = " ";
			}
			if (text2[Math.Max(text2.Length - 1, 0)] == '.')
			{
				flag = true;
			}
			for (int j = 1; j < Data.order; j++)
			{
				text = text + " " + array[i + j];
			}
			if (flag & addOpeningWords)
			{
				Data.OpeningWords.Add(text);
				flag = false;
			}
			if (Data.Chain.ContainsKey(text))
			{
				Data.Chain[text].Add(array[i + Data.order]);
				continue;
			}
			Data.Chain[text] = new List<string> { array[i + Data.order] };
		}
		return Data;
	}

	public static MarkovChainData AppendSecret(MarkovChainData Data, string Secret, bool addOpeningWords = false)
	{
		string text = "";
		Match match = Regex.Match(Secret, "{.*?}");
		if (match != null && !string.IsNullOrEmpty(match.Value))
		{
			text = match.Groups[0].Value;
			Secret = Secret.Replace(text, "{");
		}
		string[] array = Secret.Split(' ');
		bool flag = false;
		for (int i = 0; i < array.Length - Data.order; i++)
		{
			string text2 = array[i];
			text2 = text2.TrimStart(' ');
			text2 = text2.TrimEnd(' ');
			string text3 = ((i != 0) ? array[i - 1] : " ");
			if (text3 == "")
			{
				text3 = " ";
			}
			if (text3[Math.Max(text3.Length - 1, 0)] == '.')
			{
				flag = true;
			}
			for (int j = 1; j < Data.order; j++)
			{
				text2 = text2 + " " + array[i + j];
			}
			if (flag & addOpeningWords)
			{
				Data.OpeningWords.Add(text2);
				flag = false;
			}
			if (Data.Chain.ContainsKey(text2))
			{
				Data.Chain[text2].Add(array[i + Data.order].Replace("{", text).Replace("{", "").Replace("}", ""));
				continue;
			}
			Data.Chain[text2] = new List<string> { array[i + Data.order].Replace("{", text).Replace("{", "").Replace("}", "") };
		}
		return Data;
	}

	public static string GenerateSentence(MarkovChainData Data, string seed = null)
	{
		if (string.IsNullOrEmpty(seed))
		{
			seed = Data.OpeningWords[Stat.Random(0, Data.OpeningWords.Count - 1)];
		}
		List<string> list = new List<string>();
		for (int i = 0; i < Data.order; i++)
		{
			list.Add(seed.Split(' ')[i]);
		}
		for (int j = 0; j < 100; j++)
		{
			string text = list[j];
			for (int k = 1; k < Data.order; k++)
			{
				text = text + " " + list[j + k];
			}
			try
			{
				string text2 = Data.Chain[text][Stat.Random(0, Data.Chain[text].Count - 1)];
				list.Add(text2);
				if (text2.Contains("."))
				{
					return string.Join(" ", list.ToArray()) + " ";
				}
			}
			catch
			{
				Debug.Log("Error on phrase: " + text);
				if (!Data.Chain.ContainsKey(text))
				{
					Debug.Log("Phrase not found in Data.Chain");
				}
				else
				{
					Debug.Log("Data was " + Data.Chain[text]);
				}
				throw;
			}
		}
		return GenerateSentence(Data);
	}

	public static string GenerateShortSentence(MarkovChainData Data, string seed = null, int maxWords = 18)
	{
		int num = 50;
		string text;
		do
		{
			text = GenerateSentence(Data, seed);
			num--;
		}
		while (text.Split(' ').Length > maxWords && num > 0);
		return text;
	}

	public static string GenerateParagraph(MarkovChainData Data)
	{
		string text = "";
		int num = Stat.Random(3, 6);
		for (int i = 0; i < num; i++)
		{
			text += GenerateSentence(Data);
		}
		return text + "\n\n";
	}

	public static string GenerateTitle(MarkovChainData Data)
	{
		string text = "";
		string text2 = "";
		do
		{
			string seed = Data.Chain.Keys.ToArray()[Stat.Random(0, Data.Chain.Keys.Count - 1)];
			text2 = GenerateFragment(Data, seed, Stat.Random(1, 5));
			while (string.IsNullOrEmpty(text2))
			{
				text2 = GenerateFragment(Data, seed = Data.Chain.Keys.ToArray()[Stat.Random(0, Data.Chain.Keys.Count - 1)], Stat.Random(1, 5));
			}
			text = Grammar.MakeTitleCase(Grammar.RemoveBadTitleStartingWords(Grammar.RemoveBadTitleEndingWords(text2)));
		}
		while (string.IsNullOrEmpty(text) || (text.Distinct().Count() == 1 && text.Contains(' ')));
		return text;
	}

	public static string GenerateFragment(MarkovChainData Data, string Seed, int Length)
	{
		if (Seed.Contains("."))
		{
			return null;
		}
		string text = Seed;
		string text2 = "";
		for (int i = 0; i < Length; i++)
		{
			if (Data.Chain.ContainsKey(Seed))
			{
				text2 = Data.Chain[Seed][Stat.Random(0, Data.Chain[Seed].Count - 1)];
			}
			text += " ";
			text += text2;
			if (text2.Contains("."))
			{
				break;
			}
			Seed = Seed + " " + text2;
			Seed = Seed.Substring(Seed.Split(' ')[0].Length).TrimStart(' ');
		}
		return text.Replace(",", "").Replace(".", "").Replace("\"", "")
			.Replace("(", "")
			.Replace(")", "")
			.Replace("\r", "");
	}

	public static string GenerateSeedFromWord(MarkovChainData Data, string word)
	{
		string[] array = Data.Chain.Keys.ToArray();
		int num = array.Length;
		int num2 = Stat.Random(0, num - 1);
		while (!array[num2].StartsWith(word + " ") && !(word == "0"))
		{
			num2++;
			if (num2 >= num)
			{
				num2 = 0;
			}
		}
		return array[num2];
	}

	public static int Count(MarkovChainData Data)
	{
		int num = 0;
		foreach (List<string> value in Data.Chain.Values)
		{
			num += value.Count;
		}
		return num;
	}

	public static int[] GetMostCommonPhrases(MarkovChainData Data)
	{
		int[] array = new int[3];
		string[] array2 = new string[3] { "", "", "" };
		foreach (string key in Data.Chain.Keys)
		{
			if (Data.Chain[key].Count > array[0])
			{
				array[2] = array[1];
				array[1] = array[0];
				array[0] = Data.Chain[key].Count;
				array2[2] = array2[1];
				array2[1] = array2[0];
				array2[0] = key;
			}
			else if (Data.Chain[key].Count > array[1])
			{
				array[2] = array[1];
				array[1] = Data.Chain[key].Count;
				array2[2] = array2[1];
				array2[1] = key;
			}
			else if (Data.Chain[key].Count > array[2])
			{
				array[2] = Data.Chain[key].Count;
				array2[2] = key;
			}
		}
		return array;
	}
}
