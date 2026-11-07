using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Slumberling : IPart
{
	public bool Initial = true;

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckHibernate(Amount);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == EnteredCellEvent.ID)
			{
				return Initial;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Initial)
		{
			Initial = false;
			ParentObject.ApplyEffect(new Asleep(9999, forced: false, quicksleep: false, Voluntary: true));
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool CheckHibernate(int Chances = 1)
	{
		if (ParentObject.HasEffect<Asleep>())
		{
			return true;
		}
		for (int i = 0; i < Chances; i++)
		{
			if (10.in100())
			{
				if (ParentObject.ApplyEffect(new Asleep(9999, forced: false, quicksleep: false, Voluntary: true)))
				{
					DidX("lapse", "back into hibernation", null, null, null, null, ParentObject);
					return true;
				}
				return false;
			}
		}
		return false;
	}
}
