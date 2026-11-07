using System;

namespace XRL.World.Parts;

[Serializable]
public class Uplift : IPart
{
	public string AdditionalBaseTemplate;

	public string AdditionalSpecializationTemplate;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (E.ReplacementObject == null)
		{
			HeroMaker.MakeHero(ParentObject, AdditionalBaseTemplate, AdditionalSpecializationTemplate);
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}
