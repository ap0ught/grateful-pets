using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class AdjustingAura : IActivePart
{
	public string AdjustSpec;

	public int Duration = 3;

	public int Radius = 4;

	public int TargetingInterval = 5;

	public bool RequiresParty = true;

	public bool AdjustSubject;

	public bool AdjustLeader;

	[NonSerialized]
	private string AdjustDetails;

	[NonSerialized]
	private List<Effect> Effects = new List<Effect>();

	[NonSerialized]
	private int RefreshDelay;

	public AdjustingAura()
	{
		WorksOnSelf = true;
	}

	public AdjustingAura(string Spec)
		: this()
	{
		WorksOnSelf = true;
		AdjustSpec = Spec;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (!AdjustSpec.IsNullOrEmpty() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (RefreshDelay-- <= 0)
			{
				RefreshTargets(activePartFirstSubject);
				RefreshDelay = TargetingInterval;
			}
			RefreshEffects(activePartFirstSubject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!WorksOnSelf)
		{
			if (AdjustDetails == null)
			{
				Adjusted adjusted = new Adjusted(AdjustSpec, Duration, ParentObject);
				adjusted.ApplySpec();
				AdjustDetails = Grammar.MakeAndList(adjusted.GetDetails().Split('\n'));
			}
			E.Postfix.Compound("{{rules|", '\n').Append(AdjustDetails).Append(" to ");
			if (AdjustSubject)
			{
				E.Postfix.Append("you and ");
			}
			E.Postfix.Append("all your followers");
			if (Radius < 80)
			{
				E.Postfix.Append(" within a radius of ").Append(Radius);
			}
			E.Postfix.Append("}}");
		}
		return base.HandleEvent(E);
	}

	public void RefreshTargets(GameObject Subject)
	{
		Cell cell = Subject.CurrentCell;
		if (cell == null || !cell.ParentZone.IsActive())
		{
			return;
		}
		if (AdjustSubject)
		{
			RefreshTarget(Subject, Subject, RefreshSubject: true);
		}
		if (Radius >= cell.ParentZone.Width)
		{
			Zone.ObjectEnumerator enumerator = cell.ParentZone.IterateObjects().GetEnumerator();
			while (enumerator.MoveNext())
			{
				GameObject current = enumerator.Current;
				RefreshTarget(Subject, current);
			}
			return;
		}
		foreach (Cell item in cell.IterateAdjacent(Radius))
		{
			foreach (GameObject @object in item.Objects)
			{
				RefreshTarget(Subject, @object);
			}
		}
	}

	public void RefreshTarget(GameObject Subject, GameObject Target, bool RefreshSubject = false)
	{
		if (!Target.IsCombatObject())
		{
			return;
		}
		if (Subject == Target)
		{
			if (!RefreshSubject)
			{
				return;
			}
		}
		else if (RequiresParty)
		{
			GameObject partyLeader = Target.PartyLeader;
			if (partyLeader == null)
			{
				if (!AdjustLeader || !Subject.IsLedBy(Target))
				{
					return;
				}
			}
			else if (partyLeader != Subject && partyLeader != Subject.PartyLeader)
			{
				return;
			}
		}
		else if (!Target.IsAlliedTowards(Subject))
		{
			return;
		}
		Effect effect = Target.GetEffect(IsOurEffect);
		if (effect == null)
		{
			Effects.Add(effect = new Adjusted(AdjustSpec, Duration, ParentObject));
			Target.ApplyEffect(effect);
			int num = Mathf.CeilToInt((float)Target.Speed / 100f - 1f);
			if (num > 0)
			{
				effect.Duration += num;
			}
		}
		else if (!Effects.Contains(effect))
		{
			Effects.Add(effect);
		}
	}

	public bool IsOurEffect(Effect FX)
	{
		if (FX is Adjusted adjusted)
		{
			return adjusted.SourceID == ParentObject.ID;
		}
		return false;
	}

	public void RefreshEffects(GameObject Subject)
	{
		for (int num = Effects.Count - 1; num >= 0; num--)
		{
			Effect effect = Effects[num];
			if (effect.Object == null || !effect.Object.IsValid())
			{
				Effects.RemoveAt(num);
			}
			else if (effect.Object.DistanceTo(Subject) <= Radius)
			{
				effect.Duration = Duration;
				int num2 = Mathf.CeilToInt((float)effect.Object.Speed / 100f - 1f);
				if (num2 > 0)
				{
					effect.Duration += num2;
				}
			}
		}
	}
}
