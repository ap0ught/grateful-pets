using System;

namespace XRL.World.Parts;

[Serializable]
public class ForceWallTarget : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}
}
