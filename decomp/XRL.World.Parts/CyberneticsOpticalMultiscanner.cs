using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsOpticalMultiscanner : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetCyberneticsBehaviorDescriptionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCyberneticsBehaviorDescriptionEvent E)
	{
		if (Options.AnySifrah)
		{
			if (Options.SifrahExamine)
			{
				E.Description = "You gain access to the precise hit point, armor, and dodge values of robotic creatures, biological creatures, and structures.\nStaircases and other up/down map transitions are always revealed to you.";
			}
			E.Add("Adds a bonus turn, and is otherwise useful, in most tinkering Sifrah games, and is useful in many social Sifrah games.");
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
