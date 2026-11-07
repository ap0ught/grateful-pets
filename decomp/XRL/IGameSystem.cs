using System;
using Qud.API;
using XRL.Core;
using XRL.Messages;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Conversations;

namespace XRL;

[Serializable]
public abstract class IGameSystem : IEventHandler, IComposite
{
	public class EventBinder : IEventBinder
	{
		public static readonly EventBinder Instance = new EventBinder();

		public override void WriteBind(SerializationWriter Writer, IEventHandler Handler, int ID)
		{
			Writer.WriteTokenized(Handler.GetType());
		}

		public override IEventHandler ReadBind(SerializationReader Reader, int ID)
		{
			return The.Game.GetSystem(Reader.ReadTokenizedType());
		}
	}

	[NonSerialized]
	public bool Removed;

	public IEventBinder Binder => EventBinder.Instance;

	public XRLGame Game => XRLCore.Core.Game;

	public GameObject Player => XRLCore.Core.Game.Player._Body;

	public virtual bool IsValid => !Removed;

	public virtual bool WantFieldReflection => true;

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

	public void RegisterEvent(int EventID, int Order = 0, bool Serialize = false)
	{
		Game.RegisterEvent(this, EventID, Order, Serialize);
	}

	public void UnregisterEvent(int EventID)
	{
		Game.UnregisterEvent(this, EventID);
	}

	/// <summary>Register to events from the <see cref="T:XRL.XRLGame" />.</summary>
	/// <param name="Game">The current game instance.</param>
	/// <param name="Registrar">An event registrar with this <see cref="T:XRL.IGameSystem" /> and <see cref="T:XRL.XRLGame" /> provisioned as defaults.</param>
	public virtual void Register(XRLGame Game, IEventRegistrar Registrar)
	{
	}

	public virtual void ApplyRegistrar(XRLGame Game)
	{
		Register(Game, EventRegistrar.Get(Game, this));
	}

	public virtual void ApplyUnregistrar(XRLGame Game)
	{
		Register(Game, EventUnregistrar.Get(Game, this));
	}

	public virtual void FireEvent(Event E)
	{
	}

	public virtual bool HandleEvent(MinEvent E)
	{
		return true;
	}

	public virtual void BeforeSave()
	{
	}

	public virtual void AfterSave()
	{
	}

	public virtual void AfterLoad(XRLGame game)
	{
	}

	public virtual void OnAdded()
	{
	}

	public virtual void OnRemoved()
	{
	}

	public virtual void Write(SerializationWriter Writer)
	{
		SaveGame(Writer);
	}

	public virtual void Read(SerializationReader Reader)
	{
		LoadGame(Reader);
	}

	public void AddPlayerMessage(string Message, string Color = null, bool Capitalize = true)
	{
		MessageQueue.AddPlayerMessage(Message, Color, Capitalize);
	}

	public void AddPlayerMessage(string Message, char Color, bool Capitalize = true)
	{
		MessageQueue.AddPlayerMessage(Message, Color, Capitalize);
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, GetAmbientLightEvent.ID)'.")]
	public virtual int GetDaylightRadius(int radius)
	{
		return radius;
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, EmbarkEvent.ID)', EventID == \"BootGame\".")]
	public virtual void PlayerEmbarking()
	{
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, EndSegmentEvent.ID)'.")]
	public virtual void BeginSegment()
	{
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, EndTurnEvent.ID)'.")]
	public virtual void EndTurn()
	{
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, QuestStartedEvent.ID)'.")]
	public virtual void QuestStarted(Quest Quest)
	{
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, QuestFinishedEvent.ID)'.")]
	public virtual void QuestCompleted(Quest Quest)
	{
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, SecretVisibilityChangedEvent.ID)'.")]
	public virtual void LocationDiscovered(string locationName)
	{
	}

	[Obsolete("Use 'The.Player.RegisterEvent(this, WaterRitualStartEvent.ID)'.")]
	public virtual void WaterRitualPerformed(GameObject go)
	{
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, AfterConversationEvent.ID)'.")]
	public virtual void PlayerAfterConversation(GameObject speaker, Conversation conversation)
	{
	}

	[Obsolete("Use 'The.Player.RegisterEvent(this, EnteredCellEvent.ID)'.")]
	public virtual void PlayerEnteredCell(Cell cell)
	{
	}

	[Obsolete("Use 'The.Player.RegisterEvent(this, EnteredCellEvent.ID)'.")]
	public virtual void PlayerEnteredWorldMapCell(ObjectEnteredCellEvent E)
	{
	}

	[Obsolete("Use 'The.Player.RegisterEvent(this, TookEvent.ID)'.")]
	public virtual void PlayerTook(GameObject go)
	{
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, SecretVisibilityChangedEvent.ID)'.")]
	public virtual void AfterSecretRevealed(IBaseJournalEntry secret)
	{
	}

	[Obsolete("Use 'The.Player.RegisterEvent(this, EquippedEvent.ID)'.")]
	public virtual void PlayerEquipped(GameObject go, BodyPart part)
	{
	}

	[Obsolete("Use 'The.Player.RegisterEvent(this, InventoryActionEvent.ID)'.")]
	public virtual bool PlayerActivating(GameObject go)
	{
		return true;
	}

	[Obsolete("Use 'The.Player.RegisterEvent(this, AddedToInventoryEvent.ID)'.")]
	public virtual void PlayerObjectAddedToInventory(GameObject go)
	{
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, AfterReputationChangeEvent.ID)'.")]
	public virtual void PlayerReputationChanged(string faction, int oldValue, int newValue, string type, string because)
	{
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, ZoneBuiltEvent.ID)'.")]
	public virtual void NewZoneGenerated(Zone zone)
	{
	}

	[Obsolete("Use 'The.Game.RegisterEvent(this, ZoneActivatedEvent.ID)'.")]
	public virtual void ZoneActivated(Zone zone)
	{
	}

	[Obsolete("Use 'The.Player.RegisterEvent(this, AwardXPEvent.ID)'.")]
	public virtual bool AwardingXP(ref GameObject Actor, ref int Amount, ref int Tier, ref int Minimum, ref int Maximum, ref GameObject Kill, ref GameObject InfluencedBy, ref GameObject PassedUpFrom, ref GameObject PassedDownFrom, ref string Deed)
	{
		return true;
	}

	[Obsolete("Use 'RegisterEvent(MinEvent.ID, Order: EventOrder.VERY_EARLY)'.")]
	public virtual int GetPriority()
	{
		return 0;
	}

	[Obsolete("Override IGameSystem.Write().")]
	public virtual void SaveGame(SerializationWriter Writer)
	{
	}

	[Obsolete("Override IGameSystem.Read().")]
	public virtual void LoadGame(SerializationReader Reader)
	{
	}
}
