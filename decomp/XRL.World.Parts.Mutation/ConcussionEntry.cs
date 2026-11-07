namespace XRL.World.Parts.Mutation;

public class ConcussionEntry
{
	public Cell C;

	public int Distance;

	public string Direction;

	public ConcussionEntry(Cell C, int D, string Dir)
	{
		this.C = C;
		Distance = D;
		Direction = Dir;
	}
}
