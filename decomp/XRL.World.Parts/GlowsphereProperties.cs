using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class GlowsphereProperties : IPart
{
	public string AllowedParts = "Hand";

	public override bool SameAs(IPart p)
	{
		if ((p as GlowsphereProperties).AllowedParts != AllowedParts)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("stars", 1);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Equipped");
		Registrar.Register("Unequipped");
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Equipped")
		{
			BodyPart bodyPart = E.GetParameter("BodyPart") as BodyPart;
			if (AllowedParts.Contains(bodyPart.Type))
			{
				LightSource part = ParentObject.GetPart<LightSource>();
				if (part != null)
				{
					part.Lit = true;
				}
			}
			else
			{
				LightSource part2 = ParentObject.GetPart<LightSource>();
				if (part2 != null)
				{
					part2.Lit = false;
				}
			}
		}
		if (E.ID == "Unequipped")
		{
			LightSource part3 = ParentObject.GetPart<LightSource>();
			if (part3 != null)
			{
				part3.Lit = true;
			}
			return true;
		}
		if (E.ID == "EnteredCell")
		{
			LightSource part4 = ParentObject.GetPart<LightSource>();
			if (part4 != null)
			{
				part4.Lit = true;
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
