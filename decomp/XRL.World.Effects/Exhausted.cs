using System;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Exhausted : Effect, ITierInitialized
{
	public Exhausted()
	{
		DisplayName = "{{K|exhaustion}}";
	}

	public Exhausted(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = 3;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override string GetDetails()
	{
		return "Can't take actions.";
	}

	public override string GetStateDescription()
	{
		return "{{K|exhausted}}";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyExhausted")))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You are {{K|exhausted}}!");
		}
		Object.ParticleText("*exhausted*", 'C');
		Object.ForfeitTurn();
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == PooledEvent<IsConversationallyResponsiveEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			if (Duration != 9999)
			{
				Duration--;
			}
			if (Object.IsPlayer())
			{
				XRLCore.Core.RenderBase();
				Popup.Show("You are too exhausted to act!");
			}
			E.PreventAction = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == Object)
		{
			if (E.Mental && !E.Physical)
			{
				E.Message = Object.Poss("mind") + " is present, but doesn't seem to be responding to you at all.";
			}
			else
			{
				E.Message = Object.T() + Object.GetVerb("stare") + " at you dully.";
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanMoveExtremities");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanMoveExtremities" && Duration > 0 && !E.HasFlag("Involuntary"))
		{
			if (E.HasFlag("ShowMessage") && Object.IsPlayer())
			{
				Popup.ShowFail("You are too exhausted to do that.");
			}
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 10 && num < 25)
		{
			E.Tile = null;
			E.RenderString = "_";
			E.ColorString = "&C^c";
		}
		return true;
	}
}
