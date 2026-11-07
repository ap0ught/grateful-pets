using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class PowerSwitch : IPoweredPart, IHackingSifrahHandler
{
	public bool Active = true;

	public bool CanActivate = true;

	public bool CanDeactivate = true;

	public bool Flippable = true;

	public bool FlippableKinetically = true;

	public bool FlippableWithoutUnderstanding;

	public int ChanceActive = -1;

	public int ActivateActionPriority = 8;

	public int DeactivateActionPriority = 4;

	public int EnergyCost = 1000;

	public string KeyObject;

	public bool GrantLeaderAccess = true;

	public bool GrantAlliesAccess;

	public bool KeylessActivation;

	public bool KeylessDeactivation;

	public string FrequencyCode;

	public string ActivateVerb = "press";

	public string ActivatePreposition = "the power button on";

	public string ActivateExtra;

	public string ActivateSuccessMessage = "=subject.The==subject.name= =verb:start= up with a hum.";

	public string ActivateFailureMessage = "Nothing happens.";

	public string DeactivateVerb = "press";

	public string DeactivatePreposition = "the power button on";

	public string DeactivateExtra;

	public string DeactivateSuccessMessage = "=subject.The==subject.name= =verb:shut= down with a whir.";

	public string DeactivateFailureMessage = "Nothing happens.";

	public string PsychometryAccessMessage = "{{g|You touch =subject.the==subject.name= and recall =pronouns.possessive= passcode. =pronouns.Subjective= =verb:beep:afterpronoun= warmly.}}";

	public string KeyObjectAccessMessage = "=subject.The==subject.name= =verb:recognize= your =object.name=.";

	public string AccessFailureMessage = "{{r|A loud buzz is emitted. The unauthorized glyph flashes on the display.}}";

	public string ActiveAdjective;

	public string InactiveAdjective = "{{K|deactivated}}";

	public string ActiveDescription;

	public string InactiveDescription = "{{C|=pronouns.Subjective= =verb:are:afterpronoun= powered off.}}";

	public string ActiveColorString;

	public string ActiveDetailColor;

	public string InactiveColorString;

	public string InactiveDetailColor;

	public string ActivatedAbilityItemName;

	public string ActivatedAbilityClass;

	public string ActivatedAbilityIcon = "û";

	public Guid ActivatedAbilityID;

	public int SecurityClearance
	{
		get
		{
			return XRL.World.Capabilities.SecurityClearance.GetSecurityClearanceByKeySpecification(KeyObject);
		}
		set
		{
			XRL.World.Capabilities.SecurityClearance.HandleSecurityClearanceSpecification(value, ref KeyObject);
		}
	}

	public PowerSwitch()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		IsPowerSwitchSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		PowerSwitch powerSwitch = p as PowerSwitch;
		if (powerSwitch.Active != Active)
		{
			return false;
		}
		if (powerSwitch.Flippable != Flippable)
		{
			return false;
		}
		if (powerSwitch.FlippableKinetically != FlippableKinetically)
		{
			return false;
		}
		if (powerSwitch.FlippableWithoutUnderstanding != FlippableWithoutUnderstanding)
		{
			return false;
		}
		if (powerSwitch.ChanceActive != ChanceActive)
		{
			return false;
		}
		if (powerSwitch.ActivateActionPriority != ActivateActionPriority)
		{
			return false;
		}
		if (powerSwitch.DeactivateActionPriority != DeactivateActionPriority)
		{
			return false;
		}
		if (powerSwitch.EnergyCost != EnergyCost)
		{
			return false;
		}
		if (powerSwitch.KeyObject != KeyObject)
		{
			return false;
		}
		if (powerSwitch.KeylessActivation != KeylessActivation)
		{
			return false;
		}
		if (powerSwitch.KeylessDeactivation != KeylessDeactivation)
		{
			return false;
		}
		if (powerSwitch.FrequencyCode != FrequencyCode)
		{
			return false;
		}
		if (powerSwitch.ActivateVerb != ActivateVerb)
		{
			return false;
		}
		if (powerSwitch.ActivatePreposition != ActivatePreposition)
		{
			return false;
		}
		if (powerSwitch.ActivateExtra != ActivateExtra)
		{
			return false;
		}
		if (powerSwitch.ActivateSuccessMessage != ActivateSuccessMessage)
		{
			return false;
		}
		if (powerSwitch.ActivateFailureMessage != ActivateFailureMessage)
		{
			return false;
		}
		if (powerSwitch.DeactivateVerb != DeactivateVerb)
		{
			return false;
		}
		if (powerSwitch.DeactivatePreposition != DeactivatePreposition)
		{
			return false;
		}
		if (powerSwitch.DeactivateExtra != DeactivateExtra)
		{
			return false;
		}
		if (powerSwitch.DeactivateSuccessMessage != DeactivateSuccessMessage)
		{
			return false;
		}
		if (powerSwitch.DeactivateFailureMessage != DeactivateFailureMessage)
		{
			return false;
		}
		if (powerSwitch.PsychometryAccessMessage != PsychometryAccessMessage)
		{
			return false;
		}
		if (powerSwitch.AccessFailureMessage != AccessFailureMessage)
		{
			return false;
		}
		if (powerSwitch.ActiveAdjective != ActiveAdjective)
		{
			return false;
		}
		if (powerSwitch.InactiveAdjective != InactiveAdjective)
		{
			return false;
		}
		if (powerSwitch.ActiveDescription != ActiveDescription)
		{
			return false;
		}
		if (powerSwitch.InactiveDescription != InactiveDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterObjectCreatedEvent.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != BootSequenceDoneEvent.ID && (ID != CanSmartUseEvent.ID || !Flippable) && ID != CellChangedEvent.ID && (ID != CommandSmartUseEarlyEvent.ID || !Flippable) && ID != EffectRemovedEvent.ID && ID != EquippedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetHostileWalkRadiusEvent.ID && ID != GetShortDescriptionEvent.ID && ID != GetInventoryActionsAlwaysEvent.ID && ID != InventoryActionEvent.ID && ID != PooledEvent<IsConversationallyResponsiveEvent>.ID && ID != ObjectCreatedEvent.ID && ID != PooledEvent<SyncRenderEvent>.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		SyncActivatedAbilityNameAndIcon();
		ParentObject.Equipped?.DescribeActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		SetUpActivatedAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		SetUpActivatedAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		SetUpActivatedAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		SetUpActivatedAbility(E.Actor);
		E.Actor.RegisterPartEvent(this, GetActivatedAbilityCommandName());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, GetActivatedAbilityCommandName(E.Actor));
		E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (E.Object == ParentObject && !Active && !ParentObject.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == ParentObject && !Active && !ParentObject.HasPart<ArtificialIntelligence>())
		{
			E.Message = ParentObject.T() + ParentObject.Is + " utterly unresponsive.";
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetHostileWalkRadiusEvent E)
	{
		if (!Active)
		{
			E.Radius = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (Active)
		{
			if (!ActiveAdjective.IsNullOrEmpty() && E.Understood() && !E.Object.HasProperName)
			{
				E.AddAdjective(ActiveAdjective);
			}
		}
		else if (!InactiveAdjective.IsNullOrEmpty() && E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective(InactiveAdjective);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Active)
		{
			if (!ActiveDescription.IsNullOrEmpty())
			{
				E.Postfix.Append('\n').Append(GameText.VariableReplace(ActiveDescription, ParentObject));
			}
		}
		else if (!InactiveDescription.IsNullOrEmpty())
		{
			E.Postfix.Append('\n').Append(GameText.VariableReplace(InactiveDescription, ParentObject));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		if (Flippable && (FlippableWithoutUnderstanding || ParentObject.Understood()) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (Active)
			{
				E.AddAction("Deactivate", "deactivate", "PowerSwitchOff", null, 'a', FireOnActor: false, DeactivateActionPriority, 0, Override: false, WorksAtDistance: false, FlippableKinetically);
			}
			else
			{
				E.AddAction("Activate", "activate", "PowerSwitchOn", null, 'a', FireOnActor: false, ActivateActionPriority, 0, Override: false, WorksAtDistance: false, FlippableKinetically);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "PowerSwitchOn")
		{
			TryPowerSwitchOn(E.Actor, E);
		}
		else if (E.Command == "PowerSwitchOff")
		{
			TryPowerSwitchOff(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SyncRenderEvent E)
	{
		SyncRender();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (Flippable && E.Actor.IsPlayer() && (FlippableWithoutUnderstanding || ParentObject.Understood()))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		if (Flippable && E.Actor.IsPlayer() && (FlippableWithoutUnderstanding || ParentObject.Understood()) && !ParentObject.HasTagOrProperty("SuppressPowerSwitchTwiddle"))
		{
			ParentObject.Twiddle();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (ChanceActive >= 0)
		{
			Active = ChanceActive.in100();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		SyncRenderEvent.Send(ParentObject);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("LiquidFueledPowerPlantFueled");
		Registrar.Register("PowerSwitchActivate");
		Registrar.Register("PowerSwitchDeactivate");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "PowerSwitchActivate")
		{
			if ((!CanActivate || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L)) && !E.HasFlag("Forced"))
			{
				ParentObject.FireEvent("PowerSwitchActivateFailed");
				return false;
			}
			Active = true;
			PowerSwitchFlippedEvent.Send(E.GetGameObjectParameter("Actor"), ParentObject);
			ParentObject.FireEvent("PowerSwitchActivated");
			SyncRenderEvent.Send(ParentObject);
			SyncActivatedAbilityNameAndIcon();
		}
		else if (E.ID == "PowerSwitchDeactivate")
		{
			if ((!CanDeactivate || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L)) && !E.HasFlag("Forced"))
			{
				ParentObject.FireEvent("PowerSwitchDeactivateFailed");
				return false;
			}
			Active = false;
			PowerSwitchFlippedEvent.Send(E.GetGameObjectParameter("Actor"), ParentObject);
			ParentObject.FireEvent("PowerSwitchDeactivated");
			SyncRenderEvent.Send(ParentObject);
			SyncActivatedAbilityNameAndIcon();
		}
		else if (E.ID.StartsWith(GetActivatedAbilityCommandNamePrefix()) && E.ID == GetActivatedAbilityCommandName())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
			if (Active)
			{
				TryPowerSwitchOff(gameObjectParameter, E);
			}
			else
			{
				TryPowerSwitchOn(gameObjectParameter, E);
			}
			SyncActivatedAbilityNameAndIcon();
		}
		else if (E.ID == "LiquidFueledPowerPlantFueled")
		{
			SetUpActivatedAbility();
		}
		return base.FireEvent(E);
	}

	public override void SyncRender()
	{
		base.SyncRender();
		if (Active)
		{
			if (!ActiveColorString.IsNullOrEmpty())
			{
				ParentObject.Render.ColorString = ActiveColorString;
			}
			if (!ActiveDetailColor.IsNullOrEmpty())
			{
				ParentObject.Render.DetailColor = ActiveDetailColor;
			}
		}
		else
		{
			if (!InactiveColorString.IsNullOrEmpty())
			{
				ParentObject.Render.ColorString = InactiveColorString;
			}
			if (!InactiveDetailColor.IsNullOrEmpty())
			{
				ParentObject.Render.DetailColor = InactiveDetailColor;
			}
		}
	}

	public bool AccessCheck(GameObject Actor, bool Silent = false, IEvent FromEvent = null)
	{
		if (KeyObject.IsNullOrEmpty())
		{
			return true;
		}
		if (Actor == null)
		{
			return false;
		}
		if (GrantLeaderAccess)
		{
			if (Actor.IsPlayer())
			{
				if (ParentObject.IsPlayerControlled())
				{
					return true;
				}
			}
			else if (Actor == ParentObject.PartyLeader)
			{
				return true;
			}
		}
		if (GrantAlliesAccess && ParentObject.IsAlliedTowards(Actor))
		{
			return true;
		}
		List<string> list = KeyObject.CachedCommaExpansion();
		GameObject gameObject = Actor.FindContainedObjectByAnyBlueprint(list);
		if (gameObject != null)
		{
			if (!Silent && !KeyObjectAccessMessage.IsNullOrEmpty())
			{
				IComponent<GameObject>.EmitMessage(Actor, GameText.VariableReplace(KeyObjectAccessMessage, ParentObject, gameObject), ' ', FromDialog: true);
			}
			return true;
		}
		if (list.Contains("*Psychometry") && UsePsychometry(Actor, ParentObject))
		{
			if (!Silent && !PsychometryAccessMessage.IsNullOrEmpty())
			{
				IComponent<GameObject>.EmitMessage(Actor, GameText.VariableReplace(PsychometryAccessMessage, ParentObject), ' ', FromDialog: true);
			}
			return true;
		}
		if (Actor.IsPlayer() && Options.SifrahHacking && IsHackable() && ParentObject.GetIntProperty("SifrahHack") >= 0)
		{
			int num = XRL.World.Capabilities.SecurityClearance.GetSecurityClearanceByKeySpecification(KeyObject);
			if (!KeyObject.CachedCommaExpansion().Contains("*Psychometry"))
			{
				num += 2;
			}
			HackingSifrah hackingSifrah = new HackingSifrah(ParentObject, num, num, Actor.Stat("Intelligence"));
			hackingSifrah.HandlerID = ParentObject.ID;
			hackingSifrah.HandlerPartName = GetType().Name;
			hackingSifrah.Play(ParentObject);
			if (hackingSifrah.InterfaceExitRequested)
			{
				FromEvent?.RequestInterfaceExit();
			}
			if (ParentObject.GetIntProperty("SifrahHack") > 0)
			{
				ParentObject.ModIntProperty("SifrahHack", -1, RemoveIfZero: true);
				return true;
			}
		}
		if (!Silent && !AccessFailureMessage.IsNullOrEmpty())
		{
			IComponent<GameObject>.EmitMessage(Actor, GameText.VariableReplace(AccessFailureMessage, ParentObject), ' ', FromDialog: true);
		}
		return false;
	}

	public bool IsHackable()
	{
		if (!KeyObject.IsNullOrEmpty())
		{
			return XRL.World.Capabilities.SecurityClearance.GetSecurityClearanceByKeySpecification(KeyObject) > 0;
		}
		return false;
	}

	public void HackingResultSuccess(GameObject Actor, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			ParentObject.ModIntProperty("SifrahHack", 1);
			if (Actor.IsPlayer())
			{
				Popup.Show("You hack " + obj.t() + ".");
			}
		}
	}

	public void HackingResultExceptionalSuccess(GameObject Actor, GameObject obj, HackingSifrah game)
	{
		if (obj != ParentObject)
		{
			return;
		}
		ParentObject.ModIntProperty("SifrahHack", 1);
		if (KeyObject.IsNullOrEmpty())
		{
			return;
		}
		List<string> list = KeyObject.CachedCommaExpansion();
		int num = 0;
		while (++num < 10)
		{
			try
			{
				if (70.in100())
				{
					string randomBits = BitType.GetRandomBits("2d4".Roll(), obj.GetTechTier());
					if (!randomBits.IsNullOrEmpty())
					{
						Actor.RequirePart<BitLocker>().AddBits(randomBits);
						if (Actor.IsPlayer())
						{
							Popup.Show("You hack " + ParentObject.t() + " and find tinkering bits <{{|" + BitType.GetDisplayString(randomBits) + "}}> in " + ParentObject.them + "!");
						}
						break;
					}
					continue;
				}
				GameObject gameObject = GameObject.Create(list.GetRandomElement());
				if (gameObject != null)
				{
					if (Actor.IsPlayer())
					{
						Popup.Show("You hack " + ParentObject.t() + " and find " + gameObject.an() + " stuck in " + ParentObject.them + "!");
					}
					Actor.ReceiveObject(gameObject);
					break;
				}
			}
			catch
			{
			}
		}
	}

	public void HackingResultPartialSuccess(GameObject Actor, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject && Actor.IsPlayer())
		{
			Popup.Show("You feel like you're making progress on hacking " + obj.t() + ".");
		}
	}

	public void HackingResultFailure(GameObject Actor, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			ParentObject.ModIntProperty("SifrahHack", -1);
			if (Actor.IsPlayer())
			{
				Popup.Show("You cannot seem to work out how to hack " + obj.t() + ".");
			}
		}
	}

	public void HackingResultCriticalFailure(GameObject Actor, GameObject obj, HackingSifrah game)
	{
		if (obj != ParentObject)
		{
			return;
		}
		ParentObject.ModIntProperty("SifrahHack", -1);
		if (Actor.HasPart<Dystechnia>())
		{
			Dystechnia.CauseExplosion(ParentObject, Actor);
			game.RequestInterfaceExit();
			return;
		}
		if (Actor.IsPlayer())
		{
			Popup.Show("Your attempt to hack " + obj.t() + " has gone very wrong.");
		}
		List<Cell> localAdjacentCells = ParentObject.CurrentCell.GetLocalAdjacentCells();
		Cell cell = Actor.CurrentCell;
		ParentObject.Discharge((cell != null && localAdjacentCells.Contains(cell)) ? cell : localAdjacentCells.GetRandomElement(), "3d8".Roll(), 0, "2d4", null, Actor, ParentObject);
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		Description part = ParentObject.GetPart<Description>();
		if (part != null)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			part.GetLongDescription(stringBuilder);
			stats.Set("ItemDescription", stringBuilder.ToString());
		}
	}

	public void SetUpActivatedAbility(GameObject Actor = null)
	{
		if (Actor == null)
		{
			Actor = ParentObject.Equipped;
		}
		if (Actor == null)
		{
			return;
		}
		if (Flippable && (FlippableWithoutUnderstanding || !Actor.IsPlayer() || ParentObject.Understood()) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: true, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: true, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ActivatedAbilityID == Guid.Empty)
			{
				string nextAvailableCommandString = Actor.ActivatedAbilities.GetNextAvailableCommandString(GetActivatedAbilityCommandNamePrefix());
				ActivatedAbilityID = Actor.AddActivatedAbility(GetActivatedAbilityName(Actor), nextAvailableCommandString, ActivatedAbilityClass ?? ((Actor == ParentObject) ? "Maneuvers" : "Items"), null, ActivatedAbilityIcon, null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, "CommandPowerSwitch");
				Actor.DescribeActivatedAbility(ActivatedAbilityID, CollectStats);
			}
			SyncActivatedAbilityNameAndIcon(Actor);
		}
		else
		{
			Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		}
	}

	public string GetActivatedAbilityName(GameObject Actor = null)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(Active ? "Deactivate" : "Activate").Append(' ').Append(ActivatedAbilityItemName ?? Grammar.MakeTitleCase(ParentObject.BaseDisplayNameStripped));
		return stringBuilder.ToString();
	}

	[Obsolete("Just proxies to SyncActivatedAbilityNameAndIcon - will remove q3 2024")]
	public void SyncActivatedAbilityName(GameObject Actor = null)
	{
		SyncActivatedAbilityNameAndIcon(Actor);
	}

	public void SyncActivatedAbilityNameAndIcon(GameObject Actor = null)
	{
		if (!(ActivatedAbilityID == Guid.Empty))
		{
			if (Actor == null)
			{
				Actor = ParentObject.Equipped;
			}
			Actor.SetActivatedAbilityDisplayName(ActivatedAbilityID, GetActivatedAbilityName(Actor));
			RenderEvent source = ParentObject.RenderForUI(null, AsIfKnown: true);
			ActivatedAbilityEntry activatedAbility = Actor.GetActivatedAbility(ActivatedAbilityID);
			if (activatedAbility != null)
			{
				activatedAbility.UITileDefault = new Renderable(source);
				activatedAbility.UITileCoolingDown = null;
				activatedAbility.UITileDisabled = null;
				activatedAbility.UITileToggleOn = null;
			}
		}
	}

	public string GetActivatedAbilityCommandNamePrefix()
	{
		return "TogglePowerSwitch";
	}

	public string GetActivatedAbilityCommandName(GameObject From = null)
	{
		return (From ?? ParentObject.Equipped)?.GetActivatedAbility(ActivatedAbilityID)?.Command;
	}

	public void TryPowerSwitchOn(GameObject Actor, IEvent FromEvent = null)
	{
		if (!Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return;
		}
		ParentObject.PlayWorldOrUISound("Sounds/Interact/sfx_interact_powerSwitch_on");
		IComponent<GameObject>.XDidYToZ(Actor, ActivateVerb, ActivatePreposition, ParentObject, ActivateExtra, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		if (KeylessActivation || AccessCheck(Actor, Silent: false, FromEvent))
		{
			string text = GameText.VariableReplace(ActivateSuccessMessage, ParentObject);
			string text2 = GameText.VariableReplace(ActivateFailureMessage, ParentObject);
			if (ParentObject.FireEvent(Event.New("PowerSwitchActivate", "Actor", Actor)))
			{
				if (!text.IsNullOrEmpty())
				{
					IComponent<GameObject>.EmitMessage(Actor, text, ' ', FromDialog: true);
				}
			}
			else if (!text2.IsNullOrEmpty())
			{
				IComponent<GameObject>.EmitMessage(Actor, text2, ' ', FromDialog: true);
			}
		}
		if (EnergyCost > 0)
		{
			Actor.UseEnergy(1000, "Item Activate");
			SyncActivatedAbilityNameAndIcon();
			FromEvent?.RequestInterfaceExit();
		}
	}

	public void TryPowerSwitchOff(GameObject Actor, IEvent FromEvent = null)
	{
		if (!Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return;
		}
		ParentObject.PlayWorldOrUISound("Sounds/Interact/sfx_interact_powerSwitch_off");
		IComponent<GameObject>.XDidYToZ(Actor, DeactivateVerb, DeactivatePreposition, ParentObject, DeactivateExtra, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		if (KeylessDeactivation || AccessCheck(Actor, Silent: false, FromEvent))
		{
			string text = GameText.VariableReplace(DeactivateSuccessMessage, ParentObject);
			string text2 = GameText.VariableReplace(DeactivateFailureMessage, ParentObject);
			if (ParentObject.FireEvent(Event.New("PowerSwitchDeactivate", "Actor", Actor)))
			{
				if (!text.IsNullOrEmpty())
				{
					IComponent<GameObject>.EmitMessage(Actor, text, ' ', FromDialog: true);
				}
			}
			else if (!text2.IsNullOrEmpty())
			{
				IComponent<GameObject>.EmitMessage(Actor, text2, ' ', FromDialog: true);
			}
		}
		if (EnergyCost > 0)
		{
			Actor.UseEnergy(1000, "Item Deactivate");
			SyncActivatedAbilityNameAndIcon();
			FromEvent?.RequestInterfaceExit();
		}
	}
}
