using System;
using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class RocketSkates : IActivePart
{
	public int PlumeLevel = 3;

	private FlamingRay _flamingRay;

	public RocketSkates()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as RocketSkates).PlumeLevel != PlumeLevel)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CellChangedEvent.ID && ID != EquippedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != PooledEvent<GetJumpingBehaviorEvent>.ID && ID != PooledEvent<GetRunningBehaviorEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != PooledEvent<JumpedEvent>.ID && ID != PooledEvent<PartSupportEvent>.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PartSupportEvent E)
	{
		if (E.Skip != this && E.Type == "Run" && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRunningBehaviorEvent E)
	{
		if (E.Priority < 20 && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.AbilityName = "Power Skate";
			E.Verb = "power skate";
			E.EffectDisplayName = "{{K-y-Y-M-m-K-m-M-Y-y-K-y-Y sequence|power skating}}";
			E.EffectMessageName = "power skating";
			E.EffectDuration = 9999;
			E.SpringingEffective = true;
			E.Priority = 20;
			E.Stats?.Set("PowerSkate", "true");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetJumpingBehaviorEvent E)
	{
		if (E.Priority < 20 && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.AbilityName = "Rocket Jump";
			E.Verb = "rocket jump";
			E.ProviderKey = GetType().Name;
			E.RangeModifier = 4;
			E.MinimumRange = 3;
			E.CanJumpOverCreatures = true;
			E.Priority = 20;
			E.Stats?.Set("CanJumpOverCreatures", "true");
			E.Stats?.Set("Rocket", "true");
			E.Stats?.AddLinearBonusModifier("Range", 4, ParentObject.BaseDisplayName);
			E.Stats?.AddLinearBonusModifier("MinimumRange", 1, ParentObject.BaseDisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(JumpedEvent E)
	{
		if (E.Pass == 2 && E.ProviderKey == GetType().Name)
		{
			ConsumeCharge((E.Path?.Count ?? E.OriginCell?.DistanceTo(E.TargetCell) ?? 5) * 2);
			if (E.Path != null)
			{
				bool flag = false;
				Zone parentZone = E.OriginCell.ParentZone;
				int i = 0;
				for (int num = E.Path.Count - 1; i < num; i++)
				{
					Cell cell = parentZone?.GetCell(E.Path[i].X, E.Path[i].Y);
					if (cell != null && EmitFlamePlume(cell, null, E.Actor))
					{
						flag = true;
					}
				}
				if (flag)
				{
					IComponent<GameObject>.EmitMessage(E.Actor, "A {{fiery|jet of flame}} shoots out of " + ParentObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: true, Reflexive: false, null, AsPossessed: true) + "!", ' ', FromDialog: true);
				}
			}
			if (!E.Actor.MakeSave("Agility", 16, null, null, "RocketSkates RocketJump Knockdown", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject) && E.Actor.ApplyEffect(new Prone(Voluntary: false, E.Actor.IsPlayer(), E.Actor.IsPlayer())))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Replaces Sprint with Power Skate (unlimited duration).");
		E.Postfix.AppendRules("Emits plumes of fire when the wearer moves while power skating.", GetEventSensitiveAddStatusSummary(E));
		E.Postfix.AppendRules("Replaces Jump with Rocket Jump.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		SyncAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		SyncAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		SyncAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "AfterMoved");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "AfterMoved");
		NeedPartSupportEvent.Send(E.Actor, "Run", this);
		Run.SyncAbility(E.Actor);
		Acrobatics_Jump.SyncAbility(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineFailureEvent E)
	{
		if (ExamineFailure(E, 50))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (ExamineFailure(E, 75))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (IsSkating() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ParentObject.Smoke();
		}
		SyncAbility();
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterMoved")
		{
			Cell cell = E.GetParameter("FromCell") as Cell;
			Cell cell2 = ParentObject.GetCurrentCell();
			if (cell != null && cell.ParentZone == cell2.ParentZone && PlumeLevel > 0 && !cell2.OnWorldMap() && IsSkating() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				string directionFromCell = cell2.GetDirectionFromCell(cell);
				Cell cell3 = cell.GetCellFromDirection(directionFromCell);
				if (cell3 == null || cell3.ParentZone != cell2.ParentZone)
				{
					cell3 = cell;
				}
				EmitFlamePlume(cell3, cell);
			}
		}
		return base.FireEvent(E);
	}

	private bool EmitFlamePlume(Cell FlameCell, Cell FromCell = null, GameObject Actor = null, bool ShowMessage = false, bool UsePopups = false)
	{
		if (FlameCell == null)
		{
			return false;
		}
		if (ShowMessage)
		{
			DidX("emit", "a {{fiery|plume of flame}}", "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopups);
		}
		if (_flamingRay == null)
		{
			_flamingRay = new FlamingRay();
		}
		_flamingRay.ParentObject = Actor ?? ParentObject.Equipped ?? ParentObject;
		_flamingRay.Level = PlumeLevel;
		FlameCell?.ParticleBlip("&r^W" + (char)(219 + Stat.Random(0, 4)), 6, 0L);
		if (FromCell != FlameCell)
		{
			FromCell?.ParticleBlip("&R^W" + (char)(219 + Stat.Random(0, 4)), 3, 0L);
		}
		_flamingRay.Flame(FlameCell, null, DoEffect: false, UsePopups);
		return true;
	}

	public bool IsSkating()
	{
		Running running = ParentObject.Equipped?.GetEffect<Running>();
		if (running != null)
		{
			return running.MessageName == "power skating";
		}
		return false;
	}

	public override bool IsActivePartEngaged()
	{
		if (!IsSkating())
		{
			return false;
		}
		return base.IsActivePartEngaged();
	}

	private void SyncAbility(GameObject who = null)
	{
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return;
			}
		}
		if (IsObjectActivePartSubject(who) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			who.RequirePart<Run>();
		}
		Run.SyncAbility(who);
		Acrobatics_Jump.SyncAbility(who);
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && EmitFlamePlume(E.Actor.CurrentCell, ParentObject.CurrentCell, ParentObject, ShowMessage: true, E.Actor.IsPlayer()))
		{
			E.Identify = true;
			return true;
		}
		return false;
	}
}
