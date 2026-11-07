using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class FuriouslyConfused : Effect
{
	public int Level;

	public int AppliedPlayerConfusion;

	public int MentalPenalty;

	public FuriouslyConfused()
	{
		DisplayName = "{{R|FURIOUS}}";
	}

	public FuriouslyConfused(int Duration, int Level, int MentalPenalty)
		: this()
	{
		base.Duration = Duration;
		this.Level = Level;
		this.MentalPenalty = MentalPenalty;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Acts semi-randomly.\n-").Append(Level).Append(" DV\n-")
			.Append(Level)
			.Append(" MA");
		if (MentalPenalty > 0)
		{
			stringBuilder.Append("\n-").Append(MentalPenalty).Append(" to all mental attributes");
		}
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.Brain == null)
		{
			return false;
		}
		if (!Object.FireEvent("CanApplyConfusion"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyConfusion"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Confusion", this))
		{
			return false;
		}
		if (Object.HasEffect<Confused>())
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_enraged_big");
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_bewilderment");
		ApplyChanges();
		DidX("become", "confused", "!", null, null, null, Object);
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		if (!Object.IsPlayer())
		{
			Object.Brain.Goals.Clear();
			Object.Brain.Target = null;
		}
		UnapplyChanges();
		base.Remove(Object);
	}

	private void ApplyChanges()
	{
		StatShifter.SetStatShift(Object, "DV", -Level);
		StatShifter.SetStatShift(Object, "MA", -Level);
		StatShifter.SetStatShift(Object, "Willpower", -MentalPenalty);
		StatShifter.SetStatShift(Object, "Intelligence", -MentalPenalty);
		StatShifter.SetStatShift(Object, "Ego", -MentalPenalty);
	}

	private void UnapplyChanges()
	{
		StatShifter.RemoveStatShifts();
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration == 0)
		{
			return true;
		}
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 60)
		{
			E.Tile = null;
			E.RenderString = "?";
			E.ColorString = "&R";
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == GetLostChanceEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			Duration--;
		}
		if (Duration > 0 && !Object.IsPlayer())
		{
			int num = Stat.Random(1, 100);
			if (num <= 50)
			{
				Object.Move(Directions.GetRandomDirection());
				Object.UseEnergy(Object.Energy.Value);
				return false;
			}
			if (num <= 60)
			{
				GameObject gameObject = Object;
				Cell cell = currentCell;
				object target;
				if (cell == null)
				{
					target = null;
				}
				else
				{
					List<Cell> localAdjacentCells = cell.GetLocalAdjacentCells(5);
					if (localAdjacentCells == null)
					{
						target = null;
					}
					else
					{
						Cell randomElement = localAdjacentCells.GetRandomElement();
						if (randomElement == null)
						{
							target = null;
						}
						else
						{
							List<GameObject> objectsInCell = randomElement.GetObjectsInCell();
							target = ((objectsInCell != null) ? objectsInCell.GetRandomElement() : null);
						}
					}
				}
				gameObject.Target = (GameObject)target;
				Object.UseEnergy(Object.Energy.Value);
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetLostChanceEvent E)
	{
		if (E.Actor == Object)
		{
			E.PercentageBonus -= Level * 100;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("CanApplyConfusion");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyConfusion")
		{
			return false;
		}
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
		}
		return base.FireEvent(E);
	}
}
