using System;
using System.Linq;
using JoppaTutorial;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class TutorialBear : IPart
{
	public enum BehaviorStageType
	{
		START = 0,
		STEPSOUTH = 100,
		ATTACKANDMISS = 200,
		ATTACKANDHIT = 300,
		PURSUE = 400
	}

	public BehaviorStageType BehaviorStage;

	private bool attackerMissed;

	private bool defenderMissed;

	private bool attackerHit;

	private bool defenderHit;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeforeTakeActionEvent>.ID && ID != BeforeDeathRemovalEvent.ID && ID != AttackerDealingDamageEvent.ID && ID != GetDefenderHitDiceEvent.ID && ID != BeforeApplyDamageEvent.ID)
		{
			return ID == PooledEvent<GetMeleeAttackChanceEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (TutorialManager.currentStep == null)
		{
			return true;
		}
		E.Chance = ((E.Weapon.Blueprint == "Bear_Bite" && !E.Inherited) ? 100 : (-100));
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		Registrar.Register("DefenderHit");
		Registrar.Register("GetDefenderDV");
		Registrar.Register("TakeDamage");
		Registrar.Register("DealDamage");
		Registrar.Register("AttackerGetDefenderDV");
		Registrar.Register("Regenerating");
		Registrar.Register("Healing");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (TutorialManager.currentStep == null)
		{
			return true;
		}
		if (E.ID == "Healing")
		{
			E.SetParameter("Amount", 0);
			return false;
		}
		if (E.ID == "Regenerating")
		{
			E.SetParameter("Amount", 0);
			return false;
		}
		if (BehaviorStage <= BehaviorStageType.ATTACKANDMISS || BehaviorStage == BehaviorStageType.PURSUE)
		{
			if (TutorialManager.currentStep == null || TutorialManager.currentStep.GetType() != typeof(FightBear))
			{
				return true;
			}
			if (E.ID == "GetDefenderDV" && BehaviorStage == BehaviorStageType.ATTACKANDMISS)
			{
				int value = Stat.Random(2, 2);
				E.SetParameter("NaturalHitResult", value);
				E.SetParameter("Result", value);
				attackerMissed = true;
			}
			if (E.ID == "AttackerGetDefenderDV")
			{
				int value2 = Stat.Random(2, 2);
				E.SetParameter("NaturalHitResult", value2);
				E.SetParameter("Result", value2);
				defenderMissed = true;
			}
			if (attackerMissed && defenderMissed)
			{
				(TutorialManager.currentStep as FightBear).step = 400;
				BehaviorStage = BehaviorStageType.ATTACKANDHIT;
			}
		}
		else if (BehaviorStage <= BehaviorStageType.ATTACKANDHIT)
		{
			if (TutorialManager.currentStep == null || TutorialManager.currentStep.GetType() != typeof(FightBear))
			{
				return true;
			}
			if (E.ID == "GetDefenderDV")
			{
				int value3 = Stat.Random(18, 19);
				E.SetParameter("NaturalHitResult", value3);
				E.SetParameter("Result", value3);
				attackerHit = true;
			}
			if (E.ID == "AttackerGetDefenderDV")
			{
				int value4 = Stat.Random(18, 19);
				if (defenderHit)
				{
					value4 = 2;
				}
				E.SetParameter("NaturalHitResult", value4);
				E.SetParameter("Result", value4);
				defenderHit = true;
			}
			if (E.ID == "AttackerHit")
			{
				E.SetParameter("Penetrations", 1);
			}
			if (E.ID == "DefenderHit")
			{
				E.SetParameter("Penetrations", 0);
			}
			if (E.ID == "DealDamage")
			{
				Damage parameter = E.GetParameter<Damage>("Damage");
				if (The.Player.hitpoints <= 4)
				{
					The.Player.hitpoints = 5;
				}
				parameter.Amount = 4;
			}
			if (attackerHit && defenderHit)
			{
				(TutorialManager.currentStep as FightBear).step = 500;
				BehaviorStage = BehaviorStageType.PURSUE;
			}
		}
		return true;
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (TutorialManager.currentStep == null || TutorialManager.currentStep.GetType() != typeof(FightBear))
		{
			return true;
		}
		if (E.Damage.HasAttribute("Cold"))
		{
			E.Damage.Amount = ParentObject.hitpoints * 2;
		}
		return true;
	}

	public override bool HandleEvent(AttackerDealingDamageEvent E)
	{
		if (TutorialManager.currentStep == null || TutorialManager.currentStep.GetType() != typeof(FightBear))
		{
			return true;
		}
		if (E.Damage.Amount >= The.Player.hitpoints)
		{
			E.Damage.Amount = The.Player.hitpoints - 1;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (TutorialManager.currentStep == null || TutorialManager.currentStep.GetType() != typeof(FightBear))
		{
			return true;
		}
		(TutorialManager.currentStep as FightBear).chemCell = ParentObject.CurrentCell.AddObject("TutorialChemCell");
		(TutorialManager.currentStep as FightBear).chemCell?.RequireID();
		(TutorialManager.currentStep as FightBear).bearDead = true;
		ParentObject.GetEquippedObjects().ToList().ForEach((GameObject i) =>
		{
			i.Obliterate();
		});
		ParentObject.GetPart<Inventory>().DropOnDeath = false;
		ParentObject.Inventory.Clear();
		ParentObject.Obliterate();
		return true;
	}

	public override bool HandleEvent(BeforeTakeActionEvent E)
	{
		if (BehaviorStage == BehaviorStageType.START)
		{
			ParentObject.hitpoints = 2;
			BehaviorStage = BehaviorStageType.STEPSOUTH;
		}
		if (BehaviorStage == BehaviorStageType.STEPSOUTH)
		{
			if (!ParentObject.IsVisible())
			{
				return false;
			}
			BehaviorStage = BehaviorStageType.ATTACKANDMISS;
			ParentObject.Brain.Target = The.Player;
			ParentObject.Brain.PushGoal(new Kill(The.Player));
			return false;
		}
		return true;
	}
}
