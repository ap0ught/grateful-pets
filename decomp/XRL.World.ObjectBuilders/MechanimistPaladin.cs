using XRL.World.Parts;

namespace XRL.World.ObjectBuilders;

public class MechanimistPaladin : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		GameObject gameObject = null;
		gameObject = ((!75.in100()) ? GameObject.Create("Cudgel4", 0, 0, null, null, null, Context) : GameObject.Create("Cudgel3", 0, 0, null, null, null, Context));
		if (50.in100() && !gameObject.HasPart<ModFreezing>())
		{
			gameObject.AddPart(new ModFreezing(4));
		}
		GO.ReceiveObject(gameObject);
		if (50.in100())
		{
			GO.ReceiveObject("Leather Boots");
		}
		else
		{
			GO.ReceiveObject("Steel Boots");
		}
	}
}
