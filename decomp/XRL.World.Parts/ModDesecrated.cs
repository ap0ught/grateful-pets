using System;

namespace XRL.World.Parts;

[Serializable]
public class ModDesecrated : IModification
{
	public static readonly int ICON_COLOR_PRIORITY = 160;

	public ModDesecrated()
	{
	}

	public ModDesecrated(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
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
		if (!E.Understood() || !E.Object.HasProperName)
		{
			E.AddAdjective("{{K|desecrated}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Desecrated: This object has been desecrated by vandals.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		E.ApplyColors("&K", "r", ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY);
		return true;
	}
}
