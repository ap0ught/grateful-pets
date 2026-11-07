using System;

namespace XRL.World.Parts;

[Serializable]
public class NoForcedEffects : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ForceApplyEffectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ForceApplyEffectEvent E)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
