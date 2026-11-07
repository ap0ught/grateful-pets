using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Genkit;
using Occult.Engine.CodeGeneration;
using Unity.Profiling;
using XRL.Collections;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.AI.Pathfinding;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
[GenerateSerializationPartial(PostWrite = "PostWrite", PostRead = "PostRead")]
[GeneratePoolingPartial(Capacity = 64)]
public class Brain : IPart
{
	public enum FeelingLevel
	{
		Hostile,
		Neutral,
		Allied
	}

	public enum AllegianceLevel
	{
		None,
		Associated,
		Affiliated,
		Member
	}

	public class WeaponSorter : Comparer<GameObject>
	{
		private GameObject POV;

		private bool Reverse;

		public bool IsValid()
		{
			return GameObject.Validate(ref POV);
		}

		public WeaponSorter()
		{
		}

		public WeaponSorter(GameObject POV)
			: this()
		{
			Setup(POV);
		}

		public WeaponSorter(GameObject POV, bool Reverse)
			: this()
		{
			Setup(POV, Reverse);
		}

		public void Setup(GameObject POV = null, bool Reverse = false)
		{
			this.POV = POV;
			this.Reverse = Reverse;
		}

		public override int Compare(GameObject Object1, GameObject Object2)
		{
			return CompareWeapons(Object1, Object2, POV) * ((!Reverse) ? 1 : (-1));
		}
	}

	public class MissileWeaponSorter : Comparer<GameObject>
	{
		private GameObject POV;

		private bool Reverse;

		public MissileWeaponSorter(GameObject POV)
		{
			this.POV = POV;
		}

		public MissileWeaponSorter(GameObject POV, bool Reverse)
			: this(POV)
		{
			this.Reverse = Reverse;
		}

		public override int Compare(GameObject o1, GameObject o2)
		{
			return CompareMissileWeapons(o1, o2, POV) * ((!Reverse) ? 1 : (-1));
		}
	}

	public class GearSorter : Comparer<GameObject>
	{
		private GameObject POV;

		private bool Reverse;

		public bool IsValid()
		{
			return GameObject.Validate(ref POV);
		}

		public GearSorter()
		{
		}

		public GearSorter(GameObject POV)
			: this()
		{
			Setup(POV);
		}

		public GearSorter(GameObject POV, bool Reverse)
			: this()
		{
			Setup(POV, Reverse);
		}

		public void Setup(GameObject POV = null, bool Reverse = false)
		{
			this.POV = POV;
			this.Reverse = Reverse;
		}

		public override int Compare(GameObject Object1, GameObject Object2)
		{
			return CompareGear(Object1, Object2, POV) * ((!Reverse) ? 1 : (-1));
		}
	}

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	private static readonly IPartPool BrainPool = new IPartPool(64);

	/// <todo>Tweak thresholds to have a better feeling spread.</todo>
	public const int FEELING_HOSTILE_THRESHOLD = -10;

	public const int FEELING_ALLIED_THRESHOLD = 50;

	public static readonly int DEFAULT_MIN_KILL_RADIUS = 5;

	public static readonly int DEFAULT_MAX_KILL_RADIUS = 15;

	public static readonly int DEFAULT_HOSTILE_WALK_RADIUS = 84;

	public static readonly int DEFAULT_MAX_WANDER_RADIUS = 12;

	public const int PARTY_PROSELYTIZED = 1;

	public const int PARTY_BEGUILED = 2;

	public const int PARTY_BONDED = 4;

	public const int PARTY_REBUKED = 8;

	public const int PARTY_INDEPENDENT = 8388608;

	public const int PARTY_SUPPORT = 15;

	public const int BRAIN_FLAG_HOSTILE = 1;

	public const int BRAIN_FLAG_CALM = 2;

	public const int BRAIN_FLAG_WANDERS = 4;

	public const int BRAIN_FLAG_WANDERS_RANDOMLY = 8;

	public const int BRAIN_FLAG_AQUATIC = 16;

	public const int BRAIN_FLAG_LIVES_ON_WALLS = 32;

	public const int BRAIN_FLAG_WALL_WALKER = 64;

	public const int BRAIN_FLAG_MOBILE = 128;

	public const int BRAIN_FLAG_HIBERNATING = 256;

	public const int BRAIN_FLAG_POINT_BLANK_RANGE = 512;

	public const int BRAIN_FLAG_DO_REEQUIP = 1024;

	public const int BRAIN_FLAG_NEED_TO_RELOAD = 2048;

	public const int BRAIN_FLAG_STAYING = 4096;

	public const int BRAIN_FLAG_PASSIVE = 8192;

	public const int BRAIN_FLAG_DO_PRIMARY_CHOICE_ON_REEQUIP = 16384;

	public int Flags = 17792;

	public int MaxMissileRange = 80;

	public int MinKillRadius = DEFAULT_MIN_KILL_RADIUS;

	public int MaxKillRadius = DEFAULT_MAX_KILL_RADIUS;

	public int HostileWalkRadius = DEFAULT_HOSTILE_WALK_RADIUS;

	public int MaxWanderRadius = DEFAULT_MAX_WANDER_RADIUS;

	private GameObjectReference LeaderReference;

	public GlobalLocation StartingCell;

	[NonSerialized]
	public string LastThought;

	public AllegianceSet Allegiance = new AllegianceSet();

	[NonSerialized]
	public StringMap<int> FactionFeelings = new StringMap<int>();

	public PartyCollection PartyMembers = new PartyCollection();

	public OpinionMap Opinions = new OpinionMap();

	[NonSerialized]
	public CleanStack<GoalHandler> Goals = new CleanStack<GoalHandler>();

	[NonSerialized]
	public Dictionary<GameObject, int> FriendlyFire;

	[NonSerialized]
	private static Event eAITakingAction = new ImmutableEvent("AITakingAction");

	[NonSerialized]
	private static Event eCommandEquipObject = new Event("CommandEquipObject", "Object", (object)null, "BodyPart", (object)null);

	[NonSerialized]
	private static Event eCommandEquipObjectFree = new Event("CommandEquipObject", "Object", (object)null, "BodyPart", (object)null, "EnergyCost", (object)0);

	[NonSerialized]
	private static Event eFactionsAdded = new ImmutableEvent("FactionsAdded");

	[NonSerialized]
	private static Event eTakingAction = new ImmutableEvent("TakingAction");

	private static Dictionary<string, int> ProcFactions = new Dictionary<string, int>();

	private static readonly ProfilerMarker s_BrainCommandTakeActionEvent = new ProfilerMarker("s_BrainCommandTakeActionEvent");

	[NonSerialized]
	public static List<GameObject> objectsToRemove = new List<GameObject>();

	[NonSerialized]
	private static GearSorter SharedGearSorter = new GearSorter();

	[NonSerialized]
	private static WeaponSorter SharedWeaponSorter = new WeaponSorter();

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override IPartPool Pool => BrainPool;

	public bool Hostile
	{
		get
		{
			return (Allegiance.Flags & 1) == 1;
		}
		[Obsolete("Hostile is part of allegiance set, use Brain.Allegiance.Hostile = value")]
		set
		{
			Allegiance.Flags = (value ? (Allegiance.Flags | 1) : (Allegiance.Flags & -2));
		}
	}

	public bool Calm
	{
		get
		{
			return (Allegiance.Flags & 2) == 2;
		}
		[Obsolete("Calm is part of allegiance set, use Brain.Allegiance.Calm = value")]
		set
		{
			Allegiance.Flags = (value ? (Allegiance.Flags | 2) : (Allegiance.Flags & -3));
		}
	}

	public bool Wanders
	{
		get
		{
			return (Flags & 4) == 4;
		}
		set
		{
			Flags = (value ? (Flags | 4) : (Flags & -5));
		}
	}

	public bool WandersRandomly
	{
		get
		{
			return (Flags & 8) == 8;
		}
		set
		{
			Flags = (value ? (Flags | 8) : (Flags & -9));
		}
	}

	public bool Aquatic
	{
		get
		{
			return (Flags & 0x10) == 16;
		}
		set
		{
			Flags = (value ? (Flags | 0x10) : (Flags & -17));
		}
	}

	public bool LivesOnWalls
	{
		get
		{
			return (Flags & 0x20) == 32;
		}
		set
		{
			Flags = (value ? (Flags | 0x20) : (Flags & -33));
		}
	}

	public bool WallWalker
	{
		get
		{
			return (Flags & 0x40) == 64;
		}
		set
		{
			Flags = (value ? (Flags | 0x40) : (Flags & -65));
		}
	}

	public bool Mobile
	{
		get
		{
			return (Flags & 0x80) == 128;
		}
		set
		{
			Flags = (value ? (Flags | 0x80) : (Flags & -129));
		}
	}

	public bool Hibernating
	{
		get
		{
			return (Flags & 0x100) == 256;
		}
		set
		{
			Flags = (value ? (Flags | 0x100) : (Flags & -257));
		}
	}

	public bool PointBlankRange
	{
		get
		{
			return (Flags & 0x200) == 512;
		}
		set
		{
			Flags = (value ? (Flags | 0x200) : (Flags & -513));
		}
	}

	public bool DoReequip
	{
		get
		{
			return (Flags & 0x400) == 1024;
		}
		set
		{
			Flags = (value ? (Flags | 0x400) : (Flags & -1025));
		}
	}

	public bool NeedToReload
	{
		get
		{
			return (Flags & 0x800) == 2048;
		}
		set
		{
			Flags = (value ? (Flags | 0x800) : (Flags & -2049));
		}
	}

	public bool Staying
	{
		get
		{
			return (Flags & 0x1000) == 4096;
		}
		set
		{
			Flags = (value ? (Flags | 0x1000) : (Flags & -4097));
		}
	}

	public bool Passive
	{
		get
		{
			return (Flags & 0x2000) == 8192;
		}
		set
		{
			Flags = (value ? (Flags | 0x2000) : (Flags & -8193));
		}
	}

	public bool DoPrimaryChoiceOnReequip
	{
		get
		{
			return (Flags & 0x4000) == 16384;
		}
		set
		{
			Flags = (value ? (Flags | 0x4000) : (Flags & -16385));
		}
	}

	public GameObject PartyLeader
	{
		get
		{
			return LeaderReference?.Object;
		}
		set
		{
			SetPartyLeader(value);
		}
	}

	public string Factions
	{
		set
		{
			FillFactionMembership(FindAllegiance(0), value);
		}
	}

