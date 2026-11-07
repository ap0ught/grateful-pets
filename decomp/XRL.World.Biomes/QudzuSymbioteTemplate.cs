using XRL.Wish;
using XRL.World.Parts;

namespace XRL.World.Biomes;

[HasWishCommand]
public static class QudzuSymbioteTemplate
{
	public static void Apply(GameObject GO, Zone Z)
	{
		if (GO?.Render != null && GO.HasPart<Body>() && !GO.GetBlueprint().DescendsFrom("BaseRobot"))
		{
			GO.AddPart(new RustOnHit());
			GO.RequirePart<SocialRoles>().RequireRole("{{r|qudzu}} symbiote");
			GO.RequirePart<QudzuSymbioteIconColor>();
			if (GO.Brain != null && !Z.GetZoneProperty("relaxedbiomes").EqualsNoCase("true"))
			{
				GO.Brain.Allegiance["Vines"] = 100;
			}
		}
	}

	[WishCommand("qudzusymbiote", null)]
	public static void Wish(string Blueprint)
	{
		WishResult wishResult = WishSearcher.SearchForBlueprint(Blueprint);
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(wishResult.Result, 0, 0, null, null, null, "Wish");
		Apply(gameObject, The.ActiveZone);
		The.PlayerCell.getClosestEmptyCell().AddObject(gameObject);
	}
}
