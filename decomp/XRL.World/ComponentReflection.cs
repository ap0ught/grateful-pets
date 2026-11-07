using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Genkit;
using XRL.Collections;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using XRL.World.Quests;
using XRL.World.Text;

namespace XRL.World;

public static class ComponentReflection
{
	public delegate void Add(GameObject Object, IComponent<GameObject> Component, bool Registration, bool Creation);

	public delegate void FinalizeRead(GameObject Object, IComponent<GameObject> Component);

	public class ExtensionRecord
	{
		public Add Add;

		public FinalizeRead FinalizeRead;

		public ExtensionRecord(Add Add, FinalizeRead FinalizeRead)
		{
			this.Add = Add;
			this.FinalizeRead = FinalizeRead;
		}
	}

	public static readonly Dictionary<Type, int[]> PartEvents = new Dictionary<Type, int[]>();

	public static readonly Dictionary<Type, int[]> EffectEvents = new Dictionary<Type, int[]>();

	public static readonly Dictionary<Type, ExtensionRecord> PartExtensions = new Dictionary<Type, ExtensionRecord>();

	public static readonly Dictionary<Type, ExtensionRecord> EffectExtensions = new Dictionary<Type, ExtensionRecord>();

	public static ulong ExtensionHash;

	private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static Type[] AddSignature = new Type[4]
	{
		typeof(GameObject),
		typeof(IComponent<GameObject>),
		typeof(bool),
		typeof(bool)
	};

	private static Type[] FinalizeReadSignature = new Type[2]
	{
		typeof(GameObject),
		typeof(IComponent<GameObject>)
	};

	private static Type[] TargetSignature = new Type[1];

	private static MethodInfo AddPartInternals = typeof(GameObject).GetMethod("AddPartInternals", Flags, null, new Type[4]
	{
		typeof(IPart),
		typeof(bool),
		typeof(bool),
		typeof(bool)
	}, null);

	private static MethodInfo HasPart = typeof(GameObject).GetMethod("HasPart", Flags, null, new Type[1] { typeof(Type) }, null);

	private static MethodInfo ApplyEffect = typeof(GameObject).GetMethod("ApplyEffect", new Type[2]
	{
		typeof(Effect),
		typeof(GameObject)
	});

	private static MethodInfo HasEffect = typeof(GameObject).GetMethod("HasEffect", Flags, null, new Type[1] { typeof(Type) }, null);

