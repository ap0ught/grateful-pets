using System;

namespace XRL.World.Parts;

[Serializable]
public class ModLanterned : IModification
{
	public ModLanterned()
	{
	}

	public ModLanterned(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return !Object.HasPart<LightSource>();
	}

	public override void ApplyModification(GameObject Object)
	{
		if (!Object.HasPart<LightSource>())
		{
			LightSource lightSource = new LightSource();
			lightSource.Lit = true;
			lightSource.Radius = Tier + 1;
			Object.AddPart(lightSource);
		}
		IncreaseComplexityIfComplex(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{lanterned|lanterned}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("stars", 1);
		}
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Lanterned: This item provides light.";
	}
}
