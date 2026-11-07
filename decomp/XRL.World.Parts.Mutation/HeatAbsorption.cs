using System;
using XRL.Messages;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HeatAbsorption : BaseMutation
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeApplyDamage");
		Registrar.Register("BeforeTemperatureChange");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You regenerate by absorbing heat.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Immune to heat damage\n";
		if (Level > 1)
		{
			text = text + "Whenever you would have taken heat damage, you heal for " + (Level - 1) * 10 + "% of that damage instead";
		}
		return text;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.HasAttribute("Fire") || damage.HasAttribute("Heat"))
			{
				if (Level > 1)
				{
					int num = (int)Math.Ceiling((float)damage.Amount * (0.1f * (float)(Level - 1)));
					if (num > 0 && ParentObject.Statistics["Hitpoints"].Penalty > 0)
					{
						if (ParentObject.IsPlayer())
						{
							MessageQueue.AddPlayerMessage("You are healed for " + num + " by the heat.");
						}
						ParentObject.Heal(num);
					}
				}
				return false;
			}
		}
		else if (E.ID == "BeforeTemperatureChange")
		{
			int intParameter = E.GetIntParameter("Amount");
			if (E.HasFlag("Radiant"))
			{
				if (intParameter > 0 && ParentObject.Physics.Temperature > 0)
				{
					E.SetParameter("Amount", (int)((double)(25 - ParentObject.Physics.Temperature) / 0.05));
				}
			}
			else if (intParameter > 0 && ParentObject.Physics.Temperature > 0)
			{
				E.SetParameter("Amount", 25 - ParentObject.Physics.Temperature);
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
