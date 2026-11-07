using System;
using System.Collections.Generic;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class LowStatBooster : IPoweredPart
{
	public string _AffectedStats = "Strength,Agility,Toughness,Intelligence,Willpower,Ego";

	public int _Amount = 3;

	public string _ActiveStat;

	[NonSerialized]
	private List<string> _AffectedStatList;

	public bool Applied;

	public string AffectedStats
	{
		get
		{
			return _AffectedStats;
		}
		set
		{
			if (value != AffectedStats)
			{
				UnapplyEffects();
				_AffectedStats = value;
				CheckApplyEffects();
			}
		}
	}

	public int Amount
	{
		get
		{
			return _Amount;
		}
		set
		{
			if (value != Amount)
			{
				UnapplyEffects();
				_Amount = value;
				CheckApplyEffects();
			}
		}
	}

	public string ActiveStat
	{
		get
		{
			return _ActiveStat;
		}
		set
		{
			if (value != ActiveStat)
			{
				UnapplyEffects();
				_ActiveStat = value;
				CheckApplyEffects();
			}
		}
	}

	public List<string> AffectedStatList
	{
		get
		{
			if (_AffectedStatList == null)
			{
				_AffectedStatList = new List<string>(AffectedStats.Split(','));
			}
			return _AffectedStatList;
		}
	}

	public LowStatBooster()
	{
		ChargeUse = 0;
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		LowStatBooster lowStatBooster = p as LowStatBooster;
		if (lowStatBooster.AffectedStats != AffectedStats)
		{
			return false;
		}
		if (lowStatBooster.Amount != Amount)
		{
			return false;
		}
		if (lowStatBooster.ActiveStat != ActiveStat)
		{
			return false;
		}
		if (lowStatBooster.Applied != Applied)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EffectAppliedEvent.ID && ID != EquippedEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApplyEffects(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		UnapplyEffects(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckApplyEffects();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.Append("\n{{rules|");
		if (Amount > 0)
		{
			E.Postfix.Append('+').Append(Amount);
		}
		else
		{
			E.Postfix.Append(Amount);
		}
		E.Postfix.Append(" to the lowest of ");
		List<string> list = new List<string>(AffectedStatList.Count);
		foreach (string affectedStat in AffectedStatList)
		{
			list.Add(Statistic.GetStatDisplayName(affectedStat));
		}
		E.Postfix.Append(Grammar.MakeOrList(list)).Append('.');
		AddStatusSummary(E.Postfix);
		E.Postfix.Append("}}");
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckApplyEffects(null, Applied, Amount);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CellChanged");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CellChanged")
		{
			CheckApplyEffects();
		}
		return base.FireEvent(E);
	}

	private GameObject DefaultToSubject(GameObject obj = null)
	{
		return obj ?? GetActivePartFirstSubject();
	}

	private bool ApplyEffects(GameObject obj = null)
	{
		if (Applied)
		{
			return false;
		}
		if ((obj = DefaultToSubject(obj)) == null)
		{
			return false;
		}
		if (ActiveStat != null && obj.HasStat(ActiveStat))
		{
			int num = (Statistic.IsInverseBenefit(ActiveStat) ? (-Amount) : Amount);
			if (num > 0)
			{
				obj.GetStat(ActiveStat).Bonus += num;
			}
			else
			{
				obj.GetStat(ActiveStat).Penalty += -num;
			}
		}
		Applied = true;
		return true;
	}

	private bool UnapplyEffects(GameObject obj = null)
	{
		if (!Applied)
		{
			return false;
		}
		if ((obj = DefaultToSubject(obj)) == null)
		{
			return false;
		}
		if (ActiveStat != null && obj.HasStat(ActiveStat))
		{
			int num = (Statistic.IsInverseBenefit(ActiveStat) ? (-Amount) : Amount);
			if (num > 0)
			{
				obj.GetStat(ActiveStat).Bonus -= num;
			}
			else
			{
				obj.GetStat(ActiveStat).Penalty -= -num;
			}
		}
		Applied = false;
		return true;
	}

	private string LowestStat(GameObject obj)
	{
		if ((obj = DefaultToSubject(obj)) == null)
		{
			return null;
		}
		string result = null;
		int num = int.MaxValue;
		foreach (string affectedStat in AffectedStatList)
		{
			if (obj.HasStat(affectedStat))
			{
				int num2 = obj.BaseStat(affectedStat);
				if (Statistic.IsInverseBenefit(affectedStat))
				{
					num2 = 115 - num2;
				}
				if (num2 < num)
				{
					num = num2;
					result = affectedStat;
				}
			}
		}
		return result;
	}

	public void CheckApplyEffects(GameObject obj = null, bool UseCharge = false, int MultipleCharge = 1)
	{
		if (IsDisabled(UseCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, MultipleCharge, null, UseChargeIfUnpowered: false, 0L))
		{
			UnapplyEffects(obj);
			return;
		}
		string text = LowestStat(obj);
		if (text != ActiveStat)
		{
			UnapplyEffects(obj);
			_ActiveStat = text;
			ApplyEffects(obj);
		}
		else
		{
			ApplyEffects(obj);
		}
	}
}
