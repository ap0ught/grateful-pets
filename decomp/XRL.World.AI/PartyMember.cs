namespace XRL.World.AI;

public struct PartyMember(GameObjectReference Reference, int Flags)
{
	public GameObjectReference Reference = Reference;

	public int Flags = Flags;
}
