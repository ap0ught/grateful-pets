using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class AddObservationOnLook : IPart
{
	public string Text;

	public string Category = "Gossip";

	public string Attributes = "";

	public bool bLookedAt;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterLookedAt");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt")
		{
			if (bLookedAt)
			{
				return true;
			}
			bLookedAt = true;
			string text = Guid.NewGuid().ToString();
			JournalAPI.AddObservation(Text, text, Category, text, Attributes.Split(','), revealed: true, -1L);
			return true;
		}
		return base.FireEvent(E);
	}
}
