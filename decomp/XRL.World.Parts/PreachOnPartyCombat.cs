using System;

namespace XRL.World.Parts;

[Serializable]
public class PreachOnPartyCombat : IPart
{
	public bool leaderInCombat;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("CanPreach");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanPreach")
		{
			if (ParentObject.PartyLeader != null && ParentObject.PartyLeader.Brain != null && ParentObject.PartyLeader.Brain.Target != null)
			{
				return true;
			}
			return false;
		}
		if (E.ID == "BeginTakeAction" && ParentObject.PartyLeader != null && ParentObject.PartyLeader.Brain != null && leaderInCombat != (ParentObject.PartyLeader.Brain.Target != null))
		{
			if (!leaderInCombat && ParentObject.HasPart<Preacher>())
			{
				ParentObject.GetPart<Preacher>().PreacherHomily(Dialog: false);
			}
			leaderInCombat = !leaderInCombat;
		}
		return base.FireEvent(E);
	}
}
