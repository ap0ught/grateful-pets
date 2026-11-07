using System;
using XRL.Language;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ObjectBuilders;

[Serializable]
public class Tattooed : IObjectBuilder
{
	public bool CanTattoo;

	public bool CanEngrave;

	public override void Initialize()
	{
		CanTattoo = true;
		CanEngrave = true;
	}

	public override void Apply(GameObject Object, string Context)
	{
		string text = "LibraryCorpus.json";
		MarkovBook.EnsureCorpusLoaded(text);
		MarkovChainData data = MarkovBook.CorpusData[text];
		try
		{
			string text2 = Grammar.InitLower(Grammar.RemoveBadTitleEndingWords(MarkovChain.GenerateFragment(data, MarkovChain.GenerateSeedFromWord(data, Grammar.GetWeightedStartingArticle()), Stat.Random(1, 5))));
			if (!text2.IsNullOrEmpty())
			{
				Tattoos.ApplyTattoo(Object, CanTattoo, CanEngrave, text2);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("tattoo generation", x);
		}
	}
}
