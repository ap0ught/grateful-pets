using System.Collections.Generic;
using XRL.Names;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.ObjectBuilders;

public class EyelessKingCrabHero1 : IObjectBuilder
{
	public override void Apply(GameObject Object, string Context)
	{
		string text = NameMaker.MakeTitle(Object, null, null, null, null, null, null, null, null, null, "Hero", null, SpecialFaildown: true);
		Object.GiveProperName(null, Force: false, "Hero", SpecialFaildown: true);
		Object.RequirePart<DisplayNameColor>().SetColorByPriority("M", 30);
		if (!text.IsNullOrEmpty())
		{
			Object.RequirePart<Titles>().AddTitle(text, -40);
			if (text.Contains("many-legged"))
			{
				Object.RequirePart<Mutations>().AddMutation(new MultipleLegs(), 5);
			}
			if (text.Contains("one-clawed"))
			{
				Body body = Object.Body;
				if (body != null)
				{
					List<BodyPart> part = body.GetPart("Hand");
					if (part != null)
					{
						part.GetRandomElement()?.Dismember()?.Destroy();
					}
				}
			}
			if (text.Contains("massive"))
			{
				Object.AddBaseStat("MoveSpeed", Stat.Random(10, 20));
				Object.AddBaseStat("Hitpoints", Stat.Random(10, 20));
			}
			if (text.Contains("echoing"))
			{
				Object.BoostStat("Ego", Stat.Random(1, 3));
			}
			if (text.Contains("frenetic"))
			{
				Object.AddBaseStat("Speed", Stat.Random(15, 30));
			}
			if (text.Contains("shell-cracked"))
			{
				Object.AddBaseStat("AV", -1);
				Object.AddBaseStat("Hitpoints", Stat.Random(20, 30));
			}
			if (text.Contains("Skuttler"))
			{
				Object.AddBaseStat("MoveSpeed", -Stat.Random(10, 20));
			}
			if (text.Contains("Ancient"))
			{
				Object.AddBaseStat("AV", 2);
			}
			if (text.Contains("Deepcrawler"))
			{
				Object.RequirePart<Mutations>().AddMutation(new BurrowingClaws(), 5);
			}
			if (text.Contains("Goliath"))
			{
				Object.AddBaseStat("AV", 1);
				Object.AddBaseStat("MoveSpeed", Stat.Random(10, 20));
				Object.AddBaseStat("Hitpoints", Stat.Random(30, 50));
			}
			if (text.Contains("Lord") || text.Contains("Patriarch"))
			{
				Object.BoostStat("Strength", Stat.Random(1, 3));
				Object.BoostStat("Ego", Stat.Random(1, 3));
				Object.BoostStat("Willpower", Stat.Random(1, 3));
				Object.AddBaseStat("Hitpoints", Stat.Random(10, 20));
			}
		}
		Object.ReceiveObjectFromPopulation("Melee Weapons 2", null, NoStack: false, 50, 0, null, null, null, Context);
		Object.ReceiveObjectFromPopulation("Armor 2", null, NoStack: false, 50, 0, null, null, null, Context);
		Object.ReceiveObjectFromPopulation("Junk 4R", null, NoStack: false, 0, 0, null, null, null, Context);
		Object.ReceiveObjectFromPopulation("Junk 5", null, NoStack: false, 0, 0, null, null, null, Context);
		Object.MultiplyStat("Hitpoints", 2);
	}
}
