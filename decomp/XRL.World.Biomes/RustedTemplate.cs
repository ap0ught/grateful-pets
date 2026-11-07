using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Biomes;

public static class RustedTemplate
{
	public static void Apply(GameObject GO)
	{
		if (GO.HasPart<Metal>())
		{
			GO.ApplyEffect(new Rusted());
		}
	}
}
