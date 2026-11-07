namespace XRL.World.AI;

public class AllyCurio : IAllyReasonSourced
{
	public override string GetText(GameObject Actor)
	{
		return "I like " + Name + ".";
	}
}
