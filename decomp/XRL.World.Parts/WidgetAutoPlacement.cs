using System;

namespace XRL.World.Parts;

[Serializable]
public class WidgetAutoPlacement : IPart
{
	public bool Placed;

	public string Widget;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && !Placed)
		{
			Placed = true;
			GameObject randomElement = ParentObject.CurrentCell.ParentZone.GetObjects((GameObject o) => o.Blueprint == Widget).GetRandomElement();
			if (randomElement != null)
			{
				ParentObject.DirectMoveTo(randomElement.CurrentCell);
			}
		}
		return base.FireEvent(E);
	}
}
