using System;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Esper : BaseMutation
{
	public Esper()
	{
		Type = "Esper";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetPsionicSifrahSetupEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPsionicSifrahSetupEvent E)
	{
		E.Rating++;
		E.Turns++;
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		string text = "You only manifest mental mutations, and all of your mutation choices when manifesting a new mutation are mental.";
		if (Options.AnySifrah)
		{
			text += "\nAdds a bonus turn and improves performance in psionic Sifrah games.";
		}
		return text;
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}
}
