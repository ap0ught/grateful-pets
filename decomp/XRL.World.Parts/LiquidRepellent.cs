using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LiquidRepellent : IActivePart
{
	public string _Liquids;

	[NonSerialized]
	private List<string> LiquidList;

	public string Liquids
	{
		get
		{
			return _Liquids;
		}
		set
		{
			_Liquids = value;
			LiquidList = ((_Liquids == null) ? null : Liquids.CachedCommaExpansion());
		}
	}

	public LiquidRepellent()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as LiquidRepellent)._Liquids != _Liquids)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID && ID != CanApplyEffectEvent.ID)
		{
			return ID == ForceApplyEffectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ForceApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	private bool Check(IEffectCheckEvent E)
	{
		if (E.Name == "LiquidCovered" && LiquidList == null)
		{
			return false;
		}
		return true;
	}

	private bool Check(IActualEffectCheckEvent E)
	{
		if (E.Effect is LiquidCovered liquidCovered)
		{
			if (LiquidList == null)
			{
				return false;
			}
			if (liquidCovered.Liquid != null)
			{
				foreach (string key in liquidCovered.Liquid.ComponentLiquids.Keys)
				{
					if (LiquidList.Contains(key))
					{
						return false;
					}
				}
			}
		}
		return true;
	}
}
