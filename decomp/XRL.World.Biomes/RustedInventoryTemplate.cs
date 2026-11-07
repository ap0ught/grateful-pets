using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Biomes;

public static class RustedInventoryTemplate
{
	private static void ApplyMetalRust(GameObject Object)
	{
		if (Object.HasPart<Metal>() && 50.in100())
		{
			Object.ApplyEffect(new Rusted());
		}
	}

	public static void Apply(GameObject GO)
	{
		if (GO?.Render != null)
		{
			GO.Body?.ForeachEquippedObject(ApplyMetalRust);
			GO.Inventory?.ReverseForeachObject(ApplyMetalRust);
		}
	}
}
