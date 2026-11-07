using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class ChangeEquipment : GoalHandler
{
	public string ToUnequip;

	public ChangeEquipment()
	{
	}

	public ChangeEquipment(string ToUnequip)
		: this()
	{
		this.ToUnequip = ToUnequip;
	}

	public ChangeEquipment(GameObject ToUnequip)
		: this(ToUnequip.ID)
	{
	}

	public override bool Finished()
	{
		return !ParentBrain.ParentObject.HasEquippedItem((GameObject obj) => obj.IDMatch(ToUnequip));
	}

	public override void TakeAction()
	{
		GameObject gameObject = ParentBrain.ParentObject.FindEquippedItem((GameObject obj) => obj.IDMatch(ToUnequip));
		if (gameObject == null)
		{
			FailToParent();
		}
		else if (!gameObject.TryUnequip())
		{
			FailToParent();
		}
		else
		{
			ParentBrain.PerformReequip(Silent: false, DoPrimaryChoice: false, Initial: false, gameObject);
		}
	}
}
