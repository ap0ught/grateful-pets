using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class GenerateName : IPart
{
	public string SpecialType;

	public string NamingContext;

	public bool SpecialFaildown = true;

	public override bool SameAs(IPart p)
	{
		GenerateName generateName = p as GenerateName;
		if (generateName.SpecialType != SpecialType)
		{
			return false;
		}
		if (generateName.NamingContext != NamingContext)
		{
			return false;
		}
		if (generateName.SpecialFaildown != SpecialFaildown)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ApplyName(E.Object);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void AddedAfterCreation()
	{
		ApplyName(ParentObject);
	}

	public void ApplyName(GameObject Object)
	{
		string specialType = SpecialType;
		Dictionary<string, string> namingContext = (NamingContext.IsNullOrEmpty() ? null : NamingContext.CachedDictionaryExpansion());
		Object.GiveProperName(null, Force: false, specialType, SpecialFaildown: false, null, null, namingContext);
	}
}
