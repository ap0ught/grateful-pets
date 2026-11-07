using System;

namespace XRL.World.Parts;

[Serializable]
public class Notes : IPart
{
	public string Text;

	public override bool SameAs(IPart p)
	{
		if ((p as Notes).Text != Text)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != GetUnknownShortDescriptionEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(Text))
		{
			E.Postfix.Append("\n{{playernotes|Notes: ").Append(Text).Append("}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(Text))
		{
			E.Postfix.Append("\n{{playernotes|Notes: ").Append(Text).Append("}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}
}
