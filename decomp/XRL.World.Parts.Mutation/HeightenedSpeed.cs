using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HeightenedSpeed : BaseMutation
{
	public int Bonus;

	public int Penalty;

	public HeightenedSpeed()
	{
		Type = "Physical";
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
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public int GetSpeedBonus(int Level)
	{
		return 13 + 2 * Level;
	}

	public override string GetDescription()
	{
		return "You are gifted with tremendous speed.";
	}

	public override string GetLevelText(int Level)
	{
		return "+{{rules|" + GetSpeedBonus(Level) + "}} Quickness";
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		ParentObject.Statistics["Speed"].BaseValue -= Bonus;
		Bonus = GetSpeedBonus(Level);
		ParentObject.Statistics["Speed"].BaseValue += Bonus;
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.Statistics["Speed"].BaseValue -= Bonus;
		Bonus = 0;
		return base.Unmutate(GO);
	}
}
