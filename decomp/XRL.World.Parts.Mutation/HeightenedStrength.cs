using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HeightenedStrength : BaseMutation
{
	[NonSerialized]
	private bool UpdatingStatShifts;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			if (ID == PooledEvent<CheckOverburdenedOnStrengthUpdateEvent>.ID)
			{
				return UpdatingStatShifts;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(CheckOverburdenedOnStrengthUpdateEvent E)
	{
		if (UpdatingStatShifts)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("might", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DealDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DealDamage" && GetDazedChance(Level).in100())
		{
			E.GetGameObjectParameter("Defender")?.ApplyEffect(new Dazed("2-3".RollCached()));
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return "You are possessed of hulking strength.";
	}

	public int GetDazedChance(int Level)
	{
		return 13 + 2 * Level;
	}

	public int GetStrengthBonus(int Level)
	{
		return 2 + (Level - 1) / 2;
	}

	public int GetStrengthBonusAttributableToTemporaryLevels()
	{
		return GetStrengthBonus(Level) - GetStrengthBonus(Level - GetTemporaryLevels());
	}

	public override string GetLevelText(int Level)
	{
		return "+{{C|" + GetStrengthBonus(Level) + "}} Strength\n{{C|" + GetDazedChance(Level) + "%}} chance to daze your opponent on a successful melee attack for 2-3 rounds";
	}

	public override bool ChangeLevel(int NewLevel)
	{
		int strengthBonusAttributableToTemporaryLevels = GetStrengthBonusAttributableToTemporaryLevels();
		int amount = GetStrengthBonus(Level) - strengthBonusAttributableToTemporaryLevels;
		try
		{
			UpdatingStatShifts = true;
			StatShifter.SetStatShift(ParentObject, "Strength", amount, baseValue: true);
			StatShifter.SetStatShift(ParentObject, "Strength", strengthBonusAttributableToTemporaryLevels);
		}
		finally
		{
			UpdatingStatShifts = false;
		}
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		StatShifter.RemoveStatShifts(GO);
		return base.Unmutate(GO);
	}
}
