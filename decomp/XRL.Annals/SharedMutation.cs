using System;
using HistoryKit;
using XRL.Language;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace XRL.Annals;

[Serializable]
public class SharedMutation : HistoricEvent
{
	public bool bVillageZero;

	public SharedMutation()
	{
		bVillageZero = false;
	}

	public SharedMutation(bool VillageZero)
	{
		bVillageZero = VillageZero;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string text = "";
		string randomProperty = QudHistoryHelpers.GetRandomProperty(snapshotAtYear, snapshotAtYear.GetProperty("defaultSacredThing"), "sacredThings");
		SetEntityProperty("villageGatherReason", "commune");
		while (true)
		{
			int num = Stat.Random(1, 1000);
			if (num <= 905)
			{
				int num2 = Stat.Random(1, 1000);
				BaseMutation randomMutation;
				if (num2 <= 600)
				{
					randomMutation = MutationFactory.GetRandomMutation("Physical");
					AddEntityListItem("sharedMutations", randomMutation.Name);
				}
				else if (num2 <= 900)
				{
					randomMutation = MutationFactory.GetRandomMutation("Mental");
					AddEntityListItem("sharedMutations", randomMutation.Name);
				}
				else if (num2 <= 960)
				{
					randomMutation = MutationFactory.GetRandomMutation("PhysicalDefects");
					AddEntityListItem("sharedMutations", randomMutation.Name);
				}
				else if (num2 <= 1000)
				{
					randomMutation = MutationFactory.GetRandomMutation("MentalDefects");
					AddEntityListItem("sharedMutations", randomMutation.Name);
				}
				else
				{
					randomMutation = MutationFactory.GetRandomMutation("Physical");
					AddEntityListItem("sharedMutations", randomMutation.Name);
				}
				if (randomMutation.Name == "Confusion" || randomMutation.Name == "Disintegration")
				{
					SetEntityProperty("highlyEntropicBeingWorshipAttitude", "50");
				}
				text = randomMutation.GetBearerDescription();
				try
				{
					Faction ifExists = Factions.GetIfExists("Entropic");
					if (ifExists != null && ifExists.PartReputation.TryGetValue(randomMutation.GetMutationEntry().Class, out var value))
					{
						SetEntityProperty("highlyEntropicBeingWorshipAttitude", value.ToString());
					}
				}
				catch (Exception)
				{
				}
				break;
			}
			if (num <= 995)
			{
				if (!bVillageZero)
				{
					if (If.CoinFlip())
					{
						AddEntityListItem("sharedDiseases", "Glotrot");
						text = "those afflicted with glotrot";
					}
					else
					{
						AddEntityListItem("sharedDiseases", "Ironshank");
						text = "those afflicted with ironshank";
					}
					break;
				}
				continue;
			}
			if (num <= 1000)
			{
				AddEntityListItem("sharedTransformations", "Mechanical");
				text = "automatons";
			}
			break;
		}
		string value2 = Stat.Random(1, 3) switch
		{
			1 => string.Format("Since the time of the {0} {1}, {2} have {3} inside the {4} walls of {5}.|{6}", ExpandString("<spice.adjectives.!random>"), ExpandString("<spice.personNouns.!random>"), text, ExpandString("<spice.commonPhrases.congregated.!random>"), ExpandString("<spice.commonPhrases.sacred.!random>"), snapshotAtYear.GetProperty("name"), id), 
			2 => string.Format("{0} are welcome in {1}. By the grace of {2}, {3}.|{4}", Grammar.InitCap(text), snapshotAtYear.GetProperty("name"), randomProperty, ExpandString("<spice.instancesOf.letItAlwaysBeSo.!random>"), id), 
			3 => string.Format("{4}! {0} {1} here. Be {2} to them, or may the {3} {5} take you.|{6}", Grammar.InitCap(text), ExpandString("<spice.commonPhrases.gather.!random>"), ExpandString("<spice.commonPhrases.kind.!random>"), ExpandString("<spice.adjectives.!random>"), Grammar.InitCap(ExpandString("<spice.commonPhrases.hark.!random>")), ExpandString("<spice.personNouns.!random>"), id), 
			_ => string.Format("Since the time of the {0} {1}, {2} have {3} inside the {4} walls of {5}.|{6}", ExpandString("<spice.adjectives.!random>"), ExpandString("<spice.personNouns.!random>"), text, ExpandString("<spice.commonPhrases.congregated.!random>"), ExpandString("<spice.commonPhrases.sacred.!random>"), snapshotAtYear.GetProperty("name"), id), 
		};
		AddEntityListItem("sacredThings", text);
		AddEntityListItem("Gospels", value2);
	}
}
