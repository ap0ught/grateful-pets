using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HeightenedWillpower : BaseMutation
{
	public int Bonus;

	public HeightenedWillpower()
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
			E.Add("salt", 1);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You are possessed of an indefatigable focus which every so often manifests itself as stubbornness.";
	}

	public override string GetLevelText(int Level)
	{
		string text = (2 + (Level - 1) / 2).Signed() + " Willpower\n";
		if (Level == BaseLevel)
		{
			text += "Small chance that you stubbornly refuse to flee from a fight [unimplemented]\n";
		}
		else if (Level % 2 != 0)
		{
			text += "Increased chance that you stubbornly refuse to flee from a flight\n";
		}
		if (Level == BaseLevel)
		{
			text += "Small chance when you are injured to ignore all damage for the next 5 rounds [unimplemented]";
		}
		else if (Level % 2 == 0)
		{
			text += "Increased chance when you are injured to ignore all damage for the next 5 rounds [unimplemented]";
		}
		return text;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		ParentObject.Statistics["Willpower"].BaseValue -= Bonus;
		Bonus = 2 + (Level - 1) / 2;
		ParentObject.Statistics["Willpower"].BaseValue += Bonus;
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.Statistics["Willpower"].BaseValue -= Bonus;
		return base.Unmutate(GO);
	}
}
