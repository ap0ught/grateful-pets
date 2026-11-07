using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Engulfed : Effect
{
	public GameObject EngulfedBy;

	public bool ChangesWereApplied;

	public int TurnsEngulfed;

	public Engulfed()
	{
		Duration = 1;
	}

	public Engulfed(GameObject EngulfedBy)
		: this()
	{
		this.EngulfedBy = EngulfedBy;
		DisplayName = "{{B|engulfed by " + EngulfedBy.an() + "}}";
	}

	public override int GetEffectType()
	{
		return 33554464;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (GameObject.Validate(ref EngulfedBy))
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num >= 0 && num <= 30)
			{
				E.ColorString = EngulfedBy.Render.ColorString;
				E.DetailColor = EngulfedBy.Render.DetailColor;
				E.Tile = EngulfedBy.Render.Tile;
				E.RenderString = EngulfedBy.Render.RenderString;
			}
		}
		return base.Render(E);
	}

	public override string GetDetails()
	{
		if (!CheckEngulfedBy())
		{
			return null;
		}
		Engulfing part = EngulfedBy.GetPart<Engulfing>();
		if (part.MustBeUnderstood && Object.IsPlayer() && !EngulfedBy.Understood())
		{
			return null;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (!part.EffectDescriptionPrefix.IsNullOrEmpty())
		{
			stringBuilder.Append(part.EffectDescriptionPrefix);
		}
		if (part.AVBonus != 0)
		{
			stringBuilder.Append(part.AVBonus.Signed()).Append(" AV.\n");
		}
		if (part.DVPenalty != 0)
		{
			stringBuilder.Append((-part.DVPenalty).Signed()).Append(" DV.\n");
		}
		if (!part.Damage.IsNullOrEmpty() && part.DamageChance > 0)
		{
			if (part.DamageChance >= 100)
			{
				stringBuilder.Append("Inflicts periodic damage.\n");
			}
			else
			{
				stringBuilder.Append("May inflict periodic damage.\n");
			}
		}
		stringBuilder.Append("Cannot move until @they break free.\n");
		if (part.ExitSaveTarget > 0)
		{
			if (!part.ExitSaveStat.IsNullOrEmpty())
			{
				stringBuilder.Append("Exiting may not succeed, and is a task dependent on " + part.ExitSaveStat + ".\n");
			}
			else
			{
				stringBuilder.Append("Exiting may not succeed.\n");
			}
		}
		if (!part.Damage.IsNullOrEmpty() && part.ExitDamageChance > 0)
		{
			if (part.ExitDamageChance >= 100)
			{
				if (part.ExitSaveTarget > 0)
				{
					if (part.ExitDamageFailOnly)
					{
						stringBuilder.Append("Attempting to exit and failing inflicts damage.\n");
					}
					else
					{
						stringBuilder.Append("Attempting to exit inflicts damage.\n");
					}
				}
				else
				{
					stringBuilder.Append("Exiting inflicts damage.\n");
				}
			}
			else if (part.ExitSaveTarget > 0)
			{
				if (part.ExitDamageFailOnly)
				{
					stringBuilder.Append("Attempting to exit and failing may inflict damage.\n");
				}
				else
				{
					stringBuilder.Append("Attempting to exit may inflict damage.\n");
				}
			}
			else
			{
				stringBuilder.Append("Exiting may inflict damage.\n");
			}
		}
		if (!part.EffectDescriptionPostfix.IsNullOrEmpty())
		{
			stringBuilder.Append(part.EffectDescriptionPostfix);
		}
		return stringBuilder.ToString().TrimEnd('\n');
	}

	public override bool Apply(GameObject Object)
	{
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_organicEngulfment");
		if (!Object.FireEvent(Event.New("ApplyEngulfed")))
		{
			return false;
		}
		if (Object.CurrentCell != null)
		{
			Object.CurrentCell.FireEvent(Event.New("ObjectBecomingEngulfed", "Object", Object));
		}
		EngulfedBy?.MovementModeChanged("Engulfing", Involuntary: true);
		EngulfedBy?.BodyPositionChanged("Engulfing", Involuntary: true);
		Object.MovementModeChanged("Engulfed", Involuntary: true);
		Object.BodyPositionChanged("Engulfed", Involuntary: true);
		ApplyChanges();
		ApplyStats();
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		UnapplyChanges();
	}

	public bool IsEngulfedByValid()
	{
		if (!GameObject.Validate(ref EngulfedBy))
		{
			return false;
		}
		if (EngulfedBy.CurrentCell == null)
		{
			return false;
		}
		if (Object == null)
		{
			return false;
		}
		if (EngulfedBy.CurrentCell != Object.CurrentCell)
		{
			return false;
		}
		if (!Object.PhaseMatches(EngulfedBy))
		{
			return false;
		}
		return true;
	}

	public bool CheckEngulfedBy(bool AllowDisplayNameUpdate = true)
	{
		if (!IsEngulfedByValid())
		{
			Duration = 0;
			Object?.RemoveEffect(this);
			EngulfedBy = null;
			return false;
		}
		if ((EngulfedBy != null) & AllowDisplayNameUpdate)
		{
			DisplayName = "{{B|engulfed by " + EngulfedBy.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "}}";
		}
		return true;
	}

	private void ApplyChangesCore()
	{
		if (ChangesWereApplied)
		{
			UnapplyChangesCore();
		}
		if (EngulfedBy != null)
		{
			Engulfing part = EngulfedBy.GetPart<Engulfing>();
			if (part != null && !part.AffectedProperties.IsNullOrEmpty())
			{
				Dictionary<string, int> propertyMap = part.PropertyMap;
				foreach (string key in propertyMap.Keys)
				{
					Object.ModIntProperty(key, propertyMap[key], RemoveIfZero: true);
				}
			}
		}
		ChangesWereApplied = true;
	}

	public void ApplyChanges()
	{
		if (Object == null)
		{
			return;
		}
		if (ChangesWereApplied)
		{
			UnapplyChanges();
		}
		if (EngulfedBy == null)
		{
			return;
		}
		Engulfing part = EngulfedBy.GetPart<Engulfing>();
		if (part != null && (!part.MustBeUnderstood || !Object.IsPlayer() || EngulfedBy.Understood()) && !part.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ApplyChangesCore();
			if (!part.ApplyChangesEventSelf.IsNullOrEmpty())
			{
				EngulfedBy.FireEvent(part.ApplyChangesEventSelf);
			}
			if (!part.ApplyChangesEventUser.IsNullOrEmpty())
			{
				Object.FireEvent(part.ApplyChangesEventUser);
			}
		}
	}

	private void UnapplyChangesCore()
	{
		if (!ChangesWereApplied || EngulfedBy == null)
		{
			return;
		}
		Engulfing part = EngulfedBy.GetPart<Engulfing>();
		if (part == null)
		{
			return;
		}
		if (!part.AffectedProperties.IsNullOrEmpty())
		{
			Dictionary<string, int> propertyMap = part.PropertyMap;
			foreach (string key in propertyMap.Keys)
			{
				Object.ModIntProperty(key, -propertyMap[key], RemoveIfZero: true);
			}
		}
		ChangesWereApplied = false;
	}

	public void UnapplyChanges()
	{
		if (Object == null || !ChangesWereApplied || EngulfedBy == null)
		{
			return;
		}
		Engulfing part = EngulfedBy.GetPart<Engulfing>();
		if (part != null)
		{
			UnapplyChangesCore();
			if (!part.UnapplyChangesEventSelf.IsNullOrEmpty())
			{
				EngulfedBy.FireEvent(part.UnapplyChangesEventSelf);
			}
			if (!part.UnapplyChangesEventUser.IsNullOrEmpty())
			{
				Object.FireEvent(part.UnapplyChangesEventUser);
			}
		}
	}

	private void ApplyStats()
	{
		if (EngulfedBy != null)
		{
			Engulfing part = EngulfedBy.GetPart<Engulfing>();
			if (part != null)
			{
				StatShifter.SetStatShift(Object, "AV", part.AVBonus);
				StatShifter.SetStatShift(Object, "DV", -part.DVPenalty);
			}
		}
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CanChangeMovementModeEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetCompanionStatusEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (E.Object == Object && CheckEngulfedBy())
		{
			if (E.ShowMessage)
			{
				E.Object.Fail("You cannot do that while engulfed by " + EngulfedBy.t() + ".");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			CheckEngulfedBy();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == Object)
		{
			E.AddStatus("engulfed", 100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference && CheckEngulfedBy(AllowDisplayNameUpdate: false))
		{
			E.AddTag("[{{B|engulfed by " + EngulfedBy.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: true) + "}}]");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (CheckEngulfedBy())
		{
			Engulfing part = EngulfedBy.GetPart<Engulfing>();
			part.ProcessTurnEngulfed(Object, ++TurnsEngulfed);
			if (ChangesWereApplied)
			{
				if (part.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					UnapplyChanges();
				}
			}
			else if (!part.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				ApplyChanges();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		Object.RemoveEffect(this);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BeginMove");
		Registrar.Register("BodyPositionChanged");
		Registrar.Register("CanChangeBodyPosition");
		Registrar.Register("LeaveCell");
		Registrar.Register("MovementModeChanged");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMove")
		{
			if (Duration > 0 && !E.HasFlag("Teleporting") && CheckEngulfedBy())
			{
				Object.PerformMeleeAttack(EngulfedBy);
				if (!GameObject.Validate(ref EngulfedBy))
				{
					Object?.RemoveEffect(this);
				}
				return false;
			}
		}
		else if (E.ID == "LeaveCell" || E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			Object.RemoveEffect(this);
		}
		else if (E.ID == "CanChangeBodyPosition")
		{
			if (CheckEngulfedBy())
			{
				if (E.HasFlag("ShowMessage") && Object.IsPlayer())
				{
					Popup.Show("You cannot do that while engulfed by " + EngulfedBy.the + EngulfedBy.ShortDisplayName + ".");
				}
				return false;
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
			UnapplyChangesCore();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChangesCore();
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}
