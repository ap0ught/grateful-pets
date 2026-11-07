namespace XRL.World.AI;

public class AllyWish : IAllyReason
{
	public override string GetText(GameObject Actor)
	{
		return "I was compelled by an outside force.";
	}
}
