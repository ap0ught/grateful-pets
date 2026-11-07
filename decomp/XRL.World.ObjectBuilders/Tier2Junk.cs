namespace XRL.World.ObjectBuilders;

public class Tier2Junk : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		for (int i = 0; i < 3; i++)
		{
			if (25.in100())
			{
				GO.ReceiveObjectFromPopulation("Junk 2", null, NoStack: false, 0, 0, null, null, null, Context);
			}
		}
		if (5.in100())
		{
			GO.ReceiveObjectFromPopulation("Junk 3", null, NoStack: false, 0, 0, null, null, null, Context);
		}
	}
}
