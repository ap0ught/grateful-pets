using System;
using System.Threading;
using Qud.API;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

public class WakingDream : Effect
{
	public const string END_CMD = "EndWakingDream";

	public const int FLAG_PLEASANT = 1;

	public GameObject Dreamer;

	public GameObject Source;

	public bool FromOriginalPlayerBody;

	[NonSerialized]
	public bool Pleasant;

	[NonSerialized]
	public bool Metempsychosis;

	public WakingDream()
	{
		DisplayName = "waking dream";
		Duration = 1;
	}

	public WakingDream(GameObject Dreamer, GameObject Source)
		: this()
	{
		this.Dreamer = Dreamer;
		this.Source = Source;
		FromOriginalPlayerBody = Dreamer.IsOriginalPlayerBody();
	}

	public override string GetDetails()
	{
		return "Gain a level as this creature to wake pleasantly.\nDie or abandon character to wake fitfully.";
	}

	public override string GetStateDescription()
	{
		return "in a waking dream";
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterDieEvent.ID)
		{
			return ID == PooledEvent<AfterLevelGainedEvent>.ID;
		}
		return true;
	}

	public override void RegisterActive(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(The.Game, PooledEvent<GenericCommandEvent>.ID);
	}

	public override bool HandleEvent(AfterDieEvent E)
	{
		Object.RemoveEffect(this);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterLevelGainedEvent E)
	{
		E.RequestInterfaceExit();
		GameEventCommand.Issue("EndWakingDream", null, ID.GetHashCode(), 1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericCommandEvent E)
	{
		if (E.Command == "EndWakingDream" && E.Level == ID.GetHashCode() && Object != null && Duration > 0)
		{
			Duration = 0;
			Pleasant = E.Flags.HasBit(1);
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public static bool CanApply(GameObject Object)
	{
		if (Object.HasEffect<WakingDream>())
		{
			return false;
		}
		if (!CanApplyEffectEvent.Check<WakingDream>(Object))
		{
			return false;
		}
		return true;
	}

	public static bool ApplyTo(GameObject Object, GameObject Dreamer, GameObject Source)
	{
		if (!CanApply(Object))
		{
			return false;
		}
		return Object.ApplyEffect(new WakingDream(Dreamer, Source));
	}

	public override void Remove(GameObject Object)
	{
		if (!Object.IsPlayer())
		{
			return;
		}
		Object.PullDown();
		if (GameObject.Validate(ref Dreamer) && !Dreamer.IsDying && !Dreamer.IsNowhere() && !Metempsychosis)
		{
			SoundManager.PlaySound("sfx_characterTrigger_dreamcrungle_start");
			GameManager.Instance.Spin = 10f;
			DeepDream.TransitionOut(2f);
			The.Game.Player.Body = Dreamer;
			Dreamer.CurrentZone.SetActive();
			Thread.Sleep(2000);
			bool flag = !Options.ModernUI;
			if (Pleasant)
			{
				if (flag)
				{
					DeepDream.ClassicFade();
				}
				Dreamer.PlayWorldSound("sfx_characterTrigger_dreamcrungle_wake_peaceful");
				Popup.Show("{{G|You wake up peacefully.}}", null, null);
				DeepDream.TransitionIn(flag ? 0f : 2f);
				Achievement.SOUND_SLEEPER.Progress.Increment();
				JournalAPI.AddAccomplishment("You woke from a peaceful dream.", "<spice.commonPhrases.blessed.!random.capitalize> =name= dreamed of a thousand years of peace, and the people of Qud <spice.history.gospels.Celebration.LateSultanate.!random> in <spice.commonPhrases.celebration.!random>.", "One auspicious evening, =name= had a dream that <entity.subjectPronoun> " + The.Player.GetVerb("were") + " not =name= but " + The.Player.GetVerb("were") + " instead the " + Object.GetBlueprint().CachedDisplayNameStripped + " " + Object.DisplayNameOnlyDirect + ". After a full otherlife, " + The.Player.GetPronounProvider().Subjective + " woke from the dream in a state of deep peace.", null, "general", MuralCategory.BodyExperienceGood, MuralWeight.High, null, -1L);
			}
			else
			{
				if (flag)
				{
					DeepDream.ClassicFade();
				}
				Dreamer.PlayWorldSound("sfx_characterTrigger_dreamcrungle_wake_fitful");
				Popup.Show("{{r|You wake in a fitful start.}}", null, null);
				DeepDream.TransitionIn(flag ? 0f : 2f);
				JournalAPI.AddAccomplishment("You woke from a fitful dream.", "<spice.commonPhrases.blessed.!random.capitalize> =name= dreamed of a thousand years of woe, and the people of Qud <spice.history.gospels.Celebration.LateSultanate.!random> in <spice.commonPhrases.woe.!random>.", "One auspicious evening, =name= had a dream that <entity.subjectPronoun> " + The.Player.GetVerb("were") + " not =name= but " + The.Player.GetVerb("were") + " instead the " + Object.GetBlueprint().CachedDisplayNameStripped + " " + Object.DisplayNameOnlyDirect + ". After a full otherlife, " + The.Player.GetPronounProvider().Subjective + " woke from the dream in a state of deep horror.", null, "general", MuralCategory.BodyExperienceGood, MuralWeight.High, null, -1L);
			}
			Award(Dreamer);
		}
		else
		{
			Domination.Metempsychosis(Object, FromOriginalPlayerBody);
			Award(Object);
		}
		if (Object.HasRegisteredEvent("EndWakingDream"))
		{
			Object.FireEvent(Event.New("EndWakingDream", "Pleasant", Pleasant ? 1 : 0, "Metempsychosis", Metempsychosis ? 1 : 0));
		}
	}

	public void Award(GameObject Target)
	{
		if (Pleasant)
		{
			if (GameObject.Validate(ref Target))
			{
				Target.AwardXP(15000, -1, 0, int.MaxValue, null, Object);
			}
			if (GameObject.Validate(ref Source) && !Source.IsNowhere())
			{
				Source.ReplaceWith("Sated Dreamcrungle");
			}
			return;
		}
		if (GameObject.Validate(ref Target))
		{
			Target.GetStat("Willpower").BaseValue--;
			if (Target.IsPlayer())
			{
				Popup.Show("You lose {{rules|1}} point of Willpower permanently.");
			}
		}
		if (GameObject.Validate(ref Source) && !Source.IsNowhere())
		{
			Source.SetIntProperty("NoXP", 1);
			Source.Die(Target, null, "Your dream collapsed.", Source.Poss("dream") + " collapsed");
		}
	}
}
