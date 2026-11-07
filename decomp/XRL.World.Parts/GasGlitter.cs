using System;

namespace XRL.World.Parts;

[Serializable]
public class GasGlitter : IGasBehavior
{
	public string GasType = "Glitter";

	public override bool SameAs(IPart p)
	{
		if ((p as GasGlitter).GasType != GasType)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("RefractLight");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "RefractLight")
		{
			Gas part = ParentObject.GetPart<Gas>();
			if (part != null && part.Density.in100())
			{
				E.SetParameter("By", ParentObject);
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
