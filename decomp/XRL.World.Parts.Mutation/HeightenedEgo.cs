using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HeightenedEgo : BaseMutation
{
	public int Bonus;

	public HeightenedEgo()
	{
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("jewels", 1);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You possess a towering vision of self that you project onto the minds of nearby creatures.";
	}

	public int GetAlertRadius(int Level)
	{
		if (Level < 5)
		{
			return 5;
		}
		if (Level < 9)
		{
			return 10;
		}
		return 15;
	}

	public override string GetLevelText(int Level)
	{
		string text = (2 + (Level - 1) / 2).Signed() + " Ego\n";
		text = text + "Creatures within a radius of " + GetAlertRadius(Level) + " are alerted to your presence [unimplemented]\n";
		if (Level == BaseLevel)
		{
			text += "Small chance to frighten an adjacent enemy [unimplemented]";
		}
		else if (Level % 2 == 0)
		{
			text += "Increased chance to frighten an adjacent enemy [unimplemented]";
		}
		return text;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		ParentObject.Statistics["Ego"].BaseValue -= Bonus;
		Bonus = 2 + (Level - 1) / 2;
		ParentObject.Statistics["Ego"].BaseValue += Bonus;
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.Statistics["Ego"].BaseValue -= Bonus;
		return base.Unmutate(GO);
	}
}
