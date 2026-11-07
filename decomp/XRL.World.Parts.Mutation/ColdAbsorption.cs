using System;
using XRL.Messages;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ColdAbsorption : BaseMutation
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeApplyDamage");
		Registrar.Register("BeforeTemperatureChange");
		base.Register(Object, Registrar);
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
			E.Add("ice", 1);
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You regenerate by absorbing cold.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Immune to heat damage\n";
		if (Level > 1)
		{
			text = text + "Whenever you would have taken cold damage, you heal for 0." + (Level - 1) + "% of that damage instead";
		}
		return text;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.HasAttribute("Ice") || damage.HasAttribute("Cold"))
			{
				if (Level > 1)
				{
					int num = (int)Math.Ceiling((float)damage.Amount * (0.1f * (float)(Level - 1)));
					if (num > 0 && ParentObject.Statistics["Hitpoints"].Penalty > 0)
					{
						if (ParentObject.IsPlayer())
						{
							MessageQueue.AddPlayerMessage("You are healed for " + num + " by the cold.");
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
				if (intParameter < 0 && (double)ParentObject.Physics.Temperature - (double)intParameter * 0.05 < 25.0)
				{
					E.SetParameter("Amount", (int)((double)(25 - ParentObject.Physics.Temperature) / 0.05));
				}
			}
			else if (intParameter < 0 && ParentObject.Physics.Temperature < 25)
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
