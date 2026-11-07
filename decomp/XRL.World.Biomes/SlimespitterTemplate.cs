using XRL.Wish;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Biomes;

[HasWishCommand]
public static class SlimespitterTemplate
{
	public static void Apply(GameObject GO)
	{
		if (GO.IsCombatObject())
		{
			GO.Slimewalking = true;
			GO.RequirePart<DisplayNameAdjectives>().AddAdjective("slime-spitting");
			GO.RequirePart<SlimespitterIconColor>();
			if (!GO.HasPart<SlimeGlands>())
			{
				GO.RequirePart<Mutations>().AddMutation(new LiquidSpitter("slime"));
			}
		}
	}

	[WishCommand("slimespitter", null)]
	public static void Wish(string Blueprint)
	{
		WishResult wishResult = WishSearcher.SearchForBlueprint(Blueprint);
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(wishResult.Result, 0, 0, null, null, null, "Wish");
		Apply(gameObject);
		The.PlayerCell.getClosestEmptyCell().AddObject(gameObject);
	}
}
