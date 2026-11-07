using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Newtonsoft.Json;
using Occult.Engine.CodeGeneration;
using Unity.Profiling;
using XRL.Core;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
[GeneratePoolingPartial(Capacity = 128)]
[GenerateSerializationPartial(PreWrite = "ValidateGameObjects")]
public class Physics : IPart, IContextRelationManager
{
	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	private static readonly IPartPool PhysicsPool = new IPartPool(128);

	public static readonly string ATTACK_COMMAND = "Attack";

	public const int BASE_TEMPERATURE = 25;

	public const int PHYSICS_FLAG_SOLID = 1;

	public const int PHYSICS_FLAG_TAKEABLE = 2;

	public const int PHYSICS_FLAG_IS_REAL = 4;

	public const int PHYSICS_FLAG_USES_TWO_SLOTS = 8;

	public const int PHYSICS_FLAG_LAST_DAMAGE_ACCIDENTAL = 16;

	public const int PHYSICS_FLAG_WAS_FROZEN = 32;

	public const int PHYSICS_FLAG_WAS_AFLAME = 64;

	public const int PHYSICS_FLAG_ORGANIC = 128;

	public const int PHYSICS_FLAG_WAS_FREEZING = 256;

	public int Flags = 2;

	[NonSerialized]
	private int AmbientCache = int.MinValue;

	public string Category = "Unknown";

	public int _Weight;

	public int _Temperature = 25;

	public int FlameTemperature = 350;

	public int VaporTemperature = 10000;

	public int FreezeTemperature;

	public int BrittleTemperature = -100;

	public int BaseElectricalConductivity;

	public string Owner;

	public float SpecificHeat = 1f;

	public Cell _CurrentCell;

	public GameObject _InInventory;

	public GameObject _Equipped;

	public GameObject LastDamagedBy;

	public GameObject LastWeaponDamagedBy;

	public GameObject LastProjectileDamagedBy;

	public GameObject InflamedBy;

	public string LastDamagedByType = "";

	public string LastDeathReason = "";

	public string LastDeathCategory = "";

	public string LastThirdPersonDeathReason = "";

	[NonSerialized]
	public string ConfusedName;

	[NonSerialized]
	private int lastPushSegment = int.MinValue;

	[NonSerialized]
	private int lastPushCount;

	[NonSerialized]
	private static List<GameObject> DischargeObjects = new List<GameObject>();

	[NonSerialized]
	private static List<string> PassingBy = new List<string>();

	private static Event eBeforePhysicsRejectObjectEntringCell = new Event("BeforePhysicsRejectObjectEntringCell", "Object", (object)null, "Actual", 1);

	private static readonly ProfilerMarker s_PhysicsUpdateTemperature = new ProfilerMarker("s_PhysicsUpdateTemperature");

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override IPartPool Pool => PhysicsPool;

	[JsonIgnore]
	public bool Solid
	{
		get
		{
			return (Flags & 1) == 1;
		}
		set
		{
			Flags = (value ? (Flags | 1) : (Flags & -2));
		}
	}

	[JsonIgnore]
	public bool Takeable
	{
		get
		{
			return (Flags & 2) == 2;
		}
		set
		{
			Flags = (value ? (Flags | 2) : (Flags & -3));
		}
	}

	[JsonIgnore]
	public bool IsReal
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

	[JsonIgnore]
	public bool UsesTwoSlots
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

	[JsonIgnore]
	public bool LastDamageAccidental
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

	[JsonIgnore]
	public bool WasFreezing
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

	[JsonIgnore]
	public bool WasFrozen
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

	[JsonIgnore]
	public bool WasAflame
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

	[JsonIgnore]
	public bool Organic
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

	[JsonIgnore]
	public override int Priority => 90000;

	[JsonIgnore]
	public int AmbientTemperature
	{
		get
		{
			if (AmbientCache != int.MinValue)
			{
				return AmbientCache;
			}
			Cell cell = CurrentCell ?? ParentObject.GetCurrentCell();
			if (cell != null && cell.ParentZone != null)
			{
				if (ShouldRestAtAmbient())
				{
					AmbientCache = cell.ParentZone.BaseTemperature;
				}
				else if (cell.ParentZone.BaseTemperature > 25 && ParentObject.Stat("HeatResistance") > 0)
				{
					AmbientCache = Math.Max(25, cell.ParentZone.BaseTemperature - 4 * ParentObject.Stat("HeatResistance"));
				}
				else if (cell.ParentZone.BaseTemperature < 25 && ParentObject.Stat("ColdResistance") > 0)
				{
					AmbientCache = Math.Min(25, cell.ParentZone.BaseTemperature + ParentObject.Stat("ColdResistance"));
				}
				else
				{
					AmbientCache = cell.ParentZone.BaseTemperature;
				}
			}
			else
			{
				AmbientCache = 25;
			}
			return AmbientCache;
		}
	}

	[JsonIgnore]
	public int Temperature
	{
		get
		{
			return _Temperature;
		}
		set
		{
			if (SpecificHeat != 0f || Temperature == 25)
			{
				_Temperature = Math.Clamp(value, -10000, 10000);
			}
		}
	}

	public int IntrinsicWeight
	{
		get
		{
			return (int)GetIntrinsicWeight();
		}
		set
		{
			_Weight = value;
		}
	}

	public int Weight
	{
		get
		{
			return (int)GetWeight();
		}
		set
		{
			_Weight = value;
		}
	}

	public int IntrinsicWeightEach => (int)GetIntrinsicWeightEach();

	public int WeightEach => (int)GetWeightEach();

	public string VaporObject => ParentObject.GetTag("VaporObject", "");

	[JsonIgnore]
	public Cell CurrentCell
	{
		get
		{
			return _CurrentCell;
		}
		set
		{
			if (_CurrentCell != null && _CurrentCell != value && _CurrentCell.Objects.Contains(ParentObject))
			{
				try
				{
					_CurrentCell.RemoveObject(ParentObject);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("CurrentCell failsafe remove", x);
				}
			}
			_CurrentCell = value;
			if (value != null)
			{
				InInventory = null;
				Equipped = null;
			}
		}
	}

	public Zone CurrentZone => CurrentCell?.ParentZone;

