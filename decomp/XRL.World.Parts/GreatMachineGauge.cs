using System;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class GreatMachineGauge : IPart
{
	public const int CONFUSION_LEVEL = 10;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		GameObject equipped = Object.Equipped;
		if (GameObject.Validate(equipped))
		{
			Registrar.Register(equipped, PooledEvent<GenericCommandEvent>.ID);
			Registrar.Register(equipped, AfterMentalAttackEvent.ID);
		}
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterEvent(this, PooledEvent<GenericCommandEvent>.ID);
		E.Actor.RegisterEvent(this, AfterMentalAttackEvent.ID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterEvent(this, PooledEvent<GenericCommandEvent>.ID);
		E.Actor.UnregisterEvent(this, AfterMentalAttackEvent.ID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericCommandEvent E)
	{
		if (E.Command == Persuasion_Berate.COMMAND_ATTACK && E.Object is GameObject gameObject)
		{
			Confuse(gameObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterMentalAttackEvent E)
	{
		if (E.Penetrations > 0 && (E.Command == Persuasion_MenacingStare.COMMAND_ATTACK || E.Command == Persuasion_Intimidate.COMMAND_ATTACK))
		{
			Confuse(E.Defender);
		}
		return base.HandleEvent(E);
	}

	public void Confuse(GameObject Object)
	{
		GameObject gameObject = ParentObject.EquippedProperlyBy();
		if (GameObject.Validate(gameObject))
		{
			int level = Confusion.GetConfusionLevel(10);
			int penalty = Confusion.GetMentalPenalty(10);
			PerformMentalAttack((MentalAttackEvent MAE) => Confusion.Confuse(MAE, Attack: true, level, penalty), gameObject, Object, ParentObject, "Confuse GreatMachineGauge", "1d8", 8388610, Confusion.GetDuration(10).RollCached(), int.MinValue, Math.Max(gameObject.StatMod("Ego"), 10));
		}
	}
}
