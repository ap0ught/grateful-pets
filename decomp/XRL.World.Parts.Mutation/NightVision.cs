using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class NightVision : BaseMutation
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
	}

	public override string GetDescription()
	{
		return "";
	}

	public override string GetLevelText(int Level)
	{
		return "You see in the dark.\n";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (ParentObject.IsPlayer())
		{
			AddLight(Level, LightLevel.Darkvision);
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