	public GameObject InInventory
	{
		get
		{
			return _InInventory;
		}
		set
		{
			if (_InInventory != null && _InInventory != value)
			{
				try
				{
					Inventory inventory = _InInventory.Inventory;
					if (inventory != null && inventory.Objects.Contains(ParentObject))
					{
						inventory.RemoveObject(ParentObject);
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogException("InInventory failsafe remove", x);
				}
			}
			_InInventory = value;
			if (value != null)
			{
				Equipped = null;
				CurrentCell = null;
				if (Sidebar.CurrentTarget == ParentObject)
				{
					Sidebar.CurrentTarget = null;
				}
			}
		}
	}

	public GameObject Equipped
	{
		get
		{
			return _Equipped;
		}
		set
		{
			GameObject equipped = _Equipped;
			if (equipped != null)
			{
				if (equipped == value)
				{
					MetricsManager.LogWarning($"Physics.Equipped assigned to identical object: {value}.\n{new StackTrace()}");
					return;
				}
				_Equipped = null;
				BeforeUnequippedEvent.Send(ParentObject, equipped);
				UnequippedEvent.Send(ParentObject, equipped);
			}
			_Equipped = value;
			if (value != null)
			{
				InInventory = null;
				CurrentCell = null;
				if (Sidebar.CurrentTarget == ParentObject)
				{
					Sidebar.CurrentTarget = null;
				}
			}
		}
	}

	[JsonIgnore]
	public int ElectricalConductivity
	{
		get
		{
			return GetElectricalConductivity();
		}
		set
		{
			BaseElectricalConductivity = value;
		}
	}

	[JsonIgnore]
	public int Conductivity
	{
		get
		{
			return GetElectricalConductivity();
		}
		set
		{
			BaseElectricalConductivity = value;
		}
	}

	public string UsesSlots
	{
		get
		{
			return ParentObject.GetTagOrStringProperty("UsesSlots");
		}
		set
		{
			ParentObject.SetStringProperty("UsesSlots", value);
		}
	}

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override void Reset()
	{
		base.Reset();
		Flags = 2;
		AmbientCache = int.MinValue;
		Category = "Unknown";
		_Weight = 0;
		_Temperature = 25;
		FlameTemperature = 350;
		VaporTemperature = 10000;
		FreezeTemperature = 0;
		BrittleTemperature = -100;
		BaseElectricalConductivity = 0;
		Owner = null;
		SpecificHeat = 1f;
		_CurrentCell = null;
		_InInventory = null;
		_Equipped = null;
		LastDamagedBy = null;
		LastWeaponDamagedBy = null;
		LastProjectileDamagedBy = null;
		InflamedBy = null;
		LastDamagedByType = "";
		LastDeathReason = "";
		LastDeathCategory = "";
		LastThirdPersonDeathReason = "";
		ConfusedName = null;
		lastPushSegment = int.MinValue;
		lastPushCount = 0;
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		ValidateGameObjects();
		Writer.WriteOptimized(Flags);
		Writer.WriteOptimized(Category);
		Writer.WriteOptimized(_Weight);
		Writer.WriteOptimized(_Temperature);
		Writer.WriteOptimized(FlameTemperature);
		Writer.WriteOptimized(VaporTemperature);
		Writer.WriteOptimized(FreezeTemperature);
		Writer.WriteOptimized(BrittleTemperature);
		Writer.WriteOptimized(BaseElectricalConductivity);
		Writer.WriteOptimized(Owner);
		Writer.Write(SpecificHeat);
		Writer.Write(_CurrentCell);
		Writer.WriteGameObject(_InInventory);
		Writer.WriteGameObject(_Equipped);
		Writer.WriteGameObject(LastDamagedBy);
		Writer.WriteGameObject(LastWeaponDamagedBy);
		Writer.WriteGameObject(LastProjectileDamagedBy);
		Writer.WriteGameObject(InflamedBy);
		Writer.WriteOptimized(LastDamagedByType);
		Writer.WriteOptimized(LastDeathReason);
		Writer.WriteOptimized(LastDeathCategory);
		Writer.WriteOptimized(LastThirdPersonDeathReason);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Flags = Reader.ReadOptimizedInt32();
		Category = Reader.ReadOptimizedString();
		_Weight = Reader.ReadOptimizedInt32();
		_Temperature = Reader.ReadOptimizedInt32();
		FlameTemperature = Reader.ReadOptimizedInt32();
		VaporTemperature = Reader.ReadOptimizedInt32();
		FreezeTemperature = Reader.ReadOptimizedInt32();
		BrittleTemperature = Reader.ReadOptimizedInt32();
		BaseElectricalConductivity = Reader.ReadOptimizedInt32();
		Owner = Reader.ReadOptimizedString();
		SpecificHeat = Reader.ReadSingle();
		_CurrentCell = Reader.ReadCell();
		_InInventory = Reader.ReadGameObject();
		_Equipped = Reader.ReadGameObject();
		LastDamagedBy = Reader.ReadGameObject();
		LastWeaponDamagedBy = Reader.ReadGameObject();
		LastProjectileDamagedBy = Reader.ReadGameObject();
		InflamedBy = Reader.ReadGameObject();
		LastDamagedByType = Reader.ReadOptimizedString();
		LastDeathReason = Reader.ReadOptimizedString();
		LastDeathCategory = Reader.ReadOptimizedString();
		LastThirdPersonDeathReason = Reader.ReadOptimizedString();
	}

	public bool ShouldRestAtAmbient()
	{
		if (ParentObject.IsWall() || ParentObject.IsScenery)
		{
			return !ParentObject.IsCombatObject();
		}
		return false;
	}

	public override void Attach()
	{
		ParentObject.Physics = this;
	}

	public override void Remove()
	{
		if (ParentObject?.Physics == this)
		{
			ParentObject.Physics = null;
		}
	}

	public double GetIntrinsicWeightEach()
	{
		double num = (double)_Weight + (double)ParentObject.GetBodyWeight();
		double num2 = num;
		if (ParentObject.WantEvent(GetIntrinsicWeightEvent.ID, MinEvent.CascadeLevel))
		{
			GetIntrinsicWeightEvent getIntrinsicWeightEvent = GetIntrinsicWeightEvent.FromPool(ParentObject, num, num2);
			ParentObject.HandleEvent(getIntrinsicWeightEvent);
			num2 = getIntrinsicWeightEvent.Weight;
		}
		if (ParentObject.WantEvent(AdjustWeightEvent.ID, MinEvent.CascadeLevel))
		{
			AdjustWeightEvent adjustWeightEvent = AdjustWeightEvent.FromPool(ParentObject, num, num2);
			ParentObject.HandleEvent(adjustWeightEvent);
			num2 = adjustWeightEvent.Weight;
		}
		return num2;
	}

	public double GetWeightEach()
	{
		double baseWeight = (double)_Weight + (double)ParentObject.GetBodyWeight();
		double num = GetIntrinsicWeightEach();
		if (ParentObject.WantEvent(GetExtrinsicWeightEvent.ID, MinEvent.CascadeLevel))
		{
			GetExtrinsicWeightEvent getExtrinsicWeightEvent = GetExtrinsicWeightEvent.FromPool(ParentObject, baseWeight, num);
			ParentObject.HandleEvent(getExtrinsicWeightEvent);
			num = getExtrinsicWeightEvent.Weight;
		}
		if (ParentObject.WantEvent(AdjustTotalWeightEvent.ID, MinEvent.CascadeLevel))
		{
			AdjustTotalWeightEvent adjustTotalWeightEvent = AdjustTotalWeightEvent.FromPool(ParentObject, baseWeight, num);
			ParentObject.HandleEvent(adjustTotalWeightEvent);
			num = adjustTotalWeightEvent.Weight;
		}
		return num;
	}

	public double GetIntrinsicWeight()
	{
		return GetIntrinsicWeightEach() * (double)ParentObject.Count;
	}

	public double GetWeight()
	{
		return GetWeightEach() * (double)ParentObject.Count;
	}

	public int GetIntrinsicWeightTimes(double Factor)
	{
		return (int)(GetIntrinsicWeight() * Factor);
	}

	public int GetWeightTimes(double Factor)
	{
		return (int)(GetWeight() * Factor);
	}

	public int GetElectricalConductivity(out GameObject ReductionObject, out string ReductionReason, GameObject Source = null, int Phase = 0)
	{
		return GetElectricalConductivityEvent.GetFor(ParentObject, out ReductionObject, out ReductionReason, int.MinValue, Source, Phase);
	}

	public int GetElectricalConductivity(GameObject Source = null, int Phase = 0)
	{
		GameObject ReductionObject;
		string ReductionReason;
		return GetElectricalConductivity(out ReductionObject, out ReductionReason, Source, Phase);
	}

	public bool IsFrozen()
	{
		return Temperature <= BrittleTemperature;
	}

	public bool IsFreezing()
	{
		return Temperature <= FreezeTemperature;
	}

	public bool IsAflame()
	{
		return Temperature >= FlameTemperature;
	}

	public bool IsVaporizing()
	{
		return Temperature >= VaporTemperature;
	}

	public void ValidateGameObjects()
	{
		GameObject.Validate(ref LastDamagedBy);
		GameObject.Validate(ref LastWeaponDamagedBy);
		GameObject.Validate(ref LastProjectileDamagedBy);
		GameObject.Validate(ref InflamedBy);
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Physics obj = base.DeepCopy(Parent, MapInv) as Physics;
		obj._CurrentCell = null;
		obj._Equipped = null;
		obj._InInventory = null;
		return obj;
	}

	public override bool SameAs(IPart p)
	{
		Physics physics = p as Physics;
		if (physics._Weight != _Weight)
		{
			return false;
		}
		if (physics.Solid != Solid)
		{
			return false;
		}
		if (physics.Category != Category)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public static bool IsMoveable(GameObject obj, bool Involuntary = true)
	{
		if (!obj.IsReal)
		{
			return false;
		}
		if (obj.IsScenery)
		{
			return false;
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return false;
		}
		if (Involuntary && !obj.CanBeInvoluntarilyMoved())
		{
			return false;
		}
		return true;
	}

	public bool Push(string Direction, int Force, int MaxDistance = 9999999, bool IgnoreGravity = false, bool Involuntary = true, GameObject Actor = null)
	{
		if (lastPushSegment != The.Game.Segments)
		{
			lastPushCount = 0;
		}
		else
		{
			if (lastPushCount >= 2)
			{
				return true;
			}
			lastPushCount++;
		}
		if (MaxDistance < 0)
		{
			return false;
		}
		if (CurrentCell == null)
		{
			return false;
		}
		if (Involuntary)
		{
			if (!IsMoveable(ParentObject))
			{
				return false;
			}
			int kineticResistance = ParentObject.GetKineticResistance();
			if (kineticResistance > Force)
			{
				return false;
			}
			if (kineticResistance < 0)
			{
				return false;
			}
		}
		List<string> adjacentDirections = Directions.GetAdjacentDirections(Direction, 2);
		if (50.in100())
		{
			adjacentDirections.Reverse();
		}
		adjacentDirections.Remove(Direction);
		adjacentDirections.Insert(0, Direction);
		foreach (string item in adjacentDirections)
		{
			Cell localCellFromDirection = CurrentCell.GetLocalCellFromDirection(item);
			if (localCellFromDirection == null)
			{
				return false;
			}
			if (localCellFromDirection.IsEmpty())
			{
				if (ParentObject.Move(Direction, Forced: true, System: false, IgnoreGravity))
				{
					return true;
				}
				continue;
			}
			List<GameObject> list = Event.NewGameObjectList();
			list.AddRange(localCellFromDirection.GetSolidObjects());
			foreach (GameObject item2 in localCellFromDirection.GetObjectsWithPartReadonly("Combat"))
			{
				if (!list.Contains(item2))
				{
					list.Add(item2);
				}
			}
			if (list.Count > 0)
			{
				int force = (Force * 9 / 10 - 10) / list.Count;
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					list[i].Push(Direction, force, MaxDistance - 1);
				}
			}
			if (ParentObject.Move(Direction, Forced: true, System: false, IgnoreGravity))
			{
				return true;
			}
		}
		return false;
	}

	public int Accelerate(int Force, string Direction = null, Cell Toward = null, Cell AwayFrom = null, string Type = null, GameObject Actor = null, bool Accidental = false, GameObject IntendedTarget = null, string BonusDamage = null, double DamageFactor = 1.0, bool SuspendFalling = true, bool OneShort = false, bool Repeat = false, bool BuiltOnly = true, bool MessageForInanimate = true, bool DelayForDisplay = true)
	{
		if (!IsMoveable(ParentObject))
		{
			return 0;
		}
		return AccelerateInternal(Force, Direction, Toward, AwayFrom, Type, Actor, Accidental, IntendedTarget, BonusDamage, DamageFactor, SuspendFalling, OneShort, Repeat, BuiltOnly, MessageForInanimate, DelayForDisplay);
	}

	protected int AccelerateInternal(int Force, string Direction = null, Cell Toward = null, Cell AwayFrom = null, string Type = null, GameObject Actor = null, bool Accidental = false, GameObject IntendedTarget = null, string BonusDamage = null, double DamageFactor = 1.0, bool SuspendFalling = true, bool OneShort = false, bool Repeat = false, bool BuiltOnly = true, bool MessageForInanimate = true, bool DelayForDisplay = true, bool Subsequent = false)
	{
		int num = 0;
		int myMatterPhase = 0;
		int num2 = 5;
		List<string> list = null;
		List<string> list2 = null;
		List<GameObject> list3 = null;
		if (num > 10000 || (num >= 100 && num > Force * 2))
		{
			MetricsManager.LogError("infinite loop on " + ParentObject.DebugName);
			return num;
		}
		bool flag = Subsequent && (MessageForInanimate || ParentObject.IsCreature);
		while (true)
		{
			if (CurrentCell == null)
			{
				return num;
			}
			int num3 = ParentObject.GetKineticResistance();
			if (num3 > Force)
			{
				return num;
			}
			if (num3 < 0)
			{
				return num;
			}
			if (num3 == 0)
			{
				num3 = 1;
			}
			Force -= num3;
			int springiness = ParentObject.GetSpringiness();
			int num4 = num3 + springiness;
			string text = Direction;
			if (text.IsNullOrEmpty())
			{
				if (list != null && list.Count == 0 && list2 != null)
				{
					list = new List<string>(list2);
				}
				if (Toward != null && !Repeat && (CurrentCell == Toward || (OneShort && ParentObject.DistanceTo(Toward) <= 1)))
				{
					return num;
				}
				if (list != null && list.Count > 0)
				{
					text = list[0];
					list.RemoveAt(0);
				}
				else if (Toward != null)
				{
					list = null;
					if (CurrentCell.ParentZone == Toward.ParentZone)
					{
						List<Tuple<Cell, char>> lineTo = ParentObject.GetLineTo(Toward);
						if (lineTo != null && lineTo.Count > 1)
						{
							list = new List<string>(lineTo.Count - 1);
							int i = 0;
							for (int num5 = lineTo.Count - 1; i < num5; i++)
							{
								string directionFromCell = lineTo[i].Item1.GetDirectionFromCell(lineTo[i + 1].Item1);
								if (!directionFromCell.IsNullOrEmpty() && directionFromCell != "." && directionFromCell != "?")
								{
									list.Add(directionFromCell);
									continue;
								}
								list = null;
								break;
							}
							if (list.Count <= 0)
							{
								list = null;
							}
						}
					}
					if (list != null)
					{
						num2 = 5 - list.Count / 5;
						if (Repeat)
						{
							list2 = new List<string>(list);
						}
						text = list[0];
						if (AwayFrom == null)
						{
							list.RemoveAt(0);
						}
						else
						{
							list = null;
						}
					}
					else
					{
						text = CurrentCell.GetDirectionFromCell(Toward);
					}
					if (AwayFrom != null)
					{
						text = Directions.CombineDirections(text, Directions.GetOppositeDirection(CurrentCell.GetDirectionFromCell(AwayFrom)), (int)CurrentCell.RealDistanceTo(Toward), (int)CurrentCell.RealDistanceTo(AwayFrom));
					}
				}
				else if (AwayFrom != null)
				{
					text = ((AwayFrom != CurrentCell) ? Directions.GetOppositeDirection(CurrentCell.GetDirectionFromCell(AwayFrom)) : Directions.GetRandomDirection());
				}
			}
			if (text.IsNullOrEmpty() || text == "." || text == "?")
			{
				return num;
			}
			Cell cellFromDirection = CurrentCell.GetCellFromDirection(text, BuiltOnly);
			if (cellFromDirection == null || cellFromDirection == CurrentCell)
			{
				return num;
			}
			string type;
			if (cellFromDirection.IsEmpty())
			{
				GameObject parentObject = ParentObject;
				string direction = text;
				bool ignoreGravity = SuspendFalling;
				type = Type;
				if (!parentObject.Move(direction, Forced: true, System: false, ignoreGravity, NoStack: true, AllowDashing: true, DoConfirmations: true, null, null, NearestAvailable: false, null, type))
				{
					ParentObject.CheckStack();
					return num;
				}
				if (flag)
				{
					DidX("are", "knocked " + Directions.GetDirectionDescription(text));
					flag = false;
				}
				if (Type == "Telekinetic")
				{
					ParentObject.TelekinesisBlip();
				}
				if (Direction != null)
				{
					Direction = text;
				}
				num++;
				if (!Subsequent)
				{
					The.Core.RenderBase(UpdateSidebar: false);
					if ((num2 > 0) & DelayForDisplay)
					{
						Thread.Sleep(num2);
					}
				}
				continue;
			}
			if (flag)
			{
				DidX("are", "knocked " + Directions.GetDirectionDescription(text));
				flag = false;
			}
			List<GameObject> list4 = Event.NewGameObjectList();
			int j = 0;
			for (int count = cellFromDirection.Objects.Count; j < count; j++)
			{
				GameObject gameObject = cellFromDirection.Objects[j];
				if (CollidesWith(gameObject, ref myMatterPhase))
				{
					list4.Add(gameObject);
				}
			}
			if (list4.Count > 0)
			{
				Direction = Directions.GetOppositeDirection(text);
				int num6 = num4;
				foreach (GameObject item in list4)
				{
					num6 += item.GetKineticAbsorption();
				}
				int num7 = Force;
				Force = num7 * num4 / num6;
				if (list3 == null)
				{
					list3 = Event.NewGameObjectList();
				}
				string text2 = (ParentObject.IsPlayer() ? "you" : ParentObject.an());
				foreach (GameObject item2 in list4)
				{
					int kineticResistance = item2.GetKineticResistance();
					int springiness2 = item2.GetSpringiness();
					if (MessageForInanimate || ParentObject.IsCreature || item2.IsCreature)
					{
						DidXToY("collide", "with", item2);
					}
					if (!list3.Contains(item2))
					{
						list3.Add(item2);
						int increments = (int)((double)(num7 * (num3 + kineticResistance - springiness - springiness2)) * DamageFactor / (double)num6) / 20;
						CalculateIncrementalDamageRange(increments, out var Low, out var High);
						bool flag2 = item2.ConsiderSolid() || item2.IsCombat;
						int num8;
						if (flag2)
						{
							num8 = 1;
						}
						else
						{
							GetCollidedWithPenetration(item2, Force, out var Bonus, out var MaxBonus);
							num8 = Stat.RollDamagePenetrations(Stats.GetCombatAV(ParentObject), Bonus, MaxBonus);
						}
						int num9 = 0;
						for (int k = 0; k < num8; k++)
						{
							num9 += Stat.Random(Low, High);
							if (!BonusDamage.IsNullOrEmpty())
							{
								num9 += BonusDamage.RollCached();
							}
						}
						bool flag3 = Solid || ParentObject.IsCombat;
						int num10;
						if (flag3)
						{
							num10 = 1;
						}
						else
						{
							GetCollidedWithPenetration(ParentObject, Force, out var Bonus2, out var MaxBonus2);
							num10 = Stat.RollDamagePenetrations(Stats.GetCombatAV(item2), Bonus2, MaxBonus2);
						}
						int num11 = 0;
						for (int l = 0; l < num10; l++)
						{
							num11 += Stat.Random(Low, High);
							if (!BonusDamage.IsNullOrEmpty())
							{
								num11 += BonusDamage.RollCached();
							}
						}
						if (num9 > 0)
						{
							string text3 = "from colliding with " + (item2.IsPlayer() ? "you" : item2.an()) + ".";
							if (!flag2)
							{
								string resultColor = Stat.GetResultColor(num8);
								text3 = ("(x" + num8 + ")").Color(resultColor) + " " + text3;
							}
							GameObject parentObject2 = ParentObject;
							int amount = num9;
							bool accidental = Accidental && ParentObject != IntendedTarget;
							GameObject attacker = Actor;
							GameObject source = item2;
							parentObject2.TakeDamage(amount, text3, "Crushing Collision", null, null, null, attacker, source, null, null, accidental, Environmental: false, Indirect: false, ShowUninvolved: false, IgnoreVisibility: false, MessageForInanimate);
						}
						if (num11 > 0)
						{
							string text4 = "from " + text2 + " colliding with " + item2.them + ".";
							if (!flag3)
							{
								string resultColor2 = Stat.GetResultColor(num8);
								text4 = ("(x" + num10 + ")").Color(resultColor2) + " " + text4;
							}
							int amount2 = num11;
							bool accidental = Accidental && ParentObject != IntendedTarget;
							GameObject source = Actor;
							GameObject attacker = ParentObject;
							item2.TakeDamage(amount2, text4, "Crushing Collision", null, null, null, source, attacker, null, null, accidental, Environmental: false, Indirect: false, ShowUninvolved: false, IgnoreVisibility: false, MessageForInanimate);
						}
					}
					if (GameObject.Validate(item2) && item2.GetMatterPhase() <= 1)
					{
						item2.Physics.AccelerateInternal(num7 * kineticResistance / num6, text, null, null, Type, Actor, Accidental: true, null, BonusDamage, DamageFactor, SuspendFalling, OneShort: false, Repeat: false, BuiltOnly, MessageForInanimate, DelayForDisplay, Subsequent: true);
					}
					if (!GameObject.Validate(ParentObject))
					{
						return num;
					}
				}
			}
			GameObject parentObject3 = ParentObject;
			string direction2 = text;
			bool ignoreGravity2 = SuspendFalling;
			type = Type;
			if (!parentObject3.Move(direction2, Forced: true, System: false, ignoreGravity2, NoStack: true, AllowDashing: true, DoConfirmations: true, null, null, NearestAvailable: false, null, type))
			{
				break;
			}
			if (Type == "Telekinetic")
			{
				ParentObject.TelekinesisBlip();
			}
			if (Direction != null)
			{
				Direction = text;
			}
			num++;
			if (!Subsequent)
			{
				The.Core.RenderBase(UpdateSidebar: false);
				if ((num2 > 0) & DelayForDisplay)
				{
					Thread.Sleep(num2);
				}
			}
		}
		if (SuspendFalling)
		{
			ParentObject.Gravitate();
		}
		ParentObject.CheckStack();
		return num;
	}

	private void CalculateIncrementalDamageRange(int Increments, out int Low, out int High)
	{
		Low = 0;
		High = 0;
		if (Increments <= 0)
		{
			return;
		}
		Low = 1;
		High = 2;
		for (int i = 1; i < Increments; i++)
		{
			if (i % 3 == 0)
			{
				Low++;
			}
			else
			{
				High += 2;
			}
		}
	}

	public static void LegacyApplyExplosion(Cell C, List<Cell> UsedCells, List<GameObject> Hit, int Force, bool Local = true, bool Show = true, GameObject Owner = null, string BonusDamage = null, int Phase = 1, float DamageModifier = 1f)
	{
		if (C == null)
		{
			return;
		}
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		if (Show)
		{
			TextConsole.LoadScrapBuffers();
			The.Core.RenderMapToBuffer(scrapBuffer);
		}
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		CleanQueue<int> cleanQueue2 = new CleanQueue<int>();
		CleanQueue<string> cleanQueue3 = new CleanQueue<string>();
		cleanQueue.Enqueue(C);
		cleanQueue2.Enqueue(Force);
		cleanQueue3.Enqueue(".");
		UsedCells.Add(C);
		while (cleanQueue.Count > 0)
		{
			Event.PinCurrentPool();
			Cell cell = cleanQueue.Dequeue();
			int num = cleanQueue2.Dequeue();
			string text = cleanQueue3.Dequeue();
			for (int i = 0; i < UsedCells.Count; i++)
			{
				Cell cell2 = UsedCells[i];
				if (cell2 == null)
				{
					return;
				}
				if (cell2.ParentZone == The.ZoneManager.ActiveZone)
				{
					scrapBuffer.Goto(cell2.X, cell2.Y);
					scrapBuffer.Write("&" + XRL.World.Capabilities.Phase.getRandomExplosionColor(Phase) + "*");
				}
			}
			if (Show && C.ParentZone != null && C.ParentZone.IsActive())
			{
				textConsole.DrawBuffer(scrapBuffer);
				if (Force < 100000)
				{
					Thread.Sleep(5);
				}
			}
			List<Cell> list = ((!Local) ? cell.GetAdjacentCells() : cell.GetLocalAdjacentCells(1));
			for (int j = 0; j < UsedCells.Count; j++)
			{
				Cell item = UsedCells[j];
				if (list.CleanContains(item))
				{
					list.Remove(item);
				}
			}
			int num2 = 0;
			Damage damage = null;
			Event obj = null;
			foreach (GameObject item2 in cell.GetObjectsWithPartReadonly("Physics"))
			{
				if (Hit.Contains(item2))
				{
					continue;
				}
				Hit.Add(item2);
				if (!item2.PhaseMatches(Phase))
				{
					continue;
				}
				num2 += item2.GetKineticResistance();
				if (damage == null || !BonusDamage.IsNullOrEmpty())
				{
					damage = new Damage((int)(DamageModifier * (float)num / 250f));
					if (!BonusDamage.IsNullOrEmpty())
					{
						damage.Amount += BonusDamage.RollCached();
					}
					damage.AddAttribute("Explosion");
					if (cell != C)
					{
						damage.AddAttribute("Accidental");
					}
				}
				if (obj == null || !BonusDamage.IsNullOrEmpty())
				{
					obj = Event.New("TakeDamage");
					obj.SetParameter("Damage", damage);
					obj.SetParameter("Owner", Owner);
					obj.SetParameter("Attacker", Owner);
					obj.SetParameter("Message", "from %t explosion!");
				}
				item2.FireEvent(obj);
			}
			Random random = new Random();
			for (int k = 0; k < list.Count; k++)
			{
				int index = random.Next(0, list.Count);
				Cell value = list[k];
				list[k] = list[index];
				list[index] = value;
			}
			Damage damage2 = null;
			Event obj2 = null;
			while (true)
			{
				IL_0303:
				for (int l = 0; l < list.Count; l++)
				{
					Cell cell3 = list[l];
					if (Local && (cell3.X == 0 || cell3.X == 79 || cell3.Y == 0 || cell3.Y == 24))
					{
						continue;
					}
					foreach (GameObject item3 in cell3.GetObjectsWithPartReadonly("Physics"))
					{
						if (!Hit.Contains(item3))
						{
							Hit.Add(item3);
							if (item3.PhaseMatches(Phase))
							{
								if (damage2 == null || !BonusDamage.IsNullOrEmpty())
								{
									damage2 = new Damage(num / 250);
									if (!BonusDamage.IsNullOrEmpty())
									{
										damage2.Amount += BonusDamage.RollCached();
									}
									damage2.AddAttribute("Explosion");
									damage2.AddAttribute("Accidental");
								}
								if (obj2 == null || !BonusDamage.IsNullOrEmpty())
								{
									obj2 = Event.New("TakeDamage");
									obj2.SetParameter("Damage", damage2);
									obj2.SetParameter("Owner", Owner);
									obj2.SetParameter("Attacker", Owner);
									obj2.SetParameter("Message", "from %t explosion!");
								}
								item3.FireEvent(obj2);
							}
						}
						if (item3.PhaseMatches(Phase))
						{
							int kineticResistance = item3.GetKineticResistance();
							if (kineticResistance > num)
							{
								list.Remove(cell3);
								goto IL_0303;
							}
							if (kineticResistance > 0)
							{
								item3.Move((text == ".") ? Directions.GetRandomDirection() : text, Forced: true);
							}
						}
					}
					if (cell3.IsSolid())
					{
						list.Remove(cell3);
						goto IL_0303;
					}
				}
				break;
			}
			if (list.Count > 0)
			{
				int num3 = (num - num2) / list.Count;
				if (num3 > 100)
				{
					foreach (Cell item4 in list)
					{
						if (item4 != null && !UsedCells.Contains(item4))
						{
							UsedCells.Add(item4);
							cleanQueue.Enqueue(item4);
							cleanQueue2.Enqueue(num3);
							cleanQueue3.Enqueue(cell.GetDirectionFromCell(item4));
						}
					}
				}
			}
			Event.ResetToPin();
		}
	}

	private void GetCollidingPenetration(GameObject obj, int Force, out int Bonus, out int MaxBonus)
	{
		Bonus = 0;
		MaxBonus = 0;
		ThrownWeapon part = obj.GetPart<ThrownWeapon>();
		MeleeWeapon part2 = obj.GetPart<MeleeWeapon>();
		if (part != null)
		{
			Bonus = part.Penetration;
			MaxBonus = Bonus * 2;
		}
		else if (part2 != null)
		{
			Bonus = (4 + part2.PenBonus) / 2;
			MaxBonus = Bonus + part2.MaxStrengthBonus;
		}
		else
		{
			Bonus = 2;
			MaxBonus = 4;
		}
		Bonus += Stat.GetScoreModifier(Force / 15);
		MaxBonus += obj.Weight / 50;
		if (Bonus > MaxBonus)
		{
			Bonus = MaxBonus;
		}
		if (Bonus < 0)
		{
			Bonus = 0;
		}
	}

	private void GetCollidedWithPenetration(GameObject obj, int Force, out int Bonus, out int MaxBonus)
	{
		Bonus = 0;
		MaxBonus = 0;
		MeleeWeapon part = obj.GetPart<MeleeWeapon>();
		if (part != null)
		{
			Bonus = (4 + part.PenBonus) / 2;
			MaxBonus = Bonus + part.MaxStrengthBonus;
		}
		else
		{
			Bonus = 2;
			MaxBonus = 4;
		}
		Bonus += Stat.GetScoreModifier(Force / 15);
		MaxBonus += obj.Weight / 50;
		if (Bonus > MaxBonus)
		{
			Bonus = MaxBonus;
		}
		if (Bonus < 0)
		{
			Bonus = 0;
		}
	}

	private bool CollidesWith(GameObject obj, ref int myMatterPhase)
	{
		if (!IsReal)
		{
			return false;
		}
		if (obj == ParentObject)
		{
			return false;
		}
		if (!GameObject.Validate(ref obj) || !GameObject.Validate(ParentObject))
		{
			return false;
		}
		if (obj.IsScenery)
		{
			return false;
		}
		if (obj.IsInGraveyard() || ParentObject.IsInGraveyard())
		{
			return false;
		}
		if (obj.GetMatterPhase() >= 3)
		{
			return false;
		}
		if (myMatterPhase == 0)
		{
			myMatterPhase = ParentObject.GetMatterPhase();
		}
		if (myMatterPhase >= 3)
		{
			return false;
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return false;
		}
		bool flag = false;
		if (obj.ConsiderSolidFor(ParentObject) || ParentObject.ConsiderSolidFor(obj))
		{
			flag = true;
		}
		else if (obj.IsCombat)
		{
			if (ParentObject.IsCombat)
			{
				flag = true;
			}
			else if (Stat.Random(1, 20) > Stats.GetCombatDV(obj))
			{
				flag = true;
			}
		}
		else if (obj.IsReal && Math.Min(20 + ParentObject.Weight + obj.Weight, 80).in100() && Stat.Random(1, 20) > Stats.GetCombatDV(obj))
		{
			flag = true;
		}
		if (!flag)
		{
			return false;
		}
		if (!obj.PhaseMatches(ParentObject))
		{
			return false;
		}
		if (!obj.FlightMatches(ParentObject))
		{
			return 50.in100();
		}
		return true;
	}

	public bool CollidesWith(GameObject obj)
	{
		int myMatterPhase = 0;
		return CollidesWith(obj, ref myMatterPhase);
	}

	public static void ApplyExplosion(Cell C, int Force, List<Cell> UsedCells = null, List<GameObject> Hit = null, bool Local = true, bool Show = true, GameObject Owner = null, string BonusDamage = null, int Phase = 1, bool Neutron = false, bool Indirect = false, float DamageModifier = 1f, GameObject WhatExploded = null)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		if (Show && Options.UseParticleVFX)
		{
			TextConsole.LoadScrapBuffers();
			The.Core.RenderMapToBuffer(scrapBuffer);
		}
		if (UsedCells == null)
		{
			UsedCells = Event.NewCellList();
		}
		if (Hit == null)
		{
			Hit = Event.NewGameObjectList();
		}
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		CleanQueue<int> cleanQueue2 = new CleanQueue<int>();
		CleanQueue<string> cleanQueue3 = new CleanQueue<string>();
		cleanQueue.Enqueue(C);
		cleanQueue2.Enqueue(Force);
		cleanQueue3.Enqueue(".");
		UsedCells.Add(C);
		if (Options.UseParticleVFX && C != null && C.IsVisible() && !Neutron)
		{
			CombatJuice.playPrefabAnimation(C.Location, "Impacts/ImpactVFXExplosion", null, null, null, async: true);
		}
		if ((Options.UseParticleVFX && C != null && C.IsVisible()) & Neutron)
		{
			CombatJuice.playPrefabAnimation(C.Location, "Impacts/ImpactVFXNeutronImpact", null, null, null, async: true);
		}
		int num = 20 - Force / 1000;
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			if (cell == null)
			{
				continue;
			}
			if (Options.UseParticleVFX && cell.IsVisible() && !Neutron)
			{
				CombatJuice.playPrefabAnimation(cell.Location, "Impacts/ImpactVFXExplosion", null, null, null, async: true);
			}
			if ((Options.UseParticleVFX && cell.IsVisible()) & Neutron)
			{
				CombatJuice.playPrefabAnimation(cell.Location, "Impacts/ImpactVFXNeutronImpact", null, null, null, async: true);
			}
			cell.WakeCreaturesInArea();
			int num2 = cleanQueue2.Dequeue();
			string text = cleanQueue3.Dequeue();
			List<Cell> list;
			List<GameObject> list2;
			if (cell.OnWorldMap())
			{
				list = new List<Cell>(0);
				list2 = Event.NewGameObjectList();
				if (cell.Objects.Contains(The.Player))
				{
					list2.Add(The.Player);
				}
			}
			else
			{
				list = ((!Local) ? cell.GetAdjacentCells() : cell.GetLocalAdjacentCells(1));
				for (int i = 0; i < UsedCells.Count; i++)
				{
					list.Remove(UsedCells[i]);
				}
				list2 = cell.GetObjectsWithPartReadonly("Physics");
			}
			int num3 = 0;
			foreach (GameObject item in list2)
			{
				if (!Hit.Contains(item))
				{
					Hit.Add(item);
					int num4 = ExplosionDamage(item, Owner, C, cell, num2, Phase, BonusDamage, DamageModifier, Neutron, Indirect, WhatExploded);
					num3 += num4;
				}
			}
			Random random = new Random();
			for (int j = 0; j < list.Count; j++)
			{
				int index = random.Next(0, list.Count);
				Cell value = list[j];
				list[j] = list[index];
				list[index] = value;
			}
			while (true)
			{
				IL_0298:
				for (int num5 = UsedCells.Count - 1; num5 >= 0; num5--)
				{
					Cell cell2 = UsedCells[num5];
					if (cell2 == null)
					{
						return;
					}
					if (cell2.ParentZone.IsActive() && !Options.UseParticleVFX)
					{
						scrapBuffer.WriteAt(cell2, "&" + XRL.World.Capabilities.Phase.getRandomExplosionColor(Phase) + "*");
					}
				}
				for (int num6 = list.Count - 1; num6 >= 0; num6--)
				{
					Cell cell3 = list[num6];
					if (cell3 == null)
					{
						return;
					}
					if (cell3.ParentZone.IsActive() && !Options.UseParticleVFX)
					{
						scrapBuffer.WriteAt(cell3, "&" + XRL.World.Capabilities.Phase.getRandomExplosionColor(Phase) + "*");
					}
				}
				if (Show && C.ParentZone != null && C.ParentZone.IsActive() && !Options.UseParticleVFX)
				{
					textConsole.DrawBuffer(scrapBuffer);
					if (num > 0)
					{
						Thread.Sleep(num);
					}
				}
				for (int k = 0; k < list.Count; k++)
				{
					Cell cell4 = list[k];
					if (Local && (cell4.X == 0 || cell4.X == 79 || cell4.Y == 0 || cell4.Y == 24))
					{
						continue;
					}
					foreach (GameObject item2 in cell4.GetObjectsWithPartReadonly("Physics"))
					{
						if (Hit.Contains(item2))
						{
							continue;
						}
						Hit.Add(item2);
						int num7 = ExplosionDamage(item2, Owner, C, cell, num2, Phase, BonusDamage, DamageModifier, Neutron, Indirect, WhatExploded);
						if (num7 > 0)
						{
							num3 += num7;
							if (num7 > num2)
							{
								list.Remove(cell4);
								goto IL_0298;
							}
							if (IsMoveable(item2))
							{
								item2.Move((text == ".") ? Directions.GetRandomDirection() : text, Forced: true, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: true, DoConfirmations: true, null, null, NearestAvailable: false, null, "Explosion");
							}
						}
					}
					if (cell4.IsSolid())
					{
						list.Remove(cell4);
						goto IL_0298;
					}
				}
				break;
			}
			if (list.Count <= 0)
			{
				continue;
			}
			int num8 = (num2 - num3) / list.Count;
			if (num8 <= 100)
			{
				continue;
			}
			foreach (Cell item3 in list)
			{
				if (item3 != null && !UsedCells.Contains(item3))
				{
					UsedCells.Add(item3);
					cleanQueue.Enqueue(item3);
					cleanQueue2.Enqueue(num8);
					cleanQueue3.Enqueue(cell.GetDirectionFromCell(item3));
				}
			}
		}
	}

	private static int ExplosionDamage(GameObject GO, GameObject Owner, Cell C, Cell CurrentC, int CurrentForce, int Phase, string BonusDamage, float DamageModifier, bool Neutron, bool Indirect, GameObject WhatExploded, bool TrackResistance = true)
	{
		int result = 0;
		if (GO.PhaseMatches(Phase))
		{
			result = GO.GetKineticResistance();
			int num = (int)(DamageModifier * (float)CurrentForce / 250f);
			bool flag = CurrentC != C;
			if (!BonusDamage.IsNullOrEmpty())
			{
				num += BonusDamage.RollCached();
			}
			string message;
			string deathReason;
			string thirdPersonDeathReason;
			if (Neutron)
			{
				message = "from being crushed under the weight of a thousand suns.";
				deathReason = "You were crushed under the weight of a thousand suns.";
				thirdPersonDeathReason = GO.It + GO.GetVerb("were") + " @@crushed under the weight of a thousand suns.";
			}
			else
			{
				message = "from %t explosion!";
				StringBuilder stringBuilder = Event.NewStringBuilder();
				StringBuilder stringBuilder2 = Event.NewStringBuilder();
				stringBuilder.Append("You");
				stringBuilder2.Append(GO.It);
				if (WhatExploded == GO)
				{
					stringBuilder.Append(" exploded");
					stringBuilder2.Append(" @@exploded");
				}
				else
				{
					stringBuilder.Append(" died in ");
					stringBuilder2.Append(" @@died in ");
					if (WhatExploded != null)
					{
						stringBuilder.Append("the explosion of ").Append(WhatExploded.an());
						stringBuilder2.Append("the explosion of ").Append(WhatExploded.an());
					}
					else
					{
						stringBuilder.Append("an explosion");
						stringBuilder2.Append("an explosion");
					}
				}
				if (Owner != null && Owner != WhatExploded)
				{
					if (Owner == GO)
					{
						if (WhatExploded != null)
						{
							stringBuilder.Append(", which you caused");
							stringBuilder2.Append(", which ").Append(GO.it).Append(" caused");
						}
						else
						{
							stringBuilder.Append(" you caused");
							stringBuilder2.Append(' ').Append(GO.it).Append(" caused");
						}
					}
					else if (GO.IsPlayer() && Owner.GetStringProperty("PlayerCopy") == "true" && Owner.HasStringProperty("PlayerCopyDescription"))
					{
						stringBuilder.Append(" caused by ").Append(Owner.GetStringProperty("PlayerCopyDescription"));
						stringBuilder2.Append(" caused by ").Append(Owner.GetStringProperty("PlayerCopyDescription"));
					}
					else
					{
						stringBuilder.Append(" caused by ").Append(Owner.an());
						stringBuilder2.Append(" caused by ").Append(Owner.an());
					}
				}
				stringBuilder.Append('.');
				stringBuilder2.Append('.');
				deathReason = stringBuilder.ToString();
				thirdPersonDeathReason = stringBuilder2.ToString();
			}
			int amount = num;
			string attributes = (Neutron ? "Neutron Explosion" : "Explosion");
			bool accidental = flag;
			bool indirect = Indirect;
			GO.TakeDamage(amount, message, attributes, deathReason, thirdPersonDeathReason, Owner, null, WhatExploded, null, null, accidental, Environmental: false, indirect, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: false, SilentIfNoDamage: false, NoSetTarget: false, UsePopups: false, Phase);
		}
		return result;
	}

	public int ApplyDischarge(Cell C, Cell TargetCell, int Voltage, int Damage = 0, string DamageRange = null, DieRoll DamageRoll = null, GameObject Target = null, List<Cell> UsedCells = null, GameObject Owner = null, GameObject Source = null, GameObject DescribeAsFrom = null, GameObject Skip = null, List<GameObject> SkipList = null, bool? SourceVisible = null, string SourceDesc = null, string SourceDirectionTowardTarget = null, int Phase = 0, bool Accidental = false, bool Environmental = false, GameObject Alternate = null, GameObject AlternateAvoidedBecauseObject = null, string AlternateAvoidedBecauseReason = null, bool UsePopups = false)
	{
		int Shown = 0;
		return ApplyDischarge(C, TargetCell, Voltage, ref Shown, Damage, DamageRange, DamageRoll, Target, UsedCells, Owner, Source, DescribeAsFrom, Skip, SkipList, SourceVisible, SourceDesc, SourceDirectionTowardTarget, Phase, Accidental, Environmental, Alternate, AlternateAvoidedBecauseObject, AlternateAvoidedBecauseReason, 0, UsePopups);
	}

	public int ApplyDischarge(Cell C, Cell TargetCell, int Voltage, ref int Shown, int Damage = 0, string DamageRange = null, DieRoll DamageRoll = null, GameObject Target = null, List<Cell> UsedCells = null, GameObject Owner = null, GameObject Source = null, GameObject DescribeAsFrom = null, GameObject Skip = null, List<GameObject> SkipList = null, bool? SourceVisible = null, string SourceDesc = null, string SourceDirectionTowardTarget = null, int Phase = 0, bool Accidental = false, bool Environmental = false, GameObject Alternate = null, GameObject AlternateAvoidedBecauseObject = null, string AlternateAvoidedBecauseReason = null, int Depth = 0, bool UsePopups = false)
	{
		if (C == null)
		{
			return Shown;
		}
		if (C.ParentZone == null)
		{
			return Shown;
		}
		if (TargetCell == null)
		{
			TargetCell = Target?.CurrentCell;
		}
		if (TargetCell == null)
		{
			return Shown;
		}
		if (TargetCell.ParentZone == null)
		{
			return Shown;
		}
		if (Damage == 0)
		{
			if (DamageRoll == null && !DamageRange.IsNullOrEmpty())
			{
				DamageRoll = DamageRange.GetCachedDieRoll();
			}
			if (DamageRoll != null)
			{
				Damage = DamageRoll.Resolve();
				if (Depth > 0)
				{
					Damage = Damage * 4 * Depth / (5 * Depth);
				}
			}
		}
		if (Damage <= 0)
		{
			return Shown;
		}
		if (Phase == 0 && Owner != null)
		{
			Phase = Owner.GetPhase();
		}
		if (UsedCells == null)
		{
			UsedCells = Event.NewCellList();
		}
		if (Options.DrawArcs)
		{
			TargetCell.ParticleBlip("&WX", 300, 0L);
		}
		List<Point> list;
		int num;
		if (C.IsSameOrAdjacent(TargetCell))
		{
			list = null;
			num = 0;
		}
		else
		{
			list = Zone.Line(C.X, C.Y, TargetCell.X, TargetCell.Y);
			num = 1;
		}
		for (int i = num; (list == null) ? (i == num) : (i < list.Count); i++)
		{
			Cell cell = ((list == null) ? TargetCell : TargetCell.ParentZone.GetCell(list[i]));
			if (cell.IsVisible())
			{
				cell.ParticleBlip("&" + XRL.World.Capabilities.Phase.getRandomElectricArcColor(Phase) + (char)Stat.RandomCosmetic(191, 198), 30, 0L);
				cell.PlayWorldSound("Sounds/Enhancements/sfx_enhancement_electric_conductiveJump");
			}
			if (Target == null && (C.X != cell.X || C.Y != cell.Y || cell == TargetCell))
			{
				GameObject gameObject = ((Owner != null && Depth == 0) ? cell.GetCombatTarget(Owner, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, Phase, null, null, null, Skip, SkipList) : null);
				if (gameObject != null && gameObject.GetElectricalConductivity(Source, Phase) > 0)
				{
					Target = gameObject;
				}
				if (Target == null)
				{
					int num2 = 0;
					DischargeObjects.Clear();
					foreach (GameObject item in cell.GetObjectsWithPartReadonly("Combat"))
					{
						if (Skip != item && (SkipList == null || !SkipList.Contains(item)))
						{
							int electricalConductivity = item.GetElectricalConductivity(Source, Phase);
							if (electricalConductivity > num2)
							{
								DischargeObjects.Clear();
								DischargeObjects.Add(item);
								num2 = electricalConductivity;
							}
							else if (electricalConductivity == num2)
							{
								DischargeObjects.Add(item);
							}
						}
					}
					if (DischargeObjects.Count == 0)
					{
						foreach (GameObject item2 in cell.GetObjectsWithPartReadonly("Physics"))
						{
							if (item2.IsReal && item2.IsScenery && Skip != item2 && (SkipList == null || !SkipList.Contains(item2)))
							{
								int electricalConductivity2 = item2.GetElectricalConductivity(Source, Phase);
								if (electricalConductivity2 > num2)
								{
									DischargeObjects.Clear();
									DischargeObjects.Add(item2);
									num2 = electricalConductivity2;
								}
								else if (electricalConductivity2 == num2)
								{
									DischargeObjects.Add(item2);
								}
							}
						}
					}
					Target = DischargeObjects.GetRandomElement();
					DischargeObjects.Clear();
				}
			}
			bool flag = false;
			if (Target == null)
			{
				continue;
			}
			flag = Target.PhaseMatches(Phase);
			if (!UsedCells.Contains(C))
			{
				UsedCells.Add(C);
			}
			if (!UsedCells.Contains(cell))
			{
				UsedCells.Add(cell);
			}
			string text = ((Shown == 0) ? "An" : "The");
			string text2 = null;
			bool flag2 = false;
			if (GameObject.Validate(Source))
			{
				if (Shown == 0 && IComponent<GameObject>.Visible(Source) && IComponent<GameObject>.Visible(Target))
				{
					text2 = text + " {{electrical|electrical arc}} leaps from " + Source.t() + " toward " + Target.t() + (Target.IsPlayer() ? "" : (" " + Source.DescribeDirectionToward(Target)));
					Shown++;
					flag2 = true;
				}
				else if (IComponent<GameObject>.Visible(Target))
				{
					text2 = text + " {{electrical|electrical arc}} leaps toward " + Target.t() + (Target.IsPlayer() ? "" : (" " + Source.DescribeDirectionToward(Target)));
					Shown++;
				}
				else if (IComponent<GameObject>.Visible(Source))
				{
					text2 = text + " {{electrical|electrical arc}} leaps from " + Source.t() + " toward something " + Source.DescribeDirectionToward(Target);
					Shown++;
					flag2 = true;
				}
			}
			else if (!SourceDesc.IsNullOrEmpty() && !SourceDirectionTowardTarget.IsNullOrEmpty())
			{
				if (Shown == 0 && SourceVisible == true && IComponent<GameObject>.Visible(Target))
				{
					text2 = text + " {{electrical|electrical arc}} leaps from " + SourceDesc + " toward " + Target.t() + " " + SourceDirectionTowardTarget;
					Shown++;
				}
				else if (IComponent<GameObject>.Visible(Target))
				{
					text2 = text + " {{electrical|electrical arc}} leaps toward " + Target.t() + " " + SourceDirectionTowardTarget;
					Shown++;
				}
				else if (SourceVisible == true)
				{
					text2 = text + " {{electrical|electrical arc}} leaps from " + SourceDesc + " toward something " + SourceDirectionTowardTarget;
					Shown++;
				}
			}
			else if (IComponent<GameObject>.Visible(Target))
			{
				text2 = text + " {{electrical|electrical arc}} leaps toward " + Target.t();
				Shown++;
			}
			if (!text2.IsNullOrEmpty())
			{
				if (GameObject.Validate(Alternate) && IComponent<GameObject>.Visible(Alternate))
				{
					text2 = text2 + ", avoiding " + Alternate.t();
					if (flag2 && GameObject.Validate(Source))
					{
						text2 = text2 + " " + Source.DescribeRelativeDirectionToward(Alternate);
					}
					if (GameObject.Validate(AlternateAvoidedBecauseObject))
					{
						text2 = ((AlternateAvoidedBecauseObject.GetObjectContext() != Alternate) ? (text2 + " because of " + AlternateAvoidedBecauseObject.an()) : (text2 + " because of " + Alternate.its_(AlternateAvoidedBecauseObject)));
					}
					else if (!AlternateAvoidedBecauseReason.IsNullOrEmpty())
					{
						text2 = text2 + " because of " + AlternateAvoidedBecauseReason;
					}
				}
				if (!flag && IComponent<GameObject>.Visible(Target))
				{
					text2 = text2 + ", but passes through " + Target.them;
				}
				text2 += "!";
				if (UsePopups)
				{
					Popup.Show(text2);
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(text2);
				}
			}
			bool flag3 = false;
			int num3 = 0;
			GameObject gameObject2 = null;
			bool value = false;
			string text3 = null;
			string text4 = null;
			GameObject gameObject3 = null;
			int num4 = 0;
			GameObject alternateAvoidedBecauseObject = null;
			string alternateAvoidedBecauseReason = null;
			if (Voltage > 1)
			{
				if (DamageRoll == null && DamageRange.IsNullOrEmpty())
				{
					num3 = Damage * 4 / 5;
					flag3 = num3 > 0;
				}
				else
				{
					num3 = 0;
					flag3 = true;
				}
				if (flag3)
				{
					GameObject gameObject4 = null;
					GameObject gameObject5 = null;
					string text5 = null;
					int num5 = 0;
					List<Cell> adjacentCells = cell.GetAdjacentCells();
					foreach (Cell UsedCell in UsedCells)
					{
						adjacentCells.Remove(UsedCell);
					}
					if (adjacentCells.Count > 0)
					{
						DischargeObjects.Clear();
						foreach (Cell item3 in adjacentCells)
						{
							foreach (GameObject item4 in item3.GetObjectsWithPartReadonly("Physics"))
							{
								int electricalConductivity3 = item4.GetElectricalConductivity(out var ReductionObject, out var ReductionReason, Source, Phase);
								if (electricalConductivity3 > num5)
								{
									if (gameObject4 != null && (gameObject3 == null || num5 > num4) && (gameObject5 != null || !text5.IsNullOrEmpty()))
									{
										gameObject3 = gameObject4;
										num4 = num5;
										alternateAvoidedBecauseObject = gameObject5;
										alternateAvoidedBecauseReason = text5;
									}
									gameObject4 = item4;
									num5 = electricalConductivity3;
									gameObject5 = ReductionObject;
									text5 = ReductionReason;
									DischargeObjects.Clear();
									DischargeObjects.Add(item4);
								}
								else if (electricalConductivity3 == num5)
								{
									if (item4.IsCombatObject() && (gameObject4 == null || !gameObject4.IsCombatObject()) && (ReductionObject != null || !ReductionReason.IsNullOrEmpty()))
									{
										gameObject4 = item4;
										num5 = electricalConductivity3;
										gameObject5 = ReductionObject;
										text5 = ReductionReason;
									}
									DischargeObjects.Add(item4);
								}
								else if ((gameObject3 == null || electricalConductivity3 > num4) && (ReductionObject != null || !ReductionReason.IsNullOrEmpty()))
								{
									gameObject3 = item4;
									num4 = electricalConductivity3;
									alternateAvoidedBecauseObject = ReductionObject;
									alternateAvoidedBecauseReason = ReductionReason;
								}
							}
						}
						gameObject2 = DischargeObjects.GetRandomElement();
						DischargeObjects.Clear();
						if (gameObject2 != null)
						{
							value = IComponent<GameObject>.Visible(Target);
							text3 = Target.t();
							text4 = Target.DescribeDirectionToward(gameObject2);
						}
					}
				}
			}
			if (flag && Target.TakeDamage(Damage, "from %t electrical discharge!", "Electric Shock", null, null, Accidental: Accidental, Environmental: Environmental, Phase: Phase, Owner: Owner, Attacker: Owner ?? Source, Source: Source, Perspective: null, DescribeAsFrom: DescribeAsFrom, Indirect: false, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: false, SilentIfNoDamage: false, NoSetTarget: false, UsePopups: UsePopups))
			{
				for (int j = 0; j < 3; j++)
				{
					Target.ParticleText("&" + XRL.World.Capabilities.Phase.getRandomElectricArcColor(Phase) + "ú");
					Target.PlayWorldSound("Sounds/Enhancements/sfx_enhancement_electric_conductiveJump");
				}
				if (IComponent<GameObject>.Visible(Target))
				{
					Shown++;
				}
			}
			if (flag3 && gameObject2 != null)
			{
				Cell cell2 = gameObject2?.CurrentCell;
				if (cell2 != null)
				{
					Cell c = TargetCell;
					GameObject target = gameObject2;
					int voltage = Voltage - 1;
					int damage = num3;
					DieRoll damageRoll = DamageRoll;
					List<Cell> usedCells = UsedCells;
					GameObject source = Target;
					bool? sourceVisible = value;
					string sourceDesc = text3;
					string sourceDirectionTowardTarget = text4;
					int phase = Phase;
					bool environmental = Environmental;
					int depth = Depth + 1;
					ApplyDischarge(c, cell2, voltage, ref Shown, damage, DamageRange, damageRoll, target, usedCells, Owner, source, DescribeAsFrom, null, null, sourceVisible, sourceDesc, sourceDirectionTowardTarget, phase, Accidental: true, environmental, gameObject3, alternateAvoidedBecauseObject, alternateAvoidedBecauseReason, depth, UsePopups);
				}
			}
			break;
		}
		return Shown;
	}

	public override bool RenderTile(ConsoleChar E)
	{
		return true;
	}

	private void DoSearching(Cell C, ref Event eSearched)
	{
		if (C.HasObjectWithRegisteredEvent("Searched"))
		{
			if (eSearched == null)
			{
				eSearched = Event.New("Searched", "Searcher", ParentObject);
			}
			C.FireEvent(eSearched);
		}
	}

	public void Search()
	{
		Event eSearched = null;
		DoSearching(CurrentCell, ref eSearched);
		List<Cell> localAdjacentCells = CurrentCell.GetLocalAdjacentCells();
		int i = 0;
		for (int count = localAdjacentCells.Count; i < count; i++)
		{
			DoSearching(localAdjacentCells[i], ref eSearched);
		}
	}

	public bool EnterCell(Cell C)
	{
		if (_CurrentCell == C)
		{
			return false;
		}
		if (!ParentObject.IsValid())
		{
			MetricsManager.LogError("Attempting to add invalid object '" + ParentObject.DebugName + "' to cell.");
			return false;
		}
		if (ParentObject.IsInGraveyard())
		{
			MetricsManager.LogError("Attempting to add graveyard object '" + ParentObject.DebugName + "' to cell.");
			return false;
		}
		CurrentCell = C;
		InInventory = null;
		Equipped = null;
		EnvironmentalUpdateEvent.Send(ParentObject);
		if (IsPlayer())
		{
			Search();
			if (!IComponent<GameObject>.TerseMessages)
			{
				PassingBy.Clear();
				int i = 0;
				for (int count = CurrentCell.Objects.Count; i < count; i++)
				{
					GameObject gameObject = CurrentCell.Objects[i];
					if (gameObject == ParentObject || SkipInPassingBy(gameObject, CurrentCell))
					{
						continue;
					}
					if (gameObject.HasProperty("SeenMessage"))
					{
						string stringProperty = gameObject.GetStringProperty("SeenMessage");
						if (!stringProperty.IsNullOrEmpty())
						{
							PassingBy.Add(stringProperty);
						}
					}
					else
					{
						PassingBy.Add(gameObject.an());
					}
				}
				if (PassingBy.Count > 0)
				{
					IComponent<GameObject>.AddPlayerMessage("You pass by " + Grammar.MakeAndList(PassingBy) + ".");
				}
			}
		}
		return true;
	}

	private bool SkipInPassingBy(GameObject obj, Cell C)
	{
		if (obj.Render == null)
		{
			return true;
		}
		if (!obj.Render.Visible)
		{
			return true;
		}
		if (obj.HasTagOrProperty("NoPassByMessage"))
		{
			return true;
		}
		if (obj.IsWadingDepthLiquid() && !C.HasBridge() && obj.PhaseAndFlightMatches(ParentObject))
		{
			return true;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (ID != PooledEvent<CanBeTradedEvent>.ID && ID != DroppedEvent.ID && ID != EquippedEvent.ID && ID != PooledEvent<EnvironmentalUpdateEvent>.ID && ID != SingletonEvent<GeneralAmnestyEvent>.ID && ID != PooledEvent<GetContextEvent>.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetInventoryCategoryEvent>.ID && ID != GetNavigationWeightEvent.ID && (ID != PooledEvent<GetSlotsRequiredEvent>.ID || !UsesTwoSlots) && ID != PooledEvent<HasFlammableEquipmentEvent>.ID && ID != PooledEvent<HasFlammableEquipmentOrInventoryEvent>.ID && ID != InventoryActionEvent.ID && ID != LeftCellEvent.ID && ID != ObjectEnteredCellEvent.ID && (ID != ObjectEnteringCellEvent.ID || !Solid) && (ID != PooledEvent<OkayToDamageEvent>.ID || Owner.IsNullOrEmpty()) && ID != QueryEquippableListEvent.ID && ID != RemoveFromContextEvent.ID && ID != PooledEvent<ReplaceInContextEvent>.ID && ID != PooledEvent<StatChangeEvent>.ID && ID != SuspendingEvent.ID && ID != TakenEvent.ID && ID != TryRemoveFromContextEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(OkayToDamageEvent E)
	{
		if (!Owner.IsNullOrEmpty())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		LastDamagedBy = null;
		LastWeaponDamagedBy = null;
		LastProjectileDamagedBy = null;
		InflamedBy = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeTradedEvent E)
	{
		if (IsAflame())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnvironmentalUpdateEvent E)
	{
		AmbientCache = int.MinValue;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(HasFlammableEquipmentEvent E)
	{
		if (E.Object != ParentObject && FlameTemperature <= E.Temperature)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(HasFlammableEquipmentOrInventoryEvent E)
	{
		if (E.Object != ParentObject && FlameTemperature <= E.Temperature)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContextEvent E)
	{
		if (_CurrentCell != null)
		{
			E.CellContext = _CurrentCell;
			E.Relation = 1;
			E.RelationManager = this;
			return false;
		}
		if (GameObject.Validate(ref _InInventory))
		{
			E.ObjectContext = _InInventory;
			E.Relation = 2;
			E.RelationManager = this;
			return false;
		}
		if (GameObject.Validate(ref _Equipped))
		{
			E.ObjectContext = _Equipped;
			E.BodyPartContext = _Equipped.FindEquippedObject(ParentObject);
			E.Relation = 3;
			E.RelationManager = this;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		if (GameObject.Validate(ref _InInventory))
		{
			try
			{
				Inventory inventory = _InInventory.Inventory;
				if (inventory != null)
				{
					inventory.RemoveObject(ParentObject);
					inventory.ParentObject.ReceiveObject(E.Replacement);
				}
			}
			catch
			{
			}
		}
		if (GameObject.Validate(ref _Equipped))
		{
			try
			{
				ParentObject.SplitFromStack();
				GameObject equipped = _Equipped;
				BodyPart bodyPart = equipped.FindEquippedObject(ParentObject);
				if (bodyPart != null)
				{
					bodyPart.ForceUnequip(Silent: true);
					equipped.FireEvent(Event.New("CommandEquipObject", "Object", E.Replacement, "BodyPart", bodyPart));
				}
			}
			catch
			{
			}
		}
		if (_CurrentCell != null)
		{
			try
			{
				Cell cell = _CurrentCell;
				_CurrentCell.RemoveObject(ParentObject);
				cell.AddObject(E.Replacement);
			}
			catch
			{
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RemoveFromContextEvent E)
	{
		if (GameObject.Validate(ref _InInventory))
		{
			try
			{
				_InInventory.Inventory?.RemoveObject(ParentObject, E);
				_InInventory = null;
			}
			catch
			{
			}
		}
		if (GameObject.Validate(ref _Equipped))
		{
			try
			{
				BodyPart bodyPart = _Equipped.FindEquippedObject(ParentObject);
				if (bodyPart != null)
				{
					bodyPart.ForceUnequip(Silent: true, NoStack: false, NoTake: true);
					if (bodyPart.Equipped == ParentObject)
					{
						bodyPart.Unequip();
					}
				}
				Equipped = null;
			}
			catch
			{
				_Equipped = null;
			}
		}
		if (_CurrentCell != null)
		{
			try
			{
				_CurrentCell.RemoveObject(ParentObject, Forced: false, System: false, IgnoreGravity: false, Silent: false, NoStack: false, Repaint: true, FlushTransient: true, null, null, null, null, null, null, E);
				_CurrentCell = null;
			}
			catch
			{
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TryRemoveFromContextEvent E)
	{
		if (GameObject.Validate(ref _InInventory))
		{
			Inventory inventory = _InInventory.Inventory;
			if (inventory == null || !inventory.FireEvent(Event.New("CommandRemoveObject", "Object", ParentObject).SetSilent(Silent: true), E))
			{
				return false;
			}
			if (_InInventory != null)
			{
				return false;
			}
		}
		if (GameObject.Validate(ref _Equipped))
		{
			_Equipped.FindEquippedObject(ParentObject)?.TryUnequip(Silent: true, SemiForced: false, NoStack: false, NoTake: true);
			if (_Equipped != null)
			{
				return false;
			}
		}
		if (_CurrentCell != null)
		{
			if (!_CurrentCell.RemoveObject(ParentObject, Forced: false, System: false, IgnoreGravity: false, Silent: false, NoStack: false, Repaint: true, FlushTransient: true, null, null, null, null, null, null, E))
			{
				return false;
			}
			if (_CurrentCell != null)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		bool flag = true;
		if (Solid && !E.IgnoresWalls && !E.WallWalker && (E.PhaseMatches(ParentObject) || (E.Autoexploring && Phase.IsTemporarilyPhasedOut(E.Actor))) && (!E.Flying || !ParentObject.HasTag("Flyover")) && !ParentObject.IsCombat)
		{
			flag = false;
			if (E.Smart)
			{
				if (ParentObject.TryGetPart<Forcefield>(out var Part))
				{
					E.Uncacheable = true;
					if (Part.CanPass(E.Actor))
					{
						flag = true;
					}
				}
				if (!flag && ParentObject.TryGetPart<Door>(out var Part2))
				{
					E.Uncacheable = true;
					if (Part2.CanPathThrough(E.Actor))
					{
						flag = true;
					}
				}
			}
			if (!flag && E.Burrower && ParentObject.IsDiggable())
			{
				if (OkayToDamageEvent.Check(ParentObject, E.Actor, out var WasWanted))
				{
					flag = true;
					int baseRating = 4 + ParentObject.Stat("AV");
					baseRating = GenericDeepRatingEvent.GetFor(ParentObject, "WallDigNavigationWeight", E.Actor, null, 0, baseRating);
					E.MinWeight(baseRating);
					E.MinSocialWeight(100);
				}
				if (WasWanted)
				{
					E.Uncacheable = true;
				}
			}
		}
		if (!flag)
		{
			E.MinWeight(100);
			return false;
		}
		if (Temperature > 200)
		{
			E.Uncacheable = true;
			int num = Temperature / 20;
			if (E.Smart && E.Actor != null)
			{
				int num2 = E.Actor.Stat("HeatResistance");
				if (num2 != 0)
				{
					num = num * (100 - num2) / 100;
				}
			}
			E.MinWeight(num, 98);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		CurrentCell = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSlotsRequiredEvent E)
	{
		if (E.Object == ParentObject && UsesTwoSlots)
		{
			E.Increases++;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E != null && E.Object?.IsPlayer() == true)
		{
			E.Object.PlayCombatSoundTag("StepSound", null, null, 0.25f);
		}
		return true;
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		if (Solid && !E.Object.HasTagOrProperty("IgnoresWalls"))
		{
			Brain brain = E.Object.Brain;
			if ((brain == null || !brain.WallWalker || E.Object.IsFlying) && (!E.Object.IsPlayer() || !The.Core.IDKFA) && (!ParentObject.HasPropertyOrTag("Flyover") || !E.Object.IsFlying) && E.Object.PhaseMatches(ParentObject) && (E.Ignore == null || E.Ignore != ParentObject))
			{
				eBeforePhysicsRejectObjectEntringCell.SetParameter("Object", E.Object);
				if (ParentObject.FireEvent(eBeforePhysicsRejectObjectEntringCell) && Solid && E.Object.FireEvent(Event.New("ObjectEnteringCellBlockedBySolid", "Object", ParentObject)) && Solid)
				{
					if (E.Object.IsPlayer() && !ParentObject.HasTagOrProperty("NoBlockMessage"))
					{
						if (E.Forced)
						{
							string text = "OUCH! You collide with " + ParentObject.an() + ".";
							if (The.Game.Player.Messages.LastLine != text)
							{
								IComponent<GameObject>.AddPlayerMessage(text);
							}
						}
						else if (E.Object.OnWorldMap())
						{
							if (ParentObject.HasTag("OverlandBlockMessage"))
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.GetTag("OverlandBlockMessage"));
							}
							else
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("are") + " too difficult to traverse via the world map. You'll have to find your way on the surface.");
							}
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("The way is blocked by " + ParentObject.an() + ".");
						}
					}
					E.Blocking = ParentObject;
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (E.SlotType == "Thrown Weapon" && E.Item == ParentObject && !E.RequireDesirable && !E.List.Contains(E.Item))
		{
			if (!E.RequirePossible)
			{
				E.List.Add(E.Item);
			}
			else if (E.Actor.IsGiganticCreature)
			{
				if (E.Item.IsGiganticEquipment || E.Item.HasPropertyOrTag("GiganticEquippable") || E.Item.IsNatural())
				{
					E.List.Add(E.Item);
				}
			}
			else if (!E.Item.IsGiganticEquipment || E.Item.IsNatural())
			{
				E.List.Add(E.Item);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!ParentObject.IsCombat)
		{
			if (Equipped != null)
			{
				if (!ParentObject.IsNatural() && !HasPropertyOrTag("NoRemoveOptionInInventory"))
				{
					E.AddAction("Remove", "remove", "Unequip", null, 'r', FireOnActor: true, 10);
				}
			}
			else if (Takeable)
			{
				bool flag = true;
				if (InInventory == null || !InInventory.IsPlayer())
				{
					if (ParentObject.IsTakeable())
					{
						E.AddAction("Get", "get", "CommandTakeObject", null, 'g', FireOnActor: true, 30, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: false);
					}
					else
					{
						flag = false;
					}
				}
				else if (!ParentObject.HasTagOrProperty("CannotDrop") && !The.Player.OnWorldMap())
				{
					E.AddAction("Drop", "drop", "CommandDropObject", null, 'd', FireOnActor: true);
					if (Options.DropAll)
					{
						E.AddAction("DropAll", "drop all", "CommandDropAllObject", null, 'D', FireOnActor: true);
					}
				}
				if (flag && !ParentObject.HasTagOrProperty("CannotEquip"))
				{
					E.AddAction("AutoEquip", "equip (auto)", "CommandAutoEquipObject", null, 'e', FireOnActor: true, GetAutoEquipPriorityEvent.GetFor(ParentObject), 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: false);
					E.AddAction("DoEquip", "equip (manual)", "CommandEquipObject", null, 'E', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: false);
				}
			}
		}
		if (IsAflame())
		{
			E.AddAction("Firefight", "fight fire", "FightFire", null, 'f', FireOnActor: false, 40);
		}
		if (CurrentCell != null && IsReal && ParentObject.HasStat("Hitpoints"))
		{
			E.AddAction("Attack", "attack", ATTACK_COMMAND, null, 'k', FireOnActor: false, -10);
		}
		GameObject equipped = Equipped;
		if (equipped == null || !equipped.IsPlayer())
		{
			GameObject inInventory = InInventory;
			if (inInventory == null || !inInventory.IsPlayer())
			{
				goto IL_02f9;
			}
		}
		if (!ParentObject.IsNatural() && !The.Player.IsConfused)
		{
			if (ItemModding.CanMod(ParentObject, The.Player))
			{
				E.AddAction("Mod", "mod with tinkering", "Mod", "tinkering", 't', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: true, null, ReturnToModernUI: true);
			}
			E.AddAction("Add Notes", "add notes", "AddNotes", null, 'n');
			if (ParentObject.IsImportant())
			{
				E.AddAction("Mark Unimportant", "mark unimportant", "MarkUnimportant", null, 'i', FireOnActor: false, -1);
			}
			else
			{
				E.AddAction("Mark Important", "mark important", "MarkImportant", null, 'i', FireOnActor: false, -1);
			}
		}
		goto IL_02f9;
		IL_02f9:
		if (ParentObject.HasPart<Notes>() && !The.Player.IsConfused)
		{
			E.AddAction("Remove Notes", "remove notes", "RemoveNotes", null, 'n', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
		}
		if (CheckAnythingToCleanEvent.Check(ParentObject) && CheckAnythingToCleanWithEvent.Check(The.Player, ParentObject))
		{
			E.AddAction("Clean", "clean", "CleanItem", null, 'a', FireOnActor: false, -1, 10);
		}
		if (Options.DebugInternals)
		{
			E.AddAction("Show Internals", "show internals", "ShowInternals", null, 'W', FireOnActor: false, -1, 0, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Unequip")
		{
			BodyPart bodyPart = E.Actor.Body?.FindEquippedItem(E.Item);
			if (bodyPart != null)
			{
				E.Actor.FireEvent(Event.New("CommandUnequipObject", "BodyPart", bodyPart));
			}
		}
		else if (E.Command == "CleanItem")
		{
			if (!E.Actor.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			string cleaningLiquidGeneralization = LiquidVolume.GetCleaningLiquidGeneralization();
			List<GameObject> list = GetCleaningItemsEvent.GetFor(E.Actor, ParentObject);
			if (list != null && list.Count > 0)
			{
				GameObject gameObject = PickItem.ShowPicker(list, null, PickItem.PickItemDialogStyle.SelectItemDialog, Title: "[ {{W|Choose where to use a dram of " + cleaningLiquidGeneralization + " from}} ]", Actor: E.Actor, Container: null, Cell: null, PreserveOrder: false, Regenerate: null, ShowContext: true);
				if (gameObject != null)
				{
					if (!gameObject.Owner.IsNullOrEmpty() && Popup.ShowYesNoCancel(gameObject.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " not owned by you. Are you sure you want to pour from " + gameObject.them + "?") != DialogResult.Yes)
					{
						return false;
					}
					if (CleanItemsEvent.PerformFor(E.Actor, ParentObject, gameObject, out var Objects, out var Types) && Objects != null && Objects.Count > 0)
					{
						E.Actor.PlayWorldSound("Sounds/Interact/sfx_interact_liquid_clean");
						LiquidVolume.CleaningMessage(E.Actor, Objects, Types, gameObject, null, UseDram: true);
						E.Actor.UseEnergy(1000, "Cleaning");
						E.RequestInterfaceExit();
						if (!gameObject.Owner.IsNullOrEmpty())
						{
							gameObject.Physics?.BroadcastForHelp(E.Actor);
						}
					}
				}
			}
			else
			{
				Popup.ShowFail("You don't have any " + cleaningLiquidGeneralization + " to clean " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " with.");
			}
		}
		else if (E.Command == "FightFire")
		{
			if (Firefighting.AttemptFirefighting(E.Actor, E.Item, 1000, Automatic: false, Dialog: true))
			{
				E.RequestInterfaceExit();
			}
		}
		else if (E.Command == ATTACK_COMMAND)
		{
			bool flag = true;
			if (E.Actor.IsPlayer() && !ParentObject.IsHostileTowards(E.Actor) && ParentObject != E.Actor.Target && Popup.ShowYesNo("Do you really want to attack " + ((ParentObject == E.Actor) ? E.Actor.itself : ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true)) + "?") != DialogResult.Yes)
			{
				flag = false;
			}
			if (flag)
			{
				Combat.AttackObject(E.Actor, ParentObject);
				E.RequestInterfaceExit();
			}
		}
		else if (E.Command == "Mod")
		{
			if (Options.ModernUI)
			{
				return false;
			}
			ScreenBuffer screenBuffer = ScreenBuffer.create(80, 25);
			screenBuffer.Copy(TextConsole.CurrentBuffer);
			new TinkeringScreen().Show(The.Player, E.Item, E);
			Popup._TextConsole.DrawBuffer(screenBuffer);
		}
		else if (E.Command == "MarkImportant")
		{
			E.Item.SetImportant(flag: true, force: true, player: true);
		}
		else if (E.Command == "MarkUnimportant")
		{
			E.Item.SetImportant(flag: false, force: true, player: true);
		}
		else if (E.Command == "AddNotes")
		{
			string text = Popup.AskString("Enter notes for " + E.Item.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".", "", "Sounds/UI/ui_notification", null, null, 30);
			if (!text.IsNullOrEmpty())
			{
				Notes notes = E.Item.RequirePart<Notes>();
				if (notes.Text.IsNullOrEmpty())
				{
					notes.Text = text;
				}
				else
				{
					notes.Text = notes.Text + "\n" + text;
				}
				Popup.Show("Notes added.", null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
			}
		}
		else if (E.Command == "RemoveNotes")
		{
			Notes part = E.Item.GetPart<Notes>();
			if (part != null)
			{
				ParentObject.RemovePart(part);
				Popup.Show("Notes removed.", null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
			}
			else
			{
				Popup.ShowFail("No notes found.");
			}
		}
		else if (E.Command == "ShowInternals")
		{
			Popup.Show(GetDebugInternalsEvent.GetFor(ParentObject), null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		Equipped = E.Actor;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		if (_Equipped == E.Actor)
		{
			_Equipped = null;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		if (E.Actor != null && E.Actor.IsPlayer())
		{
			InInventory = E.Actor;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		InInventory = null;
		return base.HandleEvent(E);
	}

	private bool ShouldUpdateTemperature()
	{
		if (ParentObject == null)
		{
			return false;
		}
		if (ParentObject.IsPlayer())
		{
			return true;
		}
		if (CurrentCell != null && CurrentCell.ParentZone != null && CurrentCell.ParentZone.IsActive())
		{
			return true;
		}
		if (Equipped != null && Equipped.IsPlayerControlled())
		{
			return true;
		}
		if (InInventory != null && InInventory.IsPlayerControlled())
		{
			return true;
		}
		if (CurrentCell == null && Equipped == null && InInventory == null)
		{
			GameObject objectContext = ParentObject.GetObjectContext();
			if (objectContext != null)
			{
				if (objectContext.IsPlayer())
				{
					return true;
				}
				GameObject equipped = objectContext.Equipped;
				if (equipped != null && equipped.IsPlayer())
				{
					return true;
				}
				GameObject inInventory = objectContext.InInventory;
				if (inInventory != null && inInventory.IsPlayer())
				{
					return true;
				}
				if ((equipped != null || inInventory != null) && (IsAflame() || IsFrozen()))
				{
					return true;
				}
			}
		}
		else if (IsAflame() || IsFrozen())
		{
			return true;
		}
		return false;
	}

	private void UpdateTemperature()
	{
		using (s_PhysicsUpdateTemperature.Auto())
		{
			if (IsReal && Temperature != AmbientTemperature && !ParentObject.IsScenery)
			{
				if (ParentObject.HasTag("Nullphase") || ParentObject.GetIntProperty("Nullphase") > 0)
				{
					Temperature = 25;
					return;
				}
				int intProperty = ParentObject.GetIntProperty("ThermalInsulation", 5);
				if (Temperature < AmbientTemperature - intProperty || Temperature > AmbientTemperature + intProperty)
				{
					int num = Temperature - AmbientTemperature;
					if (Temperature < AmbientTemperature)
					{
						num += intProperty;
						int num2 = Math.Max(5, (int)((double)(AmbientTemperature - Temperature) * 0.02));
						if (CanTemperatureReturnToAmbientEvent.Check(ParentObject, num2))
						{
							if (Temperature >= 25)
							{
								num2 = num2 * (100 - ParentObject.Stat("HeatResistance")) / 100;
							}
							if (num2 > 0)
							{
								Temperature += num2;
							}
						}
					}
					else if (Temperature > AmbientTemperature)
					{
						num -= intProperty;
						int num3 = Math.Max(5, (int)((double)(Temperature - AmbientTemperature) * 0.02));
						if (CanTemperatureReturnToAmbientEvent.Check(ParentObject, num3))
						{
							if (Temperature <= 25 && !ShouldRestAtAmbient())
							{
								num3 = num3 * (100 - ParentObject.Stat("ColdResistance")) / 100;
							}
							if (num3 > 0)
							{
								Temperature -= num3;
							}
						}
					}
					if (CurrentCell != null && !CurrentCell.OnWorldMap() && Temperature > AmbientTemperature)
					{
						int amount = AmbientTemperature + num;
						int phase = ParentObject.GetPhase();
						foreach (Cell localAdjacentCell in CurrentCell.GetLocalAdjacentCells())
						{
							localAdjacentCell.TemperatureChange(amount, InflamedBy, Radiant: true, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, phase);
						}
					}
				}
			}
			CheckThresholds();
		}
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		GameObject.Validate(ref LastDamagedBy);
		GameObject.Validate(ref LastWeaponDamagedBy);
		GameObject.Validate(ref LastProjectileDamagedBy);
		GameObject.Validate(ref InflamedBy);
		if (ShouldUpdateTemperature())
		{
			UpdateTemperature();
		}
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.NoConfusion && The.Player != null && The.Player.IsConfused)
		{
			bool flag = The.Player.GetFuriousConfusion() > 0;
			if (ConfusedName == null)
			{
				ConfusedName = NameMaker.MakeName(null, null, null, null, null, null, null, null, null, null, null, flag ? "FuriousConfusion" : "Confusion");
			}
			E.ReplaceEntirety(ConfusedName);
			if (flag)
			{
				E.AddColor("R");
			}
			return false;
		}
		if (!E.Reference)
		{
			if (IsFrozen())
			{
				E.AddAdjective("{{freezing|frozen}}", -40);
			}
			if (IsAflame())
			{
				if (ParentObject.HasEffect<CoatedInPlasma>())
				{
					E.AddAdjective("{{auroral|auroral}}", -40);
				}
				else
				{
					E.AddAdjective("{{fiery|flaming}}", -40);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		if (ParentObject != null)
		{
			E.AddEntry("GameObject", "Flags", ParentObject.GetFlagsDebugString());
			E.AddEntry("GameObject", "Blueprint", ParentObject.Blueprint);
			E.AddEntry("GameObject", "DisplayNameStripped", ParentObject.DisplayNameStripped);
			E.AddEntry("GameObject", "ShortDisplayNameStripped", ParentObject.ShortDisplayNameStripped);
			E.AddEntry("GameObject", "IsEMPSensitive", ParentObject.IsEMPSensitive());
			E.AddEntry("GameObject", "IsEquippedProperly", ParentObject.IsEquippedProperly());
			E.AddEntry("GameObject", "KineticAbsorption", ParentObject.GetKineticAbsorption());
			E.AddEntry("GameObject", "KineticResistance", ParentObject.GetKineticResistance());
			E.AddEntry("GameObject", "MaximumLiquidExposure", ParentObject.GetMaximumLiquidExposure());
			E.AddEntry("GameObject", "PathAsBurrower", ParentObject.PathAsBurrower);
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (ParentObject.Property != null && ParentObject.Property.Count > 0)
			{
				stringBuilder.Clear();
				List<string> list = new List<string>(ParentObject.Property.Keys);
				list.Sort();
				foreach (string item in list)
				{
					stringBuilder.Append(item).Append(": ").Append(ParentObject.Property[item] ?? "NULL")
						.Append('\n');
				}
				E.AddEntry("GameObject", "Property", stringBuilder.ToString());
			}
			if (ParentObject.IntProperty != null && ParentObject.IntProperty.Count > 0)
			{
				stringBuilder.Clear();
				List<string> list2 = new List<string>(ParentObject.IntProperty.Keys);
				list2.Sort();
				foreach (string item2 in list2)
				{
					stringBuilder.Append(item2).Append(": ").Append(ParentObject.IntProperty[item2])
						.Append('\n');
				}
				E.AddEntry("GameObject", "IntProperty", stringBuilder.ToString());
			}
			if (ParentObject.Statistics != null && ParentObject.Statistics.Count > 0)
			{
				stringBuilder.Clear();
				List<string> list3 = new List<string>(ParentObject.Statistics.Keys);
				list3.Sort();
				foreach (string item3 in list3)
				{
					stringBuilder.Append(item3).Append(": ").Append(ParentObject.Stat(item3) + " / " + ParentObject.BaseStat(item3))
						.Append('\n');
				}
				E.AddEntry("GameObject", "Statistics", stringBuilder.ToString());
			}
			if (!ParentObject._Effects.IsNullOrEmpty())
			{
				stringBuilder.Clear();
				foreach (Effect effect in ParentObject._Effects)
				{
					stringBuilder.Append(effect.GetType().GetName()).Append(": ").Append((effect.Duration == 9999) ? "Indefinite" : effect.Duration.ToString())
						.Append('\n');
				}
				E.AddEntry("GameObject", "Effects", stringBuilder.ToString());
			}
			E.AddEntry("GameObject", "Species", ParentObject.GetSpecies());
			E.AddEntry("Scanning", "ScanType", Scanning.GetScanTypeFor(ParentObject).ToString());
			if (CurrentCell != null && CurrentCell.ParentZone != null)
			{
				E.AddEntry("Cell", "LightLevel", CurrentCell.GetLight().ToString());
				E.AddEntry("Cell", "NavigationWeight", CurrentCell.GetNavigationWeightFor(The.Player));
				E.AddEntry("Cell", "AutoexploreNavigationWeight", CurrentCell.GetNavigationWeightFor(The.Player, Autoexploring: true));
			}
		}
		E.AddEntry(this, "Temperature", Temperature);
		E.AddEntry(this, "FlameTemperature", FlameTemperature);
		E.AddEntry(this, "VaporTemperature", VaporTemperature);
		E.AddEntry(this, "VaporObject", VaporObject);
		E.AddEntry(this, "FreezeTemperature", FreezeTemperature);
		E.AddEntry(this, "BaseElectricalConductivity", BaseElectricalConductivity);
		E.AddEntry(this, "ElectricalConductivity", ElectricalConductivity);
		E.AddEntry(this, "Flags", Flags);
		E.AddEntry(this, "Solid", Solid);
		E.AddEntry(this, "Takeable", Takeable);
		E.AddEntry(this, "IsReal", IsReal);
		E.AddEntry(this, "LastDamageAccidental", LastDamageAccidental);
		E.AddEntry(this, "WasFrozen", WasFrozen);
		E.AddEntry(this, "Organic", Organic);
		E.AddEntry(this, "UsesTwoSlots", UsesTwoSlots);
		E.AddEntry(this, "UsesSlots", UsesSlots);
		if (CurrentCell != null)
		{
			E.AddEntry(this, "Position", CurrentCell.X + ", " + CurrentCell.Y);
		}
		if (!Owner.IsNullOrEmpty())
		{
			E.AddEntry(this, "Owner", Owner);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryCategoryEvent E)
	{
		if (The.Player != null && The.Player.IsConfused && !E.AsIfKnown)
		{
			E.Category = "???";
			return false;
		}
		if (E.Category.IsNullOrEmpty())
		{
			E.Category = Category;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (E.Name == "Hitpoints")
		{
			if (E.NewValue < E.OldValue && E.NewValue - E.OldValue != E.NewBaseValue - E.OldBaseValue)
			{
				bool flag = ParentObject.IsPlayer();
				if (flag)
				{
					int hPWarningThreshold = Globals.HPWarningThreshold;
					if (hPWarningThreshold > E.NewValue * 100 / E.NewBaseValue && hPWarningThreshold <= E.OldValue * 100 / E.OldBaseValue)
					{
						The.Core.HPWarning = true;
					}
				}
				if (AutoAct.IsInterruptable())
				{
					if (flag)
					{
						AutoAct.Interrupt();
					}
					else if (ParentObject.IsPlayerLedAndPerceptible() && !ParentObject.IsTrifling)
					{
						AutoAct.Interrupt("you " + ParentObject.GetPerceptionVerb() + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: true) + " being injured" + (ParentObject.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(ParentObject))), null, ParentObject, IsThreat: true);
					}
				}
			}
			if (!ParentObject.WillCheckHP())
			{
				CheckHP(E.NewValue, E.OldValue, E.Stat.BaseValue);
			}
		}
		else if (E.Name == "HeatResistance" || E.Name == "ColdResistance")
		{
			AmbientCache = int.MinValue;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		LastDamagedBy = null;
		LastWeaponDamagedBy = null;
		LastProjectileDamagedBy = null;
		InflamedBy = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		AmbientCache = int.MinValue;
		if (IsReal && CurrentCell != null && CurrentCell.ParentZone != null)
		{
			int i = 0;
			for (int count = CurrentCell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = CurrentCell.Objects[i];
				if (gameObject == ParentObject || !ParentObject.ConsiderUnableToOccupySameCell(gameObject))
				{
					continue;
				}
				if (ParentObject.IsWall() && gameObject.IsWall() && !ParentObject.IsCombat && !gameObject.IsCombat)
				{
					if (ParentObject.GetTier() > gameObject.GetTier())
					{
						gameObject.Obliterate();
					}
					else
					{
						ParentObject.Obliterate();
					}
					break;
				}
				if (!CheckSpawnMergeEvent.Check(ParentObject, gameObject))
				{
					if (!GameObject.Validate(ParentObject) || ParentObject.IsNowhere())
					{
						break;
					}
					i = -1;
					count = CurrentCell.Objects.Count;
					continue;
				}
				foreach (Cell item in CurrentCell.YieldAdjacentCells(9, LocalOnly: true))
				{
					if (item.IsPassable(ParentObject) && !item.HasSpawnBlocker())
					{
						ParentObject.SystemMoveTo(item, null, forced: true);
						break;
					}
				}
				break;
			}
		}
		return base.HandleEvent(E);
	}

	public bool ProcessTakeDamage(Event E)
	{
		if (ParentObject == null)
		{
			return false;
		}
		if (ParentObject.IsPlayer() && The.Core.IDKFA)
		{
			return false;
		}
		bool flag = E.HasFlag("Indirect");
		Damage damage = E.GetParameter("Damage") as Damage;
		GameObject gameObject = E.GetGameObjectParameter("Source") ?? E.GetGameObjectParameter("Attacker") ?? E.GetGameObjectParameter("Owner");
		GameObject gameObject2 = E.GetGameObjectParameter("Owner") ?? E.GetGameObjectParameter("Attacker");
		GameObject gameObject3 = E.GetGameObjectParameter("DescribeAsFrom") ?? (flag ? gameObject : null) ?? gameObject2;
		GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
		GameObject gameObjectParameter2 = E.GetGameObjectParameter("Projectile");
		if (E.HasParameter("Phase"))
		{
			if (!ParentObject.PhaseMatches(E.GetIntParameter("Phase")))
			{
				return false;
			}
		}
		else if (gameObject != null && !ParentObject.PhaseMatches(gameObject))
		{
			return false;
		}
		LastWeaponDamagedBy = gameObjectParameter;
		LastProjectileDamagedBy = gameObjectParameter2;
		LastDamagedBy = gameObject2;
		if (GameObject.Validate(ref LastDamagedBy))
		{
			LastDamageAccidental = damage.HasAttribute("Accidental") && !LastDamagedBy.IsHostileTowards(ParentObject);
			if (LastDamagedBy.IsPlayer() && LastDamagedBy != ParentObject)
			{
				if (!flag && !E.HasFlag("NoSetTarget") && Sidebar.CurrentTarget == null && ParentObject.IsCombat && IComponent<GameObject>.Visible(ParentObject))
				{
					Sidebar.CurrentTarget = ParentObject;
				}
				if (The.Core.IDKFA)
				{
					damage.Amount = 999;
				}
			}
		}
		else
		{
			LastDamageAccidental = false;
		}
		if (!damage.HasAttribute("IgnoreResist"))
		{
			int num = ParentObject.Stat("AcidResistance");
			if (num != 0 && damage.Amount > 0 && damage.IsAcidDamage())
			{
				if (num > 0)
				{
					damage.Amount = (int)((float)damage.Amount * ((float)(100 - num) / 100f));
					if (num < 100 && damage.Amount < 1)
					{
						damage.Amount = 1;
					}
				}
				else if (num < 0)
				{
					damage.Amount += (int)((float)damage.Amount * ((float)num / -100f));
				}
			}
			int num2 = ParentObject.Stat("HeatResistance");
			if (num2 != 0 && damage.Amount > 0 && damage.IsHeatDamage())
			{
				if (num2 > 0)
				{
					damage.Amount = (int)((float)damage.Amount * ((float)(100 - num2) / 100f));
					if (num2 < 100 && damage.Amount < 1)
					{
						damage.Amount = 1;
					}
				}
				else if (num2 < 0)
				{
					damage.Amount += (int)((float)damage.Amount * ((float)num2 / -100f));
				}
			}
			int num3 = ParentObject.Stat("ColdResistance");
			if (num3 != 0 && damage.Amount > 0 && damage.IsColdDamage())
			{
				if (num3 > 0)
				{
					damage.Amount = (int)((float)damage.Amount * ((float)(100 - num3) / 100f));
					if (num3 < 100 && damage.Amount < 1)
					{
						damage.Amount = 1;
					}
				}
				else if (num3 < 0)
				{
					damage.Amount += (int)((float)damage.Amount * ((float)num3 / -100f));
				}
			}
			int num4 = ParentObject.Stat("ElectricResistance");
			if (num4 != 0 && damage.Amount > 0 && damage.IsElectricDamage())
			{
				if (num4 > 0)
				{
					damage.Amount = (int)((float)damage.Amount * ((float)(100 - num4) / 100f));
					if (num4 < 100 && damage.Amount < 1)
					{
						damage.Amount = 1;
					}
				}
				else if (num4 < 0)
				{
					damage.Amount += (int)((float)damage.Amount * ((float)num4 / -100f));
				}
			}
		}
		string stringParameter = E.GetStringParameter("Message", "");
		if (!BeforeApplyDamageEvent.Check(damage, ParentObject, gameObject2, gameObject, gameObjectParameter, gameObjectParameter2, flag, E))
		{
			return false;
		}
		if (!AttackerDealingDamageEvent.Check(damage, ParentObject, gameObject2, gameObject, gameObjectParameter, gameObjectParameter2, flag, E))
		{
			return false;
		}
		if (!LateBeforeApplyDamageEvent.Check(damage, ParentObject, gameObject2, gameObject, gameObjectParameter, gameObjectParameter2, flag, E))
		{
			return false;
		}
		string text = "killed";
		string value = "by";
		string text2 = null;
		bool flag2 = false;
		bool flag3 = LastDamageAccidental;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		if (damage.Amount > 0)
		{
			if (damage.IsHeatDamage())
			{
				if (damage.HasAttribute("NoBurn"))
				{
					LastDamagedByType = "Heat";
					text = "cooked";
				}
				else
				{
					LastDamagedByType = "Fire";
					text = "immolated";
				}
			}
			else if (damage.HasAttribute("Vaporized"))
			{
				LastDamagedByType = "Vaporized";
				text = "vaporized";
				flag3 = false;
			}
			else if (damage.IsColdDamage())
			{
				LastDamagedByType = "Cold";
				text = "frozen to death";
			}
			else if (damage.IsElectricDamage())
			{
				LastDamagedByType = "Electric";
				text = "electrocuted";
			}
			else if (damage.IsAcidDamage())
			{
				LastDamagedByType = "Acid";
				text = "dissolved";
			}
			else if (damage.IsDisintegrationDamage())
			{
				LastDamagedByType = "Disintegration";
				text = "disintegrated";
			}
			else if (damage.HasAttribute("Plasma"))
			{
				LastDamagedByType = "Plasma";
				text = "plasma-burned to death";
			}
			else if (damage.HasAttribute("Laser"))
			{
				LastDamagedByType = "Light";
				text = "lased to death";
			}
			else if (damage.HasAttribute("Light"))
			{
				LastDamagedByType = "Light";
				text = "illuminated to death";
			}
			else if (damage.HasAttribute("Poison"))
			{
				LastDamagedByType = "Poison";
				text = "died of poison";
				flag2 = true;
				value = "from";
			}
			else if (damage.HasAttribute("Bleeding"))
			{
				LastDamagedByType = "Bleeding";
				text = "bled to death";
				flag2 = true;
				value = "because of";
			}
			else if (damage.HasAttribute("Asphyxiation"))
			{
				LastDamagedByType = "Asphyxiation";
				text = "died of asphyxiation";
				flag2 = true;
				value = "from";
			}
			else if (damage.HasAttribute("Metabolic"))
			{
				LastDamagedByType = "Metabolic";
				text2 = "metabolism";
				text = "failed";
				value = "from";
			}
			else if (damage.HasAttribute("Drain"))
			{
				LastDamagedByType = "Drain";
				text2 = "vital essence was";
				text = "drained to extinction";
				value = "by";
			}
			else if (damage.HasAttribute("Psionic"))
			{
				LastDamagedByType = "Psionic";
				text = "psychically extinguished";
			}
			else if (damage.HasAttribute("Mental"))
			{
				LastDamagedByType = "Mental";
				text = "mentally obliterated";
			}
			else if (damage.HasAttribute("Thorns"))
			{
				LastDamagedByType = "Thorns";
				text = "pricked to death";
			}
			else if (damage.HasAttribute("Collision"))
			{
				LastDamagedByType = "Collision";
				value = "by colliding with";
			}
			else if (damage.HasAttribute("Bite"))
			{
				LastDamagedByType = "Bite";
				text = "bitten to death";
			}
			else
			{
				LastDamagedByType = ((damage.Attributes.Count > 0) ? damage.Attributes[0] : "Physical");
				flag6 = true;
			}
			if (damage.HasAttribute("Illusion"))
			{
				flag4 = true;
			}
			if (damage.HasAttribute("reflected"))
			{
				flag5 = true;
			}
			if (damage.HasAttribute("Neutron") && ParentObject.IsPlayer())
			{
				Achievement.CRUSHED_UNDER_SUNS.Unlock();
			}
		}
		LastDeathCategory = text;
		if (E.HasParameter("DeathReason"))
		{
			LastDeathReason = E.GetStringParameter("DeathReason");
			if (E.HasParameter("ThirdPersonDeathReason"))
			{
				LastThirdPersonDeathReason = E.GetStringParameter("ThirdPersonDeathReason");
			}
			else
			{
				LastThirdPersonDeathReason = GameText.RoughConvertSecondPersonToThirdPerson(LastDeathReason, ParentObject);
			}
		}
		else
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			if (flag4)
			{
				stringBuilder.Append("You convinced yourself ").Append((text2 != null) ? "your" : "you").Append(' ');
				stringBuilder2.Append(ParentObject.It).Append(" @@convinced ").Append(ParentObject.itself)
					.Append(' ')
					.Append((text2 != null) ? ParentObject.its : ParentObject.it)
					.Append(' ');
			}
			else
			{
				stringBuilder.Append((text2 != null) ? "Your " : "You ");
				stringBuilder2.Append((text2 != null) ? ParentObject.Its : ParentObject.It).Append(' ');
			}
			if (text2 != null)
			{
				stringBuilder.Append(text2).Append(' ');
				stringBuilder2.Append(text2).Append(' ');
			}
			else if (!flag2)
			{
				stringBuilder.Append("were ");
				stringBuilder2.Append(ParentObject.GetVerb("were", PrependSpace: false, PronounAntecedent: true)).Append(' ');
			}
			if (flag3 && text2 == null)
			{
				stringBuilder.Append("accidentally ");
				stringBuilder2.Append("accidentally ");
			}
			stringBuilder.Append(text);
			if (!flag4)
			{
				stringBuilder2.Append("@@");
			}
			stringBuilder2.Append(text);
			if (flag5)
			{
				stringBuilder.Append(" via reflected damage");
				stringBuilder2.Append(" via reflected damage");
			}
			if (gameObject2 != null && gameObject2 != ParentObject)
			{
				if (!value.IsNullOrEmpty())
				{
					stringBuilder.Append(' ').Append(value);
					stringBuilder2.Append(' ').Append(value);
				}
				stringBuilder.Append(' ');
				stringBuilder2.Append(' ');
				if (ParentObject.IsPlayer() && gameObject2.GetStringProperty("PlayerCopy") == "true" && gameObject2.HasStringProperty("PlayerCopyDescription"))
				{
					stringBuilder.Append(gameObject2.GetStringProperty("PlayerCopyDescription"));
					stringBuilder2.Append(gameObject2.GetStringProperty("PlayerCopyDescription"));
				}
				else
				{
					string value2 = gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: false);
					stringBuilder.Append(value2);
					stringBuilder2.Append(value2);
				}
			}
			if (flag6)
			{
				GameObject gameObject4 = gameObjectParameter2 ?? gameObjectParameter ?? gameObject;
				if (gameObject4 != null)
				{
					stringBuilder2.Append("## with ").Append(gameObject4.an()).Append("##");
				}
			}
			stringBuilder.Append('.');
			stringBuilder2.Append('.');
			LastDeathReason = stringBuilder.ToString();
			LastThirdPersonDeathReason = stringBuilder2.ToString();
		}
		if (IsPlayer())
		{
			The.Game.DeathReason = LastDeathReason;
			The.Game.DeathCategory = LastDeathCategory;
		}
		Statistic stat = ParentObject.GetStat("Hitpoints");
		if (stat == null)
		{
			return false;
		}
		bool flag7 = false;
		if (damage.Amount > 0)
		{
			int count = ParentObject.Count;
			ParentObject.SplitFromStack();
			if (count != ParentObject.Count)
			{
				flag7 = true;
			}
		}
		bool? _visible;
		if (!stringParameter.IsNullOrEmpty() && (damage.Amount > 0 || !E.HasFlag("SilentIfNoDamage")))
		{
			bool flag8 = false;
			_visible = null;
			if (ParentObject.IsPlayer())
			{
				flag8 = true;
			}
			else if (gameObject2 != null && gameObject2.IsPlayer() && (visible() ?? gameObject2.isAdjacentTo(ParentObject)))
			{
				flag8 = true;
			}
			else if (ParentObject.IsCombatObject() || E.HasFlag("ShowForInanimate"))
			{
				GameObject gameObjectParameter3 = E.GetGameObjectParameter("Perspective");
				if (gameObjectParameter3 != null && IComponent<GameObject>.Visible(gameObjectParameter3))
				{
					flag8 = true;
				}
				else if ((gameObject2 == null || E.HasFlag("ShowUninvolved")) && visible() == true)
				{
					flag8 = true;
				}
			}
			string stringParameter2 = E.GetStringParameter("ShowDamageType", "damage");
			if (flag8)
			{
				stringParameter = stringParameter.Replace("%d", damage.Amount.ToString());
				stringParameter = stringParameter.Replace("%e", stringParameter2);
				if (gameObject3 != null)
				{
					if (gameObject3.IsPlayer())
					{
						stringParameter = stringParameter.Replace("%o", "your");
						stringParameter = stringParameter.Replace("%O", "you");
						stringParameter = stringParameter.Replace("%S", " you started");
						stringParameter = stringParameter.Replace("%t", "your");
						stringParameter = stringParameter.Replace("%T", "you");
					}
					else if (gameObject3 == ParentObject)
					{
						if (stringParameter.Contains("%o"))
						{
							stringParameter = stringParameter.Replace("%o", gameObject3.its);
						}
						if (stringParameter.Contains("%O"))
						{
							stringParameter = stringParameter.Replace("%O", gameObject3.itself);
						}
						if (stringParameter.Contains("%S"))
						{
							stringParameter = stringParameter.Replace("%S", " started by " + gameObject3.itself);
						}
						if (stringParameter.Contains("%t"))
						{
							stringParameter = stringParameter.Replace("%t", gameObject3.its);
						}
						if (stringParameter.Contains("%T"))
						{
							stringParameter = stringParameter.Replace("%T", gameObject3.It);
						}
					}
					else
					{
						if (stringParameter.Contains("%o"))
						{
							stringParameter = stringParameter.Replace("%o", Grammar.MakePossessive(gameObject3.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, true)));
						}
						if (stringParameter.Contains("%O"))
						{
							stringParameter = stringParameter.Replace("%O", gameObject3.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true));
						}
						if (stringParameter.Contains("%S"))
						{
							stringParameter = stringParameter.Replace("%S", " started by " + gameObject3.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true));
						}
						if (stringParameter.Contains("%t"))
						{
							stringParameter = stringParameter.Replace("%t", Grammar.MakePossessive(gameObject3.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true)));
						}
						if (stringParameter.Contains("%T"))
						{
							stringParameter = stringParameter.Replace("%T", gameObject3.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true));
						}
					}
				}
				else
				{
					stringParameter = stringParameter.Replace("%o", "the");
					stringParameter = stringParameter.Replace("%O ", "");
					stringParameter = stringParameter.Replace("%S", "");
					stringParameter = stringParameter.Replace("%t", "the");
					stringParameter = stringParameter.Replace("%T ", "");
				}
				if (E.HasFlag("NoDamageMessage"))
				{
					EmitMessage(stringParameter, ' ', FromDialog: false, E.HasFlag("UsePopups"), AlwaysVisible: true);
				}
				else if (ParentObject.IsPlayer())
				{
					StringBuilder stringBuilder3 = Event.NewStringBuilder();
					stringBuilder3.Append("{{r|You take ");
					if (damage.Amount > 0)
					{
						stringBuilder3.Append(damage.Amount).Append(' ').Append(stringParameter2);
					}
					else
					{
						stringBuilder3.Append("no damage");
					}
					stringBuilder3.Append(' ').Append(stringParameter).Append("}}");
					EmitMessage(stringBuilder3.ToString(), ' ', FromDialog: false, E.HasFlag("UsePopups"), AlwaysVisible: true);
				}
				else if (ParentObject.IsCombat || E.HasFlag("ShowForInanimate"))
				{
					StringBuilder stringBuilder4 = Event.NewStringBuilder();
					stringBuilder4.Append(ParentObject.T()).Append(' ').Append(ParentObject.GetVerb("take", PrependSpace: false))
						.Append(' ');
					if (damage.Amount > 0)
					{
						stringBuilder4.Append(damage.Amount).Append(' ').Append(stringParameter2);
					}
					else
					{
						stringBuilder4.Append("no damage");
					}
					stringBuilder4.Append(' ').Append(stringParameter);
					EmitMessage(stringBuilder4.ToString(), ' ', FromDialog: false, E.HasFlag("UsePopups"), AlwaysVisible: true);
				}
			}
		}
		AttackerDealtDamageEvent.Send(damage, ParentObject, gameObject2, gameObject, gameObjectParameter, gameObjectParameter2, flag, E);
		if (damage.Amount > 0)
		{
			if (ParentObject.IsPlayer())
			{
				if (AutoAct.IsInterruptable())
				{
					AutoAct.Interrupt();
				}
			}
			else
			{
				if (!Owner.IsNullOrEmpty())
				{
					CheckBroadcastForHelp(gameObject2, LastDamageAccidental);
				}
				if (AutoAct.IsInterruptable() && ParentObject.IsPlayerLedAndPerceptible() && !ParentObject.IsTrifling)
				{
					AutoAct.Interrupt("you " + ParentObject.GetPerceptionVerb() + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: true) + " being injured" + (ParentObject.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(ParentObject))), null, ParentObject, IsThreat: true);
				}
				ParentObject.PlayCombatSoundTag("TakeDamageSound");
			}
			if (ParentObject.IsFrozen())
			{
				PlayWorldSound("Sounds/Damage/sfx_damage_ice");
			}
			if (juiceEnabled && GameObject.Validate(ParentObject) && (ParentObject.IsPlayer() || (gameObject2 != null && gameObject2.IsPlayer() && gameObject2.isAdjacentTo(ParentObject)) || Visible()))
			{
				float scale = 1f;
				if (stat.BaseValue > 0)
				{
					float num5 = (float)damage.Amount / (float)stat.BaseValue;
					if ((double)num5 >= 0.5)
					{
						scale = 1.8f;
					}
					else if ((double)num5 >= 0.4)
					{
						scale = 1.6f;
					}
					else if ((double)num5 >= 0.3)
					{
						scale = 1.4f;
					}
					else if ((double)num5 >= 0.2)
					{
						scale = 1.2f;
					}
					else if ((double)num5 >= 0.1)
					{
						scale = 1.1f;
					}
				}
				CombatJuice.floatingText(ParentObject, "-" + damage.Amount, The.Color.Red, 1.5f, 24f, scale);
			}
			BeforeTookDamageEvent.Send(damage, ParentObject, gameObject2, gameObject, gameObjectParameter, gameObjectParameter2, flag, E);
			if (damage.Amount != 0)
			{
				stat.Penalty += damage.Amount;
			}
			if (GameObject.Validate(ParentObject))
			{
				TookDamageEvent.Send(damage, ParentObject, gameObject2, gameObject, gameObjectParameter, gameObjectParameter2, flag, E);
				if (GameObject.Validate(ParentObject) && damage.HasAttribute("Environmental"))
				{
					TookEnvironmentalDamageEvent.Send(damage, ParentObject, gameObject2, gameObject, flag, E);
				}
			}
		}
		if (flag7)
		{
			ParentObject?.CheckStack();
		}
		return true;
		bool? visible()
		{
			if (!_visible.HasValue)
			{
				_visible = E.HasFlag("IgnoreVisibility") || Visible();
			}
			return _visible;
		}
	}

	public bool ProcessTargetedMove(Cell TargetCell, string Type, string PreEvent, string PostEvent, int? EnergyCost = null, bool Forced = false, bool System = false, bool IgnoreCombat = false, bool IgnoreGravity = false, bool NoStack = false, bool UsePopups = false, string LeaveVerb = null, string ArriveVerb = null, GameObject Ignore = null)
	{
		Cell cell = CurrentCell;
		if (cell == TargetCell)
		{
			return true;
		}
		if (!ParentObject.IsPlayer() || TutorialManager.BeforePlayerEnterCell(TargetCell))
		{
			if (TargetCell != null && TargetCell.HasObjectWithTagOrProperty("NoTeleport"))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show(TargetCell.GetFirstObjectWithPropertyOrTag("NoTeleport").GetPropertyOrTag("NoTeleport"));
				}
			}
			else
			{
				Zone zone = cell?.ParentZone;
				int amount = EnergyCost ?? 1000;
				if ((ParentObject.FireEvent(Event.New(PreEvent, "Cell", TargetCell, Type, 1)) || System) && ParentObject.ProcessBeginMove(out var _, TargetCell, Forced, System, IgnoreGravity, NoStack, null, Type, null, null, null, Ignore))
				{
					if (ParentObject.IsCombat && !IgnoreCombat)
					{
						foreach (GameObject item in TargetCell.GetObjectsWithPartReadonly("Combat"))
						{
							if (item.IsHostileTowards(ParentObject))
							{
								if (!ParentObject.IsPlayer())
								{
									continue;
								}
								if (!(The.Core.PlayerWalking == ""))
								{
									The.Core.RenderBase();
									continue;
								}
								Combat.AttackCell(ParentObject, TargetCell);
							}
							else if (ParentObject.IsPlayer() && Popup.ShowYesNo("Do you really want to attack " + item.DisplayName + "?") == DialogResult.Yes)
							{
								Combat.AttackCell(ParentObject, TargetCell);
							}
							goto IL_03ad;
						}
					}
					cell = CurrentCell;
					zone = cell?.ParentZone;
					GameObject Blocking = null;
					if (ParentObject.ProcessObjectLeavingCell(cell, ref Blocking, Forced, System, IgnoreGravity, NoStack, null, Type, null, null, null, Ignore) && ParentObject.ProcessEnteringCell(TargetCell, ref Blocking, Forced, System, IgnoreGravity, NoStack, null, Type, null, null, null, Ignore) && ParentObject.ProcessObjectEnteringCell(TargetCell, ref Blocking, Forced, System, IgnoreGravity, NoStack, null, Type, null, null, null, Ignore) && (zone == TargetCell.ParentZone || ParentObject.ProcessEnteringZone(cell, TargetCell, ref Blocking, Forced, System, IgnoreGravity, NoStack, null, Type, null, null, null, Ignore)) && ParentObject.ProcessLeaveCell(cell, ref Blocking, Forced, System, IgnoreGravity, NoStack, null, Type, null, null, null, Ignore))
					{
						if (!ParentObject.IsPlayer() && !LeaveVerb.IsNullOrEmpty())
						{
							DidX(LeaveVerb, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopups);
						}
						cell?.RemoveObject(ParentObject, Forced, System, IgnoreGravity, Silent: false, NoStack, Repaint: true, FlushTransient: true, null, Type, null, null, null, Ignore);
						TargetCell.AddObject(ParentObject, Forced, System, IgnoreGravity, NoStack, Silent: false, Repaint: true, FlushTransient: true, null, Type, null, null, null, Ignore);
						if (!ParentObject.IsPlayer() && !ArriveVerb.IsNullOrEmpty())
						{
							DidX(ArriveVerb, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: true, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true, AlwaysVisible: false, FromDialog: false, UsePopups);
						}
						ParentObject.UseEnergy(amount, "Movement");
						if (IsPlayer() && zone != TargetCell.ParentZone)
						{
							The.ZoneManager.SetActiveZone(TargetCell.ParentZone);
							The.ZoneManager.ProcessGoToPartyLeader();
						}
						ParentObject.FireEvent(Event.New(PostEvent, "FromCell", cell, Type, 1));
						return true;
					}
				}
			}
		}
		goto IL_03ad;
		IL_03ad:
		AfterMoveFailedEvent.Send(ParentObject, cell, TargetCell, Forced, System, IgnoreGravity, NoStack, null, Type, null, null, null, Ignore);
		return false;
	}

	public void TeardownForDestroy(bool MoveToGraveyard = true, bool Silent = false)
	{
		Zone zone = CurrentZone;
		GameObject equipped = Equipped;
		if (equipped != null)
		{
			if (zone == null)
			{
				zone = equipped.FastGetCurrentCell()?.ParentZone;
			}
			Body body = equipped.Body;
			if (body != null)
			{
				int num = 0;
				BodyPart bodyPart;
				while ((bodyPart = body.FindDefaultOrEquippedItem(ParentObject)) != null)
				{
					if (++num >= 100)
					{
						MetricsManager.LogError("infinite looping trying to unequip " + ParentObject.DebugName);
						break;
					}
					if (bodyPart.Equipped == ParentObject)
					{
						Event obj = Event.New("CommandForceUnequipObject");
						obj.SetParameter("BodyPart", bodyPart);
						obj.SetFlag("NoTake", State: true);
						obj.SetSilent(Silent: true);
						equipped.FireEvent(obj);
					}
					if (bodyPart.DefaultBehavior == ParentObject)
					{
						bodyPart.DefaultBehavior = null;
					}
				}
			}
		}
		equipped = InInventory;
		if (equipped != null)
		{
			if (zone == null)
			{
				zone = equipped.FastGetCurrentCell()?.ParentZone;
			}
			equipped.Inventory?.RemoveObject(ParentObject);
		}
		Cell cell = CurrentCell;
		if (cell != null)
		{
			if (cell.ParentZone != null && ParentObject.WantEvent(PooledEvent<CheckExistenceSupportEvent>.ID, CheckExistenceSupportEvent.CascadeLevel))
			{
				cell.ParentZone.WantSynchronizeExistence();
			}
			cell.RemoveObject(ParentObject);
		}
		The.ActionManager?.RemoveActiveObject(ParentObject);
		if (MoveToGraveyard && !ParentObject.IsInGraveyard())
		{
			if (zone != null)
			{
				zone.Graveyard.Add(ParentObject);
			}
			else
			{
				The.Graveyard?.Add(ParentObject);
			}
		}
	}

	public bool ProcessTemperatureChange(int Amount, GameObject Actor = null, bool Radiant = false, bool MinAmbient = false, bool MaxAmbient = false, bool IgnoreResistance = false, int Phase = 0, int? Min = null, int? Max = null)
	{
		if (!IsReal)
		{
			return false;
		}
		if (SpecificHeat == 0f)
		{
			return false;
		}
		if (Phase == 0)
		{
			if (Actor != null)
			{
				Phase = Actor.GetPhase();
			}
			if (Phase == 0)
			{
				Phase = 5;
			}
		}
		if (!ParentObject.PhaseMatches(Phase))
		{
			return false;
		}
		Amount = BeforeTemperatureChangeEvent.GetFor(ParentObject, Amount, Actor, Radiant, MinAmbient, MaxAmbient, IgnoreResistance, Phase, Min, Max);
		if (Amount == 0)
		{
			return false;
		}
		Amount = AttackerBeforeTemperatureChangeEvent.GetFor(ParentObject, Amount, Actor, Radiant, MinAmbient, MaxAmbient, IgnoreResistance, Phase, Min, Max);
		if (Amount == 0)
		{
			return false;
		}
		int temperature = Temperature;
		if (Radiant)
		{
			if (!IgnoreResistance)
			{
				if (Amount > 0)
				{
					int num = Math.Min(100, Math.Max(0, ParentObject.Stat("HeatResistance")));
					if (num != 0)
					{
						Amount = (int)((float)Amount * ((float)(100 - num) / 100f));
					}
				}
				else if (Amount < 0)
				{
					int num2 = Math.Min(100, Math.Max(0, ParentObject.Stat("ColdResistance")));
					if (num2 != 0)
					{
						Amount = (int)((float)Amount * ((float)(100 - num2) / 100f));
					}
				}
			}
			if (Temperature > Amount)
			{
				if (!IsAflame())
				{
					Temperature += (int)((float)(Amount - Temperature) * (0.035f / SpecificHeat));
				}
			}
			else
			{
				Temperature += (int)((float)(Amount - Temperature) * (0.035f / SpecificHeat));
			}
		}
		else
		{
			Amount = (int)((float)Amount / SpecificHeat);
			if (!IgnoreResistance)
			{
				if (ParentObject.TryGetPart<Mutations>(out var Part) && Part.HasMutation("Fatty Hump"))
				{
					Amount /= 2;
				}
				if (Amount > 0 && Temperature + Amount > 50)
				{
					int num3 = ParentObject.Stat("HeatResistance");
					if (num3 != 0)
					{
						Amount = (int)((float)Amount * ((float)(100 - num3) / 100f));
					}
				}
				else if (Amount < 0 && Temperature + Amount < 25)
				{
					int num4 = ParentObject.Stat("ColdResistance");
					if (num4 != 0)
					{
						Amount = (int)((float)Amount * ((float)(100 - num4) / 100f));
					}
				}
			}
			Temperature += Amount;
		}
		if (MinAmbient && Temperature < AmbientTemperature)
		{
			Temperature = AmbientTemperature;
		}
		if (MaxAmbient && Temperature > AmbientTemperature)
		{
			Temperature = AmbientTemperature;
		}
		if (Min.HasValue && Temperature < Min)
		{
			Temperature = Min.Value;
		}
		if (Max.HasValue && Temperature > Max)
		{
			Temperature = Max.Value;
		}
		if (Temperature != temperature)
		{
			EnvironmentalUpdateEvent.Send(ParentObject);
		}
		CheckThresholds(Actor);
		return true;
	}

	public void CheckThresholds(GameObject Actor = null)
	{
		if (IsVaporizing())
		{
			GameObject.Validate(ref InflamedBy);
			if (Actor == null)
			{
				Actor = InflamedBy;
			}
			if (VaporizedEvent.Check(ParentObject, Actor))
			{
				LastDamagedByType = "Vaporized";
				LastDamageAccidental = true;
				if (ParentObject.IsPlayer())
				{
					Achievement.VAPORIZED.Unlock();
				}
				ParentObject.Die(Actor, null, "You were vaporized.", ParentObject.Does("were", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " @@vaporized.", Accidental: true);
				if (CurrentCell != null && !VaporObject.IsNullOrEmpty())
				{
					GameObject gameObject = GameObject.Create(VaporObject, 0, 0, null, null, null, "Vaporization");
					if (Actor != null && gameObject.TryGetPart<Gas>(out var Part))
					{
						Part.Creator = Actor;
					}
					CurrentCell.AddObject(gameObject);
				}
			}
		}
		else if (IsAflame())
		{
			if (!WasAflame)
			{
				WasAflame = true;
				InflamedBy = Actor;
				Burning.ApplyTo(ParentObject);
			}
		}
		else if (WasAflame)
		{
			WasAflame = false;
			ParentObject.RemoveEffect<Burning>();
		}
		if (IsFrozen())
		{
			if (!WasFrozen)
			{
				WasFrozen = true;
				InflamedBy = Actor;
				Frozen.ApplyTo(ParentObject);
			}
		}
		else if (WasFrozen)
		{
			WasFrozen = false;
			ParentObject.RemoveEffect<Frozen>();
		}
		if (IsFreezing())
		{
			if (!WasFreezing)
			{
				WasFreezing = true;
				InflamedBy = Actor;
				Freezing.ApplyTo(ParentObject);
			}
		}
		else if (WasFreezing)
		{
			WasFreezing = false;
			ParentObject.RemoveEffect<Freezing>();
		}
	}

	public bool CheckHP(int? CurrentHP = null, int? PreviousHP = null, int? MaxHP = null, bool Preregistered = false)
	{
		if (Preregistered)
		{
			ParentObject.WillCheckHP(false);
		}
		if ((CurrentHP ?? ParentObject.hitpoints) <= 0)
		{
			bool flag = ParentObject.IsPlayer();
			if (GameObject.Validate(ref LastDamagedBy))
			{
				if (LastDeathReason.IsNullOrEmpty())
				{
					if (LastDamagedBy == ParentObject)
					{
						LastDeathReason = "You " + (LastDamageAccidental ? "accidentally " : "") + "killed " + ParentObject.itself + ".";
					}
					else
					{
						LastDeathReason = "You were " + (LastDamageAccidental ? "accidentally " : "") + "killed by " + LastDamagedBy.an() + ".";
					}
				}
				if (LastThirdPersonDeathReason.IsNullOrEmpty())
				{
					if (LastDamagedBy == ParentObject)
					{
						LastThirdPersonDeathReason = ParentObject.It + " @@" + (LastDamageAccidental ? "accidentally " : "") + "killed " + ParentObject.itself + ".";
					}
					else
					{
						LastThirdPersonDeathReason = ParentObject.It + " were @@" + (LastDamageAccidental ? "accidentally " : "") + "killed by " + LastDamagedBy.an() + ".";
					}
				}
				if (flag)
				{
					if (LastDamagedBy.HasProperty("EvilTwin") && LastDamagedBy.HasProperty("PlayerCopy"))
					{
						Achievement.KILLED_BY_TWIN.Unlock();
					}
					if (LastDamagedBy.Blueprint == "Chute Crab")
					{
						Achievement.KILLED_BY_CHUTE_CRAB.Unlock();
					}
					if (LastDamagedBy.IsPlayerLed())
					{
						Achievement.DIE_COMPANION.Unlock();
					}
				}
			}
			if (flag)
			{
				if (LastDeathReason != null && LastDeathReason.EndsWith("crags of spacetime."))
				{
					Achievement.DIE_NORMALITY.Unlock();
				}
				if (LastDamagedByType == "Dessicated")
				{
					Achievement.DIE_THIRST.Unlock();
				}
				if (The.Game.DeathReason.IsNullOrEmpty() && !LastDeathReason.IsNullOrEmpty())
				{
					The.Game.DeathReason = LastDeathReason;
				}
			}
			ParentObject.Die(LastDamagedBy, null, LastDeathReason, DeathCategory: LastDeathCategory, ThirdPersonReason: LastThirdPersonDeathReason, Accidental: LastDamageAccidental, Weapon: LastWeaponDamagedBy, Projectile: LastProjectileDamagedBy);
			return true;
		}
		if ((!PreviousHP.HasValue || CurrentHP < PreviousHP) && CurrentHP <= (MaxHP ?? ParentObject.baseHitpoints) / 4 && !ParentObject.IsCreature && ParentObject.HasTagOrProperty("Breakable"))
		{
			ParentObject.ForceApplyEffect(new Broken(FromDamage: true));
		}
		return false;
	}

	public bool RestoreContextRelation(GameObject Object, GameObject ObjectContext, Cell CellContext, BodyPart BodyPartContext, int Relation, bool Silent = true)
	{
		if (Relation == 3 && BodyPartContext != null)
		{
			if (BodyPartContext.Equipped == Object && Object.Equipped == ObjectContext)
			{
				return true;
			}
			if (BodyPartContext.Equip(Object, 0, Silent, ForDeepCopy: false, Forced: true))
			{
				return true;
			}
		}
		else if (Relation == 2 && ObjectContext != null)
		{
			if (Object.InInventory == ObjectContext)
			{
				return true;
			}
			if (ObjectContext.Inventory?.AddObject(Object, null, Silent) == Object)
			{
				return true;
			}
		}
		else if (Relation == 1 && CellContext != null)
		{
			if (Object.CurrentCell == CellContext)
			{
				return true;
			}
			if (CellContext.AddObject(Object, Forced: false, System: true, IgnoreGravity: false, NoStack: false, Silent: false, Repaint: true, FlushTransient: true, null, "Context Relation Restore") == Object)
			{
				return true;
			}
		}
		return false;
	}

	private Cell GetBroadcastCell(GameObject Target)
	{
		if (!GameObject.Validate(ref Target))
		{
			return null;
		}
		if (Owner == null)
		{
			return null;
		}
		if (ParentObject.Brain != null)
		{
			return null;
		}
		if (Target.Owns(Owner) && !Target.IsPlayerControlled())
		{
			return null;
		}
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return null;
		}
		return cell;
	}

	public void CheckBroadcastForHelp(GameObject Target, bool Accidental = false)
	{
		if (GetBroadcastCell(Target) == null)
		{
			return;
		}
		double howMuch = (Accidental ? 0.3 : 0.5);
		if (!ParentObject.isDamaged(howMuch))
		{
			string name = "HelpBroadcastChecksFor" + Target.ID;
			int num = (Accidental ? 5 : 3);
			if (ParentObject.ModIntProperty(name, 1) < num)
			{
				return;
			}
		}
		BroadcastForHelp(Target);
	}

	public void BroadcastForHelp(GameObject Target)
	{
		if (GetBroadcastCell(Target) != null)
		{
			AIHelpBroadcastEvent.Send(ParentObject, Target, null, null, 20, 1f, HelpCause.Trespass);
		}
	}

	public override void BasisError(GameObject Basis, SerializationReader Reader)
	{
		if (Basis.GetBlueprint(UseDefault: false) == null)
		{
			if (Basis.IsPlayer())
			{
				Basis.Blueprint = "Humanoid";
			}
			else
			{
				Reader.FlagObjectForRemoval(Basis);
			}
		}
	}
}
