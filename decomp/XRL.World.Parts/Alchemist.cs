using System;

namespace XRL.World.Parts;

[Serializable]
public class Alchemist : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetOffensiveItemListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveItemListEvent E)
	{
		E.Add("AlchemistExplode");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AlchemistExplode")
		{
			ParentObject.Explode(15000, null, "10d10+250", 1f, Neutron: true);
		}
		return base.FireEvent(E);
	}
}
