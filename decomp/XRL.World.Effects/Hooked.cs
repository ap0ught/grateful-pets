using System;
using XRL.Core;
using XRL.Language;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Hooked : Effect
{
	public int SaveTarget;

	public string HookedMessage = "You are hooked!";

	public GameObject HookingWeapon;

	public Hooked()
	{
		DisplayName = "hooked";
		Duration = 9;
	}

	public Hooked(GameObject HookingWeapon)
		: this()
	{
		this.HookingWeapon = HookingWeapon;
	}

	public Hooked(GameObject HookingWeapon, int SaveTarget)
		: this(HookingWeapon)
	{
		this.SaveTarget = SaveTarget;
	}

	public Hooked(GameObject HookingWeapon, int SaveTarget, int Duration)
		: this(HookingWeapon, SaveTarget)
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 33554464;
	}

	public override string GetDetails()
	{
		return "Is being dragged.\nCan't move without breaking free first.";
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_attachedTo");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == SingletonEvent<CommandTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			Axe_HookAndDrag axe_HookAndDrag = HookingWeapon?.Equipped?.GetPart<Axe_HookAndDrag>();
			if (axe_HookAndDrag != null)
			{
				axe_HookAndDrag.Validate();
			}
			else
			{
				Object.RemoveEffect(this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		if (Duration > 0 && (!Object.FireEvent("BeforeGrabbed") || Object.MakeSave("Strength", SaveTarget, HookingWeapon.Equipped, null, "HookAndDrag Continue Grab Restraint Escape", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, HookingWeapon)))
		{
			DidX("break", "free from " + WhatSubjectIsHeldBy(), "!", null, null, Object);
			Object.ParticleText("*broke free*", IComponent<GameObject>.ConsequentialColorChar(Object));
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginMove");
		Registrar.Register("Juked");
		Registrar.Register("IsMobile");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration <= 0)
		{
			return true;
		}
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "X";
			E.ColorString = "&R";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile")
		{
			return false;
		}
		if (E.ID == "Juked")
		{
			Object.RemoveEffect(this);
		}
		else if (E.ID == "BeginMove")
		{
			Axe_HookAndDrag axe_HookAndDrag = HookingWeapon?.Equipped?.GetPart<Axe_HookAndDrag>();
			if (axe_HookAndDrag != null && E.GetGameObjectParameter("Dragging") != HookingWeapon && axe_HookAndDrag.Validate() && E.GetParameter("DestinationCell") is Cell cell)
			{
				foreach (GameObject item in cell.LoopObjectsWithPart("Brain"))
				{
					if (!item.Brain.IsHostileTowards(Object) || !item.IsCombat)
					{
						continue;
					}
					goto IL_0199;
				}
				if (E.HasParameter("Teleporting"))
				{
					Object.ParticleText("*broke free*", IComponent<GameObject>.ConsequentialColorChar(Object));
					Duration = 0;
				}
				else if (Duration > 0 && !E.HasParameter("Dragging") && !E.HasParameter("Teleporting"))
				{
					if (Object.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage(HookedMessage, 'R');
					}
					Object.UseEnergy(1000, "Movement Failure");
					return false;
				}
			}
		}
		goto IL_0199;
		IL_0199:
		return base.FireEvent(E);
	}

	public string WhatSubjectIsHeldBy()
	{
		GameObject.Validate(ref HookingWeapon);
		if (HookingWeapon == null)
		{
			return "the hook maneuver";
		}
		if (HookingWeapon.HasProperName || HookingWeapon.Equipped == null)
		{
			return HookingWeapon.the + HookingWeapon.ShortDisplayName;
		}
		if (HookingWeapon.Equipped.IsPlayer())
		{
			return "your " + HookingWeapon.ShortDisplayName;
		}
		return Grammar.MakePossessive(HookingWeapon.Equipped.the + HookingWeapon.Equipped.ShortDisplayName) + " " + HookingWeapon.ShortDisplayName;
	}
}
