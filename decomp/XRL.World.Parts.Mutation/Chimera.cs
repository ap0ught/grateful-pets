using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Chimera : BaseMutation
{
	public Chimera()
	{
		Type = "Chimera";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You only manifest physical mutations, and all of your mutation choices when manifesting a new mutation are physical.\n\n" + "Whenever you manifest a new mutation, one of your choices will also cause you to grow a new limb at random.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}
}
