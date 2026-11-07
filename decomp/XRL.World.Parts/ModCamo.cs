using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class ModCamo : IModification
{
	public static readonly int ICON_COLOR_FOREGROUND_PRIORITY = 40;

	public static readonly int ICON_COLOR_DETAIL_PRIORITY = 80;

	public ModCamo()
	{
	}

	public ModCamo(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if ((Object.HasPart<Armor>() || Object.HasPart<Shield>()) && !Object.HasPart<FoliageCamouflage>())
		{
			return !Object.HasPart<UrbanCamouflage>();
		}
		return false;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<FoliageCamouflage>().Level = Math.Max(Tier / 2, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{camouflage|camo}}", 20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(GetPhysicalDescription());
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public string GetPhysicalDescription()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(ParentObject.It).Append(ParentObject.GetVerb("are")).Append(" covered in mottled green, brown, gray, and black markings.");
		return stringBuilder.ToString();
	}

	public static string GetDescription(int Tier)
	{
		return "Camo: This item provides camouflage in foliage.";
	}

	public override bool Render(RenderEvent E)
	{
		E.ApplyColors("&g", "K", ICON_COLOR_FOREGROUND_PRIORITY, ICON_COLOR_DETAIL_PRIORITY);
		return base.Render(E);
	}
}
