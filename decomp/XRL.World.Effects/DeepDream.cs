using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using Qud.API;
using Qud.UI;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI.GoalHandlers;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[HasWishCommand]
public class DeepDream : Effect
{
	public const int DefaultWeight = 100;

	public static readonly Dictionary<string, int> BlueprintWeights = new Dictionary<string, int>
	{
		{ "Cave Spider", 30 },
		{ "Glowfish", 30 },
		{ "GiantAmoeba", 30 }
	};

	public GameObject Target;

	[NonSerialized]
	public GameObject Source;

	public DeepDream()
	{
		DisplayName = "{{b|deep dreaming}}";
		Duration = 1;
	}

	public DeepDream(GameObject Source)
		: this()
	{
		this.Source = Source;
	}

	public override string GetDetails()
	{
		return "This creature is unresponsive.";
	}

	public override int GetEffectType()
	{
		return 33554434;
	}

	public override string GetStateDescription()
	{
		return "{{b|deeply dreaming}}";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != TookDamageEvent.ID && ID != EffectAppliedEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != BeforeDeathRemovalEvent.ID && ID != GetZoneFreezabilityEvent.ID)
		{
			return ID == PooledEvent<IsConversationallyResponsiveEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		Target.RemoveEffect(typeof(WakingDream));
		E.RequestInterfaceExit();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (E.Effect != this && E.Effect.IsOfType(33554432))
		{
			Object.RemoveEffect(this);
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		WakingDream effect = Target.GetEffect<WakingDream>();
		if (effect != null && effect.Dreamer == Object)
		{
			effect.Metempsychosis = true;
			Target.RemoveEffect(effect);
			Object.RemoveEffect(this);
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == Object)
		{
			E.Message = Asleep.GetSleepMessage(Object, E.Physical, E.Mental);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Object.IsPlayer() || !GameObject.Validate(ref Target) || !Target.HasEffect(typeof(WakingDream)))
		{
			Object.RemoveEffect(this);
			return true;
		}
		if (!(Object.Brain.Goals.Peek() is Dormant))
		{
			Object.Brain.Goals.Clear();
			Object.Brain.PushGoal(new Dormant(-1));
		}
		return false;
	}

	public override bool HandleEvent(GetZoneFreezabilityEvent E)
	{
		E.Freezability = Freezability.FormerPlayerObject;
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect(typeof(DeepDream)) || !ApplyEffectEvent.Check(Object, "DeepDream", this))
		{
			return false;
		}
		if (!Object.IsPlayer())
		{
			Object.ApplyEffect(new Asleep(20));
			return false;
		}
		if (Object.HasEffect(typeof(WakingDream)))
		{
			Achievement.DEEP_DREAM.Unlock();
		}
		SoundManager.PlaySound("sfx_characterTrigger_dreamcrungle_start");
		The.Player?.SleepytimeParticles();
		GameManager.Instance.Spin = 10f;
		TransitionOut(2f);
		if (!Crungle())
		{
			return false;
		}
		return base.Apply(Object);
	}

	public override void Applied(GameObject Object)
	{
		The.ZoneManager.SuspendAll();
		The.ZoneManager.CheckCached();
	}

