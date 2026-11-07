using XRL.Wish;
using XRL.World.Parts;

namespace XRL.World.Biomes;

[HasWishCommand]
public static class SlimewalkerTemplate
{
	public static void Apply(GameObject GO)
	{
		if (GO.IsCombatObject())
		{
			GO.Slimewalking = true;
			if (GO.HasBodyPart("Foot") || GO.HasBodyPart("Feet"))
			{
				GO.RequirePart<DisplayNameAdjectives>().AddAdjective("web-toed");
			}
			else
			{
				GO.RequirePart<DisplayNameAdjectives>().AddAdjective("slimy-finned");
			}
			GO.RequirePart<SlimewalkerIconColor>();
		}
	}

	[WishCommand("slimewalker", null)]
	public static void Wish(string Blueprint)
	{
		WishResult wishResult = WishSearcher.SearchForBlueprint(Blueprint);
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(wishResult.Result, 0, 0, null, null, null, "Wish");
		Apply(gameObject);
		The.PlayerCell.getClosestEmptyCell().AddObject(gameObject);
	}
}
