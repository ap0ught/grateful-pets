using System;

namespace XRL.World.Parts;

[Serializable]
public class RandomDetailColor : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectCreated");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectCreated")
		{
			if (!ParentObject.HasPart<Render>())
			{
				ParentObject.AddPart(new Render());
			}
			char[] list = new char[15]
			{
				'y', 'Y', 'b', 'B', 'c', 'C', 'r', 'R', 'g', 'G',
				'm', 'M', 'k', 'w', 'W'
			};
			ParentObject.GetPart<Render>().DetailColor = list.GetRandomElement().ToString();
			ParentObject.RemovePart(this);
			return true;
		}
		return base.FireEvent(E);
	}
}
