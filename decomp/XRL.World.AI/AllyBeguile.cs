namespace XRL.World.AI;

public class AllyBeguile : IAllyReasonSourced
{
	public override ReplaceTarget Replace => ReplaceTarget.Type;

	public override void Initialize(GameObject Actor, GameObject Source, AllegianceSet Set)
	{
		base.Initialize(Actor, Source, Set);
	}

	public override string GetText(GameObject Actor)
	{
		return "I fell in love with " + Name + ".";
	}
}
