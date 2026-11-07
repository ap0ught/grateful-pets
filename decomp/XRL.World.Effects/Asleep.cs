using System;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Asleep : Effect, ITierInitialized
{
	public bool quicksleep;

	public bool forced;

	public GameObject AsleepOn;

	public int TurnsAsleep;

	public bool Voluntary;

	public int IndefinitePlayerSleepTurns;

	[NonSerialized]
	private long TurnWentToSleep;

	[NonSerialized]
	private bool WakeMessageDone;

	[NonSerialized]
	private bool ApplyProne = true;

	public Asleep()
	{
		DisplayName = "{{c|asleep}}";
	}

	public Asleep(int Duration, bool forced = false, bool quicksleep = false, bool Voluntary = false, bool Prone = true)
		: this()
	{
		base.Duration = Duration;
		this.forced = forced;
		this.quicksleep = quicksleep;
		this.Voluntary = Voluntary;
		ApplyProne = Prone;
		TurnWentToSleep = The.Game.Turns;
	}

	public Asleep(GameObject AsleepOn, int Duration, bool forced = false, bool quicksleep = false, bool Voluntary = false, bool Prone = true)
		: this(Duration, forced, quicksleep, Voluntary, Prone)
	{
		this.AsleepOn = AsleepOn;
		TurnWentToSleep = The.Game.Turns;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(40, 200);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		int num = 117440514;
		if (Voluntary)
		{
			num |= 0x8000000;
		}
		return num;
	}

	public static bool UsesSleepMode(GameObject obj)
	{
		return obj?.HasTag("Robot") ?? false;
	}

	public bool UsesSleepMode()
	{
		return UsesSleepMode(Object);
	}

	public override string GetDescription()
	{
		if (UsesSleepMode())
		{
			return "{{c|sleep mode}}";
		}
		return base.GetDescription();
	}

	public override string GetDetails()
	{
		if (UsesSleepMode())
		{
			return "In power conservation mode.\n-12 DV.\nAttackers receive +4 to penetration rolls.\nWill return to normal operations dazed if damage is taken.";
		}
		return "Unconscious.\n-12 DV.\nAttackers receive +4 to penetration rolls.\nWill wake up dazed if damage is taken.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasPart<Brain>())
		{
			return false;
		}
		if (Object.HasEffect<Asleep>())
		{
			return false;
		}
		if (((Voluntary || Object.FireEvent("CanApplyInvoluntarySleep")) && Object.FireEvent("CanApplySleep") && (Voluntary || Object.FireEvent("ApplyInvoluntarySleep")) && Object.FireEvent("ApplySleep") && ApplyEffectEvent.Check(Object, "Sleep", this)) || forced)
		{
			if (!UsesSleepMode(Object) && ApplyProne)
			{
				Object.ApplyEffect(new Prone(LyingOn: AsleepOn, Voluntary: Voluntary));
			}
			Object.MovementModeChanged("Asleep", !Voluntary);
			if (Voluntary)
			{
				PlayWorldSound("sfx_characterTrigger_sleep_idle");
			}
			if (Object.IsPlayer())
			{
				if (UsesSleepMode(Object))
				{
					IComponent<GameObject>.AddPlayerMessage("You enter {{C|sleep mode}}.");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage("You fall {{C|asleep}}!");
				}
			}
			else if (Visible())
			{
				if (UsesSleepMode(Object))
				{
					IComponent<GameObject>.AddPlayerMessage(Object.Does("go") + " into {{C|sleep mode}}.");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(Object.Does("fall") + " {{C|asleep}}.");
				}
			}
			if (Object.Brain != null)
			{
				Object.Brain.Goals.Clear();
			}
			Object.ForfeitTurn();
			if (Object.IsPlayer())
			{
				AutoAct.Interrupt();
			}
			ApplyStats();
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		if (!WakeMessageDone)
		{
			if (UsesSleepMode(Object))
			{
				DidX("exit", "sleep mode");
			}
			else
			{
				DidX("wake", "up");
			}
		}
		if (!Voluntary)
		{
			Object.ApplyEffect(new Wakeful(Stat.Random(3, 5)));
		}
		base.Remove(Object);
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift("DV", -12);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CanChangeMovementModeEvent>.ID && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetCompanionStatusEvent>.ID && ID != GetDefenderHitDiceEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == PooledEvent<IsConversationallyResponsiveEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (!E.Involuntary && E.Object == Object)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == Object)
		{
			E.AddStatus("asleep", 100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == Object)
		{
			E.Message = GetSleepMessage(Object, E.Physical, E.Mental);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			if (Duration == 9999)
			{
				if (Object.IsPlayer() && (1.in1000() || IndefinitePlayerSleepTurns == int.MaxValue))
				{
					Duration = 0;
				}
				else
				{
					IndefinitePlayerSleepTurns++;
					if (IndefinitePlayerSleepTurns % 10 == 0)
					{
						XRLCore.TenPlayerTurnsPassed();
						The.Core.RenderBase(UpdateSidebar: false);
					}
					if (Object.IsPlayer() && The.Game.Player.Messages.LastLine != "You are asleep.")
					{
						IComponent<GameObject>.AddPlayerMessage("You are asleep.");
					}
				}
			}
			else
			{
				Duration--;
			}
			if (Duration > 0)
			{
				if (Duration % 10 == 0)
				{
					XRLCore.TenPlayerTurnsPassed();
					XRLCore.Core.RenderBase();
				}
				if (Object.IsPlayer() && The.Game.Player.Messages.LastLine != "You are asleep.")
				{
					IComponent<GameObject>.AddPlayerMessage("You are asleep.");
				}
				E.PreventAction = true;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDefenderHitDiceEvent E)
	{
		E.PenetrationBonus += 4;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Duration > 0)
		{
			CheckAsleepOn();
			TurnsAsleep++;
			if (AsleepOn != null && AsleepOn.TryGetPart<Bed>(out var Part))
			{
				Part.ProcessTurnAsleep(Object, TurnsAsleep);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (CanWake(E.Actor))
		{
			E.AddAction("Wake", "wake", "WakeSleeper", null, 'w', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "WakeSleeper")
		{
			GameObject actor = E.Actor;
			if (CanWake(actor) && actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				PlayWorldSound("Sounds/Interact/sfx_interact_creaturesleeping_wake");
				Duration = 0;
				if (actor.IsPlayer())
				{
					if (UsesSleepMode(Object))
					{
						Popup.Show("You press " + Object.poss("activation panel") + ".");
					}
					else
					{
						Popup.Show("You gently shake " + Object.t() + " awake.");
						WakeMessageDone = true;
					}
				}
				else if (Object.IsPlayer())
				{
					if (UsesSleepMode(Object))
					{
						IComponent<GameObject>.AddPlayerMessage(actor.Does("press") + " your activation panel.");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(actor.T() + " gently" + actor.GetVerb("shake") + " you awake.");
						WakeMessageDone = true;
					}
				}
				else if (Visible())
				{
					if (UsesSleepMode(Object))
					{
						IComponent<GameObject>.AddPlayerMessage(actor.Does("press") + " " + Object.poss("activation panel") + ".");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(actor.T() + " gently" + actor.GetVerb("shake") + " " + Object.t() + " awake.");
						WakeMessageDone = true;
					}
				}
				Object.RemoveEffect(this);
				actor.UseEnergy(1000, "Physical Action Wake");
				E.RequestInterfaceExit();
			}
			else if (actor.IsPlayer())
			{
				Popup.Show("You can't figure out how to wake " + Object.t() + ".");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			Object.Twiddle();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("CanChangeBodyPosition");
		Registrar.Register("CanHaveSmartUseConversation");
		Registrar.Register("CanMoveExtremities");
		Registrar.Register("IsMobile");
		Registrar.Register("TakeDamage");
		Registrar.Register("WakeUp");
		base.Register(Object, Registrar);
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
		else if (E.ID == "TakeDamage" || E.ID == "WakeUp")
		{
			if (TurnWentToSleep == The.Game.Turns)
			{
				return true;
			}
			Duration = 0;
			if (UsesSleepMode(Object))
			{
				DidX("exit", "sleep mode by emergency interrupt", null, null, null, null, Object);
			}
			else
			{
				DidX("wake", "up in a daze", null, null, null, null, Object);
			}
			WakeMessageDone = true;
			Object.ApplyEffect(new Dazed(Stat.Random(3, 4)));
			Object.RemoveEffect(this);
		}
		else if (E.ID == "CanChangeBodyPosition" || E.ID == "CanMoveExtremities")
		{
			if (!E.HasFlag("Involuntary"))
			{
				return false;
			}
		}
		else
		{
			if (E.ID == "CanHaveSmartUseConversation")
			{
				return false;
			}
			if (E.ID == "BeforeDeepCopyWithoutEffects")
			{
				UnapplyStats();
			}
			else if (E.ID == "AfterDeepCopyWithoutEffects")
			{
				ApplyStats();
			}
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		_ = Object.Render;
		int num = XRLCore.CurrentFrame % 60;
		if (num > 10 && num < 25)
		{
			E.RenderString = "z";
			E.ColorString = "&C^c";
			return false;
		}
		return true;
	}

	public static string GetSleepMessage(GameObject Object, bool Physical = true, bool Mental = false)
	{
		if (Mental && !Physical)
		{
			string text = (20.in100() ? GameText.GenerateMarkovMessageParagraph() : GameText.GenerateMarkovMessageSentence());
			text = GameText.VariableReplace(text.Trim(), Object);
			if (Object.GetPrimaryFaction() == "Birds")
			{
				text = ((!(Object.GetTag("Species") == "flamingo")) ? TextFilters.Corvid(text) : TextFilters.WaterBird(text));
			}
			Brain brain = Object.Brain;
			if (brain != null && brain.Aquatic)
			{
				text = TextFilters.Fish(text);
			}
			return "{{c|" + text + "}}";
		}
		if (UsesSleepMode(Object))
		{
			return Object.T() + Object.Is + " utterly unresponsive.";
		}
		return Object.T() + Object.GetVerb("snore") + " loudly.";
	}

	public bool IsAsleepOnValid()
	{
		if (!GameObject.Validate(ref AsleepOn))
		{
			return false;
		}
		if (AsleepOn.CurrentCell == null)
		{
			return false;
		}
		if (Object == null)
		{
			return false;
		}
		if (Object.CurrentCell == null)
		{
			return false;
		}
		if (AsleepOn.CurrentCell != Object.CurrentCell)
		{
			return false;
		}
		return true;
	}

	public bool CheckAsleepOn()
	{
		if (IsAsleepOnValid())
		{
			return true;
		}
		AsleepOn = null;
		return false;
	}

	public bool CanWake(GameObject who)
	{
		if (!UsesSleepMode(Object))
		{
			return true;
		}
		if (who.HasSkill("Tinkering_Repair") || who.HasSkill("Tinkering_Tinker1"))
		{
			return true;
		}
		return false;
	}
}
