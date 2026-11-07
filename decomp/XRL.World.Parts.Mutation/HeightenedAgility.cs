using System;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HeightenedAgility : BaseMutation
{
	public int GetCooldownCancelChance(int Level)
	{
		return 7 + Level * 3;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeCooldownActivatedAbility");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeCooldownActivatedAbility")
		{
			if (E.GetStringParameter("Tags") == "Agility" && Stat.Random(1, 100) <= GetCooldownCancelChance(Level))
			{
				E.SetParameter("Turns", 0);
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You do that with ease.");
					ParentObject.ParticleText("No cooldown!", 'G');
				}
				return false;
			}
			return true;
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return "Your joints stretch much further than usual.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("+{{rules|" + (2 + (Level - 1) / 2) + "}} Agility\n", "{{rules|", GetCooldownCancelChance(Level).ToString(), "%}} chance that Sprint and skills with Agility prerequisites don't go on cooldown after use");
	}

	public int GetAgilityBonus(int Level)
	{
		return 2 + (Level - 1) / 2;
	}

	public int GetAgilityBonusAttributableToTemporaryLevels()
	{
		return GetAgilityBonus(Level) - GetAgilityBonus(Level - GetTemporaryLevels());
	}

	public override bool ChangeLevel(int NewLevel)
	{
		int agilityBonusAttributableToTemporaryLevels = GetAgilityBonusAttributableToTemporaryLevels();
		int amount = GetAgilityBonus(Level) - agilityBonusAttributableToTemporaryLevels;
		StatShifter.SetStatShift(ParentObject, "Agility", amount, baseValue: true);
		StatShifter.SetStatShift(ParentObject, "Agility", agilityBonusAttributableToTemporaryLevels);
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
