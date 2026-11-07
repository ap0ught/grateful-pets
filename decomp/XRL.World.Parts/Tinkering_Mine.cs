using System;
using System.Collections.Generic;
using System.Text;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class Tinkering_Mine : IPart, IDisarmingSifrahHandler
{
	public int Timer = -1;

	public bool PlayerMine = true;

	public GameObject Explosive;

	public GameObject Owner;

	public AllegianceSet OwnerAllegiance;

	public string Message = "AfterThrown";

	public bool Armed = true;

	public string ArmedDetailColor = "R";

	public string DisarmedDetailColor = "y";

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Tinkering_Mine tinkering_Mine = new Tinkering_Mine();
		tinkering_Mine.Timer = Timer;
		tinkering_Mine.PlayerMine = PlayerMine;
		if (GameObject.Validate(ref Explosive))
		{
			tinkering_Mine.Explosive = MapInv?.Invoke(Explosive) ?? Explosive.DeepCopy(CopyEffects: false, CopyID: false, MapInv);
			if (tinkering_Mine.Explosive != null)
			{
				tinkering_Mine.Explosive.ForeachPartDescendedFrom((Tinkering_Layable p) =>
				{
					p.ComponentOf = Parent;
				});
			}
		}
		tinkering_Mine.Owner = Owner;
		tinkering_Mine.OwnerAllegiance = OwnerAllegiance;
		tinkering_Mine.Message = Message;
		tinkering_Mine.Armed = Armed;
		tinkering_Mine.ArmedDetailColor = ArmedDetailColor;
		tinkering_Mine.DisarmedDetailColor = DisarmedDetailColor;
		tinkering_Mine.ParentObject = Parent;
		return tinkering_Mine;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (GameObject.Validate(ref Explosive))
		{
			Explosive.TurnTick(TimeTick, Amount);
		}
	}

	public void SetExplosive(GameObject Object)
	{
		if (Object == Explosive)
		{
			return;
		}
		if (Explosive != null)
		{
			Explosive.ForeachPartDescendedFrom((Tinkering_Layable p) =>
			{
				p.ComponentOf = null;
			});
		}
		Explosive = Object;
		Object?.ForeachPartDescendedFrom((Tinkering_Layable p) =>
		{
			p.ComponentOf = ParentObject;
		});
		if (Sidebar.CurrentTarget == Object)
		{
			Sidebar.CurrentTarget = null;
		}
		FlushTransientCaches();
	}

	private bool ExplosiveWantsEvent(int ID, int cascade)
	{
		if (!GameObject.Validate(ref Explosive))
		{
			return false;
		}
		if (!MinEvent.CascadeTo(cascade, 8))
		{
			return false;
		}
		return Explosive.WantEvent(ID, cascade);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != EffectAppliedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != SingletonEvent<GeneralAmnestyEvent>.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetExtrinsicValueEvent.ID && ID != GetExtrinsicWeightEvent.ID && ID != GetInventoryActionsEvent.ID && (ID != GetAdjacentNavigationWeightEvent.ID || Timer <= 0) && ID != GetNavigationWeightEvent.ID && ID != GetShortDescriptionEvent.ID && (ID != PooledEvent<InterruptAutowalkEvent>.ID || !Armed || Timer > 0) && ID != InventoryActionEvent.ID && ID != ObjectCreatedEvent.ID && ID != ObjectEnteredCellEvent.ID && ID != OnDestroyObjectEvent.ID && ID != TookDamageEvent.ID)
		{
			return ExplosiveWantsEvent(ID, cascade);
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		ParentObject.Twiddle();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Timer", Timer);
		E.AddEntry(this, "PlayerMine", PlayerMine);
		E.AddEntry(this, "Explosive", Explosive);
		E.AddEntry(this, "Owner", Owner);
		if (OwnerAllegiance != null)
		{
			E.AddEntry(this, "OwnerAllegiance", OwnerAllegiance.GetDebugInternalsEntry());
		}
		E.AddEntry(this, "Message", Message);
		E.AddEntry(this, "Armed", Armed);
		E.AddEntry(this, "ArmedDetailColor", ArmedDetailColor);
		E.AddEntry(this, "DisarmedDetailColor", DisarmedDetailColor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (Armed && Options.SifrahDisarming && Options.SifrahDisarmingAuto == "Always" && E.Actor.IsPlayer() && ParentObject.Understood() && !ParentObject.OnWorldMap() && TinkeringHelpers.ConsiderScrap(ParentObject, E.Actor))
		{
			int sifrahDisarmingDifficulty = GetSifrahDisarmingDifficulty(Explosive);
			int sifrahDisarmingRating = GetSifrahDisarmingRating(E.Actor);
			if (DisarmingSifrah.IsMastered(ParentObject, sifrahDisarmingDifficulty, sifrahDisarmingRating))
			{
				E.Command = "DisarmMine";
				E.AllowRetry = false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (E.Smart && Armed && E.PhaseMatches(ParentObject) && GameObject.Validate(ref Explosive) && Avoidable(E.Actor))
		{
			GetComponentNavigationWeightEvent.Process(Explosive, E);
			if (E.Weight < 7 && WillTrigger(E.Actor))
			{
				E.MinWeight(7);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (E.Smart && Armed && E.PhaseMatches(ParentObject))
		{
			if (Timer >= 0)
			{
				if (GameObject.Validate(ref Explosive) && Avoidable(E.Actor))
				{
					GetComponentAdjacentNavigationWeightEvent.Process(Explosive, E);
					if (E.Weight < 3 && WillTrigger(E.Actor))
					{
						E.MinWeight(3);
					}
				}
			}
			else if (GameObject.Validate(ref Explosive) && Avoidable(E.Actor))
			{
				GetComponentAdjacentNavigationWeightEvent.Process(Explosive, E);
				if (E.Weight < 2 && WillTrigger(E.Actor))
				{
					E.MinWeight(2);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		OwnerAllegiance = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (!base.HandleEvent(E))
		{
			return false;
		}
		if (E.CascadeTo(8) && GameObject.Validate(ref Explosive) && !E.Dispatch(Explosive))
		{
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (Armed)
		{
			if (IsEMPed())
			{
				Disarm();
			}
			else if (IsBroken() || IsRusted())
			{
				Boom();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (Armed && E.Object == ParentObject && E.Damage.Amount > 0)
		{
			Boom();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		if (GameObject.Validate(ref Explosive))
		{
			E.Value += Explosive.Value;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		if (GameObject.Validate(ref Explosive))
		{
			E.Weight += Explosive.GetWeight();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Armed && Timer > 0)
		{
			if (!ParentObject.HasEffect(typeof(Stasis)))
			{
				Timer--;
			}
			if (Timer <= 0)
			{
				Boom();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!RouteEventToExplosiveModifications(E))
		{
			return false;
		}
		if (E.Understood())
		{
			if (!GameObject.Validate(ref Explosive))
			{
				E.AddAdjective("empty", -40);
			}
			else if (Armed)
			{
				if (Timer > 0)
				{
					E.AddTag("{{y|[{{R|" + Timer + " sec}}]}}");
				}
			}
			else
			{
				E.AddAdjective("disarmed", -40);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!RouteEventToExplosiveModifications(E))
		{
			return false;
		}
		if (!Armed)
		{
			E.Base.Compound(ParentObject.Ithas).Append(" been disarmed.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			if (Armed)
			{
				E.AddAction("Detonate", "detonate", "Detonate", null, 'n', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
			}
			if (ParentObject.CurrentCell != null && !ParentObject.OnWorldMap())
			{
				if (Armed)
				{
					E.AddAction("Disarm", "disarm", "DisarmMine", null, 'd', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				}
				else
				{
					E.AddAction("Arm", "arm", "ArmMine", null, 'a', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				}
			}
			else
			{
				GameObject inInventory = ParentObject.InInventory;
				if (inInventory != null && inInventory.IsPlayer() && !ParentObject.InInventory.OnWorldMap())
				{
					if (Timer > 0)
					{
						E.AddAction("SetBomb", "set", "LayMine", null, 's', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
					}
					else
					{
						E.AddAction("LayMine", "lay", "LayMine", null, 'L', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "LayMine")
		{
			if (!CheckInteraction(E.Actor))
			{
				return false;
			}
			if (!AttemptLay(E.Actor))
			{
				return false;
			}
			E.RequestInterfaceExit();
		}
		else if (E.Command == "ArmMine")
		{
			if (!CheckInteraction(E.Actor))
			{
				return false;
			}
			if (!AttemptArm(E.Actor))
			{
				return false;
			}
			E.RequestInterfaceExit();
		}
		else if (E.Command == "DisarmMine")
		{
			if (!CheckInteraction(E.Actor))
			{
				return false;
			}
			if (!AttemptDisarm(E.Actor, E, E.Auto))
			{
				return false;
			}
			E.RequestInterfaceExit();
		}
		else if (E.Command == "Detonate")
		{
			if (!CheckInteraction(E.Actor))
			{
				return false;
			}
			if (!Armed)
			{
				return false;
			}
			if (!Boom())
			{
				return false;
			}
			E.Actor.UseEnergy(1000, "Item Tinkering " + ((Timer > 0) ? "Bomb" : "Mine") + " Detonate");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		Disarm();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (Armed && Timer <= 0 && WillTrigger(E.Object))
		{
			Boom();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		if (GameObject.Validate(ref Explosive))
		{
			Explosive.Obliterate();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		if (Armed && Timer <= 0 && GameObject.Validate(ref Explosive) && Avoidable(E.Actor) && WillTrigger(E.Actor))
		{
			E.IndicateObject = ParentObject;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineFailureEvent E)
	{
		if (ExamineFailure(E, 50))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (ExamineFailure(E, 75))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100())
		{
			E.IdentifyIfDestroyed = true;
			if (Boom())
			{
				return true;
			}
		}
		return false;
	}

	private bool RouteEventToExplosiveModifications(MinEvent E)
	{
		if (GameObject.Validate(ref Explosive))
		{
			List<IModification> partsDescendedFrom = Explosive.GetPartsDescendedFrom<IModification>();
			if (partsDescendedFrom.Count > 0)
			{
				int iD = E.ID;
				int cascadeLevel = E.GetCascadeLevel();
				int i = 0;
				for (int count = partsDescendedFrom.Count; i < count; i++)
				{
					IModification modification = partsDescendedFrom[i];
					if (modification.WantEvent(iD, cascadeLevel))
					{
						if (!E.Dispatch(modification))
						{
							return false;
						}
						if (!modification.HandleEvent(E))
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanBeDisassembled");
		Registrar.Register("CanBeTaken");
		base.Register(Object, Registrar);
	}

	public bool WillTrigger(GameObject Actor)
	{
		if (GameObject.Validate(ref Actor) && GameObject.Validate(ref Explosive) && Actor.IsCombatObject() && Actor.PhaseAndFlightMatches(ParentObject) && ConsiderHostile(Actor))
		{
			return true;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "CanBeTaken" || E.ID == "CanBeDisassembled") && Armed)
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool Boom()
	{
		if (!GameObject.Validate(ref Explosive))
		{
			return false;
		}
		GameObject.Validate(ref Owner);
		GameObject explosive = Explosive;
		SetExplosive(null);
		Cell C = ParentObject.GetCurrentCell();
		ParentObject.RemoveFromContext();
		if (C != null)
		{
			C.AddObject(explosive, Forced: true, System: false, IgnoreGravity: true);
			if (explosive.Render != null && ParentObject.Render != null)
			{
				explosive.Render.DisplayName = ParentObject.Render.DisplayName;
			}
			explosive.SetStringProperty("DetonatedSound", "Sounds/Interact/sfx_interact_mine_detonate");
			Temporary.CarryOver(ParentObject, explosive);
			Hidden.CarryOver(ParentObject, explosive);
			Phase.carryOver(ParentObject, explosive);
			if (explosive.ForeachPartDescendedFrom((IGrenade p) => !p.Detonate(C, Owner, null, Indirect: true)))
			{
				Event obj = Event.New(Message);
				if (GameObject.Validate(ref Owner))
				{
					obj.SetParameter("Owner", Owner);
				}
				explosive.FireEvent(obj);
			}
		}
		ParentObject.Destroy(null, Silent: true);
		return true;
	}

	public bool AttemptArm(GameObject Actor)
	{
		if (Armed)
		{
			return false;
		}
		if (Actor.IsPlayer() && !ParentObject.Understood())
		{
			return false;
		}
		if (Actor.OnWorldMap())
		{
			return Actor.Fail("You cannot do that on the world map.");
		}
		if (!Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		Arm(Actor);
		IComponent<GameObject>.XDidYToZ(Actor, "arm", ParentObject, null, null, null, null, Actor, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true);
		Actor.UseEnergy(1000, "Skill Tinkering " + ((Timer > 0) ? "Bomb" : "Mine") + " Arm");
		PlayWorldSound("Sounds/Interact/sfx_interact_mine_arm");
		return true;
	}

	public bool AttemptLay(GameObject Actor)
	{
		if (ParentObject.InInventory != Actor)
		{
			return false;
		}
		if (Armed)
		{
			return false;
		}
		if (Actor.IsPlayer() && !ParentObject.Understood())
		{
			return false;
		}
		if (Actor.OnWorldMap())
		{
			return Actor.Fail("You cannot do that on the world map.");
		}
		if (!Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		Cell cell;
		if (Actor.IsPlayer())
		{
			string text = XRL.UI.PickDirection.ShowPicker("Lay Mine");
			if (text == null)
			{
				return false;
			}
			cell = Actor.CurrentCell.GetCellFromDirection(text);
		}
		else
		{
			cell = Actor.CurrentCell.GetEmptyAdjacentCells().GetRandomElement();
		}
		if (cell == null || !cell.IsEmpty())
		{
			return Actor.Fail("You can't deploy there!");
		}
		GameObject gameObject = ParentObject.RemoveOne();
		gameObject.RemoveFromContext();
		gameObject.GetPart<Tinkering_Mine>().Arm(Actor);
		string verb = ((Timer > 0) ? "set" : "lay");
		IComponent<GameObject>.XDidYToZ(Actor, verb, gameObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, !Actor.IsPlayer());
		cell.AddObject(gameObject);
		Actor.UseEnergy(1000, "Skill Tinkering " + ((Timer > 0) ? "Mine Lay" : "Bomb Set"));
		gameObject.PlayWorldSound("Sounds/Interact/sfx_interact_mine_lay");
		return true;
	}

	public static int GetSifrahDisarmingDifficulty(GameObject Explosive)
	{
		return Explosive.GetTier();
	}

	public static int GetSifrahDisarmingRating(GameObject Actor)
	{
		int num = Actor.Stat("Intelligence");
		if (Actor.HasSkill("Tinkering_GadgetInspector"))
		{
			num += 4;
		}
		if (Actor.HasSkill("Tinkering_LayMine"))
		{
			num += 4;
		}
		return num;
	}

	public bool AttemptDisarm(GameObject Actor, IEvent FromEvent = null, bool Auto = false)
	{
		if (!Armed)
		{
			return false;
		}
		if (Actor.IsPlayer() && !ParentObject.Understood())
		{
			return false;
		}
		if (Actor.OnWorldMap())
		{
			return Actor.Fail("You cannot do that on the world map.");
		}
		if (!Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		if (Options.SifrahDisarming)
		{
			int sifrahDisarmingDifficulty = GetSifrahDisarmingDifficulty(Explosive);
			int sifrahDisarmingRating = GetSifrahDisarmingRating(Actor);
			DisarmingSifrah disarmingSifrah = new DisarmingSifrah(ParentObject, sifrahDisarmingDifficulty, sifrahDisarmingRating, Auto);
			disarmingSifrah.HandlerID = ParentObject.ID;
			disarmingSifrah.HandlerPartName = Name;
			disarmingSifrah.Play(ParentObject);
			if (disarmingSifrah.InterfaceExitRequested)
			{
				FromEvent?.RequestInterfaceExit();
			}
		}
		else
		{
			int num = 9 + Explosive.GetTier() + Explosive.GetMark();
			if (Actor.HasSkill("Tinkering_GadgetInspector"))
			{
				num -= 4;
			}
			if (Actor.HasSkill("Tinkering_LayMine"))
			{
				num -= 4;
			}
			string vs = ((Timer <= 0) ? "Tinkering Mine Disarm" : "Tinkering Bomb Disarm");
			int num2 = Actor.SaveChance("Intelligence", num, null, null, vs);
			int num3 = Actor.Stat("Intelligence");
			if (Actor.TryGetPart<Mutations>(out var Part))
			{
				if (Part.HasMutation("Intuition"))
				{
					num3 += 10;
				}
				if (Part.HasMutation("Precognition"))
				{
					num3 += 5;
				}
				if (Part.HasMutation("Skittish"))
				{
					num2 -= 10;
				}
			}
			if (Actor.HasSkill("Discipline_IronMind"))
			{
				num3 += 2;
			}
			if (Actor.HasSkill("Discipline_Lionheart"))
			{
				num2 += 3;
			}
			if (num3 <= 10)
			{
				num2 += 20;
			}
			else if (num3 <= 15)
			{
				num2 += 10;
			}
			else if (num3 <= 20)
			{
				num2 -= 10;
			}
			else if (num3 <= 25)
			{
				num2 -= 5;
			}
			int num4;
			if (num3 <= 10)
			{
				num4 = 20;
			}
			else
			{
				num4 = ((num3 <= 20) ? 10 : ((num3 > 30) ? 1 : 5));
			}
			if (num4 > 1)
			{
				num2 -= num2 % num4;
			}
			if (num2 > 100 - num4)
			{
				num2 = 100 - num4;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (Actor.IsPlayer())
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				string chanceColor = Stat.GetChanceColor(num2);
				stringBuilder.Append("Failing to disarm ").Append(ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true)).Append(" will detonate ")
					.Append(ParentObject.it)
					.Append(". You estimate you have");
				if (num2 < num4)
				{
					stringBuilder.Append(" less than a ").Append(chanceColor).Append(num4.ToString())
						.Append("%");
				}
				else
				{
					stringBuilder.Append(" about a ").Append(chanceColor).Append(num2.ToString())
						.Append("%");
				}
				stringBuilder.Append(chanceColor).Append(" chance of success. Do you want to make the attempt?");
				if (Popup.ShowYesNo(stringBuilder.ToString()) != DialogResult.Yes)
				{
					return false;
				}
			}
			else if (!num2.in100() && !num2.in100())
			{
				return false;
			}
			if (Actor.MakeSave("Intelligence", num, null, null, vs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				IComponent<GameObject>.XDidYToZ(Actor, "disarm", ParentObject, null, null, null, null, Actor, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true);
				Disarm();
			}
			else
			{
				Boom();
			}
		}
		Actor.UseEnergy(1000, "Skill Tinkering Mine Disarm");
		return true;
	}

	public void DisarmingResultSuccess(GameObject Actor, GameObject Object, bool Auto)
	{
		if (Actor.IsPlayer())
		{
			IComponent<GameObject>.EmitMessage(Actor, "You disarm " + Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".", ' ', FromDialog: false, !Auto);
		}
		Disarm();
	}

	public void DisarmingResultExceptionalSuccess(GameObject Actor, GameObject Object, bool Auto)
	{
		DisarmingResultSuccess(Actor, Object, Auto);
		string randomBits = BitType.GetRandomBits("1d4".Roll(), Explosive.GetTier());
		if (!randomBits.IsNullOrEmpty())
		{
			Actor.RequirePart<BitLocker>().AddBits(randomBits);
			if (Actor.IsPlayer())
			{
				IComponent<GameObject>.EmitMessage(Actor, "You receive tinkering bits <{{|" + BitType.GetDisplayString(randomBits) + "}}>", ' ', FromDialog: false, !Auto);
			}
		}
	}

	public void DisarmingResultPartialSuccess(GameObject Actor, GameObject Object, bool Auto)
	{
		if (Actor.IsPlayer())
		{
			IComponent<GameObject>.EmitMessage(Actor, "You make some progress disarming " + Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".", ' ', FromDialog: false, !Auto);
		}
	}

	public void DisarmingResultFailure(GameObject Actor, GameObject Object, bool Auto)
	{
		Boom();
	}

	public void DisarmingResultCriticalFailure(GameObject Actor, GameObject Object, bool Auto)
	{
		List<Cell> localAdjacentCells = ParentObject.CurrentCell.GetLocalAdjacentCells();
		Cell cell = Actor.CurrentCell;
		ParentObject.Discharge((cell != null && localAdjacentCells.Contains(cell)) ? cell : localAdjacentCells.GetRandomElement(), "3d8".Roll(), 0, "2d4", null, Actor, Object);
		Boom();
	}

	public void Disarm()
	{
		if (Armed)
		{
			PlayWorldSound("Sounds/Interact/sfx_interact_mine_disarm");
			Armed = false;
			if (!DisarmedDetailColor.IsNullOrEmpty() && ParentObject.Render != null)
			{
				ParentObject.Render.DetailColor = DisarmedDetailColor;
			}
			ParentObject.RemoveIntProperty("AutoexploreActionAutoget");
		}
	}

	public void Arm(GameObject Actor = null)
	{
		Armed = true;
		Owner = Actor;
		if (Actor?.Brain != null)
		{
			if (OwnerAllegiance == null)
			{
				OwnerAllegiance = new AllegianceSet();
			}
			OwnerAllegiance.Copy(Actor.Brain.Allegiance);
			OwnerAllegiance.SourceID = Actor._BaseID;
		}
		PlayerMine = Owner != null && Owner.IsPlayer();
		if (!ArmedDetailColor.IsNullOrEmpty() && ParentObject.Render != null)
		{
			ParentObject.Render.DetailColor = ArmedDetailColor;
		}
	}

	public bool ConsiderNonHostile(GameObject Actor)
	{
		return !ConsiderHostile(Actor);
	}

	public bool ConsiderHostile(GameObject Actor)
	{
		if (PlayerMine && Actor.IsPlayerControlled())
		{
			return false;
		}
		if (GameObject.Validate(Owner) && Actor.IsHostileTowards(Owner))
		{
			return true;
		}
		if (!OwnerAllegiance.IsNullOrEmpty() && Brain.GetFeelingLevel(OwnerAllegiance.GetBaseFeeling(Actor)) == Brain.FeelingLevel.Hostile)
		{
			return true;
		}
		return false;
	}

	public bool ConsiderAllied(GameObject Actor)
	{
		if (PlayerMine && Actor.IsPlayerControlled())
		{
			return true;
		}
		if (GameObject.Validate(Owner) && Actor.IsAlliedTowards(Owner))
		{
			return true;
		}
		if (!OwnerAllegiance.IsNullOrEmpty() && Brain.GetFeelingLevel(OwnerAllegiance.GetBaseFeeling(Actor)) == Brain.FeelingLevel.Allied)
		{
			return true;
		}
		return false;
	}

	public bool Avoidable(GameObject Actor)
	{
		if (Actor == null)
		{
			return false;
		}
		if (Actor.IsPlayer() && !ParentObject.Understood())
		{
			return false;
		}
		if (ParentObject.TryGetPart<Hidden>(out var Part))
		{
			if (Part.Found)
			{
				return true;
			}
			if (!Actor.IsPlayerControlled() && ConsiderAllied(Actor))
			{
				return true;
			}
		}
		else
		{
			if (PlayerMine && !Actor.IsHostileTowards(IComponent<GameObject>.ThePlayer))
			{
				return true;
			}
			if (ConsiderNonHostile(Actor))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckInteraction(GameObject Actor)
	{
		if (ParentObject.InInventory != Actor && ParentObject.Equipped != Actor)
		{
			if (!Actor.FlightMatches(ParentObject))
			{
				return Actor.Fail("You cannot reach " + ParentObject.t() + ".");
			}
			if (!Actor.PhaseMatches(ParentObject))
			{
				return Actor.Fail("Your " + (Actor.GetFirstBodyPart("Hands")?.GetOrdinalName() ?? "appendages") + " pass through " + ParentObject.t() + ".");
			}
		}
		return true;
	}
}
