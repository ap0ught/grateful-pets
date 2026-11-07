using XRL.Names;
using XRL.World.Parts;

namespace XRL.World.ObjectBuilders;

public class SnapjawHero0 : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		string text = NameMaker.MakeTitle(GO, null, null, null, null, null, null, null, null, null, "Hero", null, SpecialFaildown: true);
		GO.GiveProperName(null, Force: false, "Hero", SpecialFaildown: true);
		if (!text.IsNullOrEmpty())
		{
			GO.RequirePart<Titles>().AddTitle(text, -40);
		}
		GO.RequirePart<DisplayNameColor>().SetColorByPriority("M", 30);
		SnapjawHero.ApplySnapjawTraits(GO, text, Context);
		GO.ReceiveObjectFromPopulation("Melee Weapons 1", null, NoStack: false, 25, 0, null, null, null, Context);
		GO.ReceiveObjectFromPopulation("Armor 1", null, NoStack: false, 25, 0, null, null, null, Context);
		GO.GetStat("Hitpoints").BaseValue *= 2;
		GO.ReceiveObjectFromPopulation("Junk 1R", null, NoStack: false, 0, 0, null, null, null, Context);
		if (50.in100())
		{
			GO.ReceiveObjectFromPopulation("Junk 2", null, NoStack: false, 0, 0, null, null, null, Context);
		}
	}
}
