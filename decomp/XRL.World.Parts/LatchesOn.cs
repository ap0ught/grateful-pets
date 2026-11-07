using System;
using XRL.Language;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is set to
/// true, which it is not by default, save targets and damage from auto
/// attacks are increased by the standard power load bonus, i.e. 2 for
/// the standard overload power load of 400.
/// </remarks>
[Serializable]
public class LatchesOn : IActivePart
{
	public GameObject LatchedOnto;

	public Cell LeftCell;

	public int InitialSaveTarget = 5;

	public string InitialSaveStat = "Agility";

	public string InitialSaveDifficultyStat = "Agility";

	public int MoveSaveTarget = 15;

	public string MoveSaveStat = "Strength";

	public string MoveSaveDifficultyStat = "Strength";

	public int BreakSaveTarget = 15;

	public string BreakSaveStat = "Strength";

	public string BreakSaveDifficultyStat = "Strength";

	public string Duration = "3d4";

	public string RequiresPartOnTargetOrEquipment;

	public string SuppressedByPartOnTargetOrEquipment;

	public bool AutoAttackOnMoveAway;

	public bool AutoAttackOnMove;

	public bool AutoAttackPerTurn = true;

	public bool BloodSpatterOnLatch = true;

	public bool IsMagnetic;

	public string BehaviorDescription;

	public bool FirstTurn;

