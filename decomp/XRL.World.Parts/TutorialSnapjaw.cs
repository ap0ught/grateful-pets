using System;
using System.Linq;
using JoppaTutorial;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class TutorialSnapjaw : IPart
{
	public enum BehaviorStageType
	{
		START = 0,
		STEPSOUTH = 100,
		ATTACK = 200
	}

	public BehaviorStageType BehaviorStage;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeAITakingActionEvent>.ID && ID != BeforeDeathRemovalEvent.ID)
		{
			return ID == AttackerDealingDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AttackerDealingDamageEvent E)
	{
		if (E.Damage.Amount >= The.Player.hitpoints)
		{
			E.Damage.Amount = The.Player.hitpoints - 1;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		(TutorialManager.currentStep as FightSnapjaw).leatherArmor = ParentObject.CurrentCell.AddObject("TutorialLeatherArmor");
		(TutorialManager.currentStep as FightSnapjaw).leatherArmor?.RequireID();
		(TutorialManager.currentStep as FightSnapjaw).snapjawDead = true;
		ParentObject.GetEquippedObjects().ToList().ForEach((GameObject i) =>
		{
			i.Obliterate();
		});
		ParentObject.GetPart<Inventory>().DropOnDeath = false;
		ParentObject.Inventory.Clear();
		ParentObject.Obliterate();
		return true;
	}

	public override bool HandleEvent(BeforeAITakingActionEvent E)
	{
		if (BehaviorStage == BehaviorStageType.START)
		{
			if (!ParentObject.IsVisible() && ParentObject.DistanceTo(The.PlayerCell) > 3)
			{
				ParentObject.Energy.BaseValue = 999;
				return false;
			}
			ParentObject.ForceUnequipRemoveAndRemoveContents(Silent: true);
			BehaviorStage = BehaviorStageType.STEPSOUTH;
		}
		if (BehaviorStage == BehaviorStageType.STEPSOUTH)
		{
			ParentObject.Move("S");
			BehaviorStage = BehaviorStageType.ATTACK;
			ParentObject.Brain.Target = The.Player;
			ParentObject.Brain.PushGoal(new Kill(The.Player));
			(TutorialManager.currentStep as FightSnapjaw)?.SnapjawSeen(ParentObject?.CurrentCell?.Location);
			ParentObject.Energy.BaseValue = -11;
			return false;
		}
		return true;
	}
}
