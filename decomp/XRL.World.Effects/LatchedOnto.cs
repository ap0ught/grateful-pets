using System;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Language;

namespace XRL.World.Effects;

[Serializable]
public class LatchedOnto : Effect
{
	public int SaveTarget;

	public string SaveStat = "Strength";

	public string SaveDifficultyStat = "Strength";

	public GameObject LatchedOnWeapon;

	public LatchedOnto()
	{
		DisplayName = "{{r|latched onto}}";
		Duration = 9;
	}

	public LatchedOnto(GameObject LatchedOnWeapon)
		: this()
	{
		this.LatchedOnWeapon = LatchedOnWeapon;
	}

	public LatchedOnto(GameObject LatchedOnWeapon, int SaveTarget)
		: this(LatchedOnWeapon)
	{
		this.SaveTarget = SaveTarget;
	}

	public LatchedOnto(GameObject LatchedOnWeapon, int SaveTarget, string SaveStat)
		: this(LatchedOnWeapon, SaveTarget)
	{
		this.SaveStat = SaveStat;
	}

	public LatchedOnto(GameObject LatchedOnWeapon, int SaveTarget, string SaveStat, string SaveDifficultyStat)
		: this(LatchedOnWeapon, SaveTarget, SaveStat)
	{
		this.SaveDifficultyStat = SaveDifficultyStat;
	}

	public LatchedOnto(GameObject LatchedOnWeapon, int SaveTarget, string SaveStat, string SaveDifficultyStat, int Duration)
		: this(LatchedOnWeapon, SaveTarget, SaveStat, SaveDifficultyStat)
	{
		base.Duration = Duration;
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
		string text = WhatSubjectIsHeldBy(NullOkay: true, Indefinite: true);
		if (text == null)
		{
			return "Has been latched onto.\nCan't move without breaking free first.";
		}
		return "Has been latched onto by " + text + ".\nCan't move without breaking free first.";
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
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0 && (!Object.FireEvent("BeforeGrabbed") || Object.MakeSave(SaveStat, SaveTarget, LatchedOnWeapon?.Equipped, SaveDifficultyStat, "LatchOn Continue Grab Restraint Escape", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, LatchedOnWeapon)))
		{
			Object.ParticleText("*broke free*", IComponent<GameObject>.ConsequentialColorChar(Object));
			string text = WhatSubjectIsHeldBy(NullOkay: false, Indefinite: false, LatchedOnWeapon.Physics.Equipped.IsPlayer());
			DidX("break", "free from " + text, "!", null, null, Object);
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
		if (Duration > 0 && XRLCore.CurrentFrame > 35 && XRLCore.CurrentFrame < 45)
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
		else if (E.ID == "BeginMove" && E.GetGameObjectParameter("Dragging") != LatchedOnWeapon && E.GetParameter("DestinationCell") is Cell cell)
		{
			foreach (GameObject item in cell.LoopObjectsWithPart("Brain"))
			{
				if (item.Brain.IsHostileTowards(Object) && item.IsCombat)
				{
					return true;
				}
			}
			if (E.HasParameter("Teleporting"))
			{
				Object.ParticleText("*broke free*", IComponent<GameObject>.ConsequentialColorChar(Object));
				Duration = 0;
			}
			else if (Duration > 0)
			{
				string text = WhatSubjectIsHeldBy(NullOkay: false, Indefinite: false, LatchedOnWeapon.Physics.Equipped.IsPlayer());
				DidX("are", "held in place by " + text, "!", null, null, null, Object);
				Object.UseEnergy(1000, "Movement Failure");
				return false;
			}
		}
		return base.FireEvent(E);
	}

	public string WhatSubjectIsHeldBy(bool NullOkay = false, bool Indefinite = false, bool SecondPerson = false)
	{
		if (LatchedOnWeapon != null && LatchedOnWeapon.IsNowhere())
		{
			LatchedOnWeapon = null;
		}
		if (LatchedOnWeapon == null)
		{
			if (!NullOkay)
			{
				return "being latched onto";
			}
			return null;
		}
		if (LatchedOnWeapon.HasProperName || LatchedOnWeapon.Equipped == null)
		{
			return (Indefinite ? LatchedOnWeapon.a : LatchedOnWeapon.the) + LatchedOnWeapon.ShortDisplayName;
		}
		return (SecondPerson ? "your" : Grammar.MakePossessive((Indefinite ? LatchedOnWeapon.Equipped.a : LatchedOnWeapon.Equipped.the) + LatchedOnWeapon.Equipped.ShortDisplayName)) + " " + LatchedOnWeapon.ShortDisplayName;
	}

	public override void Expired()
	{
		if (LatchedOnWeapon == null)
		{
			return;
		}
		string text = WhatSubjectIsHeldBy(NullOkay: true);
		if (text != null)
		{
			string text2 = IComponent<GameObject>.ConsequentialColor(Object);
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(text2 + ColorUtility.CapitalizeExceptFormatting(text) + text2 + LatchedOnWeapon.GetVerb("release") + " you.");
			}
			else if (Object.IsVisible())
			{
				IComponent<GameObject>.AddPlayerMessage(text2 + ColorUtility.CapitalizeExceptFormatting(text) + text2 + LatchedOnWeapon.GetVerb("release") + " " + Object.the + Object.ShortDisplayName + text2 + ".");
			}
		}
	}
}
