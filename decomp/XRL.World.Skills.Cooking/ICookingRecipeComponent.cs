using System.Collections.Generic;

namespace XRL.World.Skills.Cooking;

public interface ICookingRecipeComponent : IComposite
{
	bool doesPlayerHaveEnough();

	string createPlayerDoesNotHaveEnoughMessage();

	void use(List<GameObject> used);

	string getDisplayName();

	bool HasPlants();

	bool HasFungi();

	int PlayerHolding();

	string getIngredientId();
}
