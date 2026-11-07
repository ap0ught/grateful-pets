using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class NoBleed : IPart
{
	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == CanApplyEffectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanApplyEffectEvent E)
	{
		return (object)E.Type != typeof(Bleeding);
	}
}
