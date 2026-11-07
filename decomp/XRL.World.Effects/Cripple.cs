using System;
using XRL.Core;
using XRL.Messages;

namespace XRL.World.Effects;

[Serializable]
public class Cripple : Effect
{
	public Cripple()
	{
		DisplayName = "{{R|crippled}}";
	}

	public Cripple(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 100664320;
	}

	public override string GetDetails()
	{
		return "-50 Quickness";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.Statistics.ContainsKey("Energy"))
		{
			return false;
		}
		if (!Object.Statistics.ContainsKey("Speed"))
		{
			return false;
		}
		if (Object.HasEffect<Cripple>())
		{
			return false;
		}
		if (Object.FireEvent("ApplyCripple"))
		{
			if (Object.IsPlayer())
			{
				Object.ParticleText("*crippled*", 'R');
				MessageQueue.AddPlayerMessage("You are crippled for " + Duration.Things("turn") + "!", 'R');
			}
			else
			{
				Object.ParticleText("*crippled*", 'C');
			}
			Object.ForfeitTurn();
			ApplyStats();
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift(Object, "Speed", -50);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("Recuperating");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 20 && num < 25)
		{
			E.RenderString = "X";
			E.ColorString = "&R^c";
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Recuperating")
		{
			Duration = 0;
			DidX("are", "no longer crippled", "!", null, null, Object);
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}
