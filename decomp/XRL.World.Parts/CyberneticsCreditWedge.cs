using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCreditWedge : IPart
{
	public int Credits;

	public override bool SameAs(IPart p)
	{
		if ((p as CyberneticsCreditWedge).Credits != Credits)
		{
			return false;
		}
		return base.SameAs(p);
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
		E.AddBase("{{C|" + Credits + "\u009b}}", 20);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.Append("\nCredits remaining: {{C|").Append(Credits).Append('\u009b')
			.Append("}}");
		return base.HandleEvent(E);
	}
}
