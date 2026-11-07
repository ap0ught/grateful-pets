using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World.ObjectBuilders;

public static class SnapjawHero
{
	public static void ApplySnapjawTraits(GameObject GO, string title, string Context = null)
	{
		if (!title.IsNullOrEmpty())
		{
			if (title.Contains("fleet-footed"))
			{
				GO.AddBaseStat("MoveSpeed", -Stat.Random(25, 50));
			}
			if (title.Contains("learned"))
			{
				GO.BoostStat("Intelligence", Stat.Random(1, 3));
			}
			if (title.Contains("stalwart"))
			{
				GO.AddBaseStat("Hitpoints", Stat.Random(10, 20));
			}
			if (title.Contains("fearsome"))
			{
				GO.BoostStat("Ego", Stat.Random(1, 3));
			}
			if (title.Contains("nimble"))
			{
				GO.AddBaseStat("DV", Stat.Random(3, 6));
			}
			if (title.Contains("hulking"))
			{
				GO.BoostStat("Strength", Stat.Random(1, 3));
			}
			if (title.Contains("calloused") && !GO.HasSkill("Endurance_Calloused"))
			{
				GO.AddSkill("Endurance_Calloused");
			}
			if (title.Contains("Skullsplitter"))
			{
				GO.ReceiveObject(GameObject.Create("Steel Battle Axe", 0, 0, null, null, null, Context));
				XRL.World.Parts.Skills part = GO.GetPart<XRL.World.Parts.Skills>();
				part.AddSkill(new Axe());
				part.AddSkill(new Axe_Dismember());
			}
			if (title.Contains("Firesnarler"))
			{
				GO.GetPart<Mutations>().AddMutation(new Pyrokinesis(), 2);
			}
			if (title.Contains("Bear-baiter"))
			{
				GO.AddBaseStat("DV", 1);
			}
			if (title.Contains("Tot-eater"))
			{
				GO.BoostStat("Agility", Stat.Random(1, 3));
			}
			if (title.Contains("Gutspiller"))
			{
				GO.RequirePart<Mutations>().AddMutation(new Horns(), 2);
			}
			if (title.Contains("King"))
			{
				GO.BoostStat("Strength", Stat.Random(1, 3));
				GO.BoostStat("Ego", Stat.Random(1, 3));
				GO.BoostStat("Willpower", Stat.Random(1, 3));
				GO.AddBaseStat("Hitpoints", Stat.Random(10, 20));
			}
		}
	}
}
