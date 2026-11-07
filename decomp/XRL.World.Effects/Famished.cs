using System;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Famished : Effect, ITierInitialized
{
	public string mode = "famished";

	public int Penalty;

	public Famished()
	{
		Duration = 1;
	}

	public override int GetEffectType()
	{
		return 33554436;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		if (mode == "wilted")
		{
			return "{{R|wilted}}";
		}
		return "{{R|famished}}";
	}

	public override string GetDetails()
	{
		if (mode == "wilted")
		{
			return "-5 Quickness\n-10% to natural healing rate";
		}
		return "-10 Quickness";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasEffect(typeof(Famished)) && !Object.HasPart(typeof(Discipline_MindOverBody)) && Object.FireEvent("ApplyFamished"))
		{
			Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_negativeVitality");
			if (Object.HasPart(typeof(PhotosyntheticSkin)))
			{
				mode = "wilted";
				Penalty = 5;
			}
			else
			{
				Penalty = 10;
			}
			StatShifter.SetStatShift("Speed", -Penalty);
			if (Object.IsPlayer())
			{
				if (mode == "wilted")
				{
					Popup.Show("You have wilted! You'll move and regenerate slower until you eat or bask in the sunlight again.");
				}
				else
				{
					Popup.Show("You are famished! You'll act more slowly until you eat again.");
				}
				if (AutoAct.IsInterruptable())
				{
					AutoAct.Interrupt();
				}
			}
			return true;
		}
		return false;
	}

	public override void Applied(GameObject Object)
	{
		if (Object.TryGetPart<Stomach>(out var Part))
		{
			int num = Part.CalculateCookingIncrement() * 2;
			if (Part.CookingCounter < num)
			{
				Part.CookingCounter = num;
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Regenerating2" && mode == "wilted")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") * 9 / 10);
		}
		return base.FireEvent(E);
	}

	public override void Remove(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		Penalty = 0;
		base.Remove(Object);
	}
}
