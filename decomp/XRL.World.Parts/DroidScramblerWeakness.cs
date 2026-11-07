using System;

namespace XRL.World.Parts;

[Serializable]
public class DroidScramblerWeakness : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterDieEvent.ID)
		{
			return ID == PooledEvent<BeforeSetFeelingEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterDieEvent E)
	{
		if (IsScrambler(E.Killer))
		{
			ParentObject.SetIntProperty("NoXP", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetFeelingEvent E)
	{
		if (E.Feeling < 0)
		{
			DroidScrambler.CheckScramblingFactions(The.Game.TimeTicks);
			if (IsScrambler(E.Target))
			{
				E.Feeling = 0;
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeSetFeelingEvent E)
	{
		if (E.Feeling < 0 && IsScrambler(E.Target))
		{
			if (!E.Target.IsPlayer() && E.Target.Target == ParentObject)
			{
				E.Target.StopFighting();
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool IsScrambler(GameObject Object)
	{
		string text = Object?.CurrentZone?.ZoneID;
		if (text != null && DroidScrambler.Scrambled.TryGetValue(text, out var value) && value.Contains(DroidScrambler.GetScrambledFaction(Object)))
		{
			return true;
		}
		string text2 = ParentObject?.CurrentZone?.ZoneID;
		if (text2 != null && (object)text != text2 && DroidScrambler.Scrambled.TryGetValue(text2, out value) && value.Contains(DroidScrambler.GetScrambledFaction(Object)))
		{
			return true;
		}
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AIBeginKill");
		Registrar.Register(PooledEvent<GetFeelingEvent>.ID);
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIBeginKill")
		{
			GameObject target = ParentObject.Target;
			if (target != null)
			{
				DroidScrambler.CheckScramblingFactions(The.Game.TimeTicks);
				if (IsScrambler(target))
				{
					ParentObject.StopFighting();
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}
}
