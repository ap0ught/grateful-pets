namespace XRL.World.Parts.Mutation;

public interface IRankedMutation
{
	int GetRank();

	int AdjustRank(int amount);
}
