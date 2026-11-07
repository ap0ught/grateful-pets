using System;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
[Obsolete]
public class Tattooed : IPart
{
	public int ChanceOneIn = 10000;

	public bool CanTattoo = true;

	public bool CanEngrave = true;

	public override bool SameAs(IPart p)
	{
		Tattooed tattooed = p as Tattooed;
		if (tattooed.ChanceOneIn != ChanceOneIn)
		{
			return false;
		}
		if (tattooed.CanTattoo != CanTattoo)
		{
			return false;
		}
		if (tattooed.CanEngrave != CanEngrave)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Stat.Random(1, ChanceOneIn) == 1)
		{
			string text = "LibraryCorpus.json";
			MarkovBook.EnsureCorpusLoaded(text);
			MarkovChainData data = MarkovBook.CorpusData[text];
			try
			{
				string text2 = Grammar.InitLower(Grammar.RemoveBadTitleEndingWords(MarkovChain.GenerateFragment(data, MarkovChain.GenerateSeedFromWord(data, Grammar.GetWeightedStartingArticle()), Stat.Random(1, 5))));
				if (!text2.IsNullOrEmpty())
				{
					Tattoos.ApplyTattoo(ParentObject, CanTattoo, CanEngrave, text2);
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("tattoo generation", x);
			}
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}
