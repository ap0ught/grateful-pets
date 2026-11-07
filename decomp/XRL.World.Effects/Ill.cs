using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Ill : Effect, ITierInitialized
{
	public int Level = 1;

	public string Message = "The poison begins to abate, but you still feel nauseous.";

	public bool StartMessageUsePopup;

	public bool StopMessageUsePopup;

	public Ill()
	{
		DisplayName = "{{g|illness}}";
	}

	public Ill(int Duration, int Level = 1, string Message = null, bool StartMessageUsePopup = false, bool StopMessageUsePopup = false)
		: this()
	{
		base.Duration = Duration;
		this.Message = Message;
		this.StartMessageUsePopup = StartMessageUsePopup;
		this.StopMessageUsePopup = StopMessageUsePopup;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(30, 200);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override string GetDescription()
	{
		return "{{g|ill}}";
	}

	public override string GetDetails()
	{
		return "Doesn't heal hit points naturally.\nExternal healing is only half as effective.";
	}

	public override bool Apply(GameObject Object)
	{
		Ill effect = Object.GetEffect<Ill>();
		if (effect != null)
		{
			if (Duration > effect.Duration)
			{
				effect.Duration = Duration;
			}
			return false;
		}
		if (Object.FireEvent("ApplyIll"))
		{
			if (!Message.IsNullOrEmpty() && Object.IsPlayer())
			{
				IComponent<GameObject>.EmitMessage(Object, Message, ' ', FromDialog: false, StartMessageUsePopup, AlwaysVisible: false, null, Object);
			}
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.EmitMessage(Object, "You no longer feel ill.", ' ', FromDialog: false, StopMessageUsePopup, AlwaysVisible: false, Object);
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Healing");
		Registrar.Register("Recuperating");
		Registrar.Register("Regenerating");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Healing")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") / 2);
		}
		else
		{
			if (E.ID == "Regenerating")
			{
				E.SetParameter("Amount", 0);
				return false;
			}
			if (E.ID == "Recuperating")
			{
				Duration = 0;
				DidX("are", "no longer ill", null, null, null, Object);
			}
		}
		return base.FireEvent(E);
	}
}
