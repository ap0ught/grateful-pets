using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Enclosed : Effect
{
	public GameObject EnclosedBy;

	public bool ChangesWereApplied;

	public int TurnsEnclosed;

	[FieldSaveVersion(397)]
	public int AppliedRenderLayer;

	public Enclosed()
	{
		Duration = 1;
	}

	public Enclosed(GameObject EnclosedBy)
		: this()
	{
		this.EnclosedBy = EnclosedBy;
		DisplayName = "{{C|enclosed in " + EnclosedBy.an() + "}}";
	}

	public override int GetEffectType()
	{
		return 32;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		if (!CheckEnclosedBy())
		{
			return null;
		}
		Enclosing part = EnclosedBy.GetPart<Enclosing>();
		if (part.MustBeUnderstood && Object.IsPlayer() && !EnclosedBy.Understood())
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
		stringBuilder.Append("Must spend a turn exiting before moving.\n");
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
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_mechanicalEngulfment");
		if (Object.FireEvent("ApplyEnclosed"))
		{
			if (Object.CurrentCell != null)
			{
				Object.CurrentCell.FireEvent(Event.New("ObjectBecomingEnclosed", "Object", Object));
			}
			ApplyChanges();
			ApplyStats();
			ApplyRenderLayer();
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		UnapplyChanges();
		UnapplyRenderLayer();
	}

	public bool IsEnclosedByValid()
	{
		if (EnclosedBy == null)
		{
			return false;
		}
		if (EnclosedBy.IsInvalid())
		{
			return false;
		}
		if (EnclosedBy.CurrentCell == null)
		{
			return false;
		}
		if (Object == null)
		{
			return false;
		}
		if (Object.CurrentCell == null)
		{
			return false;
		}
		if (EnclosedBy.CurrentCell != Object.CurrentCell)
		{
			return false;
		}
		return true;
	}

	public bool CheckEnclosedBy()
	{
		if (!IsEnclosedByValid())
		{
			EnclosedBy = null;
			Object.RemoveEffect(this);
			return false;
		}
		DisplayName = "{{C|enclosed in " + EnclosedBy.an() + "}}";
		return true;
	}

	private void ApplyChangesCore()
	{
		if (ChangesWereApplied)
		{
			UnapplyChangesCore();
		}
		if (EnclosedBy == null)
		{
			return;
		}
		Enclosing part = EnclosedBy.GetPart<Enclosing>();
		if (part == null)
		{
			return;
		}
		if (!part.AffectedProperties.IsNullOrEmpty())
		{
			Dictionary<string, int> propertyMap = part.PropertyMap;
			foreach (string key in propertyMap.Keys)
			{
				Object.ModIntProperty(key, propertyMap[key], RemoveIfZero: true);
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
		Enclosing part = EnclosedBy.GetPart<Enclosing>();
		if ((!part.MustBeUnderstood || !Object.IsPlayer() || EnclosedBy.Understood()) && !part.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ApplyChangesCore();
			if (!part.ApplyChangesEventSelf.IsNullOrEmpty())
			{
				EnclosedBy.FireEvent(part.ApplyChangesEventSelf);
			}
			if (!part.ApplyChangesEventUser.IsNullOrEmpty())
			{
				Object.FireEvent(part.ApplyChangesEventUser);
			}
		}
	}

	private void UnapplyChangesCore()
	{
		if (!ChangesWereApplied || EnclosedBy == null)
		{
			return;
		}
		Enclosing part = EnclosedBy.GetPart<Enclosing>();
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
		if (Object == null || !ChangesWereApplied || EnclosedBy == null)
		{
			return;
		}
		Enclosing part = EnclosedBy.GetPart<Enclosing>();
		if (part != null)
		{
			UnapplyChangesCore();
			if (!part.UnapplyChangesEventSelf.IsNullOrEmpty())
			{
				EnclosedBy.FireEvent(part.UnapplyChangesEventSelf);
			}
			if (!part.UnapplyChangesEventUser.IsNullOrEmpty())
			{
				Object.FireEvent(part.UnapplyChangesEventUser);
			}
			ChangesWereApplied = false;
		}
	}

	private void ApplyStats()
	{
		if (EnclosedBy != null)
		{
			Enclosing part = EnclosedBy.GetPart<Enclosing>();
			StatShifter.SetStatShift(Object, "AV", part.AVBonus);
			StatShifter.SetStatShift(Object, "DV", -part.DVPenalty);
		}
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	private void ApplyRenderLayer()
	{
		if (EnclosedBy.IsValid())
		{
			int num = Object.Render.RenderLayer - EnclosedBy.Render.RenderLayer + 1;
			if (num > 0)
			{
				AppliedRenderLayer += num;
				Object.Render.RenderLayer -= num;
			}
		}
	}

	private void UnapplyRenderLayer()
	{
		if (AppliedRenderLayer > 0)
		{
			Object.Render.RenderLayer += AppliedRenderLayer;
			AppliedRenderLayer = 0;
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanChangeMovementModeEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (E.Object == Object && CheckEnclosedBy() && EnclosedBy.GetPart<Enclosing>().EnclosureExitImpeded(Object, E.ShowMessage, this))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference && CheckEnclosedBy())
		{
			E.AddTag("[{{B|enclosed in " + EnclosedBy.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + "}}]");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (CheckEnclosedBy())
		{
			Enclosing part = EnclosedBy.GetPart<Enclosing>();
			part.ProcessTurnEnclosed(Object, ++TurnsEnclosed);
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

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BodyPositionChanged");
		Registrar.Register("CanChangeBodyPosition");
		Registrar.Register("CanMoveExtremities");
		Registrar.Register("LeaveCell");
		Registrar.Register("MovementModeChanged");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LeaveCell" || E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			if (CheckEnclosedBy() && !EnclosedBy.GetPart<Enclosing>().ExitEnclosure(Object, E, this))
			{
				return false;
			}
		}
		else if (E.ID == "CanChangeBodyPosition" || E.ID == "CanMoveExtremities")
		{
			if (CheckEnclosedBy() && EnclosedBy.GetPart<Enclosing>().EnclosureExitImpeded(Object, E?.HasFlag("ShowMessage") ?? false, this))
			{
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
