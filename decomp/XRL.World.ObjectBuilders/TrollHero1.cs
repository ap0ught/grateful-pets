using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.ObjectBuilders;

public class TrollHero1 : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		string text = NameMaker.MakeTitle(GO, null, null, null, null, null, null, null, null, null, "Hero", null, SpecialFaildown: true);
		GO.GiveProperName(null, Force: false, "Hero", SpecialFaildown: true);
		GO.RequirePart<DisplayNameColor>().SetColorByPriority("M", 30);
		if (!text.IsNullOrEmpty())
		{
			GO.RequirePart<Titles>().AddTitle(text, -40);
			if (text.Contains("frenetic"))
			{
				GO.AddBaseStat("Speed", Stat.Random(25, 50));
			}
			if (text.Contains("learned"))
			{
				GO.BoostStat("Intelligence", Stat.Random(1, 3));
			}
			if (text.Contains("everlasting"))
			{
				GO.AddBaseStat("Hitpoints", Stat.Random(40, 60));
			}
			if (text.Contains("bestial"))
			{
				GO.BoostStat("Ego", Stat.Random(1, 3));
			}
			if (text.Contains("bestial"))
			{
				GO.BoostStat("Ego", Stat.Random(1, 3));
			}
			if (text.Contains("bloodthirsty"))
			{
				GO.BoostStat("Agility", Stat.Random(1, 3));
			}
			if (text.Contains("hulking"))
			{
				GO.BoostStat("Strength", Stat.Random(1, 3));
			}
			if (text.Contains("rubberhide"))
			{
				GO.AddBaseStat("AV", 4);
			}
			if (text.Contains("Skull-collector"))
			{
				GO.ReceiveObject(GameObject.Create("Battle Axe4", 0, 0, null, null, null, Context));
				if (!GO.HasSkill("Axe_Dismember"))
				{
					GO.AddSkill("Axe_Dismember");
				}
			}
			if (text.Contains("Heart-eater"))
			{
				GO.BoostStat("Strength", Stat.Random(1, 3));
			}
			if (text.Contains("Hunt-master"))
			{
				GO.AddBaseStat("DV", 4);
			}
			if (text.Contains("Man-eater"))
			{
				GO.BoostStat("Agility", Stat.Random(1, 3));
			}
			if (text.Contains("two-headed"))
			{
				GO.RequirePart<Mutations>().AddMutation(new TwoHeaded(), 2);
			}
			if (text.Contains("Caveking"))
			{
				GO.BoostStat("Strength", Stat.Random(1, 3));
				GO.BoostStat("Ego", Stat.Random(1, 3));
				GO.BoostStat("Willpower", Stat.Random(1, 3));
				GO.AddBaseStat("Hitponts", Stat.Random(10, 20));
			}
		}
		GO.ReceiveObjectFromPopulation("Armor 2", null, NoStack: false, 50, 0, null, null, null, Context);
		GO.ReceiveObjectFromPopulation("Junk 4R", null, NoStack: false, 0, 0, null, null, null, Context);
		GO.ReceiveObjectFromPopulation("Junk 4", null, NoStack: false, 0, 0, null, null, null, Context);
	}
}
