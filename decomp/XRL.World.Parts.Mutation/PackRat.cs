using System;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class PackRat : BaseMutation
{
	public int DropCooldown;

	public bool Entered;

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("BeginDrop");
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You compulsively lug around everything you can.\n\nYou must maintain at least 90% of your carry capacity.\n\nYou cannot drop items if dropping them would reduce your weight beneath this requirement.\n\nYou suffer one point of damage each round you do not maintain this requirement.\n\nYou can only drop one set of items every 10 rounds.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && !Entered)
		{
			Inventory inventory = ParentObject.Inventory;
			int maxCarriedWeight = ParentObject.GetMaxCarriedWeight();
			while (ParentObject.GetCarriedWeight() < (int)((double)maxCarriedWeight * 0.9))
			{
				inventory.AddObject(PopulationManager.CreateOneFrom("Junk 1"));
			}
			Entered = true;
			ParentObject.UnregisterPartEvent(this, "EnteredCell");
		}
		if (E.ID == "BeginTakeAction")
		{
			int maxCarriedWeight2 = ParentObject.GetMaxCarriedWeight();
			if (ParentObject.GetCarriedWeight() < (int)((double)maxCarriedWeight2 * 0.9) && ParentObject.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("&RYou must collect more junk! (minimum: " + (int)((double)maxCarriedWeight2 * 0.9) + " lbs.)");
				ParentObject.Statistics["Hitpoints"].Penalty++;
			}
			if (DropCooldown > 0)
			{
				DropCooldown--;
			}
			return true;
		}
		if (E.ID == "BeginDrop")
		{
			if (E.HasFlag("Forced") || E.HasFlag("ForEquip"))
			{
				return true;
			}
			_ = ParentObject.Inventory;
			_ = ParentObject.Body;
			int maxCarriedWeight3 = ParentObject.GetMaxCarriedWeight();
			int carriedWeight = ParentObject.GetCarriedWeight();
			GameObject gameObject = E.GetParameter("Object") as GameObject;
			if (carriedWeight - gameObject.Weight < (int)((double)maxCarriedWeight3 * 0.9))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("That wouldn't leave you with NEARLY enough junk! You can't drop that!");
				}
				return false;
			}
			if (DropCooldown == 0)
			{
				DropCooldown = 10;
				return true;
			}
			if (ParentObject.IsPlayer())
			{
				Popup.Show("You must wait " + DropCooldown.Things("more turn", "more turns") + " to work up the willpower to drop something!");
			}
			return false;
		}
		return true;
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
