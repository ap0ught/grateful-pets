namespace XRL.World.AI;

public class AllyBirth : IAllyReasonSourced
{
	public override string GetText(GameObject Actor)
	{
		return "I was birthed by " + Name + ".";
	}
}
