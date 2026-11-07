using System;
using System.Collections.Generic;
using XRL.Liquids;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 135, Base = true)]
public abstract class ILiquidEvent : MinEvent
{
	public new static readonly int CascadeLevel = 135;

	public GameObject Actor;

	public string Liquid;

	public LiquidVolume LiquidVolume;

	public int Drams;

	public GameObject Skip;

	public List<GameObject> SkipList;

	public Predicate<GameObject> Filter;

	public bool Auto;

	public bool ImpureOkay;

	public bool SafeOnly;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Liquid = null;
		LiquidVolume = null;
		Drams = 0;
		Skip = null;
		SkipList = null;
		Filter = null;
		Auto = false;
		ImpureOkay = false;
		SafeOnly = false;
	}

	public bool ApplyTo(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj == Skip)
		{
			return false;
		}
		if (SkipList != null && SkipList.Contains(obj))
		{
			return false;
		}
		if (Filter != null && !Filter(obj))
		{
			return false;
		}
		if (SafeOnly)
		{
			if (LiquidVolume == null)
			{
				BaseLiquid liquid = LiquidVolume.GetLiquid(Liquid);
				if (liquid != null)
				{
					if (!liquid.SafeContainer(obj))
					{
						return false;
					}
				}
				else
				{
					LiquidVolume = new LiquidVolume(Liquid, 0);
				}
			}
			if (LiquidVolume != null && !LiquidVolume.SafeContainer(obj))
			{
				return false;
			}
		}
		return true;
	}
}
