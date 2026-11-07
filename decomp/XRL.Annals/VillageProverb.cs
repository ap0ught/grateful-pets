using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;

namespace XRL.Annals;

[Serializable]
public class VillageProverb : HistoricEvent
{
	private string SpiceElement;

	private string SacredThing;

	private string ProfaneThing;

	private int Chance;

	public VillageProverb(string SpiceElement = "proverbs", string SacredThing = null, string ProfaneThing = null, int Chance = 50)
	{
		this.SpiceElement = SpiceElement;
		this.SacredThing = SacredThing;
		this.ProfaneThing = ProfaneThing;
		this.Chance = Chance;
	}

	public override void Generate()
	{
		duration = 0L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		if (snapshotAtYear == null)
		{
			throw new Exception("Cannot load snapshot for year " + entity.lastYear);
		}
		if (Random(0, 100) <= Chance)
		{
			Dictionary<string, string> dictionary = QudHistoryHelpers.BuildContextFromMemberTextFragments(entity, Throw: true);
			dictionary.Add("*sacredThing*", SacredThing.Coalesce(snapshotAtYear.sacredThing));
			dictionary.Add("*profaneThing*", ProfaneThing.Coalesce(snapshotAtYear.profaneThing));
			string word = Grammar.ConvertAtoAn(HistoricStringExpander.ExpandString("<spice." + SpiceElement + ".!random.capitalize>.", null, null, dictionary));
			SetEntityProperty("proverb", Grammar.InitCap(word));
		}
		else
		{
			SetEntityProperty("proverb", "Live and drink.");
		}
	}
}
