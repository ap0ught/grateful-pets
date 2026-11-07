using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class LifeDrain : Effect, ITierInitialized
{
	public string Damage = "1";

	public int SaveTarget = 20;

	public GameObject Drainer;

	public int Level;

	public bool RealityDistortionBased;

	public LifeDrain()
	{
		DisplayName = "syphoned";
	}

	public LifeDrain(int Duration, int Level, string DamagePerRound, GameObject Drainer)
		: this()
	{
		Damage = DamagePerRound;
		base.Duration = Duration;
		this.Drainer = Drainer;
		this.Level = Level;
	}

	public LifeDrain(int Duration, int Level, string DamagePerRound, GameObject Drainer, bool RealityDistortionBased)
		: this(Duration, Level, DamagePerRound, Drainer)
	{
		this.RealityDistortionBased = RealityDistortionBased;
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
		Level = Stat.Random(-1, 1) + Tier * 2;
		if (Tier < 1)
		{
			Tier = 1;
		}
		if (Tier > 8)
		{
			Tier = 8;
		}
		Damage = Level.ToString();
		Duration = 20;
		RealityDistortionBased = true;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 33587204;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return Damage + " life drained per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!XRL.World.Parts.Mutation.LifeDrain.IsValidTarget(Object))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyLifeDrain"))
		{
			return false;
		}
		if (Drainer == null)
		{
			if (Object.Target != null)
			{
				Drainer = Object.Target;
			}
			else
			{
				Cell cell = Object.CurrentCell;
				List<GameObject> list = cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 6, "Physics", Object);
				if (list.Count > 0)
				{
					Drainer = list.GetRandomElement();
				}
				else
				{
					Drainer = Object;
				}
			}
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_negativeVitality");
		Drainer?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
		IComponent<GameObject>.XDidYToZ(Drainer, "bond", "with", Object, null, ".", null, null, null, Object);
		Object.ParticleText("*bonded*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		IComponent<GameObject>.XDidYToZ(Drainer, "begin", "to drain life essence from", Object, null, "!", null, null, null, Object);
		if (Drainer.IsPlayer() && Drainer.Target == null)
		{
			Drainer.Target = Object;
		}
		if (Object.IsPlayer())
		{
			AutoAct.Interrupt();
		}
		else if (Object.IsPlayerLed() && !Object.IsTrifling)
		{
			if (Object.IsVisible())
			{
				AutoAct.Interrupt(null, null, Object, IsThreat: true);
			}
			else if (Object.IsAudible(IComponent<GameObject>.ThePlayer))
			{
				AutoAct.Interrupt("you hear a cry of distress from " + Object.t(), null, Object, IsThreat: true);
			}
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (E.Actor == Drainer && GameObject.Validate(ref Drainer))
		{
			E.AddAction("CancelLifeDrain", "cancel life drain", "CancelLifeDrain", null, 'c', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "CancelLifeDrain" && E.Actor == Drainer)
		{
			IComponent<GameObject>.XDidYToZ(E.Actor, "release", Object, "from " + E.Actor.its + " life drain", null, null, null, E.Actor, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: true);
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (!GameObject.Validate(ref Drainer) || !Drainer.HasHitpoints())
		{
			Duration = 0;
			Drainer = null;
			return true;
		}
		if (!XRL.World.Parts.Mutation.LifeDrain.IsValidTarget(Object))
		{
			Duration = 0;
			return true;
		}
		if (RealityDistortionBased && (!IComponent<GameObject>.CheckRealityDistortionUsability(Drainer, null, Drainer, null, Drainer.GetPart<XRL.World.Parts.Mutation.LifeDrain>()) || !IComponent<GameObject>.CheckRealityDistortionAccessibility(Object, null, Drainer, null, Drainer.GetPart<XRL.World.Parts.Mutation.LifeDrain>())))
		{
			Duration = 0;
			return true;
		}
		if (Duration > 0)
		{
			if (Stat.Random(1, 8) + Math.Max(Drainer.StatMod("Ego"), Level) > Stats.GetCombatMA(Object) + 4)
			{
				Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_life_drain");
				int Amount = Damage.RollCached();
				if (Amount > 0 && Object.TakeDamage(ref Amount, "Drain Unavoidable", null, null, Drainer, null, null, null, null, "from %t life drain!", Accidental: false, Environmental: false, Indirect: true) && Amount > 0)
				{
					Drainer.Heal(Amount, Message: true, FloatText: true, RandomMinimum: true);
				}
			}
			else if (Drainer.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(Object.Does("resist") + " your life drain!", 'r');
			}
			else if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You resist " + Drainer.poss("life drain") + "!", 'g');
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 25 && num < 35)
		{
			E.Tile = null;
			E.RenderString = "\u0003";
			E.ColorString = "&K^k";
		}
		return true;
	}
}
