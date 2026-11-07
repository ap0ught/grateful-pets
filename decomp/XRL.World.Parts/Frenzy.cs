using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Frenzy : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			GameObject partyLeader = ParentObject.PartyLeader;
			if ((partyLeader == null || partyLeader.IsInvalid() || partyLeader.CurrentCell == null) && !ParentObject.HasEffect<Frenzied>())
			{
				ParentObject.ApplyEffect(new Frenzied(Stat.Random(100, 200)));
			}
		}
		return base.FireEvent(E);
	}
}
