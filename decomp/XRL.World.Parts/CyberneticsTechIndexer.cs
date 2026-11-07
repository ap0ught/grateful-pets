using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsTechIndexer : IPart
{
	public string AffectedProperty = "TechScannerEquipped";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCyberneticsBehaviorDescriptionEvent>.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCyberneticsBehaviorDescriptionEvent E)
	{
		if (Options.AnySifrah)
		{
			if (Options.SifrahExamine)
			{
				E.Description = "You gain access to the precise hit point, armor, and dodge values of robotic creatures.";
			}
			E.Add("Adds a bonus turn, and is otherwise useful, in many tinkering Sifrah games, and is useful in some social Sifrah games involving robots.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.ModIntProperty(AffectedProperty, 1, RemoveIfZero: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.ModIntProperty(AffectedProperty, -1, RemoveIfZero: true);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
