using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Myopia : BaseMutation
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
		return "You are nearsighted.\n\nYou can only see up to a radius of 10.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
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

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<AdjustVisibilityRadiusEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AdjustVisibilityRadiusEvent E)
	{
		if (!ParentObject.HasEffect<XRL.World.Effects.Spectacles>())
		{
			E.Radius = 10;
		}
		return base.HandleEvent(E);
	}
}
