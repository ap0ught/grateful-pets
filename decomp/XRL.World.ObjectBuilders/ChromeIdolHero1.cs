using System;
using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ObjectBuilders;

public class ChromeIdolHero1 : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		string text = NameMaker.MakeTitle(GO, null, null, null, null, null, null, null, null, null, "Hero", null, SpecialFaildown: true);
		GO.GiveProperName(null, Force: false, "Hero", SpecialFaildown: true);
		GO.RequirePart<DisplayNameColor>().SetColorByPriority("M", 30);
		string text2 = null;
		if (!text.IsNullOrEmpty())
		{
			int num = text.IndexOf(" of ");
			if (num != -1)
			{
				text2 = text.Substring(num + 4);
			}
			GO.RequirePart<Titles>().AddTitle(text, -40);
			if (text.Contains("revered") || text.Contains("venerated") || text.Contains("terrible"))
			{
				GO.BoostStat("Ego", Stat.Random(1, 3));
			}
			if (text.Contains("ancient") || text.Contains("corroded") || text.Contains("rusted"))
			{
				GO.BoostStat("Toughness", Stat.Random(1, 3));
			}
			if (text.Contains("joyous"))
			{
				GO.BoostStat("Willpower", Stat.Random(1, 3));
			}
		}
		if (!text2.IsNullOrEmpty())
		{
			string text3 = "NaphtaaliDeityMaxWorshipPower" + text2;
			int num2;
			if (The.Game.HasIntGameState(text3))
			{
				num2 = The.Game.GetIntGameState(text3);
			}
			else
			{
				num2 = Math.Max(Stat.Random(1, 20), Stat.Random(1, 5));
				The.Game.SetIntGameState(text3, num2);
			}
			GO.RequirePart<Shrine>();
			GO.SetStringProperty("Worshippable", "yes");
			GO.SetStringProperty("WorshippedAs", text2);
			GO.SetIntProperty("WorshipPower", Stat.Random(1, num2));
		}
		GO.ReceiveObjectFromPopulation("Melee Weapons 2", null, NoStack: false, 50, 0, null, null, null, Context);
		GO.ReceiveObjectFromPopulation("Armor 2", null, NoStack: false, 50, 0, null, null, null, Context);
		GO.ReceiveObjectFromPopulation("Junk 3R", null, NoStack: false, 0, 0, null, null, null, Context);
		GO.ReceiveObjectFromPopulation("Junk 3", null, NoStack: false, 0, 0, null, null, null, Context);
		GO.MultiplyStat("Hitpoints", 2);
	}
}
