using XRL.World;

namespace XRL;

public interface IEventSource : IEventHandler
{
	bool IEventHandler.HandleEvent(AIAfterMissileEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIAfterThrowEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIBoredEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIGetDefensiveAbilityListEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIGetDefensiveItemListEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIGetMovementAbilityListEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIGetOffensiveItemListEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIGetPassiveAbilityListEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIGetPassiveItemListEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIGetRetreatAbilityListEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIHelpBroadcastEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AIWantUseWeaponEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ActorGetNavigationWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AddedToInventoryEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AdjustTotalWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AdjustValueEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AdjustVisibilityRadiusEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AdjustWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterAddActiveObjectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterAddOpinionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterAddSkillEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterAfterThrownEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterBasicBestowalEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterChangePartyLeaderEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterConsumeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterContentsTakenEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterConversationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterDieEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterDismemberEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterElementBestowalEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterExamineCriticalFailureEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterExamineFailureEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterGameLoadedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterInventoryActionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterLevelGainedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterMentalAttackEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterMentalDefendEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterMoveFailedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterObjectCreatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterPetEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterPilotChangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterPseudoRelicGeneratedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterReadBookEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterRelicGeneratedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterRemoveActiveObjectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterRemoveSkillEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterReputationChangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterShieldBlockEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterThrownEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterTravelEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AfterZoneBuiltEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AllowInventoryStackEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AllowLiquidCollectionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AllowPolypPluckingEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AllowTradeWithNoInventoryEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AnimateEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AnyAutoCollectDramsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AnyRegenerableLimbsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ApplyEffectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AttackerBeforeTemperatureChangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AttackerDealingDamageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AttackerDealtDamageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AttemptConversationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AttemptToLandEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AutoexploreObjectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AwardXPEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AwardedXPEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(AwardingXPEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeAITakingActionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeAddSkillEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeAfterThrownEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeApplyDamageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeBeginTakeActionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeConsumeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeConversationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeDeathRemovalEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeDestroyObjectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeDetonateEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeDieEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeDismemberEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeFireMissileWeaponsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeInteriorCollapseEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeLevelGainedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeMeleeAttackEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeMentalAttackEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeMentalDefendEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeObjectCreatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforePilotChangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforePlayMusicEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeProjectileHitEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforePullDownEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeRemoveSkillEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeRenderEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeSetFeelingEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeShieldBlockEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeSlamEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeTakeActionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeTemperatureChangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeTookDamageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeTravelDownEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeUnequippedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeforeZoneBuiltEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeginBeingUnequippedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeginConversationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeginMentalAttackEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeginMentalDefendEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeginTakeActionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BeingConsumedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BlasphemyPerformedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BlocksRadarEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BodyPositionChangedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BootSequenceAbortedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BootSequenceDoneEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(BootSequenceInitializedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanAcceptObjectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanApplyEffectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanBeDismemberedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanBeMagneticallyManipulatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanBeModdedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanBeNamedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanBeReplicatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanBeSlottedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanBeTradedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanBeUnequippedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanChangeMovementModeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanDrinkEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanEnterInteriorEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanFallEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanFireAllMissileWeaponsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanGiveDirectionsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanHaveConversationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanJoinPartyLeaderEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanMissilePassForcefieldEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanReceiveEmpathyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanReceiveTelepathyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanRefreshAbilityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanSmartUseEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanStartConversationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanTemperatureReturnToAmbientEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanTradeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CanTravelEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CarryingCapacityChangedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CellChangedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CellDepletedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ChargeAvailableEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ChargeUsedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckAnythingToCleanEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckAnythingToCleanWithEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckAnythingToCleanWithNearbyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckAttackableEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckExistenceSupportEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckGasCanAffectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckLoadAmmoEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckOverburdenedOnStrengthUpdateEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckPaintabilityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckReadyToFireEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckRealityDistortionAccessibilityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckRealityDistortionAdvisabilityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckRealityDistortionUsabilityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckSpawnMergeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckTileChangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CheckUsesChargeWhileEquippedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CleanItemsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CollectBroadcastChargeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CommandEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CommandReloadEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CommandReplaceCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CommandSmartUseEarlyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CommandSmartUseEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(CommandTakeActionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ConfigureMissileVisualEffectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ContainsAnyBlueprintEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ContainsBlueprintEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ContainsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DamageConstantAdjustedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DamageDieSizeAdjustedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DeathEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DecorateDefaultEquipmentEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DefendMeleeHitEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DefenderMissileHitEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DerivationCreatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DidInitialEquipEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DidReequipEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DropOnDeathEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(DroppedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EarlyBeforeBeginTakeActionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EarlyBeforeDeathRemovalEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EffectAppliedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EffectForceAppliedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EffectRemovedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EmbarkEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EncounterChanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EncumbranceChangedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EndActionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EndTurnEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EnterCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EnteredCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EnteringCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EnteringZoneEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EnvironmentalUpdateEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EquippedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(EquipperEquippedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ExamineCriticalFailureEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ExamineFailureEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ExamineSuccessEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ExtraHostilePerceptionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ExtremitiesMovedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(FellDownEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(FindObjectByIdEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(FinishChargeAvailableEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(FinishRechargeAvailableEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(FlushWeightCacheEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ForceApplyEffectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(FrozeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GaveDirectionsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GeneralAmnestyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GenericCommandEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GenericDeepNotifyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GenericDeepQueryEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GenericDeepRatingEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GenericNotifyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GenericQueryEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GenericRatingEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetActivationPhaseEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetAmbientLightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetAmmoCountAvailableEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetApparentSpeciesEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetAttackerHitDiceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetAttackerMeleePenetrationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetAutoCollectDramsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetAutoEquipPriorityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetAvailableComputePowerEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetBandagePerformanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetBleedLiquidEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCarriedWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCleaningItemsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCleaningItemsNearbyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCleaveAmountEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCleaveMaxPenaltyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCompanionLimitEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCompanionStatusEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetComponentNavigationWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetContentsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetContextEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCookingActionsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCooldownEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCriticalThresholdEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetCyberneticsBehaviorDescriptionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetDebugInternalsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetDefenderHitDiceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetDefenderMeleePenetrationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetDifficultyEvaluationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetDiseaseOnsetEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetDisplayNameEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetDisplayNamePenetrationColorEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetDisplayStatBonusEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetElectricalConductivityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetEnergyCostEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetExtraPhysicalFeaturesEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetExtrinsicValueEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetExtrinsicWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetFactionRankEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetFeelingEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetFirefightingPerformanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetFixedMissileSpreadEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetFreeDramsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetGameObjectSortEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetGenderEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetHostileWalkRadiusEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetHostilityRecognitionLimitsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetIdealTemperatureEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetIntrinsicValueEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetIntrinsicWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetInventoryActionsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetInventoryCategoryEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetItemElementsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetJumpingBehaviorEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetKineticResistanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetLevelUpDiceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetLevelUpPointsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetLevelUpSkillPointsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetLostChanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMatterPhaseEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMaxCarriedWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMeleeAttackChanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMeleeAttacksEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMissileCoverPercentageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMissileStatusColorEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMissileWeaponPerformanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMissileWeaponProjectileEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMissileWeaponStatusEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetModRarityWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMovementCapabilitiesEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetMutationTermEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetNamingBestowalChanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetNamingChanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetNavigationWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetNotReadyToFireMessageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetOverloadChargeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetPartyLeaderFollowDistanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetPointsOfInterestEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetPowerLoadLevelEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetPrecognitionRestoreGameStateEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetPreferredLiquidEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetProjectileBlueprintEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetProjectileObjectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetPropertyModDescription E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetPsionicSifrahSetupEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetPsychicGlimmerEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetRandomBuyChimericBodyPartRollsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetRandomBuyMutationCountEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetRealityStabilizationPenetrationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetRebukeLevelEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetRecoilersEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetReplaceCellInteractionsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetRespiratoryAgentPerformanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetRitualSifrahSetupEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetRunningBehaviorEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetScanTypeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetShieldBlockPreferenceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetShieldSlamDamageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetShortDescriptionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetSkillEffectChanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetSlotsRequiredEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetSlottedInventoryActionsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetSocialSifrahSetupEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetSpaceTimeAnomalyEmergencePermillageChanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetSpecialEffectChanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetSpringinessEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetSprintDurationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetStorableDramsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetSwimmingPerformanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetThrowProfileEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetThrownWeaponFlexPhaseProviderEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetThrownWeaponPerformanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetThrownWeaponRangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetTinkeringBonusEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetToHitModifierEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetTonicCapacityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetTonicDosageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetTonicDurationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetTradePerformanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetTransitiveLocationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetUtilityScoreEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetWadingPerformanceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetWaterRitualCostEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetWaterRitualLiquidEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetWaterRitualReputationAmountEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetWaterRitualSecretWeightEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetWaterRitualSellSecretBehaviorEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetWeaponHitDiceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetWeaponMeleeAttacksEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetWeaponMeleePenetrationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetZoneEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetZoneFreezabilityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GetZoneSuspendabilityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GiveDramsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GlimmerChangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(GravitationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(HasBeenReadEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(HasBlueprintEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(HasFlammableEquipmentEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(HasFlammableEquipmentOrInventoryEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(HealsNaturallyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IAICommandListEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IAIItemCommandListEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IAIMoveCommandListEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IActOnItemEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IActualEffectCheckEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IAddSkillEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IAdjacentNavigationWeightEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IBootSequenceEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IChargeConsumptionEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IChargeEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IChargeProductionEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IChargeStorageEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IConsumeEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IConversationMinEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IDamageEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IDeathEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IDerivationEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IDestroyObjectEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IEffectCheckEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IExamineEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IFinalChargeProductionEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IHitDiceEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IInitialChargeProductionEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IInventoryActionsEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(ILimbRegenerationEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(ILiquidEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IMeleeAttackEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IMeleePenetrationEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IMentalAttackEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(INavigationWeightEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IObjectCellInteractionEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IObjectCreationEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IObjectInventoryInteractionEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IQuestEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IRemoveFromContextEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IReplicationEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(ISaveEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IShallowZoneEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IShortDescriptionEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(ITravelEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IValueEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IWeightEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IXPEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IZoneEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IZoneOnlyEvent E)
	{
		return true;
	}

	bool IEventHandler.HandleEvent(IdleQueryEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ImplantAddedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ImplantRemovedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ImplantedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(InduceVomitingEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(InductionChargeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(InteriorZoneBuiltEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(InterruptAutowalkEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(InventoryActionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsAdaptivePenetrationActiveEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsAfflictionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsConversationallyResponsiveEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsExplosiveEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsMutantEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsOverloadableEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsRepairableEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsRootedInPlaceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsSensableAsPsychicEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsTrueKinEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(IsVehicleOperationalEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(JoinPartyLeaderPossibleEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(JoinedPartyLeaderEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(JumpedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(KilledEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(KilledPlayerEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(LateBeforeApplyDamageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(LeaveCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(LeavingCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(LeftCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(LiquidMixedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(LoadAmmoEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(MakeTemporaryEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(MentalAttackEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(MissilePenetrateEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(MissileTraversingCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ModificationAppliedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ModifyAttackingSaveEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ModifyBitCostEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ModifyDefendingSaveEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ModifyOriginatingSaveEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(MovementModeChangedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(MutationsSubjectToEMPEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(NeedPartSupportEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(NeedsReloadEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(NeutronFluxPourExplodesEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(NotifyTargetImmuneEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ObjectCreatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ObjectEnteredCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ObjectEnteringCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ObjectGoingProneEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ObjectLeavingCellEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ObjectStartedFlyingEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ObjectStoppedFlyingEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(OkayToDamageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(OnDeathRemovalEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(OnDestroyObjectEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(OwnerAfterInventoryActionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(OwnerGetShortDescriptionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(OwnerGetUnknownShortDescriptionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PartAddedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PartRemovedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PartSupportEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PathAsBurrowerEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PhysicalContactEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PointDefenseInterceptEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PollForHealingLocationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PowerSwitchFlippedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PowerUpdatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PreferDefaultBehaviorEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PreferTargetEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PreventSmartUseEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(PrimePowerSystemsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ProducesLiquidEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ProjectileMovingEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QueryBroadcastDrawEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QueryChargeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QueryChargeProductionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QueryChargeStorageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QueryDrawEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QueryEquippableListEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QueryInductionChargeStorageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QueryRechargeStorageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QuerySlotListEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QuestFinishedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QuestStartedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(QuestStepFinishedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RadiatesHeatAdjacentEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RadiatesHeatEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RealityStabilizeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RechargeAvailableEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RefreshTileEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RegenerateDefaultEquipmentEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RegenerateLimbEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RemoveFromContextEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RepaintedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RepairCriticalFailureEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RepairedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ReplaceInContextEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ReplaceThrownWeaponEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ReplicaCreatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ReputationChangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(RespiresEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(SecretVisibilityChangedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ShieldSlamPerformedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ShotCompleteEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ShouldAttackToReachTargetEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ShouldDescribeStatBonusEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(SlamEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(StackCountChangedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(StartTradeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(StatChangeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(StockedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(StripContentsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(SubjectToGravityEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(SuspendingEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(SyncMutationLevelsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(SyncRenderEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(SynchronizeExistenceEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(TakeOnRoleEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(TakenEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(TestChargeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ThawedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(TookDamageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(TookEnvironmentalDamageEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(TookEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(TransparentToEMPEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(TravelSpeedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(TriggersMakersMarkCreationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(TryRemoveFromContextEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(UnequippedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(UnimplantedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(UseChargeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(UseDramsEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(UseEnergyEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(UseHealingLocationEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(UsingChargeEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(VaporizedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(WantsLiquidCollectionEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(WasDerivedFromEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(WasReplicatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(WaterRitualStartEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(WorshipPerformedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ZoneActivatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ZoneBuiltEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ZoneDeactivatedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	bool IEventHandler.HandleEvent(ZoneThawedEvent E)
	{
		return HandleEvent((MinEvent)E);
	}

	/// <inheritdoc cref="M:XRL.Collections.EventRegistry.Register(XRL.IEventHandler,System.Int32,System.Int32,System.Boolean)" />
	void RegisterEvent(IEventHandler Handler, int EventID, int Order = 0, bool Serialize = false)
	{
	}

	/// <inheritdoc cref="M:XRL.Collections.EventRegistry.Unregister(XRL.IEventHandler,System.Int32)" />
	void UnregisterEvent(IEventHandler Handler, int EventID)
	{
	}
}
