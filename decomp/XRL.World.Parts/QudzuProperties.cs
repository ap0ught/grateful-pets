using System;

namespace XRL.World.Parts;

[Serializable]
public class QudzuProperties : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (ParentObject.CurrentCell.IsOccluding())
			{
				ParentObject.Render.ColorString = "&r^w";
			}
			else
			{
				ParentObject.Render.ColorString = "&r";
			}
		}
		return base.FireEvent(E);
	}
}
