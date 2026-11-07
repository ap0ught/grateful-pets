using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Skittish : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You startle easily and engage your defense mechanisms.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public bool LoseControl()
	{
		if (ParentObject.Target != null)
		{
			ParentObject.DistanceTo(ParentObject.Target);
			List<AICommandList> list = AIGetOffensiveAbilityListEvent.GetFor(ParentObject);
			list.AddRange(AIGetPassiveAbilityListEvent.GetFor(ParentObject));
			list.AddRange(AIGetMovementAbilityListEvent.GetFor(ParentObject));
			if (list.Count > 0)
			{
				ParentObject.SetStringProperty("Skittishing", "1");
				try
				{
					if (AICommandList.HandleCommandList(list, ParentObject, ParentObject.Target) && ParentObject.IsPlayer())
					{
						Popup.Show("You are startled!");
					}
				}
				finally
				{
					ParentObject.RemoveStringProperty("Skittishing");
				}
				return true;
			}
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && 3.in1000() && LoseControl())
		{
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