	public bool IsOurTargetEffect(Effect FX)
	{
		if (FX is WakingDream wakingDream)
		{
			return wakingDream.Dreamer == Object;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		if (GameObject.Validate(ref Target))
		{
			Target.RemoveEffect(IsOurTargetEffect);
		}
		base.Remove(Object);
	}

	public int GetObjectWeight(GameObject Object)
	{
		if (Object.IsCombatObject() && Object.IsMobile() && Object.HasStat("XP") && !Object.HasEffect(typeof(WakingDream)) && !Object.HasEffect(typeof(DeepDream)))
		{
			if (BlueprintWeights.TryGetValue(Object.Blueprint, out var value))
			{
				return value;
			}
			return 100;
		}
		return 0;
	}

	public GameObject GetDreamSubject()
	{
		Zone zone = The.ActiveZone;
		BallBag<GameObject> ballBag = new BallBag<GameObject>();
		for (int i = 0; i < 15; i++)
		{
			if (!zone.IsActive())
			{
				The.ZoneManager.SuspendZone(zone);
				The.ZoneManager.CheckCached();
			}
			string randomDestinationZoneID = SpaceTimeVortex.GetRandomDestinationZoneID(The.Player.CurrentZone.ZoneWorld, Validate: false);
			if (The.ZoneManager.IsZoneLive(randomDestinationZoneID))
			{
				continue;
			}
			zone = The.ZoneManager.GetZone(randomDestinationZoneID);
			ballBag.Clear();
			ballBag.AddRange(zone.YieldObjects(), GetObjectWeight);
			if (ballBag.Count != 0)
			{
				int num = ballBag.TotalWeight / ballBag.Count;
				MetricsManager.LogInfo($"WakingDream::{randomDestinationZoneID}:{num}");
				if (num.ChanceIn(100))
				{
					break;
				}
			}
		}
		return ballBag.PeekOne();
	}

	public bool Crungle()
	{
		GameObject player = The.Player;
		bool flag = !Options.ModernUI;
		Loading.SetHideLoadStatus(hidden: true);
		Popup.Suppress = true;
		try
		{
			Target = GetDreamSubject();
			if (Target == null)
			{
				return false;
			}
			MetricsManager.LogInfo("WakingDream::Subject:" + Target.Blueprint);
			Target.GiveProperName();
			if (!WakingDream.ApplyTo(Target, player, Source))
			{
				return false;
			}
			int level = Target.Level;
			int xPForLevel = Leveler.GetXPForLevel(level + 1);
			xPForLevel -= Math.Max(1, Stat.Random(level * 25, level * 75));
			Target.GetStat("XP").BaseValue = xPForLevel;
			Target.Brain.PartyLeader = null;
			Target.Brain.Goals.Clear();
			The.Game.Player.Body = Target;
			Target.CurrentZone.SetActive();
			Thread.Sleep(2000);
			Popup.Suppress = false;
			if (flag)
			{
				ClassicFade();
			}
			Popup.Show("You have the feeling of waking from a dream.");
			JournalAPI.AddAccomplishment("You woke from a long, convincing dream and returned to your life as the " + Target.GetBlueprint().CachedDisplayNameStripped + " " + Target.DisplayNameOnlyDirect + ".", "<spice.commonPhrases.blessed.!random.capitalize> =name= revealed the details of " + IComponent<GameObject>.ThePlayer.GetPronounProvider().PossessiveAdjective + " former life as a " + Target.GetBlueprint().CachedDisplayNameStripped + ", and announced " + IComponent<GameObject>.ThePlayer.GetPronounProvider().PossessiveAdjective + " plans to return to it!", "One auspicious evening, " + Target.DisplayNameOnlyDirect + " had a dream that <entity.subjectPronoun> was not the " + Target.GetBlueprint().CachedDisplayNameStripped + " " + Target.DisplayNameOnlyDirect + " but was instead the Moon King =name=. <spice.commonPhrases.fromThenOn.!random.capitalize>, <entity.subjectPronoun> always kept some moondust on <entity.possessivePronoun> person.", null, "general", MuralCategory.WeirdThingHappens, MuralWeight.High, null, -1L);
			RequireLightSource(Target);
			Popup.Suppress = true;
		}
		finally
		{
			TransitionIn(flag ? 0f : 4f);
			Loading.SetHideLoadStatus(hidden: false);
			Popup.Suppress = false;
		}
		return true;
	}

	public void RequireLightSource(GameObject Object)
	{
		if (!Object.HasPart<DarkVision>())
		{
			if (Object.Body.HasPart("Hand"))
			{
				Object.ReceiveObject("Torch", 3);
				Object.Brain.PerformReequip(Silent: true, DoPrimaryChoice: false);
			}
			else
			{
				Object.RequirePart<Mutations>().AddMutation("DarkVision");
			}
		}
	}

	public static void ClassicFade()
	{
		ScreenBuffer scrapBuffer = TextConsole.GetScrapBuffer1();
		scrapBuffer.Clear();
		scrapBuffer.Draw();
		FadeToBlack.FadeIn(0.5f, Color.black);
	}

	public static void TransitionOut(float Fade = 0f, string Sound = null)
	{
		SoundManager.StopMusic("music", Fade > 0f, Fade);
		if (Sound != null)
		{
			SoundManager.PlaySound(Sound);
		}
		FadeToBlack.FadeOut(Fade, Color.black);
		GameManager.Instance.PushGameView("Empty");
		if (Fade > 0f)
		{
			The.Core.RenderDelay((int)(Fade * 1000f), Interruptible: false);
		}
	}

	public static void TransitionIn(float Fade = 0f)
	{
		FadeToBlack.FadeIn(Fade, Color.black);
		GameManager.Instance.PushGameView("Empty");
		if (Fade > 0f)
		{
			The.Core.RenderDelay((int)(Fade * 1000f), Interruptible: false);
		}
		if (GameManager.Instance.CurrentGameView == "Empty")
		{
			GameManager.Instance.PopGameView();
		}
	}

	[WishCommand("crungle", null)]
	public static void Wish()
	{
		The.Player.ApplyEffect(new DeepDream());
	}
}
