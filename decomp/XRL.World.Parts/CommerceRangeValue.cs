using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class CommerceRangeValue : IPart
{
	public string Range = "1";

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
			if (!ParentObject.HasPart<Commerce>())
			{
				ParentObject.AddPart(new Commerce());
			}
			ParentObject.GetPart<Commerce>().Value = Stat.Roll(Range);
			ParentObject.RemovePart(this);
			return true;
		}
		return base.FireEvent(E);
	}
}
