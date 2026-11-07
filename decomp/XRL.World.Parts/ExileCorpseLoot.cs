using System;

namespace XRL.World.Parts;

[Serializable]
public class ExileCorpseLoot : IPart
{
	public bool bCreated;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (bCreated)
			{
				return true;
			}
			bCreated = true;
			Cell cell = ParentObject.CurrentCell;
			cell.AddObject("Godshroom Cap");
			cell.AddObject("Godshroom Cap");
			cell.AddObject("Scroll Bound by Kelp");
			cell.AddObject("CyberneticsCreditWedge3");
			ParentObject.UnregisterPartEvent(this, "EnteredCell");
		}
		return base.FireEvent(E);
	}
}