	private static MethodInfo GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public);

	private static FieldInfo EffectID = typeof(Effect).GetField("ID", Flags);

	public static void Initialize()
	{
		if (ModManager.HasActiveScripts())
		{
			Evaluate(ModManager.ActiveTypes, out var _, out var _);
		}
		else
		{
			Thaw();
		}
	}

	private static void Thaw()
	{
		PartEvents.EnsureCapacity(289);
		EffectEvents.EnsureCapacity(100);
		int[] value = new int[2] { 737650316, -912057913 };
		int[] value2 = new int[2] { -912057913, -1345588527 };
		int[] value3 = new int[1] { 737650316 };
		int[] value4 = new int[1] { -912057913 };
		int[] value5 = new int[2] { 737650316, 1583436105 };
		int[] value6 = new int[2] { 737650316, -1345588527 };
		int[] value7 = new int[1] { -1345588527 };
		int[] value8 = new int[1] { 1583436105 };
		PartEvents[typeof(IPartWithPrefabImposter)] = value;
		PartEvents[typeof(AscensionRenderer)] = value2;
		PartEvents[typeof(GraftekGraft)] = value3;
		PartEvents[typeof(Brain)] = value3;
		PartEvents[typeof(LiquidVolume)] = value6;
		PartEvents[typeof(Physics)] = value7;
		PartEvents[typeof(TerrainNotes)] = value;
		PartEvents[typeof(BaetylHostility)] = value7;
		PartEvents[typeof(RandomAltarBaetyl)] = value3;
		PartEvents[typeof(Body)] = value7;
		PartEvents[typeof(Tattoos)] = value3;
		PartEvents[typeof(AIPassenger)] = value7;
		PartEvents[typeof(AIUrnDuster)] = value7;
		PartEvents[typeof(AIWorldMapTravel)] = value7;
		PartEvents[typeof(Distraction)] = value7;
		PartEvents[typeof(Breeder)] = value7;
		PartEvents[typeof(ConfuseOnSight)] = value7;
		PartEvents[typeof(CrossFlameOnStep)] = value7;
		PartEvents[typeof(CrypticParticleText)] = value7;
		PartEvents[typeof(LavaSludge)] = value7;
		PartEvents[typeof(MamonPart)] = value3;
		PartEvents[typeof(Slumberling)] = value7;
		PartEvents[typeof(SoupSludge)] = value3;
		PartEvents[typeof(Spawner)] = value7;
		PartEvents[typeof(SpawnVessel)] = value7;
		PartEvents[typeof(TrollKing)] = value7;
		PartEvents[typeof(DischargeOnStep)] = value7;
		PartEvents[typeof(DromadCaravan)] = value3;
		PartEvents[typeof(ElementalJelly)] = value3;
		PartEvents[typeof(Engulfing)] = value3;
		PartEvents[typeof(ExtradimensionalHunterSummoner)] = value7;
		PartEvents[typeof(FabricateFromSelf)] = value7;
		PartEvents[typeof(FallsApart)] = value7;
		PartEvents[typeof(FireSuppressionSystem)] = value7;
		PartEvents[typeof(FugueOnStep)] = value7;
		PartEvents[typeof(GenericInventoryRestocker)] = value7;
		PartEvents[typeof(LiquidProximityFrenzies)] = value7;
		PartEvents[typeof(LiquidProximityTriggersEffect)] = value7;
		PartEvents[typeof(Mimic)] = value3;
		PartEvents[typeof(NephalVFX)] = value4;
		PartEvents[typeof(Nest)] = value7;
		PartEvents[typeof(Pounder)] = value7;
		PartEvents[typeof(RunOver)] = value3;
		PartEvents[typeof(SetStateOnSeen)] = value3;
		PartEvents[typeof(SlowDangerousMovement)] = value4;
		PartEvents[typeof(TemperatureVenting)] = value7;
		PartEvents[typeof(WallColor)] = value3;
		PartEvents[typeof(CyberneticsAnomalyFumigator)] = value7;
		PartEvents[typeof(CyberneticsAutomatedInternalDefibrillator)] = value7;
		PartEvents[typeof(CyberneticsElectromagneticSensor)] = value7;
		PartEvents[typeof(CyberneticsFireSuppressionSystem)] = value7;
		PartEvents[typeof(CyberneticsHasRandomImplants)] = value3;
		PartEvents[typeof(CyberneticsMedassistModule)] = value7;
		PartEvents[typeof(CyberneticsPenetratingRadar)] = value7;
		PartEvents[typeof(PaintTest)] = value3;
		PartEvents[typeof(MessageWhenFirstSeen)] = value3;
		PartEvents[typeof(SkrefCorpseLoot)] = value3;
		PartEvents[typeof(Interior)] = value7;
		PartEvents[typeof(InteriorPortal)] = value7;
		PartEvents[typeof(XRL.World.Parts.MotedAir)] = value3;
		PartEvents[typeof(ItemConvertor)] = value7;
		PartEvents[typeof(LiquidPump)] = value7;
		PartEvents[typeof(RepairBay)] = value7;
		PartEvents[typeof(TeleportGate)] = value7;
		PartEvents[typeof(Gas)] = value6;
		PartEvents[typeof(GasAsh)] = value7;
		PartEvents[typeof(GasConfusion)] = value7;
		PartEvents[typeof(GasDamaging)] = value7;
		PartEvents[typeof(GasDisease)] = value7;
		PartEvents[typeof(GasFungalSpores)] = value7;
		PartEvents[typeof(GasPlasma)] = value7;
		PartEvents[typeof(GasPoison)] = value7;
		PartEvents[typeof(GasShame)] = value7;
		PartEvents[typeof(GasSleep)] = value7;
		PartEvents[typeof(HindrenMysteryExile)] = value7;
		PartEvents[typeof(Inventory)] = value7;
		PartEvents[typeof(ActiveLightSource)] = value7;
		PartEvents[typeof(ActiveStatPercent)] = value7;
		PartEvents[typeof(AddsRep)] = value7;
		PartEvents[typeof(AdjustSpecialEffectChances)] = value7;
		PartEvents[typeof(AilingQuickness)] = value7;
		PartEvents[typeof(ArtifactDetection)] = value7;
		PartEvents[typeof(AttributeMultiplier)] = value7;
		PartEvents[typeof(BootSequence)] = value7;
		PartEvents[typeof(ComputeNode)] = value7;
		PartEvents[typeof(Cursed)] = value7;
		PartEvents[typeof(DamageContents)] = value7;
		PartEvents[typeof(DismemberAdjacentHostiles)] = value7;
		PartEvents[typeof(Displacer)] = value7;
		PartEvents[typeof(EquipIntProperties)] = value7;
		PartEvents[typeof(EquipStatBoost)] = value7;
		PartEvents[typeof(FlareCompensation)] = value7;
		PartEvents[typeof(FoliageCamouflage)] = value7;
		PartEvents[typeof(FungalFortitude)] = value7;
		PartEvents[typeof(GasTumbler)] = value7;
		PartEvents[typeof(Gaslight)] = value7;
		PartEvents[typeof(GlimmerAlteration)] = value7;
		PartEvents[typeof(Hangable)] = value3;
		PartEvents[typeof(HologramProjector)] = value7;
		PartEvents[typeof(IntPropertyChanger)] = value7;
		PartEvents[typeof(InventoryItemConvertor)] = value7;
		PartEvents[typeof(KindrishProperties)] = value7;
		PartEvents[typeof(LeakWhenBroken)] = value7;
		PartEvents[typeof(LiquidProducer)] = value7;
		PartEvents[typeof(LowStatBooster)] = value7;
		PartEvents[typeof(MechanicalWings)] = value7;
		PartEvents[typeof(MentalScreen)] = value7;
		PartEvents[typeof(MoteProperties)] = value3;
		PartEvents[typeof(MultiIntPropertyChanger)] = value7;
		PartEvents[typeof(MultiNavigationBonus)] = value7;
		PartEvents[typeof(NavigationBonus)] = value7;
		PartEvents[typeof(NeutronFluxContainment)] = value7;
		PartEvents[typeof(PartsGas)] = value7;
		PartEvents[typeof(PointDefense)] = value7;
		PartEvents[typeof(PoweredFloating)] = value7;
		PartEvents[typeof(PsychicMeridian)] = value7;
		PartEvents[typeof(RealityStabilization)] = value7;
		PartEvents[typeof(ReduceCooldowns)] = value7;
		PartEvents[typeof(ReduceEnergyCosts)] = value7;
		PartEvents[typeof(ReshephProjectionWidget)] = value7;
		PartEvents[typeof(RestoreOnDeath)] = value7;
		PartEvents[typeof(RocketSkates)] = value7;
		PartEvents[typeof(SaveModifier)] = value7;
		PartEvents[typeof(SaveModifiers)] = value7;
		PartEvents[typeof(SlaveMask)] = value7;
		PartEvents[typeof(SlipRing)] = value7;
		PartEvents[typeof(Suspensor)] = value7;
		PartEvents[typeof(TemperatureAdjuster)] = value7;
		PartEvents[typeof(TemperatureController)] = value7;
		PartEvents[typeof(ThermalAmp)] = value7;
		PartEvents[typeof(TorchProperties)] = value3;
		PartEvents[typeof(UrbanCamouflage)] = value7;
		PartEvents[typeof(Waldopack)] = value7;
		PartEvents[typeof(DrawInTheDark)] = value3;
		PartEvents[typeof(LeaksFluid)] = value7;
		PartEvents[typeof(AIShootRound)] = value7;
		PartEvents[typeof(MagazineAmmoLoader)] = value7;
		PartEvents[typeof(ExplodeAfterTurns)] = value4;
		PartEvents[typeof(ModCoProcessor)] = value7;
		PartEvents[typeof(ModDesecrated)] = value3;
		PartEvents[typeof(ModMagnetized)] = value7;
		PartEvents[typeof(ModCamo)] = value3;
		PartEvents[typeof(ModMetallized)] = value3;
		PartEvents[typeof(ModNanochelated)] = value3;
		PartEvents[typeof(ModUrbanCamo)] = value3;
		PartEvents[typeof(DisperseEMP)] = value3;
		PartEvents[typeof(Explores)] = value7;
		PartEvents[typeof(PetGloaming)] = value3;
		PartEvents[typeof(LightDimmer)] = value7;
		PartEvents[typeof(MechaPlayer)] = value3;
		PartEvents[typeof(PetEitherOr)] = value3;
		PartEvents[typeof(AmbientCollector)] = value7;
		PartEvents[typeof(AmbientPowerReceiver)] = value7;
		PartEvents[typeof(BiomechanicalPowerTransmission)] = value6;
		PartEvents[typeof(BroadcastPowerReceiver)] = value7;
		PartEvents[typeof(ChargeSink)] = value7;
		PartEvents[typeof(ElectricalPowerTransmission)] = value6;
		PartEvents[typeof(EnergyCellChargeLevel)] = value7;
		PartEvents[typeof(EnergyCellSocket)] = value7;
		PartEvents[typeof(EquipCharge)] = value7;
		PartEvents[typeof(FusionReactor)] = value7;
		PartEvents[typeof(GenericPowerTransmission)] = value6;
		PartEvents[typeof(HydraulicPowerTransmission)] = value6;
		PartEvents[typeof(HydroTurbine)] = value7;
		PartEvents[typeof(LiquidFueledPowerPlant)] = value7;
		PartEvents[typeof(MechanicalPowerTransmission)] = value6;
		PartEvents[typeof(RequiresPowerToEquip)] = value7;
		PartEvents[typeof(SolarArray)] = value7;
		PartEvents[typeof(WindTurbine)] = value7;
		PartEvents[typeof(ZeroPointEnergyCollector)] = value7;
		PartEvents[typeof(PrefabImposter)] = value3;
		PartEvents[typeof(PrefabImposterFinal)] = value4;
		PartEvents[typeof(ExistenceSupport)] = value7;
		PartEvents[typeof(XRL.World.Parts.Temporary)] = value7;
		PartEvents[typeof(AIBarathrumShuttle)] = value7;
		PartEvents[typeof(QuestGiverImposter)] = value;
		PartEvents[typeof(QuestStarter)] = value3;
		PartEvents[typeof(QuestStepFinisher)] = value3;
		PartEvents[typeof(Reconstitution)] = value7;
		PartEvents[typeof(RespondToEvent)] = value7;
		PartEvents[typeof(SecretObject)] = value3;
		PartEvents[typeof(Tinkering_Mine)] = value7;
		PartEvents[typeof(SoundRender)] = value3;
		PartEvents[typeof(AlternateOverlayRender)] = value8;
		PartEvents[typeof(CryoZone)] = value;
		PartEvents[typeof(FactoryArm)] = value3;
		PartEvents[typeof(Fan)] = value6;
		PartEvents[typeof(GrabberArm)] = value3;
		PartEvents[typeof(PistonPressController)] = value3;
		PartEvents[typeof(PistonPressElement)] = value3;
		PartEvents[typeof(LocationFinder)] = value3;
		PartEvents[typeof(PhaseSticky)] = value3;
		PartEvents[typeof(PluckablePolyp)] = value7;
		PartEvents[typeof(PyroZone)] = value;
		PartEvents[typeof(SpaceTimeRift)] = value3;
		PartEvents[typeof(SpaceTimeVortex)] = value3;
		PartEvents[typeof(StairHighlight)] = value4;
		PartEvents[typeof(WorldTeleporter)] = value7;
		PartEvents[typeof(VillageSurface)] = value7;
		PartEvents[typeof(Walltrap)] = value7;
		PartEvents[typeof(Wormhole)] = value3;
		PartEvents[typeof(ThinWorld)] = value4;
		PartEvents[typeof(AIVehiclePilot)] = value7;
		PartEvents[typeof(ShevaStarshipControl)] = value7;
		PartEvents[typeof(Vehicle)] = value7;
		PartEvents[typeof(VehiclePairBonus)] = value7;
		PartEvents[typeof(AnimatedMaterialElectric)] = value3;
		PartEvents[typeof(AnimatedMaterialExtradimensional)] = value3;
		PartEvents[typeof(AnimatedMaterialFire)] = value3;
		PartEvents[typeof(AnimatedMaterialForcefield)] = value;
		PartEvents[typeof(AnimatedMaterialForcefieldColors)] = value3;
		PartEvents[typeof(AnimatedMaterialGeneric)] = value6;
		PartEvents[typeof(AnimatedMaterialGenericAlternate)] = value6;
		PartEvents[typeof(AnimatedMaterialGenericSecondAlternate)] = value6;
		PartEvents[typeof(AnimatedMaterialGreenFire)] = value3;
		PartEvents[typeof(AnimatedMaterialHighlyEntropic)] = value3;
		PartEvents[typeof(AnimatedMaterialLuminous)] = value3;
		PartEvents[typeof(AnimatedMaterialMagentaFire)] = value3;
		PartEvents[typeof(AnimatedMaterialMainframeTapeDrive)] = value3;
		PartEvents[typeof(AnimatedMaterialRealityStabilizationField)] = value;
		PartEvents[typeof(AnimatedMaterialSaltDunes)] = value3;
		PartEvents[typeof(AnimatedMaterialStasisfield)] = value;
		PartEvents[typeof(AnimatedMaterialTechlight)] = value3;
		PartEvents[typeof(AnimatedWallGeneric)] = value3;
		PartEvents[typeof(AnimatedMaterialWater)] = value3;
		PartEvents[typeof(AnimatedMaterialOverlandWater)] = value3;
		PartEvents[typeof(AnimatedMaterialWhiteFire)] = value3;
		PartEvents[typeof(ApothecaryIconColor)] = value3;
		PartEvents[typeof(BackdropWhenVisible)] = value4;
		PartEvents[typeof(ConcealedHologramMaterial)] = value3;
		PartEvents[typeof(FirethumbedIconColor)] = value3;
		PartEvents[typeof(FungiFriendIconColor)] = value3;
		PartEvents[typeof(FungusRiddenIconColor)] = value3;
		PartEvents[typeof(HeroIconColor)] = value3;
		PartEvents[typeof(HologramMaterial)] = value3;
		PartEvents[typeof(HologramMaterialPrimary)] = value3;
		PartEvents[typeof(KindlethumbedIconColor)] = value3;
		PartEvents[typeof(MerchantIconColor)] = value3;
		PartEvents[typeof(MossyUnityMaterial)] = value;
		PartEvents[typeof(MossyUnityMaterial2)] = value3;
		PartEvents[typeof(NephilimCultistIconColor)] = value3;
		PartEvents[typeof(QudzuSymbioteIconColor)] = value3;
		PartEvents[typeof(RenderLiquidBackground)] = value3;
		PartEvents[typeof(SlimespitterIconColor)] = value3;
		PartEvents[typeof(SlimewalkerIconColor)] = value3;
		PartEvents[typeof(ThinWorldMaterial)] = value3;
		PartEvents[typeof(TinkerIconColor)] = value3;
		PartEvents[typeof(UnityPrefabImposter)] = value;
		PartEvents[typeof(WantsBackdrop)] = value4;
		PartEvents[typeof(NoBackdropBleedthrough)] = value4;
		PartEvents[typeof(WardenIconColor)] = value3;
		PartEvents[typeof(AlarmCircle)] = value3;
		PartEvents[typeof(GritGatePowerDisplay)] = value3;
		PartEvents[typeof(IfThenElseQuestWidget)] = value7;
		PartEvents[typeof(GolemQuestMound)] = value7;
		PartEvents[typeof(ConveyorPad)] = value3;
		PartEvents[typeof(Smash_Floor)] = value3;
		PartEvents[typeof(Endurance_ShakeItOff)] = value7;
		PartEvents[typeof(Discipline_Lionheart)] = value7;
		PartEvents[typeof(SpontaneousCombustion)] = value7;
		PartEvents[typeof(WeakHeart)] = value7;
		PartEvents[typeof(MentalMirror)] = value7;
		PartEvents[typeof(SensePsychic)] = value7;
		PartEvents[typeof(TimeDilation)] = value3;
		PartEvents[typeof(CrungleGaze)] = value;
		PartEvents[typeof(Decarbonizer)] = value;
		PartEvents[typeof(Infiltrate)] = value;
		PartEvents[typeof(Interdiction)] = value3;
		PartEvents[typeof(IrisdualBeam)] = value4;
		PartEvents[typeof(LiquidFont)] = value7;
		PartEvents[typeof(MagneticPulse)] = value3;
		PartEvents[typeof(MultiHorns)] = value3;
		PartEvents[typeof(QuantumFugue)] = value7;
		PartEvents[typeof(StickyTongue)] = value3;
		PartEvents[typeof(StoneGaze)] = value;
		PartEvents[typeof(AdrenalControl2)] = value3;
		PartEvents[typeof(ElectricalGeneration)] = value7;
		PartEvents[typeof(SleepGasGeneration)] = value3;
		PartEvents[typeof(CorrosiveGasGeneration)] = value3;
		PartEvents[typeof(PlasmaGeneration)] = value3;
		PartEvents[typeof(PoisonGasGeneration)] = value3;
		PartEvents[typeof(DefoliantGeneration)] = value3;
		PartEvents[typeof(FungicideGeneration)] = value3;
		PartEvents[typeof(ConfusionGasGeneration)] = value3;
		PartEvents[typeof(NormalityGasGeneration)] = value3;
		PartEvents[typeof(GlitterDustGeneration)] = value3;
		PartEvents[typeof(GasGeneration)] = value3;
		PartEvents[typeof(PhotosyntheticSkin)] = value3;
		PartEvents[typeof(ReflectShame)] = value7;
		EffectEvents[typeof(Adjusted)] = value3;
		EffectEvents[typeof(ArtifactDetectionEffect)] = value4;
		EffectEvents[typeof(AshPoison)] = value3;
		EffectEvents[typeof(Asleep)] = value3;
		EffectEvents[typeof(AxonsInflated)] = value3;
		EffectEvents[typeof(AxonsDeflated)] = value3;
		EffectEvents[typeof(BasiliskPoison)] = value3;
		EffectEvents[typeof(Berserk)] = value3;
		EffectEvents[typeof(Blaze_Tonic)] = value3;
		EffectEvents[typeof(Bleeding)] = value3;
		EffectEvents[typeof(Burning)] = value3;
		EffectEvents[typeof(CardiacArrest)] = value3;
		EffectEvents[typeof(XRL.World.Effects.Confused)] = value3;
		EffectEvents[typeof(CorpseTethered)] = value3;
		EffectEvents[typeof(Cripple)] = value3;
		EffectEvents[typeof(Dashing)] = value3;
		EffectEvents[typeof(Dazed)] = value3;
		EffectEvents[typeof(Monochrome)] = value3;
		EffectEvents[typeof(Disguised)] = value5;
		EffectEvents[typeof(Ecstatic)] = value3;
		EffectEvents[typeof(ElectromagneticPulsed)] = value3;
		EffectEvents[typeof(EmptyTheClips)] = value3;
		EffectEvents[typeof(Engulfed)] = value3;
		EffectEvents[typeof(EpistemicDisguise)] = value5;
		EffectEvents[typeof(Exhausted)] = value3;
		EffectEvents[typeof(Flagging)] = value3;
		EffectEvents[typeof(Flying)] = value3;
		EffectEvents[typeof(FoliageCamouflaged)] = value3;
		EffectEvents[typeof(Freezing)] = value3;
		EffectEvents[typeof(Frenzied)] = value3;
		EffectEvents[typeof(Frozen)] = value3;
		EffectEvents[typeof(FuriouslyConfused)] = value3;
		EffectEvents[typeof(GeometricHeal)] = value3;
		EffectEvents[typeof(Healing)] = value3;
		EffectEvents[typeof(HeightenedHearingEffect)] = value4;
		EffectEvents[typeof(HeightenedSmellEffect)] = value4;
		EffectEvents[typeof(Hoarshroom_Tonic)] = value3;
		EffectEvents[typeof(HolographicBleeding)] = value3;
		EffectEvents[typeof(Hooked)] = value3;
		EffectEvents[typeof(HulkHoney_Tonic)] = value3;
		EffectEvents[typeof(Immobilized)] = value3;
		EffectEvents[typeof(Interdicted)] = value3;
		EffectEvents[typeof(LatchedOnto)] = value3;
		EffectEvents[typeof(XRL.World.Effects.LifeDrain)] = value3;
		EffectEvents[typeof(LiquidCovered)] = value3;
		EffectEvents[typeof(LiquidStained)] = value3;
		EffectEvents[typeof(LongbladeEffect_EnGarde)] = value3;
		EffectEvents[typeof(LoveTonic)] = value3;
		EffectEvents[typeof(Lovesick)] = value3;
		EffectEvents[typeof(Meditating)] = value3;
		EffectEvents[typeof(NocturnalApexed)] = value3;
		EffectEvents[typeof(Nosebleed)] = value3;
		EffectEvents[typeof(Nullphased)] = value3;
		EffectEvents[typeof(Omniphase)] = value3;
		EffectEvents[typeof(Paralyzed)] = value3;
		EffectEvents[typeof(PhasePoisoned)] = value3;
		EffectEvents[typeof(Phased)] = value3;
		EffectEvents[typeof(PoisonGasPoison)] = value3;
		EffectEvents[typeof(Poisoned)] = value3;
		EffectEvents[typeof(Prone)] = value3;
		EffectEvents[typeof(RealityStabilized)] = value;
		EffectEvents[typeof(Refresh)] = value3;
		EffectEvents[typeof(RifleMark)] = value3;
		EffectEvents[typeof(Rubbergum_Tonic)] = value3;
		EffectEvents[typeof(Running)] = value3;
		EffectEvents[typeof(Rusted)] = value3;
		EffectEvents[typeof(Salve_Tonic)] = value3;
		EffectEvents[typeof(Scintillating)] = value3;
		EffectEvents[typeof(SensePsychicEffect)] = value4;
		EffectEvents[typeof(SenseRobotEffect)] = value4;
		EffectEvents[typeof(ShadeOil_Tonic)] = value3;
		EffectEvents[typeof(Shamed)] = value3;
		EffectEvents[typeof(ShatterArmor)] = value3;
		EffectEvents[typeof(ShatterMentalArmor)] = value3;
		EffectEvents[typeof(ShieldWall)] = value3;
		EffectEvents[typeof(Skulk_Tonic)] = value3;
		EffectEvents[typeof(SphynxSalt_Tonic)] = value3;
		EffectEvents[typeof(SporeCloudPoison)] = value3;
		EffectEvents[typeof(StingerPoisoned)] = value3;
		EffectEvents[typeof(Stressed)] = value3;
		EffectEvents[typeof(Stuck)] = value3;
		EffectEvents[typeof(Stun)] = value3;
		EffectEvents[typeof(StunGasStun)] = value3;
		EffectEvents[typeof(Suppressed)] = value3;
		EffectEvents[typeof(Swimming)] = value3;
		EffectEvents[typeof(SynapseSnap)] = value3;
		EffectEvents[typeof(TimeDilated)] = value3;
		EffectEvents[typeof(TimeDilatedIndependent)] = value3;
		EffectEvents[typeof(ToughnessDrained)] = value3;
		EffectEvents[typeof(Trance)] = value6;
		EffectEvents[typeof(Ubernostrum_Tonic)] = value3;
		EffectEvents[typeof(UrbanCamouflaged)] = value3;
		EffectEvents[typeof(VehicleUnpowered)] = value3;
		EffectEvents[typeof(Wading)] = value3;
		EffectEvents[typeof(WarTrance)] = value3;
		EffectEvents[typeof(WarmingUp)] = value3;
		EffectEvents[typeof(ProjectileReflectionShield)] = value3;
		EffectEvents[typeof(IrisdualMolting)] = value;
		EffectEvents[typeof(Cudgel_SmashingUp)] = value3;
		EffectEvents[typeof(Hobbled)] = value3;
	}

	public static void Evaluate(IEnumerable<Type> Types, out Rack<int[]> Arrays, out ulong Hash)
	{
		PartEvents.Clear();
		EffectEvents.Clear();
		PartExtensions.Clear();
		EffectExtensions.Clear();
		Arrays = new Rack<int[]>();
		Hash = 14695981039346656037uL;
		Rack<int> rack = new Rack<int>();
		Dictionary<Type, Rack<Type>> dictionary = new Dictionary<Type, Rack<Type>>();
		Type typeFromHandle = typeof(IComponent<GameObject>);
		Type[] types = new Type[1] { typeof(RenderEvent) };
		Type[] types2 = new Type[2]
		{
			typeof(long),
			typeof(int)
		};
		string name = "Render";
		string name2 = "OverlayRender";
		string name3 = "FinalRender";
		string name4 = "TurnTick";
		foreach (Type Type in Types)
		{
			if (Type.IsAbstract || !typeof(IComponent<GameObject>).IsAssignableFrom(Type))
			{
				continue;
			}
			if (Type.GetMethod(name, types).DeclaringType != typeFromHandle)
			{
				rack.Add(RenderEvent.ID);
			}
			if (Type.GetMethod(name2, types).DeclaringType != typeFromHandle)
			{
				rack.Add(RenderEvent.OverlayID);
			}
			if (Type.GetMethod(name3, types).DeclaringType != typeFromHandle)
			{
				rack.Add(RenderEvent.FinalID);
			}
			if (Type.GetMethod(name4, types2).DeclaringType != typeFromHandle)
			{
				rack.Add(IComponent<GameObject>.TurnTickID);
			}
			int[] array = null;
			if (rack.Count > 0)
			{
				int i = 0;
				for (int count = Arrays.Count; i < count; i++)
				{
					if (array != null)
					{
						break;
					}
					array = Arrays[i];
					if (array.Length != rack.Count)
					{
						array = null;
						continue;
					}
					int j = 0;
					for (int num = array.Length; j < num; j++)
					{
						if (array[j] != rack[j])
						{
							array = null;
							break;
						}
					}
				}
				if (array == null)
				{
					array = rack.ToArray();
					Arrays.Add(array);
				}
				if (typeof(IPart).IsAssignableFrom(Type))
				{
					PartEvents[Type] = array;
				}
				else
				{
					EffectEvents[Type] = array;
				}
				rack.Clear();
			}
			if (!Type.IsDefined(typeof(ComponentExtensionAttribute)))
			{
				continue;
			}
			Type baseType = Type.BaseType;
			while ((object)baseType != null)
			{
				if (baseType.IsAbstract && baseType.IsGenericType)
				{
					Type genericTypeDefinition = baseType.GetGenericTypeDefinition();
					if ((object)genericTypeDefinition == typeof(IPartExtension<>) || (object)genericTypeDefinition == typeof(IEffectExtension<>))
					{
						Type type = baseType.GetGenericArguments().FirstOrDefault();
						if ((object)type != null && !type.IsAbstract)
						{
							if (!dictionary.TryGetValue(type, out var value))
							{
								value = (dictionary[type] = new Rack<Type>());
							}
							value.Add(Type);
						}
						break;
					}
				}
				baseType = baseType.BaseType;
			}
		}
		using TextBuilder textBuilder = TextBuilder.Get();
		Rack<string> rack3 = new Rack<string>();
		foreach (var (type3, rack5) in dictionary)
		{
			TargetSignature[0] = type3;
			bool flag = typeof(IPart).IsAssignableFrom(type3);
			if (!flag && !typeof(Effect).IsAssignableFrom(type3))
			{
				continue;
			}
			Add add = CreateAddDelegate(type3, rack5, flag);
			if (add == null)
			{
				continue;
			}
			FinalizeRead finalizeRead = CreateFinalizeReadDelegate(type3, rack5, flag);
			if (finalizeRead == null)
			{
				continue;
			}
			if (flag)
			{
				PartExtensions[type3] = new ExtensionRecord(add, finalizeRead);
			}
			else
			{
				EffectExtensions[type3] = new ExtensionRecord(add, finalizeRead);
			}
			foreach (Type item in rack5)
			{
				textBuilder.Clear();
				if (ModManager.ModAssemblyMap.TryGetValue(item.Assembly, out var value2))
				{
					textBuilder.Append(value2.ID).Append('.');
				}
				item.AppendName(textBuilder, Full: true);
				rack3.Add(textBuilder.ToString());
			}
		}
		rack3.Sort();
		ExtensionHash = Genkit.Hash.FNV1A64(rack3);
	}

	private static Add CreateAddDelegate(Type Target, Rack<Type> Extensions, bool Part)
	{
		DynamicMethod dynamicMethod = new DynamicMethod("OnAddComponentDelegate_" + Target.Name, typeof(void), AddSignature, Target, skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.DeclareLocal(typeof(IComponent<GameObject>));
		foreach (Type Extension in Extensions)
		{
			EmitBody(iLGenerator, Extension, Conditional: false, Part);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return (Add)dynamicMethod.CreateDelegate(typeof(Add));
	}

	private static FinalizeRead CreateFinalizeReadDelegate(Type Target, Rack<Type> Extensions, bool Part)
	{
		DynamicMethod dynamicMethod = new DynamicMethod("OnFinalizeReadComponentDelegate_" + Target.Name, typeof(void), FinalizeReadSignature, Target, skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.DeclareLocal(typeof(IComponent<GameObject>));
		foreach (Type Extension in Extensions)
		{
			EmitBody(iLGenerator, Extension, Conditional: true, Part);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return (FinalizeRead)dynamicMethod.CreateDelegate(typeof(FinalizeRead));
	}

	private static void EmitBody(ILGenerator Generator, Type Extension, bool Conditional, bool Part)
	{
		FieldInfo field = Extension.GetField(Part ? "ParentPart" : "ParentEffect", Flags);
		if ((object)field == null)
		{
			return;
		}
		Type[] array = TargetSignature;
		ConstructorInfo constructor = Extension.GetConstructor(Flags, null, array, null);
		if ((object)constructor == null)
		{
			array = Type.EmptyTypes;
			constructor = Extension.GetConstructor(Flags, null, array, null);
			if ((object)constructor == null)
			{
				return;
			}
		}
		Label label = default;
		if (Conditional)
		{
			label = Generator.DefineLabel();
			Generator.Emit(OpCodes.Ldarg_0);
			Generator.Emit(OpCodes.Ldtoken, Extension);
			Generator.Emit(OpCodes.Call, GetTypeFromHandle);
			Generator.Emit(OpCodes.Call, Part ? HasPart : HasEffect);
			Generator.Emit(OpCodes.Brtrue_S, label);
		}
		Generator.Emit(OpCodes.Ldarg_0);
		if (array.Length == 1)
		{
			Generator.Emit(OpCodes.Ldarg_1);
		}
		Generator.Emit(OpCodes.Newobj, constructor);
		Generator.Emit(OpCodes.Stloc_0);
		Generator.Emit(OpCodes.Ldloc_0);
		Generator.Emit(OpCodes.Ldarg_1);
		Generator.Emit(OpCodes.Stfld, field);
		Generator.Emit(OpCodes.Ldloc_0);
		if (Part)
		{
			Generator.Emit(Conditional ? OpCodes.Ldc_I4_1 : OpCodes.Ldarg_2);
			Generator.Emit(OpCodes.Ldc_I4_1);
			Generator.Emit(Conditional ? OpCodes.Ldc_I4_0 : OpCodes.Ldarg_3);
			Generator.Emit(OpCodes.Call, AddPartInternals);
		}
		else
		{
			Generator.Emit(OpCodes.Ldloc_0);
			Generator.Emit(OpCodes.Ldarg_1);
			Generator.Emit(OpCodes.Ldfld, EffectID);
			Generator.Emit(OpCodes.Stfld, Extension.GetField("ParentEffectID", Flags));
			Generator.Emit(OpCodes.Ldnull);
			Generator.Emit(OpCodes.Call, ApplyEffect);
			Generator.Emit(OpCodes.Pop);
		}
		if (Conditional)
		{
			Generator.MarkLabel(label);
		}
	}
}
