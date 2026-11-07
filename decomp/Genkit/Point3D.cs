namespace Genkit;

public struct Point3D
{
	public static readonly Point3D zero = new Point3D(0, 0, 0);

	public static readonly Point3D invalid = new Point3D(int.MinValue, int.MaxValue, int.MinValue);

	public int x;

	public int y;

	public int z;

	public override string ToString()
	{
		return x + "," + y + "," + z;
	}

	public Point3D(int _x, int _y, int _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	public override int GetHashCode()
	{
		return x ^ y ^ z;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return (Point3D)obj == this;
	}

	public static bool operator ==(Point3D c1, Point3D c2)
	{
		if (c1.x == c2.x && c1.y == c2.y)
		{
			return c1.z == c2.z;
		}
		return false;
	}

	public static bool operator !=(Point3D c1, Point3D c2)
	{
		if (c1.x == c2.x && c1.y == c2.y)
		{
			return c1.z != c2.z;
		}
		return true;
	}
}
