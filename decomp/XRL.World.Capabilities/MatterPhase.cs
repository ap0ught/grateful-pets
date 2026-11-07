using System;

namespace XRL.World.Capabilities;

public static class MatterPhase
{
	public const int SOLID = 1;

	public const int LIQUID = 2;

	public const int GAS = 3;

	public const int PLASMA = 4;

	public static string getName(int matterPhase)
	{
		return matterPhase switch
		{
			1 => "solid", 
			2 => "liquid", 
			3 => "gas", 
			4 => "plasma", 
			_ => throw new Exception("invalid matter phase " + matterPhase), 
		};
	}

	public static int getMatterPhase(GameObject obj)
	{
		return GetMatterPhaseEvent.GetFor(obj);
	}
}
