using System;

namespace Genkit;

[Serializable]
public class MazeCell
{
	public bool N;

	public bool S;

	public bool E;

	public bool W;

	public int x;

	public int y;

	public MazeCell()
	{
		N = false;
		S = false;
		E = false;
		W = false;
	}

	public MazeCell(bool Init)
	{
		N = Init;
		S = Init;
		E = Init;
		W = Init;
	}

	public bool AnyOpen()
	{
		if (!N && !S && !E)
		{
			return W;
		}
		return true;
	}

	public bool DeadEnd()
	{
		if (!NorthOnly() && !SouthOnly() && !EastOnly())
		{
			return WestOnly();
		}
		return true;
	}

	public bool NorthOnly()
	{
		if (N && !S && !E)
		{
			return !W;
		}
		return false;
	}

	public bool SouthOnly()
	{
		if (!N && S && !E)
		{
			return !W;
		}
		return false;
	}

	public bool EastOnly()
	{
		if (!N && !S && E)
		{
			return !W;
		}
		return false;
	}

	public bool WestOnly()
	{
		if (!N && !S && !E)
		{
			return W;
		}
		return false;
	}
}
