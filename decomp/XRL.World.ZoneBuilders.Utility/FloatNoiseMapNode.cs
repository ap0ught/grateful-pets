namespace XRL.World.ZoneBuilders.Utility;

public class FloatNoiseMapNode
{
	public int x;

	public int y;

	public float depth = -1f;

	public FloatNoiseMapNode(int x, int y)
	{
		this.x = x;
		this.y = y;
		depth = -1f;
	}

	public FloatNoiseMapNode(int x, int y, float depth)
	{
		this.x = x;
		this.y = y;
		this.depth = depth;
	}
}
