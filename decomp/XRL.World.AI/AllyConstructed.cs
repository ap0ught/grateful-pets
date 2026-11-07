namespace XRL.World.AI;

public class AllyConstructed : IAllyReasonSourced
{
	public override string GetText(GameObject Actor)
	{
		return "I was constructed by " + Name + ".";
	}
}
