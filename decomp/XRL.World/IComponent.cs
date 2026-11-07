using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ConsoleLib.Console;
using Genkit;
using Newtonsoft.Json;
using UnityEngine;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.Serialization;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public abstract class IComponent<T> : IEventHandler
{
	public static readonly int TurnTickID = MinEvent.RegisterEvent(typeof(IComponent<T>), "TurnTickEvent");

	public abstract IEventBinder Binder { get; }

	public static long frameTimerMS => XRLCore.FrameTimer.ElapsedMilliseconds;

	[JsonIgnore]
	public Cell currentCell => GetBasisCell();

	public virtual bool IsValid => GetComponentBasis() != null;

	public static long currentTurn => The.Game.Turns;

	public static long wallTime => The.Game.WallTime.ElapsedMilliseconds;

	public static XRLCore TheCore => The.Core;

	public static XRLGame TheGame => The.Game;

	public static GameObject ThePlayer => The.Player;

	public static Cell ThePlayerCell => The.Player?.CurrentCell;

	public static string ThePlayerMythDomain => The.Game.GetStringGameState("ThePlayerMythDomain", "glass");

	public static bool TerseMessages
	{
		get
		{
			if (The.Game.Player.Messages.Terse)
			{
				return true;
			}
			return false;
		}
	}

	[JsonIgnore]
	public bool juiceEnabled => Options.UseOverlayCombatEffects;

	public bool IsHidden
	{
		get
		{
			Hidden hidden = GetBasisGameObject()?.GetPart<Hidden>();
			if (hidden != null)
			{
				return !hidden.Found;
			}
			return false;
		}
	}

	[JsonIgnore]
	public ActivatedAbilities MyActivatedAbilities => GetBasisGameObject()?.ActivatedAbilities;

	[JsonIgnore]
	public bool OnWorldMap => GetAnyBasisCell()?.OnWorldMap() ?? false;

	[JsonIgnore]
	public bool IsWorldMapActive => The.ActiveZone?.IsWorldMap() ?? false;

	[JsonIgnore]
	public virtual string DebugName
	{
		get
		{
			GameObject basisGameObject = GetBasisGameObject();
			if (basisGameObject != null)
			{
				return basisGameObject.DebugName + ":" + GetType().Name;
			}
			return GetType().Name;
		}
	}

	public virtual bool HandleEvent(AIAfterMissileEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIAfterThrowEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIBoredEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIGetDefensiveAbilityListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIGetMovementAbilityListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIGetOffensiveItemListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIGetPassiveAbilityListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIGetPassiveItemListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIGetRetreatAbilityListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIHelpBroadcastEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AIWantUseWeaponEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ActorGetNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AddedToInventoryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AdjustTotalWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AdjustValueEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AdjustVisibilityRadiusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AdjustWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterAddActiveObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterAddOpinionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterAddSkillEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterAfterThrownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterBasicBestowalEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterChangePartyLeaderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterConsumeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterContentsTakenEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterDieEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterDismemberEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterElementBestowalEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterExamineCriticalFailureEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterExamineFailureEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterGameLoadedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterInventoryActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterLevelGainedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterMentalAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterMentalDefendEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterMoveFailedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterObjectCreatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterPetEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterPilotChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterPseudoRelicGeneratedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterReadBookEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterRelicGeneratedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterRemoveActiveObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterRemoveSkillEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterReputationChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterShieldBlockEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterThrownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterTravelEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterZoneBuiltEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AllowInventoryStackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AllowPolypPluckingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AllowTradeWithNoInventoryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AnimateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AnyAutoCollectDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AnyRegenerableLimbsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ApplyEffectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AttackerBeforeTemperatureChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AttackerDealingDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AttackerDealtDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AttemptConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AttemptToLandEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AutoexploreObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AwardXPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AwardedXPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AwardingXPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeAITakingActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeAddSkillEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeAfterThrownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeApplyDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeConsumeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeDetonateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeDieEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeDismemberEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeFireMissileWeaponsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeInteriorCollapseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeLevelGainedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeMeleeAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeMentalAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeMentalDefendEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforePilotChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforePlayMusicEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeProjectileHitEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforePullDownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeRemoveSkillEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeRenderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeSetFeelingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeShieldBlockEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeSlamEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeTakeActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeTemperatureChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeTookDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeTravelDownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeUnequippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeZoneBuiltEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeginBeingUnequippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeginConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeginMentalAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeginMentalDefendEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeginTakeActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeingConsumedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BlasphemyPerformedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BlocksRadarEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BodyPositionChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BootSequenceAbortedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BootSequenceDoneEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BootSequenceInitializedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanAcceptObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanApplyEffectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeDismemberedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeMagneticallyManipulatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeModdedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeNamedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeReplicatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeSlottedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeTradedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeUnequippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanChangeMovementModeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanDrinkEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanEnterInteriorEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanFallEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanFireAllMissileWeaponsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanGiveDirectionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanHaveConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanJoinPartyLeaderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanMissilePassForcefieldEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanReceiveEmpathyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanReceiveTelepathyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanRefreshAbilityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanSmartUseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanStartConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanTemperatureReturnToAmbientEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanTradeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanTravelEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CarryingCapacityChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CellChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CellDepletedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ChargeAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ChargeUsedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckAnythingToCleanEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckAnythingToCleanWithEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckAnythingToCleanWithNearbyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckAttackableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckExistenceSupportEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckGasCanAffectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckLoadAmmoEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckOverburdenedOnStrengthUpdateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckPaintabilityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckReadyToFireEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckRealityDistortionAccessibilityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckRealityDistortionAdvisabilityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckRealityDistortionUsabilityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckSpawnMergeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckTileChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckUsesChargeWhileEquippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CleanItemsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CollectBroadcastChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandReloadEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandReplaceCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandSmartUseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandTakeActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ConfigureMissileVisualEffectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ContainsAnyBlueprintEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ContainsBlueprintEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ContainsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DamageConstantAdjustedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DamageDieSizeAdjustedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DeathEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DecorateDefaultEquipmentEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DefendMeleeHitEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DefenderMissileHitEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DerivationCreatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DidInitialEquipEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DidReequipEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DropOnDeathEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DroppedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EarlyBeforeBeginTakeActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EarlyBeforeDeathRemovalEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EffectAppliedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EffectForceAppliedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EffectRemovedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EmbarkEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EncounterChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EncumbranceChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EndActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EndTurnEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnterCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnteredCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnteringCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnteringZoneEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnvironmentalUpdateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EquippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EquipperEquippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ExamineFailureEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ExamineSuccessEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ExtraHostilePerceptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ExtremitiesMovedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FellDownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FindObjectByIdEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FinishChargeAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FinishRechargeAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FlushWeightCacheEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ForceApplyEffectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FrozeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GaveDirectionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GeneralAmnestyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericCommandEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericDeepNotifyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericDeepQueryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericDeepRatingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericNotifyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericQueryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericRatingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetActivationPhaseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAmbientLightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAmmoCountAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetApparentSpeciesEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAttackerHitDiceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAttackerMeleePenetrationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAutoCollectDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAutoEquipPriorityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAvailableComputePowerEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetBandagePerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetBleedLiquidEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCarriedWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCleaningItemsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCleaningItemsNearbyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCleaveAmountEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCleaveMaxPenaltyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCompanionLimitEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCompanionStatusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetContentsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetContextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCookingActionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCooldownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCriticalThresholdEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCyberneticsBehaviorDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDebugInternalsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDefenderHitDiceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDefenderMeleePenetrationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDifficultyEvaluationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDiseaseOnsetEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDisplayNameEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDisplayNamePenetrationColorEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDisplayStatBonusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetElectricalConductivityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetEnergyCostEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetExtraPhysicalFeaturesEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetExtrinsicValueEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetFactionRankEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetFeelingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetFirefightingPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetFixedMissileSpreadEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetFreeDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetGameObjectSortEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetGenderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetHostileWalkRadiusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetHostilityRecognitionLimitsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetIdealTemperatureEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetIntrinsicValueEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetIntrinsicWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetInventoryActionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetInventoryCategoryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetItemElementsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetJumpingBehaviorEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetKineticResistanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetLevelUpDiceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetLevelUpPointsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetLevelUpSkillPointsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetLostChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMatterPhaseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMeleeAttacksEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMissileCoverPercentageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMissileStatusColorEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMissileWeaponPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMissileWeaponProjectileEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMissileWeaponStatusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetModRarityWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMutationTermEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetNamingBestowalChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetNamingChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetNotReadyToFireMessageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetOverloadChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPartyLeaderFollowDistanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPointsOfInterestEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPowerLoadLevelEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPrecognitionRestoreGameStateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPreferredLiquidEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetProjectileBlueprintEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetProjectileObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPropertyModDescription E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPsionicSifrahSetupEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPsychicGlimmerEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRandomBuyChimericBodyPartRollsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRandomBuyMutationCountEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRealityStabilizationPenetrationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRebukeLevelEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRecoilersEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetReplaceCellInteractionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRespiratoryAgentPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRitualSifrahSetupEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRunningBehaviorEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetScanTypeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetShieldBlockPreferenceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetShieldSlamDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetShortDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSkillEffectChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSlotsRequiredEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSlottedInventoryActionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSocialSifrahSetupEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSpaceTimeAnomalyEmergencePermillageChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSpecialEffectChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSpringinessEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSprintDurationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetStorableDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSwimmingPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetThrowProfileEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetThrownWeaponFlexPhaseProviderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetThrownWeaponPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetThrownWeaponRangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTinkeringBonusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetToHitModifierEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTonicCapacityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTonicDosageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTonicDurationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTradePerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTransitiveLocationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetUtilityScoreEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWadingPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWaterRitualCostEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWaterRitualLiquidEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWaterRitualReputationAmountEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWaterRitualSecretWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWaterRitualSellSecretBehaviorEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWeaponHitDiceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWeaponMeleeAttacksEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWeaponMeleePenetrationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetZoneEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetZoneFreezabilityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetZoneSuspendabilityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GiveDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GlimmerChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GravitationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(HasBeenReadEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(HasBlueprintEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(HasFlammableEquipmentEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(HasFlammableEquipmentOrInventoryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(HealsNaturallyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IAICommandListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IAIItemCommandListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IAIMoveCommandListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IActOnItemEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IActualEffectCheckEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IAddSkillEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IAdjacentNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IBootSequenceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IChargeConsumptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IChargeProductionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IChargeStorageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IConsumeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IConversationMinEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IDeathEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IDerivationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IDestroyObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IEffectCheckEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IExamineEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IFinalChargeProductionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IHitDiceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IInitialChargeProductionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IInventoryActionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ILimbRegenerationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ILiquidEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IMeleeAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IMeleePenetrationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IMentalAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(INavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IObjectCellInteractionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IObjectCreationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IObjectInventoryInteractionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IQuestEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IRemoveFromContextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IReplicationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ISaveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IShallowZoneEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IShortDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ITravelEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IValueEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IXPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IZoneEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IZoneOnlyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IdleQueryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ImplantAddedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ImplantRemovedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ImplantedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(InduceVomitingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(InductionChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(InteriorZoneBuiltEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(InterruptAutowalkEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(InventoryActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsAdaptivePenetrationActiveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsAfflictionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsExplosiveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsMutantEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsOverloadableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsRepairableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsRootedInPlaceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsTrueKinEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsVehicleOperationalEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(JoinPartyLeaderPossibleEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(JoinedPartyLeaderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(JumpedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(KilledEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(KilledPlayerEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LateBeforeApplyDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LeaveCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LeavingCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LeftCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LiquidMixedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LoadAmmoEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(MakeTemporaryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(MentalAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(MissilePenetrateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(MissileTraversingCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ModificationAppliedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ModifyAttackingSaveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ModifyBitCostEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ModifyOriginatingSaveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(MovementModeChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(MutationsSubjectToEMPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(NeedPartSupportEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(NeedsReloadEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(NeutronFluxPourExplodesEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(NotifyTargetImmuneEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectCreatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectEnteredCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectEnteringCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectGoingProneEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectLeavingCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectStartedFlyingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectStoppedFlyingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OkayToDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OnDeathRemovalEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OnDestroyObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OnQuestAddedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OwnerAfterInventoryActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OwnerGetShortDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OwnerGetUnknownShortDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PartAddedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PartRemovedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PartSupportEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PathAsBurrowerEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PhysicalContactEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PointDefenseInterceptEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PollForHealingLocationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PowerUpdatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PreferDefaultBehaviorEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PreferTargetEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PreventSmartUseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PrimePowerSystemsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ProducesLiquidEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ProjectileMovingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryBroadcastDrawEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryChargeProductionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryChargeStorageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryDrawEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryEquippableListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryInductionChargeStorageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryRechargeStorageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QuerySlotListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QuestFinishedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QuestStartedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QuestStepFinishedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RadiatesHeatAdjacentEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RadiatesHeatEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RealityStabilizeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RechargeAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RefreshTileEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RegenerateDefaultEquipmentEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RegenerateLimbEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RemoveFromContextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RepaintedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RepairCriticalFailureEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RepairedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ReplaceInContextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ReplaceThrownWeaponEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ReplicaCreatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ReputationChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RespiresEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(SecretVisibilityChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ShieldSlamPerformedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ShotCompleteEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ShouldAttackToReachTargetEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ShouldDescribeStatBonusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(SlamEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(StackCountChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(StartTradeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(StatChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(StockedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(StripContentsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(SubjectToGravityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(SuspendingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(SyncMutationLevelsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(SyncRenderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(SynchronizeExistenceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TakeOnRoleEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TakenEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TestChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ThawedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TookDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TookEnvironmentalDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TookEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TransparentToEMPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TravelSpeedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TriggersMakersMarkCreationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TryRemoveFromContextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UnequippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UnimplantedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UseChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UseDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UseEnergyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UseHealingLocationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UsingChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(VaporizedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(WantsLiquidCollectionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(WasDerivedFromEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(WasReplicatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(WaterRitualStartEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(WorshipPerformedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ZoneActivatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ZoneBuiltEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ZoneDeactivatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ZoneThawedEvent E)
	{
		return true;
	}

	public abstract T GetComponentBasis();

	public virtual GameObject GetBasisGameObject()
	{
		return GetComponentBasis() as GameObject;
	}

	public virtual Cell GetBasisCell()
	{
		return (GetComponentBasis() as Cell) ?? FindBasisCell();
	}

	private Cell FindBasisCell()
	{
		return GetBasisGameObject()?.CurrentCell;
	}

	public virtual Cell GetAnyBasisCell()
	{
		return (GetComponentBasis() as Cell) ?? FindAnyBasisCell();
	}

	private Cell FindAnyBasisCell()
	{
		return GetBasisGameObject()?.GetCurrentCell();
	}

	public virtual Zone GetBasisZone()
	{
		return (GetComponentBasis() as Zone) ?? FindBasisZone();
	}

	private Zone FindBasisZone()
	{
		return GetBasisCell()?.ParentZone;
	}

	public virtual Zone GetAnyBasisZone()
	{
		return (GetComponentBasis() as Zone) ?? FindAnyBasisZone();
	}

	private Zone FindAnyBasisZone()
	{
		return GetAnyBasisCell()?.ParentZone;
	}

	public virtual bool handleDispatch(MinEvent e)
	{
		MetricsManager.LogError("base IComponent::handleDispatch called for " + e.GetType().Name);
		return true;
	}

	public bool hasProperty(string property)
	{
		return GetBasisGameObject()?.HasProperty(property) ?? false;
	}

	public void LogInEditor(string s)
	{
	}

	public void LogWarningInEditor(string s)
	{
	}

	public void LogErrorInEditor(string s)
	{
	}

	public void Log(string s)
	{
		MetricsManager.LogInfo(s);
	}

	public void LogWarning(string s)
	{
		MetricsManager.LogWarning(s);
	}

	public void LogError(string s)
	{
		MetricsManager.LogError(s);
	}

	public void LogError(string s, Exception ex)
	{
		MetricsManager.LogError(s, ex);
	}

	public float ConTarget(GameObject target = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return -1f;
		}
		if (target == null)
		{
			target = basisGameObject.Target;
			if (target == null)
			{
				return -1f;
			}
		}
		if (target.IsPlayer())
		{
			return 1f;
		}
		if (!basisGameObject.HasStat("Level"))
		{
			return 1f;
		}
		if (!target.HasStat("Level"))
		{
			return 0f;
		}
		int num = target.Stat("Level");
		if (target.GetTagOrStringProperty("Role", "Minion") == "Minion")
		{
			num /= 4;
		}
		if (basisGameObject.HasStat("Hitpoints"))
		{
			if (basisGameObject.hitpoints < basisGameObject.BaseStat("Hitpoints") / 2)
			{
				num *= 2;
			}
			if (basisGameObject.hitpoints < basisGameObject.BaseStat("Hitpoints") / 4)
			{
				num *= 2;
			}
		}
		return (float)num / (float)basisGameObject.Stat("Level");
	}

	public float ConTarget(Event E)
	{
		return ConTarget(E.GetGameObjectParameter("Target"));
	}

	public CombatJuiceEntryWorldSound GetJuiceWorldSound(string Clip, float Volume = 0.5f, float PitchVariance = 0f, float Delay = 0f)
	{
		if (!Clip.IsNullOrEmpty() && Options.Sound)
		{
			Cell anyBasisCell = GetAnyBasisCell();
			if (anyBasisCell == null)
			{
				return null;
			}
			if (!anyBasisCell.TryGetSoundPropagation(Mathf.RoundToInt(40f * Volume), out var Cost, out var Occluded))
			{
				return null;
			}
			return new CombatJuiceEntryWorldSound(Clip, Cost, Occluded, Volume, PitchVariance, Delay, anyBasisCell.Location);
		}
		return null;
	}

	public void PlayWorldSound(string Clip, float Volume = 0.5f, float PitchVariance = 0f, bool Combat = false, Cell SourceCell = null, float Delay = 0f, float Pitch = 1f, float CostMultiplier = 1f, int CostMaximum = int.MaxValue)
	{
		if (Clip.IsNullOrEmpty() || !Options.Sound)
		{
			return;
		}
		if (SourceCell == null)
		{
			SourceCell = GetAnyBasisCell();
			if (SourceCell == null)
			{
				return;
			}
		}
		SourceCell.PlayWorldSound(Clip, Volume, PitchVariance, Combat, Delay, Pitch, CostMultiplier, CostMaximum);
	}

	public static void PlayUISound(string Clip, float Volume = 1f, bool Combat = false, SoundRequest.SoundEffectType Effect = SoundRequest.SoundEffectType.None)
	{
		if (!Clip.IsNullOrEmpty() && Options.Sound && (!Combat || Options.UseCombatSounds))
		{
			SoundManager.PlaySound(Clip, 0f, Volume, 1f, Effect);
		}
	}

	public virtual bool FireEvent(Event E)
	{
		return true;
	}

	public bool FireEvent(Event E, IEvent ParentEvent)
	{
		bool result = FireEvent(E);
		ParentEvent?.ProcessChildEvent(E);
		return result;
	}

	public bool IsDay()
	{
		return Calendar.IsDay();
	}

	public bool IsNight()
	{
		return !Calendar.IsDay();
	}

	public static void AddPlayerMessage(string Message, string Color = null, bool Capitalize = true)
	{
		MessageQueue.AddPlayerMessage(Message, Color, Capitalize);
	}

	public static void AddPlayerMessage(string Message, char Color, bool Capitalize = true)
	{
		MessageQueue.AddPlayerMessage(Message, Color, Capitalize);
	}

	public bool IsPlayer()
	{
		GameObject player = The.Player;
		if (player != null)
		{
			return GetBasisGameObject() == player;
		}
		return false;
	}

	public void Reveal()
	{
		(GetBasisGameObject()?.GetPart<Hidden>())?.Reveal();
	}

	public Cell PickDirection(string Label = null, GameObject POV = null)
	{
		return PickDirection(ForAttack: true, Label, POV);
	}

	public string PickDirectionS(string Label = null, GameObject POV = null)
	{
		return PickDirectionS(ForAttack: true, POV, Label);
	}

	public string PickDirectionS(bool ForAttack, GameObject POV = null, string Label = null)
	{
		if (POV == null)
		{
			GameObject basisGameObject = GetBasisGameObject();
			POV = basisGameObject.Holder ?? basisGameObject;
			if (POV == null)
			{
				return null;
			}
		}
		if (POV.IsSelfControlledPlayer())
		{
			return XRL.UI.PickDirection.ShowPicker(Label);
		}
		GameObject target = POV.Target;
		if (target != null)
		{
			return POV.CurrentCell.GetDirectionFromCell(target.CurrentCell);
		}
		return null;
	}

	public Cell PickDirection(bool ForAttack, string Label = null, GameObject POV = null)
	{
		if (POV == null)
		{
			GameObject basisGameObject = GetBasisGameObject();
			POV = basisGameObject.Holder ?? basisGameObject;
			if (POV == null)
			{
				return null;
			}
		}
		if (POV.IsSelfControlledPlayer())
		{
			string text = XRL.UI.PickDirection.ShowPicker(Label);
			if (text != null)
			{
				return POV.CurrentCell.GetCellFromDirection(text);
			}
		}
		else
		{
			GameObject target = POV.Target;
			if (target != null)
			{
				Cell cell = POV.CurrentCell;
				return cell.GetCellFromDirection(cell.GetDirectionFromCell(target.CurrentCell));
			}
		}
		return null;
	}

	public bool HasPropertyOrTag(string TagName)
	{
		return GetBasisGameObject()?.HasPropertyOrTag(TagName) ?? false;
	}

	public bool HasTag(string TagName)
	{
		return GetBasisGameObject()?.HasTag(TagName) ?? false;
	}

	public string GetPropertyOrTag(string Name, string Default = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return Default;
		}
		return basisGameObject.GetPropertyOrTag(Name, Default);
	}

	public string GetTag(string TagName, string Default = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return Default;
		}
		return basisGameObject.GetTag(TagName, Default);
	}

	public List<Cell> PickCloud(int Radius)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return null;
		}
		List<Cell> adjacentCells = basisGameObject.CurrentCell.GetAdjacentCells(Radius);
		if (!basisGameObject.IsSelfControlledPlayer())
		{
			GameObject except = basisGameObject.Brain?.Target;
			foreach (Cell item in adjacentCells)
			{
				if (item.HasRelevantFriendlyExcept(basisGameObject, except))
				{
					return null;
				}
			}
		}
		return adjacentCells;
	}

	public List<Cell> PickLine(int Length = 9999, AllowVis VisLevel = AllowVis.OnlyVisible, Predicate<GameObject> Filter = null, bool IgnoreSolid = false, bool IgnoreLOS = false, bool RequireCombat = true, bool BlackoutStops = false, GameObject Attacker = null, GameObject Projectile = null, string Label = null, bool Snap = false, bool Locked = true)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return Event.NewCellList();
		}
		Cell cell = basisGameObject.CurrentCell;
		if (cell == null)
		{
			return Event.NewCellList();
		}
		Zone parentZone = cell.ParentZone;
		if (parentZone == null)
		{
			return Event.NewCellList();
		}
		List<Point> list = new List<Point>();
		if (basisGameObject.IsSelfControlledPlayer())
		{
			Cell cell2 = cell;
			if (Snap && basisGameObject.GetTotalConfusion() <= 0)
			{
				GameObject gameObject = basisGameObject.Target ?? basisGameObject.GetNearestVisibleObject(Hostile: true, IncludeSolid: !IgnoreSolid, IgnoreLOS: IgnoreLOS, SearchPart: RequireCombat ? "Combat" : "Physics");
				if (gameObject != null)
				{
					cell2 = gameObject.CurrentCell ?? cell2;
				}
			}
			Cell cell3 = PickTarget.ShowPicker(PickTarget.PickStyle.Line, Length, 9999, cell2.X, cell2.Y, Locked, VisLevel, null, Filter, null, null, Label);
			if (cell3 == null)
			{
				return null;
			}
			List<Cell> list2 = Event.NewCellList();
			Zone.Line(cell.X, cell.Y, cell3.X, cell3.Y, list);
			{
				foreach (Point item in list)
				{
					list2.Add(parentZone.GetCell(item.X, item.Y));
				}
				return list2;
			}
		}
		GameObject target = basisGameObject.Target;
		if (target != null)
		{
			if (Filter != null && !Filter(target))
			{
				return null;
			}
			Cell cell4 = target.CurrentCell;
			int x = cell.X - Length;
			int x2 = cell.X + Length;
			int y = cell.Y - Length;
			int y2 = cell.Y + Length;
			parentZone.Constrain(ref x, ref y, ref x2, ref y2);
			List<Cell> list3 = Event.NewCellList();
			for (int i = y; i <= y2; i++)
			{
				for (int j = x; j <= x2; j++)
				{
					list3.Clear();
					bool flag = false;
					Zone.Line(cell.X, cell.Y, j, i, list);
					foreach (Point item2 in list)
					{
						if ((item2.X != cell.X || item2.Y != cell.Y) && item2.X > 0 && item2.Y > 0 && item2.X < parentZone.Width && item2.Y < parentZone.Height)
						{
							Cell cell5 = parentZone.GetCell(item2.X, item2.Y);
							list3.Add(cell5);
							if (cell5 == cell4)
							{
								flag = true;
							}
						}
					}
					if (!flag)
					{
						continue;
					}
					bool flag2 = false;
					for (int k = 0; k < list3.Count; k++)
					{
						if (flag2)
						{
							break;
						}
						if (BlackoutStops && list3[k].IsBlackedOut())
						{
							flag2 = true;
							break;
						}
						if (!IgnoreSolid && list3[k] != cell4 && list3[k].IsSolidForProjectile(Projectile, Attacker, null, target, null, null, Prospective: true))
						{
							flag2 = true;
							break;
						}
						flag2 = list3[k].HasRelevantFriendlyExcept(basisGameObject, target);
					}
					if (!flag2)
					{
						return list3;
					}
				}
			}
		}
		return null;
	}

	public List<Cell> PickCone(int Length = 9999, int Angle = 45, AllowVis VisLevel = AllowVis.OnlyVisible, Predicate<GameObject> Filter = null, string Label = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject.IsInvalid())
		{
			return null;
		}
		if (basisGameObject.IsSelfControlledPlayer())
		{
			Cell cell = basisGameObject.CurrentCell;
			Cell cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.Cone, Angle, Length, cell.X, cell.Y, Locked: false, VisLevel, null, Filter, null, null, Label);
			if (cell2 == null)
			{
				return null;
			}
			List<Location2D> cone = XRL.Rules.Geometry.GetCone(Location2D.Get(cell.X, cell.Y), Location2D.Get(cell2.X, cell2.Y), Length, Angle);
			List<Cell> list = new List<Cell>();
			{
				foreach (Location2D item in cone)
				{
					list.Add(cell2.ParentZone.GetCell(item.X, item.Y));
				}
				return list;
			}
		}
		GameObject target = basisGameObject.Target;
		if (target != null)
		{
			if (Filter != null && !Filter(target))
			{
				return null;
			}
			Cell cell3 = basisGameObject.CurrentCell;
			Cell cell4 = target.CurrentCell;
			Zone parentZone = cell3.ParentZone;
			List<Location2D> cone2 = XRL.Rules.Geometry.GetCone(cell3.Location, cell4.Location, Length, Angle);
			List<Cell> list2 = new List<Cell>();
			foreach (Location2D item2 in cone2)
			{
				list2.Add(parentZone.GetCell(item2.X, item2.Y));
			}
			list2.Remove(cell3);
			if (list2.Contains(cell4))
			{
				foreach (Cell item3 in list2)
				{
					if (item3.HasRelevantFriendlyExcept(basisGameObject, target))
					{
						return null;
					}
				}
				return list2;
			}
		}
		return null;
	}

	public List<Cell> PickBurst(int Radius = 1, int Range = 9999, bool Locked = false, AllowVis VisLevel = AllowVis.OnlyVisible, string Label = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		try
		{
			if (basisGameObject == null)
			{
				return null;
			}
			if (basisGameObject.IsSelfControlledPlayer())
			{
				Cell cell = basisGameObject.CurrentCell;
				if (cell == null)
				{
					return null;
				}
				Cell cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.Burst, Radius, Range, cell.X, cell.Y, Locked, VisLevel, null, null, null, null, Label);
				if (cell2 == null)
				{
					return null;
				}
				List<Cell> result = new List<Cell>();
				result.Add(cell2);
				List<Cell> list = new List<Cell>(8 * Radius);
				for (int i = 0; i < Radius; i++)
				{
					int j = 0;
					for (int count = result.Count; j < count; j++)
					{
						Cell cell3 = result[j];
						if (list.CleanContains(cell3))
						{
							continue;
						}
						list.Add(cell3);
						cell3.ForeachAdjacentCell((Cell AC) =>
						{
							if (AC != null && !result.Contains(AC))
							{
								result.Add(AC);
							}
						});
					}
				}
				return result;
			}
			GameObject target = basisGameObject.Target;
			if (target != null)
			{
				Cell cell4 = basisGameObject.CurrentCell;
				if (cell4 == null)
				{
					return null;
				}
				Zone parentZone = cell4.ParentZone;
				if (parentZone == null)
				{
					return null;
				}
				Cell cell5 = target.CurrentCell;
				if (cell5 == null || cell5.ParentZone != parentZone)
				{
					return null;
				}
				int x = cell4.X - Range;
				int x2 = cell4.X + Range;
				int y = cell4.Y - Range;
				int y2 = cell4.Y + Range;
				parentZone.Constrain(ref x, ref y, ref x2, ref y2);
				for (int num = y; num <= y2; num++)
				{
					for (int num2 = x; num2 <= x2; num2++)
					{
						List<Cell> Cells = new List<Cell>((Radius * 2 + 1) * (Radius * 2 + 1)) { parentZone.GetCell(num2, num) };
						List<Cell> list2 = new List<Cell>((Radius * 2 + 1) * (Radius * 2 + 1));
						for (int num3 = 0; num3 < Radius; num3++)
						{
							int num4 = 0;
							for (int count2 = Cells.Count; num4 < count2; num4++)
							{
								Cell cell6 = Cells[num4];
								if (list2.Contains(cell6))
								{
									continue;
								}
								list2.Add(cell6);
								cell6.ForeachLocalAdjacentCell((Cell AC) =>
								{
									if (!Cells.Contains(AC))
									{
										Cells.Add(AC);
									}
								});
							}
						}
						if (!Cells.Contains(cell5) || Cells.Contains(cell4))
						{
							continue;
						}
						foreach (Cell item in Cells)
						{
							if (item.HasRelevantFriendlyExcept(basisGameObject, target))
							{
								return null;
							}
						}
						return Cells;
					}
				}
			}
			return null;
		}
		catch (Exception x3)
		{
			if (basisGameObject == null)
			{
				MetricsManager.LogException("PickBurst(null)", x3);
			}
			else if (basisGameObject.IsPlayer())
			{
				MetricsManager.LogException("PickBurst(player)", x3);
			}
			else
			{
				MetricsManager.LogException("PickBurst(" + basisGameObject.DebugName + ")", x3);
			}
			return null;
		}
	}

	public List<Cell> PickCircle(int Radius = 1, int Range = 9999, bool Locked = false, AllowVis VisLevel = AllowVis.OnlyVisible, string Label = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject.IsSelfControlledPlayer())
		{
			Cell cell = basisGameObject.CurrentCell;
			if (cell == null)
			{
				return null;
			}
			Cell cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.Circle, Radius, Range, cell.X, cell.Y, Locked, VisLevel, null, null, null, null, Label);
			if (cell2 == null)
			{
				return null;
			}
			List<Cell> list = new List<Cell>();
			list.Add(cell2);
			int x = cell2.X;
			int y = cell2.Y;
			int x2 = cell2.X - Radius;
			int x3 = cell2.X + Radius;
			int y2 = cell2.Y - Radius;
			int y3 = cell2.Y + Radius;
			cell2.ParentZone.Constrain(ref x2, ref y2, ref x3, ref y3);
			for (int i = y2; i <= y3; i++)
			{
				for (int j = x2; j <= x3; j++)
				{
					if (Math.Sqrt((j - x) * (j - x) + (i - y) * (i - y)) <= (double)Radius)
					{
						list.Add(cell2.ParentZone.GetCell(j, i));
					}
				}
			}
			return list;
		}
		Cell cell3 = basisGameObject?.Target?.CurrentCell;
		if (cell3 != null)
		{
			List<Cell> localAdjacentCells = cell3.GetLocalAdjacentCells(1);
			if (!localAdjacentCells.Contains(cell3))
			{
				localAdjacentCells.Add(cell3);
			}
			return localAdjacentCells;
		}
		return null;
	}

	public List<Cell> PickField(int Cells, GameObject Actor = null, string What = null, bool ReturnNullForAbort = false, bool RequireVisibility = false)
	{
		GameObject gameObject = Actor ?? GetBasisGameObject();
		if (gameObject == null)
		{
			return null;
		}
		if (gameObject.IsSelfControlledPlayer())
		{
			if (What.IsNullOrEmpty())
			{
				return PickTarget.ShowFieldPicker(Cells, 1, gameObject.CurrentCell.X, gameObject.CurrentCell.Y, "Wall", StartAdjacent: false, ReturnNullForAbort, AllowDiagonals: false, AllowDiagonalStart: true, RequireVisibility);
			}
			return PickTarget.ShowFieldPicker(Cells, 1, gameObject.CurrentCell.X, gameObject.CurrentCell.Y, What, StartAdjacent: false, ReturnNullForAbort, AllowDiagonals: false, AllowDiagonalStart: true, RequireVisibility);
		}
		return null;
	}

	public List<Cell> PickFieldAdjacent(int Cells, GameObject Actor = null, string What = null, bool ReturnNullForAbort = false, bool RequireVisibility = false)
	{
		GameObject gameObject = Actor ?? GetBasisGameObject();
		if (gameObject == null)
		{
			return null;
		}
		if (gameObject.IsSelfControlledPlayer())
		{
			if (What.IsNullOrEmpty())
			{
				return PickTarget.ShowFieldPicker(Cells, 1, gameObject.CurrentCell.X, gameObject.CurrentCell.Y, "Wall", StartAdjacent: true, ReturnNullForAbort, AllowDiagonals: false, AllowDiagonalStart: true, RequireVisibility);
			}
			return PickTarget.ShowFieldPicker(Cells, 1, gameObject.CurrentCell.X, gameObject.CurrentCell.Y, What, StartAdjacent: true, ReturnNullForAbort, AllowDiagonals: false, AllowDiagonalStart: true, RequireVisibility);
		}
		return null;
	}

	public Cell PickDestinationCell(int Range = 9999, AllowVis VisLevel = AllowVis.OnlyVisible, bool Locked = true, bool IgnoreSolid = false, bool IgnoreLOS = false, bool RequireCombat = true, PickTarget.PickStyle Style = PickTarget.PickStyle.EmptyCell, string Label = null, bool Snap = false, Predicate<GameObject> ExtraVisibility = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject.IsSelfControlledPlayer())
		{
			Cell cell = basisGameObject.CurrentCell;
			if (Snap && basisGameObject.GetTotalConfusion() <= 0)
			{
				GameObject gameObject = basisGameObject.Target ?? basisGameObject.GetNearestVisibleObject(Hostile: true, IncludeSolid: !IgnoreSolid, IgnoreLOS: IgnoreLOS, SearchPart: RequireCombat ? "Combat" : "Physics", Radius: 80, ExtraVisibility: ExtraVisibility);
				if (gameObject != null)
				{
					cell = gameObject.CurrentCell ?? cell;
				}
			}
			if (cell != null)
			{
				return PickTarget.ShowPicker(Style, 1, Range, cell.X, cell.Y, Locked, VisLevel, ExtraVisibility, null, null, null, Label);
			}
		}
		else
		{
			GameObject target = basisGameObject.Target;
			if (target != null)
			{
				return target.CurrentCell;
			}
		}
		return null;
	}

	public bool DoIHaveAMissileWeapon()
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return false;
		}
		List<GameObject> missileWeapons = basisGameObject.GetMissileWeapons();
		if (missileWeapons != null && missileWeapons.Count > 0)
		{
			foreach (GameObject item in missileWeapons)
			{
				MissileWeapon part = item.GetPart<MissileWeapon>();
				if (part != null && part.ReadyToFire())
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Dictionary<string, int> MapFromString(string raw)
	{
		string[] array = raw.Split(',');
		Dictionary<string, int> dictionary = new Dictionary<string, int>(array.Length);
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(':');
			if (array3.Length != 2)
			{
				throw new Exception("bad element in string map: " + text);
			}
			string key = array3[0];
			int value = Convert.ToInt32(array3[1]);
			dictionary.Add(key, value);
		}
		return dictionary;
	}

	public bool LiquidAvailable(string LiquidID, int Amount = 1, bool impureOkay = true)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return false;
		}
		return basisGameObject.GetFreeDrams(LiquidID, null, null, null, impureOkay) >= Amount;
	}

	public bool ConsumeLiquid(string LiquidID, int Amount = 1, bool impureOkay = true)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return false;
		}
		if (impureOkay)
		{
			return basisGameObject.UseImpureDrams(Amount, LiquidID);
		}
		return basisGameObject.UseDrams(Amount, LiquidID);
	}

	public static void EmitMessage(GameObject Source, string Msg, char Color = ' ', bool FromDialog = false, bool UsePopup = false, bool AlwaysVisible = false, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		Messaging.EmitMessage(Source, Msg, Color, FromDialog, UsePopup, AlwaysVisible, ColorAsGoodFor, ColorAsBadFor);
	}

	public void EmitMessage(GameObject Source, StringBuilder Msg, char Color = ' ', bool FromDialog = false, bool UsePopup = false, bool AlwaysVisible = false, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		EmitMessage(Source, Msg.ToString(), Color, FromDialog, UsePopup, AlwaysVisible, ColorAsGoodFor, ColorAsBadFor);
	}

	public void EmitMessage(string Msg, char Color = ' ', bool FromDialog = false, bool UsePopup = false, bool AlwaysVisible = false, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		EmitMessage(GetBasisGameObject(), Msg, Color, FromDialog, UsePopup, AlwaysVisible, ColorAsGoodFor, ColorAsBadFor);
	}

	public void EmitMessage(StringBuilder Msg, char Color = ' ', bool FromDialog = false, bool UsePopup = false, bool AlwaysVisible = false, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		EmitMessage(Msg.ToString(), Color, FromDialog, UsePopup, AlwaysVisible, ColorAsGoodFor, ColorAsBadFor);
	}

	protected static string ConsequentialColor(GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		return ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
	}

	protected static char ConsequentialColorChar(GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		return ColorCoding.ConsequentialColorChar(ColorAsGoodFor, ColorAsBadFor);
	}

	protected static string ConsequentialColorize(string Text, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, char Default = ' ')
	{
		return ColorCoding.ConsequentialColorize(Text, ColorAsGoodFor, ColorAsBadFor, Default);
	}

	public static void XDidY(GameObject Actor, string Verb, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, GameObject SubjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidY(Actor, Verb, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, SubjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void XDidY(string SubjectOverride, string Verb, string Extra = null, string EndMark = null, GameObject Actor = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, GameObject SubjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidY(Actor, Verb, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, SubjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public void DidX(string Verb, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, GameObject SubjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidY(GetBasisGameObject(), Verb, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, SubjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void XDidYToZ(GameObject Actor, string Verb, string Preposition, GameObject Object, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidYToZ(Actor, Verb, Preposition, Object, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteObject, IndefiniteObjectForOthers, PossessiveObject, SubjectPossessedBy, ObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void XDidYToZ(string SubjectOverride, string Verb, string Preposition, GameObject Object, string Extra = null, string EndMark = null, GameObject Actor = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidYToZ(Actor, Verb, Preposition, Object, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteObject, IndefiniteObjectForOthers, PossessiveObject, SubjectPossessedBy, ObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void XDidYToZ(GameObject Actor, string Verb, GameObject Object, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidYToZ(Actor, Verb, null, Object, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteObject, IndefiniteObjectForOthers, PossessiveObject, SubjectPossessedBy, ObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void XDidYToZ(string SubjectOverride, string Verb, GameObject Object, string Extra = null, string EndMark = null, GameObject Actor = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidYToZ(Actor, Verb, null, Object, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteObject, IndefiniteObjectForOthers, PossessiveObject, SubjectPossessedBy, ObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public void DidXToY(string Verb, string Preposition, GameObject Object, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidYToZ(GetBasisGameObject(), Verb, Preposition, Object, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteObject, IndefiniteObjectForOthers, PossessiveObject, SubjectPossessedBy, ObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public void DidXToY(string Verb, GameObject Object, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidYToZ(GetBasisGameObject(), Verb, null, Object, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteObject, IndefiniteObjectForOthers, PossessiveObject, SubjectPossessedBy, ObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void WDidXToYWithZ(GameObject Actor, string Verb, string DirectPreposition, GameObject DirectObject, string IndirectPreposition, GameObject IndirectObject, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteDirectObject = false, bool IndefiniteIndirectObject = false, bool IndefiniteDirectObjectForOthers = false, bool IndefiniteIndirectObjectForOthers = false, bool PossessiveDirectObject = false, bool PossessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject DirectObjectPossessedBy = null, GameObject IndirectObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.WDidXToYWithZ(Actor, Verb, DirectPreposition, DirectObject, IndirectPreposition, IndirectObject, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteDirectObject, IndefiniteIndirectObject, IndefiniteDirectObjectForOthers, IndefiniteIndirectObjectForOthers, PossessiveDirectObject, PossessiveIndirectObject, SubjectPossessedBy, DirectObjectPossessedBy, IndirectObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void WDidXToYWithZ(string SubjectOverride, string Verb, string DirectPreposition, GameObject DirectObject, string IndirectPreposition, GameObject IndirectObject, string Extra = null, string EndMark = null, GameObject Actor = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteDirectObject = false, bool IndefiniteIndirectObject = false, bool IndefiniteDirectObjectForOthers = false, bool IndefiniteIndirectObjectForOthers = false, bool PossessiveDirectObject = false, bool PossessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject DirectObjectPossessedBy = null, GameObject IndirectObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.WDidXToYWithZ(Actor, Verb, DirectPreposition, DirectObject, IndirectPreposition, IndirectObject, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteDirectObject, IndefiniteIndirectObject, IndefiniteDirectObjectForOthers, IndefiniteIndirectObjectForOthers, PossessiveDirectObject, PossessiveIndirectObject, SubjectPossessedBy, DirectObjectPossessedBy, IndirectObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void WDidXToYWithZ(GameObject Actor, string Verb, GameObject DirectObject, string IndirectPreposition, GameObject IndirectObject, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteDirectObject = false, bool IndefiniteIndirectObject = false, bool IndefiniteDirectObjectForOthers = false, bool IndefiniteIndirectObjectForOthers = false, bool PossessiveDirectObject = false, bool PossessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject DirectObjectPossessedBy = null, GameObject IndirectObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.WDidXToYWithZ(Actor, Verb, null, DirectObject, IndirectPreposition, IndirectObject, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteDirectObject, IndefiniteIndirectObject, IndefiniteDirectObjectForOthers, IndefiniteIndirectObjectForOthers, PossessiveDirectObject, PossessiveIndirectObject, SubjectPossessedBy, DirectObjectPossessedBy, IndirectObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void WDidXToYWithZ(string SubjectOverride, string Verb, GameObject DirectObject, string IndirectPreposition, GameObject IndirectObject, string Extra = null, string EndMark = null, GameObject Actor = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteDirectObject = false, bool IndefiniteIndirectObject = false, bool IndefiniteDirectObjectForOthers = false, bool IndefiniteIndirectObjectForOthers = false, bool PossessiveDirectObject = false, bool PossessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject DirectObjectPossessedBy = null, GameObject IndirectObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.WDidXToYWithZ(Actor, Verb, null, DirectObject, IndirectPreposition, IndirectObject, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteDirectObject, IndefiniteIndirectObject, IndefiniteDirectObjectForOthers, IndefiniteIndirectObjectForOthers, PossessiveDirectObject, PossessiveIndirectObject, SubjectPossessedBy, DirectObjectPossessedBy, IndirectObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public void DidXToYWithZ(string Verb, string DirectPreposition, GameObject DirectObject, string IndirectPreposition, GameObject IndirectObject, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteDirectObject = false, bool IndefiniteIndirectObject = false, bool IndefiniteDirectObjectForOthers = false, bool IndefiniteIndirectObjectForOthers = false, bool PossessiveDirectObject = false, bool PossessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject DirectObjectPossessedBy = null, GameObject IndirectObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.WDidXToYWithZ(GetBasisGameObject(), Verb, DirectPreposition, DirectObject, IndirectPreposition, IndirectObject, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteDirectObject, IndefiniteIndirectObject, IndefiniteDirectObjectForOthers, IndefiniteIndirectObjectForOthers, PossessiveDirectObject, PossessiveIndirectObject, SubjectPossessedBy, DirectObjectPossessedBy, IndirectObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public void DidXToYWithZ(string Verb, GameObject DirectObject, string IndirectPreposition, GameObject IndirectObject, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteDirectObject = false, bool IndefiniteIndirectObject = false, bool IndefiniteDirectObjectForOthers = false, bool IndefiniteIndirectObjectForOthers = false, bool PossessiveDirectObject = false, bool PossessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject DirectObjectPossessedBy = null, GameObject IndirectObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.WDidXToYWithZ(GetBasisGameObject(), Verb, null, DirectObject, IndirectPreposition, IndirectObject, Extra, EndMark, SubjectOverride, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteDirectObject, IndefiniteIndirectObject, IndefiniteDirectObjectForOthers, IndefiniteIndirectObjectForOthers, PossessiveDirectObject, PossessiveIndirectObject, SubjectPossessedBy, DirectObjectPossessedBy, IndirectObjectPossessedBy, Source, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public bool IsBroken()
	{
		return GetBasisGameObject()?.IsBroken() ?? false;
	}

	public bool IsRusted()
	{
		return GetBasisGameObject()?.IsRusted() ?? false;
	}

	public bool IsEMPed()
	{
		return GetBasisGameObject()?.IsEMPed() ?? false;
	}

	public static bool Visible(GameObject Object)
	{
		return Object?.IsVisible() ?? false;
	}

	public bool Visible()
	{
		return Visible(GetBasisGameObject());
	}

	public ActivatedAbilities GetMyActivatedAbilities(GameObject who = null)
	{
		return (who ?? GetBasisGameObject())?.ActivatedAbilities;
	}

	public ActivatedAbilityEntry MyActivatedAbility(Guid ID, GameObject who = null)
	{
		return (who ?? GetBasisGameObject())?.GetActivatedAbility(ID);
	}

	public Guid AddMyActivatedAbility(string Name, string Command, string Class, string Description = null, string Icon = "\a", string DisabledMessage = null, bool Toggleable = false, bool DefaultToggleState = false, bool ActiveToggle = false, bool IsAttack = false, bool IsRealityDistortionBased = false, bool IsWorldMapUsable = false, bool Silent = false, bool AIDisable = false, bool AlwaysAllowToggleOff = true, bool AffectedByWillpower = true, bool TickPerTurn = false, int Cooldown = -1, GameObject who = null, string CommandForDescription = null, Renderable UITileDefault = null, Renderable UITileToggleOn = null, Renderable UITileDisabled = null, Renderable UITileCoolingDown = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return Guid.Empty;
			}
		}
		return who.AddActivatedAbility(Name, Command, Class, Description, Icon, DisabledMessage, Toggleable, DefaultToggleState, ActiveToggle, IsAttack, IsRealityDistortionBased, IsWorldMapUsable, Silent, AIDisable, AlwaysAllowToggleOff, AffectedByWillpower, TickPerTurn, Distinct: false, Cooldown, CommandForDescription, UITileDefault, UITileToggleOn, UITileDisabled, UITileCoolingDown);
	}

	public bool RemoveMyActivatedAbility(ref Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.RemoveActivatedAbility(ref ID);
	}

	public bool DescribeMyActivatedAbility(Guid ID, Action<Templates.StatCollector> statCollector, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.DescribeActivatedAbility(ID, statCollector);
	}

	public bool DescribeMyActivatedAbility(Guid ID, Templates.StatCollector values, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.DescribeActivatedAbility(ID, values);
	}

	public bool EnableMyActivatedAbility(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.EnableActivatedAbility(ID);
	}

	public bool DisableMyActivatedAbility(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.DisableActivatedAbility(ID);
	}

	public bool ToggleMyActivatedAbility(Guid ID, GameObject who = null, bool Silent = false, bool? SetState = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.ToggleActivatedAbility(ID, Silent, SetState);
	}

	public bool IsMyActivatedAbilityToggledOn(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.IsActivatedAbilityToggledOn(ID);
	}

	public bool IsMyActivatedAbilityCoolingDown(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.IsActivatedAbilityCoolingDown(ID);
	}

	public int GetMyActivatedAbilityCooldown(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return -1;
			}
		}
		return who.GetActivatedAbilityCooldown(ID);
	}

	public int GetMyActivatedAbilityCooldownTurns(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return -1;
			}
		}
		return who.GetActivatedAbilityCooldownRounds(ID);
	}

	public string GetMyActivatedAbilityCooldownDescription(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return "";
			}
		}
		return who.GetActivatedAbilityCooldownDescription(ID);
	}

	public bool CooldownMyActivatedAbility(Guid ID, int Turns, GameObject who = null, string tags = null, bool Involuntary = false)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.CooldownActivatedAbility(ID, Turns, tags, Involuntary);
	}

	public bool TakeMyActivatedAbilityOffCooldown(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.TakeActivatedAbilityOffCooldown(ID);
	}

	public bool IsMyActivatedAbilityUsable(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.IsActivatedAbilityUsable(ID);
	}

	public bool IsMyActivatedAbilityAIUsable(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.IsActivatedAbilityAIUsable(ID);
	}

	public bool IsMyActivatedAbilityAIDisabled(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.IsActivatedAbilityAIDisabled(ID);
	}

	public bool IsMyActivatedAbilityVoluntarilyUsable(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		if (!who.IsPlayer())
		{
			return IsMyActivatedAbilityAIUsable(ID);
		}
		return IsMyActivatedAbilityUsable(ID);
	}

	public bool SetMyActivatedAbilityDisplayName(Guid ID, string DisplayName, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.SetActivatedAbilityDisplayName(ID, DisplayName);
	}

	public bool SetMyActivatedAbilityDisabledMessage(Guid ID, string DisabledMessage, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.SetActivatedAbilityDisabledMessage(ID, DisabledMessage);
	}

	public void FlushWeightCaches()
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject != null)
		{
			basisGameObject.FlushWeightCaches();
			basisGameObject.FlushContextWeightCaches();
		}
	}

	public void FlushNavigationCaches()
	{
		GetAnyBasisCell()?.FlushNavigationCache();
	}

	public void FlushLocalNavigationCaches()
	{
		Cell anyBasisCell = GetAnyBasisCell();
		if (anyBasisCell == null)
		{
			return;
		}
		anyBasisCell.FlushNavigationCache();
		foreach (Cell localAdjacentCell in anyBasisCell.GetLocalAdjacentCells())
		{
			localAdjacentCell.FlushNavigationCache();
		}
	}

	public void FlushZoneNavigationCaches()
	{
		GetAnyBasisZone()?.FlushNavigationCaches();
	}

	public void FlushTransientCaches()
	{
		GetBasisGameObject()?.FlushTransientCache();
	}

	public int StepValue(int Value, int Step = 5)
	{
		int num = Value % Step;
		if (num == 0)
		{
			return Value;
		}
		if (num >= Step / 2)
		{
			return Value - num + Step;
		}
		return Value - num;
	}

	public virtual int GetEpistemicStatus()
	{
		return GetBasisGameObject()?.GetEpistemicStatus() ?? 2;
	}

	public void AddLight(int r, LightLevel Level = LightLevel.Light, bool Force = false)
	{
		Cell basisCell = GetBasisCell();
		basisCell?.ParentZone?.AddLight(basisCell.X, basisCell.Y, r, Level, Force);
	}

	public static int PowerLoadBonus(int Load, int Baseline = 100, int Divisor = 150)
	{
		if (Load > Baseline)
		{
			return (Load - Baseline) / Divisor;
		}
		return 0;
	}

	public virtual int MyPowerLoadBonus(int Load = int.MinValue, int Baseline = 100, int Divisor = 150)
	{
		if (Load == int.MinValue)
		{
			Load = GetBasisGameObject()?.GetPowerLoadLevel() ?? 100;
		}
		return PowerLoadBonus(Load, Baseline, Divisor);
	}

	public virtual int MyPowerLoadLevel()
	{
		return GetBasisGameObject()?.GetPowerLoadLevel() ?? 100;
	}

	public bool PerformMentalAttack(Mental.Attack Handler, GameObject Attacker, GameObject Defender, GameObject Source = null, string Command = null, string Dice = null, int Type = 0, int Magnitude = int.MinValue, int Penetrations = int.MinValue, int AttackModifier = 0, int DefenseModifier = 0)
	{
		return Mental.PerformAttack(Handler, Attacker, Defender, Source ?? GetBasisGameObject(), Command, Dice, Type, Magnitude, Penetrations, AttackModifier, DefenseModifier);
	}

	public static void ConstrainTier(ref int tier)
	{
		Tier.Constrain(ref tier);
	}

	public static int ConstrainTier(int tier)
	{
		return Tier.Constrain(tier);
	}

	/// <remarks>
	/// The result of this is cached per <see cref="T:XRL.World.GameObject" /> until transient caches are flushed.
	/// Prefer returning a value unlikely to change during the lifetime of the component. 
	/// </remarks>
	public virtual bool WantTurnTick()
	{
		return false;
	}

	/// <summary>Called when in-game time passes, either from turns passing or world map travel.</summary>
	/// <param name="TimeTick">The time tick of the game world, i.e. the value of <see cref="F:XRL.XRLGame.TimeTicks" />.</param>
	/// <param name="Amount">The interval in turns from the last call to TurnTick.</param>
	public virtual void TurnTick(long TimeTick, int Amount)
	{
	}

	[Obsolete("Use WantTurnTick")]
	public virtual bool WantTenTurnTick()
	{
		return false;
	}

	[Obsolete("Use TurnTick(long TimeTick, int Amount)")]
	public virtual void TenTurnTick(long TurnNumber)
	{
	}

	[Obsolete("Use WantTurnTick")]
	public virtual bool WantHundredTurnTick()
	{
		return false;
	}

	[Obsolete("Use TurnTick(long TimeTick, int Amount)")]
	public virtual void HundredTurnTick(long TurnNumber)
	{
	}

	public virtual bool WantEvent(int ID, int Cascade)
	{
		return false;
	}

	public virtual bool HandleEvent(MinEvent E)
	{
		return true;
	}

	/// <inheritdoc cref="M:XRL.Collections.EventRegistry.Register(XRL.IEventHandler,System.Int32,System.Int32,System.Boolean)" />
	public virtual void RegisterEvent(int EventID, int Order = 0, bool Serialize = false)
	{
	}

	/// <inheritdoc cref="M:XRL.Collections.EventRegistry.Unregister(XRL.IEventHandler,System.Int32)" />
	public virtual void UnregisterEvent(int EventID)
	{
	}

	public virtual bool Render(RenderEvent E)
	{
		return true;
	}

	public virtual bool OverlayRender(RenderEvent E)
	{
		return true;
	}

	public virtual bool FinalRender(RenderEvent E)
	{
		return true;
	}

	[Obsolete("Use FinalRender(RenderEvent)")]
	public virtual bool FinalRender(RenderEvent E, bool bAlt)
	{
		return true;
	}

	[Obsolete("Use ISoundRenderer, SoundRendererPart, or SoundRendererEffect")]
	public virtual bool RenderSound(ConsoleChar C, ConsoleChar[,] Buffer)
	{
		return true;
	}

	public static bool CheckRealityDistortionAdvisability(GameObject Object = null, Cell Cell = null, GameObject Actor = null, GameObject Device = null, IPart Mutation = null, int? Threshold = null, int Penetration = 0)
	{
		return CheckRealityDistortionAdvisabilityEvent.Check(Object, Cell, Actor, Device, Mutation, Threshold, Penetration);
	}

	public bool CheckMyRealityDistortionAdvisability(GameObject Object = null, Cell Cell = null, GameObject Actor = null, GameObject Device = null, IPart Mutation = null, int? Threshold = null, int Penetration = 0)
	{
		GameObject basisGameObject = GetBasisGameObject();
		return CheckRealityDistortionAdvisability(Object ?? basisGameObject, Cell, Actor ?? basisGameObject, Device, Mutation ?? (this as IPart), Threshold, Penetration);
	}

	public static bool CheckRealityDistortionUsability(GameObject Object = null, Cell Cell = null, GameObject Actor = null, GameObject Device = null, IPart Mutation = null, int? Threshold = null, int Penetration = 0)
	{
		return CheckRealityDistortionUsabilityEvent.Check(Object, Cell, Actor, Device, Mutation, Threshold, Penetration);
	}

	public bool CheckMyRealityDistortionUsability(GameObject Object = null, Cell Cell = null, GameObject Actor = null, GameObject Device = null, IPart Mutation = null, int? Threshold = null, int Penetration = 0)
	{
		GameObject basisGameObject = GetBasisGameObject();
		return CheckRealityDistortionUsability(Object ?? basisGameObject, Cell, Actor ?? basisGameObject, Device, Mutation ?? (this as IPart), Threshold, Penetration);
	}

	public static bool CheckRealityDistortionAccessibility(GameObject Object)
	{
		return CheckRealityDistortionAccessibilityEvent.Check(Object);
	}

	public static bool CheckRealityDistortionAccessibility(Cell Cell)
	{
		return CheckRealityDistortionAccessibilityEvent.Check(null, Cell);
	}

	public static bool CheckRealityDistortionAccessibility(GameObject Object = null, Cell Cell = null, GameObject Actor = null, GameObject Device = null, IPart Mutation = null, int Penetration = 0)
	{
		return CheckRealityDistortionAccessibilityEvent.Check(Object, Cell, Actor, Device, Mutation, Penetration);
	}

	public bool CheckMyRealityDistortionAccessibility(GameObject Object = null, Cell Cell = null, GameObject Actor = null, GameObject Device = null, IPart Mutation = null, int Penetration = 0)
	{
		GameObject basisGameObject = GetBasisGameObject();
		return CheckRealityDistortionAccessibility(Object ?? basisGameObject, Cell, Actor ?? basisGameObject, Device, Mutation ?? (this as IPart), Penetration);
	}

	public virtual void Write(T Basis, SerializationWriter Writer)
	{
		FieldInfo[] cachedFields = GetType().GetCachedFields();
		foreach (FieldInfo fieldInfo in cachedFields)
		{
			if ((fieldInfo.Attributes & (FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)) == 0)
			{
				object value = fieldInfo.GetValue(this);
				Type fieldType = fieldInfo.FieldType;
				if (fieldType == typeof(Cell))
				{
					Writer.Write((Cell)value);
				}
				else if (fieldType == typeof(ActivatedAbilityEntry))
				{
					Writer.Write((value == null) ? Guid.Empty : ((ActivatedAbilityEntry)value).ID);
				}
				else if (fieldType == typeof(GameObject))
				{
					Writer.WriteGameObject((GameObject)value);
				}
				else
				{
					Writer.WriteObject(value);
				}
			}
		}
	}

	public virtual void Read(T Basis, SerializationReader Reader)
	{
		FieldInfo fieldInfo = null;
		Type type = null;
		Type type2 = GetType();
		try
		{
			FieldInfo[] cachedFields = type2.GetCachedFields();
			foreach (FieldInfo fieldInfo2 in cachedFields)
			{
				if ((fieldInfo2.Attributes & (FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)) != 0)
				{
					continue;
				}
				fieldInfo = fieldInfo2;
				type = fieldInfo2.FieldType;
				if (FastSerialization.FieldSaveVersionInfo.TryGetValue(fieldInfo2, out var value) && value.minimumSaveVersion > Reader.FileVersion)
				{
					continue;
				}
				if (type == typeof(Cell))
				{
					fieldInfo2.SetValue(this, Reader.ReadCell());
				}
				else if (type == typeof(ActivatedAbilityEntry))
				{
					Guid guid = Reader.ReadGuid();
					if (guid != Guid.Empty && Basis is GameObject gameObject && gameObject.TryGetPart<ActivatedAbilities>(out var Part) && Part.AbilityByGuid.TryGetValue(guid, out var value2))
					{
						fieldInfo2.SetValue(this, value2);
					}
				}
				else if (type == typeof(GameObject))
				{
					fieldInfo2.SetValue(this, Reader.ReadGameObject());
				}
				else
				{
					fieldInfo2.SetValue(this, Reader.ReadObject());
				}
			}
		}
		catch (Exception ex)
		{
			if ((object)type2 != null)
			{
				ex.Data["Type"] = type2;
			}
			if ((object)fieldInfo != null)
			{
				ex.Data["Field"] = fieldInfo;
			}
			if ((object)type != null)
			{
				ex.Data["FieldType"] = type;
			}
			throw;
		}
	}

	/// <summary>Called if there was an error reading this component.</summary>
	/// <returns><c>true</c> if the error was handled; <c>false</c> to skip the rest of this component.</returns>
	public virtual bool ReadError(Exception Exception, SerializationReader Reader, long Start, int Length)
	{
		return false;
	}

	/// <summary>Called if there was at least one unhandled error reading this component's basis.</summary>
	public virtual void BasisError(T Basis, SerializationReader Reader)
	{
	}
}
