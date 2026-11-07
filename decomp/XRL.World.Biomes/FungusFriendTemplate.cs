using XRL.Wish;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Biomes;

[HasWishCommand]
public static class FungusFriendTemplate
{
	public static void Apply(GameObject GO, string InfectionBlueprint, Zone Z)
	{
		if (GO?.Render != null && GO.HasPart<Body>())
		{
			GO.RequirePart<SocialRoles>().RequireRole("friend to fungi");
			GO.RequirePart<FungiFriendIconColor>();
			if (33.in100())
			{
				FungalSporeInfection.ApplyFungalInfection(GO, InfectionBlueprint);
			}
			if (15.in100())
			{
				FungalSporeInfection.ApplyFungalInfection(GO, InfectionBlueprint);
			}
			if (!Z.GetZoneProperty("relaxedbiomes").EqualsNoCase("true") && GO.Brain != null)
			{
				GO.Brain.Allegiance["Fungi"] = 100;
			}
		}
	}

	[WishCommand("fungusfriend", null)]
	public static void Wish(string Blueprint)
	{
		WishResult wishResult = WishSearcher.SearchForBlueprint(Blueprint);
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(wishResult.Result, 0, 0, null, null, null, "Wish");
		Apply(gameObject, SporePuffer.InfectionObjectList.GetRandomElement(), The.ActiveZone);
		The.PlayerCell.getClosestEmptyCell().AddObject(gameObject);
	}
}
