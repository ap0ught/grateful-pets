using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Paralyzed : Effect, ITierInitialized
{
	public int DVPenalty;

	public int SaveTarget;

	public Paralyzed()
	{
		DisplayName = "{{C|paralyzed}}";
	}

	public Paralyzed(int Duration, int SaveTarget)
		: this()
	{
		this.SaveTarget = SaveTarget;
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Tier = Stat.Random(Tier - 1, Tier + 1);
		if (Tier < 1)
		{
			Tier = 1;
		}
		if (Tier > 8)
		{
			Tier = 8;
		}
		if (Tier < 2)
		{
			Duration = Stat.Roll("1d3+1");
		}
		else
		{
			Duration = Stat.Roll("1d3+" + Math.Min(7, Tier * 2 / 3 + 1));
		}
		SaveTarget = 14 + Tier * 2 * 2;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool SameAs(Effect e)
	{
		Paralyzed paralyzed = e as Paralyzed;
		if (paralyzed.DVPenalty != DVPenalty)
		{
			return false;
		}
		if (paralyzed.SaveTarget != SaveTarget)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		if (DVPenalty < 0)
		{
			return "Can't move or attack.\n" + DVPenalty + " DV";
		}
		return "Can't move or attack.\nDV set to 0.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Paralyzed>())
		{
			Paralyzed effect = Object.GetEffect<Paralyzed>();
			if (Duration > effect.Duration)
			{
				effect.Duration = Duration;
			}
			return true;
		}
		if (!Object.FireEvent("ApplyParalyze"))
		{
			return false;
		}
		ApplyStats();
		DidX("are", "paralyzed", "!", null, null, null, Object);
		Object.ParticleText("*paralyzed*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		base.Remove(Object);
	}

	private void ApplyStats()
	{
		int combatDV = Stats.GetCombatDV(Object);
		if (combatDV > 0)
		{
			DVPenalty += combatDV;
			StatShifter.SetStatShift(Object, "DV", -DVPenalty);
		}
		else
		{
			DVPenalty = 0;
		}
	}

	private void UnapplyStats()
	{
		DVPenalty = 0;
		StatShifter.RemoveStatShifts(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CanChangeMovementModeEvent>.ID && ID != PooledEvent<GetCompanionStatusEvent>.ID)
		{
			return ID == PooledEvent<IsConversationallyResponsiveEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (Duration > 0 && !E.Involuntary && E.Object == Object)
		{
			if (E.ShowMessage)
			{
				E.Object.Fail("You are paralyzed!");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == Object)
		{
			E.AddStatus("paralyzed", 90);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You are {{C|paralyzed}}.");
			}
			E.PreventAction = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == Object && !E.Mental)
		{
			E.Message = Object.T() + Object.Is + " utterly unresponsive.";
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("CanChangeBodyPosition");
		Registrar.Register("CanMoveExtremities");
		Registrar.Register("IsMobile");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 15 && num < 30)
		{
			E.Tile = null;
			E.RenderString = "X";
			E.ColorString = "&C^c";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile")
		{
			if (Duration > 0)
			{
				return false;
			}
		}
		else if (E.ID == "CanChangeBodyPosition" || E.ID == "CanMoveExtremities")
		{
			if (Duration > 0 && !E.HasFlag("Involuntary"))
			{
				if (E.HasFlag("ShowMessage") && Object.IsPlayer())
				{
					Popup.ShowFail("You are {{C|paralyzed}}!");
				}
				return false;
			}
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
