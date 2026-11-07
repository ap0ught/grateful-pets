using System;
using System.Collections.Generic;
using System.Text;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class Body : IPart
{
	[Serializable]
	public class DismemberedPart
	{
		public BodyPart Part;

		public int ParentID;

		public DismemberedPart()
		{
		}

		public DismemberedPart(BodyPart P, BodyPart ParentPart)
			: this()
		{
			Part = P;
			if (ParentPart != null)
			{
				ParentID = ParentPart.ID;
			}
		}

		public void Save(SerializationWriter Writer)
		{
			Part.Write(Writer);
			Writer.WriteOptimized(ParentID);
		}

		public void Load(SerializationReader Reader)
		{
			Part = new BodyPart();
			Part.Read(Reader);
			ParentID = Reader.ReadOptimizedInt32();
		}

		public bool IsReattachable(Body ParentBody)
		{
			return ParentBody.GetPartByID(ParentID) != null;
		}

		public void Reattach(Body ParentBody)
		{
			BodyPart bodyPart = ParentBody.GetPartByID(ParentID) ?? throw new Exception("cannot reattach, parent " + ParentID + " missing");
			ParentBody.DismemberedParts.Remove(this);
			bodyPart.AddPart(Part, Part.Position, DoUpdate: false);
			ParentBody.RecalculateTypeArmor(Part.Type);
		}

		public void CheckRenumberingOnPositionAssignment(BodyPart Parent, int AssignedPosition, BodyPart ExceptPart = null)
		{
			if (Parent.IDMatch(ParentID) && Part.Position >= AssignedPosition && Part != ExceptPart)
			{
				Part.Position++;
			}
		}

		public bool HasPosition(BodyPart Parent, int Position, BodyPart ExceptPart = null)
		{
			if (Parent.IDMatch(ParentID) && Part.Position == Position)
			{
				return Part != ExceptPart;
			}
			return false;
		}
	}

	public const int MAXIMUM_MOBILITY_MOVE_SPEED_PENALTY = 60;

	public const int BASIC_FULL_MOBILITY = 2;

	public int MobilitySpeedPenaltyApplied;

	[NonSerialized]
	public BodyPart _Body;

	[NonSerialized]
	public int RequiredMobility;

	public bool built;

	[NonSerialized]
	public List<DismemberedPart> DismemberedParts;

	public string _Anatomy;

	[NonSerialized]
	private static Event eBodypartsUpdated = new ImmutableEvent("BodypartsUpdated");

	[NonSerialized]
	public static Dictionary<GameObject, BodyPart> DeepCopyEquipMap = null;

	public int WeightCache = -1;

	[NonSerialized]
	public static List<BodyPart> partStatic = new List<BodyPart>();

	[NonSerialized]
	private static List<BodyPart> parts2 = new List<BodyPart>();

	[NonSerialized]
	private static List<BodyPart> _bodyParts = new List<BodyPart>();

	[NonSerialized]
	private static bool _bodyPartsInUse;

	private static List<BodyPart> AllBodyParts = new List<BodyPart>(16);

	private static List<BodyPart> SomeBodyParts = new List<BodyPart>(4);

	private static List<BodyPart> OtherBodyParts = new List<BodyPart>(4);

	public string Anatomy
	{
		get
		{
			return _Anatomy;
		}
		set
		{
			Anatomies.GetAnatomyOrFail(value).ApplyTo(this);
		}
	}

	public override int Priority => 90000;

	public override void Initialize()
	{
		if (_Anatomy.IsNullOrEmpty())
		{
			string tag = ParentObject.GetTag("BodyType");
			if (!tag.IsNullOrEmpty())
			{
				Anatomy = tag;
			}
		}
		if (RequiredMobility <= 0 || _Anatomy.IsNullOrEmpty())
		{
			return;
		}
		int totalMobility = GetTotalMobility();
		if (totalMobility < RequiredMobility)
		{
			(GetFirstPart((BodyPart x) => x.Mobility > 0) ?? GetFirstPart((BodyPart x) => x.TypeModel().Mobility > 0) ?? GetBody()).Mobility += RequiredMobility - totalMobility;
		}
	}

	public override void Attach()
	{
		ParentObject.Body = this;
	}

	public override void Remove()
	{
		if (ParentObject?.Body == this)
		{
			ParentObject.Body = null;
		}
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		_Body.Write(Writer);
		if (DismemberedParts == null)
		{
			Writer.Write(0);
		}
		else
		{
			Writer.Write(DismemberedParts.Count);
			if (DismemberedParts.Count > 0)
			{
				foreach (DismemberedPart item in new List<DismemberedPart>(DismemberedParts))
				{
					item.Save(Writer);
				}
			}
		}
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		_Body = new BodyPart(this);
		_Body.Read(Reader);
		int num = Reader.ReadInt32();
		if (num > 0)
		{
			DismemberedParts = new List<DismemberedPart>(num);
			for (int i = 0; i < num; i++)
			{
				DismemberedPart dismemberedPart = new DismemberedPart();
				dismemberedPart.Load(Reader);
				DismemberedParts.Add(dismemberedPart);
			}
		}
		else
		{
			DismemberedParts = null;
		}
		base.Read(Basis, Reader);
	}

	public void Clear()
	{
		_Body.Clear();
	}

	[Obsolete("version with MapInv argument should always be called")]
	public override IPart DeepCopy(GameObject Parent)
	{
		return base.DeepCopy(Parent);
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		DeepCopyEquipMap = new Dictionary<GameObject, BodyPart>(8);
		Body body = base.DeepCopy(Parent, MapInv) as Body;
		body.built = false;
		body._Body = _Body.DeepCopy(Parent, body, null, MapInv);
		body.built = true;
		body.UpdateBodyParts();
		return body;
	}

	public override void FinalizeCopy(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopy(Source, CopyEffects, CopyID, MapInv);
		if (ParentObject.DeepCopyInventoryObjectMap != null && DeepCopyEquipMap != null)
		{
			foreach (GameObject key in DeepCopyEquipMap.Keys)
			{
				if (!key.IsValid())
				{
					continue;
				}
				if (key.IsImplant)
				{
					if (key.HasTag("CyberneticsUsesEqSlot"))
					{
						key.Physics._Equipped = ParentObject;
						DeepCopyEquipMap[key].DoEquip(key);
					}
					DeepCopyEquipMap[key].Implant(key, ForDeepCopy: true);
				}
				else
				{
					key.Physics._Equipped = ParentObject;
					DeepCopyEquipMap[key].DoEquip(key, Silent: false, ForDeepCopy: true);
				}
			}
		}
		DeepCopyEquipMap = null;
	}

	public int GetWeight()
	{
		if (WeightCache == -1)
		{
			return RecalculateWeight();
		}
		return WeightCache;
	}

	public int RecalculateWeight()
	{
		WeightCache = _Body.GetWeight();
		return WeightCache;
	}

	public void FlushWeightCache()
	{
		WeightCache = -1;
		ParentObject.FlushCarriedWeightCache();
	}

	public string GetPrimaryLimbType()
	{
		return ParentObject.GetPropertyOrTag("PrimaryLimbType", "Hand");
	}

	public GameObject GetShield(Predicate<GameObject> Filter = null, GameObject Attacker = null)
	{
		GameObject obj = null;
		_Body.GetShield(ref obj, Filter, Attacker, ParentObject);
		return obj;
	}

	public GameObject GetShieldWithHighestAV(Predicate<GameObject> Filter = null, GameObject Attacker = null)
	{
		GameObject obj = null;
		int highestAV = int.MinValue;
		_Body.GetShieldWithHighestAV(ref obj, ref highestAV, Filter, Attacker, ParentObject);
		return obj;
	}

	public GameObject GetMainWeapon(out int PossibleWeapons, out BodyPart PrimaryWeaponPart, GameObject Target = null, bool NeedPrimary = false, bool FailDownFromPrimary = false, List<BodyPart> PartList = null)
	{
		PrimaryWeaponPart = null;
		GameObject Weapon = null;
		bool HadPrimary = false;
		int PickPriority = 0;
		PossibleWeapons = 0;
		_Body.ScanForWeapon(NeedPrimary, Target, ref Weapon, ref PrimaryWeaponPart, ref PickPriority, ref PossibleWeapons, ref HadPrimary, PartList);
		if (NeedPrimary && !FailDownFromPrimary && !HadPrimary)
		{
			return null;
		}
		return Weapon;
	}

	public GameObject GetMainWeapon(GameObject Target = null, bool NeedPrimary = false, bool FailDownFromPrimary = false)
	{
		int PossibleWeapons;
		BodyPart PrimaryWeaponPart;
		return GetMainWeapon(out PossibleWeapons, out PrimaryWeaponPart, Target, NeedPrimary, FailDownFromPrimary);
	}

	public bool HasWeaponOfType(string Type, bool NeedPrimary = false)
	{
		return _Body.HasWeaponOfType(Type, NeedPrimary);
	}

	public bool HasPrimaryWeaponOfType(string Type)
	{
		return HasWeaponOfType(Type, NeedPrimary: true);
	}

	public bool HasWeaponOfTypeOnBodyPartOfType(string WeaponType, string BodyPartType, bool NeedPrimary = false)
	{
		return _Body.HasWeaponOfTypeOnBodyPartOfType(WeaponType, BodyPartType, NeedPrimary);
	}

	public GameObject GetWeaponOfType(string Type, bool NeedPrimary = false, bool PreferPrimary = false)
	{
		if (PreferPrimary && !NeedPrimary)
		{
			GameObject weaponOfType = _Body.GetWeaponOfType(Type, NeedPrimary: true);
			if (weaponOfType != null)
			{
				return weaponOfType;
			}
		}
		return _Body.GetWeaponOfType(Type, NeedPrimary);
	}

	public GameObject GetPrimaryWeaponOfType(string Type)
	{
		return GetWeaponOfType(Type, NeedPrimary: true);
	}

	public GameObject GetPrimaryWeaponOfType(string Type, bool AcceptFirstHandForNonHandPrimary)
	{
		GameObject weaponOfType = GetWeaponOfType(Type, NeedPrimary: true);
		if (weaponOfType != null)
		{
			return weaponOfType;
		}
		if (AcceptFirstHandForNonHandPrimary && GetPrimaryLimbType() != "Hand")
		{
			BodyPart firstPart = GetFirstPart("Hand");
			if (firstPart != null)
			{
				weaponOfType = firstPart.ThisPartWeaponOfType(Type, NeedPrimary: false);
				if (weaponOfType != null)
				{
					return weaponOfType;
				}
			}
		}
		return null;
	}

	public GameObject GetWeaponOfTypeOnBodyPartOfType(string WeaponType, string BodyPartType, bool NeedPrimary = false, bool PreferPrimary = false)
	{
		if (PreferPrimary && !NeedPrimary)
		{
			GameObject weaponOfTypeOnBodyPartOfType = _Body.GetWeaponOfTypeOnBodyPartOfType(WeaponType, BodyPartType, NeedPrimary: true);
			if (weaponOfTypeOnBodyPartOfType != null)
			{
				return weaponOfTypeOnBodyPartOfType;
			}
		}
		return _Body.GetWeaponOfTypeOnBodyPartOfType(WeaponType, BodyPartType, NeedPrimary);
	}

	public GameObject GetPrimaryWeaponOfTypeOnBodyPartOfType(string WeaponType, string BodyPartType)
	{
		return GetWeaponOfTypeOnBodyPartOfType(WeaponType, BodyPartType, NeedPrimary: true);
	}

	public GameObject GetWeapon(Predicate<GameObject> Filter = null)
	{
		return _Body.GetWeapon(Filter);
	}

	public bool HasWeapon(Predicate<GameObject> Filter = null)
	{
		return _Body.HasWeapon(Filter);
	}

	public bool IsPrimaryWeapon(GameObject Object)
	{
		return _Body.IsPrimaryWeapon(Object);
	}

	public void ClearShieldBlocks()
	{
		_Body.ClearShieldBlocks();
	}

	public List<GameObject> StripAllEquipment()
	{
		List<GameObject> list = new List<GameObject>();
		_Body.StripAllEquipment(list);
		return list;
	}

	public List<GameObject> GetPrimaryHandEquippedObjects()
	{
		List<GameObject> list = new List<GameObject>();
		_Body.GetPrimaryHandEquippedObjects(list);
		return list;
	}

	public List<GameObject> GetPrimaryEquippedObjects()
	{
		List<GameObject> list = new List<GameObject>();
		_Body.GetPrimaryEquippedObjects(list);
		return list;
	}

	public void GetEquippedObjects(List<GameObject> Return)
	{
		_Body.GetEquippedObjects(Return);
	}

	public void GetEquippedObjects(List<GameObject> Return, Predicate<GameObject> Filter)
	{
		_Body.GetEquippedObjects(Return, Filter);
	}

	public List<GameObject> GetEquippedObjects()
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount());
		_Body.GetEquippedObjects(list);
		return list;
	}

	public List<GameObject> GetEquippedObjects(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetEquippedObjects();
		}
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount(Filter));
		_Body.GetEquippedObjects(list, Filter);
		return list;
	}

	public List<GameObject> GetEquippedObjectsReadonly()
	{
		return _Body.GetEquippedObjectsReadonly();
	}

	public List<GameObject> GetEquippedObjectsReadonly(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetEquippedObjectsReadonly();
		}
		return _Body.GetEquippedObjectsReadonly(Filter);
	}

	public void GetEquippedObjectsExceptNatural(IList<GameObject> Return)
	{
		_Body.GetEquippedObjectsExceptNatural(Return);
	}

	public List<GameObject> GetEquippedObjectsExceptNatural()
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount());
		_Body.GetEquippedObjectsExceptNatural(list);
		return list;
	}

	public List<GameObject> GetEquippedObjectsExceptNatural(Predicate<GameObject> Filter)
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount(Filter));
		_Body.GetEquippedObjectsExceptNatural(list, Filter);
		return list;
	}

	public void ForeachEquippedObject(Action<GameObject> Proc)
	{
		_Body.ForeachEquippedObject(Proc);
	}

	public void SafeForeachEquippedObject(Action<GameObject> Proc)
	{
		_Body.SafeForeachEquippedObject(Proc);
	}

	public void ForeachDefaultBehavior(Action<GameObject> Proc)
	{
		_Body.ForeachDefaultBehavior(Proc);
	}

	public void SafeForeachDefaultBehavior(Action<GameObject> Proc)
	{
		_Body.SafeForeachDefaultBehavior(Proc);
	}

	public bool HasInstalledCybernetics(string Blueprint = null, Predicate<GameObject> Filter = null)
	{
		return _Body.HasInstalledCybernetics(Blueprint, Filter);
	}

	public void GetInstalledCybernetics(List<GameObject> Return)
	{
		_Body.GetInstalledCybernetics(Return);
	}

	public List<GameObject> GetInstalledCybernetics()
	{
		List<GameObject> list = new List<GameObject>(_Body.GetInstalledCyberneticsCount());
		_Body.GetInstalledCybernetics(list);
		return list;
	}

	public List<GameObject> GetInstalledCyberneticsReadonly()
	{
		return _Body.GetInstalledCyberneticsReadonly();
	}

	public List<GameObject> GetInstalledCybernetics(Predicate<GameObject> Filter)
	{
		List<GameObject> list = new List<GameObject>(_Body.GetInstalledCyberneticsCount(Filter));
		_Body.GetInstalledCybernetics(list, Filter);
		return list;
	}

	public bool AnyInstalledCybernetics()
	{
		return _Body.AnyInstalledCybernetics();
	}

	public bool AnyInstalledCybernetics(Predicate<GameObject> pFilter)
	{
		return _Body.AnyInstalledCybernetics(pFilter);
	}

	public void ForeachInstalledCybernetics(Action<GameObject> aProc)
	{
		_Body.ForeachInstalledCybernetics(aProc);
	}

	public void SafeForeachInstalledCybernetics(Action<GameObject> aProc)
	{
		_Body.SafeForeachInstalledCybernetics(aProc);
	}

	public void GetEquippedObjectsAndInstalledCybernetics(List<GameObject> Return)
	{
		_Body.GetEquippedObjectsAndInstalledCybernetics(Return);
	}

	public List<GameObject> GetEquippedObjectsAndInstalledCybernetics()
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount() + _Body.GetInstalledCyberneticsCount());
		_Body.GetEquippedObjectsAndInstalledCybernetics(list);
		return list;
	}

	public List<GameObject> GetEquippedObjectsAndInstalledCyberneticsReadonly()
	{
		return _Body.GetEquippedObjectsAndInstalledCyberneticsReadonly();
	}

	public List<GameObject> GetEquippedObjectsAndInstalledCybernetics(Predicate<GameObject> Filter)
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount(Filter) + _Body.GetInstalledCyberneticsCount(Filter));
		_Body.GetEquippedObjectsAndInstalledCybernetics(list, Filter);
		return list;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public bool HasPrimaryHand()
	{
		foreach (BodyPart item in GetPart("Hand"))
		{
			if (item.Primary)
			{
				return true;
			}
		}
		return false;
	}

	public BodyPart GetBody()
	{
		return _Body;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		_Body.ToString(stringBuilder);
		return stringBuilder.ToString();
	}

	public bool AnyRegisteredEvent(string ID)
	{
		return _Body.AnyRegisteredEvent(ID);
	}

	public BodyPart GetPartByManager(string Manager, bool EvenIfDismembered = false)
	{
		BodyPart bodyPart = _Body.FindByManager(Manager);
		if (bodyPart != null)
		{
			return bodyPart;
		}
		if (EvenIfDismembered && DismemberedParts != null)
		{
			for (int num = DismemberedParts.Count - 1; num >= 0; num--)
			{
				if (DismemberedParts[num].Part.Manager == Manager)
				{
					return DismemberedParts[num].Part;
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByManager(string Manager, string Type, bool EvenIfDismembered = false)
	{
		BodyPart bodyPart = _Body.FindByManager(Manager, Type);
		if (bodyPart != null)
		{
			return bodyPart;
		}
		if (EvenIfDismembered && DismemberedParts != null)
		{
			for (int num = DismemberedParts.Count - 1; num >= 0; num--)
			{
				if (DismemberedParts[num].Part.Manager == Manager && DismemberedParts[num].Part.Type == Type)
				{
					return DismemberedParts[num].Part;
				}
			}
		}
		return null;
	}

	public void GetPartsByManager(string Manager, List<BodyPart> Store, bool EvenIfDismembered = false)
	{
		_Body.FindByManager(Manager, Store);
		if (!EvenIfDismembered || DismemberedParts == null)
		{
			return;
		}
		for (int num = DismemberedParts.Count - 1; num >= 0; num--)
		{
			if (DismemberedParts[num].Part.Manager == Manager)
			{
				Store.Add(DismemberedParts[num].Part);
			}
		}
	}

	public void GetPartsByManager(string Manager, string Type, List<BodyPart> Store, bool EvenIfDismembered = false)
	{
		_Body.FindByManager(Manager, Store);
		if (!EvenIfDismembered || DismemberedParts == null)
		{
			return;
		}
		for (int num = DismemberedParts.Count - 1; num >= 0; num--)
		{
			if (DismemberedParts[num].Part.Manager == Manager && DismemberedParts[num].Part.Type == Type)
			{
				Store.Add(DismemberedParts[num].Part);
			}
		}
	}

	public int RemovePartsByManager(string Manager, bool EvenIfDismembered = false)
	{
		int num = 0;
		BodyPart partByManager;
		while ((partByManager = GetPartByManager(Manager)) != null)
		{
			RemovePart(partByManager, DoUpdate: false);
			num++;
		}
		if (EvenIfDismembered && DismemberedParts != null)
		{
			for (int num2 = DismemberedParts.Count - 1; num2 >= 0; num2--)
			{
				if (DismemberedParts[num2].Part.Manager == Manager)
				{
					DismemberedParts.RemoveAt(num2);
					num++;
				}
			}
		}
		if (num > 0)
		{
			UpdateBodyParts();
			RecalculateArmor();
		}
		return num;
	}

	public void FindPartsEquipping(GameObject GO, List<BodyPart> Return)
	{
		_Body.FindPartsEquipping(GO, Return);
	}

	public List<BodyPart> FindPartsEquipping(GameObject GO)
	{
		List<BodyPart> list = new List<BodyPart>();
		FindPartsEquipping(GO, list);
		return list;
	}

	public BodyPart FindParentPartOf(BodyPart FindPart)
	{
		return _Body.FindParentPartOf(FindPart);
	}

	public BodyPart FindPreviousPartOf(BodyPart FindPart)
	{
		return _Body.FindPreviousPartOf(FindPart);
	}

	public BodyPart FindNextPartOf(BodyPart FindPart)
	{
		return _Body.FindNextPartOf(FindPart);
	}

	public BodyPart FindEquippedItem(GameObject GO)
	{
		return _Body.FindEquippedItem(GO);
	}

	public BodyPart FindEquippedItem(string Blueprint)
	{
		return _Body.FindEquippedItem(Blueprint);
	}

	public BodyPart FindDefaultOrEquippedItem(GameObject GO)
	{
		return _Body.FindDefaultOrEquippedItem(GO);
	}

	public BodyPart FindDefaultOrEquippedItem(string Blueprint)
	{
		return _Body.FindDefaultOrEquippedItem(Blueprint);
	}

	public GameObject FindEquipmentOrDefaultByBlueprint(string Blueprint)
	{
		return _Body.FindEquipmentOrDefaultByBlueprint(Blueprint);
	}

	public GameObject FindEquipmentOrDefaultByID(string ID)
	{
		return _Body.FindEquipmentOrDefaultByID(ID);
	}

	public GameObject FindEquipmentByEvent(string ID)
	{
		return _Body.FindEquipmentByEvent(ID);
	}

	public GameObject FindEquipmentByEvent(Event E)
	{
		return _Body.FindEquipmentByEvent(E);
	}

	public GameObject FindEquipmentOrCyberneticsByEvent(string ID)
	{
		return _Body.FindEquipmentOrCyberneticsByEvent(ID);
	}

	public GameObject FindEquipmentOrCyberneticsByEvent(Event E)
	{
		return _Body.FindEquipmentOrCyberneticsByEvent(E);
	}

	public GameObject FindEquippedItem(Predicate<GameObject> Filter)
	{
		return _Body.FindEquippedItem(Filter);
	}

	public bool HasEquippedItem(GameObject GO)
	{
		return _Body.HasEquippedItem(GO);
	}

	public bool HasEquippedItem(string Blueprint)
	{
		return _Body.HasEquippedItem(Blueprint);
	}

	public bool HasEquippedItem(Predicate<GameObject> Filter)
	{
		return _Body.HasEquippedItem(Filter);
	}

	public bool IsItemEquippedOnLimbType(GameObject GO, string FindType)
	{
		return _Body.IsItemEquippedOnLimbType(GO, FindType);
	}

	public BodyPart FindCybernetics(GameObject GO)
	{
		return _Body.FindCybernetics(GO);
	}

	public bool IsItemImplantedInLimbType(GameObject GO, string FindType)
	{
		return _Body.IsItemImplantedInLimbType(GO, FindType);
	}

	public bool IsADefaultBehavior(GameObject obj)
	{
		return _Body.IsADefaultBehavior(obj);
	}

	public bool IsItemDefaultBehaviorOnLimbType(GameObject GO, string FindType)
	{
		return _Body.IsItemDefaultBehaviorOnLimbType(GO, FindType);
	}

	public BodyPart FindDefaultBehavior(GameObject GO)
	{
		return _Body.FindDefaultBehavior(GO);
	}

	public GameObject FindObject(Predicate<GameObject> pFilter)
	{
		return _Body.GetPart((BodyPart P) => P.Equipped != null && pFilter(P.Equipped))?.Equipped;
	}

	public GameObject FindObjectByBlueprint(string Blueprint)
	{
		return _Body.GetPart((BodyPart P) => P.Equipped != null && P.Equipped.Blueprint == Blueprint)?.Equipped;
	}

	public void ForeachPart(Action<BodyPart> aProc)
	{
		_Body.ForeachPart(aProc);
	}

	public bool ForeachPart(Predicate<BodyPart> pProc)
	{
		return _Body.ForeachPart(pProc);
	}

	public List<BodyPart> GetEquippedParts()
	{
		int partCount = GetPartCount((BodyPart P) => P.Equipped != null);
		List<BodyPart> Return = new List<BodyPart>(partCount);
		if (partCount > 0)
		{
			_Body.ForeachPart((BodyPart P) =>
			{
				if (P.Equipped != null)
				{
					Return.Add(P);
				}
			});
		}
		return Return;
	}

	public void GetParts(List<BodyPart> Return)
	{
		_Body.GetParts(Return);
	}

	public void GetParts(List<BodyPart> Return, bool EvenIfDismembered)
	{
		_Body.GetParts(Return);
		if (!EvenIfDismembered || DismemberedParts == null || DismemberedParts.Count <= 0)
		{
			return;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			Return.Add(dismemberedPart.Part);
		}
		Return.Sort(BodyPart.Sort);
	}

	public List<BodyPart> GetParts()
	{
		List<BodyPart> list = new List<BodyPart>(GetPartCount());
		GetParts(list);
		return list;
	}

	public List<BodyPart> GetParts(bool EvenIfDismembered)
	{
		List<BodyPart> list = new List<BodyPart>(GetPartCount());
		GetParts(list, EvenIfDismembered);
		return list;
	}

	public IEnumerable<BodyPart> LoopParts()
	{
		foreach (BodyPart item in _Body.LoopParts())
		{
			yield return item;
		}
	}

	public void GetTopLevelDynamicParts(List<BodyPart> Return)
	{
		_Body.GetTopLevelDynamicParts(Return);
	}

	public List<BodyPart> GetTopLevelDynamicParts()
	{
		List<BodyPart> list = new List<BodyPart>();
		GetTopLevelDynamicParts(list);
		return list;
	}

	public void GetPartsSkippingDynamicTrees(List<BodyPart> Return)
	{
		_Body.GetPartsSkippingDynamicTrees(Return);
	}

	public List<BodyPart> GetPartsSkippingDynamicTrees()
	{
		List<BodyPart> list = new List<BodyPart>();
		GetPartsSkippingDynamicTrees(list);
		return list;
	}

	public void GetConcreteParts(List<BodyPart> Return)
	{
		_Body.GetConcreteParts(Return);
	}

	public List<BodyPart> GetConcreteParts()
	{
		List<BodyPart> list = new List<BodyPart>(GetConcretePartCount());
		GetConcreteParts(list);
		return list;
	}

	public void GetAbstractParts(List<BodyPart> Return)
	{
		_Body.GetAbstractParts(Return);
	}

	public List<BodyPart> GetAbstractParts()
	{
		List<BodyPart> list = new List<BodyPart>(GetAbstractPartCount());
		GetAbstractParts(list);
		return list;
	}

	public int GetPartCount()
	{
		return _Body.GetPartCount();
	}

	public int GetPartCount(string RequiredType)
	{
		return _Body.GetPartCount(RequiredType);
	}

	public int GetPartCount(string RequiredType, int RequiredLaterality)
	{
		return _Body.GetPartCount(RequiredType, RequiredLaterality);
	}

	public int GetPartCount(Predicate<BodyPart> Filter)
	{
		return _Body.GetPartCount(Filter);
	}

	public int GetConcretePartCount()
	{
		return _Body.GetConcretePartCount();
	}

	public int GetAbstractPartCount()
	{
		return _Body.GetAbstractPartCount();
	}

	public bool AnyCategoryParts(int FindCategory)
	{
		return _Body.AnyCategoryParts(FindCategory);
	}

	public int GetCategoryPartCount(int FindCategory)
	{
		return _Body.GetCategoryPartCount(FindCategory);
	}

	public int GetCategoryPartCount(int FindCategory, string FindType)
	{
		return _Body.GetCategoryPartCount(FindCategory, FindType);
	}

	public int GetCategoryPartCount(int FindCategory, Predicate<BodyPart> pFilter)
	{
		return _Body.GetCategoryPartCount(FindCategory, pFilter);
	}

	public int GetNativePartCount()
	{
		return _Body.GetNativePartCount();
	}

	public int GetNativePartCount(string RequiredType)
	{
		return _Body.GetNativePartCount(RequiredType);
	}

	public int GetNativePartCount(Predicate<BodyPart> pFilter)
	{
		return _Body.GetNativePartCount(pFilter);
	}

	public int GetAddedPartCount()
	{
		return _Body.GetAddedPartCount();
	}

	public int GetAddedPartCount(string RequiredType)
	{
		return _Body.GetAddedPartCount(RequiredType);
	}

	public int GetAddedPartCount(Predicate<BodyPart> pFilter)
	{
		return _Body.GetAddedPartCount(pFilter);
	}

	public int GetMortalPartCount()
	{
		return _Body.GetMortalPartCount();
	}

	public int GetMortalPartCount(string RequiredType)
	{
		return _Body.GetMortalPartCount(RequiredType);
	}

	public int GetMortalPartCount(Predicate<BodyPart> pFilter)
	{
		return _Body.GetMortalPartCount(pFilter);
	}

	public bool AnyMortalParts()
	{
		return _Body.AnyMortalParts();
	}

	public bool IsEquippedOnType(GameObject FindObj, string FindType)
	{
		return _Body.IsEquippedOnType(FindObj, FindType);
	}

	public bool IsEquippedOnCategory(GameObject FindObj, int FindCategory)
	{
		return _Body.IsEquippedOnCategory(FindObj, FindCategory);
	}

	public bool IsEquippedOnPrimary(GameObject FindObj)
	{
		return _Body.IsEquippedOnPrimary(FindObj);
	}

	public bool IsImplantedInCategory(GameObject FindObj, int FindCategory)
	{
		return _Body.IsImplantedInCategory(FindObj, FindCategory);
	}

	public void GetMobilityProvidingParts(List<BodyPart> Return)
	{
		_Body.GetMobilityProvidingParts(Return);
	}

	public List<BodyPart> GetMobilityProvidingParts()
	{
		List<BodyPart> list = new List<BodyPart>();
		GetMobilityProvidingParts(list);
		return list;
	}

	public void GetConcreteMobilityProvidingParts(List<BodyPart> Return)
	{
		_Body.GetConcreteMobilityProvidingParts(Return);
	}

	public List<BodyPart> GetConcreteMobilityProvidingParts()
	{
		List<BodyPart> list = new List<BodyPart>();
		GetConcreteMobilityProvidingParts(list);
		return list;
	}

	public int GetTotalMobility()
	{
		return _Body.GetTotalMobility();
	}

	public int GetBodyMobility()
	{
		return _Body.Mobility;
	}

	public void MarkAllNative()
	{
		_Body.MarkAllNative();
	}

	public void CategorizeAll(int ApplyCategory)
	{
		_Body.CategorizeAll(ApplyCategory);
	}

	public void CategorizeAllExcept(int ApplyCategory, int SkipCategory)
	{
		_Body.CategorizeAllExcept(ApplyCategory, SkipCategory);
	}

	public void GetPartsEquippedOn(GameObject obj, List<BodyPart> Result)
	{
		_Body.GetPartsEquippedOn(obj, Result);
	}

	public List<BodyPart> GetPartsEquippedOn(GameObject obj)
	{
		return _Body.GetPartsEquippedOn(obj);
	}

	public int GetPartCountEquippedOn(GameObject obj)
	{
		return _Body.GetPartCountEquippedOn(obj);
	}

	public List<BodyPart> GetUnequippedPart(string RequiredType)
	{
		List<BodyPart> list = new List<BodyPart>();
		_Body.GetUnequippedPart(RequiredType, list);
		return list;
	}

	public List<BodyPart> GetUnequippedPart(string RequiredType, int RequiredLaterality)
	{
		List<BodyPart> list = new List<BodyPart>();
		_Body.GetUnequippedPart(RequiredType, RequiredLaterality, list);
		return list;
	}

	public int GetUnequippedPartCount(string RequiredType)
	{
		return _Body.GetUnequippedPartCount(RequiredType);
	}

	public int GetUnequippedPartCount(string RequiredType, int RequiredLaterality)
	{
		return _Body.GetUnequippedPartCount(RequiredType, RequiredLaterality);
	}

	public int GetUnequippedPartCountExcept(string RequiredType, BodyPart ExceptPart)
	{
		return _Body.GetUnequippedPartCountExcept(RequiredType, ExceptPart);
	}

	public int GetUnequippedPartCountExcept(string RequiredType, int RequiredLaterality, BodyPart ExceptPart)
	{
		return _Body.GetUnequippedPartCountExcept(RequiredType, RequiredLaterality, ExceptPart);
	}

	public List<BodyPart> GetPartStatic(string RequiredType)
	{
		if (partStatic == null)
		{
			partStatic = new List<BodyPart>(4);
		}
		else
		{
			partStatic.Clear();
		}
		_Body.GetPart(RequiredType, partStatic);
		return partStatic;
	}

	public List<BodyPart> GetPart(string RequiredType)
	{
		List<BodyPart> list = new List<BodyPart>(GetPartCount(RequiredType));
		_Body.GetPart(RequiredType, list);
		return list;
	}

	public List<BodyPart> GetPart(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetPart(RequiredType);
		}
		List<BodyPart> list = new List<BodyPart>(GetPartCount(RequiredType, RequiredLaterality));
		_Body.GetPart(RequiredType, RequiredLaterality, list);
		return list;
	}

	public List<BodyPart> GetPart(string RequiredType, bool EvenIfDismembered)
	{
		List<BodyPart> list = new List<BodyPart>(GetPartCount(RequiredType));
		_Body.GetPart(RequiredType, list);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Type == RequiredType || dismemberedPart.Part.VariantType == RequiredType)
				{
					list.Add(dismemberedPart.Part);
				}
			}
		}
		return list;
	}

	public List<BodyPart> GetPart(string RequiredType, int RequiredLaterality, bool EvenIfDismembered)
	{
		if (RequiredLaterality == 65535)
		{
			return GetPart(RequiredType, EvenIfDismembered);
		}
		List<BodyPart> list = new List<BodyPart>(GetPartCount(RequiredType, RequiredLaterality));
		_Body.GetPart(RequiredType, RequiredLaterality, list);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if ((dismemberedPart.Part.Type == RequiredType || dismemberedPart.Part.VariantType == RequiredType) && Laterality.Match(dismemberedPart.Part, RequiredLaterality))
				{
					list.Add(dismemberedPart.Part);
				}
			}
		}
		return list;
	}

	public IEnumerable<BodyPart> LoopPart(string RequiredType)
	{
		foreach (BodyPart item in _Body.LoopPart(RequiredType))
		{
			yield return item;
		}
	}

	public IEnumerable<BodyPart> LoopPart(string RequiredType, int RequiredLaterality)
	{
		foreach (BodyPart item in _Body.LoopPart(RequiredType, RequiredLaterality))
		{
			yield return item;
		}
	}

	public BodyPart GetFirstPart()
	{
		return _Body;
	}

	public BodyPart GetFirstPart(string RequiredType)
	{
		return _Body.GetFirstPart(RequiredType);
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality)
	{
		return _Body.GetFirstPart(RequiredType, RequiredLaterality);
	}

	public BodyPart GetFirstPart(Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstPart(Filter);
	}

	public BodyPart GetFirstPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstPart(RequiredType, Filter);
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstPart(RequiredType, RequiredLaterality, Filter);
	}

	public BodyPart GetFirstPart(string RequiredType, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstPart(RequiredType);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Type == RequiredType && dismemberedPart.Part.Position < num)
				{
					result = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return result;
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstPart(RequiredType, RequiredLaterality);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Type == RequiredType && dismemberedPart.Part.Position < num && Laterality.Match(dismemberedPart.Part, RequiredLaterality))
				{
					result = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return result;
	}

	public BodyPart GetFirstPart(Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstPart(Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Position < num && Filter(dismemberedPart.Part))
				{
					result = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return result;
	}

	public BodyPart GetFirstPart(string RequiredType, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstPart(RequiredType, Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Type == RequiredType && dismemberedPart.Part.Position < num && Filter(dismemberedPart.Part))
				{
					result = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return result;
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstPart(RequiredType, RequiredLaterality, Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Type == RequiredType && dismemberedPart.Part.Position < num && Laterality.Match(dismemberedPart.Part, RequiredLaterality) && Filter(dismemberedPart.Part))
				{
					result = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return result;
	}

	public BodyPart GetFirstVariantPart(string RequiredType)
	{
		return _Body.GetFirstVariantPart(RequiredType);
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality)
	{
		return _Body.GetFirstVariantPart(RequiredType, RequiredLaterality);
	}

	public BodyPart GetFirstVariantPart(Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstVariantPart(Filter);
	}

	public BodyPart GetFirstVariantPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstVariantPart(RequiredType, Filter);
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstVariantPart(RequiredType, RequiredLaterality, Filter);
	}

	public BodyPart GetFirstVariantPart(string RequiredType, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstVariantPart(RequiredType);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.VariantType == RequiredType && dismemberedPart.Part.Position < num)
				{
					result = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return result;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstVariantPart(RequiredType, RequiredLaterality);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.VariantType == RequiredType && dismemberedPart.Part.Position < num && Laterality.Match(dismemberedPart.Part, RequiredLaterality))
				{
					result = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return result;
	}

	public BodyPart GetFirstVariantPart(Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstVariantPart(Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Position < num && Filter(dismemberedPart.Part))
				{
					result = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return result;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstVariantPart(RequiredType, Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.VariantType == RequiredType && dismemberedPart.Part.Position < num && Filter(dismemberedPart.Part))
				{
					result = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return result;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstVariantPart(RequiredType, RequiredLaterality, Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.VariantType == RequiredType && dismemberedPart.Part.Position < num && Laterality.Match(dismemberedPart.Part, RequiredLaterality) && Filter(dismemberedPart.Part))
				{
					result = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return result;
	}

	public bool HasPart(string RequiredType)
	{
		return GetFirstPart(RequiredType) != null;
	}

	public bool HasPart(string RequiredType, int RequiredLaterality)
	{
		return GetFirstPart(RequiredType, RequiredLaterality) != null;
	}

	public bool HasPart(Predicate<BodyPart> Filter)
	{
		return GetFirstPart(Filter) != null;
	}

	public bool HasPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		return GetFirstPart(RequiredType, Filter) != null;
	}

	public bool HasPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		return GetFirstPart(RequiredType, RequiredLaterality, Filter) != null;
	}

	public bool HasPart(string RequiredType, bool EvenIfDismembered)
	{
		return GetFirstPart(RequiredType, EvenIfDismembered) != null;
	}

	public bool HasPart(string RequiredType, int RequiredLaterality, bool EvenIfDismembered)
	{
		return GetFirstPart(RequiredType, RequiredLaterality, EvenIfDismembered) != null;
	}

	public bool HasPart(Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstPart(Filter, EvenIfDismembered) != null;
	}

	public bool HasPart(string RequiredType, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstPart(RequiredType, Filter, EvenIfDismembered) != null;
	}

	public bool HasPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstPart(RequiredType, RequiredLaterality, Filter, EvenIfDismembered) != null;
	}

	public bool HasVariantPart(string RequiredType)
	{
		return GetFirstVariantPart(RequiredType) != null;
	}

	public bool HasVariantPart(string RequiredType, int RequiredLaterality)
	{
		return GetFirstVariantPart(RequiredType, RequiredLaterality) != null;
	}

	public bool HasVariantPart(Predicate<BodyPart> Filter)
	{
		return GetFirstVariantPart(Filter) != null;
	}

	public bool HasVariantPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		return GetFirstVariantPart(RequiredType, Filter) != null;
	}

	public bool HasVariantPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		return GetFirstVariantPart(RequiredType, RequiredLaterality, Filter) != null;
	}

	public bool HasVariantPart(string RequiredType, bool EvenIfDismembered)
	{
		return GetFirstVariantPart(RequiredType, EvenIfDismembered) != null;
	}

	public bool HasVariantPart(string RequiredType, int RequiredLaterality, bool EvenIfDismembered)
	{
		return GetFirstVariantPart(RequiredType, RequiredLaterality, EvenIfDismembered) != null;
	}

	public bool HasVariantPart(Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstVariantPart(Filter, EvenIfDismembered) != null;
	}

	public bool HasVariantPart(string RequiredType, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstVariantPart(RequiredType, Filter, EvenIfDismembered) != null;
	}

	public bool HasVariantPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstVariantPart(RequiredType, RequiredLaterality, Filter, EvenIfDismembered) != null;
	}

	public BodyPart GetPartByName(string RequiredPart)
	{
		return _Body.GetPartByName(RequiredPart);
	}

	public BodyPart GetPartByName(string RequiredPart, bool EvenIfDismembered)
	{
		BodyPart bodyPart = _Body.GetPartByName(RequiredPart);
		if (((bodyPart == null) & EvenIfDismembered) && DismemberedParts != null)
		{
			int num = int.MaxValue;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Type == RequiredPart && dismemberedPart.Part.Position < num)
				{
					bodyPart = dismemberedPart.Part;
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return bodyPart;
	}

	public BodyPart GetPartByNameStartsWith(string RequiredPart)
	{
		return _Body.GetPartByNameStartsWith(RequiredPart);
	}

	public BodyPart GetPartByNameWithoutCybernetics(string RequiredPart)
	{
		return _Body.GetPartByNameWithoutCybernetics(RequiredPart);
	}

	public BodyPart GetPartByDescription(string RequiredPart)
	{
		return _Body.GetPartByDescription(RequiredPart);
	}

	public BodyPart GetPartByDescriptionStartsWith(string RequiredPart)
	{
		return _Body.GetPartByDescriptionStartsWith(RequiredPart);
	}

	public bool RemoveUnmanagedPartsByVariantPrefix(string Prefix)
	{
		if (_Body.RemoveUnmanagedPartsByVariantPrefix(Prefix))
		{
			UpdateBodyParts();
			RecalculateArmor();
			return true;
		}
		return false;
	}

	public bool RemovePart(BodyPart removePart, bool DoUpdate = true)
	{
		bool result = false;
		if (_Body.RemovePart(removePart, DoUpdate))
		{
			result = true;
		}
		if (DismemberedParts != null)
		{
			bool flag = false;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part == removePart)
				{
					flag = true;
					result = true;
					break;
				}
			}
			if (flag)
			{
				foreach (DismemberedPart item in new List<DismemberedPart>(DismemberedParts))
				{
					if (item.Part == removePart)
					{
						DismemberedParts.Remove(item);
					}
					else if (item.ParentID == removePart.ID)
					{
						RemovePart(item.Part, DoUpdate: false);
					}
				}
			}
		}
		return result;
	}

	public bool RemovePartByID(int removeID)
	{
		bool result = false;
		if (_Body.RemovePartByID(removeID))
		{
			return true;
		}
		if (DismemberedParts != null)
		{
			bool flag = false;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.IDMatch(removeID))
				{
					flag = true;
					result = true;
					break;
				}
			}
			if (flag)
			{
				foreach (DismemberedPart item in new List<DismemberedPart>(DismemberedParts))
				{
					if (item.Part.IDMatch(removeID))
					{
						DismemberedParts.Remove(item);
					}
				}
			}
		}
		return result;
	}

	public BodyPart GetPartByID(int findID, bool EvenIfDismembered = false)
	{
		BodyPart partByID = _Body.GetPartByID(findID);
		if (partByID != null)
		{
			return partByID;
		}
		if (EvenIfDismembered && DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.IDMatch(findID))
				{
					return dismemberedPart.Part;
				}
			}
		}
		return null;
	}

	public List<BodyPart> GetPartsBySupportsDependent(string findSupportsDependent)
	{
		_Body.GetPartBySupportsDependent(findSupportsDependent, parts2);
		return parts2;
	}

	public BodyPart GetPartBySupportsDependent(string findSupportsDependent)
	{
		return _Body.GetPartBySupportsDependent(findSupportsDependent);
	}

	public bool CheckSlotEquippedMatch(GameObject obj, string SlotSpec)
	{
		if (SlotSpec.IndexOf(',') != -1)
		{
			return _Body.CheckSlotEquippedMatch(obj, SlotSpec.CachedCommaExpansion());
		}
		return _Body.CheckSlotEquippedMatch(obj, SlotSpec);
	}

	public bool CheckSlotCyberneticsMatch(GameObject obj, string SlotSpec)
	{
		if (SlotSpec.IndexOf(',') != -1)
		{
			return _Body.CheckSlotCyberneticsMatch(obj, SlotSpec.CachedCommaExpansion());
		}
		return _Body.CheckSlotCyberneticsMatch(obj, SlotSpec);
	}

	public bool CheckSlotDefaultBehaviorMatch(GameObject obj, string SlotSpec)
	{
		if (SlotSpec.IndexOf(',') != -1)
		{
			return _Body.CheckSlotDefaultBehaviorMatch(obj, SlotSpec.CachedCommaExpansion());
		}
		return _Body.CheckSlotDefaultBehaviorMatch(obj, SlotSpec);
	}

	public bool HasReadyMissileWeapon(Predicate<GameObject> Filter = null, Predicate<MissileWeapon> PartFilter = null)
	{
		return _Body.HasReadyMissileWeapon(Filter, PartFilter);
	}

	public bool HasMissileWeapon(Predicate<GameObject> Filter = null, Predicate<MissileWeapon> PartFilter = null)
	{
		return _Body.HasMissileWeapon(Filter, PartFilter);
	}

	public bool HasHeavyWeaponEquipped()
	{
		return _Body.HasHeavyWeaponEquipped();
	}

	public void GetMissileWeapons(List<GameObject> List, Predicate<GameObject> Filter = null, Predicate<MissileWeapon> PartFilter = null)
	{
		_Body.GetMissileWeapons(ref List, Filter, PartFilter);
	}

	public List<GameObject> GetMissileWeapons(Predicate<GameObject> Filter = null, Predicate<MissileWeapon> PartFilter = null)
	{
		List<GameObject> List = null;
		_Body.GetMissileWeapons(ref List, Filter, PartFilter);
		return List;
	}

	public GameObject GetFirstThrownWeapon(Predicate<GameObject> Filter = null, Predicate<ThrownWeapon> PartFilter = null)
	{
		return _Body.GetFirstThrownWeapon(Filter, PartFilter);
	}

	public void GetThrownWeapons(IList<GameObject> List, Predicate<GameObject> Filter = null, Predicate<ThrownWeapon> PartFilter = null)
	{
		_Body.GetThrownWeapons(ref List, Filter, PartFilter);
	}

	public IList<GameObject> GetThrownWeapons(Predicate<GameObject> Filter = null, Predicate<ThrownWeapon> PartFilter = null)
	{
		IList<GameObject> List = null;
		_Body.GetThrownWeapons(ref List, Filter, PartFilter);
		return List;
	}

	public int GetDismemberedPartCount()
	{
		if (DismemberedParts == null)
		{
			return 0;
		}
		return DismemberedParts.Count;
	}

	public BodyPart GetDismemberedPartByID(int ID)
	{
		if (DismemberedParts == null)
		{
			return null;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.IDMatch(ID))
			{
				return dismemberedPart.Part;
			}
		}
		return null;
	}

	public BodyPart GetDismemberedPartByType(string RequiredType)
	{
		if (DismemberedParts == null)
		{
			return null;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.Type == RequiredType || dismemberedPart.Part.VariantType == RequiredType)
			{
				return dismemberedPart.Part;
			}
		}
		return null;
	}

	public BodyPart GetDismemberedPartBySupportsDependent(string RequiredSupportsDependent)
	{
		if (DismemberedParts == null)
		{
			return null;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.SupportsDependent == RequiredSupportsDependent)
			{
				return dismemberedPart.Part;
			}
		}
		return null;
	}

	public BodyPart GetDismemberedPartByDependsOn(string RequiredDependsOn)
	{
		if (DismemberedParts == null)
		{
			return null;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.DependsOn == RequiredDependsOn)
			{
				return dismemberedPart.Part;
			}
		}
		return null;
	}

	public bool ValidateDismemberedParentPart(BodyPart Parent, BodyPart Child)
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part == Child && Parent.IDMatch(dismemberedPart.ParentID))
				{
					return true;
				}
			}
		}
		return false;
	}

	public BodyPart GetFirstDismemberedPartByType(BodyPart Parent, string RequiredType)
	{
		BodyPart bodyPart = null;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (Parent.IDMatch(dismemberedPart.ParentID) && dismemberedPart.Part.Type == RequiredType && (bodyPart == null || dismemberedPart.Part.Position < bodyPart.Position))
				{
					bodyPart = dismemberedPart.Part;
				}
			}
		}
		return bodyPart;
	}

	public BodyPart GetFirstDismemberedPartByTypeAndLaterality(BodyPart Parent, string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstDismemberedPartByType(Parent, RequiredType);
		}
		BodyPart bodyPart = null;
		if (DismemberedParts != null)
		{
			if (RequiredLaterality == 0)
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (Parent.IDMatch(dismemberedPart.ParentID) && dismemberedPart.Part.Type == RequiredType && dismemberedPart.Part.Laterality == 0 && (bodyPart == null || dismemberedPart.Part.Position < bodyPart.Position))
					{
						bodyPart = dismemberedPart.Part;
					}
				}
			}
			else
			{
				foreach (DismemberedPart dismemberedPart2 in DismemberedParts)
				{
					if (Parent.IDMatch(dismemberedPart2.ParentID) && dismemberedPart2.Part.Type == RequiredType && (dismemberedPart2.Part.Laterality & RequiredLaterality) == RequiredLaterality && (bodyPart == null || dismemberedPart2.Part.Position < bodyPart.Position))
					{
						bodyPart = dismemberedPart2.Part;
					}
				}
			}
		}
		return bodyPart;
	}

	public bool IsDismembered(BodyPart checkPart)
	{
		if (DismemberedParts == null)
		{
			return false;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part == checkPart)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPartOrDismemberedPart(string partName)
	{
		if (GetPartByName(partName) != null)
		{
			return true;
		}
		if (HasDismemberedPartNamed(partName))
		{
			return true;
		}
		return false;
	}

	public bool HasDismemberedPartNamed(string RequiredPart)
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Name == RequiredPart || dismemberedPart.Part.Description == RequiredPart)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AnyDismemberedMortalParts()
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Mortal)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetFirstDismemberedPartPosition(BodyPart Parent, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (Parent.IDMatch(dismemberedPart.ParentID) && dismemberedPart.Part != ExceptPart && (num == -1 || dismemberedPart.Part.Position < num))
				{
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return num;
	}

	public int GetLastDismemberedPartPosition(BodyPart Parent, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (Parent.IDMatch(dismemberedPart.ParentID) && dismemberedPart.Part != ExceptPart && (num == -1 || dismemberedPart.Part.Position > num))
				{
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return num;
	}

	public int GetFirstDismemberedPartTypePosition(BodyPart Parent, string Type, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (Parent.IDMatch(dismemberedPart.ParentID) && dismemberedPart.Part.Type == Type && dismemberedPart.Part != ExceptPart && (num == -1 || dismemberedPart.Part.Position < num))
				{
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return num;
	}

	public int GetLastDismemberedPartTypePosition(BodyPart Parent, string Type, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (Parent.IDMatch(dismemberedPart.ParentID) && dismemberedPart.Part.Type == Type && dismemberedPart.Part != ExceptPart && dismemberedPart.Part.Position > num)
				{
					num = dismemberedPart.Part.Position;
				}
			}
		}
		return num;
	}

	public DismemberedPart FindRegenerablePart(int? ParentID = null, int? Category = null, int[] Categories = null, int? ExceptCategory = null, int[] ExceptCategories = null)
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.IsRegenerable() && (!ParentID.HasValue || dismemberedPart.ParentID == ParentID) && (!Category.HasValue || dismemberedPart.Part.Category == Category) && (Categories == null || Array.IndexOf(Categories, dismemberedPart.Part.Category) != -1) && (!ExceptCategory.HasValue || dismemberedPart.Part.Category != ExceptCategory) && (ExceptCategories == null || Array.IndexOf(ExceptCategories, dismemberedPart.Part.Category) == -1) && dismemberedPart.IsReattachable(this))
				{
					return dismemberedPart;
				}
			}
		}
		return null;
	}

	public DismemberedPart FindRegenerablePart(ILimbRegenerationEvent E)
	{
		return FindRegenerablePart(E.ParentID, E.Category, E.Categories, E.ExceptCategory, E.ExceptCategories);
	}

	public List<DismemberedPart> FindRecoverableParts()
	{
		if (DismemberedParts == null)
		{
			return null;
		}
		List<DismemberedPart> list = null;
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.IsRecoverable(this) && dismemberedPart.IsReattachable(this))
			{
				if (list == null)
				{
					list = new List<DismemberedPart>(1) { dismemberedPart };
				}
				else
				{
					list.Add(dismemberedPart);
				}
			}
		}
		return list;
	}

	public void CheckDismemberedPartRenumberingOnPositionAssignment(BodyPart Parent, int AssignedPosition, BodyPart ExceptPart = null)
	{
		if (DismemberedParts == null)
		{
			return;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			dismemberedPart.CheckRenumberingOnPositionAssignment(Parent, AssignedPosition, ExceptPart);
		}
	}

	public bool DismemberedPartHasPosition(BodyPart Parent, int Position, BodyPart ExceptPart = null)
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.HasPosition(Parent, Position, ExceptPart))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void GetTypeArmorInfo(string ForType, ref GameObject First, ref int Count, ref int AV, ref int DV)
	{
		_Body.GetTypeArmorInfo(ForType, ref First, ref Count, ref AV, ref DV);
	}

	public void RecalculateArmor()
	{
		if (built)
		{
			_Body.RecalculateArmor();
		}
	}

	public void RecalculateArmorExcept(GameObject obj)
	{
		if (built)
		{
			_Body.RecalculateArmorExcept(obj);
		}
	}

	public void RecalculateTypeArmor(string ForType)
	{
		if (built)
		{
			_Body.RecalculateTypeArmor(ForType);
		}
	}

	public void RecalculateTypeArmorExcept(string ForType, GameObject obj)
	{
		if (built)
		{
			_Body.RecalculateTypeArmorExcept(ForType, obj);
		}
	}

	public void RecalculateFirsts()
	{
		if (built)
		{
			bool PrimarySet = false;
			bool HasPreferredPrimary = false;
			BodyPart CurrentPrimaryPart = null;
			ForeachPart((BodyPart x) =>
			{
				x.Primary = false;
				x.DefaultPrimary = false;
				return true;
			});
			_Body.SetPrimaryScan(GetPrimaryLimbType(), ref PrimarySet, ref HasPreferredPrimary, ref CurrentPrimaryPart);
			if (CurrentPrimaryPart == null)
			{
				CurrentPrimaryPart = _Body;
				_Body.DefaultPrimary = true;
				_Body.Primary = true;
			}
			if (!HasPreferredPrimary && CurrentPrimaryPart != null)
			{
				CurrentPrimaryPart.PreferredPrimary = true;
			}
			ParentObject.FireEvent("PrimaryLimbRecalculated");
		}
	}

	public void RecalculateFirstEquipped(GameObject Object)
	{
		bool Found = false;
		_Body.RecalculateFirstEquipped(Object, ref Found);
	}

	public void RecalculateFirstCybernetics(GameObject Object)
	{
		bool Found = false;
		_Body.RecalculateFirstCybernetics(Object, ref Found);
	}

	public void RecalculateFirstDefaultBehavior(GameObject Object)
	{
		bool Found = false;
		_Body.RecalculateFirstDefaultBehavior(Object, ref Found);
	}

	public void CheckUnsupportedPartLoss()
	{
		List<BodyPart> list = _Body.FindUnsupportedParts();
		if (list == null)
		{
			return;
		}
		foreach (BodyPart item in list)
		{
			if (ParentObject.IsPlayer())
			{
				string ordinalName = item.GetOrdinalName();
				CutAndQueueForRegeneration(item);
				IComponent<GameObject>.AddPlayerMessage("You have lost the use of your " + ordinalName + ".", 'R');
			}
			else
			{
				CutAndQueueForRegeneration(item);
			}
		}
	}

	public void CheckPartRecovery()
	{
		List<DismemberedPart> list = FindRecoverableParts();
		if (list == null)
		{
			return;
		}
		foreach (DismemberedPart item in list)
		{
			item.Reattach(this);
			if (ParentObject.IsPlayer() && item.Part.IsAbstractlyDependent())
			{
				IComponent<GameObject>.AddPlayerMessage("You have recovered the use of your " + item.Part.GetOrdinalName() + ".", 'G');
			}
		}
	}

	public void RegenerateDefaultEquipment()
	{
		List<BodyPart> list;
		if (_bodyPartsInUse)
		{
			list = new List<BodyPart>(GetPartCount());
		}
		else
		{
			list = _bodyParts;
			_bodyPartsInUse = true;
		}
		GetParts(list);
		try
		{
			foreach (BodyPart item in list)
			{
				GameObject gameObject = item.DefaultBehavior;
				if (gameObject != null && ((item.DefaultBehaviorBlueprint != null && gameObject.Blueprint == item.DefaultBehaviorBlueprint) || gameObject.HasTagOrProperty("TemporaryDefaultBehavior")))
				{
					gameObject.Obliterate();
					gameObject = (item._DefaultBehavior = null);
				}
				if (gameObject == null && item.DefaultBehaviorBlueprint != null)
				{
					item.DefaultBehavior = GameObject.CreateUnmodified(item.DefaultBehaviorBlueprint);
					item.FirstSlotForDefaultBehavior = true;
				}
			}
			RegenerateDefaultEquipmentEvent.Send(ParentObject, this);
			DecorateDefaultEquipmentEvent.Send(ParentObject, this);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("RegenerateDefaultEquipment", x);
		}
		finally
		{
			if (list == _bodyParts)
			{
				_bodyParts.Clear();
				_bodyPartsInUse = false;
			}
		}
	}

	public void UpdateBodyParts(int Depth = 0)
	{
		if (built)
		{
			RegenerateDefaultEquipment();
			RecalculateFirsts();
			UpdateMobilitySpeedPenalty();
			CheckUnsupportedPartLoss();
			CheckPartRecovery();
			FireEventOnBodyparts(eBodypartsUpdated);
		}
	}

	public void CutAndQueueForRegeneration(BodyPart Part)
	{
		BodyPart parentPart = Part.GetParentPart();
		DismemberedPart dismemberedPart = (Part.Extrinsic ? null : new DismemberedPart(Part, parentPart));
		if (Part.Parts != null && Part.Parts.Count > 0)
		{
			if (Part.Parts.Count > 1)
			{
				foreach (BodyPart item in new List<BodyPart>(Part.Parts))
				{
					CutAndQueueForRegeneration(item);
				}
			}
			else
			{
				CutAndQueueForRegeneration(Part.Parts[0]);
			}
			Part.Parts = null;
		}
		parentPart?.RemovePart(Part, DoUpdate: false);
		Part.ParentBody?.RecalculateTypeArmor(Part.Type);
		if (dismemberedPart != null)
		{
			if (DismemberedParts == null)
			{
				DismemberedParts = new List<DismemberedPart>(1) { dismemberedPart };
			}
			else
			{
				DismemberedParts.Add(dismemberedPart);
			}
		}
		Part.ParentBody = null;
		Part.Primary = false;
		Part.PreferredPrimary = false;
		Part.DefaultPrimary = false;
	}

	/// Determines the speed penalty we should have based on our current
	/// intact and dismembered mobility-providing limbs.
	public int CalculateMobilitySpeedPenalty(out bool AnyDismembered)
	{
		AnyDismembered = false;
		if (ParentObject.IsFlying)
		{
			return 0;
		}
		int totalMobility = GetTotalMobility();
		if (totalMobility == 0)
		{
			if (DismemberedParts != null)
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.GetTotalMobility() > 0)
					{
						AnyDismembered = true;
						break;
					}
				}
			}
			return 60;
		}
		int num = 0;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart2 in DismemberedParts)
			{
				num += dismemberedPart2.Part.GetTotalMobility();
			}
		}
		if (num > 0)
		{
			AnyDismembered = true;
		}
		if (totalMobility >= 2)
		{
			if (num == 0)
			{
				return 0;
			}
			return 60 * num / (totalMobility + num + 2);
		}
		if (num == 0)
		{
			return 60 * totalMobility / 2;
		}
		return 60 * num / (totalMobility + num);
	}

	public int CalculateMobilitySpeedPenalty()
	{
		bool AnyDismembered;
		return CalculateMobilitySpeedPenalty(out AnyDismembered);
	}

	public void UpdateMobilitySpeedPenalty()
	{
		int num = CalculateMobilitySpeedPenalty(out var AnyDismembered);
		if (num == MobilitySpeedPenaltyApplied || !ParentObject.Statistics.ContainsKey("MoveSpeed"))
		{
			return;
		}
		ParentObject.Statistics["MoveSpeed"].Bonus -= MobilitySpeedPenaltyApplied;
		ParentObject.Statistics["MoveSpeed"].Bonus += num;
		MobilitySpeedPenaltyApplied = num;
		if (MobilitySpeedPenaltyApplied == 0)
		{
			ParentObject.RemoveEffect<MobilityImpaired>();
		}
		else if (AnyDismembered)
		{
			if (ParentObject.HasEffect<MobilityImpaired>())
			{
				ParentObject.GetEffect<MobilityImpaired>().Amount = MobilitySpeedPenaltyApplied;
			}
			else
			{
				ParentObject.ApplyEffect(new MobilityImpaired(MobilitySpeedPenaltyApplied));
			}
		}
	}

	public GameObject Dismember(BodyPart Part, GameObject Actor = null, IInventory Where = null, bool Obliterate = false, bool Silent = false, IEvent ParentEvent = null)
	{
		if (Part == null)
		{
			return null;
		}
		GameObject parentObject = ParentObject;
		bool obliterate = Obliterate;
		if (!BeforeDismemberEvent.Check(Actor, parentObject, Part, Where, Silent, obliterate) && !Obliterate)
		{
			return null;
		}
		ParentObject.StopMoving();
		GameObject gameObject = null;
		GameObject gameObject2 = Part.Unimplant();
		Part.UnequipPartAndChildren(Silent: false, Where);
		IInventory inventory = Where ?? ParentObject.GetDropInventory();
		if (!Part.Extrinsic && !Obliterate)
		{
			BodyPartType bodyPartType = Part.VariantTypeModel();
			gameObject = GameObject.Create(ParentObject.GetPropertyOrTag(bodyPartType.LimbBlueprintProperty, bodyPartType.LimbBlueprintDefault), 0, 0, null, null, null, "Dismember");
			string displayName = ParentObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: true, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: false, BaseOnly: true);
			ParentObject.An(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: true, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true, IndicateHidden: false, SecondPerson: false);
			if (!gameObject.HasPropertyOrTag("SeveredLimbKeepAppearance"))
			{
				Render render = gameObject.Render;
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append(Grammar.MakePossessive(displayName)).Append(' ');
				string color = BodyPartCategory.GetColor(Part.Category);
				if (color != null)
				{
					stringBuilder.Append("{{").Append(color).Append('|');
				}
				stringBuilder.Append(Part.Name);
				if (color != null)
				{
					stringBuilder.Append("}}");
				}
				render.DisplayName = stringBuilder.ToString();
				if (ParentObject.HasProperName)
				{
					gameObject.HasProperName = true;
				}
				else
				{
					string text = ParentObject.GetxTag("Grammar", "iArticle", gameObject.GetStringProperty("OverrideIArticle"));
					text = ParentObject.GetPropertyOrTag("OverrideIArticle", text);
					if (!text.IsNullOrEmpty())
					{
						gameObject.SetStringProperty("OverrideIArticle", text);
					}
				}
				string primaryLiquidNameForLiquidSpecification = Bleeding.GetPrimaryLiquidNameForLiquidSpecification(ParentObject.GetBleedLiquid());
				gameObject.GetPart<Description>().Short = "Dried " + primaryLiquidNameForLiquidSpecification + " crusts on the severed " + Part.Name + " of " + ParentObject.a + ParentObject.ShortDisplayNameSingleStripped + ".";
				if (ParentObject.HasPropertyOrTag("SeveredLimbColorString"))
				{
					gameObject.Render.ColorString = ParentObject.GetPropertyOrTag("SeveredLimbColorString");
				}
				if (ParentObject.HasPropertyOrTag("SeveredLimbTileColor"))
				{
					gameObject.Render.TileColor = ParentObject.GetPropertyOrTag("SeveredLimbTileColor");
				}
				if (ParentObject.HasPropertyOrTag("SeveredLimbDetailColor"))
				{
					gameObject.Render.DetailColor = ParentObject.GetPropertyOrTag("SeveredLimbDetailColor");
				}
			}
			gameObject.RequirePart<DismemberedProperties>().SetFrom(Part);
			if (ParentObject.IsGiganticCreature)
			{
				gameObject.IsGiganticEquipment = true;
				gameObject.ModIntProperty("ModGiganticNoDisplayName", 1, RemoveIfZero: true);
			}
			if (ParentObject.IsPlayer())
			{
				gameObject.AddPart(new EatenAchievement(Achievement.EAT_OWN_LIMB));
			}
			if (ParentObject.HasPart<Extradimensional>())
			{
				gameObject.AddPart(new CookedAchievement(Achievement.COOKED_EXTRADIMENSIONAL));
			}
			if (gameObject2 != null)
			{
				gameObject2.RemoveFromContext();
				gameObject.AddPart(new CyberneticsButcherableCybernetic(gameObject2));
				gameObject.RemovePart<Food>();
			}
			if (Part.Type == "Face")
			{
				Armor armor = gameObject.RequirePart<Armor>();
				armor.WornOn = "Face";
				if (ParentObject.IsPlayer() || ParentObject.HasProperty("PlayerCopy"))
				{
					gameObject.AddPart(new EquippedAchievement(Achievement.WEAR_OWN_FACE));
				}
				int value = ParentObject.Statistics["Level"]._Value;
				int ego = ((value < 20) ? 1 : ((value >= 35) ? 3 : 2));
				armor.Ego = ego;
				foreach (KeyValuePair<string, int> item in ParentObject.Brain.GetBaseAllegiance())
				{
					if (Factions.Get(item.Key).Visible)
					{
						AddsRep.AddModifier(gameObject, item.Key, -500);
					}
				}
			}
			Temporary.CarryOver(ParentObject, gameObject);
			Phase.carryOver(ParentObject, gameObject, 50);
			inventory?.AddObjectToInventory(gameObject, Actor, Silent: false, NoStack: false, FlushTransient: true, null, ParentEvent);
			DroppedEvent.Send(ParentObject, gameObject);
		}
		AfterDismemberEvent.Send(Actor, ParentObject, gameObject, Part, inventory, Silent, Obliterate);
		if (ParentObject.IsPlayer() && Part.Name != null && !Obliterate)
		{
			string ordinalName = Part.GetOrdinalName();
			bool plural = Part.Plural;
			if (!Silent)
			{
				Popup.Show("Your " + ordinalName + " " + (plural ? "are" : "is") + " dismembered!");
			}
			JournalAPI.AddAccomplishment("Your " + ordinalName + " " + (plural ? "were" : "was") + " dismembered.", "While fighting a battle to protect the practice of " + HistoricStringExpander.ExpandString("<spice.elements." + The.Player.GetMythicDomain() + ".practices.!random>") + ", =name= valorously had " + The.Player.GetPronounProvider().PossessiveAdjective + " " + ordinalName + " dismembered.", "While fighting a battle to protect the practice of " + HistoricStringExpander.ExpandString("<spice.elements." + The.Player.GetMythicDomain() + ".practices.!random>") + ", =name= valorously had " + The.Player.GetPronounProvider().PossessiveAdjective + " " + ordinalName + " dismembered.", null, "general", MuralCategory.BodyExperienceBad, MuralWeight.Medium, null, -1L);
		}
		if (!Obliterate)
		{
			CutAndQueueForRegeneration(Part);
		}
		UpdateBodyParts();
		RecalculateArmor();
		return gameObject;
	}

	public bool RegenerateLimb(bool WholeLimb = false, DismemberedPart Part = null, int? ParentID = null, int? Category = null, int[] Categories = null, int? ExceptCategory = null, int[] ExceptCategories = null, bool DoUpdate = true)
	{
		if (Part == null)
		{
			Part = FindRegenerablePart(ParentID, Category, Categories, ExceptCategory, ExceptCategories);
			if (Part == null)
			{
				return false;
			}
		}
		else
		{
			if (ParentID.HasValue && Part.ParentID != ParentID)
			{
				return false;
			}
			if (Category.HasValue && Part.Part.Category != Category)
			{
				return false;
			}
			if (Categories != null && Array.IndexOf(Categories, Part.Part.Category) == -1)
			{
				return false;
			}
			if (ExceptCategory.HasValue && Part.Part.Category == ExceptCategory)
			{
				return false;
			}
			if (ExceptCategories != null && Array.IndexOf(ExceptCategories, Part.Part.Category) != -1)
			{
				return false;
			}
		}
		ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_regeneration_limbRegrowth");
		Part.Reattach(this);
		if (ParentObject.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You regenerate your " + Part.Part.GetOrdinalName() + "!", 'G');
			Achievement.REGENERATE_LIMB.Unlock();
		}
		else if (Visible())
		{
			DidX("regenerate", ParentObject.its + " " + Part.Part.GetOrdinalName(), "!", null, null, ParentObject);
		}
		if (WholeLimb && Part.Part.HasID())
		{
			DismemberedPart dismemberedPart = null;
			while ((dismemberedPart = FindRegenerablePart(Part.Part.ID, Category, Categories, ExceptCategory, ExceptCategories)) != null)
			{
				RegenerateLimb(WholeLimb, dismemberedPart, Part.Part.ID, Category, Categories, ExceptCategory, ExceptCategories, DoUpdate: false);
			}
		}
		if (DoUpdate)
		{
			UpdateBodyParts();
		}
		return true;
	}

	public bool RegenerateLimb(ILimbRegenerationEvent E)
	{
		return RegenerateLimb(E.Whole, null, E.ParentID, E.Category, E.Categories, E.ExceptCategory, E.ExceptCategories);
	}

	private List<string> SummarizeMissingBodyParts(List<BodyPart> Parts)
	{
		Dictionary<string, int> Counts = new Dictionary<string, int>(Parts.Count);
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>(Parts.Count);
		foreach (BodyPart Part in Parts)
		{
			string name = Part.Name;
			if (Counts.ContainsKey(name))
			{
				Counts[name]++;
				continue;
			}
			Counts.Add(name, 1);
			dictionary[name] = Part.Plural;
		}
		List<string> list = new List<string>(Counts.Keys);
		list.Sort((string n1, string n2) =>
		{
			int num2 = Counts[n1].CompareTo(Counts[n2]);
			return (num2 != 0) ? (-num2) : n1.CompareTo(n2);
		});
		List<string> list2 = new List<string>(list.Count);
		foreach (string item in list)
		{
			int num = Counts[item];
			if (num == 1)
			{
				if (GetPartByName(item) == null)
				{
					list2.Add(ParentObject.its + " " + item);
				}
				else if (dictionary[item])
				{
					list2.Add("a set of " + item);
				}
				else
				{
					list2.Add(Grammar.A(item));
				}
			}
			else if (dictionary[item])
			{
				list2.Add(Grammar.Cardinal(num) + " sets of " + item);
			}
			else
			{
				list2.Add(Grammar.Cardinal(num) + " " + Grammar.Pluralize(item));
			}
		}
		return list2;
	}

	public bool GetMissingLimbsDescription(StringBuilder SB, bool PrependNewlineIfContent = false)
	{
		if (DismemberedParts == null)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.Abstract)
			{
				num2++;
			}
			else if (dismemberedPart.Part.SupportsDependent == null || GetDismemberedPartByDependsOn(dismemberedPart.Part.SupportsDependent) == null)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return false;
		}
		if (PrependNewlineIfContent)
		{
			SB.Append('\n');
		}
		if (num2 > 0)
		{
			List<BodyPart> list = new List<BodyPart>(num);
			List<BodyPart> list2 = new List<BodyPart>(num2);
			foreach (DismemberedPart dismemberedPart2 in DismemberedParts)
			{
				if (dismemberedPart2.Part.Abstract)
				{
					list2.Add(dismemberedPart2.Part);
				}
				else if (dismemberedPart2.Part.SupportsDependent == null || GetDismemberedPartByDependsOn(dismemberedPart2.Part.SupportsDependent) == null)
				{
					list.Add(dismemberedPart2.Part);
				}
			}
			List<string> words = SummarizeMissingBodyParts(list);
			List<string> words2 = SummarizeMissingBodyParts(list2);
			SB.Append(ParentObject.Itis).Append(" missing ").Append(Grammar.MakeAndList(words))
				.Append(", and so ")
				.Append(ParentObject.it)
				.Append(ParentObject.GetVerb("do", PrependSpace: true, PronounAntecedent: true))
				.Append(" not have the use of ")
				.Append(Grammar.MakeOrList(words2))
				.Append('.');
		}
		else
		{
			List<BodyPart> list3 = new List<BodyPart>(num);
			foreach (DismemberedPart dismemberedPart3 in DismemberedParts)
			{
				if (!dismemberedPart3.Part.Abstract && (dismemberedPart3.Part.SupportsDependent == null || GetDismemberedPartByDependsOn(dismemberedPart3.Part.SupportsDependent) == null))
				{
					list3.Add(dismemberedPart3.Part);
				}
			}
			List<string> words3 = SummarizeMissingBodyParts(list3);
			SB.Append(ParentObject.Itis).Append(" missing ").Append(Grammar.MakeAndList(words3))
				.Append('.');
		}
		return true;
	}

	public string GetMissingLimbsDescription(bool PrependNewlineIfContent = false)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		GetMissingLimbsDescription(stringBuilder, PrependNewlineIfContent);
		if (stringBuilder.Length != 0)
		{
			return stringBuilder.ToString();
		}
		return null;
	}

	public bool Rebuild(string AsAnatomy)
	{
		List<BodyPart> topLevelDynamicParts = GetTopLevelDynamicParts();
		List<BodyPartPositionHint> list = null;
		List<BodyPart> partsSkippingDynamicTrees = GetPartsSkippingDynamicTrees();
		List<GameObject> list2 = Event.NewGameObjectList();
		Dictionary<GameObject, string> dictionary = new Dictionary<GameObject, string>(partsSkippingDynamicTrees.Count);
		List<GameObject> list3 = Event.NewGameObjectList();
		Dictionary<GameObject, bool> dictionary2 = new Dictionary<GameObject, bool>(partsSkippingDynamicTrees.Count);
		List<GameObject> list4 = Event.NewGameObjectList();
		List<GameObject> list5 = null;
		bool flag = false;
		if (topLevelDynamicParts.Count > 0)
		{
			list = new List<BodyPartPositionHint>();
			foreach (BodyPart item in topLevelDynamicParts)
			{
				list.Add(new BodyPartPositionHint(item));
			}
		}
		foreach (BodyPart item2 in partsSkippingDynamicTrees)
		{
			GameObject cybernetics = item2.Cybernetics;
			if (cybernetics != null && !list2.Contains(cybernetics))
			{
				if (cybernetics.IsStackable())
				{
					cybernetics.SetIntProperty("NeverStack", 1);
				}
				list2.Add(cybernetics);
				dictionary.Add(cybernetics, item2.Type);
				item2.Unimplant();
			}
		}
		foreach (BodyPart item3 in partsSkippingDynamicTrees)
		{
			GameObject equipped = item3.Equipped;
			if (!GameObject.Validate(equipped))
			{
				continue;
			}
			if (equipped.IsStackable())
			{
				if (item3.TryUnequip(Silent: true, SemiForced: true, NoStack: true))
				{
					list3.Add(equipped);
					dictionary2.Add(equipped, item3.Type == "Hand");
					if (!list4.Contains(equipped))
					{
						list4.Add(equipped);
					}
				}
			}
			else if (item3.TryUnequip(Silent: true, SemiForced: true))
			{
				list3.Add(equipped);
				dictionary2.Add(equipped, item3.Type == "Hand");
			}
		}
		List<BaseMutation> list6 = ParentObject.GetPart<Mutations>()?.ActiveMutationList;
		List<BaseMutation> list7 = null;
		if (list6 != null)
		{
			foreach (BaseMutation item4 in list6)
			{
				if (item4.AffectsBodyParts() || item4.GeneratesEquipment())
				{
					item4.Unmutate(item4.ParentObject);
					if (list7 == null)
					{
						list7 = new List<BaseMutation>(3) { item4 };
					}
					else
					{
						list7.Add(item4);
					}
				}
			}
		}
		foreach (BodyPart part in GetParts())
		{
			GameObject equipped2 = part.Equipped;
			if (!GameObject.Validate(equipped2) || list3.Contains(equipped2) || !part.ForceUnequip(Silent: true, NoStack: true))
			{
				continue;
			}
			list3.Add(equipped2);
			dictionary2.Add(equipped2, part.Type == "Hand");
			if (!list4.Contains(equipped2) && equipped2.IsStackable())
			{
				list4.Add(equipped2);
			}
			if (!equipped2.HasPropertyOrTag("CursedBodyRebuildAllowUnequip"))
			{
				if (list5 == null)
				{
					list5 = new List<GameObject>();
				}
				list5.Add(equipped2);
			}
		}
		Anatomy = AsAnatomy;
		if (list != null)
		{
			foreach (BodyPartPositionHint item5 in list)
			{
				item5.GetBestParentAndPosition(this, out var Parent, out var Position);
				if (Parent == null)
				{
					Parent = _Body;
					Position = -1;
					if (_Body.Parts != null)
					{
						foreach (BodyPart part2 in _Body.Parts)
						{
							if (part2.Type == "Thrown Weapon" || part2.Type == "Floating Nearby")
							{
								Position = part2.Position - 1;
								break;
							}
						}
					}
				}
				Parent.AddPart(item5.Self, Position, DoUpdate: false);
				flag = true;
			}
		}
		if (list7 != null)
		{
			ActivatedAbilities activatedAbilities = ParentObject.ActivatedAbilities;
			bool flag2 = false;
			if (activatedAbilities != null)
			{
				flag2 = activatedAbilities.Silent;
				if (!flag2)
				{
					activatedAbilities.Silent = true;
				}
			}
			try
			{
				foreach (BaseMutation item6 in list7)
				{
					item6.Mutate(item6.ParentObject, item6.BaseLevel);
				}
			}
			finally
			{
				if (activatedAbilities != null && !flag2)
				{
					activatedAbilities.Silent = false;
				}
			}
		}
		if (list2.Count > 0)
		{
			List<GameObject> list8 = null;
			foreach (GameObject item7 in list2)
			{
				if (!item7.IsValid())
				{
					continue;
				}
				bool flag3 = false;
				foreach (BodyPart item8 in GetPart(dictionary[item7]))
				{
					if (item8.Cybernetics == null)
					{
						item7.RemoveFromContext();
						item8.Implant(item7);
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					if (list8 == null)
					{
						list8 = new List<GameObject>(list2.Count) { item7 };
					}
					else
					{
						list8.Add(item7);
					}
				}
			}
			if (list8 != null)
			{
				foreach (GameObject item9 in list8)
				{
					string[] array = item9.GetPart<CyberneticsBaseItem>().Slots.Split(',');
					int num = 0;
					while (true)
					{
						if (num < array.Length)
						{
							string requiredType = array[num];
							foreach (BodyPart item10 in GetPart(requiredType))
							{
								if (item10.Cybernetics == null)
								{
									item9.RemoveFromContext();
									item10.Implant(item9);
									goto end_IL_05ee;
								}
							}
							num++;
							continue;
						}
						if (item9.HasTagOrProperty("CyberneticsNoRemove") || item9.HasTagOrProperty("CyberneticsDestroyOnRemoval"))
						{
							item9.Destroy();
						}
						break;
						continue;
						end_IL_05ee:
						break;
					}
				}
			}
		}
		if (list3.Count > 0)
		{
			foreach (GameObject item11 in list3)
			{
				ParentObject.AutoEquip(item11, Forced: true, dictionary2[item11], Silent: true);
			}
		}
		foreach (GameObject item12 in list4)
		{
			if (GameObject.Validate(item12))
			{
				item12.CheckStack();
			}
		}
		if (list5 != null)
		{
			foreach (GameObject item13 in list5)
			{
				if (GameObject.Validate(item13) && item13.Physics?.Equipped == null)
				{
					item13.Obliterate();
				}
			}
		}
		foreach (BodyPart item14 in partsSkippingDynamicTrees)
		{
			item14._Equipped = null;
			item14._Cybernetics = null;
			item14.DefaultBehavior = null;
		}
		if (flag)
		{
			UpdateBodyParts();
		}
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public bool FireEventOnBodyparts(Event E)
	{
		return _Body.FireEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeathRemoval" && !FireEventOnBodyparts(E))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		_Body.TurnTick(TimeTick, Amount);
	}

	public void TypeDump(StringBuilder SB)
	{
		_Body.TypeDump(SB);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (base.WantEvent(ID, cascade) || ID == AnyRegenerableLimbsEvent.ID || ID == BeforeDeathRemovalEvent.ID || ID == EffectAppliedEvent.ID || ID == EffectRemovedEvent.ID || ID == SingletonEvent<EndTurnEvent>.ID || ID == SingletonEvent<FlushWeightCacheEvent>.ID || ID == PooledEvent<GenericQueryEvent>.ID || ID == GetCarriedWeightEvent.ID || ID == PooledEvent<GetContentsEvent>.ID || ID == PooledEvent<GetElectricalConductivityEvent>.ID || ID == GetExtrinsicValueEvent.ID || ID == GetExtrinsicWeightEvent.ID || ID == GetShortDescriptionEvent.ID || ID == PooledEvent<HasBlueprintEvent>.ID || ID == PooledEvent<QuerySlotListEvent>.ID || ID == RegenerateLimbEvent.ID || ID == PooledEvent<StripContentsEvent>.ID)
		{
			return true;
		}
		if (MinEvent.CascadeTo(cascade, 1) && _Body.WantEvent(ID, cascade))
		{
			return true;
		}
		return false;
	}

	public override bool HandleEvent(HasBlueprintEvent E)
	{
		if (_Body.HasBlueprint(E.Blueprint))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		UpdateMobilitySpeedPenalty();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		UpdateMobilitySpeedPenalty();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		_Body.HandleEvent(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (!base.HandleEvent(E))
		{
			return false;
		}
		if (E.CascadeTo(1) && !_Body.HandleEvent(E))
		{
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (!ParentObject.HasTag("NoDropOnDeath") && !ParentObject.IsTemporary && _Body != null)
		{
			_Body.UnequipPartAndChildren(Silent: true, null, ForDeath: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 2)
		{
			int maximumEffectiveElectricalConductivityOfMobilityBodyParts = GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts(out var CriticalObject);
			int meanElectricalConductivity = GetMeanElectricalConductivity();
			int amount;
			if (maximumEffectiveElectricalConductivityOfMobilityBodyParts * 3 < meanElectricalConductivity)
			{
				amount = Math.Max(maximumEffectiveElectricalConductivityOfMobilityBodyParts * 3 * meanElectricalConductivity / 100, maximumEffectiveElectricalConductivityOfMobilityBodyParts);
				if (CriticalObject != null && E.ReductionObject == null)
				{
					E.ReductionObject = CriticalObject;
				}
			}
			else
			{
				amount = meanElectricalConductivity;
			}
			E.MinValue(amount);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		if (!_Body.ProcessExtrinsicValue(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		if (!_Body.ProcessExtrinsicWeight(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCarriedWeightEvent E)
	{
		E.Weight += GetWeight();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QuerySlotListEvent E)
	{
		if (!_Body.ProcessQuerySlotList(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (DismemberedParts != null && DismemberedParts.Count > 0)
		{
			GetMissingLimbsDescription(E.Postfix, PrependNewlineIfContent: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StripContentsEvent E)
	{
		List<GameObject> list = Event.NewGameObjectList();
		GetEquippedObjects(list);
		List<GameObject> list2 = Event.NewGameObjectList();
		GetInstalledCybernetics(list2);
		foreach (GameObject item in list)
		{
			if ((!E.KeepNatural || !item.IsNatural()) && (item.Physics == null || item.Physics.IsReal) && !list2.Contains(item))
			{
				list2.Add(item);
			}
		}
		foreach (GameObject item2 in list2)
		{
			item2.Obliterate(null, E.Silent);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContentsEvent E)
	{
		_Body.GetContents(E.Objects);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RegenerateLimbEvent E)
	{
		if (E.Object == ParentObject)
		{
			if (E.All)
			{
				int num = Math.Max(100, GetDismemberedPartCount() * 2);
				int num2 = 0;
				while (RegenerateLimb(E) && ++num2 < num)
				{
				}
			}
			else
			{
				RegenerateLimb(E);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AnyRegenerableLimbsEvent E)
	{
		if (FindRegenerablePart(E) != null)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(FlushWeightCacheEvent E)
	{
		FlushWeightCache();
		return base.HandleEvent(E);
	}

	public int GetPartDepth(BodyPart Part)
	{
		return _Body.GetPartDepth(Part, 0);
	}

	public void GetBodyPartsImplying(out List<BodyPart> Result, BodyPart Part, bool EvenIfDismembered = true)
	{
		Result = null;
		BodyPartType bodyPartType = Part.VariantTypeModel();
		if (bodyPartType.ImpliedBy.IsNullOrEmpty())
		{
			return;
		}
		int impliedPer = bodyPartType.GetImpliedPer();
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		SomeBodyParts.Clear();
		OtherBodyParts.Clear();
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart.VariantType == bodyPartType.ImpliedBy && bodyPart.Laterality == Part.Laterality)
			{
				SomeBodyParts.Add(bodyPart);
			}
			if (bodyPart.VariantType == bodyPartType.Type || bodyPart.VariantType == bodyPart.VariantTypeModel().ImpliedBy)
			{
				OtherBodyParts.Add(bodyPart);
			}
		}
		if (OtherBodyParts.Count == 1 && OtherBodyParts[0] == Part && SomeBodyParts.Count <= impliedPer)
		{
			Result = new List<BodyPart>(SomeBodyParts);
		}
		else
		{
			Result = new List<BodyPart>(impliedPer);
			int num = 0;
			int j = 0;
			for (int count2 = OtherBodyParts.Count; j < count2; j++)
			{
				if (OtherBodyParts[j] == Part)
				{
					for (int k = 0; k < impliedPer; k++)
					{
						if (num < SomeBodyParts.Count)
						{
							Result.Add(SomeBodyParts[num]);
						}
						num++;
					}
				}
				else
				{
					num += impliedPer;
				}
			}
		}
		if (EvenIfDismembered || Result == null || Result.Count <= 0 || DismemberedParts == null || DismemberedParts.Count <= 0)
		{
			return;
		}
		SomeBodyParts.Clear();
		SomeBodyParts.AddRange(Result);
		foreach (BodyPart someBodyPart in SomeBodyParts)
		{
			if (IsDismembered(someBodyPart))
			{
				Result.Remove(someBodyPart);
			}
		}
	}

	public List<BodyPart> GetBodyPartsImplying(BodyPart Part, bool EvenIfDismembered = true)
	{
		GetBodyPartsImplying(out var Result, Part, EvenIfDismembered);
		return Result;
	}

	public int GetBodyPartCountImplying(BodyPart Part)
	{
		if (Part.VariantTypeModel().ImpliedBy.IsNullOrEmpty())
		{
			return 0;
		}
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		return GetBodyPartCountImplyingInternal(Part);
	}

	private int GetBodyPartCountImplyingInternal(BodyPart Part, BodyPartType Type = null)
	{
		if (Type == null)
		{
			Type = Part.VariantTypeModel();
			if (Type.ImpliedBy.IsNullOrEmpty())
			{
				return 0;
			}
		}
		int impliedPer = Type.GetImpliedPer();
		SomeBodyParts.Clear();
		OtherBodyParts.Clear();
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart.VariantType == Type.ImpliedBy && bodyPart.Laterality == Part.Laterality)
			{
				SomeBodyParts.Add(bodyPart);
			}
			if (bodyPart.VariantType == Type.Type || bodyPart.VariantType == bodyPart.VariantTypeModel().ImpliedBy)
			{
				OtherBodyParts.Add(bodyPart);
			}
		}
		if (OtherBodyParts.Count == 1 && OtherBodyParts[0] == Part && SomeBodyParts.Count <= impliedPer)
		{
			return SomeBodyParts.Count;
		}
		int num = 0;
		int num2 = 0;
		int j = 0;
		for (int count2 = OtherBodyParts.Count; j < count2; j++)
		{
			if (OtherBodyParts[j] == Part)
			{
				for (int k = 0; k < impliedPer; k++)
				{
					if (num2 < SomeBodyParts.Count)
					{
						num++;
					}
					num2++;
				}
			}
			else
			{
				num2 += impliedPer;
			}
		}
		return num;
	}

	public bool AnyBodyPartsImplying(BodyPart Part)
	{
		if (Part.VariantTypeModel().ImpliedBy.IsNullOrEmpty())
		{
			return false;
		}
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		return AnyBodyPartsImplyingInternal(Part);
	}

	private bool AnyBodyPartsImplyingInternal(BodyPart Part, BodyPartType Type = null)
	{
		if (Type == null)
		{
			Type = Part.VariantTypeModel();
			if (Type.ImpliedBy.IsNullOrEmpty())
			{
				return false;
			}
		}
		int impliedPer = Type.GetImpliedPer();
		SomeBodyParts.Clear();
		OtherBodyParts.Clear();
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart.VariantType == Type.ImpliedBy && bodyPart.Laterality == Part.Laterality)
			{
				SomeBodyParts.Add(bodyPart);
			}
			if (bodyPart.VariantType == Type.Type || bodyPart.VariantType == bodyPart.VariantTypeModel().ImpliedBy)
			{
				OtherBodyParts.Add(bodyPart);
			}
		}
		if (OtherBodyParts.Count == 1 && OtherBodyParts[0] == Part && SomeBodyParts.Count <= impliedPer)
		{
			if (SomeBodyParts.Count > 0)
			{
				return true;
			}
		}
		else
		{
			int num = 0;
			int j = 0;
			for (int count2 = OtherBodyParts.Count; j < count2; j++)
			{
				if (OtherBodyParts[j] == Part)
				{
					for (int k = 0; k < impliedPer; k++)
					{
						if (num < SomeBodyParts.Count)
						{
							return true;
						}
						num++;
					}
				}
				else
				{
					num += impliedPer;
				}
			}
		}
		return false;
	}

	public bool ShouldRemoveDueToLackOfImplication(BodyPart Part)
	{
		if (Part.VariantTypeModel().ImpliedBy.IsNullOrEmpty())
		{
			return false;
		}
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		return ShouldRemoveDueToLackOfImplicationInternal(Part);
	}

	private bool ShouldRemoveDueToLackOfImplicationInternal(BodyPart Part, BodyPartType Type = null)
	{
		if (Type == null)
		{
			Type = Part.VariantTypeModel();
			if (Type.ImpliedBy.IsNullOrEmpty())
			{
				return false;
			}
		}
		int impliedPer = Type.GetImpliedPer();
		SomeBodyParts.Clear();
		OtherBodyParts.Clear();
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart.VariantType == Type.ImpliedBy && bodyPart.Laterality == Part.Laterality)
			{
				SomeBodyParts.Add(bodyPart);
			}
			if (bodyPart.VariantType == Type.Type || bodyPart.VariantType == bodyPart.VariantTypeModel().ImpliedBy)
			{
				OtherBodyParts.Add(bodyPart);
			}
		}
		if (OtherBodyParts.Count == 1 && OtherBodyParts[0] == Part && SomeBodyParts.Count <= impliedPer)
		{
			if (SomeBodyParts.Count > 0)
			{
				return false;
			}
		}
		else
		{
			int num = 0;
			int j = 0;
			for (int count2 = OtherBodyParts.Count; j < count2; j++)
			{
				if (OtherBodyParts[j] == Part)
				{
					for (int k = 0; k < impliedPer; k++)
					{
						if (num < SomeBodyParts.Count)
						{
							return false;
						}
						num++;
					}
				}
				else
				{
					num += impliedPer;
				}
			}
		}
		return true;
	}

	public bool DoesPartImplyPart(BodyPart PartMaybeImplying, BodyPart PartMaybeImplied)
	{
		if (PartMaybeImplied.VariantTypeModel().ImpliedBy.IsNullOrEmpty())
		{
			return false;
		}
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		return DoesPartImplyPartInternal(PartMaybeImplied, PartMaybeImplying);
	}

	private bool DoesPartImplyPartInternal(BodyPart PartMaybeImplying, BodyPart PartMaybeImplied, BodyPartType Type = null, bool TypeOnly = false)
	{
		if (Type == null)
		{
			Type = PartMaybeImplied.VariantTypeModel();
			if (Type.ImpliedBy.IsNullOrEmpty())
			{
				return false;
			}
		}
		SomeBodyParts.Clear();
		OtherBodyParts.Clear();
		int impliedPer = Type.GetImpliedPer();
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart.VariantType == Type.ImpliedBy && bodyPart.Laterality == PartMaybeImplied.Laterality)
			{
				SomeBodyParts.Add(bodyPart);
			}
			if (bodyPart.VariantType == Type.Type || (!TypeOnly && bodyPart.VariantType == bodyPart.VariantTypeModel().ImpliedBy))
			{
				OtherBodyParts.Add(bodyPart);
			}
		}
		if (OtherBodyParts.Count == 1 && OtherBodyParts[0] == PartMaybeImplied && SomeBodyParts.Count <= impliedPer)
		{
			if (SomeBodyParts.Contains(PartMaybeImplying))
			{
				return true;
			}
		}
		else
		{
			int num = 0;
			int j = 0;
			for (int count2 = OtherBodyParts.Count; j < count2; j++)
			{
				if (OtherBodyParts[j] == PartMaybeImplied)
				{
					for (int k = 0; k < impliedPer; k++)
					{
						if (num < SomeBodyParts.Count && SomeBodyParts[num] == PartMaybeImplying)
						{
							return true;
						}
						num++;
					}
				}
				else
				{
					num += impliedPer;
				}
			}
		}
		return false;
	}

	public void GetBodyPartsImpliedBy(out List<BodyPart> Result, BodyPart Part)
	{
		Result = null;
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart != Part && DoesPartImplyPartInternal(Part, bodyPart))
			{
				if (Result == null)
				{
					Result = new List<BodyPart>();
				}
				Result.Add(bodyPart);
			}
		}
	}

	public List<BodyPart> GetBodyPartsImpliedBy(BodyPart Part)
	{
		GetBodyPartsImpliedBy(out var Result, Part);
		return Result;
	}

	public BodyPart GetBodyPartOfTypeImpliedBy(BodyPart Part, BodyPartType Type)
	{
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		return GetBodyPartOfTypeImpliedByInternal(Part, Type);
	}

	private BodyPart GetBodyPartOfTypeImpliedByInternal(BodyPart Part, BodyPartType Type)
	{
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart != Part && bodyPart.VariantType == Type.Type && DoesPartImplyPartInternal(Part, bodyPart, Type, TypeOnly: true))
			{
				return bodyPart;
			}
		}
		return null;
	}

	public void CheckImpliedParts(int Depth = 0)
	{
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		bool flag = false;
		int i = 0;
		for (int count = Anatomies.BodyPartTypeList.Count; i < count; i++)
		{
			BodyPartType bodyPartType = Anatomies.BodyPartTypeList[i];
			if (bodyPartType.ImpliedBy.IsNullOrEmpty())
			{
				continue;
			}
			int num = 0;
			int num2 = 0;
			int num3 = -1;
			BodyPart bodyPart = null;
			int j = 0;
			for (int count2 = AllBodyParts.Count; j < count2; j++)
			{
				BodyPart bodyPart2 = AllBodyParts[j];
				if (bodyPart2.VariantType == bodyPartType.ImpliedBy)
				{
					num++;
				}
				if (bodyPart2.VariantType == bodyPartType.Type)
				{
					num2++;
					if (bodyPart2.Position > num3)
					{
						num3 = bodyPart2.Position;
						bodyPart = bodyPart2;
					}
				}
			}
			if (num <= 0)
			{
				continue;
			}
			int impliedPer = bodyPartType.GetImpliedPer();
			int num4 = num / impliedPer;
			if (num % impliedPer != 0)
			{
				num4++;
			}
			for (int k = num2; k < num4; k++)
			{
				BodyPart bodyPart3 = null;
				int l = 0;
				for (int count3 = AllBodyParts.Count; l < count3; l++)
				{
					BodyPart bodyPart4 = AllBodyParts[l];
					if (bodyPart4.VariantType == bodyPartType.ImpliedBy && GetBodyPartOfTypeImpliedByInternal(bodyPart4, bodyPartType) == null)
					{
						bodyPart3 = bodyPart4;
						break;
					}
				}
				if (bodyPart3 != null)
				{
					BodyPart bodyPart5;
					if (bodyPart != null)
					{
						BodyPart body = _Body;
						BodyPartType bodyPartType2 = bodyPartType;
						int laterality = bodyPart3.Laterality;
						bodyPart5 = body.AddPartAt(bodyPart, bodyPartType2, laterality, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, DoUpdate: false);
					}
					else
					{
						bodyPart5 = _Body.AddPartAt(Base: bodyPartType, Laterality: bodyPart3.Laterality, InsertBefore: new string[3] { "Feet", "Roots", "Thrown Weapon" }, DefaultBehavior: null, SupportsDependent: null, DependsOn: null, RequiresType: null, Manager: null, Category: null, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Extrinsic: null, Dynamic: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, DoUpdate: false);
					}
					bodyPart = bodyPart5;
					flag = true;
				}
			}
		}
		List<BodyPart> list = null;
		int m = 0;
		for (int count4 = AllBodyParts.Count; m < count4; m++)
		{
			if (ShouldRemoveDueToLackOfImplicationInternal(AllBodyParts[m]))
			{
				if (list == null)
				{
					list = new List<BodyPart>();
				}
				list.Add(AllBodyParts[m]);
			}
		}
		if (list != null)
		{
			int n = 0;
			for (int count5 = list.Count; n < count5; n++)
			{
				BodyPart removePart = list[n];
				RemovePart(removePart, DoUpdate: false);
				flag = true;
			}
		}
		if (flag)
		{
			UpdateBodyParts(Depth + 1);
			RecalculateArmor();
		}
	}

	public bool IsDefaultBehaviorClearOfEquipped(GameObject GO)
	{
		return _Body.IsDefaultBehaviorClearOfEquipped(GO);
	}

	public int GetMaximumElectricalConductivityOfMobilityBodyParts()
	{
		return _Body.GetMaximumElectricalConductivityOfMobilityBodyParts();
	}

	public int GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts(out GameObject CriticalObject)
	{
		return _Body.GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts(out CriticalObject);
	}

	public int GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts()
	{
		return _Body.GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts();
	}

	public int GetTotalElectricalConductivity()
	{
		return _Body.GetTotalElectricalConductivity();
	}

	public int GetMeanElectricalConductivity()
	{
		return GetTotalElectricalConductivity() / GetPartCount();
	}

	public int GetElectricalConductivity()
	{
		return Math.Min(GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts(), GetMeanElectricalConductivity());
	}

	public int GetElectricalConductivity(out GameObject CriticalObject)
	{
		return Math.Min(GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts(out CriticalObject), GetMeanElectricalConductivity());
	}

	public BodyPart ReduceParts(Func<BodyPart, BodyPart, BodyPart> Reduction)
	{
		return _Body.ReduceParts(Reduction);
	}

	public BodyPart GetUnequippedPreferredBodyPartOrAlternate(string PreferredType, string AlternateType)
	{
		BodyPart Alternate = null;
		return _Body.GetUnequippedPreferredBodyPartOrAlternate(PreferredType, AlternateType, ref Alternate) ?? Alternate;
	}
}
