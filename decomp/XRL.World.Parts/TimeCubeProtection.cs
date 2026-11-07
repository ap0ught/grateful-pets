using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class TimeCubeProtection : IPart
{
	public string Message;

	public int HarmCount;

	public void IncrementHarm()
	{
		if (!ParentObject.HasEffect<Nullphased>() && ++HarmCount >= 3)
		{
			HarmCount = 0;
			ParentObject.ApplyEffect(new Nullphased(1));
			if (!Message.IsNullOrEmpty())
			{
				ParentObject.EmitMessage(Message.StartReplace().AddObject(ParentObject).ToString(), null, IComponent<GameObject>.ConsequentialColor(ParentObject));
			}
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != TookDamageEvent.ID)
		{
			return ID == EffectAppliedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if ((E.Actor != null && E.Actor.HasEffect<TimeCubed>()) || The.Player.HasEffect<TimeCubed>())
		{
			IncrementHarm();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (E.Effect.IsOfType(33554432) && The.Player.HasEffect<TimeCubed>())
		{
			IncrementHarm();
		}
		return base.HandleEvent(E);
	}
}
