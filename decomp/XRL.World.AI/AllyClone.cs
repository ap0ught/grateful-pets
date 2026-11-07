namespace XRL.World.AI;

public class AllyClone : IAllyReasonSourced
{
	public override string GetText(GameObject Actor)
	{
		return "I am become " + Name + ".";
	}
}
