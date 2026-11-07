using System;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

/// This part is not used in the base game.
[Serializable]
public class WeakHeart : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override string GetDescription()
	{
		return "Your heart is weak.\n\n-5 to save vs. poison, disease, and cardiac arrest.\nSmall chance per turn of entering cardiac arrest.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!ParentObject.OnWorldMap() && 1.in100() && 1.in100())
		{
			ParentObject.ApplyEffect(new CardiacArrest());
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Poison,Disease,CardiacArrest", E))
		{
			E.Roll -= 5;
		}
		return base.HandleEvent(E);
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
