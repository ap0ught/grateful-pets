namespace XRL.World.ZoneBuilders.Utility;

public class NoiseMapNode
{
	public int x;

	public int y;

	public int depth = -1;

	public NoiseMapNode(int x, int y)
	{
		this.x = x;
		this.y = y;
		depth = -1;
	}

	public NoiseMapNode(int x, int y, int depth)
	{
		this.x = x;
		this.y = y;
		this.depth = depth;
	}
}
