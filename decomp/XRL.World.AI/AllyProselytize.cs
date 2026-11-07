namespace XRL.World.AI;

public class AllyProselytize : IAllyReasonSourced
{
	public override ReplaceTarget Replace => ReplaceTarget.Type;

	public override string GetText(GameObject Actor)
	{
		return "I was convinced by the pleas of " + Name + ".";
	}
}
