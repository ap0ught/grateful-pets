namespace XRL;

/// This class is not used in the base game.
public abstract class SifrahPrioritizableToken : SifrahToken, SifrahPrioritizable
{
	public abstract int GetPriority();

	public abstract int GetTiebreakerPriority();
}
