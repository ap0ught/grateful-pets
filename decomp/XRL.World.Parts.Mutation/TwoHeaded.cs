using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class TwoHeaded : BaseMutation
{
	private static List<Effect> targetEffects = new List<Effect>();

	public string AdditionsManagerID => ParentObject.ID + "::TwoHeaded::Add";

	public string ChangesManagerID => ParentObject.ID + "::TwoHeaded::Change";

	public override bool AffectsBodyParts()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == SingletonEvent<GetEnergyCostEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (E.Type != null && E.Type.Contains("Mental"))
		{
			E.PercentageReduction += GetReducedMentalActionCost(Level);
		}
		return base.HandleEvent(E);
	}

	public void PrintMessage(BodyPart Head, Effect Effect, bool Definite = true)
	{
		if (ParentObject.IsPlayerControlled() && ParentObject.IsVisible())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Compound(ParentObject.Poss(Head.GetOrdinalName())).Compound(Head.Plural ? "help " : "helps ").Append(ParentObject.them)
				.Append(" shake off ");
			string stateDescription = Effect.GetStateDescription();
			if (stateDescription.IsNullOrEmpty())
			{
				stringBuilder.Append(Definite ? "the effect!" : "a mental state!");
			}
			else
			{
				stringBuilder.Append("being ").Append(stateDescription).Append('!');
			}
			IComponent<GameObject>.AddPlayerMessage(stringBuilder.ToString(), 'g');
		}
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (AffectEffect(E.Effect) && GetShakeOff(Level).in100())
		{
			BodyPart bodyPart = FindExtraHead();
			if (bodyPart != null)
			{
				PrintMessage(bodyPart, E.Effect);
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (ParentObject.Effects != null && GetShakeOff(Level).in100())
		{
			targetEffects.Clear();
			int i = 0;
			for (int count = ParentObject.Effects.Count; i < count; i++)
			{
				if (AffectEffect(ParentObject.Effects[i]))
				{
					targetEffects.Add(ParentObject.Effects[i]);
				}
			}
			if (targetEffects.Count > 0)
			{
				BodyPart bodyPart = FindExtraHead();
				if (bodyPart != null)
				{
					Effect randomElement = targetEffects.GetRandomElement();
					if (randomElement != null)
					{
						PrintMessage(bodyPart, randomElement, Definite: false);
						ParentObject.RemoveEffect(randomElement);
					}
				}
			}
			targetEffects.Clear();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You have two heads.";
	}

	public int GetReducedMentalActionCost(int Level)
	{
		return 15 + 5 * Level;
	}

	public int GetShakeOff(int Level)
	{
		return 50;
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("Mental actions have {{rules|" + GetReducedMentalActionCost(Level) + "%}} lower action costs\n", "{{rules|", GetShakeOff(Level).ToString(), "%}} chance initially and each round to shake off a negative mental status effect");
	}

	public BodyPart FindExtraHead()
	{
		BodyPart bodyPartByManager = ParentObject.GetBodyPartByManager(AdditionsManagerID, "Head");
		if (bodyPartByManager == null)
		{
			return null;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			return null;
		}
		if (body.GetPartCount("Head") < 2)
		{
			return null;
		}
		return bodyPartByManager;
	}

	private bool AffectEffect(Effect FX)
	{
		if (FX.IsOfTypes(100663298))
		{
			return !FX.IsOfType(134217728);
		}
		return false;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		Body body = GO.Body;
		if (body != null)
		{
			BodyPart body2 = body.GetBody();
			BodyPart firstAttachedPart = body2.GetFirstAttachedPart("Head", 0, body, EvenIfDismembered: true);
			BodyPart bodyPart = firstAttachedPart?.GetFirstAttachedPart("Face", 0, body, EvenIfDismembered: true);
			if (firstAttachedPart != null && firstAttachedPart.Manager == null && bodyPart != null && bodyPart.Manager == null && firstAttachedPart.IsLateralitySafeToChange(0, body, bodyPart))
			{
				BodyPart bodyPart2 = body2.AddPartAt(firstAttachedPart, "Head", 1, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
				bodyPart2.AddPart("Face", 1, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
				firstAttachedPart.ChangeLaterality(2);
				bodyPart.ChangeLaterality(2);
				firstAttachedPart.Manager = ChangesManagerID;
				bodyPart.Manager = ChangesManagerID;
			}
			else
			{
				BodyPart bodyPart2;
				if (firstAttachedPart != null)
				{
					bodyPart2 = body2.AddPartAt(firstAttachedPart, "Head", 0, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
				}
				else
				{
					int? num = body2.Category;
					string additionsManagerID = AdditionsManagerID;
					int? category = num;
					string[] orInsertBefore = new string[8] { "Back", "Arm", "Leg", "Foot", "Hands", "Feet", "Roots", "Thrown Weapon" };
					bodyPart2 = body2.AddPartAt("Head", 0, null, null, null, null, additionsManagerID, category, null, null, null, null, null, null, null, null, null, null, null, null, "Head", orInsertBefore);
				}
				bodyPart2.AddPart("Face", 0, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
			}
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.RemoveBodyPartsByManager(AdditionsManagerID, EvenIfDismembered: true);
		foreach (BodyPart item in GO.GetBodyPartsByManager(ChangesManagerID))
		{
			if (item.Laterality == 2 && item.IsLateralityConsistent())
			{
				item.ChangeLaterality(0);
			}
		}
		return base.Unmutate(GO);
	}
}
