using System;
using XRL.Core;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Stun : Effect, ITierInitialized
{
	public int DVPenalty;

	public int SaveTarget = 15;

	public bool DontStunIfPlayer;

	public Stun()
	{
		DisplayName = "{{C|stunned}}";
	}

	public Stun(int Duration, int SaveTarget, bool DontStunIfPlayer = false)
		: this()
	{
		this.SaveTarget = SaveTarget;
		base.Duration = Duration;
		this.DontStunIfPlayer = DontStunIfPlayer;
	}

	public void Initialize(int Tier)
	{
		Tier = XRL.World.Capabilities.Tier.Constrain(Stat.Random(Tier - 2, Tier + 2));
		Duration = Stat.Random(1, 4);
		SaveTarget = 11 + Tier * 4;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{C|stunned}}";
	}

	public override string GetDetails()
	{
		if (DVPenalty < 0)
		{
			return "Can't take actions.\n" + DVPenalty + " DV";
		}
		return "Can't take actions.\nDV set to 0.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.Brain == null)
		{
			return false;
		}
		Stun effect = Object.GetEffect<Stun>();
		if (effect != null)
		{
			if (Object.IsPlayer() && DontStunIfPlayer)
			{
				Duration = 0;
				return false;
			}
			effect.Duration += Duration;
			if (effect.SaveTarget < SaveTarget)
			{
				effect.SaveTarget = SaveTarget;
			}
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Stun", this))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyStun"))
		{
			return false;
		}
		DidX("are", "stunned", "!", null, null, null, Object);
		Object.ParticleText("*stunned*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		Object.ForfeitTurn();
		ApplyStats();
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
			DVPenalty = combatDV;
			StatShifter.SetStatShift("DV", -DVPenalty);
		}
		else
		{
			DVPenalty = 0;
		}
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts();
		DVPenalty = 0;
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
		if (E.Object == Object && Duration > 0)
		{
			if (SaveTarget < 0 || !Object.MakeSave("Toughness", SaveTarget, null, null, "Stun"))
			{
				DidX("remain", "stunned", "!", null, null, null, Object);
				if (Object.IsPlayer())
				{
					XRLCore.Core.RenderDelay(500);
				}
				else
				{
					Object.ParticleText("*remains stunned*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
				}
				Object.ForfeitTurn();
				if (Duration != 9999)
				{
					Duration--;
				}
				return false;
			}
			Object.ParticleText("*made save vs. stun*", IComponent<GameObject>.ConsequentialColorChar(Object));
			Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == Object)
		{
			if (E.Mental && !E.Physical)
			{
				E.Message = Object.Poss("mind") + " is in disarray.";
			}
			else
			{
				E.Message = Object.Does("don't") + " seem to understand you.";
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("IsMobile");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 10 && num < 25)
			{
				E.Tile = null;
				E.RenderString = "!";
				E.ColorString = "&C^c";
			}
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
