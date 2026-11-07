using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsRack : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
