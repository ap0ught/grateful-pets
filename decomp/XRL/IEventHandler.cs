using XRL.World;

namespace XRL;

public interface IEventHandler
{
	/// <inheritdoc cref="T:XRL.IEventBinder" />
	IEventBinder Binder => null;

	/// <summary>Check if the current event handler is still valid for receiving events.</summary>
	/// <remarks>
	/// Usually this is invalid when the event handler has been removed.
	/// E.g. when an <see cref="T:XRL.World.IPart" /> has been removed from its parent <see cref="T:XRL.World.GameObject" />.
	/// </remarks>
	bool IsValid => true;

	bool HandleEvent(AIAfterMissileEvent E)
	{
		return true;
	}

	bool HandleEvent(AIAfterThrowEvent E)
	{
		return true;
	}

	bool HandleEvent(AIBoredEvent E)
	{
		return true;
	}

	bool HandleEvent(AIGetDefensiveAbilityListEvent E)
	{
		return true;
	}

	bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		return true;
	}

	bool HandleEvent(AIGetMovementAbilityListEvent E)
	{
		return true;
	}

	bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		return true;
	}

	bool HandleEvent(AIGetOffensiveItemListEvent E)
	{
		return true;
	}

	bool HandleEvent(AIGetPassiveAbilityListEvent E)
	{
		return true;
	}

	bool HandleEvent(AIGetPassiveItemListEvent E)
	{
		return true;
	}

	bool HandleEvent(AIGetRetreatAbilityListEvent E)
	{
		return true;
	}

	bool HandleEvent(AIHelpBroadcastEvent E)
	{
		return true;
	}

	bool HandleEvent(AIWantUseWeaponEvent E)
	{
		return true;
	}

	bool HandleEvent(ActorGetNavigationWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(AddedToInventoryEvent E)
	{
		return true;
	}

	bool HandleEvent(AdjustTotalWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(AdjustValueEvent E)
	{
		return true;
	}

	bool HandleEvent(AdjustVisibilityRadiusEvent E)
	{
		return true;
	}

	bool HandleEvent(AdjustWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterAddActiveObjectEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterAddOpinionEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterAddSkillEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterAfterThrownEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterBasicBestowalEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterChangePartyLeaderEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterConsumeEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterContentsTakenEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterConversationEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterDieEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterDismemberEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterElementBestowalEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterExamineCriticalFailureEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterExamineFailureEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterGameLoadedEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterInventoryActionEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterLevelGainedEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterMentalAttackEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterMentalDefendEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterMoveFailedEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterObjectCreatedEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterPetEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterPilotChangeEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterPseudoRelicGeneratedEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterReadBookEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterRelicGeneratedEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterRemoveActiveObjectEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterRemoveSkillEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterReputationChangeEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterShieldBlockEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterThrownEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterTravelEvent E)
	{
		return true;
	}

	bool HandleEvent(AfterZoneBuiltEvent E)
	{
		return true;
	}

	bool HandleEvent(AllowInventoryStackEvent E)
	{
		return true;
	}

	bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		return true;
	}

	bool HandleEvent(AllowPolypPluckingEvent E)
	{
		return true;
	}

	bool HandleEvent(AllowTradeWithNoInventoryEvent E)
	{
		return true;
	}

	bool HandleEvent(AnimateEvent E)
	{
		return true;
	}

	bool HandleEvent(AnyAutoCollectDramsEvent E)
	{
		return true;
	}

	bool HandleEvent(AnyRegenerableLimbsEvent E)
	{
		return true;
	}

	bool HandleEvent(ApplyEffectEvent E)
	{
		return true;
	}

	bool HandleEvent(AttackerBeforeTemperatureChangeEvent E)
	{
		return true;
	}

	bool HandleEvent(AttackerDealingDamageEvent E)
	{
		return true;
	}

	bool HandleEvent(AttackerDealtDamageEvent E)
	{
		return true;
	}

	bool HandleEvent(AttemptConversationEvent E)
	{
		return true;
	}

	bool HandleEvent(AttemptToLandEvent E)
	{
		return true;
	}

	bool HandleEvent(AutoexploreObjectEvent E)
	{
		return true;
	}

	bool HandleEvent(AwardXPEvent E)
	{
		return true;
	}

	bool HandleEvent(AwardedXPEvent E)
	{
		return true;
	}

	bool HandleEvent(AwardingXPEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeAITakingActionEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeAddSkillEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeAfterThrownEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeApplyDamageEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeConsumeEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeConversationEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeDetonateEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeDieEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeDismemberEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeFireMissileWeaponsEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeInteriorCollapseEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeLevelGainedEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeMeleeAttackEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeMentalAttackEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeMentalDefendEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforePilotChangeEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforePlayMusicEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeProjectileHitEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforePullDownEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeRemoveSkillEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeRenderEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeSetFeelingEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeShieldBlockEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeSlamEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeTakeActionEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeTemperatureChangeEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeTookDamageEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeTravelDownEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeUnequippedEvent E)
	{
		return true;
	}

	bool HandleEvent(BeforeZoneBuiltEvent E)
	{
		return true;
	}

	bool HandleEvent(BeginBeingUnequippedEvent E)
	{
		return true;
	}

	bool HandleEvent(BeginConversationEvent E)
	{
		return true;
	}

	bool HandleEvent(BeginMentalAttackEvent E)
	{
		return true;
	}

	bool HandleEvent(BeginMentalDefendEvent E)
	{
		return true;
	}

	bool HandleEvent(BeginTakeActionEvent E)
	{
		return true;
	}

	bool HandleEvent(BeingConsumedEvent E)
	{
		return true;
	}

	bool HandleEvent(BlasphemyPerformedEvent E)
	{
		return true;
	}

	bool HandleEvent(BlocksRadarEvent E)
	{
		return true;
	}

	bool HandleEvent(BodyPositionChangedEvent E)
	{
		return true;
	}

	bool HandleEvent(BootSequenceAbortedEvent E)
	{
		return true;
	}

	bool HandleEvent(BootSequenceDoneEvent E)
	{
		return true;
	}

	bool HandleEvent(BootSequenceInitializedEvent E)
	{
		return true;
	}

	bool HandleEvent(CanAcceptObjectEvent E)
	{
		return true;
	}

	bool HandleEvent(CanApplyEffectEvent E)
	{
		return true;
	}

	bool HandleEvent(CanBeDismemberedEvent E)
	{
		return true;
	}

	bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		return true;
	}

	bool HandleEvent(CanBeMagneticallyManipulatedEvent E)
	{
		return true;
	}

	bool HandleEvent(CanBeModdedEvent E)
	{
		return true;
	}

	bool HandleEvent(CanBeNamedEvent E)
	{
		return true;
	}

	bool HandleEvent(CanBeReplicatedEvent E)
	{
		return true;
	}

	bool HandleEvent(CanBeSlottedEvent E)
	{
		return true;
	}

	bool HandleEvent(CanBeTradedEvent E)
	{
		return true;
	}

	bool HandleEvent(CanBeUnequippedEvent E)
	{
		return true;
	}

	bool HandleEvent(CanChangeMovementModeEvent E)
	{
		return true;
	}

	bool HandleEvent(CanDrinkEvent E)
	{
		return true;
	}

	bool HandleEvent(CanEnterInteriorEvent E)
	{
		return true;
	}

	bool HandleEvent(CanFallEvent E)
	{
		return true;
	}

	bool HandleEvent(CanFireAllMissileWeaponsEvent E)
	{
		return true;
	}

	bool HandleEvent(CanGiveDirectionsEvent E)
	{
		return true;
	}

	bool HandleEvent(CanHaveConversationEvent E)
	{
		return true;
	}

	bool HandleEvent(CanJoinPartyLeaderEvent E)
	{
		return true;
	}

	bool HandleEvent(CanMissilePassForcefieldEvent E)
	{
		return true;
	}

	bool HandleEvent(CanReceiveEmpathyEvent E)
	{
		return true;
	}

	bool HandleEvent(CanReceiveTelepathyEvent E)
	{
		return true;
	}

	bool HandleEvent(CanRefreshAbilityEvent E)
	{
		return true;
	}

	bool HandleEvent(CanSmartUseEvent E)
	{
		return true;
	}

	bool HandleEvent(CanStartConversationEvent E)
	{
		return true;
	}

	bool HandleEvent(CanTemperatureReturnToAmbientEvent E)
	{
		return true;
	}

	bool HandleEvent(CanTradeEvent E)
	{
		return true;
	}

	bool HandleEvent(CanTravelEvent E)
	{
		return true;
	}

	bool HandleEvent(CarryingCapacityChangedEvent E)
	{
		return true;
	}

	bool HandleEvent(CellChangedEvent E)
	{
		return true;
	}

	bool HandleEvent(CellDepletedEvent E)
	{
		return true;
	}

	bool HandleEvent(ChargeAvailableEvent E)
	{
		return true;
	}

	bool HandleEvent(ChargeUsedEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckAnythingToCleanEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckAnythingToCleanWithEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckAnythingToCleanWithNearbyEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckAttackableEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckExistenceSupportEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckGasCanAffectEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckLoadAmmoEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckOverburdenedOnStrengthUpdateEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckPaintabilityEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckReadyToFireEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckRealityDistortionAccessibilityEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckRealityDistortionAdvisabilityEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckRealityDistortionUsabilityEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckSpawnMergeEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckTileChangeEvent E)
	{
		return true;
	}

	bool HandleEvent(CheckUsesChargeWhileEquippedEvent E)
	{
		return true;
	}

	bool HandleEvent(CleanItemsEvent E)
	{
		return true;
	}

	bool HandleEvent(CollectBroadcastChargeEvent E)
	{
		return true;
	}

	bool HandleEvent(CommandEvent E)
	{
		return true;
	}

	bool HandleEvent(CommandReloadEvent E)
	{
		return true;
	}

	bool HandleEvent(CommandReplaceCellEvent E)
	{
		return true;
	}

	bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		return true;
	}

	bool HandleEvent(CommandSmartUseEvent E)
	{
		return true;
	}

	bool HandleEvent(CommandTakeActionEvent E)
	{
		return true;
	}

	bool HandleEvent(ConfigureMissileVisualEffectEvent E)
	{
		return true;
	}

	bool HandleEvent(ContainsAnyBlueprintEvent E)
	{
		return true;
	}

	bool HandleEvent(ContainsBlueprintEvent E)
	{
		return true;
	}

	bool HandleEvent(ContainsEvent E)
	{
		return true;
	}

	bool HandleEvent(DamageConstantAdjustedEvent E)
	{
		return true;
	}

	bool HandleEvent(DamageDieSizeAdjustedEvent E)
	{
		return true;
	}

	bool HandleEvent(DeathEvent E)
	{
		return true;
	}

	bool HandleEvent(DecorateDefaultEquipmentEvent E)
	{
		return true;
	}

	bool HandleEvent(DefendMeleeHitEvent E)
	{
		return true;
	}

	bool HandleEvent(DefenderMissileHitEvent E)
	{
		return true;
	}

	bool HandleEvent(DerivationCreatedEvent E)
	{
		return true;
	}

	bool HandleEvent(DidInitialEquipEvent E)
	{
		return true;
	}

	bool HandleEvent(DidReequipEvent E)
	{
		return true;
	}

	bool HandleEvent(DropOnDeathEvent E)
	{
		return true;
	}

	bool HandleEvent(DroppedEvent E)
	{
		return true;
	}

	bool HandleEvent(EarlyBeforeBeginTakeActionEvent E)
	{
		return true;
	}

	bool HandleEvent(EarlyBeforeDeathRemovalEvent E)
	{
		return true;
	}

	bool HandleEvent(EffectAppliedEvent E)
	{
		return true;
	}

	bool HandleEvent(EffectForceAppliedEvent E)
	{
		return true;
	}

	bool HandleEvent(EffectRemovedEvent E)
	{
		return true;
	}

	bool HandleEvent(EmbarkEvent E)
	{
		return true;
	}

	bool HandleEvent(EncounterChanceEvent E)
	{
		return true;
	}

	bool HandleEvent(EncumbranceChangedEvent E)
	{
		return true;
	}

	bool HandleEvent(EndActionEvent E)
	{
		return true;
	}

	bool HandleEvent(EndTurnEvent E)
	{
		return true;
	}

	bool HandleEvent(EnterCellEvent E)
	{
		return true;
	}

	bool HandleEvent(EnteredCellEvent E)
	{
		return true;
	}

	bool HandleEvent(EnteringCellEvent E)
	{
		return true;
	}

	bool HandleEvent(EnteringZoneEvent E)
	{
		return true;
	}

	bool HandleEvent(EnvironmentalUpdateEvent E)
	{
		return true;
	}

	bool HandleEvent(EquippedEvent E)
	{
		return true;
	}

	bool HandleEvent(EquipperEquippedEvent E)
	{
		return true;
	}

	bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		return true;
	}

	bool HandleEvent(ExamineFailureEvent E)
	{
		return true;
	}

	bool HandleEvent(ExamineSuccessEvent E)
	{
		return true;
	}

	bool HandleEvent(ExtraHostilePerceptionEvent E)
	{
		return true;
	}

	bool HandleEvent(ExtremitiesMovedEvent E)
	{
		return true;
	}

	bool HandleEvent(FellDownEvent E)
	{
		return true;
	}

	bool HandleEvent(FindObjectByIdEvent E)
	{
		return true;
	}

	bool HandleEvent(FinishChargeAvailableEvent E)
	{
		return true;
	}

	bool HandleEvent(FinishRechargeAvailableEvent E)
	{
		return true;
	}

	bool HandleEvent(FlushWeightCacheEvent E)
	{
		return true;
	}

	bool HandleEvent(ForceApplyEffectEvent E)
	{
		return true;
	}

	bool HandleEvent(FrozeEvent E)
	{
		return true;
	}

	bool HandleEvent(GaveDirectionsEvent E)
	{
		return true;
	}

	bool HandleEvent(GeneralAmnestyEvent E)
	{
		return true;
	}

	bool HandleEvent(GenericCommandEvent E)
	{
		return true;
	}

	bool HandleEvent(GenericDeepNotifyEvent E)
	{
		return true;
	}

	bool HandleEvent(GenericDeepQueryEvent E)
	{
		return true;
	}

	bool HandleEvent(GenericDeepRatingEvent E)
	{
		return true;
	}

	bool HandleEvent(GenericNotifyEvent E)
	{
		return true;
	}

	bool HandleEvent(GenericQueryEvent E)
	{
		return true;
	}

	bool HandleEvent(GenericRatingEvent E)
	{
		return true;
	}

	bool HandleEvent(GetActivationPhaseEvent E)
	{
		return true;
	}

	bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetAmbientLightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetAmmoCountAvailableEvent E)
	{
		return true;
	}

	bool HandleEvent(GetApparentSpeciesEvent E)
	{
		return true;
	}

	bool HandleEvent(GetAttackerHitDiceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetAttackerMeleePenetrationEvent E)
	{
		return true;
	}

	bool HandleEvent(GetAutoCollectDramsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetAutoEquipPriorityEvent E)
	{
		return true;
	}

	bool HandleEvent(GetAvailableComputePowerEvent E)
	{
		return true;
	}

	bool HandleEvent(GetBandagePerformanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetBleedLiquidEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCarriedWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCleaningItemsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCleaningItemsNearbyEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCleaveAmountEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCleaveMaxPenaltyEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCompanionLimitEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCompanionStatusEvent E)
	{
		return true;
	}

	bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetContentsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetContextEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCookingActionsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCooldownEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCriticalThresholdEvent E)
	{
		return true;
	}

	bool HandleEvent(GetCyberneticsBehaviorDescriptionEvent E)
	{
		return true;
	}

	bool HandleEvent(GetDebugInternalsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetDefenderHitDiceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetDefenderMeleePenetrationEvent E)
	{
		return true;
	}

	bool HandleEvent(GetDifficultyEvaluationEvent E)
	{
		return true;
	}

	bool HandleEvent(GetDiseaseOnsetEvent E)
	{
		return true;
	}

	bool HandleEvent(GetDisplayNameEvent E)
	{
		return true;
	}

	bool HandleEvent(GetDisplayNamePenetrationColorEvent E)
	{
		return true;
	}

	bool HandleEvent(GetDisplayStatBonusEvent E)
	{
		return true;
	}

	bool HandleEvent(GetElectricalConductivityEvent E)
	{
		return true;
	}

	bool HandleEvent(GetEnergyCostEvent E)
	{
		return true;
	}

	bool HandleEvent(GetExtraPhysicalFeaturesEvent E)
	{
		return true;
	}

	bool HandleEvent(GetExtrinsicValueEvent E)
	{
		return true;
	}

	bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetFactionRankEvent E)
	{
		return true;
	}

	bool HandleEvent(GetFeelingEvent E)
	{
		return true;
	}

	bool HandleEvent(GetFirefightingPerformanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetFixedMissileSpreadEvent E)
	{
		return true;
	}

	bool HandleEvent(GetFreeDramsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetGameObjectSortEvent E)
	{
		return true;
	}

	bool HandleEvent(GetGenderEvent E)
	{
		return true;
	}

	bool HandleEvent(GetHostileWalkRadiusEvent E)
	{
		return true;
	}

	bool HandleEvent(GetHostilityRecognitionLimitsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetIdealTemperatureEvent E)
	{
		return true;
	}

	bool HandleEvent(GetIntrinsicValueEvent E)
	{
		return true;
	}

	bool HandleEvent(GetIntrinsicWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		return true;
	}

	bool HandleEvent(GetInventoryActionsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetInventoryCategoryEvent E)
	{
		return true;
	}

	bool HandleEvent(GetItemElementsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetJumpingBehaviorEvent E)
	{
		return true;
	}

	bool HandleEvent(GetKineticResistanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetLevelUpDiceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetLevelUpPointsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetLevelUpSkillPointsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetLostChanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMatterPhaseEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMeleeAttacksEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMissileCoverPercentageEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMissileStatusColorEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMissileWeaponPerformanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMissileWeaponProjectileEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMissileWeaponStatusEvent E)
	{
		return true;
	}

	bool HandleEvent(GetModRarityWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		return true;
	}

	bool HandleEvent(GetMutationTermEvent E)
	{
		return true;
	}

	bool HandleEvent(GetNamingBestowalChanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetNamingChanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetNavigationWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetNotReadyToFireMessageEvent E)
	{
		return true;
	}

	bool HandleEvent(GetOverloadChargeEvent E)
	{
		return true;
	}

	bool HandleEvent(GetPartyLeaderFollowDistanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetPointsOfInterestEvent E)
	{
		return true;
	}

	bool HandleEvent(GetPowerLoadLevelEvent E)
	{
		return true;
	}

	bool HandleEvent(GetPrecognitionRestoreGameStateEvent E)
	{
		return true;
	}

	bool HandleEvent(GetPreferredLiquidEvent E)
	{
		return true;
	}

	bool HandleEvent(GetProjectileBlueprintEvent E)
	{
		return true;
	}

	bool HandleEvent(GetProjectileObjectEvent E)
	{
		return true;
	}

	bool HandleEvent(GetPropertyModDescription E)
	{
		return true;
	}

	bool HandleEvent(GetPsionicSifrahSetupEvent E)
	{
		return true;
	}

	bool HandleEvent(GetPsychicGlimmerEvent E)
	{
		return true;
	}

	bool HandleEvent(GetRandomBuyChimericBodyPartRollsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetRandomBuyMutationCountEvent E)
	{
		return true;
	}

	bool HandleEvent(GetRealityStabilizationPenetrationEvent E)
	{
		return true;
	}

	bool HandleEvent(GetRebukeLevelEvent E)
	{
		return true;
	}

	bool HandleEvent(GetRecoilersEvent E)
	{
		return true;
	}

	bool HandleEvent(GetReplaceCellInteractionsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetRespiratoryAgentPerformanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetRitualSifrahSetupEvent E)
	{
		return true;
	}

	bool HandleEvent(GetRunningBehaviorEvent E)
	{
		return true;
	}

	bool HandleEvent(GetScanTypeEvent E)
	{
		return true;
	}

	bool HandleEvent(GetShieldBlockPreferenceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetShieldSlamDamageEvent E)
	{
		return true;
	}

	bool HandleEvent(GetShortDescriptionEvent E)
	{
		return true;
	}

	bool HandleEvent(GetSkillEffectChanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetSlotsRequiredEvent E)
	{
		return true;
	}

	bool HandleEvent(GetSlottedInventoryActionsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetSocialSifrahSetupEvent E)
	{
		return true;
	}

	bool HandleEvent(GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetSpaceTimeAnomalyEmergencePermillageChanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetSpecialEffectChanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetSpringinessEvent E)
	{
		return true;
	}

	bool HandleEvent(GetSprintDurationEvent E)
	{
		return true;
	}

	bool HandleEvent(GetStorableDramsEvent E)
	{
		return true;
	}

	bool HandleEvent(GetSwimmingPerformanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetThrowProfileEvent E)
	{
		return true;
	}

	bool HandleEvent(GetThrownWeaponFlexPhaseProviderEvent E)
	{
		return true;
	}

	bool HandleEvent(GetThrownWeaponPerformanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetThrownWeaponRangeEvent E)
	{
		return true;
	}

	bool HandleEvent(GetTinkeringBonusEvent E)
	{
		return true;
	}

	bool HandleEvent(GetToHitModifierEvent E)
	{
		return true;
	}

	bool HandleEvent(GetTonicCapacityEvent E)
	{
		return true;
	}

	bool HandleEvent(GetTonicDosageEvent E)
	{
		return true;
	}

	bool HandleEvent(GetTonicDurationEvent E)
	{
		return true;
	}

	bool HandleEvent(GetTradePerformanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetTransitiveLocationEvent E)
	{
		return true;
	}

	bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		return true;
	}

	bool HandleEvent(GetUtilityScoreEvent E)
	{
		return true;
	}

	bool HandleEvent(GetWadingPerformanceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetWaterRitualCostEvent E)
	{
		return true;
	}

	bool HandleEvent(GetWaterRitualLiquidEvent E)
	{
		return true;
	}

	bool HandleEvent(GetWaterRitualReputationAmountEvent E)
	{
		return true;
	}

	bool HandleEvent(GetWaterRitualSecretWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(GetWaterRitualSellSecretBehaviorEvent E)
	{
		return true;
	}

	bool HandleEvent(GetWeaponHitDiceEvent E)
	{
		return true;
	}

	bool HandleEvent(GetWeaponMeleeAttacksEvent E)
	{
		return true;
	}

	bool HandleEvent(GetWeaponMeleePenetrationEvent E)
	{
		return true;
	}

	bool HandleEvent(GetZoneEvent E)
	{
		return true;
	}

	bool HandleEvent(GetZoneFreezabilityEvent E)
	{
		return true;
	}

	bool HandleEvent(GetZoneSuspendabilityEvent E)
	{
		return true;
	}

	bool HandleEvent(GiveDramsEvent E)
	{
		return true;
	}

	bool HandleEvent(GlimmerChangeEvent E)
	{
		return true;
	}

	bool HandleEvent(GravitationEvent E)
	{
		return true;
	}

	bool HandleEvent(HasBeenReadEvent E)
	{
		return true;
	}

	bool HandleEvent(HasBlueprintEvent E)
	{
		return true;
	}

	bool HandleEvent(HasFlammableEquipmentEvent E)
	{
		return true;
	}

	bool HandleEvent(HasFlammableEquipmentOrInventoryEvent E)
	{
		return true;
	}

	bool HandleEvent(HealsNaturallyEvent E)
	{
		return true;
	}

	bool HandleEvent(IAICommandListEvent E)
	{
		return true;
	}

	bool HandleEvent(IAIItemCommandListEvent E)
	{
		return true;
	}

	bool HandleEvent(IAIMoveCommandListEvent E)
	{
		return true;
	}

	bool HandleEvent(IActOnItemEvent E)
	{
		return true;
	}

	bool HandleEvent(IActualEffectCheckEvent E)
	{
		return true;
	}

	bool HandleEvent(IAddSkillEvent E)
	{
		return true;
	}

	bool HandleEvent(IAdjacentNavigationWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(IBootSequenceEvent E)
	{
		return true;
	}

	bool HandleEvent(IChargeConsumptionEvent E)
	{
		return true;
	}

	bool HandleEvent(IChargeEvent E)
	{
		return true;
	}

	bool HandleEvent(IChargeProductionEvent E)
	{
		return true;
	}

	bool HandleEvent(IChargeStorageEvent E)
	{
		return true;
	}

	bool HandleEvent(IConsumeEvent E)
	{
		return true;
	}

	bool HandleEvent(IConversationMinEvent E)
	{
		return true;
	}

	bool HandleEvent(IDamageEvent E)
	{
		return true;
	}

	bool HandleEvent(IDeathEvent E)
	{
		return true;
	}

	bool HandleEvent(IDerivationEvent E)
	{
		return true;
	}

	bool HandleEvent(IDestroyObjectEvent E)
	{
		return true;
	}

	bool HandleEvent(IEffectCheckEvent E)
	{
		return true;
	}

	bool HandleEvent(IExamineEvent E)
	{
		return true;
	}

	bool HandleEvent(IFinalChargeProductionEvent E)
	{
		return true;
	}

	bool HandleEvent(IHitDiceEvent E)
	{
		return true;
	}

	bool HandleEvent(IInitialChargeProductionEvent E)
	{
		return true;
	}

	bool HandleEvent(IInventoryActionsEvent E)
	{
		return true;
	}

	bool HandleEvent(ILimbRegenerationEvent E)
	{
		return true;
	}

	bool HandleEvent(ILiquidEvent E)
	{
		return true;
	}

	bool HandleEvent(IMeleeAttackEvent E)
	{
		return true;
	}

	bool HandleEvent(IMeleePenetrationEvent E)
	{
		return true;
	}

	bool HandleEvent(IMentalAttackEvent E)
	{
		return true;
	}

	bool HandleEvent(INavigationWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(IObjectCellInteractionEvent E)
	{
		return true;
	}

	bool HandleEvent(IObjectCreationEvent E)
	{
		return true;
	}

	bool HandleEvent(IObjectInventoryInteractionEvent E)
	{
		return true;
	}

	bool HandleEvent(IQuestEvent E)
	{
		return true;
	}

	bool HandleEvent(IRemoveFromContextEvent E)
	{
		return true;
	}

	bool HandleEvent(IReplicationEvent E)
	{
		return true;
	}

	bool HandleEvent(ISaveEvent E)
	{
		return true;
	}

	bool HandleEvent(IShallowZoneEvent E)
	{
		return true;
	}

	bool HandleEvent(IShortDescriptionEvent E)
	{
		return true;
	}

	bool HandleEvent(ITravelEvent E)
	{
		return true;
	}

	bool HandleEvent(IValueEvent E)
	{
		return true;
	}

	bool HandleEvent(IWeightEvent E)
	{
		return true;
	}

	bool HandleEvent(IXPEvent E)
	{
		return true;
	}

	bool HandleEvent(IZoneEvent E)
	{
		return true;
	}

	bool HandleEvent(IZoneOnlyEvent E)
	{
		return true;
	}

	bool HandleEvent(IdleQueryEvent E)
	{
		return true;
	}

	bool HandleEvent(ImplantAddedEvent E)
	{
		return true;
	}

	bool HandleEvent(ImplantRemovedEvent E)
	{
		return true;
	}

	bool HandleEvent(ImplantedEvent E)
	{
		return true;
	}

	bool HandleEvent(InduceVomitingEvent E)
	{
		return true;
	}

	bool HandleEvent(InductionChargeEvent E)
	{
		return true;
	}

	bool HandleEvent(InteriorZoneBuiltEvent E)
	{
		return true;
	}

	bool HandleEvent(InterruptAutowalkEvent E)
	{
		return true;
	}

	bool HandleEvent(InventoryActionEvent E)
	{
		return true;
	}

	bool HandleEvent(IsAdaptivePenetrationActiveEvent E)
	{
		return true;
	}

	bool HandleEvent(IsAfflictionEvent E)
	{
		return true;
	}

	bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		return true;
	}

	bool HandleEvent(IsExplosiveEvent E)
	{
		return true;
	}

	bool HandleEvent(IsMutantEvent E)
	{
		return true;
	}

	bool HandleEvent(IsOverloadableEvent E)
	{
		return true;
	}

	bool HandleEvent(IsRepairableEvent E)
	{
		return true;
	}

	bool HandleEvent(IsRootedInPlaceEvent E)
	{
		return true;
	}

	bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		return true;
	}

	bool HandleEvent(IsTrueKinEvent E)
	{
		return true;
	}

	bool HandleEvent(IsVehicleOperationalEvent E)
	{
		return true;
	}

	bool HandleEvent(JoinPartyLeaderPossibleEvent E)
	{
		return true;
	}

	bool HandleEvent(JoinedPartyLeaderEvent E)
	{
		return true;
	}

	bool HandleEvent(JumpedEvent E)
	{
		return true;
	}

	bool HandleEvent(KilledEvent E)
	{
		return true;
	}

	bool HandleEvent(KilledPlayerEvent E)
	{
		return true;
	}

	bool HandleEvent(LateBeforeApplyDamageEvent E)
	{
		return true;
	}

	bool HandleEvent(LeaveCellEvent E)
	{
		return true;
	}

	bool HandleEvent(LeavingCellEvent E)
	{
		return true;
	}

	bool HandleEvent(LeftCellEvent E)
	{
		return true;
	}

	bool HandleEvent(LiquidMixedEvent E)
	{
		return true;
	}

	bool HandleEvent(LoadAmmoEvent E)
	{
		return true;
	}

	bool HandleEvent(MakeTemporaryEvent E)
	{
		return true;
	}

	bool HandleEvent(MentalAttackEvent E)
	{
		return true;
	}

	bool HandleEvent(MissilePenetrateEvent E)
	{
		return true;
	}

	bool HandleEvent(MissileTraversingCellEvent E)
	{
		return true;
	}

	bool HandleEvent(ModificationAppliedEvent E)
	{
		return true;
	}

	bool HandleEvent(ModifyAttackingSaveEvent E)
	{
		return true;
	}

	bool HandleEvent(ModifyBitCostEvent E)
	{
		return true;
	}

	bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		return true;
	}

	bool HandleEvent(ModifyOriginatingSaveEvent E)
	{
		return true;
	}

	bool HandleEvent(MovementModeChangedEvent E)
	{
		return true;
	}

	bool HandleEvent(MutationsSubjectToEMPEvent E)
	{
		return true;
	}

	bool HandleEvent(NeedPartSupportEvent E)
	{
		return true;
	}

	bool HandleEvent(NeedsReloadEvent E)
	{
		return true;
	}

	bool HandleEvent(NeutronFluxPourExplodesEvent E)
	{
		return true;
	}

	bool HandleEvent(NotifyTargetImmuneEvent E)
	{
		return true;
	}

	bool HandleEvent(ObjectCreatedEvent E)
	{
		return true;
	}

	bool HandleEvent(ObjectEnteredCellEvent E)
	{
		return true;
	}

	bool HandleEvent(ObjectEnteringCellEvent E)
	{
		return true;
	}

	bool HandleEvent(ObjectGoingProneEvent E)
	{
		return true;
	}

	bool HandleEvent(ObjectLeavingCellEvent E)
	{
		return true;
	}

	bool HandleEvent(ObjectStartedFlyingEvent E)
	{
		return true;
	}

	bool HandleEvent(ObjectStoppedFlyingEvent E)
	{
		return true;
	}

	bool HandleEvent(OkayToDamageEvent E)
	{
		return true;
	}

	bool HandleEvent(OnDeathRemovalEvent E)
	{
		return true;
	}

	bool HandleEvent(OnDestroyObjectEvent E)
	{
		return true;
	}

	bool HandleEvent(OnQuestAddedEvent E)
	{
		return true;
	}

	bool HandleEvent(OwnerAfterInventoryActionEvent E)
	{
		return true;
	}

	bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		return true;
	}

	bool HandleEvent(OwnerGetShortDescriptionEvent E)
	{
		return true;
	}

	bool HandleEvent(OwnerGetUnknownShortDescriptionEvent E)
	{
		return true;
	}

	bool HandleEvent(PartAddedEvent E)
	{
		return true;
	}

	bool HandleEvent(PartRemovedEvent E)
	{
		return true;
	}

	bool HandleEvent(PartSupportEvent E)
	{
		return true;
	}

	bool HandleEvent(PathAsBurrowerEvent E)
	{
		return true;
	}

	bool HandleEvent(PhysicalContactEvent E)
	{
		return true;
	}

	bool HandleEvent(PointDefenseInterceptEvent E)
	{
		return true;
	}

	bool HandleEvent(PollForHealingLocationEvent E)
	{
		return true;
	}

	bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		return true;
	}

	bool HandleEvent(PowerUpdatedEvent E)
	{
		return true;
	}

	bool HandleEvent(PreferDefaultBehaviorEvent E)
	{
		return true;
	}

	bool HandleEvent(PreferTargetEvent E)
	{
		return true;
	}

	bool HandleEvent(PreventSmartUseEvent E)
	{
		return true;
	}

	bool HandleEvent(PrimePowerSystemsEvent E)
	{
		return true;
	}

	bool HandleEvent(ProducesLiquidEvent E)
	{
		return true;
	}

	bool HandleEvent(ProjectileMovingEvent E)
	{
		return true;
	}

	bool HandleEvent(QueryBroadcastDrawEvent E)
	{
		return true;
	}

	bool HandleEvent(QueryChargeEvent E)
	{
		return true;
	}

	bool HandleEvent(QueryChargeProductionEvent E)
	{
		return true;
	}

	bool HandleEvent(QueryChargeStorageEvent E)
	{
		return true;
	}

	bool HandleEvent(QueryDrawEvent E)
	{
		return true;
	}

	bool HandleEvent(QueryEquippableListEvent E)
	{
		return true;
	}

	bool HandleEvent(QueryInductionChargeStorageEvent E)
	{
		return true;
	}

	bool HandleEvent(QueryRechargeStorageEvent E)
	{
		return true;
	}

	bool HandleEvent(QuerySlotListEvent E)
	{
		return true;
	}

	bool HandleEvent(QuestFinishedEvent E)
	{
		return true;
	}

	bool HandleEvent(QuestStartedEvent E)
	{
		return true;
	}

	bool HandleEvent(QuestStepFinishedEvent E)
	{
		return true;
	}

	bool HandleEvent(RadiatesHeatAdjacentEvent E)
	{
		return true;
	}

	bool HandleEvent(RadiatesHeatEvent E)
	{
		return true;
	}

	bool HandleEvent(RealityStabilizeEvent E)
	{
		return true;
	}

	bool HandleEvent(RechargeAvailableEvent E)
	{
		return true;
	}

	bool HandleEvent(RefreshTileEvent E)
	{
		return true;
	}

	bool HandleEvent(RegenerateDefaultEquipmentEvent E)
	{
		return true;
	}

	bool HandleEvent(RegenerateLimbEvent E)
	{
		return true;
	}

	bool HandleEvent(RemoveFromContextEvent E)
	{
		return true;
	}

	bool HandleEvent(RepaintedEvent E)
	{
		return true;
	}

	bool HandleEvent(RepairCriticalFailureEvent E)
	{
		return true;
	}

	bool HandleEvent(RepairedEvent E)
	{
		return true;
	}

	bool HandleEvent(ReplaceInContextEvent E)
	{
		return true;
	}

	bool HandleEvent(ReplaceThrownWeaponEvent E)
	{
		return true;
	}

	bool HandleEvent(ReplicaCreatedEvent E)
	{
		return true;
	}

	bool HandleEvent(ReputationChangeEvent E)
	{
		return true;
	}

	bool HandleEvent(RespiresEvent E)
	{
		return true;
	}

	bool HandleEvent(SecretVisibilityChangedEvent E)
	{
		return true;
	}

	bool HandleEvent(ShieldSlamPerformedEvent E)
	{
		return true;
	}

	bool HandleEvent(ShotCompleteEvent E)
	{
		return true;
	}

	bool HandleEvent(ShouldAttackToReachTargetEvent E)
	{
		return true;
	}

	bool HandleEvent(ShouldDescribeStatBonusEvent E)
	{
		return true;
	}

	bool HandleEvent(SlamEvent E)
	{
		return true;
	}

	bool HandleEvent(StackCountChangedEvent E)
	{
		return true;
	}

	bool HandleEvent(StartTradeEvent E)
	{
		return true;
	}

	bool HandleEvent(StatChangeEvent E)
	{
		return true;
	}

	bool HandleEvent(StockedEvent E)
	{
		return true;
	}

	bool HandleEvent(StripContentsEvent E)
	{
		return true;
	}

	bool HandleEvent(SubjectToGravityEvent E)
	{
		return true;
	}

	bool HandleEvent(SuspendingEvent E)
	{
		return true;
	}

	bool HandleEvent(SyncMutationLevelsEvent E)
	{
		return true;
	}

	bool HandleEvent(SyncRenderEvent E)
	{
		return true;
	}

	bool HandleEvent(SynchronizeExistenceEvent E)
	{
		return true;
	}

	bool HandleEvent(TakeOnRoleEvent E)
	{
		return true;
	}

	bool HandleEvent(TakenEvent E)
	{
		return true;
	}

	bool HandleEvent(TestChargeEvent E)
	{
		return true;
	}

	bool HandleEvent(ThawedEvent E)
	{
		return true;
	}

	bool HandleEvent(TookDamageEvent E)
	{
		return true;
	}

	bool HandleEvent(TookEnvironmentalDamageEvent E)
	{
		return true;
	}

	bool HandleEvent(TookEvent E)
	{
		return true;
	}

	bool HandleEvent(TransparentToEMPEvent E)
	{
		return true;
	}

	bool HandleEvent(TravelSpeedEvent E)
	{
		return true;
	}

	bool HandleEvent(TriggersMakersMarkCreationEvent E)
	{
		return true;
	}

	bool HandleEvent(TryRemoveFromContextEvent E)
	{
		return true;
	}

	bool HandleEvent(UnequippedEvent E)
	{
		return true;
	}

	bool HandleEvent(UnimplantedEvent E)
	{
		return true;
	}

	bool HandleEvent(UseChargeEvent E)
	{
		return true;
	}

	bool HandleEvent(UseDramsEvent E)
	{
		return true;
	}

	bool HandleEvent(UseEnergyEvent E)
	{
		return true;
	}

	bool HandleEvent(UseHealingLocationEvent E)
	{
		return true;
	}

	bool HandleEvent(UsingChargeEvent E)
	{
		return true;
	}

	bool HandleEvent(VaporizedEvent E)
	{
		return true;
	}

	bool HandleEvent(WantsLiquidCollectionEvent E)
	{
		return true;
	}

	bool HandleEvent(WasDerivedFromEvent E)
	{
		return true;
	}

	bool HandleEvent(WasReplicatedEvent E)
	{
		return true;
	}

	bool HandleEvent(WaterRitualStartEvent E)
	{
		return true;
	}

	bool HandleEvent(WorshipPerformedEvent E)
	{
		return true;
	}

	bool HandleEvent(ZoneActivatedEvent E)
	{
		return true;
	}

	bool HandleEvent(ZoneBuiltEvent E)
	{
		return true;
	}

	bool HandleEvent(ZoneDeactivatedEvent E)
	{
		return true;
	}

	bool HandleEvent(ZoneThawedEvent E)
	{
		return true;
	}

	bool WantEvent(int ID, int Cascade)
	{
		return false;
	}

	bool HandleEvent(MinEvent E)
	{
		return true;
	}

	void TurnTick(long TimeTick, int Amount)
	{
	}
}
