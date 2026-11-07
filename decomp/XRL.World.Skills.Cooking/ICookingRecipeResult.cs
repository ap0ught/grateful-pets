namespace XRL.World.Skills.Cooking;

public interface ICookingRecipeResult : IComposite
{
	string GetCampfireDescription();

	string apply(GameObject eater);
}