	public LatchesOn()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart Part)
	{
		LatchesOn latchesOn = Part as LatchesOn;
		if (latchesOn.LatchedOnto != null || LatchedOnto != null)
		{
			return false;
		}
		if (latchesOn.InitialSaveTarget != InitialSaveTarget)
		{
			return false;
		}
		if (latchesOn.InitialSaveStat != InitialSaveStat)
		{
			return false;
		}
		if (latchesOn.InitialSaveDifficultyStat != InitialSaveDifficultyStat)
		{
			return false;
		}
		if (latchesOn.MoveSaveTarget != MoveSaveTarget)
		{
			return false;
		}
		if (latchesOn.MoveSaveStat != MoveSaveStat)
		{
			return false;
		}
		if (latchesOn.MoveSaveDifficultyStat != MoveSaveDifficultyStat)
		{
			return false;
		}
		if (latchesOn.BreakSaveTarget != BreakSaveTarget)
		{
			return false;
		}
		if (latchesOn.BreakSaveStat != BreakSaveStat)
		{
			return false;
		}
		if (latchesOn.BreakSaveDifficultyStat != BreakSaveDifficultyStat)
		{
			return false;
		}
		if (latchesOn.Duration != Duration)
		{
			return false;
		}
		if (latchesOn.RequiresPartOnTargetOrEquipment != RequiresPartOnTargetOrEquipment)
		{
			return false;
		}
		if (latchesOn.SuppressedByPartOnTargetOrEquipment != SuppressedByPartOnTargetOrEquipment)
		{
			return false;
		}
		if (latchesOn.AutoAttackOnMoveAway != AutoAttackOnMoveAway)
		{
			return false;
		}
		if (latchesOn.AutoAttackOnMove != AutoAttackOnMove)
		{
			return false;
		}
		if (latchesOn.AutoAttackPerTurn != AutoAttackPerTurn)
		{
			return false;
		}
		if (latchesOn.BloodSpatterOnLatch != BloodSpatterOnLatch)
		{
			return false;
		}
		if (latchesOn.IsMagnetic != IsMagnetic)
		{
			return false;
		}
		if (latchesOn.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != EquippedEvent.ID && (ID != GetShortDescriptionEvent.ID || BehaviorDescription.IsNullOrEmpty()))
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(BehaviorDescription);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (FirstTurn)
		{
			FirstTurn = false;
		}
		else if (GetLatchedOnEffectValidated(UseCharge: true) != null && AutoAttackPerTurn)
		{
			Combat.MeleeAttackWithWeapon(ParentObject.Equipped, LatchedOnto, ParentObject, ParentObject.Equipped.Body.FindDefaultOrEquippedItem(ParentObject), "Autohit", 0, 0, 0, MyPowerLoadBonus());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "BeforeDeathRemoval");
		E.Actor.RegisterPartEvent(this, "BeginMove");
		E.Actor.RegisterPartEvent(this, "BeginTakeAction");
		E.Actor.RegisterPartEvent(this, "EnteredCell");
		E.Actor.RegisterPartEvent(this, "LeftCell");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "BeforeDeathRemoval");
		E.Actor.UnregisterPartEvent(this, "BeginMove");
		E.Actor.UnregisterPartEvent(this, "BeginTakeAction");
		E.Actor.UnregisterPartEvent(this, "EnteredCell");
		E.Actor.UnregisterPartEvent(this, "LeftCell");
		LatchedOnto latchedOnEffectValidated = GetLatchedOnEffectValidated(UseCharge: false, 1, E.Actor);
		if (latchedOnEffectValidated != null)
		{
			if (LatchedOnto.ReceiveObject(ParentObject))
			{
				string color = IComponent<GameObject>.ConsequentialColor(null, E.Actor);
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Since " + ParentObject.does("are") + " still latched onto " + LatchedOnto.t() + ", releasing " + ParentObject.them + " leaves " + ParentObject.them + " in " + LatchedOnto.its + " possession!", color);
				}
				else if (LatchedOnto.IsPlayer())
				{
					if (ParentObject.HasProperName)
					{
						IComponent<GameObject>.AddPlayerMessage("Since " + ParentObject.does("are") + " still latched onto you, " + E.Actor.t() + " releasing " + ParentObject.them + " leaves " + ParentObject.them + " in your possession!", color);
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage("Since " + ParentObject.does("are") + " still latched onto you, " + E.Actor.its + " releasing " + ParentObject.them + " leaves " + ParentObject.them + " in your possession!");
					}
				}
				else if (LatchedOnto.IsVisible() && E.Actor.IsVisible())
				{
					if (ParentObject.HasProperName)
					{
						IComponent<GameObject>.AddPlayerMessage("Since " + ParentObject.does("are") + " still latched onto " + LatchedOnto.t() + ", " + E.Actor.t() + " releasing " + ParentObject.them + " leaves " + ParentObject.them + " in " + LatchedOnto.poss("possession") + "!");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage("Since " + ParentObject.does("are") + " still latched onto " + LatchedOnto.t() + ", " + E.Actor.t() + " releasing " + ParentObject.them + " leaves " + ParentObject.them + " in " + LatchedOnto.poss("possession") + "!");
					}
				}
			}
			LatchedOnto.RemoveEffect(latchedOnEffectValidated);
		}
		LatchedOnto = null;
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeDeathRemoval");
		Registrar.Register("CanMeleeAttack");
		Registrar.Register("StopFighting");
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	private bool WeaponMatch(Effect FX)
	{
		if (FX is LatchedOnto latchedOnto)
		{
			return latchedOnto.LatchedOnWeapon == ParentObject;
		}
		return false;
	}

	private GameObject ValidTargetVia(GameObject who)
	{
		if (CanLatchOnto(who))
		{
			return who;
		}
		Body body = who.Body;
		if (body == null)
		{
			return null;
		}
		foreach (BodyPart part in body.GetParts())
		{
			if (CanLatchOnto(part.Equipped))
			{
				return part.Equipped;
			}
			if (CanLatchOnto(part.Cybernetics))
			{
				return part.Cybernetics;
			}
		}
		return null;
	}

	private bool CanLatchOnto(GameObject Object)
	{
		if (Object == null)
		{
			return false;
		}
		if (!SuppressedByPartOnTargetOrEquipment.IsNullOrEmpty() && Object.HasPart(SuppressedByPartOnTargetOrEquipment))
		{
			return false;
		}
		if (!RequiresPartOnTargetOrEquipment.IsNullOrEmpty() && !Object.HasPart(RequiresPartOnTargetOrEquipment))
		{
			return false;
		}
		if (IsMagnetic && !CanBeMagneticallyManipulatedEvent.Check(Object))
		{
			return false;
		}
		return true;
	}

	private bool CheckLatchedOnto()
	{
		if (GameObject.Validate(ref LatchedOnto) && LatchedOnto.IsNowhere())
		{
			LatchedOnto = null;
		}
		return LatchedOnto != null;
	}

	public LatchedOnto GetLatchedOnEffectValidated(out int Distance, bool UseCharge = false, int MaxDistance = 1, GameObject User = null, Cell UseCell = null)
	{
		Distance = 0;
		if (!CheckLatchedOnto())
		{
			return null;
		}
		LatchedOnto latchedOnto = LatchedOnto.GetEffect(typeof(LatchedOnto), WeaponMatch) as LatchedOnto;
		if (latchedOnto != null)
		{
			if (latchedOnto.Duration <= 0)
			{
				latchedOnto = null;
				LatchedOnto.CleanEffects();
			}
			else if ((UseCell != null || !ParentObject.IsNowhere()) && ParentObject.Physics != null && LatchedOnto.PhaseMatches(User ?? ParentObject.Equipped))
			{
				if (UseCell == null)
				{
					Distance = LatchedOnto.DistanceTo(User ?? ParentObject.Equipped);
				}
				else
				{
					Distance = LatchedOnto.DistanceTo(UseCell);
				}
				if (Distance <= MaxDistance && ValidTargetVia(LatchedOnto) != null && !IsDisabled(UseCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					return latchedOnto;
				}
			}
		}
		if (latchedOnto != null)
		{
			LatchedOnto.RemoveEffect(latchedOnto);
		}
		LatchedOnto = null;
		return null;
	}

	public LatchedOnto GetLatchedOnEffectValidated(bool UseCharge = false, int MaxDistance = 1, GameObject User = null, Cell UseCell = null)
	{
		int Distance;
		return GetLatchedOnEffectValidated(out Distance, UseCharge, MaxDistance, User, UseCell);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			GetLatchedOnEffectValidated();
		}
		else if (E.ID == "BeginMove")
		{
			LatchedOnto latchedOnEffectValidated = GetLatchedOnEffectValidated();
			if (latchedOnEffectValidated != null && E.GetParameter("DestinationCell") is Cell c && LatchedOnto.DistanceTo(c) == 2)
			{
				GameObject gameObject = latchedOnEffectValidated.LatchedOnWeapon?.Equipped ?? ParentObject;
				if (!LatchedOnto.FireEvent("BeforeGrabbed") || LatchedOnto.MakeSave(MoveSaveStat, MoveSaveTarget + MyPowerLoadBonus(), gameObject, MoveSaveDifficultyStat, "LatchOn Move Drag Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
				{
					IComponent<GameObject>.XDidY(LatchedOnto, "stand", LatchedOnto.its + " ground", "!", null, null, null, gameObject);
					gameObject.UseEnergy(1000, "Movement Failure");
					return false;
				}
			}
		}
		else if (E.ID == "CanMeleeAttack")
		{
			if (GetLatchedOnEffectValidated() != null)
			{
				return false;
			}
		}
		else if (E.ID == "LeftCell")
		{
			Cell cell = E.GetParameter("Cell") as Cell;
			if (GetLatchedOnEffectValidated(UseCharge: false, 2, null, cell) != null)
			{
				LeftCell = cell;
			}
			else
			{
				LeftCell = null;
			}
		}
		else if (E.ID == "EnteredCell")
		{
			if (GetLatchedOnEffectValidated(out var Distance, UseCharge: false, 2) != null)
			{
				if (Distance == 2 && LeftCell != null)
				{
					string directionFromCell = LatchedOnto.CurrentCell.GetDirectionFromCell(LeftCell);
					LatchedOnto.Move(directionFromCell, Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: true, DoConfirmations: true, ParentObject);
				}
				if (AutoAttackOnMove || (AutoAttackOnMoveAway && Distance == 2))
				{
					Combat.MeleeAttackWithWeapon(ParentObject.Equipped, LatchedOnto, ParentObject, ParentObject.Equipped.Body.FindDefaultOrEquippedItem(ParentObject), "Autohit", 0, 0, 0, MyPowerLoadBonus());
				}
			}
		}
		else if (E.ID == "WeaponHit")
		{
			if (GetLatchedOnEffectValidated() == null)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
				GameObject gameObject2 = ValidTargetVia(gameObjectParameter);
				if (gameObject2 != null && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
					if (gameObjectParameter2 != gameObjectParameter && gameObjectParameter.IsCombat && gameObjectParameter2.PhaseMatches(gameObjectParameter) && !gameObjectParameter.MakeSave(InitialSaveStat, InitialSaveTarget + MyPowerLoadBonus(), ParentObject.Equipped ?? ParentObject, InitialSaveDifficultyStat, "LatchOn Initial Grab Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
					{
						LatchedOnto e = new LatchedOnto(ParentObject, BreakSaveTarget + MyPowerLoadBonus(), BreakSaveStat, BreakSaveDifficultyStat, Stat.Roll(Duration));
						if (gameObjectParameter.FireEvent("BeforeGrabbed") && gameObjectParameter.ApplyEffect(e))
						{
							string text = IComponent<GameObject>.ConsequentialColor(null, gameObjectParameter);
							if (gameObjectParameter.IsPlayer())
							{
								if (ParentObject.HasProperName)
								{
									IComponent<GameObject>.AddPlayerMessage(text + ParentObject.The + ParentObject.ShortDisplayName + text + ParentObject.GetVerb("latch") + " onto " + ((gameObject2 == gameObjectParameter) ? "you" : (gameObject2.HasProperName ? (gameObject2.ShortDisplayName + text) : ("your " + gameObject2.ShortDisplayName + text))) + "!");
								}
								else
								{
									IComponent<GameObject>.AddPlayerMessage(text + Grammar.MakePossessive(gameObjectParameter2.The + gameObjectParameter2.ShortDisplayName) + " " + ParentObject.ShortDisplayName + text + ParentObject.GetVerb("latch") + " onto " + ((gameObject2 == gameObjectParameter) ? "you" : (gameObject2.HasProperName ? (gameObject2.ShortDisplayName + text) : ("your " + gameObject2.ShortDisplayName + text))) + "!");
								}
							}
							else if (gameObjectParameter.IsVisible())
							{
								if (gameObjectParameter2.IsPlayer())
								{
									if (ParentObject.HasProperName)
									{
										IComponent<GameObject>.AddPlayerMessage(text + ParentObject.The + ParentObject.ShortDisplayName + text + ParentObject.GetVerb("latch") + " onto " + ((gameObject2 == gameObjectParameter) ? (gameObjectParameter.the + gameObjectParameter.ShortDisplayName) : (gameObject2.HasProperName ? gameObject2.ShortDisplayName : (Grammar.MakePossessive(gameObjectParameter.the + gameObjectParameter.ShortDisplayName) + text + " " + gameObject2.ShortDisplayName))) + text + "!");
									}
									else
									{
										IComponent<GameObject>.AddPlayerMessage(text + "Your " + ParentObject.ShortDisplayName + text + ParentObject.GetVerb("latch") + " onto " + ((gameObject2 == gameObjectParameter) ? (gameObjectParameter.the + gameObjectParameter.ShortDisplayName) : (gameObject2.HasProperName ? gameObject2.ShortDisplayName : (Grammar.MakePossessive(gameObjectParameter.the + gameObjectParameter.ShortDisplayName) + text + " " + gameObject2.ShortDisplayName))) + text + "!");
									}
								}
								else if (ParentObject.HasProperName)
								{
									IComponent<GameObject>.AddPlayerMessage(text + ParentObject.The + ParentObject.ShortDisplayName + text + ParentObject.GetVerb("latch") + " onto " + ((gameObject2 == gameObjectParameter) ? (gameObjectParameter.the + gameObjectParameter.ShortDisplayName) : (gameObject2.HasProperName ? gameObject2.ShortDisplayName : (Grammar.MakePossessive(gameObjectParameter.the + gameObjectParameter.ShortDisplayName) + text + " " + gameObject2.ShortDisplayName))) + text + "!");
								}
								else
								{
									IComponent<GameObject>.AddPlayerMessage(text + Grammar.MakePossessive(gameObjectParameter2.The + gameObjectParameter2.ShortDisplayName) + " " + ParentObject.ShortDisplayName + text + ParentObject.GetVerb("latch") + " onto " + ((gameObject2 == gameObjectParameter) ? (gameObjectParameter.the + gameObjectParameter.ShortDisplayName) : (gameObject2.HasProperName ? gameObject2.ShortDisplayName : (Grammar.MakePossessive(gameObjectParameter.the + gameObjectParameter.ShortDisplayName) + text + " " + gameObject2.ShortDisplayName))) + text + "!");
								}
							}
							LatchedOnto = gameObjectParameter;
							FirstTurn = true;
							if (BloodSpatterOnLatch)
							{
								LatchedOnto.Bloodsplatter();
							}
						}
					}
				}
			}
		}
		else if (E.ID == "StopFighting")
		{
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Target");
			if (gameObjectParameter3 == null || gameObjectParameter3 == LatchedOnto)
			{
				LatchedOnto latchedOnEffectValidated2 = GetLatchedOnEffectValidated();
				if (latchedOnEffectValidated2 != null)
				{
					latchedOnEffectValidated2.Expired();
					LatchedOnto.RemoveEffect(latchedOnEffectValidated2);
					LatchedOnto = null;
				}
			}
		}
		else if (E.ID == "BeforeDeathRemoval")
		{
			LatchedOnto latchedOnEffectValidated3 = GetLatchedOnEffectValidated();
			if (latchedOnEffectValidated3 != null)
			{
				LatchedOnto.RemoveEffect(latchedOnEffectValidated3);
				LatchedOnto = null;
			}
		}
		return base.FireEvent(E);
	}
}
