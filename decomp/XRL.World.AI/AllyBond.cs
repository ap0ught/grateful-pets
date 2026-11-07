namespace XRL.World.AI;

public class AllyBond : IAllyReasonSourced
{
	public override string GetText(GameObject Actor)
	{
		return "I share a bond with " + Name + ".";
	}
}
