using System;
using XRL.World;

namespace XRL;

[Serializable]
public class Range : IComposite
{
	public int Min;

	public int Max;

	public Range()
	{
	}

	public Range(int Amount)
	{
		Min = Amount;
		Max = Amount;
	}

	public Range(int _Min, int _Max)
	{
		Min = _Min;
		Max = _Max;
	}
}
