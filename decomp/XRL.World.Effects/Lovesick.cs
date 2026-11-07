using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Lovesick : Effect, ITierInitialized
{
	private int Penalty = 6;

	private int SpeedPenalty = 5;

	private int SecondDuration;

	public GameObject Beauty;

	public GameObject PreviousLeader;

	public Lovesick()
	{
		DisplayName = "{{lovesickness|lovesick}}";
	}

	public Lovesick(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public Lovesick(int Duration, GameObject Beauty)
		: this(Duration)
	{
		this.Beauty = Beauty;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(3000, 3600);
	}

	public override int GetEffectType()
	{
		return 100663298;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool UseThawEventToUpdateDuration()
	{
		return true;
	}

	public override string GetDetails()
	{
		string text = "-6 Intelligence\n-6 Willpower\n-5 Move Speed";
		if (Options.AnySifrah)
		{
			text += "\nUseful in many social, ritual, and psionic Sifrah games.";
		}
		return text;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Lovesick>())
		{
			return false;
		}
		if (!Object.IsOrganic)
		{
			return false;
		}
		if (Beauty == null)
		{
			if (Object.Target != null)
			{
				Beauty = Object.Target;
			}
			else
			{
				Cell cell = Object.CurrentCell;
				List<GameObject> list = cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 6, Object, (GameObject x) => x.IsReal, Object.IsPlayer());
				Beauty = list.GetRandomElement() ?? Object;
			}
		}
		if (!Object.FireEvent(Event.New("ApplyLovesick", "Duration", Duration)))
		{
			return false;
		}
		if (Beauty != Object)
		{
			Object.StopFight(Beauty, Involuntary: false, Beauty.IsPlayer());
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_charm");
		if (Object.IsPlayer())
		{
			if (Beauty.IsPlayer())
			{
				Achievement.LOVE_YOURSELF.Unlock();
			}
			if (Beauty.GetBlueprint().InheritsFrom("Sign"))
			{
				Achievement.LOVE_SIGN.Unlock();
			}
			JournalAPI.AddAccomplishment("Your heart sang at the sight of " + Beauty.an() + ".", "The troubadour-hero =name= rode the tides of " + The.Player.GetPronounProvider().PossessiveAdjective + " passions and shipwrecked on the shores of " + Beauty.an() + ".", "<spice.elements." + The.Player.GetMythicDomain() + ".weddingConditions.!random.capitalize>, =name= cemented " + The.Player.GetPronounProvider().PossessiveAdjective + " friendship with " + Object.GetPrimaryFactionName(VisibleOnly: true, Formatted: true, Base: true) + " by marrying " + Object.an() + ".", null, "general", MuralCategory.CommitsFolly, MuralWeight.High, null, -1L);
		}
		else if (Object.Brain != null)
		{
			PreviousLeader = Object.Brain.PartyLeader;
			if (Object != Beauty && !Beauty.IsLedBy(Object))
			{
				Object.Brain.SetPartyLeader(Beauty, 0, Transient: true);
			}
		}
		DidXToY("fall", "in love with", Beauty, null, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		Beauty = Beauty?.DeepCopy(CopyEffects: true, CopyID: true);
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		GameObject.Validate(ref PreviousLeader);
		if (Object.Brain != null && PreviousLeader != Object.Brain.PartyLeader && Object.FireEvent(Event.New("CanRestorePartyLeader", "PreviousLeader", PreviousLeader)))
		{
			GameObject partyLeader = Object.PartyLeader;
			if (partyLeader != null && partyLeader.FireEvent(Event.New("CanCompanionRestorePartyLeader", "Companion", Object, "PreviousLeader", PreviousLeader)))
			{
				Object.PartyLeader = PreviousLeader;
			}
		}
		UnapplyStats();
		if (Object.IsPlayer() && GameObject.Validate(ref Beauty))
		{
			JournalAPI.AddAccomplishment("Though your affection for " + Beauty.an() + " endowed you with a certain wisdom, you no longer feel the tug on your heartstrings.", "The call of the sea rang through the troubadour-hero =name= once again, and " + The.Player.GetPronounProvider().Subjective + " set sail from the familiar shore of " + Beauty.an() + ".", "<spice.elements." + The.Player.GetMythicDomain() + ".weddingConditions.!random.capitalize>, =name= cemented " + The.Player.GetPronounProvider().PossessiveAdjective + " friendship with " + Object.GetPrimaryFactionName(VisibleOnly: true, Formatted: true, Base: true) + " by annulling " + The.Player.GetPronounProvider().PossessiveAdjective + " marriage to " + Object.an() + ".", null, "general", MuralCategory.HasInspiringExperience, MuralWeight.High, null, -1L);
		}
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift(Object, "Intelligence", -Penalty);
		StatShifter.SetStatShift(Object, "Willpower", -Penalty);
		StatShifter.SetStatShift(Object, "MoveSpeed", SpeedPenalty);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.Validate(ref PreviousLeader);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 10)
			{
				E.Tile = null;
				E.RenderString = "\u0003";
				if (SecondDuration > 0)
				{
					E.ColorString = "&R";
				}
				else
				{
					E.ColorString = "&R";
				}
			}
		}
		return true;
	}
}