	public string Feelings
	{
		set
		{
			if (value.IsNullOrEmpty())
			{
				return;
			}
			DelimitedEnumeratorChar enumerator = value.DelimitedBy(',').GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReadOnlySpan<char> current = enumerator.Current;
				int num = current.IndexOf(':');
				if (num == -1)
				{
					continue;
				}
				ReadOnlySpan<char> readOnlySpan = current.Slice(0, num);
				if (!int.TryParse(current.Slice(num + 1), out var result))
				{
					continue;
				}
				if (readOnlySpan.Length == 3 && readOnlySpan.SequenceEqual("All"))
				{
					foreach (Faction item in XRL.World.Factions.Loop())
					{
						FactionFeelings[item.Name] = result;
					}
				}
				else
				{
					FactionFeelings[new string(readOnlySpan)] = result;
				}
			}
		}
	}

	public override int Priority => 90000;

	public GameObject Target
	{
		get
		{
			if (ParentObject.IsPlayer())
			{
				return Sidebar.CurrentTarget;
			}
			for (int num = Goals.Items.Count - 1; num >= 0; num--)
			{
				if (Goals.Items[num] is Kill { Target: not null } kill && kill.Target.IsValid())
				{
					return kill.Target;
				}
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				if (ParentObject.IsPlayer())
				{
					Sidebar.CurrentTarget = null;
				}
				for (int num = Goals.Items.Count - 1; num >= 0; num--)
				{
					if (Goals.Items[num] is Kill)
					{
						for (int num2 = Goals.Items.Count - 1; num2 >= num; num2--)
						{
							Goals.Pop();
						}
						num = Goals.Items.Count - 1;
					}
				}
			}
			else if (Target == null && value != ParentObject)
			{
				if (ParentObject.IsPlayer())
				{
					Sidebar.CurrentTarget = value;
				}
				else
				{
					WantToKill(value);
				}
			}
		}
	}

	[Obsolete("use pPhysics on parent object")]
	public Physics pPhysics => ParentObject.Physics;

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override void Reset()
	{
		base.Reset();
		Flags = 17792;
		MaxMissileRange = 80;
		MinKillRadius = 5;
		MaxKillRadius = 15;
		HostileWalkRadius = 84;
		MaxWanderRadius = 12;
		LeaderReference?.Clear();
		StartingCell?.Clear();
		LastThought = null;
		Allegiance.Clear();
		FactionFeelings.Clear();
		PartyMembers.Clear();
		Opinions.Clear();
		Goals.Clear();
		FriendlyFire?.Clear();
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(Flags);
		Writer.WriteOptimized(MaxMissileRange);
		Writer.WriteOptimized(MinKillRadius);
		Writer.WriteOptimized(MaxKillRadius);
		Writer.WriteOptimized(HostileWalkRadius);
		Writer.WriteOptimized(MaxWanderRadius);
		Writer.Write(StartingCell);
		Writer.WriteComposite(Allegiance);
		Writer.WriteComposite(PartyMembers);
		Writer.WriteComposite(Opinions);
		PostWrite(Writer);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Flags = Reader.ReadOptimizedInt32();
		MaxMissileRange = Reader.ReadOptimizedInt32();
		MinKillRadius = Reader.ReadOptimizedInt32();
		MaxKillRadius = Reader.ReadOptimizedInt32();
		HostileWalkRadius = Reader.ReadOptimizedInt32();
		MaxWanderRadius = Reader.ReadOptimizedInt32();
		StartingCell = (GlobalLocation)Reader.ReadComposite();
		Allegiance = Reader.ReadComposite<AllegianceSet>();
		PartyMembers = Reader.ReadComposite<PartyCollection>();
		Opinions = Reader.ReadComposite<OpinionMap>();
		PostRead(Reader);
	}

	public bool StepTowards(Cell targetCell, bool Global = false)
	{
		if (targetCell == null)
		{
			return true;
		}
		Think("I'm going to move towards my target.");
		if (targetCell.ParentZone.IsWorldMap())
		{
			Think("Target's on the world map, can't follow!");
			return false;
		}
		FindPath findPath = new FindPath(ParentObject.CurrentCell, targetCell, Global, PathUnlimited: true, ParentObject, 94);
		if (findPath.Usable)
		{
			if (findPath.Directions.Count > 0)
			{
				PushGoal(new Step(findPath.Directions[0]));
			}
			return true;
		}
		return false;
	}

	public int RemoveGoalsDescendedFrom<T>()
	{
		int num = 0;
		for (int num2 = Goals.Items.Count - 1; num2 >= 0; num2--)
		{
			if (Goals.Items[num2] is T)
			{
				Goals.Items.RemoveAt(num2);
				num++;
			}
		}
		return num;
	}

	public GoalHandler FindGoal(string Name)
	{
		for (int num = Goals.Items.Count - 1; num >= 0; num--)
		{
			if (Goals.Items[num].GetType().Name == Name)
			{
				return Goals.Items[num];
			}
		}
		return null;
	}

	public bool HasGoal(string Name)
	{
		foreach (GoalHandler item in Goals.Items)
		{
			if (item.GetType().Name == Name)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	///   True unless the <see cref="F:XRL.World.Parts.Brain.Goals" /> are empty or only contain the "Bored" goal.
	/// </summary>
	public bool HasGoal()
	{
		return Goals.Count switch
		{
			0 => false, 
			1 => Goals.Peek().GetType().Name != "Bored", 
			_ => true, 
		};
	}

	public bool HasGoalOtherThan(string What)
	{
		switch (Goals.Count)
		{
		case 0:
			return false;
		case 1:
			if (Goals.Peek().GetType().Name != What)
			{
				return Goals.Peek().GetType().Name != "Bored";
			}
			return false;
		default:
			foreach (GoalHandler item in Goals.Items)
			{
				if (item.GetType().Name != What && item.GetType().Name != "Bored")
				{
					return true;
				}
			}
			return false;
		}
	}

	public AllegianceSet FindAllegiance(int ID)
	{
		for (AllegianceSet allegianceSet = Allegiance; allegianceSet != null; allegianceSet = allegianceSet.Previous)
		{
			if (allegianceSet.SourceID == ID)
			{
				return allegianceSet;
			}
		}
		return null;
	}

	public AllegianceSet FindAllegiance<T>(int ID = 0) where T : IAllyReason
	{
		return FindAllegiance(typeof(T), ID);
	}

	public AllegianceSet FindAllegiance(Type Type, int ID = 0)
	{
		for (AllegianceSet allegianceSet = Allegiance; allegianceSet != null; allegianceSet = allegianceSet.Previous)
		{
			if ((ID == 0 || allegianceSet.SourceID == ID) && (object)allegianceSet.Reason?.GetType() == Type)
			{
				return allegianceSet;
			}
		}
		return null;
	}

	public AllegianceSet GetBaseAllegiance()
	{
		return FindAllegiance(0);
	}

	public void RemoveAllegiance(AllegianceSet Set)
	{
		RemoveAllegiance(Set.SourceID);
	}

	public void RemoveAllegiance(GameObject Object)
	{
		RemoveAllegiance(Object.BaseID);
	}

	public void RemoveAllegiance<T>(GameObject Object) where T : IAllyReason
	{
		if (Object != null && Object.HasID)
		{
			RemoveAllegiance(typeof(T), Object.BaseID);
		}
	}

	public void RemoveAllegiance<T>(int ID = 0) where T : IAllyReason
	{
		RemoveAllegiance(typeof(T), ID);
	}

	public void RemoveAllegiance(Type Type, int ID = 0)
	{
		AllegianceSet allegianceSet = Allegiance;
		AllegianceSet previous = allegianceSet.Previous;
		if ((ID == 0 || allegianceSet.SourceID == ID) && (object)allegianceSet.Reason?.GetType() == Type)
		{
			Allegiance = previous;
			allegianceSet.Previous = null;
			return;
		}
		while (previous != null)
		{
			if ((ID == 0 || previous.SourceID == ID) && (object)previous.Reason?.GetType() == Type)
			{
				allegianceSet.Previous = previous.Previous;
				break;
			}
			allegianceSet = previous;
			previous = allegianceSet.Previous;
		}
	}

	public void RemoveAllegiance(int ID)
	{
		AllegianceSet allegianceSet = Allegiance;
		AllegianceSet previous = allegianceSet.Previous;
		if (allegianceSet.SourceID == ID)
		{
			Allegiance = previous;
			allegianceSet.Previous = null;
			return;
		}
		while (previous != null)
		{
			if (previous.SourceID == ID)
			{
				allegianceSet.Previous = previous.Previous;
				break;
			}
			allegianceSet = previous;
			previous = allegianceSet.Previous;
		}
	}

	public void PushAllegiance(AllegianceSet Set)
	{
		if (Allegiance != Set)
		{
			Set.Previous = Allegiance;
			Allegiance = Set;
		}
	}

	public void PopAllegiance()
	{
		if (Allegiance.Previous != null)
		{
			AllegianceSet allegiance = Allegiance;
			Allegiance = allegiance.Previous;
			allegiance.Previous = null;
		}
	}

	public void TakeBaseAllegiance(Brain Brain)
	{
		FindAllegiance(0).Copy(Brain.Allegiance);
	}

	public void TakeBaseAllegiance(GameObject Object)
	{
		TakeBaseAllegiance(Object.Brain);
	}

	public void TakeAllegiance(GameObject Object, IAllyReason Reason)
	{
		Brain brain = Object?.Brain;
		if (brain != null)
		{
			AllegianceSet allegiance = brain.Allegiance;
			Type type = Reason.GetType();
			if (Reason.Replace != IAllyReason.ReplaceTarget.None)
			{
				RemoveAllegiance(type, (Reason.Replace == IAllyReason.ReplaceTarget.Source) ? Object.BaseID : 0);
			}
			AllegianceSet allegianceSet = new AllegianceSet();
			allegianceSet.Copy(allegiance);
			allegianceSet.SourceID = Object.BaseID;
			allegianceSet.Reason = Reason;
			Reason.Time = The.Game?.TimeTicks ?? 0;
			PushAllegiance(allegianceSet);
			Reason.Initialize(ParentObject, Object, allegianceSet);
			Goals?.Clear();
		}
	}

	public void TakeAllegiance<T>(GameObject Object) where T : IAllyReason, new()
	{
		TakeAllegiance(Object, new T());
	}

	public void TakeDemeanor(GameObject Object)
	{
		Allegiance.Flags = Object.Brain.Allegiance.Flags;
	}

	public bool TakeOnAttitudesOf(GameObject Object, bool CopyLeader = false, bool CopyTarget = false)
	{
		TakeBaseAllegiance(Object);
		return true;
	}

	public bool TakeOnAttitudesOf(Brain o, bool CopyLeader = false, bool CopyTarget = false)
	{
		TakeBaseAllegiance(o);
		return true;
	}

	public void SetAlliedLeader<T>(GameObject Object, int Flags = 0, bool Silent = false) where T : IAllyReason, new()
	{
		TakeAllegiance<T>(Object);
		SetPartyLeader(Object, Flags, Transient: false, Silent);
	}

	public void SetPartyLeader(GameObject Object, int Flags = 0, bool Transient = false, bool Silent = false)
	{
		GameObject gameObject = null;
		if (LeaderReference != null)
		{
			gameObject = LeaderReference.Object;
			LeaderReference.Set(Object);
			if (gameObject != null)
			{
				gameObject.Brain?.PartyMembers.Remove(ParentObject.BaseID);
				if (!Silent && gameObject.IsPlayer())
				{
					gameObject.PlayWorldSound("sfx_characterMod_follower_lose");
				}
			}
		}
		else if (Object != null)
		{
			LeaderReference = new GameObjectReference(Object);
		}
		if (Object != null)
		{
			if (Object?.Brain == null && ParentObject != null)
			{
				MetricsManager.LogWarning("Object without brain " + Object.Blueprint + " trying to be set as partyleader for " + ParentObject?.Blueprint);
			}
			if (!Transient)
			{
				Object.Brain?.PartyMembers.TryAdd(ParentObject, Flags);
				Forgive(Object);
				if (ParentObject.Physics?.LastDamagedBy == Object)
				{
					ParentObject.Physics.LastDamagedBy = null;
				}
				if (ParentObject.Physics?.InflamedBy == Object)
				{
					ParentObject.Physics.InflamedBy = null;
				}
			}
			if (Object.IsLedBy(ParentObject))
			{
				MetricsManager.LogError($"Potential leader cycle: {Object} + {ParentObject}");
			}
			if (!Silent && Object.IsPlayer())
			{
				Object.PlayWorldSound("sfx_characterMod_follower_gain");
			}
		}
		AfterChangePartyLeaderEvent.Send(ParentObject, Object, gameObject, Transient);
		if (Object != null && Object != gameObject)
		{
			Object.FireEvent(Event.New("GainedNewFollower", "Object", ParentObject));
		}
	}

	public bool IsMobile()
	{
		if (Mobile)
		{
			return true;
		}
		if ((ParentObject != null) & ParentObject.IsFlying)
		{
			return true;
		}
		return false;
	}

	[Obsolete("use IsMobile(), will be removed after Q2 2024")]
	public bool isMobile()
	{
		return IsMobile();
	}

	public bool LimitToAquatic()
	{
		if (!Aquatic)
		{
			return false;
		}
		if (ParentObject != null && ParentObject.IsFlying)
		{
			return false;
		}
		return true;
	}

	[Obsolete("use LimitToAquatic(), will be removed after Q2 2024")]
	public bool limitToAquatic()
	{
		return LimitToAquatic();
	}

	public void CheckMobility(out bool Immobile, out bool Waterbound, out bool WallWalker)
	{
		Immobile = !Mobile;
		Waterbound = Aquatic;
		WallWalker = this.WallWalker;
		if ((Immobile | Waterbound | WallWalker) && ParentObject != null && ParentObject.IsFlying)
		{
			Immobile = false;
			Waterbound = false;
			WallWalker = false;
		}
	}

	[Obsolete("use CheckMobility(), will be removed after Q2 2024")]
	public void checkMobility(out bool Immobile, out bool Waterbound, out bool WallWalker)
	{
		CheckMobility(out Immobile, out Waterbound, out WallWalker);
	}

	public void SetFactionFeeling(string Faction, int Feeling)
	{
		FactionFeelings[Faction] = Feeling;
	}

	[Obsolete("use SetFactionFeeling(), will be removed after Q2 2024")]
	public void setFactionFeeling(string Faction, int Feeling)
	{
		SetFactionFeeling(Faction, Feeling);
	}

	public void SetFactionMembership(string Faction, int Feeling)
	{
		Allegiance[Faction] = Feeling;
	}

	[Obsolete("use SetFactionMembership(), will be removed after Q2 2024")]
	public void setFactionMembership(string Faction, int Membership)
	{
		SetFactionMembership(Faction, Membership);
	}

	public bool CanFight()
	{
		if (!ParentObject.IsCombatObject())
		{
			return false;
		}
		int i = 0;
		for (int count = Goals.Count; i < count; i++)
		{
			if (!Goals.Items[i].CanFight())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsNonAggressive()
	{
		int i = 0;
		for (int count = Goals.Count; i < count; i++)
		{
			if (Goals.Items[i].IsNonAggressive())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsBusy()
	{
		int i = 0;
		for (int count = Goals.Count; i < count; i++)
		{
			if (Goals.Items[i].IsBusy())
			{
				return true;
			}
		}
		return false;
	}

	public void WantToKill(GameObject Subject, string Because = null, bool Directed = false)
	{
		if ((The.Core.IgnoreMe && Subject != null && Subject.IsPlayer()) || !ParentObject.IsCombatObject())
		{
			return;
		}
		ThinkOfKilling(Subject, Because);
		int i = 0;
		for (int count = Goals.Items.Count; i < count; i++)
		{
			if (Goals.Items[i] is Kill kill && kill._Target == Subject)
			{
				return;
			}
		}
		if (Goals.Count == 0)
		{
			new Bored().Push(this);
		}
		Goals.Peek().PushChildGoal(new Kill(Subject, Directed));
	}

	public void Mindwipe()
	{
		Goals.Clear();
		LeaderReference = null;
		StartingCell = null;
		Staying = false;
		Passive = false;
		Allegiance.Clear();
		Opinions.Clear();
		FactionFeelings.Clear();
		PartyMembers.Clear();
		if (FriendlyFire != null)
		{
			FriendlyFire.Clear();
		}
		ParentObject.GetBlueprint()?.ReinitializePart(this);
	}

	public void StopFighting(bool Involuntary = false)
	{
		Target = null;
		ClearHostileMemory();
		ClearHostileFactionFeelings();
		FlushNavigationCaches();
		if (ParentObject.HasRegisteredEvent("StopFighting"))
		{
			ParentObject.FireEvent(Event.New("StopFighting", "Target", (object)null, "Involuntary", Involuntary ? 1 : 0));
		}
	}

	public void StopFighting(GameObject Target, bool Involuntary = false)
	{
		if (ParentObject.IsPlayer() && Sidebar.CurrentTarget == Target)
		{
			Sidebar.CurrentTarget = null;
		}
		ForgiveCombat();
		bool flag = false;
		for (int num = Goals.Items.Count - 1; num >= 0; num--)
		{
			if (Goals.Items[num] is Kill kill && kill.Target == Target)
			{
				for (int num2 = Goals.Items.Count - 1; num2 >= num; num2--)
				{
					Goals.Pop();
				}
				num = Goals.Items.Count;
				flag = true;
			}
		}
		FlushNavigationCaches();
		if (ParentObject.HasRegisteredEvent("StopFighting"))
		{
			Event obj = Event.New("StopFighting");
			obj.SetParameter("Target", Target);
			if (Involuntary)
			{
				obj.SetFlag("Involuntary", State: true);
			}
			if (flag)
			{
				obj.SetFlag("GoalRemoved", State: true);
			}
			ParentObject.FireEvent(obj);
		}
	}

	public void StopFight(bool Involuntary = false)
	{
		List<GameObject> list = Event.NewGameObjectList();
		list.Add(ParentObject);
		The.ZoneManager.FindObjects(list, (GameObject o) => o != ParentObject && (o.InSamePartyAs(ParentObject) || (o.Target?.InSamePartyAs(ParentObject) ?? false)));
		int num = 0;
		for (int count = list.Count; num < count; num++)
		{
			for (int num2 = 0; num2 < count; num2++)
			{
				if (num != num2)
				{
					list[num].StopFighting(list[num2], Involuntary);
					list[num2].StopFighting(list[num], Involuntary);
				}
			}
		}
	}

	public void StopFight(GameObject Object, bool Involuntary = false, bool Reciprocal = false)
	{
		if (Reciprocal)
		{
			List<GameObject> list = Event.NewGameObjectList();
			list.Add(ParentObject);
			list.Add(Object);
			The.ZoneManager.FindObjects(list, (GameObject o) => o != ParentObject && o != Object && (o.InSamePartyAs(ParentObject) || o.InSamePartyAs(Object)));
			int num = 0;
			for (int count = list.Count; num < count; num++)
			{
				for (int num2 = 0; num2 < count; num2++)
				{
					if (num != num2)
					{
						list[num].StopFighting(list[num2], Involuntary);
						list[num2].StopFighting(list[num], Involuntary);
					}
				}
			}
			return;
		}
		List<GameObject> list2 = Event.NewGameObjectList();
		List<GameObject> list3 = Event.NewGameObjectList();
		list2.Add(ParentObject);
		list3.Add(Object);
		The.ZoneManager.FindObjects(list2, (GameObject o) => o != ParentObject && o.InSamePartyAs(ParentObject));
		The.ZoneManager.FindObjects(list3, (GameObject o) => o != Object && o.InSamePartyAs(Object));
		int num3 = 0;
		for (int count2 = list2.Count; num3 < count2; num3++)
		{
			int num4 = 0;
			for (int count3 = list3.Count; num4 < count3; num4++)
			{
				list2[num3].StopFighting(list3[num4], Involuntary);
			}
		}
	}

	public void ClearHostileMemory()
	{
		ForgiveCombat();
	}

	public void ClearHostileFactionFeelings()
	{
		if (FactionFeelings.Count <= 0)
		{
			return;
		}
		if (FactionFeelings.Count == 1)
		{
			if (FactionFeelings.First().Value < 0)
			{
				FactionFeelings.Clear();
			}
			return;
		}
		List<string> list = null;
		foreach (KeyValuePair<string, int> factionFeeling in FactionFeelings)
		{
			if (factionFeeling.Value < 0)
			{
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(factionFeeling.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (string item in list)
		{
			FactionFeelings.Remove(item);
		}
	}

	public bool ShouldRemember(GameObject Object)
	{
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			return true;
		}
		Zone currentZone = Object.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		if (!currentZone.IsActive())
		{
			return false;
		}
		if (currentZone != ParentObject.CurrentZone)
		{
			return false;
		}
		return true;
	}

	public void PostWrite(SerializationWriter Writer)
	{
		int num = 0;
		GameObject key;
		int value;
		if (FriendlyFire != null)
		{
			foreach (KeyValuePair<GameObject, int> item in FriendlyFire)
			{
				item.Deconstruct(out key, out value);
				GameObject gameObject = key;
				if (gameObject != null && ShouldRemember(gameObject))
				{
					num++;
				}
			}
		}
		Writer.WriteOptimized(num);
		if (num > 0)
		{
			foreach (KeyValuePair<GameObject, int> item2 in FriendlyFire)
			{
				item2.Deconstruct(out key, out value);
				GameObject gameObject2 = key;
				int value2 = value;
				if (gameObject2 != null && ShouldRemember(gameObject2))
				{
					Writer.WriteGameObject(gameObject2);
					Writer.WriteOptimized(value2);
				}
			}
		}
		Writer.Write(LeaderReference);
		Writer.WriteComposite(FactionFeelings);
	}

	public void PostRead(SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		if (num == 0)
		{
			FriendlyFire = null;
		}
		else
		{
			if (FriendlyFire == null)
			{
				FriendlyFire = new Dictionary<GameObject, int>(num);
			}
			else
			{
				FriendlyFire.Clear();
				FriendlyFire.EnsureCapacity(num);
			}
			for (int i = 0; i < num; i++)
			{
				GameObject key = Reader.ReadGameObject("friendlyfire");
				FriendlyFire.TryAdd(key, Reader.ReadOptimizedInt32());
			}
		}
		LeaderReference = Reader.ReadGameObjectReference();
		FactionFeelings = Reader.ReadComposite<StringMap<int>>();
		Opinions.ClearExpired();
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Brain obj = base.DeepCopy(Parent, MapInv) as Brain;
		obj.Allegiance = new AllegianceSet();
		obj.Allegiance.Copy(Allegiance);
		obj.PartyMembers = new PartyCollection();
		obj.Opinions = new OpinionMap();
		return obj;
	}

	public override void Attach()
	{
		ParentObject.Brain = this;
		if (ParentObject.HasPart<Combat>())
		{
			ParentObject.Flags.SetBit(2, value: true);
		}
	}

	public override void Remove()
	{
		ParentObject.Flags.SetBit(2, value: false);
		if (ParentObject?.Brain == this)
		{
			ParentObject.Brain = null;
		}
	}

	public GoalHandler PushGoal(GoalHandler Goal)
	{
		Goal.Push(this);
		return Goal;
	}

	public void Think(string Hrm)
	{
		LastThought = Hrm;
		if (ParentObject.TryGetIntProperty("ThinkOutLoud", out var Result) && Result > 0)
		{
			MessageQueue.AddPlayerMessage(ParentObject.DebugName + " thinks: '" + Hrm + "'");
		}
	}

	public void ThinkOfKilling(GameObject who, string because = null)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("I'm going to kill ").Append(who.an());
		if (!because.IsNullOrEmpty())
		{
			stringBuilder.Append(" because ").Append(because);
		}
		stringBuilder.Append('!');
		Think(stringBuilder.ToString());
	}

	public void ForgiveCombat()
	{
		foreach (var (_, opinionList2) in Opinions)
		{
			for (int num2 = opinionList2.Count - 1; num2 >= 0; num2--)
			{
				if (opinionList2[num2] is IOpinionCombat && opinionList2[num2].BaseValue < 0)
				{
					opinionList2.RemoveAt(num2);
				}
			}
		}
	}

	public void Forgive(GameObject Object)
	{
		if (Object == null || !Object.HasID || !Opinions.TryGetValue(Object.BaseID, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			if (value[num].BaseValue < 0)
			{
				value.RemoveAt(num);
			}
		}
	}

	public void BecomeCompanionOf(GameObject Object, bool Trifling = false, bool Involuntary = false)
	{
		SetAlliedLeader<AllyDefault>(Object);
		StopFight(Object, Involuntary);
	}

	public bool IsLedBy(GameObject Object)
	{
		GameObject gameObject = PartyLeader;
		while (gameObject != null)
		{
			if (gameObject == Object)
			{
				return true;
			}
			GameObject partyLeader = gameObject.PartyLeader;
			if (partyLeader == ParentObject)
			{
				MetricsManager.LogError("leader cycle in " + ParentObject.DebugName + " + " + gameObject.DebugName);
				break;
			}
			gameObject = partyLeader;
		}
		return false;
	}

	public bool IsLedBy(string Blueprint)
	{
		GameObject gameObject = PartyLeader;
		while (gameObject != null)
		{
			if (gameObject.Blueprint == Blueprint)
			{
				return true;
			}
			GameObject partyLeader = gameObject.PartyLeader;
			if (partyLeader == ParentObject)
			{
				MetricsManager.LogError("leader cycle in " + ParentObject.DebugName + " + " + gameObject.DebugName);
				break;
			}
			gameObject = partyLeader;
		}
		return false;
	}

	public bool IsPlayerLed()
	{
		GameObject gameObject = PartyLeader;
		while (gameObject != null)
		{
			if (gameObject.IsPlayer() || gameObject.LeftBehindByPlayer())
			{
				return true;
			}
			GameObject partyLeader = gameObject.PartyLeader;
			if (partyLeader == ParentObject)
			{
				MetricsManager.LogError("leader cycle in " + ParentObject.DebugName + " + " + gameObject.DebugName);
				break;
			}
			gameObject = partyLeader;
		}
		return false;
	}

	public void AddOpinion<T>(GameObject Subject, float Magnitude = 1f) where T : IOpinionSubject, new()
	{
		if (!TryGetOpinions(Subject, out var List))
		{
			return;
		}
		long num = The.Game?.TimeTicks ?? 0;
		for (int num2 = List.Count - 1; num2 >= 0; num2--)
		{
			IOpinion opinion = List[num2];
			if ((object)opinion.GetType() == typeof(T))
			{
				IOpinionSubject opinionSubject = (IOpinionSubject)opinion;
				if (num - opinionSubject.Time >= opinionSubject.Cooldown)
				{
					List.RemoveAt(num2);
					opinionSubject.Time = num;
					opinionSubject.Magnitude = Math.Min(opinionSubject.Limit, opinionSubject.Magnitude + Magnitude);
					opinionSubject.Initialize(ParentObject, Subject);
					List.Add(opinionSubject);
					AfterAddOpinion(opinionSubject, ParentObject, Subject, null, Renew: true);
				}
				return;
			}
		}
		T val = new T();
		val.Time = num;
		val.Magnitude = Magnitude;
		val.Initialize(ParentObject, Subject);
		List.Add(val);
		AfterAddOpinion(val, ParentObject, Subject);
	}

	public void AddOpinion<T>(GameObject Subject, GameObject Object, float Magnitude = 1f) where T : IOpinionObject, new()
	{
		if (!TryGetOpinions(Subject, out var List))
		{
			return;
		}
		long num = The.Game?.TimeTicks ?? 0;
		for (int num2 = List.Count - 1; num2 >= 0; num2--)
		{
			IOpinion opinion = List[num2];
			if ((object)opinion.GetType() == typeof(T))
			{
				IOpinionObject opinionObject = (IOpinionObject)opinion;
				if (num - opinionObject.Time >= opinionObject.Cooldown)
				{
					List.RemoveAt(num2);
					opinionObject.Time = num;
					opinionObject.Magnitude = Math.Min(opinionObject.Limit, opinionObject.Magnitude + Magnitude);
					opinionObject.Initialize(ParentObject, Subject, Object);
					List.Add(opinionObject);
					AfterAddOpinion(opinionObject, ParentObject, Subject, Object, Renew: true);
				}
				return;
			}
		}
		T val = new T();
		val.Time = num;
		val.Magnitude = Magnitude;
		val.Initialize(ParentObject, Subject, Object);
		List.Add(val);
		AfterAddOpinion(val, ParentObject, Subject, Object);
	}

	private void AfterAddOpinion(IOpinion Opinion, GameObject Actor, GameObject Subject, GameObject Object = null, bool Renew = false)
	{
		if (!Renew && Opinion.Value < 0)
		{
			int feeling = Actor.Brain.GetFeeling(Subject);
			if (feeling < -10 && feeling - Opinion.Value >= -10)
			{
				PlayWorldSound("sfx_creature_angered");
			}
		}
		AfterAddOpinionEvent.Send(Opinion, Actor, Subject, Object, Renew);
	}

	public bool TryGetOpinions(GameObject Subject, out OpinionList List)
	{
		if (IsPlayer() || Subject == ParentObject)
		{
			List = null;
			return false;
		}
		if (!Opinions.TryGetValue(Subject.BaseID, out List))
		{
			Opinions[Subject.BaseID] = (List = new OpinionList());
		}
		return true;
	}

	public bool RemoveOpinion<T>(GameObject Object) where T : IOpinion
	{
		if (!Opinions.TryGetValue(Object.BaseID, out var value))
		{
			return false;
		}
		bool result = false;
		for (int num = value.Count - 1; num >= 0; num--)
		{
			if ((object)value[num].GetType() == typeof(T))
			{
				value.RemoveAt(num);
				result = true;
			}
		}
		return result;
	}

	public void AdjustFeeling(GameObject GO, int FeelingDelta)
	{
	}

	public bool GetAngryAt(GameObject GO, int amount = -50)
	{
		return true;
	}

	public bool LikeBetter(GameObject GO, int amount = 50)
	{
		return true;
	}

	public GameObject GetFinalLeader()
	{
		GameObject gameObject = PartyLeader;
		while (gameObject != null)
		{
			GameObject partyLeader = gameObject.PartyLeader;
			if (partyLeader == null)
			{
				break;
			}
			if (partyLeader == ParentObject)
			{
				MetricsManager.LogError("leader cycle in " + ParentObject.DebugName + " + " + gameObject.DebugName);
				break;
			}
			gameObject = partyLeader;
		}
		return gameObject;
	}

	public Brain GetFinalLeaderBrain()
	{
		GameObject gameObject = PartyLeader;
		GameObject gameObject2 = gameObject;
		while (gameObject != null)
		{
			GameObject partyLeader = gameObject.PartyLeader;
			if (partyLeader == null)
			{
				break;
			}
			if (partyLeader == ParentObject)
			{
				MetricsManager.LogError("leader cycle in " + ParentObject.DebugName + " + " + gameObject.DebugName);
				break;
			}
			gameObject = partyLeader;
			if (gameObject.Brain != null)
			{
				gameObject2 = gameObject;
			}
		}
		Brain brain = gameObject?.Brain;
		if (brain == null)
		{
			if (gameObject2 == null)
			{
				return null;
			}
			brain = gameObject2.Brain;
		}
		return brain;
	}

	public bool InSamePartyAs(GameObject who)
	{
		if (who == null)
		{
			return false;
		}
		GameObject finalLeader = GetFinalLeader();
		if (finalLeader == null)
		{
			return who.IsLedBy(ParentObject);
		}
		if (who == finalLeader)
		{
			return true;
		}
		if (who.IsLedBy(finalLeader))
		{
			return true;
		}
		return false;
	}

	public bool InSameFactionAs(GameObject Object, AllegianceLevel Minimum = AllegianceLevel.Member)
	{
		Brain brain = Object.Brain;
		if (brain == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, int> item in Allegiance)
		{
			if (GetAllegianceLevel(item.Value) >= Minimum && brain.Allegiance.TryGetValue(item.Key, out var Value) && GetAllegianceLevel(Value) >= Minimum)
			{
				return true;
			}
		}
		return false;
	}

	public int? GetPersonalFeeling(GameObject Target)
	{
		if (Target.HasID && Opinions.TryGetValue(Target.BaseID, out var value))
		{
			return GetFeelingEvent.GetFor(ParentObject, Target, value.Total, null, null, Combat: false, Personal: true);
		}
		return null;
	}

	public int GetFeeling(GameObject Target)
	{
		if (Target == null)
		{
			return 0;
		}
		GameObject parentObject = ParentObject;
		if (parentObject == null)
		{
			return 0;
		}
		if (parentObject == Target)
		{
			return 100;
		}
		Brain brain = Target.Brain;
		if (brain == null)
		{
			return 0;
		}
		Brain finalLeaderBrain = GetFinalLeaderBrain();
		if (finalLeaderBrain != null)
		{
			return GetFeelingEvent.GetFor(parentObject, Target, finalLeaderBrain.GetFeeling(Target), finalLeaderBrain.ParentObject);
		}
		if (parentObject.IsPlayer())
		{
			GameObject target = brain.Target;
			if (target != null && (target == parentObject || target.IsPlayerLed()))
			{
				return GetFeelingEvent.GetFor(parentObject, Target, -25, null, null, Combat: true);
			}
		}
		GameObject finalLeader = brain.GetFinalLeader();
		if (finalLeader != null)
		{
			return GetFeelingEvent.GetFor(parentObject, Target, GetFeeling(finalLeader), null, finalLeader);
		}
		int num = 0;
		if (Target.HasID && Opinions.TryGetValue(Target.BaseID, out var value))
		{
			num = GetFeelingEvent.GetFor(parentObject, Target, value.Total, null, null, Combat: false, Personal: true);
		}
		int baseFactionFeeling = GetBaseFactionFeeling(Target);
		baseFactionFeeling = GetFeelingEvent.GetFor(parentObject, Target, baseFactionFeeling, null, null, Combat: false, Personal: false, Faction: true);
		int num2 = num + baseFactionFeeling;
		if (Hostile && num2 < 50)
		{
			num2 = Math.Min(num2, -50);
		}
		if (Calm && num2 >= -50 && num2 < 0)
		{
			num2 = 0;
		}
		return num2;
	}

	public int GetBaseFactionFeeling(GameObject Object)
	{
		return Allegiance.GetBaseFeeling(Object, FactionFeelings);
	}

	public static string GetPrimaryFaction(string FactionSpec)
	{
		int num = -999;
		string text = null;
		FillFactionMembership(ProcFactions, FactionSpec);
		foreach (KeyValuePair<string, int> procFaction in ProcFactions)
		{
			if (procFaction.Value > num)
			{
				text = procFaction.Key;
				num = procFaction.Value;
			}
		}
		if (text == null)
		{
			if (!FactionSpec.IsNullOrEmpty())
			{
				if (FactionSpec.Contains(","))
				{
					return ExtractFaction(FactionSpec.CachedCommaExpansion()[0]);
				}
				return ExtractFaction(FactionSpec);
			}
			return "Beasts";
		}
		return text;
	}

	public string GetPrimaryFaction(bool Base = false)
	{
		if (ParentObject.IsPlayer())
		{
			return "Player";
		}
		AllegianceSet allegianceSet = (Base ? GetBaseAllegiance() : Allegiance);
		int num = int.MinValue;
		string result = "Beasts";
		foreach (var (text2, num3) in allegianceSet)
		{
			if (num3 > num)
			{
				result = text2;
				num = num3;
			}
		}
		return result;
	}

	public string GetPrimaryFactionName(bool VisibleOnly = true, bool Formatted = true, bool Base = false)
	{
		AllegianceSet allegianceSet = (Base ? GetBaseAllegiance() : Allegiance);
		int num = int.MinValue;
		Faction faction = null;
		foreach (var (name, num3) in allegianceSet)
		{
			if (num3 > num)
			{
				Faction ifExists = XRL.World.Factions.GetIfExists(name);
				if (ifExists != null && (!VisibleOnly || ifExists.Visible))
				{
					faction = ifExists;
					num = num3;
				}
			}
		}
		if (!Formatted)
		{
			return faction?.Name;
		}
		return faction?.GetFormattedName();
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public bool IsHostileTowards(GameObject Object)
	{
		return GetFeelingLevel(Object) == FeelingLevel.Hostile;
	}

	public bool IsAlliedTowards(GameObject Object)
	{
		return GetFeelingLevel(Object) == FeelingLevel.Allied;
	}

	public bool IsNeutralTowards(GameObject Object)
	{
		return GetFeelingLevel(Object) == FeelingLevel.Neutral;
	}

	public static FeelingLevel GetFeelingLevel(int Feeling)
	{
		if (Feeling < -10)
		{
			return FeelingLevel.Hostile;
		}
		if (Feeling >= 50)
		{
			return FeelingLevel.Allied;
		}
		return FeelingLevel.Neutral;
	}

	public FeelingLevel GetFeelingLevel(GameObject Object)
	{
		if (Object == null)
		{
			return FeelingLevel.Neutral;
		}
		int feeling = GetFeeling(Object);
		if (feeling < -10)
		{
			return FeelingLevel.Hostile;
		}
		if (feeling >= 50)
		{
			return FeelingLevel.Allied;
		}
		return FeelingLevel.Neutral;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID && ID != EnteredCellEvent.ID && ID != SingletonEvent<CommandTakeActionEvent>.ID && ID != SingletonEvent<GeneralAmnestyEvent>.ID && ID != PooledEvent<GetCompanionStatusEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != PooledEvent<GetTradePerformanceEvent>.ID && ID != PooledEvent<GetWaterRitualLiquidEvent>.ID && ID != GetZoneSuspendabilityEvent.ID && ID != GetZoneFreezabilityEvent.ID && ID != InventoryActionEvent.ID && ID != ObjectCreatedEvent.ID && ID != SuspendingEvent.ID && ID != TookDamageEvent.ID && ID != BeforeDeathRemovalEvent.ID)
		{
			if (ID == TookEnvironmentalDamageEvent.ID)
			{
				return !IsPlayer();
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (Options.DebugInternals || Options.DebugAttitude)
		{
			E.AddAction("Show Attitude", "show attitude", "ShowAttitude", null, 'A', FireOnActor: false, -1, 500, Override: false, WorksAtDistance: true);
		}
		if (Options.DebugInternals)
		{
			string text = ((ParentObject.TryGetIntProperty("ThinkOutLoud", out var Result) && Result != 0) ? "Disable" : "Enable");
			E.AddAction(text + " think out loud", text.ToLower() + " think out loud", "ToggleThinkOutLoud", null, '/', FireOnActor: false, -1, 500, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ShowAttitude")
		{
			Popup.Show(BuildChronology(), null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
		}
		else if (E.Command == "ToggleThinkOutLoud")
		{
			bool flag = ParentObject.TryGetIntProperty("ThinkOutLoud", out var Result) && Result != 0;
			ParentObject.SetIntProperty("ThinkOutLoud", (!flag) ? 1 : 0);
			string verb = (flag ? "disable" : "enable");
			DidX(verb, "thinking out loud");
		}
		return base.HandleEvent(E);
	}

	public string BuildChronology()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<(long, string)> list = new List<(long, string)>();
		for (AllegianceSet allegianceSet = Allegiance; allegianceSet != null; allegianceSet = allegianceSet.Previous)
		{
			stringBuilder.Clear();
			if (allegianceSet.SourceID == 0)
			{
				stringBuilder.Append("Base allegiance ");
			}
			else
			{
				The.ZoneManager.FindObjectByID(allegianceSet.SourceID);
				stringBuilder.Append(allegianceSet.Reason.GetText(ParentObject)).Append(' ');
			}
			allegianceSet.AppendTo(stringBuilder);
			list.Add((allegianceSet.Reason?.Time ?? 0, stringBuilder.ToString()));
		}
		list.Sort(((long, string) a, (long, string) b) => a.Item1.CompareTo(b.Item1));
		stringBuilder.Clear();
		if (PartyLeader.IsValid())
		{
			stringBuilder.Append("---- Leader ---- ");
			stringBuilder.Append("\n    -- ").Append(PartyLeader.BaseDisplayNameStripped);
			stringBuilder.Append("\n Feeling (").Append(PartyLeader.Brain.GetFeeling(The.Player)).Append(')');
		}
		stringBuilder.Compound("---- Allegiances ---- ", "\n\n");
		foreach (var item in list)
		{
			if (item.Item1 > 0)
			{
				stringBuilder.Append("\nOn the ").Append(Calendar.GetDay(item.Item1)).Append(" of ")
					.Append(Calendar.GetMonth(item.Item1))
					.Append(' ');
			}
			else
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append(item.Item2);
		}
		stringBuilder.Append("\n\n---- Opinions ---- ");
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (var (num2, opinionList2) in Opinions)
		{
			List<(long, string)> list2 = new List<(long, string)>();
			GameObject gameObject = The.ZoneManager.FindObjectByID(num2);
			if (gameObject != null)
			{
				stringBuilder.Append("\n    -- ").Append(gameObject.BaseDisplayNameStripped).Append('\n');
				stringBuilder.Append("    Reputation (").Append(GetBaseFactionFeeling(gameObject)).Append(")");
			}
			else
			{
				stringBuilder.Append("\n    -- ID #").Append(num2);
			}
			foreach (IOpinion item2 in opinionList2)
			{
				stringBuilder2.Clear();
				stringBuilder2.Append(item2.GetText(ParentObject)).Append(" (").Append(item2.Value)
					.Append(')');
				list2.Add((item2.Time, stringBuilder2.ToString()));
			}
			list2.Sort(((long, string) a, (long, string) b) => a.Item1.CompareTo(b.Item1));
			foreach (var item3 in list2)
			{
				stringBuilder.Append("\n    [").Append(item3.Item1).Append("] ")
					.Append(item3.Item2);
			}
		}
		return stringBuilder.ToString();
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (E.Actor.IsPlayer() && Options.AutoexploreAttackIgnoredAdjacentEnemies && !AutoAct.IsGathering(E.Setting) && ParentObject.isAdjacentTo(E.Actor) && ParentObject.IsReal && ParentObject.HasStat("Hitpoints") && E.Actor.IsIrrelevantHostile(ParentObject) && E.Actor.PhaseAndFlightMatches(ParentObject) && (FungalVisionary.VisionLevel > 0 || !ParentObject.HasPart<FungalVision>() || E.Actor.HasPart<FungalVision>()))
		{
			E.Command = "Attack";
			E.AllowRetry = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!The.ActionManager.ActionQueue.Contains(ParentObject) && E.Cell.ParentZone.IsActive())
		{
			ParentObject.MakeActive();
		}
		if (!Wanders && !WandersRandomly && StartingCell.IsNullOrEmpty() && IsMobile() && !ParentObject.HasPropertyOrTag("NoStay"))
		{
			if (StartingCell == null)
			{
				StartingCell = new GlobalLocation();
			}
			StartingCell.SetCell(ParentObject.CurrentCell);
		}
		if (DoReequip)
		{
			DoReequip = false;
			PerformEquip(Silent: true, DoPrimaryChoiceOnReequip);
			DoPrimaryChoiceOnReequip = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == ParentObject)
		{
			if (GameObject.Validate(E.ForLeader))
			{
				E.AddStatus(E.ForLeader.DistanceTo(ParentObject).Things("square") + " away", -100);
			}
			if (Staying)
			{
				E.AddStatus("staying put");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTradePerformanceEvent E)
	{
		if (E.Actor != null && E.Actor.IsPlayer() && E.Trader != null)
		{
			Faction ifExists = XRL.World.Factions.GetIfExists(E.Trader.GetPrimaryFaction());
			if (ifExists != null)
			{
				E.LinearAdjustment += The.Game.PlayerReputation.GetTradePerformance(ifExists);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetWaterRitualLiquidEvent E)
	{
		string text = XRL.World.Factions.GetIfExists(GetPrimaryFaction(Base: true))?.GetWaterRitualLiquid(E.Target);
		if (!text.IsNullOrEmpty() && (!(text == "water") || E.Liquid.IsNullOrEmpty()))
		{
			E.Liquid = text;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetZoneSuspendabilityEvent E)
	{
		if (!WallWalker && IsTryingToJoinPartyLeader())
		{
			E.Suspendability = Suspendability.CompanionTryingToJoinPartyLeader;
			return false;
		}
		if (IsPlayerLed() && The.Player.OnWorldMap())
		{
			E.Suspendability = Suspendability.CompanionWhilePlayerIsOnWorldMap;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetZoneFreezabilityEvent E)
	{
		if (!WallWalker && IsTryingToJoinPartyLeader())
		{
			E.Freezability = Freezability.CompanionTryingToJoinPartyLeader;
			return false;
		}
		if (IsPlayerLed() && The.Player.OnWorldMap())
		{
			E.Freezability = Freezability.CompanionWhilePlayerIsOnWorldMap;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (E.Object != ParentObject || E.Object.IsNowhere())
		{
			return base.HandleEvent(E);
		}
		Wake();
		if (E.Actor == null || E.Actor == ParentObject || InSamePartyAs(E.Actor))
		{
			return base.HandleEvent(E);
		}
		if (ParentObject.HasRegisteredEvent("CanBeAngeredByDamage") && !ParentObject.FireEventDirect(Event.New("CanBeAngeredByDamage", "Attacker", E.Actor, "Damage", E.Damage)))
		{
			return base.HandleEvent(E);
		}
		if (E.Damage != null && E.Damage.HasAttribute("Accidental"))
		{
			bool tookDamage = E.Damage != null && E.Damage.Amount > 0;
			if (!FriendlyFireIncident(E.Actor, tookDamage))
			{
				return base.HandleEvent(E);
			}
		}
		Attacked(E.Actor, E.Weapon);
		return base.HandleEvent(E);
	}

	public void Attacked(GameObject Attacker, GameObject Weapon = null, float Magnitude = 1f)
	{
		if (IsPlayer())
		{
			AIHelpBroadcastEvent.Send(ParentObject, Attacker, null, null, 20, 1f, HelpCause.Assault);
			return;
		}
		if (!IsHostileTowards(Attacker))
		{
			AddOpinion<OpinionAttack>(Attacker, Weapon, Magnitude);
			AIHelpBroadcastEvent.Send(ParentObject, Attacker, null, null, 20, 1f, HelpCause.Assault);
		}
		if (Target == null && CanFight() && IsHostileTowards(Attacker))
		{
			WantToKill(Attacker, "because I was attacked");
		}
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (E.Killer != null && E.Killer != ParentObject && !E.Killer.InSamePartyAs(ParentObject))
		{
			if (ParentObject.Brain.TryGetOpinions(E.Killer, out var List) && List.Find((IOpinion x) => x is OpinionAttack) != null)
			{
				AIHelpBroadcastEvent.Send(ParentObject, E.Killer, null, null, 20, 1f, HelpCause.Murder);
			}
			else
			{
				AIHelpBroadcastEvent.Send(ParentObject, E.Killer, null, null, 20, 1f, HelpCause.Killed);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookEnvironmentalDamageEvent E)
	{
		if (!IsPlayer() && MovingTo() == null && (E.Damage.Amount > 5 || Stat.Random(1, 5) <= E.Damage.Amount || ParentObject.hitpoints <= 10 || ParentObject.isDamaged(0.1)) && (!IsHostileTowards(The.Player) || Stat.Random(15, 45) < ParentObject.Stat("Intelligence") + E.Damage.Amount))
		{
			PushGoal(new FleeLocation(ParentObject.CurrentCell, 1));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		StopFighting();
		FriendlyFire = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		E.AddEntry(this, "PartyLeader", PartyLeader);
		E.AddEntry(this, "Wanders", Wanders);
		E.AddEntry(this, "WandersRandomly", WandersRandomly);
		if (Goals.Count > 0)
		{
			stringBuilder.Clear();
			for (int num = Goals.Count - 1; num > 0; num--)
			{
				int num2 = 1;
				string description = Goals.Items[num].GetDescription();
				while (num > 0 && Goals.Items[num - 1].GetDescription() == description)
				{
					num2++;
					num--;
				}
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append('\n');
				}
				stringBuilder.Append(description);
				if (num2 > 1)
				{
					stringBuilder.Append(" x").Append(num2);
				}
			}
			E.AddEntry(this, "Goals", stringBuilder.ToString());
		}
		else
		{
			E.AddEntry(this, "Goals", "none");
		}
		E.AddEntry(this, "MinKillRadius", MinKillRadius);
		E.AddEntry(this, "MaxKillRadius", MaxKillRadius);
		E.AddEntry(this, "NeedToReload", NeedToReload);
		E.AddEntry(this, "Hostile walk radius to player", ParentObject.GetHostileWalkRadius(The.Player));
		E.AddEntry(this, "Last thought", LastThought.IsNullOrEmpty() ? "none" : LastThought);
		if (Allegiance.Count > 0)
		{
			stringBuilder.Clear();
			List<string> list = new List<string>(Allegiance.Count);
			foreach (KeyValuePair<string, int> item in Allegiance)
			{
				list.Add(item.Key);
			}
			list.Sort();
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(list[i]).Append('-').Append(Allegiance[list[i]]);
			}
			E.AddEntry(this, "Faction membership", stringBuilder.ToString());
		}
		else
		{
			E.AddEntry(this, "Faction membership", "none");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Hostile)
		{
			E.Postfix.Append("\nBase demeanor: {{r|aggressive}}");
		}
		if (Calm)
		{
			E.Postfix.Append("\nBase demeanor: {{g|docile}}");
		}
		if (Passive)
		{
			E.Postfix.Append("\nEngagement style: {{g|defensive}}");
		}
		else if (IsPlayerLed())
		{
			E.Postfix.Append("\nEngagement style: {{r|aggressive}}");
		}
		if (Target != null && IsPlayerLed())
		{
			E.Postfix.Append("\nFighting {{r|" + Target.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + "}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Hibernating && ParentObject.GetIntProperty("StartNotHibernatingChanceIn100", 50).in100())
		{
			Hibernating = false;
		}
		ParentObject.FireEvent(eFactionsAdded);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		FriendlyFire = null;
		if (IsMobile() && PartyLeader != null && PartyLeader.IsPlayer())
		{
			if (!PartyLeader.InSameZone(ParentObject))
			{
				Goals.Clear();
			}
		}
		else
		{
			Goals.Clear();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		using (s_BrainCommandTakeActionEvent.Auto())
		{
			objectsToRemove.Clear();
			if (FriendlyFire != null)
			{
				foreach (GameObject key in FriendlyFire.Keys)
				{
					Zone currentZone = key.CurrentZone;
					if (currentZone == null || !currentZone.IsActive())
					{
						objectsToRemove.Add(key);
					}
				}
				if (objectsToRemove.Count > 0)
				{
					foreach (GameObject item in objectsToRemove)
					{
						FriendlyFire.Remove(item);
					}
					objectsToRemove.Clear();
					if (FriendlyFire.Count == 0)
					{
						FriendlyFire = null;
					}
				}
			}
			if (IsPlayerLed())
			{
				ParentObject.ModIntProperty("TurnsAsPlayerMinion", 1);
			}
			if (ParentObject.IsInvalid())
			{
				return true;
			}
			if (ParentObject.IsNowhere())
			{
				ParentObject.UseEnergy(1000);
				return true;
			}
			if (!BeforeAITakingActionEvent.Check(ParentObject))
			{
				return true;
			}
			if (ParentObject.IsPlayer())
			{
				if (Options.DisablePlayerbrain)
				{
					return true;
				}
			}
			else if (The.Core.Calm)
			{
				ParentObject.UseEnergy(1000);
				return true;
			}
			if (PartyLeader != null && PartyLeader.HasRegisteredEvent("MinionTakingAction"))
			{
				PartyLeader.FireEvent(Event.New("MinionTakingAction", "Object", ParentObject));
			}
			if (!ParentObject.IsPlayer())
			{
				if (GoToPartyLeader())
				{
					return true;
				}
				if (Hibernating)
				{
					ParentObject.UseEnergy(1000);
					return true;
				}
				if (ParentObject.IsFrozen())
				{
					Think("I'm frozen!");
					ParentObject.UseEnergy(1000);
					return true;
				}
				ParentObject.FireEvent(eAITakingAction);
				Cell cell = ParentObject.CurrentCell;
				if (ParentObject.HasEffect<XRL.World.Effects.Confused>())
				{
					string randomDirection = Directions.GetRandomDirection();
					Cell localCellFromDirection = cell.GetLocalCellFromDirection(randomDirection);
					if (localCellFromDirection != null)
					{
						if (LimitToAquatic() && !localCellFromDirection.HasAquaticSupportFor(ParentObject))
						{
							ParentObject.UseEnergy(1000);
							return true;
						}
						if (!localCellFromDirection.IsEmpty())
						{
							ParentObject.UseEnergy(1000);
							return true;
						}
					}
					ParentObject.Move(randomDirection);
					return true;
				}
				if (DoReequip)
				{
					DoReequip = false;
					PerformReequip(Silent: false, DoPrimaryChoiceOnReequip);
					DoPrimaryChoiceOnReequip = true;
				}
				while (Goals.Count > 0 && Goals.Peek().Finished())
				{
					Goals.Pop();
				}
				if (Target == null && PartyLeader != null && PartyLeader.IsPlayerControlled() && CanAcquireTarget())
				{
					GameObject target = PartyLeader.Target;
					if (target != null && CheckPerceptionOf(target))
					{
						WantToKill(target, "to aid my leader");
					}
				}
				if (Target == null && cell != null)
				{
					bool flag = !IsPlayerLed();
					GameObject gameObject = FindProspectiveTarget(null, flag);
					if (gameObject != null)
					{
						WantToKill(gameObject, "out of hostility");
					}
					else if (flag)
					{
						Think("I looked for the player to target but didn't find them.");
					}
					else
					{
						Think("I looked for a target but didn't find one.");
					}
				}
				if (Goals.Count == 0)
				{
					new Bored().Push(this);
				}
			}
			ParentObject.FireEvent(eTakingAction);
			while (Goals.Count > 0 && Goals.Peek().Finished())
			{
				Goals.Pop();
			}
			if (Goals.Count > 0)
			{
				Goals.Peek().TakeAction();
				GameObject parentObject = ParentObject;
				if (parentObject != null && parentObject.IsPlayer())
				{
					The.Core.RenderDelay(200);
				}
			}
			return base.HandleEvent(E);
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AIMessage");
		Registrar.Register("AIWakeupBroadcast");
		base.Register(Object, Registrar);
	}

	public void Stay(Cell C)
	{
		if (C == null)
		{
			Staying = false;
			return;
		}
		if (IsMobile() && !Wanders && !WandersRandomly)
		{
			if (StartingCell == null)
			{
				StartingCell = new GlobalLocation();
			}
			StartingCell.SetCell(C);
		}
		Staying = true;
	}

	public void FleeTo(Cell Cell, int Duration = 3)
	{
		PushGoal(new FleeLocation(Cell, Duration));
	}

	public void MoveTo(Cell Cell, bool ClearFirst = true)
	{
		MoveTo(Cell.ParentZone, Cell.Location, ClearFirst);
	}

	public void MoveTo(Zone Zone, Location2D Location, bool ClearFirst = true)
	{
		if (ClearFirst)
		{
			Goals.Clear();
		}
		PushGoal(new MoveTo(Zone.GetCell(Location)));
	}

	public void MoveTo(GameObject Object, bool ClearFirst = true)
	{
		if (ClearFirst)
		{
			Goals.Clear();
		}
		PushGoal(new MoveTo(Object));
	}

	public void MoveToGlobal(string Zone, int X, int Y)
	{
		Goals.Clear();
		PushGoal(new MoveToGlobal(Zone, X, Y));
	}

	public void StopMoving()
	{
		for (int num = Goals.Items.Count - 1; num >= 0; num--)
		{
			GoalHandler goalHandler = Goals.Items[num] as IMovementGoal;
			if (goalHandler != null)
			{
				int count = Goals.Items.Count;
				goalHandler.FailToParent();
				if (count != Goals.Items.Count)
				{
					num = Goals.Items.Count;
				}
			}
		}
	}

	public bool IsTryingToJoinPartyLeader()
	{
		GameObject partyLeader = PartyLeader;
		if (partyLeader == null || partyLeader.IsNowhere())
		{
			return false;
		}
		if (Staying)
		{
			return false;
		}
		if (ParentObject.HasTagOrProperty("DoesNotJoinPartyLeader") || ParentObject.IsPlayer())
		{
			return false;
		}
		Cell TargetCell = partyLeader.CurrentCell;
		Cell cell = ParentObject.CurrentCell;
		if (TargetCell == null || cell == null || TargetCell.ParentZone == null || cell.ParentZone == null || TargetCell.ParentZone == cell.ParentZone)
		{
			return false;
		}
		if (TargetCell.ParentZone.IsWorldMap())
		{
			return false;
		}
		if (The.ZoneManager.TryGetZoneProperty<bool>(TargetCell.ParentZone.ZoneID, "JoinPartyLeaderPossible", out var Value) && !Value)
		{
			return false;
		}
		if (!JoinPartyLeaderPossibleEvent.Check(ParentObject, partyLeader, cell, ref TargetCell, ParentObject.IsPotentiallyMobile()))
		{
			return false;
		}
		return true;
	}

	public bool GoToPartyLeader()
	{
		GameObject partyLeader = PartyLeader;
		if (partyLeader == null || partyLeader.IsNowhere())
		{
			return false;
		}
		if (Staying)
		{
			return false;
		}
		if (ParentObject.HasEffect<Dominated>() || ParentObject.IsPlayer())
		{
			return false;
		}
		Cell TargetCell = partyLeader.CurrentCell;
		Cell cell = ParentObject.CurrentCell;
		if (TargetCell == null || TargetCell.ParentZone == null || (cell != null && TargetCell.ParentZone == cell.ParentZone))
		{
			return false;
		}
		if (TargetCell.ParentZone.IsWorldMap())
		{
			return false;
		}
		if (The.ZoneManager.TryGetZoneProperty<bool>(TargetCell.ParentZone.ZoneID, "JoinPartyLeaderPossible", out var Value) && !Value)
		{
			return false;
		}
		bool flag = ParentObject.HasTagOrProperty("DoesNotJoinPartyLeader");
		bool result = false;
		if (!flag && JoinPartyLeaderPossibleEvent.Check(ParentObject, partyLeader, cell, ref TargetCell, ParentObject.IsMobile()))
		{
			Goals.Clear();
			if (!TargetCell.ParentZone.IsWorldMap())
			{
				List<Cell> list = TargetCell.GetPassableConnectedAdjacentCellsFor(ParentObject, 3).ShuffleInPlace();
				if (TargetCell.IsPassable(ParentObject))
				{
					list.Insert(0, TargetCell);
				}
				Cell cell2 = null;
				int num = int.MaxValue;
				int distanceFromPreviousCell = 0;
				int distanceFromLeader = 0;
				foreach (Cell item in list)
				{
					if (item.GetNavigationWeightFor(ParentObject) < 10 && !item.HasCombatObject() && !item.IsSolidFor(ParentObject))
					{
						int num2 = cell?.PathDistanceTo(item) ?? 0;
						int num3 = item.PathDistanceTo(TargetCell);
						int num4 = num2 + num3;
						if (num4 < num && CanJoinPartyLeaderEvent.Check(ParentObject, partyLeader, cell, item, num2, num3))
						{
							cell2 = item;
							num = num4;
							distanceFromPreviousCell = num2;
							distanceFromLeader = num3;
						}
					}
				}
				if (cell2 != null)
				{
					Think("I'm going to join my leader.");
					ParentObject.SystemLongDistanceMoveTo(cell2);
					ParentObject.UseEnergy(1000, "Move Join Leader");
					JoinedPartyLeaderEvent.Send(ParentObject, partyLeader, cell, cell2, distanceFromPreviousCell, distanceFromLeader);
					result = true;
				}
			}
		}
		return result;
	}

	public void BroadcastForHelp(GameObject Target)
	{
		AIHelpBroadcastEvent.Send(ParentObject, Target);
	}

	public override bool Render(RenderEvent E)
	{
		if (Hibernating && !E.NoWake)
		{
			Wake();
		}
		return true;
	}

	public void Wake()
	{
		if (!Hibernating)
		{
			return;
		}
		Hibernating = false;
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && cell.ParentZone != null)
		{
			Event e = Event.New("AIWakeupBroadcast");
			List<GameObject> list = cell.ParentZone.FastFloodVisibility(cell.X, cell.Y, 5, "Brain", ParentObject);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				list[i].FireEvent(e);
			}
		}
	}

	public static AllegianceLevel GetAllegianceLevel(int? Membership)
	{
		if (Membership.HasValue)
		{
			if (Membership >= 75)
			{
				return AllegianceLevel.Member;
			}
			if (Membership >= 50)
			{
				return AllegianceLevel.Affiliated;
			}
			if (Membership > 0)
			{
				return AllegianceLevel.Associated;
			}
		}
		return AllegianceLevel.None;
	}

	public static AllegianceLevel GetAllegianceLevel(IDictionary<string, int> Membership, string Faction)
	{
		if (Membership.TryGetValue(Faction, out var value))
		{
			return GetAllegianceLevel(value);
		}
		return AllegianceLevel.None;
	}

	public AllegianceLevel GetAllegianceLevel(string Faction)
	{
		return GetAllegianceLevel(Allegiance, Faction);
	}

	public bool CheckVisibilityOf(GameObject who)
	{
		if (ParentObject.IsPlayer())
		{
			if (who.IsVisible())
			{
				return true;
			}
		}
		else
		{
			if (ParentObject.HasPart<Clairvoyance>() && ParentObject.Stat("Intelligence").in100())
			{
				return true;
			}
			if (ParentObject.HasLOSTo(who, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckPerceptionOf(GameObject Object)
	{
		if (CheckVisibilityOf(Object))
		{
			return true;
		}
		if (Object.IsInActiveZone() && Object.IsAudible(ParentObject))
		{
			return true;
		}
		if (Object.IsInActiveZone() && Object.IsSmellable(ParentObject))
		{
			return true;
		}
		return false;
	}

	public bool IsSuitableTarget(GameObject Object)
	{
		if (ParentObject.IsPlayerControlled())
		{
			if (Object.IsPlayerControlled())
			{
				return false;
			}
			if (Object.HasEffect<Asleep>())
			{
				return false;
			}
			if (Object.IsHostileTowards(The.Player) && CheckPerceptionOf(Object))
			{
				return true;
			}
		}
		else if (IsHostileTowards(Object) && CheckPerceptionOf(Object))
		{
			return true;
		}
		return false;
	}

	public bool IsSuitablePlayerControlledTarget(GameObject Object)
	{
		if (!Object.IsPlayerControlled())
		{
			return false;
		}
		if (IsHostileTowards(Object) && CheckPerceptionOf(Object))
		{
			return true;
		}
		return false;
	}

	public int TargetSort(GameObject Target1, GameObject Target2)
	{
		int num = PreferTargetEvent.Check(ParentObject, Target1, Target2);
		if (num != 0)
		{
			return -num;
		}
		int num2 = Target1.IsPlayer().CompareTo(Target2.IsPlayer());
		if (num2 != 0)
		{
			return -num2;
		}
		int num3 = Target1.PhaseMatches(ParentObject).CompareTo(Target2.PhaseMatches(ParentObject));
		if (num3 != 0)
		{
			return -num3;
		}
		int num4 = Target1.FlightMatches(ParentObject).CompareTo(Target2.FlightMatches(ParentObject));
		if (num4 != 0)
		{
			return -num4;
		}
		int num5 = Target1.DistanceTo(ParentObject).CompareTo(Target2.DistanceTo(ParentObject));
		if (num5 != 0)
		{
			return num5;
		}
		return 0;
	}

	public static int ExtractFactionMembership(ref string spec)
	{
		int num = spec.LastIndexOf('-');
		if (num == -1)
		{
			MetricsManager.LogError("Invalid faction membership specification: " + spec);
			return 0;
		}
		if (!int.TryParse(spec.Substring(num + 1), out var result) || result <= 0)
		{
			MetricsManager.LogError("Invalid faction membership specification: " + spec);
			return 0;
		}
		spec = spec.Substring(0, num);
		return result;
	}

	public static int ExtractFactionMembership(string spec)
	{
		return ExtractFactionMembership(ref spec);
	}

	public static string ExtractFaction(string spec)
	{
		ExtractFactionMembership(ref spec);
		return spec;
	}

	public static void FillFactionMembership(IDictionary<string, int> Map, string Spec)
	{
		if (Map is AllegianceSet allegianceSet)
		{
			allegianceSet.ClearSlots();
		}
		else
		{
			Map.Clear();
		}
		if (Spec.IsNullOrEmpty())
		{
			return;
		}
		if (Spec.Contains(","))
		{
			foreach (string item in Spec.CachedCommaExpansion())
			{
				string spec = item;
				int num = ExtractFactionMembership(ref spec);
				if (num > 0)
				{
					if (Map.TryGetValue(spec, out var value))
					{
						if (num > value)
						{
							Map[spec] = num;
						}
					}
					else
					{
						Map.Add(spec, num);
					}
				}
			}
			return;
		}
		int num2 = ExtractFactionMembership(ref Spec);
		if (num2 <= 0)
		{
			return;
		}
		if (Map.TryGetValue(Spec, out var value2))
		{
			if (num2 > value2)
			{
				Map[Spec] = num2;
			}
		}
		else
		{
			Map.Add(Spec, num2);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIWakeupBroadcast")
		{
			Hibernating = false;
		}
		else if (E.ID == "AIMessage")
		{
			if (ParentObject.IsNowhere())
			{
				return true;
			}
			if (ParentObject.IsPlayer())
			{
				return true;
			}
			Wake();
			if (E.GetStringParameter("Message") == "Attacked")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("By");
				if (gameObjectParameter != null && gameObjectParameter != ParentObject && !InSamePartyAs(gameObjectParameter) && (gameObjectParameter.IsCreature || 3.in100()) && ParentObject.FireEvent(Event.New("CanBeAngeredByBeingAttacked", "Attacker", gameObjectParameter)))
				{
					bool flag = E.HasFlag("Accidental");
					if (flag && !FriendlyFireIncident(gameObjectParameter))
					{
						return true;
					}
					if (IsPlayerLed())
					{
						if (!gameObjectParameter.IsPlayerControlled() && Target == null)
						{
							if (flag)
							{
								WantToKill(gameObjectParameter, "because I was accidentally attacked");
							}
							else
							{
								WantToKill(gameObjectParameter, "because I was attacked");
							}
						}
					}
					else
					{
						Attacked(gameObjectParameter);
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool HandleEvent(AIHelpBroadcastEvent E)
	{
		if (!GameObject.Validate(ParentObject))
		{
			return true;
		}
		if (ParentObject.IsPlayer() || ParentObject == E.Actor)
		{
			return true;
		}
		if (E.Target == null || Target != null)
		{
			return true;
		}
		if (ParentObject.TryGetEffect<Asleep>(out var _) && !ParentObject.HasTag("NoHelpBroadcastWake"))
		{
			ParentObject.FireEvent("WakeUp");
			return true;
		}
		if (E.Target.Brain != null)
		{
			if (InSamePartyAs(E.Target))
			{
				return true;
			}
			if (InSameFactionAs(E.Target))
			{
				return true;
			}
		}
		bool flag = ParentObject.DistanceTo(E.Target) <= MaxKillRadius && Target == null && CanFight() && (!Passive || !HasGoal());
		int feeling = GetFeeling(E.Actor);
		if (E.Cause == HelpCause.Assault)
		{
			if (feeling >= 50 && feeling >= GetFeeling(E.Target))
			{
				if (!IsHostileTowards(E.Target))
				{
					AddOpinion<OpinionAttackAlly>(E.Target, E.Actor, E.Magnitude);
				}
				if (flag && IsHostileTowards(E.Target))
				{
					WantToKill(E.Target, "to help " + E.Actor.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: true));
					return true;
				}
			}
		}
		else if (E.Cause == HelpCause.Murder || E.Cause == HelpCause.Killed)
		{
			if (feeling >= 50 && ((E.Cause == HelpCause.Murder && feeling >= GetFeeling(E.Target)) || InSamePartyAs(E.Actor) || InSameFactionAs(E.Actor)))
			{
				if (!IsHostileTowards(E.Target))
				{
					AddOpinion<OpinionKilledAlly>(E.Target, E.Actor, E.Magnitude);
				}
				if (flag && IsHostileTowards(E.Target))
				{
					WantToKill(E.Target, "to avenge " + E.Actor.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: true));
					return true;
				}
			}
		}
		else
		{
			foreach (string faction in E.Factions)
			{
				if (GetAllegianceLevel(faction) < AllegianceLevel.Affiliated)
				{
					continue;
				}
				if (ParentObject.HasRegisteredEvent("CanBeAngeredByPropertyCrime"))
				{
					Event e = Event.New("CanBeAngeredByPropertyCrime", "Attacker", E.Target, "Object", E.Actor, "Faction", faction);
					if (!ParentObject.FireEventDirect(e))
					{
						return false;
					}
				}
				if (E.Cause == HelpCause.Theft)
				{
					AddOpinion<OpinionThief>(E.Target, E.Item, E.Magnitude);
				}
				else if (E.Cause == HelpCause.Trespass)
				{
					AddOpinion<OpinionTrespass>(E.Target, E.Magnitude);
				}
				if (flag && IsHostileTowards(E.Target))
				{
					WantToKill(E.Target, (E.Actor != null) ? ("to protect " + E.Actor.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: true)) : ("for " + Faction.GetFormattedName(faction)));
					return true;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public bool CanAcquireTarget()
	{
		if (Passive)
		{
			return false;
		}
		if (!CanFight())
		{
			return false;
		}
		if (!ParentObject.FireEvent("AILookForTarget"))
		{
			return false;
		}
		return true;
	}

	public GameObject FindProspectiveTarget(Cell FromCell = null, bool WantPlayer = false)
	{
		if (!CanAcquireTarget())
		{
			return null;
		}
		if (FromCell == null)
		{
			FromCell = ParentObject.CurrentCell;
			if (FromCell == null)
			{
				return null;
			}
		}
		List<GameObject> list = FromCell.ParentZone.FastCombatSquareVisibility(FromCell.X, FromCell.Y, Stat.Random(MinKillRadius, MaxKillRadius), ParentObject, null, VisibleToPlayerOnly: false, IncludeWalls: true, IncludeLooker: false);
		if (list.Count > 0)
		{
			List<GameObject> list2 = Event.NewGameObjectList();
			if (WantPlayer)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (IsSuitablePlayerControlledTarget(list[i]))
					{
						list2.Add(list[i]);
					}
				}
			}
			else
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (IsSuitableTarget(list[j]))
					{
						list2.Add(list[j]);
					}
				}
			}
			if (list2 != null && list2.Count > 0)
			{
				if (list2.Count > 1)
				{
					list2.Sort(TargetSort);
					switch (Stat.Roll(1, 10))
					{
					case 1:
						return list2.GetRandomElement();
					case 2:
						return list2[list2.Count - 1];
					}
				}
				return list2[0];
			}
		}
		return null;
	}

	public bool FriendlyFireIncident(GameObject GO, bool TookDamage = false)
	{
		FeelingLevel feelingLevel = GetFeelingLevel(GO);
		int value = (TookDamage ? 1 : 0);
		if (feelingLevel == FeelingLevel.Allied || feelingLevel == FeelingLevel.Neutral)
		{
			if (!ParentObject.FireEvent(Event.New("CanBeAngeredByFriendlyFire", "Attacker", GO)))
			{
				return false;
			}
			if (ParentObject.Stat("Intelligence") < 7)
			{
				return true;
			}
			if (FriendlyFire == null)
			{
				if (value > 0)
				{
					FriendlyFire = new Dictionary<GameObject, int> { { GO, value } };
				}
			}
			else if (FriendlyFire.TryGetValue(GO, out value))
			{
				if (TookDamage)
				{
					value++;
					FriendlyFire[GO] = value;
				}
			}
			else if (value > 0)
			{
				FriendlyFire.Add(GO, value);
			}
			switch (feelingLevel)
			{
			case FeelingLevel.Allied:
				if (GO.IsPlayerControlled())
				{
					if (value < 5 && Stat.Random(0, 400) > value)
					{
						return false;
					}
				}
				else if (value < 20 && Stat.Random(0, 1000) > value)
				{
					return false;
				}
				break;
			case FeelingLevel.Neutral:
				if (GO.IsPlayerControlled())
				{
					if (value < 3 && Stat.Random(0, 200) > value)
					{
						return false;
					}
				}
				else if (value < 5 && Stat.Random(0, 400) > value)
				{
					return false;
				}
				break;
			}
		}
		return true;
	}

	public static double PreciseWeaponScore(GameObject obj, GameObject who = null)
	{
		if (obj == null)
		{
			return 0.0;
		}
		string inventoryCategory = obj.GetInventoryCategory();
		if (inventoryCategory == "Ammo")
		{
			return 0.0;
		}
		MeleeWeapon part = obj.GetPart<MeleeWeapon>();
		if (part == null)
		{
			return 0.0;
		}
		int num = part.BaseDamage.RollMinCached();
		int num2 = part.BaseDamage.RollMaxCached();
		double num3 = num * 2 + num2;
		int Bonus = 0;
		part.GetNormalPenetration(who, out var BasePenetration, out var StatMod);
		int StatBonus;
		if (who != null && IsAdaptivePenetrationActiveEvent.Check(obj, ref Bonus))
		{
			int MaxStatBonus = part.MaxStrengthBonus;
			int MaxPenetrationBonus = 0;
			int Penetrations = 0;
			int aV = (Statistic.IsMental(part.Stat) ? Stats.GetCombatMA(who) : Stats.GetCombatAV(who));
			StatBonus = BasePenetration + Bonus + 1;
			GetWeaponMeleePenetrationEvent.Process(ref Penetrations, ref StatBonus, ref MaxStatBonus, ref StatMod, ref MaxPenetrationBonus, aV, Critical: false, "", "Primary", who, who, obj);
		}
		else
		{
			StatBonus = BasePenetration + StatMod - 1;
		}
		double num4 = ((StatBonus < 1) ? (num3 / 2.0) : (num3 * (double)StatBonus));
		double num5 = 0.0;
		double num6 = 50 + part.HitBonus * 5;
		if (who != null)
		{
			num6 += (double)(who.StatMod("Agility") * 5);
			if (who.HasSkill(part.Skill))
			{
				num6 += (double)((part.Skill == "ShortBlades") ? 5 : 10);
				num6 += 10.0;
			}
			if ((part.Skill == "LongBlades" || part.Skill == "ShortBlades") && who.HasPart<LongBladesCore>())
			{
				if (part.Skill == "LongBlades")
				{
					if (who.HasPart(typeof(LongBladesLunge)))
					{
						num5++;
					}
					if (who.HasPart(typeof(LongBladesSwipe)))
					{
						num5++;
					}
					if (who.HasPart(typeof(LongBladesDeathblow)))
					{
						num5++;
					}
				}
				if (who.HasEffect(typeof(LongbladeStance_Aggressive)))
				{
					num6 = ((!who.HasPart(typeof(LongBladesImprovedAggressiveStance))) ? (num6 - 10.0) : (num6 - 15.0));
				}
				else if (who.HasEffect(typeof(LongbladeStance_Dueling)))
				{
					num6 = ((!who.HasPart(typeof(LongBladesImprovedDuelistStance))) ? (num6 + 10.0) : (num6 + 15.0));
				}
			}
		}
		num6 = Math.Min(Math.Max(num6, 5.0), 100.0);
		num5 += num4 * num6 / 50.0;
		if (obj.HasTag("Storied"))
		{
			num5 += 5.0;
		}
		if (obj.HasTag("NaturalGear"))
		{
			num5++;
		}
		string usesSlots = obj.UsesSlots;
		if (!usesSlots.IsNullOrEmpty())
		{
			num5 = num5 * 2.0 / (double)(usesSlots.CachedCommaExpansion().Count + 1);
		}
		else
		{
			int slotsRequiredFor = obj.GetSlotsRequiredFor(who, part.Slot);
			if (slotsRequiredFor > 1)
			{
				num5 = num5 * 2.0 / (double)(slotsRequiredFor + 1);
			}
		}
		if (part.Ego != 0)
		{
			num5 += (double)part.Ego;
		}
		if (inventoryCategory == "Melee Weapons" || inventoryCategory == "Natural Weapons")
		{
			num5++;
		}
		if (num6 != 50.0)
		{
			num5 += (num6 - 50.0) / 5.0;
		}
		string tag = obj.GetTag("AdjustWeaponScore");
		if (tag != null)
		{
			num5 += Convert.ToDouble(tag);
		}
		tag = who?.GetTagOrStringProperty("AdjustWieldedScore");
		if (!tag.IsNullOrEmpty() && tag.CachedNumericDictionaryExpansion().TryGetValue(obj.Blueprint, out var value))
		{
			num5 += (double)value;
		}
		if (obj.HasRegisteredEvent("AdjustWeaponScore"))
		{
			int num7 = (int)Math.Round(num5, MidpointRounding.AwayFromZero);
			Event obj2 = Event.New("AdjustWeaponScore", "Score", num7, "OriginalScore", num5, "User", who);
			obj.FireEvent(obj2);
			int intParameter = obj2.GetIntParameter("Score");
			if (num7 != intParameter)
			{
				num5 += (double)(intParameter - num7);
			}
		}
		return num5;
	}

	public static int WeaponScore(GameObject Object, GameObject Actor = null)
	{
		return (int)Math.Round(PreciseWeaponScore(Object, Actor), MidpointRounding.AwayFromZero);
	}

	public static bool TreatAsThrownWeapon(GameObject Object)
	{
		if (Object.IsThrownWeapon)
		{
			return !Object.HasTagOrProperty("NoAIEquipAsThrownWeapon");
		}
		return false;
	}

	public static int CompareWeapons(GameObject Weapon1, GameObject Weapon2, GameObject POV)
	{
		if (Weapon1 == Weapon2)
		{
			return 0;
		}
		if (Weapon1 == null)
		{
			return 1;
		}
		if (Weapon2 == null)
		{
			return -1;
		}
		int num = Weapon1.HasTagOrProperty("AlwaysEquipAsWeapon").CompareTo(Weapon2.HasTagOrProperty("AlwaysEquipAsWeapon"));
		if (num != 0)
		{
			return -num;
		}
		int num2 = Weapon1.HasPart<MissileWeapon>().CompareTo(Weapon2.HasPart<MissileWeapon>());
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = TreatAsThrownWeapon(Weapon1).CompareTo(TreatAsThrownWeapon(Weapon2));
		if (num3 != 0)
		{
			return num3;
		}
		int num4 = Weapon1.HasPart<Food>().CompareTo(Weapon2.HasPart<Food>());
		if (num4 != 0)
		{
			return num4;
		}
		int num5 = Weapon1.HasPart<Armor>().CompareTo(Weapon2.HasPart<Armor>());
		if (num5 != 0)
		{
			return num5;
		}
		int num6 = Weapon1.HasEffectDescendedFrom<IBusted>().CompareTo(Weapon2.HasEffectDescendedFrom<IBusted>());
		if (num6 != 0)
		{
			return num6;
		}
		int num7 = (Weapon1.HasTag("MeleeWeapon") || Weapon1.HasTag("NaturalGear")).CompareTo(Weapon2.HasTag("MeleeWeapon") || Weapon2.HasTag("NaturalGear"));
		if (num7 != 0)
		{
			return -num7;
		}
		int num8 = Weapon1.HasTag("UndesirableWeapon").CompareTo(Weapon2.HasTag("UndesirableWeapon"));
		if (num8 != 0)
		{
			return num8;
		}
		double num9 = PreciseWeaponScore(Weapon1, POV);
		double value = PreciseWeaponScore(Weapon2, POV);
		return -num9.CompareTo(value);
	}

	public static bool IsNewWeaponBetter(GameObject NewWeapon, GameObject OldWeapon, GameObject POV)
	{
		return CompareWeapons(NewWeapon, OldWeapon, POV) < 0;
	}

	public bool IsNewWeaponBetter(GameObject NewWeapon, GameObject OldWeapon)
	{
		return IsNewWeaponBetter(NewWeapon, OldWeapon, ParentObject);
	}

	public static double PreciseMissileWeaponScore(GameObject obj, GameObject who = null)
	{
		if (obj == null)
		{
			return 0.0;
		}
		MissileWeapon part = obj.GetPart<MissileWeapon>();
		if (part == null)
		{
			return 0.0;
		}
		GetMissileWeaponPerformanceEvent getMissileWeaponPerformanceEvent = GetMissileWeaponPerformanceEvent.GetFor(who, obj);
		if (getMissileWeaponPerformanceEvent.BaseDamage.IsNullOrEmpty())
		{
			return 0.0;
		}
		int num = getMissileWeaponPerformanceEvent.BaseDamage.RollMinCached();
		int num2 = getMissileWeaponPerformanceEvent.BaseDamage.RollMaxCached();
		double num3 = num * 2 + num2;
		int num4 = getMissileWeaponPerformanceEvent.Penetration - 1;
		double num5 = ((num4 < 1) ? (num3 / 2.0) : (num3 * (double)num4));
		double num6 = 50 - part.WeaponAccuracy;
		if (who != null)
		{
			num6 += (double)(who.StatMod(part.Modifier) * 3);
			if (part.IsSkilled(who))
			{
				num6 += 6.0;
			}
		}
		num6 = Math.Min(Math.Max(num6, 5.0), 100.0);
		double num7 = num5 * (double)part.ShotsPerAction * num6 / 50.0;
		if (obj.HasTag("Storied"))
		{
			num7 += 5.0;
		}
		if (obj.HasTag("NaturalGear"))
		{
			num7++;
		}
		string usesSlots = obj.UsesSlots;
		if (CanFireAllMissileWeaponsEvent.Check(who))
		{
			if (!usesSlots.IsNullOrEmpty())
			{
				num7 = num7 * 2.0 / (double)(usesSlots.CachedCommaExpansion().Count + 1);
			}
			else
			{
				int slotsRequiredFor = obj.GetSlotsRequiredFor(who, part.SlotType);
				if (slotsRequiredFor > 1)
				{
					num7 = num7 * 2.0 / (double)(slotsRequiredFor + 1);
				}
			}
		}
		else if (!usesSlots.IsNullOrEmpty())
		{
			num7 = num7 * 3.0 / (double)(usesSlots.CachedCommaExpansion().Count + 2);
		}
		else
		{
			int slotsRequiredFor2 = obj.GetSlotsRequiredFor(who, part.SlotType);
			if (slotsRequiredFor2 > 1)
			{
				num7 = num7 * 4.0 / (double)(slotsRequiredFor2 + 3);
			}
		}
		if (num6 != 50.0)
		{
			num7 += (num6 - 50.0) / 5.0;
		}
		string tag = obj.GetTag("AdjustMissileWeaponScore");
		if (tag != null)
		{
			num7 += Convert.ToDouble(tag);
		}
		if (obj.HasRegisteredEvent("AdjustMissileWeaponScore"))
		{
			int num8 = (int)Math.Round(num7, MidpointRounding.AwayFromZero);
			Event obj2 = Event.New("AdjustMissileWeaponScore", "Score", num8, "OriginalScore", num7, "User", who);
			obj.FireEvent(obj2);
			int intParameter = obj2.GetIntParameter("Score");
			if (num8 != intParameter)
			{
				num7 += (double)(intParameter - num8);
			}
		}
		return num7;
	}

	public static int MissileWeaponScore(GameObject obj, GameObject who = null)
	{
		return (int)Math.Round(PreciseMissileWeaponScore(obj, who), MidpointRounding.AwayFromZero);
	}

	public static int CompareMissileWeapons(GameObject Weapon1, GameObject Weapon2, GameObject POV)
	{
		if (Weapon1 == Weapon2)
		{
			return 0;
		}
		if (Weapon1 == null)
		{
			return 1;
		}
		if (Weapon2 == null)
		{
			return -1;
		}
		int num = Weapon1.HasTagOrProperty("AlwaysEquipAsMissileWeapon").CompareTo(Weapon2.HasTagOrProperty("AlwaysEquipAsMissileWeapon"));
		if (num != 0)
		{
			return -num;
		}
		return -PreciseMissileWeaponScore(Weapon1, POV).CompareTo(PreciseMissileWeaponScore(Weapon2, POV));
	}

	public static bool IsNewMissileWeaponBetter(GameObject NewWeapon, GameObject OldWeapon, GameObject POV)
	{
		return CompareMissileWeapons(NewWeapon, OldWeapon, POV) < 0;
	}

	public bool IsNewMissileWeaponBetter(GameObject NewWeapon, GameObject OldWeapon)
	{
		return IsNewMissileWeaponBetter(NewWeapon, OldWeapon, ParentObject);
	}

	public static double PreciseArmorScore(GameObject obj, GameObject who = null)
	{
		if (obj == null)
		{
			return 0.0;
		}
		Armor part = obj.GetPart<Armor>();
		if (part == null)
		{
			return 0.0;
		}
		double num = part.Acid + part.Elec + part.Cold + part.Heat + part.Strength * 5 + part.Intelligence + part.Ego * 2 + part.ToHit * 10 - part.SpeedPenalty * 2 + part.CarryBonus / 5;
		double num2 = part.AV;
		double num3 = part.DV;
		if (who != null && part.WornOn != "*")
		{
			Body body = who.Body;
			if (body != null)
			{
				int partCount = body.GetPartCount(part.WornOn);
				if (partCount > 1)
				{
					num2 /= (double)partCount;
					num3 /= (double)partCount;
				}
			}
		}
		if (part.Agility != 0)
		{
			num += (double)part.Agility;
			num3 += (double)part.Agility * 0.5;
		}
		if (num2 > 0.0)
		{
			num += num2 * num2 * 20.0;
		}
		else if (num2 < 0.0)
		{
			num += num2 * 40.0;
		}
		if (num3 > 0.0)
		{
			num += num3 * num3 * 10.0;
		}
		else if (num3 < 0.0)
		{
			num += num3 * 20.0;
		}
		MoveCostMultiplier part2 = obj.GetPart<MoveCostMultiplier>();
		if (part2 != null)
		{
			num += (double)(-part2.Amount * 2);
		}
		EquipStatBoost part3 = obj.GetPart<EquipStatBoost>();
		if (part3 != null)
		{
			int num4 = 0;
			foreach (KeyValuePair<string, int> bonus in part3.GetBonusList())
			{
				int num5 = (Statistic.IsInverseBenefit(bonus.Key) ? (-bonus.Value) : bonus.Value);
				if (!bonus.Key.Contains("Resist"))
				{
					num5 *= 10;
				}
				num4 += num5;
			}
			if (num4 != 0)
			{
				num = ((part3.ChargeUse <= 0) ? (num + (double)(num4 * 2)) : (num + (double)num4));
			}
		}
		if (obj.HasTag("Storied"))
		{
			num += 5.0;
		}
		if (obj.HasTag("NaturalGear"))
		{
			num++;
		}
		string usesSlots = obj.UsesSlots;
		if (!usesSlots.IsNullOrEmpty())
		{
			num = num * 2.0 / (double)(usesSlots.CachedCommaExpansion().Count + 1);
		}
		if (obj.HasPart<Metal>())
		{
			num -= Math.Abs(num / 20.0);
		}
		return num;
	}

	public static int ArmorScore(GameObject obj, GameObject who = null)
	{
		return (int)Math.Round(PreciseArmorScore(obj, who), MidpointRounding.AwayFromZero);
	}

	public static int CompareArmors(GameObject Armor1, GameObject Armor2, GameObject POV)
	{
		if (Armor1 == Armor2)
		{
			return 0;
		}
		if (Armor1 == null)
		{
			return 1;
		}
		if (Armor2 == null)
		{
			return -1;
		}
		int num = Armor1.HasTagOrProperty("AlwaysEquipAsArmor").CompareTo(Armor2.HasTagOrProperty("AlwaysEquipAsArmor"));
		if (num != 0)
		{
			return -num;
		}
		return -PreciseArmorScore(Armor1, POV).CompareTo(PreciseArmorScore(Armor2, POV));
	}

	public static bool IsNewArmorBetter(GameObject NewArmor, GameObject OldArmor, GameObject POV)
	{
		return CompareArmors(NewArmor, OldArmor, POV) < 0;
	}

	public bool IsNewArmorBetter(GameObject NewArmor, GameObject OldArmor)
	{
		return IsNewArmorBetter(NewArmor, OldArmor, ParentObject);
	}

	public static double PreciseShieldScore(GameObject obj, GameObject who = null)
	{
		if (obj == null)
		{
			return 0.0;
		}
		Shield part = obj.GetPart<Shield>();
		if (part == null)
		{
			return 0.0;
		}
		double num = -part.SpeedPenalty;
		if (part.AV > 0)
		{
			num += (double)(part.AV * part.AV);
		}
		if (part.DV < 0)
		{
			num += (double)(part.DV * 2);
		}
		else if (part.DV > 0)
		{
			num += (double)(part.DV * part.DV);
		}
		if (obj.HasTag("Storied"))
		{
			num += 5.0;
		}
		if (obj.HasTag("NaturalGear"))
		{
			num++;
		}
		string usesSlots = obj.UsesSlots;
		if (!usesSlots.IsNullOrEmpty())
		{
			num = num * 2.0 / (double)(usesSlots.CachedCommaExpansion().Count + 1);
		}
		if (part.WornOn != "Hand")
		{
			num = num * 5.0 / 4.0;
		}
		if (obj.HasPart<Metal>() && num > 0.0)
		{
			num--;
		}
		return num;
	}

	public static int ShieldScore(GameObject obj, GameObject who = null)
	{
		return (int)Math.Round(PreciseShieldScore(obj, who), MidpointRounding.AwayFromZero);
	}

	public static int CompareShields(GameObject Shield1, GameObject Shield2, GameObject POV)
	{
		if (Shield1 == Shield2)
		{
			return 0;
		}
		if (Shield1 == null)
		{
			return 1;
		}
		if (Shield2 == null)
		{
			return -1;
		}
		return -PreciseShieldScore(Shield1, POV).CompareTo(PreciseShieldScore(Shield2, POV));
	}

	public static bool IsNewShieldBetter(GameObject NewShield, GameObject OldShield, GameObject POV)
	{
		return CompareShields(NewShield, OldShield, POV) < 0;
	}

	public bool IsNewShieldBetter(GameObject NewShield, GameObject OldShield)
	{
		return IsNewShieldBetter(NewShield, OldShield, ParentObject);
	}

	public static int CompareGear(GameObject obj1, GameObject obj2, GameObject POV)
	{
		if (obj1 == obj2)
		{
			return 0;
		}
		if (obj1 == null)
		{
			return 1;
		}
		if (obj2 == null)
		{
			return -1;
		}
		int num = obj1.HasEffectDescendedFrom<IBusted>().CompareTo(obj2.HasEffectDescendedFrom<IBusted>());
		if (num != 0)
		{
			return num;
		}
		int num2 = CompareArmors(obj1, obj2, POV);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = CompareMissileWeapons(obj1, obj2, POV);
		if (num3 != 0)
		{
			return num3;
		}
		int num4 = CompareShields(obj1, obj2, POV);
		if (num4 != 0)
		{
			return num4;
		}
		int num5 = CompareWeapons(obj1, obj2, POV);
		if (num5 != 0)
		{
			return num5;
		}
		int num6 = obj1.HasPartDescendedFrom<ILightSource>().CompareTo(obj2.HasPartDescendedFrom<ILightSource>());
		if (num6 != 0)
		{
			return -num6;
		}
		if (obj1.HasPart<Commerce>() && obj2.HasPart<Commerce>())
		{
			int num7 = obj1.ValueEach.CompareTo(obj2.ValueEach);
			if (num7 != 0)
			{
				return -num7;
			}
		}
		int num8 = obj1.GetTier().CompareTo(obj2.GetTier());
		if (num8 != 0)
		{
			return -num8;
		}
		if (!obj1.HasIntProperty("SortFudge"))
		{
			obj1.SetIntProperty("SortFudge", Stat.Random(0, 32768));
		}
		if (!obj2.HasIntProperty("SortFudge"))
		{
			obj2.SetIntProperty("SortFudge", Stat.Random(0, 32768));
		}
		return obj1.GetIntProperty("SortFudge").CompareTo(obj2.GetIntProperty("SortFudge"));
	}

	public static bool IsNewGearBetter(GameObject NewGear, GameObject OldGear, GameObject POV)
	{
		return CompareGear(NewGear, OldGear, POV) < 0;
	}

	public bool IsNewGearBetter(GameObject NewGear, GameObject OldGear)
	{
		return IsNewGearBetter(NewGear, OldGear, ParentObject);
	}

	public void CleanNaturalGear(Inventory Inventory = null)
	{
		if (Inventory == null)
		{
			Inventory = ParentObject.Inventory;
			if (Inventory == null)
			{
				return;
			}
		}
		List<GameObject> list = Event.NewGameObjectList();
		foreach (GameObject item in Inventory.GetObjectsDirect())
		{
			if (item.HasTag("NaturalGear"))
			{
				list.Add(item);
			}
		}
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[i].Obliterate();
		}
	}

	public void PerformEquip(bool Silent = false, bool DoPrimaryChoice = true, GameObject Avoid = null)
	{
		PerformReequip(Silent, DoPrimaryChoice, Initial: true, Avoid);
	}

	public void PerformReequip(bool Silent = false, bool DoPrimaryChoice = true, bool Initial = false, GameObject Avoid = null)
	{
		try
		{
			if (ParentObject.CurrentCell != null && !ParentObject.CanMoveExtremities())
			{
				return;
			}
			SharedGearSorter.Setup(ParentObject);
			SharedWeaponSorter.Setup(ParentObject);
			bool flag = ParentObject.IsMerchant();
			Body body = ParentObject.Body;
			Inventory inventory = ParentObject.Inventory;
			if (body != null && inventory != null)
			{
				try
				{
					if (The.Player != null && !ParentObject.HasTag("ExcludeGrenadeHack") && The.Player.Stat("Level") < 3 && IsHostileTowards(The.Player))
					{
						foreach (GameObject item in inventory.GetObjectsWithTag("Grenade"))
						{
							item.Destroy();
						}
					}
				}
				catch (Exception ex)
				{
					LogErrorInEditor(ex.ToString());
				}
				List<BodyPart> parts = body.GetParts();
				int LightSources = 0;
				GameObject Shield = null;
				foreach (BodyPart item2 in parts)
				{
					if (item2.Equipped != null && item2.Equipped.HasPartDescendedFrom<ILightSource>())
					{
						LightSources++;
					}
				}
				foreach (BodyPart item3 in parts)
				{
					if (item3.Equipped != null && item3.Equipped.HasPart<Shield>())
					{
						Shield = item3.Equipped;
						break;
					}
				}
				List<GameObject> list = Event.NewGameObjectList();
				inventory.GetObjects(list);
				if (Avoid != null)
				{
					list.Remove(Avoid);
				}
				List<GameObject> list2 = Event.NewGameObjectList();
				foreach (GameObject item4 in list)
				{
					if (item4.EquipAsDefaultBehavior())
					{
						list2.Add(item4);
					}
				}
				foreach (GameObject item5 in list2)
				{
					list.Remove(item5);
				}
				if (list2.Count > 0)
				{
					foreach (BodyPart item6 in parts)
					{
						List<GameObject> equipmentListForSlot = inventory.GetEquipmentListForSlot(item6.Type, list2, RequireDesirable: true, RequirePossible: true, SkipSort: true);
						if (equipmentListForSlot != null && equipmentListForSlot.Count > 0)
						{
							if (equipmentListForSlot.Count > 1)
							{
								equipmentListForSlot.Sort(SharedGearSorter);
							}
							GameObject Object = equipmentListForSlot[0];
							if (!Equip(Object, item6, list2, null, null, ref LightSources, ref Shield, Silent: true) && GameObject.Validate(ref Object))
							{
								MetricsManager.LogError("failed to equip default gear " + Object.DebugName + " on " + ParentObject.DebugName + "'s " + item6.Description + " (EQ:" + item6.Equipped?.DebugName + ",DF:" + item6.DefaultBehavior?.DebugName + "), destructing");
								Object.Obliterate();
							}
						}
					}
				}
				string propertyOrTag = ParentObject.GetPropertyOrTag("NoEquip");
				List<string> list3 = (propertyOrTag.IsNullOrEmpty() ? null : new List<string>(propertyOrTag.CachedCommaExpansion()));
				List<GameObject> list4 = inventory.GetEquipmentListForSlot("Hand", list, RequireDesirable: true, RequirePossible: true, SkipSort: true) ?? Event.NewGameObjectList();
				List<GameObject> list5 = null;
				for (int num = list4.Count - 1; num >= 0; num--)
				{
					GameObject gameObject = list4[num];
					if (gameObject.HasPart<Shield>())
					{
						if (list5 == null)
						{
							list5 = Event.NewGameObjectList(list4);
						}
						list4.Remove(gameObject);
					}
					else if (gameObject.HasPart<MissileWeapon>() || TreatAsThrownWeapon(gameObject))
					{
						list4.Remove(gameObject);
						list5?.Remove(gameObject);
					}
					else if (!gameObject.HasPropertyOrTag("ShowMeleeWeaponStats") && !gameObject.HasTag("MeleeWeapon") && !gameObject.HasPropertyOrTag("AlwaysEquipAsWeapon") && gameObject.GetInventoryCategory() != "Melee Weapons" && !gameObject.HasPartDescendedFrom<ILightSource>())
					{
						list4.Remove(gameObject);
						list5?.Remove(gameObject);
					}
				}
				BodyPart bodyPart = null;
				if (list4.Count > 1)
				{
					list4.Sort(SharedWeaponSorter);
				}
				if (list5 != null && list5.Count > 1)
				{
					list5.Sort(SharedGearSorter);
				}
				BodyPart bodyPart2 = null;
				foreach (BodyPart item7 in parts)
				{
					if (!item7.Primary || !(item7.Type == "Hand"))
					{
						continue;
					}
					if (item7.Equipped != null && !item7.Equipped.CanBeUnequipped())
					{
						break;
					}
					bodyPart2 = item7;
					GameObject gameObject2 = item7.Equipped ?? item7.DefaultBehavior;
					foreach (GameObject item8 in list4)
					{
						if ((list3 == null || !list3.Contains(item8.Blueprint)) && (ParentObject.IsPlayer() || !item8.HasPropertyOrTag("NoAIEquip")) && (!flag || !item8.HasProperty("_stock")) && IsNewWeaponBetter(item8, gameObject2))
						{
							gameObject2 = item8;
							break;
						}
					}
					Equip(gameObject2, item7, list4, list5, list, ref LightSources, ref Shield, Silent, Initial);
					break;
				}
				if (list4.Count > 0 && ParentObject.HasSkill("Multiweapon_Fighting"))
				{
					foreach (BodyPart item9 in parts)
					{
						if (item9 == bodyPart2 || !(item9.Type == "Hand"))
						{
							continue;
						}
						bodyPart = item9;
						if (item9.Equipped != null && !item9.Equipped.CanBeUnequipped())
						{
							break;
						}
						GameObject gameObject3 = item9.Equipped ?? item9.DefaultBehavior;
						foreach (GameObject item10 in list4)
						{
							if ((list3 == null || !list3.Contains(item10.Blueprint)) && (ParentObject.IsPlayer() || !item10.HasPropertyOrTag("NoAIEquip")) && (!flag || !item10.HasProperty("_stock")) && IsNewWeaponBetter(item10, gameObject3))
							{
								gameObject3 = item10;
								break;
							}
						}
						Equip(gameObject3, item9, list4, list5, list, ref LightSources, ref Shield, Silent, Initial);
						break;
					}
				}
				BodyPart bodyPart3 = null;
				foreach (BodyPart item11 in parts)
				{
					if (item11.Type == "Hand" && !item11.Primary)
					{
						bodyPart3 = item11;
					}
				}
				foreach (BodyPart item12 in parts)
				{
					GameObject gameObject4 = null;
					GameObject gameObject5 = item12.Equipped ?? item12.DefaultBehavior;
					List<GameObject> list6 = null;
					List<GameObject> secondaryActiveList = null;
					List<GameObject> tertiaryActiveList = null;
					List<GameObject> list7 = null;
					if (item12.Type == "Hand")
					{
						if (item12.Primary || item12 == bodyPart)
						{
							continue;
						}
						list7 = list5 ?? list4;
						list6 = list4;
						secondaryActiveList = list5;
						tertiaryActiveList = list;
					}
					else
					{
						list7 = inventory.GetEquipmentListForSlot(item12.Type, list, RequireDesirable: true, RequirePossible: true, SkipSort: true);
						if (list7 != null && list7.Count > 1)
						{
							list7.Sort(SharedGearSorter);
						}
						list6 = list;
					}
					if (list7 == null || list7.Count == 0)
					{
						continue;
					}
					foreach (GameObject item13 in list7)
					{
						if ((list3 != null && list3.Contains(item13.Blueprint)) || (!ParentObject.IsPlayer() && item13.HasPropertyOrTag("NoAIEquip")) || (flag && item13.HasProperty("_stock")) || item13.HasPart<Food>() || (item13.HasPart<Shield>() && (!ParentObject.HasSkill("Shield_Block") || (Shield != null && (gameObject5 != Shield || !IsNewShieldBetter(item13, Shield) || !IsNewGearBetter(item13, gameObject5))))))
						{
							continue;
						}
						if (item12.Type == "Hand")
						{
							if (item12 == bodyPart3 && LightSources <= 0 && item13.HasPartDescendedFrom<ILightSource>() && item12.Equipped == null)
							{
								gameObject4 = item13;
								break;
							}
							if (!ParentObject.IsPlayer() && IsNewGearBetter(item13, gameObject5))
							{
								gameObject4 = item13;
								break;
							}
							continue;
						}
						if (item12.Type == "Thrown Weapon")
						{
							if (item13.Physics != null && item13.Physics.Category == "Grenades" && IsNewGearBetter(item13, gameObject5))
							{
								gameObject4 = item13;
								break;
							}
							if (TreatAsThrownWeapon(item13) && !item13.HasTagOrProperty("NoAIEquip") && (list3 == null || !list3.Contains(item13.Blueprint)) && IsNewGearBetter(item13, gameObject5))
							{
								gameObject4 = item13;
								break;
							}
							continue;
						}
						MissileWeapon part = item13.GetPart<MissileWeapon>();
						if (part != null)
						{
							if ((!part.FiresManually || part.ValidSlotType(item12.Type)) && IsNewMissileWeaponBetter(item13, gameObject5))
							{
								gameObject4 = item13;
								break;
							}
						}
						else if (item12.Type != "Missile Weapon" && IsNewGearBetter(item13, gameObject5))
						{
							gameObject4 = item13;
							break;
						}
					}
					Equip(gameObject4, item12, list6, secondaryActiveList, tertiaryActiveList, ref LightSources, ref Shield, Silent, Initial);
				}
			}
			CleanNaturalGear(inventory);
			CommandReloadEvent.Execute(ParentObject);
			try
			{
				if (DoPrimaryChoice && ParentObject != null && body != null && !ParentObject.IsPlayer())
				{
					BodyPart bestPart = null;
					GameObject bestWeapon = null;
					body.ForeachPart((BodyPart p) =>
					{
						if (p.Equipped != null)
						{
							if (IsNewWeaponBetter(p.Equipped, bestWeapon))
							{
								bestPart = p;
								bestWeapon = p.Equipped;
							}
						}
						else if (p.DefaultBehavior != null && IsNewWeaponBetter(p.DefaultBehavior, bestWeapon))
						{
							bestPart = p;
							bestWeapon = p.DefaultBehavior;
						}
					});
					if (bestPart != null)
					{
						bestPart.SetAsPreferredDefault();
					}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Select primary limb", x);
			}
			if (Initial)
			{
				DidInitialEquipEvent.Send(ParentObject);
			}
			else
			{
				DidReequipEvent.Send(ParentObject);
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("Exception during Brain::PerformReequip", x2);
		}
	}

	private bool Equip(GameObject Object, BodyPart Part, List<GameObject> PrimaryActiveList, List<GameObject> SecondaryActiveList, List<GameObject> TertiaryActiveList, ref int LightSources, ref GameObject Shield, bool Silent = false, bool Initial = true)
	{
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		bool flag = Object.EquipAsDefaultBehavior();
		GameObject Object2;
		if (flag)
		{
			Object2 = Part.DefaultBehavior;
		}
		else
		{
			Object2 = Part.Equipped;
			if (Object2 != null)
			{
				if (Object2 == Object)
				{
					return false;
				}
				if (Object2.SameAs(Object))
				{
					return false;
				}
				Part.TryUnequip(Silent);
			}
			if (Part.Equipped != null)
			{
				return false;
			}
		}
		if (Part.DefaultBehavior != null)
		{
			if (Part.DefaultBehavior == Object)
			{
				return false;
			}
			if (Part.DefaultBehavior.SameAs(Object))
			{
				return false;
			}
		}
		Event obj = (Initial ? eCommandEquipObjectFree : eCommandEquipObject);
		obj.SetParameter("Object", Object);
		obj.SetParameter("BodyPart", Part);
		obj.SetSilent(Silent);
		bool flag2 = ParentObject.FireEvent(obj);
		if (flag2)
		{
			GameObject gameObject = (flag ? Part.DefaultBehavior : Part.Equipped);
			if (gameObject != null && gameObject != Object2)
			{
				if (gameObject.HasPartDescendedFrom<ILightSource>())
				{
					LightSources++;
				}
				if (gameObject.HasPart<Shield>())
				{
					Shield = gameObject;
				}
				if (Object == gameObject)
				{
					PrimaryActiveList?.Remove(Object);
					SecondaryActiveList?.Remove(Object);
					TertiaryActiveList?.Remove(Object);
				}
				if (Object2 != null)
				{
					if (flag)
					{
						if (GameObject.Validate(ref Object2))
						{
							Object2.Obliterate();
							return flag2;
						}
					}
					else if (GameObject.Validate(ref Object2))
					{
						if (Object2.InInventory == ParentObject)
						{
							PrimaryActiveList?.Add(Object2);
							SecondaryActiveList?.Add(Object2);
							TertiaryActiveList?.Add(Object2);
						}
						if (Object2.HasPartDescendedFrom<ILightSource>())
						{
							LightSources--;
						}
						if (Object2.HasPart<Shield>() && Object2 == Shield)
						{
							Shield = null;
						}
					}
				}
			}
		}
		return flag2;
	}

	public Cell MovingTo()
	{
		for (int num = Goals.Items.Count - 1; num >= 0; num--)
		{
			if (Goals.Items[num] is MoveTo moveTo)
			{
				Cell destinationCell = moveTo.GetDestinationCell();
				if (destinationCell != null)
				{
					return destinationCell;
				}
			}
		}
		for (int num2 = Goals.Items.Count - 1; num2 >= 0; num2--)
		{
			if (Goals.Items[num2] is Step step)
			{
				Cell destinationCell2 = step.GetDestinationCell();
				if (destinationCell2 != null)
				{
					return destinationCell2;
				}
			}
		}
		return null;
	}

	public bool IsFleeing()
	{
		for (int num = Goals.Items.Count - 1; num >= 0; num--)
		{
			if (Goals.Items[num].IsFleeing())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFactionMember(string Faction)
	{
		if (Allegiance.TryGetValue(Faction, out var Value) && Value > 0)
		{
			return true;
		}
		return false;
	}

	public void WantToReequip()
	{
		if (!ParentObject.IsPlayer())
		{
			DoReequip = true;
		}
	}

	public static bool PartyMemberOrder(KeyValuePair<int, PartyMember> KV)
	{
		GameObject Object = KV.Value.Reference.Object ?? GameObject.FindByID(KV.Key);
		if (!GameObject.Validate(ref Object))
		{
			return true;
		}
		if (KV.Value.Flags.HasBit(8388608))
		{
			return true;
		}
		return false;
	}

	[WishCommand("allfeelings", null)]
	public static void WishAllFeelings()
	{
		WriteFeelingSamples();
	}

	[WishCommand("allfeelinglevels", null)]
	public static void WishAllFeelingLevels()
	{
		WriteFeelingSamples(Level: true);
	}

	private static void WriteFeelingSamples(bool Level = false)
	{
		try
		{
			List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
			List<(GameObjectBlueprint, string)> list = new List<(GameObjectBlueprint, string)>();
			foreach (GameObjectBlueprint item2 in blueprintList)
			{
				if (item2.HasPart("Brain") && !item2.IsBaseBlueprint())
				{
					Loading.SetLoadingStatus("Sampling " + item2.Name + "...");
					GameObject gameObject = null;
					string item;
					try
					{
						gameObject = item2.createSample();
						item = (Level ? Enum.GetName(typeof(FeelingLevel), gameObject.Brain.GetFeelingLevel(The.Player)) : gameObject.Brain.GetFeeling(The.Player).ToString());
					}
					catch (Exception)
					{
						item = "invalid";
					}
					list.Add((item2, item));
					gameObject?.Pool();
				}
			}
			list.Sort(((GameObjectBlueprint Blueprint, string Value) a, (GameObjectBlueprint Blueprint, string Value) b) => string.Compare(a.Blueprint.Name, b.Blueprint.Name, StringComparison.Ordinal));
			string text = (Level ? "AllFeelingLevels.txt" : "AllFeelings.txt");
			string text2 = DataManager.SavePath("Trash/Text/");
			Directory.CreateDirectory(text2);
			using (StreamWriter streamWriter = new StreamWriter(text2 + text))
			{
				foreach (var item3 in list)
				{
					streamWriter.WriteLine(item3.Item1.Name + ": " + item3.Item2);
				}
			}
			Popup.Show(list.Count + " feelings written to " + text + " in " + text2 + "!");
		}
		finally
		{
			Loading.SetLoadingStatus(null);
		}
	}
}
