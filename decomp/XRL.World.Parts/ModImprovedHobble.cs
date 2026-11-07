using System;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedHobble : IModification
{
	public bool Applied;

	public ModImprovedHobble()
	{
	}

	public ModImprovedHobble(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnEquipper = true;
		NameForStatus = "ImprovedHobble";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != ImplantedEvent.ID && ID != GetShortDescriptionEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Your Hobble cooldown is reduced by 5 rounds.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (ParentObject.IsEquippedProperly())
		{
			E.Actor.ModIntProperty("ImprovedHobble", 1, RemoveIfZero: true);
			Applied = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		if (Applied)
		{
			Applied = false;
			E.Actor.ModIntProperty("ImprovedHobble", -1, RemoveIfZero: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		if (ParentObject.IsEquippedProperly())
		{
			E.Implantee.ModIntProperty("ImprovedHobble", 1, RemoveIfZero: true);
			Applied = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		if (Applied)
		{
			Applied = false;
			E.Actor.ModIntProperty("ImprovedHobble", -1, RemoveIfZero: true);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
