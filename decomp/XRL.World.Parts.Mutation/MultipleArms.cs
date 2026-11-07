using System;
using XRL.Rules;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MultipleArms : BaseMutation, IRankedMutation
{
	public int Rank = 1;

	public int Pairs = 1;

	public string AdditionsManagerID => ParentObject.ID + "::MultipleArms::Add";

	public string ChangesManagerID => ParentObject.ID + "::MultipleArms::Change";

	public override bool AffectsBodyParts()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You have an extra set of arms.";
	}

	public override string GetLevelText(int Level)
	{
		return "{{rules|" + GetAttackChance(Level) + "%}} chance for each extra arm to deliver an additional melee attack whenever you make a melee attack";
	}

	public int GetAttackChance(int Level)
	{
		return 7 + Level * 3;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public int GetRank()
	{
		return Rank;
	}

	public int AdjustRank(int Amount)
	{
		Rank += Amount;
		for (int i = 0; i < Amount; i++)
		{
			AddMoreArms(ParentObject);
		}
		return Rank;
	}

	public void AddMoreArms(GameObject Subject)
	{
		Body body = Subject?.Body;
		if (body == null)
		{
			return;
		}
		BodyPart body2 = body.GetBody();
		BodyPart firstAttachedPart = body2.GetFirstAttachedPart("Arm", 2, body, EvenIfDismembered: true);
		BodyPart bodyPart = firstAttachedPart?.GetFirstAttachedPart("Hand", 2, body, EvenIfDismembered: true);
		BodyPart firstAttachedPart2 = body2.GetFirstAttachedPart("Arm", 1, body, EvenIfDismembered: true);
		BodyPart bodyPart2 = firstAttachedPart2?.GetFirstAttachedPart("Hand", 1, body, EvenIfDismembered: true);
		BodyPart firstAttachedPart3 = body2.GetFirstAttachedPart("Hands", 0, body, EvenIfDismembered: true);
		BodyPart bodyPart3 = firstAttachedPart2;
		BodyPart bodyPart4 = firstAttachedPart3;
		bool flag = false;
		for (int i = 0; i < Pairs; i++)
		{
			string text = ((i == 0) ? "Multiple Arms Hands" : ("Multiple Arms Hands " + (i + 1)));
			if (i == 0 && firstAttachedPart != null && firstAttachedPart.Manager == null && bodyPart != null && bodyPart.Manager == null && firstAttachedPart2 != null && firstAttachedPart2.Manager == null && bodyPart2 != null && bodyPart2.Manager == null && firstAttachedPart3 != null && firstAttachedPart3.Manager == null && firstAttachedPart.IsLateralitySafeToChange(2, body, bodyPart) && firstAttachedPart2.IsLateralitySafeToChange(1, body, bodyPart2) && firstAttachedPart3.IsLateralitySafeToChange(0, body) && firstAttachedPart3.DependsOn == bodyPart.SupportsDependent && firstAttachedPart3.DependsOn == bodyPart2.SupportsDependent)
			{
				(bodyPart3 = body2.AddPartAt(bodyPart3, "Arm", 10, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID)).AddPart("Hand", 10, null, text, null, null, Category: body2.Category, Manager: AdditionsManagerID);
				(bodyPart3 = body2.AddPartAt(bodyPart3, "Arm", 9, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID)).AddPart("Hand", 9, null, text, null, null, Category: body2.Category, Manager: AdditionsManagerID);
				bodyPart4 = body2.AddPartAt(bodyPart4, "Hands", 8, null, null, text, null, Category: body2.Category, Manager: AdditionsManagerID);
				ProcessChangedLimb(firstAttachedPart.ChangeLaterality(firstAttachedPart.Laterality | 4));
				ProcessChangedLimb(bodyPart.ChangeLaterality(bodyPart.Laterality | 4));
				ProcessChangedLimb(firstAttachedPart2.ChangeLaterality(firstAttachedPart2.Laterality | 4));
				ProcessChangedLimb(bodyPart2.ChangeLaterality(bodyPart2.Laterality | 4));
				ProcessChangedLimb(firstAttachedPart3.ChangeLaterality(firstAttachedPart3.Laterality | 4));
				flag = true;
				continue;
			}
			int num = 2;
			int num2 = 1;
			int num3 = 0;
			if ((i == 1) & flag)
			{
				num |= 0x20;
				num2 |= 0x20;
				num3 |= 0x20;
			}
			BodyPart bodyPart5;
			int? num4;
			string[] orInsertBefore;
			if (bodyPart3 != null)
			{
				bodyPart5 = (bodyPart3 = body2.AddPartAt(bodyPart3, "Arm", num, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID));
			}
			else
			{
				int laterality = num;
				num4 = body2.Category;
				string additionsManagerID = AdditionsManagerID;
				int? category = num4;
				orInsertBefore = new string[4] { "Hands", "Feet", "Roots", "Thrown Weapon" };
				bodyPart5 = (bodyPart3 = body2.AddPartAt("Arm", laterality, null, null, null, null, additionsManagerID, category, null, null, null, null, null, null, null, null, null, null, null, null, "Arm", orInsertBefore));
			}
			bodyPart5.AddPart("Hand", num, null, text, null, null, Category: body2.Category, Manager: AdditionsManagerID);
			body2.AddPartAt(bodyPart3, "Arm", num, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID).AddPart("Hand", num, null, text, null, null, Category: body2.Category, Manager: AdditionsManagerID);
			if (bodyPart4 != null)
			{
				bodyPart4 = body2.AddPartAt(bodyPart4, "Hands", num3, null, null, text, null, Category: body2.Category, Manager: AdditionsManagerID);
				continue;
			}
			int laterality2 = num3;
			num4 = body2.Category;
			string additionsManagerID2 = AdditionsManagerID;
			int? category2 = num4;
			orInsertBefore = new string[3] { "Feet", "Roots", "Thrown Weapon" };
			bodyPart4 = body2.AddPartAt("Hands", laterality2, null, null, text, null, additionsManagerID2, category2, null, null, null, null, null, null, null, null, null, null, null, null, "Hands", orInsertBefore);
		}
		Subject.WantToReequip();
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		AddMoreArms(GO);
		return base.Mutate(GO, Level);
	}

	private BodyPart ProcessChangedLimb(BodyPart Part)
	{
		if (Part != null)
		{
			Part.Manager = ChangesManagerID;
		}
		return Part;
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.RemoveBodyPartsByManager(AdditionsManagerID, EvenIfDismembered: true);
		foreach (BodyPart item in GO.GetBodyPartsByManager(ChangesManagerID, EvenIfDismembered: true))
		{
			if (item.HasLaterality(4) && item.IsLateralityConsistent())
			{
				item.ChangeLaterality(item.Laterality & -5);
			}
		}
		GO.WantToReequip();
		return base.Unmutate(GO);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetMeleeAttackChanceEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("chance", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (E.Intrinsic && !E.Primary && E.BodyPart?.Manager != null && E.BodyPart.Manager == AdditionsManagerID)
		{
			E.Chance += GetAttackChance(Level) - RuleSettings.BASE_SECONDARY_ATTACK_CHANCE;
		}
		return base.HandleEvent(E);
	}
}
