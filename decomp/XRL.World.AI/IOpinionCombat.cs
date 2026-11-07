namespace XRL.World.AI;

public abstract class IOpinionCombat : IOpinionObject
{
	public override int Cooldown => 0;

	public override float Limit => 5f;
}
