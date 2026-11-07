using System;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class Food : IPart
{
	public static readonly int SMALL_HUNGER_AMOUNT = 200;

	public int Thirst;

	public string Satiation = "None";

	public bool Gross;

	public bool IllOnEat;

	public string Healing = "0";

	public string Message = "That hits the spot!";

	public override bool SameAs(IPart p)
	{
		Food food = p as Food;
		if (food.Thirst != Thirst)
		{
			return false;
		}
		if (food.Satiation != Satiation)
		{
			return false;
		}
		if (food.Gross != Gross)
		{
			return false;
		}
		if (food.IllOnEat != IllOnEat)
		{
			return false;
		}
		if (food.Healing != Healing)
		{
			return false;
		}
		if (food.Message != Message)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveItemListEvent.ID && ID != PooledEvent<GetAutoEquipPriorityEvent>.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		if (Healing != "0" && !ParentObject.IsImportant() && E.Actor.HasPart<Stomach>())
		{
			int num = Healing.RollMinCached();
			if (num >= 0)
			{
				int num2 = Healing.RollMaxCached();
				if (num2 > 0)
				{
					int hitpoints = E.Actor.hitpoints;
					int baseHitpoints = E.Actor.baseHitpoints;
					if (hitpoints < baseHitpoints)
					{
						int num3 = 0;
						int num4 = baseHitpoints - hitpoints;
						if (num4 >= num)
						{
							num3++;
						}
						if (num4 >= num2)
						{
							num3++;
						}
						if (num4 >= baseHitpoints * 9 / 10)
						{
							num3++;
						}
						if (num3 > 0)
						{
							E.Add("Eat", num3, ParentObject, Inv: true, Self: true);
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAutoEquipPriorityEvent E)
	{
		E.Priority -= E.Default;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		int num = 20;
		if (Healing == "0" && !ParentObject.HasPart<HealOnEat>() && !ParentObject.HasPart<GeometricHealOnEat>())
		{
			num = 0;
		}
		else if (!E.Actor.HasPart<Stomach>())
		{
			num = 0;
		}
		E.AddAction("Eat", "eat", "Eat", null, 'a', FireOnActor: false, num);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Eat")
		{
			if (!E.Actor.CanMoveExtremities("Eat", ShowMessage: true))
			{
				return false;
			}
			Stomach part = E.Actor.GetPart<Stomach>();
			if (part == null)
			{
				E.Actor.Fail("You are unable to consume food.");
				return false;
			}
			if (Gross && (!E.Actor.HasPart<Carnivorous>() || !ParentObject.HasTag("Meat")) && (part == null || (part.HungerLevel < 2 && !E.Actor.HasSkill("Discipline_MindOverBody"))))
			{
				E.Actor.Fail("You're not hungry enough to bring " + E.Actor.itself + " to eat that.");
				return true;
			}
			if (!BeforeConsumeEvent.Check(E.Actor, E.Actor, E.Item, Eat: true))
			{
				return false;
			}
			Event obj = Event.New("OnEat");
			obj.SetParameter("Actor", E.Actor);
			obj.SetParameter("Eater", E.Actor);
			obj.SetParameter("Subject", E.Actor);
			obj.SetParameter("Food", ParentObject);
			obj.SetParameter("Object", ParentObject);
			if (!ParentObject.FireEvent(obj, E))
			{
				return false;
			}
			_ = part.HungerLevel;
			obj.ID = "Eating";
			if (!E.Actor.FireEvent(obj))
			{
				return false;
			}
			E.Actor.FireEvent(Event.New("AddWater", "Amount", Thirst));
			E.Actor.Heal(Healing.RollCached(), Message: false, FloatText: true, RandomMinimum: true);
			if (E.Actor.HasPart<Stomach>())
			{
				if (!E.Actor.HasPart<Carnivorous>() || ParentObject.HasTag("Meat"))
				{
					if (Satiation == "Snack")
					{
						part.CookingCounter -= SMALL_HUNGER_AMOUNT;
					}
					if (Satiation == "Meal")
					{
						part.CookingCounter = 0;
					}
				}
				if (E.Actor.IsPlayer())
				{
					string text = Message ?? "";
					if (text != "")
					{
						if (text.Contains("~"))
						{
							text = text.Split('~')[0];
						}
						text = GameText.VariableReplace(text, ParentObject, E.Actor);
						if (!text.EndsWith("!") && !text.EndsWith(".") && !text.EndsWith("?"))
						{
							text += ".";
						}
						if (!text.EndsWith("\n"))
						{
							text += "\n";
						}
					}
					Popup.Show("You eat " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".\n" + text + "You are now {{|" + part.FoodStatus() + "}} and {{|" + part.WaterStatus() + "}}.");
				}
			}
			AfterConsumeEvent.Send(E.Actor, E.Actor, E.Item, Eat: true);
			obj.ID = "Eaten";
			ParentObject.FireEvent(obj);
			if (IllnessChance(E.Actor).in100())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("Ugh, you feel sick.");
				}
				E.Actor.ApplyEffect(new Ill(100));
			}
			E.Actor.UseEnergy(1000, "Item Eat");
			E.RequestInterfaceExit();
			obj.ID = "AfterEat";
			ParentObject.FireEvent(obj, E);
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}

	public virtual int IllnessChance(GameObject Actor)
	{
		if (Actor.HasPart<Carnivorous>())
		{
			if (ParentObject.HasTag("Meat"))
			{
				return 0;
			}
			if (!IllOnEat)
			{
				return 50;
			}
			return 100;
		}
		if (IllOnEat)
		{
			return 100;
		}
		return 0;
	}
}
