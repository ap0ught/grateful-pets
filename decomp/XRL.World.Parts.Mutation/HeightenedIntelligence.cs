using System;
using XRL.Messages;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HeightenedIntelligence : BaseMutation
{
	public int Bonus;

	public int Penalty;

	public HeightenedIntelligence()
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
			E.Add("scholarship", 1);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public int GetEgoPenalty(int Level)
	{
		if (Level >= 9)
		{
			return -3;
		}
		if (Level >= 5)
		{
			return -2;
		}
		return -1;
	}

	public override string GetDescription()
	{
		return "You possess extraordinary analytical prowess but you find difficulty in relating to others.";
	}

	public override string GetLevelText(int Level)
	{
		string text = (2 + (Level - 1) / 2).Signed() + " Intelligence\n";
		text = text + GetEgoPenalty(Level) + " Ego\n";
		if (Level == BaseLevel)
		{
			text += "Small chance to reveal the entire map in a flash of insight";
		}
		else if (Level % 2 == 0)
		{
			text += "Increased chance to reveal the entire map in a flash of insight";
		}
		return text;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && ParentObject.IsPlayer() && ParentObject.Physics != null && ParentObject.Physics.CurrentCell != null && !ParentObject.Physics.CurrentCell.ParentZone.IsWorldMap() && Stat.Random(1, 10000) < (2 + Level) * ParentObject.Statistics["Intelligence"].Modifier)
		{
			MessageQueue.AddPlayerMessage("&CA flash of insight overcomes you!");
			ParentObject.Physics.CurrentCell.ParentZone.ExploreAll();
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		ParentObject.Statistics["Intelligence"].BaseValue -= Bonus;
		Bonus = 2 + (Level - 1) / 2;
		ParentObject.Statistics["Intelligence"].BaseValue += Bonus;
		ParentObject.Statistics["Ego"].BaseValue -= Penalty;
		Penalty = GetEgoPenalty(Level);
		ParentObject.Statistics["Ego"].BaseValue += Penalty;
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.Statistics["Intelligence"].BaseValue -= Bonus;
		GO.Statistics["Ego"].BaseValue -= Penalty;
		Bonus = 0;
		Penalty = 0;
		return base.Unmutate(GO);
	}
}
