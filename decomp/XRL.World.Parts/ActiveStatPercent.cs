using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.Collections;

namespace XRL.World.Parts;

[Serializable]
public class ActiveStatPercent : IActivePart
{
	public string Boosts = "";

	public long ApplyTurn;

	public bool Describe = true;

	public bool CountAsBase;

	[NonSerialized]
	private StringMap<float> _Parsed;

	public bool Applied
	{
		get
		{
			return ApplyTurn != 0;
		}
		set
		{
			ApplyTurn = ((!value) ? 0 : (The.Game?.Turns ?? 1));
		}
	}

	public StringMap<float> Parsed
	{
		get
		{
			if (_Parsed == null)
			{
				ParseBoosts();
			}
			return _Parsed;
		}
	}

	public ActiveStatPercent()
	{
		WorksOnEquipper = true;
	}

	public void CheckStatus(GameObject Object = null)
	{
		if (Object == null)
		{
			Object = GetActivePartFirstSubject();
			if (Object == null)
			{
				return;
			}
		}
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Unapply(Object);
		}
		else
		{
			Apply(Object);
		}
	}

	public void Apply(GameObject Object, bool Force = false)
	{
		if (Applied && !Force)
		{
			XRLGame game = The.Game;
			if (game == null || ApplyTurn > game.Turns - 10)
			{
				return;
			}
		}
		Applied = true;
		foreach (KeyValuePair<string, float> item in Parsed)
		{
			if (!Object.Statistics.TryGetValue(item.Key, out var value))
			{
				continue;
			}
			int num = value.BaseValue;
			if (CountAsBase && !value.Shifts.IsNullOrEmpty())
			{
				foreach (Statistic.StatShift shift in value.Shifts)
				{
					if (shift.BaseValue)
					{
						num -= shift.Amount;
					}
				}
			}
			StatShifter.SetStatShift(Object, value.Name, Mathf.RoundToInt((float)num * item.Value), CountAsBase);
		}
	}

	public void Unapply(GameObject Object)
	{
		if (Applied)
		{
			Applied = false;
			StatShifter.RemoveStatShifts(Object);
		}
	}

	public void ParseBoosts()
	{
		if (_Parsed == null)
		{
			_Parsed = new StringMap<float>();
		}
		_Parsed.ClearValues();
		DelimitedEnumeratorChar enumerator = Boosts.DelimitedBy(';').GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Split(':', out var First, out var Second);
			if (int.TryParse(Second, out var result))
			{
				float value = _Parsed.GetValue(First);
				_Parsed[First] = value + (float)result / 100f;
			}
		}
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (Applied && !OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
		CheckStatus();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != EquippedEvent.ID || !WorksOnEquipper) && (ID != UnequippedEvent.ID || !WorksOnEquipper) && (ID != BootSequenceDoneEvent.ID || !IsBootSensitive) && (ID != BootSequenceInitializedEvent.ID || !IsBootSensitive) && (ID != PowerSwitchFlippedEvent.ID || !IsPowerSwitchSensitive) && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return Describe;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckStatus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckStatus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Describe)
		{
			bool activated = IsPowerSwitchSensitive && ParentObject.HasPart<PowerSwitch>();
			foreach (KeyValuePair<string, float> item in Parsed)
			{
				E.Postfix.Append("\n{{rules|");
				Statistic.AppendStatAdjustDescription(E.Postfix, item.Key, Mathf.RoundToInt(item.Value * 100f), activated, Percent: true);
				AddStatusSummary(E.Postfix);
				E.Postfix.Append("}}");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			if (Parsed.TryGetValue("Strength", out var Value) && Value > 0f)
			{
				E.Add("might", 1);
			}
			else if (Parsed.TryGetValue("Intelligence", out Value) && Value > 0f)
			{
				E.Add("scholarship", 1);
			}
			else if (Parsed.TryGetValue("Speed", out Value) && Value > 0f)
			{
				E.Add("time", 1);
			}
			else if (Parsed.TryGetValue("MoveSpeed", out Value) && Value > 0f)
			{
				E.Add("travel", 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckStatus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		Unapply(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		CheckStatus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		CheckStatus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		CheckStatus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		CheckStatus();
		return base.HandleEvent(E);
	}
}
