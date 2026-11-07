using System;

namespace XRL.World.Parts;

[Serializable]
public class NoEffects : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ApplyEffectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		return false;
	}
}
