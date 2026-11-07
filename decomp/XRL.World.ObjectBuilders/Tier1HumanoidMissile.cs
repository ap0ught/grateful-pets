using XRL.Rules;

namespace XRL.World.ObjectBuilders;

public class Tier1HumanoidMissile : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		GO.ReceiveObject("Short Bow", NoStack: false, 0, 0, null, null, null, Context);
		GO.ReceiveObject("Wooden Arrow", Stat.Random(20, 40), NoStack: false, 0, 0, null, null, null, Context);
	}
}
