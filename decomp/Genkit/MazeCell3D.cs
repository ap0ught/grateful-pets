using System;

namespace Genkit;

[Serializable]
public class MazeCell3D
{
	public byte Dir;

	public short x;

	public short y;

	public short z;

	public bool N
	{
		get
		{
			return GetBit(0);
		}
		set
		{
			SetBit(0, value);
		}
	}

	public bool S
	{
		get
		{
			return GetBit(1);
		}
		set
		{
			SetBit(1, value);
		}
	}

	public bool E
	{
		get
		{
			return GetBit(2);
		}
		set
		{
			SetBit(2, value);
		}
	}

	public bool W
	{
		get
		{
			return GetBit(3);
		}
		set
		{
			SetBit(3, value);
		}
	}

	public bool U
	{
		get
		{
			return GetBit(4);
		}
		set
		{
			SetBit(4, value);
		}
	}

	public bool D
	{
		get
		{
			return GetBit(5);
		}
		set
		{
			SetBit(5, value);
		}
	}

	public MazeCell3D()
	{
		N = false;
		S = false;
		E = false;
		W = false;
		U = false;
		D = false;
	}

	public MazeCell3D(bool Init)
	{
		N = Init;
		S = Init;
		E = Init;
		W = Init;
		U = Init;
		D = Init;
	}

	public bool GetBit(int n)
	{
		return (Dir & (1 << n)) != 0;
	}

	public void SetBit(int n, bool val)
	{
		if (val)
		{
			Dir |= (byte)(1 << n);
		}
		else if (GetBit(n))
		{
			Dir -= (byte)(1 << n);
		}
	}

	public bool AnyOpen()
	{
		if (!N && !S && !E && !W && !U)
		{
			return D;
		}
		return true;
	}

	public bool DeadEnd()
	{
		if (!NorthOnly() && !SouthOnly() && !EastOnly() && !WestOnly() && !UpOnly())
		{
			return DownOnly();
		}
		return true;
	}

	public bool UpOnly()
	{
		if (!N && !S && !E && !W && U)
		{
			return !D;
		}
		return false;
	}

	public bool DownOnly()
	{
		if (N && !S && !E && !W && !U)
		{
			return D;
		}
		return false;
	}

	public bool NorthOnly()
	{
		if (N && !S && !E && !W && !U)
		{
			return !D;
		}
		return false;
	}

	public bool SouthOnly()
	{
		if (!N && S && !E && !W && !U)
		{
			return !D;
		}
		return false;
	}

	public bool EastOnly()
	{
		if (!N && !S && E && !W && !U)
		{
			return !D;
		}
		return false;
	}

	public bool WestOnly()
	{
		if (!N && !S && !E && W && !U)
		{
			return !D;
		}
		return false;
	}
}
