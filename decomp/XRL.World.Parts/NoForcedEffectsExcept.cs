using System;

namespace XRL.World.Parts;

[Serializable]
public class NoForcedEffectsExcept : IPart
{
	public string Exception;

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
		if (!Exception.CachedCommaExpansion().Contains(E.Name))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
