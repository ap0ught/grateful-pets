using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsEffectSuppressor : IActivePart
{
	public int Chance = 100;

	public string Effects;

	public CyberneticsEffectSuppressor()
	{
		WorksOnImplantee = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID && ID != CanApplyEffectEvent.ID)
		{
			return ID == ImplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		if (E.Implantee.Effects.Count > 0)
		{
			List<Effect> list = null;
			List<string> list2 = Effects.CachedCommaExpansion();
			foreach (Effect effect in E.Implantee.Effects)
			{
				if (list2.Contains(effect.ClassName))
				{
					if (list == null)
					{
						list = new List<Effect>();
					}
					list.Add(effect);
				}
			}
			if (list != null)
			{
				foreach (Effect item in list)
				{
					E.Implantee.RemoveEffect(item);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanApplyEffectEvent E)
	{
		if (Chance >= 100 && Effects.CachedCommaExpansion().Contains(E.Name) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (Effects.CachedCommaExpansion().Contains(E.Name) && Chance.in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
