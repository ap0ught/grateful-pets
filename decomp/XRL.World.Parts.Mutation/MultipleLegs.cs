using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MultipleLegs : BaseMutation, IRankedMutation
{
	public int Rank = 1;

	public int Bonus;

	public string AdditionsManagerID => ParentObject.ID + "::MultipleLegs::Add";

	public string ChangesManagerID => ParentObject.ID + "::MultipleLegs::Change";

	public override bool AffectsBodyParts()
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetMaxCarriedWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("travel", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		E.AdjustWeight((1.0 + (double)GetCarryCapacityBonus(Level) / 100.0) * (double)Rank);
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You have an extra set of legs.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("+{{rules|" + GetMoveSpeedBonus(Level) + "}} move speed\n", "+{{rules|", GetCarryCapacityBonus(Level).ToString(), "%}} carry capacity");
	}

	public int GetMoveSpeedBonus(int Level)
	{
		return Level * 20;
	}

	public int GetCarryCapacityBonus(int Level)
	{
		return Level + 5;
	}

	public int GetRank()
	{
		return Rank;
	}

	public int AdjustRank(int amount)
	{
		Rank += amount;
		return Rank;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		StatShifter.SetStatShift(ParentObject, "MoveSpeed", -GetMoveSpeedBonus(NewLevel), baseValue: true);
		CarryingCapacityChangedEvent.Send(ParentObject);
		return base.ChangeLevel(NewLevel);
	}

	public void AddMoreLegs(GameObject Subject)
	{
		Body body = Subject.Body;
		if (body == null)
		{
			return;
		}
		BodyPart body2 = body.GetBody();
		BodyPart firstAttachedPart = body2.GetFirstAttachedPart("Feet", 0, body, EvenIfDismembered: true);
		if (firstAttachedPart != null)
		{
			if (firstAttachedPart.Manager != null || !firstAttachedPart.IsLateralitySafeToChange(0, body))
			{
				body2.AddPartAt(firstAttachedPart, "Feet", 0, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
			}
			else
			{
				body2.AddPartAt(firstAttachedPart, "Feet", 64, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
				firstAttachedPart.ChangeLaterality(16);
				firstAttachedPart.Manager = ChangesManagerID;
			}
		}
		else
		{
			int? num = body2.Category;
			string additionsManagerID = AdditionsManagerID;
			int? category = num;
			string[] orInsertBefore = new string[3] { "Roots", "Tail", "Thrown Weapon" };
			body2.AddPartAt("Feet", 0, null, null, null, null, additionsManagerID, category, null, null, null, null, null, null, null, null, null, null, null, null, "Feet", orInsertBefore);
		}
		Subject.WantToReequip();
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		AddMoreLegs(GO);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.RemoveBodyPartsByManager(AdditionsManagerID, EvenIfDismembered: true);
		foreach (BodyPart item in GO.GetBodyPartsByManager(ChangesManagerID, EvenIfDismembered: true))
		{
			if (item.Laterality == 16 && item.IsLateralityConsistent())
			{
				item.ChangeLaterality(item.Laterality & -17);
			}
		}
		StatShifter.RemoveStatShifts();
		CarryingCapacityChangedEvent.Send(ParentObject);
		GO.WantToReequip();
		return base.Unmutate(GO);
	}
}
