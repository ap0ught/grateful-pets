using System;
using Qud.API;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class AddObservation : IPart
{
	public string Text;

	public string Category = "Gossip";

	public string Attributes = "";

	public string ID;

	public bool bCreated;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectCreated");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectCreated" && XRLCore.Core.Game != null)
		{
			if (bCreated)
			{
				return true;
			}
			bCreated = true;
			if (ID == null)
			{
				ID = Guid.NewGuid().ToString();
			}
			JournalAPI.AddObservation(Text, ID, Category, ID, Attributes.Split(','), revealed: false, -1L);
			return true;
		}
		return base.FireEvent(E);
	}
}
