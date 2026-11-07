using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class WallWalker : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You can move across walls, and only across walls.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (GO.Brain != null)
		{
			GO.Brain.WallWalker = true;
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		if (GO.Brain != null)
		{
			GO.Brain.WallWalker = false;
		}
		return base.Unmutate(GO);
	}
}
