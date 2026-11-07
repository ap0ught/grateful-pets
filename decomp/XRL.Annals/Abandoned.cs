using System;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class Abandoned : HistoricEvent
{
	public bool bVillageZero;

	public static readonly string[] Branches = new string[8] { "sacked", "sacked", "curse", "curse", "curse", "lose interest", "embrance profanity", "lose faith" };

	public Abandoned()
	{
		bVillageZero = false;
	}

	public Abandoned(bool VillageZero)
	{
		bVillageZero = VillageZero;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string value = Branches.GetRandomElement() switch
		{
			"sacked" => Grammar.InitCap(string.Format("In %{0}%, the village of {1} was {2} by {3}.|{4}", year, snapshotAtYear.GetProperty("name"), ExpandString("<spice.commonPhrases.sacked.!random>"), Faction.GetFormattedName(Factions.GetRandomFaction().Name), id)), 
			"curse" => Grammar.InitCap(string.Format("{0} {1} and the {2} village of {3}! Let no {4} {5} here {6}.|{7}", ExpandString("<spice.instancesOf.aCurseUpon.!random>"), snapshotAtYear.sacredThing, ExpandString("<spice.commonPhrases.despicable.!random>"), snapshotAtYear.GetProperty("name"), ExpandString("<spice.personNouns.!random>"), ExpandString("<spice.commonPhrases.settle.!random>"), ExpandString("<spice.instancesOf.forAllTime.!random>"), id)), 
			"lose interest" => Grammar.InitCap(string.Format("{0}, the villagers of {1} {2} {3}, and the village was {4}.|{5}", Grammar.InitCap(ExpandString("<spice.instancesOf.overTime.!random>")), snapshotAtYear.GetProperty("name"), ExpandString("<spice.instancesOf.lostInterestIn.!random>"), snapshotAtYear.sacredThing, ExpandString("<spice.commonPhrases.abandoned.!random>"), id)), 
			"embrance profanity" => Grammar.InitCap(string.Format("{0}, the {1} of {2} was {3} by the villagers of {4}. Having lost the cohesion of its communal principles, the village was {5} in %{6}%.|{7}", Grammar.InitCap(ExpandString("<spice.instancesOf.overTime.!random>")), ExpandString("<spice.commonPhrases.profanity.!random>"), snapshotAtYear.profaneThing, ExpandString("<spice.commonPhrases.embraced.!random>"), snapshotAtYear.GetProperty("name"), ExpandString("<spice.commonPhrases.abandoned.!random>"), year, id)), 
			_ => Grammar.InitCap(string.Format("{0}, the villagers of {1} {2} {3}, and the village was {4}.|{5}", Grammar.InitCap(ExpandString("<spice.instancesOf.overTime.!random>")), snapshotAtYear.GetProperty("name"), ExpandString("<spice.instancesOf.lostFaithIn.!random>"), snapshotAtYear.GetProperty("governor"), ExpandString("<spice.commonPhrases.abandoned.!random>"), id)), 
		};
		int num = int.Parse(history.GetEntitiesWithProperty("Resheph").GetRandomElement().GetCurrentSnapshot()
			.GetProperty("flipYear"));
		AddEntityListItem("Gospels", value);
		SetEntityProperty("abandoned", "true");
		SetEntityProperty("ruinScale", Math.Max(Math.Min((1000 + num - year) / 100, 4L), 1L).ToString());
	}
}
