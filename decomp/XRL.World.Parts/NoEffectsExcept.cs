using System;

namespace XRL.World.Parts;

[Serializable]
public class NoEffectsExcept : IPart
{
	public string Exception;

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
		if (!Exception.CachedCommaExpansion().Contains(E.Name))
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
