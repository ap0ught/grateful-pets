namespace XRL.World.ObjectBuilders;

public class MechanimistZealot : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		if (75.in100())
		{
			GO.ReceiveObjectFromPopulation("Melee Weapons 3", null, NoStack: false, 0, 0, null, null, null, Context);
		}
		else
		{
			GO.ReceiveObjectFromPopulation("Melee Weapons 4", null, NoStack: false, 0, 0, null, null, null, Context);
		}
	}
}
