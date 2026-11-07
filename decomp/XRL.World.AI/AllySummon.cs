namespace XRL.World.AI;

public class AllySummon : IAllyReasonSourced
{
	public override ReplaceTarget Replace => ReplaceTarget.Type;

	public override string GetText(GameObject Actor)
	{
		return "I was summoned by " + Name + ".";
	}
}
