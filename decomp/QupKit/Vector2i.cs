namespace QupKit;

public struct Vector2i
{
	public int x;

	public int y;

	public Vector2i(int[] xy)
	{
		x = xy[0];
		y = xy[1];
	}

	public Vector2i(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public override int GetHashCode()
	{
		return x ^ y;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return (Vector2i)obj == this;
	}

	public static bool operator ==(Vector2i c1, Vector2i c2)
	{
		if (c1.x == c2.x)
		{
			return c1.y == c2.y;
		}
		return false;
	}

	public static bool operator !=(Vector2i c1, Vector2i c2)
	{
		if (c1.x == c2.x)
		{
			return c1.y != c2.y;
		}
		return true;
	}
}
