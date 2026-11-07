namespace XRL.World.AI;

public class AllyRebuke : IAllyReasonSourced
{
	public override ReplaceTarget Replace => ReplaceTarget.Type;

	public override string GetText(GameObject Actor)
	{
		return "I was rebuked into submission by " + Name + ".";
	}
}
