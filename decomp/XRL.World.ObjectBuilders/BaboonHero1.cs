using System.Collections.Generic;
using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.ObjectBuilders;

public class BaboonHero1 : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		int num = Stat.Roll("1d10+2");
		Dictionary<string, string> dictionary = new Dictionary<string, string> { 
		{
			"*Rings*",
			num + "-ringed"
		} };
		string text = NameMaker.MakeTitle(GO, null, null, null, null, null, null, null, null, null, "Hero", dictionary, SpecialFaildown: true);
		Dictionary<string, string> namingContext = dictionary;
		GO.GiveProperName(null, Force: false, "Hero", SpecialFaildown: true, null, null, namingContext);
		GO.RequirePart<DisplayNameColor>().SetColorByPriority("M", 30);
		if (!text.IsNullOrEmpty())
		{
			GO.RequirePart<Titles>().AddTitle(text, -40);
			if (text.Contains("King") || text.Contains("Queen"))
			{
				num += Stat.Random(1, 4);
				GO.RequirePart<Mutations>().AddMutation(new MultipleArms(), 2);
				GO.ReceiveObjectFromPopulation("Melee Weapons 2", null, NoStack: false, 50);
				GO.ReceiveObjectFromPopulation("Melee Weapons 2");
				GO.ReceiveObjectFromPopulation("Melee Weapons 1", null, NoStack: false, 50);
				GO.ReceiveObjectFromPopulation("Melee Weapons 1");
			}
			if (text.Contains("Philanderer") && GO.HasPart<BaboonHero1Pack>())
			{
				GO.GetPart<BaboonHero1Pack>().FollowerMultiplier = 2.5f;
			}
			if (text.Contains("Riddler"))
			{
				GO.RequirePart<Mutations>().AddMutation(new Confusion(), 2);
			}
			if (text.Contains("Hermit"))
			{
				GO.MultiplyStat("Hitpoints", 2);
				GO.RemovePart<BaboonHero1Pack>();
			}
			if (text.Contains("Sophisticate"))
			{
				GO.BoostStat("Intelligence", Stat.Random(3, 6));
				if (GO.HasPart<BaboonHero1Pack>())
				{
					GO.GetPart<BaboonHero1Pack>().Hat = true;
				}
			}
		}
		GO.ReceiveObjectFromPopulation("Junk 2R");
		GO.ReceiveObjectFromPopulation("Junk 3");
		GO.MultiplyStat("Hitpoints", 2);
		GO.AddBaseStat("Strength", Stat.Roll(1, 4) + num);
		GO.AddBaseStat("Agility", Stat.Roll(1, 4) + num);
		GO.AddBaseStat("Intelligence", Stat.Roll(1, 4));
		GO.AddBaseStat("Ego", Stat.Roll(1, 4));
		GO.AddBaseStat("Willpower", Stat.Roll(1, 4));
		GO.AddBaseStat("Toughness", Stat.Roll(1, 4));
		GO.AddBaseStat("XPValue", num * 75);
		for (int i = 0; i < num; i++)
		{
			GO.AddBaseStat("Hitpoints", Stat.Random(1, 10));
		}
	}
}
