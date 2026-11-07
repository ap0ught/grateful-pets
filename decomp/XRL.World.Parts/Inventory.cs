using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qud.API;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Inventory : IPart, IInventory
{
	public bool DropOnDeath = true;

	[NonSerialized]
	public List<GameObject> Objects = new List<GameObject>();

	[NonSerialized]
	private static Event eCommandRemoveObject = new Event("CommandRemoveObject", "Object", (object)null, "ForEquip", 0);

	[NonSerialized]
	private static Event eCommandFreeTakeObject = new Event("CommandTakeObject", "Object", null, "Context", null, "EnergyCost", 0);

	[NonSerialized]
	public bool ClearOnDeath = true;

	private int WeightCache = -1;

	public override int Priority => 90000;

	public override void Attach()
	{
		ParentObject.Inventory = this;
	}

	public override void Remove()
	{
		if (ParentObject?.Inventory == this)
		{
			ParentObject.Inventory = null;
		}
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteGameObjectList(Objects);
		base.Write(Basis, Writer);
	}

	public int Count(string blueprint)
	{
		int num = 0;
		for (int num2 = Objects.Count - 1; num2 >= 0; num2--)
		{
			if (Objects[num2].Blueprint == blueprint)
			{
				num += Objects[num2].Count;
			}
		}
		return num;
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Reader.ReadGameObjectList(Objects);
		for (int num = Objects.Count - 1; num >= 0; num--)
		{
			if (Objects[num] == null)
			{
				Objects.RemoveAt(num);
			}
		}
		base.Read(Basis, Reader);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void Validate()
	{
		List<GameObject> list = null;
		if (Objects != null)
		{
			for (int num = Objects.Count - 1; num >= 0; num--)
			{
				if (!Objects[num].IsValid())
				{
					if (list == null)
					{
						list = new List<GameObject>(1) { Objects[num] };
					}
					else
					{
						list.Add(Objects[num]);
					}
				}
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (GameObject item in list)
		{
			Objects.Remove(item);
		}
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
	}

	public bool TryStoreBackup()
	{
		List<GameObject> objects = Objects;
		if (objects == null)
		{
			return false;
		}
		if (HasObjectDirect(IsEmptyObject))
		{
			MetricsManager.LogError("Inventory has invalid objects, skipping backup store.");
			return false;
		}
		if (!SerializationWriter.TryGetShared(out var Writer))
		{
			return false;
		}
		try
		{
			ParentObject.RemoveStringProperty("InventoryBase64");
			Writer.Start(409);
			Writer.WriteGameObjectList(objects);
			Writer.FinalizeWrite();
			byte[] buffer = Writer.Stream.GetBuffer();
			int length = (int)Writer.Stream.Position;
			string value = Convert.ToBase64String(buffer, 0, length);
			ParentObject.SetStringProperty("InventoryBase64", value);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error writing container backup", x);
			return false;
		}
		finally
		{
			SerializationWriter.ReleaseShared();
		}
		return true;
	}

	public bool TryRestoreBackup()
	{
		string stringProperty = ParentObject.GetStringProperty("InventoryBase64");
		if (stringProperty.IsNullOrEmpty())
		{
			return false;
		}
		if (!SerializationReader.TryGetShared(out var Reader))
		{
			return false;
		}
		try
		{
			Reader.Stream.SetLength((4 * stringProperty.Length / 3 + 3) & -4);
			byte[] buffer = Reader.Stream.GetBuffer();
			if (!Convert.TryFromBase64String(stringProperty, buffer.AsSpan(), out var _))
			{
				throw new Exception("Failed to write Base64 string to buffer.");
			}
			List<GameObject> list = Objects ?? (Objects = new List<GameObject>());
			list.Clear();
			Reader.Start();
			Reader.ReadGameObjectList(list);
			Reader.FinalizeRead();
			foreach (GameObject item in list)
			{
				if (item.Physics != null)
				{
					item.Physics._InInventory = ParentObject;
				}
			}
			if (HasObjectDirect(IsEmptyObject))
			{
				MetricsManager.LogError("Restored backup has invalid objects, removing.");
				RemoveAll(IsEmptyObject);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error reading container backup", x);
			return false;
		}
		finally
		{
			SerializationReader.ReleaseShared();
		}
		return true;
	}

	private static bool IsEmptyObject(GameObject Object)
	{
		string blueprint = Object.Blueprint;
		return blueprint == "Object" || blueprint == "*PooledObject";
	}

	public void VerifyContents()
	{
		if (HasObjectDirect(IsEmptyObject))
		{
			bool flag = TryRestoreBackup();
			if (!flag)
			{
				RemoveAll(IsEmptyObject);
			}
			FlushWeightCache();
			string text = GetAnyBasisZone()?.ZoneID ?? "unknown";
			MetricsManager.LogError("Invalid inventory objects on " + ParentObject.DebugName + " in " + text + ", " + (flag ? "restored backup" : "cleared objects") + ".");
		}
	}

	public void AddObject(List<GameObject> Objects)
	{
		foreach (GameObject Object in Objects)
		{
			AddObject(Object, null, Silent: false, NoStack: false, FlushTransient: false);
		}
		FlushTransientCaches();
	}

	public GameObject AddObjectNoStack(GameObject GO)
	{
		return AddObject(GO, null, Silent: false, NoStack: true);
	}

	public GameObject AddObject(string Blueprint, bool Silent = false, bool NoStack = false)
	{
		GameObject gameObject = GameObject.Create(Blueprint);
		bool noStack = NoStack;
		return AddObject(gameObject, null, Silent, noStack);
	}

	public GameObject AddObjectToInventory(GameObject Object, GameObject Actor = null, bool Silent = false, bool NoStack = false, bool FlushTransient = true, string Context = null, IEvent ParentEvent = null)
	{
		Object.RemoveFromContext(ParentEvent);
		return AddObject(Object, null, Silent, NoStack, FlushTransient, ParentEvent);
	}

	public GameObject AddObject(GameObject Object, GameObject Actor = null, bool Silent = false, bool NoStack = false, bool FlushTransient = true, IEvent ParentEvent = null)
	{
		Physics physics = Object.Physics;
		if (physics == null || !physics.Takeable)
		{
			MetricsManager.LogError("Attempting to add untakeable object '" + Object.DebugName + "' to inventory of '" + ParentObject.DebugName + "'.");
			if (ParentObject.IsPlayer())
			{
				AutoAct.Interrupt(Object.t() + " can't be picked up");
			}
			return Object;
		}
		if (Object.IsInGraveyard())
		{
			MetricsManager.LogError("Attempting to add graveyard object '" + Object.DebugName + "' to inventory of '" + ParentObject.DebugName + "'.");
			return Object;
		}
		if (Object.IsInvalid())
		{
			MetricsManager.LogError("Attempting to add invalid object '" + Object.DebugName + "' to inventory of '" + ParentObject.DebugName + "'.");
			return Object;
		}
		bool flag = Objects.Count == 0;
		Objects.Add(Object);
		Cell cell = Object.CurrentCell;
		Object.Physics.InInventory = ParentObject;
		if (cell != null)
		{
			cell.RemoveObject(Object);
			Cell cell2 = Object.CurrentCell;
			if (cell2 != null && cell2.Objects.Contains(Object))
			{
				cell2.Objects.Remove(Object);
			}
			Object.Physics.CurrentCell = null;
		}
		FlushWeightCache();
		if (FlushTransient)
		{
			FlushTransientCaches();
		}
		if (flag)
		{
			CheckNonEmptyState();
		}
		AddedToInventoryEvent.Send(Actor ?? ParentObject, Object, Silent, NoStack, ParentEvent);
		if (ParentObject != null && ParentObject.IsPlayer() && Object != null)
		{
			ParentObject.FireEvent(Event.New("ObjectAddedToPlayerInventory", "Object", Object));
		}
		EncumbranceChangedEvent.Send(ParentObject, Object, Silent);
		return Object;
	}

	public void RemoveAll(Predicate<GameObject> Match, bool Obliterate = true)
	{
		for (int num = Objects.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = Objects[num];
			if (Match(gameObject))
			{
				Objects.RemoveAt(num);
				gameObject.Physics.InInventory = null;
				if (Obliterate)
				{
					gameObject.Obliterate(null, Silent: true);
				}
			}
		}
		FlushWeightCache();
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
		EncumbranceChangedEvent.Send(ParentObject);
	}

	public bool RemoveObjectFromInventory(GameObject Object, GameObject Actor = null, bool Silent = false, bool NoStack = false, bool FlushTransient = true, string Context = null, IEvent ParentEvent = null)
	{
		if (!Objects.Contains(Object))
		{
			return false;
		}
		Objects.Remove(Object);
		Object.Physics.InInventory = null;
		FlushWeightCache();
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
		EncumbranceChangedEvent.Send(ParentObject, Object, Silent);
		return true;
	}

	public void RemoveObject(GameObject Object, IEvent ParentEvent = null)
	{
		Objects.Remove(Object);
		Object.Physics.InInventory = null;
		FlushWeightCache();
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
		EncumbranceChangedEvent.Send(ParentObject, Object);
	}

	public void Clear()
	{
		Objects.Clear();
		FlushWeightCache();
		CheckEmptyState();
		EncumbranceChangedEvent.Send(ParentObject);
	}

	[Obsolete("version with MapInv argument should always be called")]
	public override IPart DeepCopy(GameObject Parent)
	{
		return base.DeepCopy(Parent);
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Inventory inventory = (Inventory)base.DeepCopy(Parent, MapInv);
		inventory.Objects = new List<GameObject>(Objects.Count);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = MapInv?.Invoke(Objects[i]) ?? Objects[i].DeepCopy(CopyEffects: false, CopyID: false, MapInv);
			if (gameObject != null)
			{
				gameObject.Physics.InInventory = Parent;
				inventory.Objects.Add(gameObject);
			}
		}
		return inventory;
	}

	public override void FinalizeCopyLate(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopyLate(Source, CopyEffects, CopyID, MapInv);
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
		else
		{
			CheckNonEmptyState();
		}
	}

	public int GetWeight()
	{
		if (WeightCache != -1)
		{
			return WeightCache;
		}
		return RecalculateWeight();
	}

	public void FlushWeightCache()
	{
		WeightCache = -1;
		ParentObject.FlushCarriedWeightCache();
		Inventory inventory = ParentObject.InInventory?.Inventory;
		if (inventory != null && inventory.WeightCache != -1)
		{
			inventory.FlushWeightCache();
		}
	}

	private int RecalculateWeight()
	{
		WeightCache = 0;
		for (int num = Objects.Count - 1; num >= 0; num--)
		{
			WeightCache += Objects[num].Weight;
		}
		return WeightCache;
	}

	public GameObject FindObject(Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject FindObjectByBlueprint(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject FindObjectByBlueprint(string Blueprint, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint && pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsDirect()
	{
		FlushWeightCache();
		return Objects;
	}

	public List<GameObject> GetObjectsDirect(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetObjectsDirect();
		}
		FlushWeightCache();
		List<GameObject> list = new List<GameObject>(Objects.Count);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Filter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public void GetObjectsDirect(List<GameObject> store)
	{
		FlushWeightCache();
		store.AddRange(Objects);
	}

	public void GetObjectsDirect(List<GameObject> store, Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			GetObjectsDirect(store);
			return;
		}
		FlushWeightCache();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Filter(Objects[i]))
			{
				store.Add(Objects[i]);
			}
		}
	}

	public List<GameObject> GetObjects()
	{
		return GetObjectsDirect((GameObject obj) => !obj.HasTag("HiddenInInventory"));
	}

	public void GetObjects(List<GameObject> Store)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory"))
			{
				Store.Add(Objects[i]);
			}
		}
	}

	public void GetObjects(List<GameObject> Store, Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			GetObjects(Store);
			return;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Filter(Objects[i]) && !Objects[i].HasTag("HiddenInInventory"))
			{
				Store.Add(Objects[i]);
			}
		}
	}

	public List<GameObject> GetObjects(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetObjects();
		}
		List<GameObject> list = new List<GameObject>(Objects.Count);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory") && Filter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsViaEventList(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetObjects();
		}
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory") && Filter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsReadonly()
	{
		List<GameObject> list = Event.NewGameObjectList();
		GetObjectsDirect(list, (GameObject obj) => !obj.HasTag("HiddenInInventory"));
		return list;
	}

	public List<GameObject> GetObjectsReadonly(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetObjectsReadonly();
		}
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory") && Filter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTag(string Name)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Name) && !Objects[i].HasTag("HiddenInInventory"))
			{
				num++;
			}
		}
		List<GameObject> list = new List<GameObject>(num);
		int j = 0;
		for (int count2 = Objects.Count; j < count2; j++)
		{
			if (Objects[j].HasTag(Name) && !Objects[j].HasTag("HiddenInInventory"))
			{
				list.Add(Objects[j]);
			}
		}
		return list;
	}

	public GameObject GetFirstObject()
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory"))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObject(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetFirstObject();
		}
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && Filter(Objects[i]))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectDirect()
	{
		return Objects?.First();
	}

	public GameObject GetFirstObjectDirect(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetFirstObjectDirect();
		}
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Filter(Objects[i]))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public int GetObjectCountDirect()
	{
		return Objects?.Count ?? 0;
	}

	public int GetObjectCountDirect(Predicate<GameObject> pFilter)
	{
		int num = 0;
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (pFilter(Objects[i]))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetObjectStackCount(bool IncludeHidden = true)
	{
		int num = 0;
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (IncludeHidden || !Objects[i].HasTag("HiddenInInventory"))
				{
					num += Objects[i].Count;
				}
			}
		}
		return num;
	}

	public int GetObjectStackCount(Predicate<GameObject> Filter, bool IncludeHidden = true)
	{
		int num = 0;
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if ((IncludeHidden || !Objects[i].HasTag("HiddenInInventory")) && Filter(Objects[i]))
				{
					num += Objects[i].Count;
				}
			}
		}
		return num;
	}

	public int GetObjectCount()
	{
		int num = 0;
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory"))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetObjectCount(Predicate<GameObject> pFilter)
	{
		int num = 0;
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && pFilter(Objects[i]))
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool HasObject()
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public Cell GetInventoryCell()
	{
		return ParentObject?.CurrentCell;
	}

	public Zone GetInventoryZone()
	{
		return ParentObject?.CurrentZone;
	}

	public bool InventoryContains(GameObject Object)
	{
		return Objects.Contains(Object);
	}

	public bool HasObject(GameObject Object)
	{
		if (Objects.Contains(Object))
		{
			return !Object.HasTag("HiddenInInventory");
		}
		return false;
	}

	public bool HasObject(string Blueprint)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Objects[i].Blueprint == Blueprint && !Objects[i].HasTag("HiddenInInventory"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObject(Predicate<GameObject> pFilter)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && pFilter(Objects[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectDirect(GameObject obj)
	{
		return Objects.Contains(obj);
	}

	public bool HasObjectDirect(string Blueprint)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Objects[i].Blueprint == Blueprint && !Objects[i].HasTag("HiddenInInventory"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectDirect(Predicate<GameObject> pFilter)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (pFilter(Objects[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ForeachObject(Action<GameObject> aProc)
	{
		if (Objects == null)
		{
			return;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory"))
			{
				aProc(Objects[i]);
			}
		}
	}

	public bool ForeachObject(Predicate<GameObject> pProc)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && !pProc(Objects[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ForeachObject(Action<GameObject> aProc, Predicate<GameObject> pFilter)
	{
		if (Objects == null)
		{
			return;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory") && pFilter(Objects[i]))
			{
				aProc(Objects[i]);
			}
		}
	}

	public bool ForeachObject(Predicate<GameObject> pProc, Predicate<GameObject> pFilter)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && pFilter(Objects[i]) && !pProc(Objects[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SafeForeachObject(Action<GameObject> aProc)
	{
		if (Objects.Count == 0)
		{
			return;
		}
		List<GameObject> list = null;
		for (int i = 0; i < (list?.Count ?? Objects.Count); i++)
		{
			GameObject gameObject = ((list == null) ? Objects[i] : list[i]);
			if (!gameObject.HasTag("HiddenInInventory"))
			{
				if (list == null)
				{
					list = new List<GameObject>(Objects);
				}
				if (Objects.Contains(gameObject))
				{
					aProc(gameObject);
				}
			}
		}
	}

	public bool SafeForeachObject(Predicate<GameObject> pProc)
	{
		if (Objects.Count == 0)
		{
			return true;
		}
		List<GameObject> list = null;
		for (int i = 0; i < (list?.Count ?? Objects.Count); i++)
		{
			GameObject gameObject = ((list == null) ? Objects[i] : list[i]);
			if (!gameObject.HasTag("HiddenInInventory"))
			{
				if (list == null)
				{
					list = new List<GameObject>(Objects);
				}
				if (Objects.Contains(gameObject) && !pProc(gameObject))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SafeForeachObject(Action<GameObject> aProc, Predicate<GameObject> pFilter)
	{
		if (Objects.Count == 0)
		{
			return;
		}
		List<GameObject> list = null;
		for (int i = 0; i < (list?.Count ?? Objects.Count); i++)
		{
			GameObject gameObject = ((list == null) ? Objects[i] : list[i]);
			if (!gameObject.HasTag("HiddenInInventory") && pFilter(gameObject))
			{
				if (list == null)
				{
					list = new List<GameObject>(Objects);
				}
				if (Objects.Contains(gameObject))
				{
					aProc(gameObject);
				}
			}
		}
	}

	public bool SafeForeachObject(Predicate<GameObject> pProc, Predicate<GameObject> pFilter)
	{
		if (Objects.Count == 0)
		{
			return true;
		}
		List<GameObject> list = null;
		for (int i = 0; i < (list?.Count ?? Objects.Count); i++)
		{
			GameObject gameObject = ((list == null) ? Objects[i] : list[i]);
			if (!gameObject.HasTag("HiddenInInventory") && pFilter(gameObject))
			{
				if (list == null)
				{
					list = new List<GameObject>(Objects);
				}
				if (Objects.Contains(gameObject) && !pProc(gameObject))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ReverseForeachObject(Action<GameObject> Action)
	{
		if (Objects.Count == 0)
		{
			return;
		}
		for (int num = Objects.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = Objects[num];
			if (!gameObject.HasTag("HiddenInInventory"))
			{
				Action(gameObject);
			}
		}
	}

	public void ForeachObjectWithRegisteredEvent(string EventName, Action<GameObject> aProc)
	{
		if (Objects == null)
		{
			return;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory") && Objects[i].HasRegisteredEvent(EventName))
			{
				aProc(Objects[i]);
			}
		}
	}

	public bool ForeachObjectWithRegisteredEvent(string EventName, Predicate<GameObject> pProc)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && Objects[i].HasRegisteredEvent(EventName) && !pProc(Objects[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ForeachObjectDirect(Action<GameObject> aProc)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				aProc(Objects[i]);
			}
		}
	}

	public bool ForeachObjectDirect(Predicate<GameObject> pProc)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!pProc(Objects[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool IsItemSlotAppropriate(GameObject Actor, GameObject Object, string SlotType)
	{
		if (Object.HasTagOrProperty("CannotEquip") || Object.HasTagOrProperty("NoEquip"))
		{
			return false;
		}
		QueryEquippableListEvent queryEquippableListEvent = QueryEquippableListEvent.FromPool(Actor, Object, SlotType);
		Object.HandleEvent(queryEquippableListEvent);
		return queryEquippableListEvent.List.Contains(Object);
	}

	private QueryEquippableListEvent GetEquipmentListEvent(List<GameObject> ObjectList, string SlotType, bool RequireDesirable = false, bool RequirePossible = false)
	{
		QueryEquippableListEvent queryEquippableListEvent = null;
		foreach (GameObject Object in ObjectList)
		{
			if (Object.WantEvent(QueryEquippableListEvent.ID, MinEvent.CascadeLevel) && !Object.HasTagOrProperty("CannotEquip") && !Object.HasTagOrProperty("NoEquip"))
			{
				if (queryEquippableListEvent == null)
				{
					queryEquippableListEvent = QueryEquippableListEvent.FromPool(ParentObject, Object, SlotType, RequireDesirable, RequirePossible);
				}
				queryEquippableListEvent.Item = Object;
				if (ParentObject.IsPlayer() || !Object.HasPropertyOrTag("NoAIEquip"))
				{
					Object.HandleEvent(queryEquippableListEvent);
				}
			}
		}
		return queryEquippableListEvent;
	}

	public void GetEquipmentListForSlot(List<GameObject> Return, string SlotType, List<GameObject> ObjectList, bool RequireDesirable = false, bool RequirePossible = false, bool SkipSort = false)
	{
		QueryEquippableListEvent equipmentListEvent = GetEquipmentListEvent(ObjectList, SlotType, RequireDesirable, RequirePossible);
		if (equipmentListEvent != null)
		{
			Return.AddRange(equipmentListEvent.List);
			if (!SkipSort && Return.Count > 1)
			{
				Return.Sort(new Brain.GearSorter(ParentObject));
			}
		}
	}

	public List<GameObject> GetEquipmentListForSlot(string SlotType, List<GameObject> ObjectList, bool RequireDesirable = false, bool RequirePossible = false, bool SkipSort = false)
	{
		QueryEquippableListEvent equipmentListEvent = GetEquipmentListEvent(ObjectList, SlotType, RequireDesirable, RequirePossible);
		if (equipmentListEvent == null)
		{
			return null;
		}
		List<GameObject> list = Event.NewGameObjectList(equipmentListEvent.List);
		if (!SkipSort && list.Count > 1)
		{
			list.Sort(new Brain.GearSorter(ParentObject));
		}
		return list;
	}

	public void GetEquipmentListForSlot(List<GameObject> Return, string SlotType, bool RequireDesirable = false, bool RequirePossible = false, bool SkipSort = false)
	{
		GetEquipmentListForSlot(Return, SlotType, GetObjectsReadonly(), RequireDesirable, RequirePossible, SkipSort);
	}

	public List<GameObject> GetEquipmentListForSlot(string SlotType, bool RequireDesirable = false, bool RequirePossible = false, bool SkipSort = false)
	{
		return GetEquipmentListForSlot(SlotType, GetObjectsReadonly(), RequireDesirable, RequirePossible, SkipSort);
	}

	public void CheckEmptyState()
	{
		if (ParentObject?.Render != null)
		{
			string propertyOrTag = ParentObject.GetPropertyOrTag("EmptyTile");
			if (propertyOrTag != null && ParentObject.Render != null)
			{
				ParentObject.Render.Tile = propertyOrTag.CachedCommaExpansion().GetRandomElement();
			}
			propertyOrTag = ParentObject.GetPropertyOrTag("EmptyDetailColor");
			if (propertyOrTag != null && ParentObject.Render != null)
			{
				ParentObject.Render.DetailColor = propertyOrTag.CachedCommaExpansion().GetRandomElement();
			}
		}
	}

	public void CheckNonEmptyState()
	{
		if (ParentObject?.Render != null)
		{
			string propertyOrTag = ParentObject.GetPropertyOrTag("FullTile");
			if (propertyOrTag != null)
			{
				ParentObject.Render.Tile = propertyOrTag.CachedCommaExpansion().GetRandomElement();
			}
			propertyOrTag = ParentObject.GetPropertyOrTag("FullDetailColor");
			if (propertyOrTag != null)
			{
				ParentObject.Render.DetailColor = propertyOrTag.CachedCommaExpansion().GetRandomElement();
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandEquipObject");
		Registrar.Register("CommandForceEquipObject");
		Registrar.Register("CommandForceUnequipObject");
		Registrar.Register("CommandGet");
		Registrar.Register("CommandGetFrom");
		Registrar.Register("CommandRemoveObject");
		Registrar.Register("CommandTakeObject");
		Registrar.Register("CommandUnequipObject");
		Registrar.Register("PerformDrop");
		Registrar.Register("PerformEquip");
		Registrar.Register("PerformTake");
		Registrar.Register("PerformUnequip");
		base.Register(Object, Registrar);
	}

	public bool CheckOverburdened()
	{
		if (ParentObject.IsOverburdened())
		{
			if (!ParentObject.HasEffect<Overburdened>())
			{
				ParentObject.ApplyEffect(new Overburdened());
			}
			return true;
		}
		ParentObject.RemoveEffect<Overburdened>();
		return false;
	}

	public static List<GameObject> GetInteractableObjects(GameObject Actor, Cell CC, Cell C, bool DoInteractNearby)
	{
		List<GameObject> list = ((C == CC || !C.IsSolidFor(Actor)) ? C.GetObjectsInCell() : C.GetCanInteractInCellWithSolidObjectsFor(Actor));
		if (list.Count > 1)
		{
			list.Sort((GameObject a, GameObject b) => a.SortVs(b, null, UseCategory: true, UseDisplayName: true, UseEvent: true, UseRenderLayer: true));
		}
		List<GameObject> list2 = Event.NewGameObjectList();
		int num = (Options.DebugInternals ? (-1) : 0);
		int num2 = 0;
		for (int count = list.Count; num2 < count; num2++)
		{
			GameObject gameObject = list[num2];
			if (gameObject != Actor && (gameObject.Render == null || (gameObject.Render.Visible && gameObject.Render.RenderLayer > num)) && (gameObject.IsTakeable() || (DoInteractNearby && EquipmentAPI.CanBeTwiddled(gameObject, Actor))))
			{
				list2.Add(gameObject);
			}
		}
		return list2;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandGet" || E.ID == "CommandGetFrom")
		{
			bool doInteractNearby = E.ID == "CommandGetFrom";
			bool flag = !doInteractNearby;
			if (flag && E.HasParameter("getOne"))
			{
				flag = false;
			}
			if (flag && Options.AskForOneItem)
			{
				flag = false;
			}
			bool flag2 = E.HasParameter("SmartUse");
			XRLCore.Core.RenderBaseToBuffer(Popup._ScreenBuffer);
			Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
			GameObject Opener = ParentObject;
			Cell CC = Opener.CurrentCell;
			Cell C = E.GetParameter("TargetCell") as Cell;
			if (E.HasParameter("Direction"))
			{
				C = CC.GetCellFromDirection(E.GetStringParameter("Direction"));
			}
			if (C == null)
			{
				if (doInteractNearby && ParentObject.IsPlayer())
				{
					string text = XRL.UI.PickDirection.ShowPicker(doInteractNearby ? "Interact" : "Get");
					if (text == null)
					{
						return true;
					}
					C = CC.GetCellFromDirection(text);
				}
				else
				{
					C = CC;
				}
				if (C == null)
				{
					return true;
				}
			}
			List<GameObject> interactableObjects = GetInteractableObjects(Opener, CC, C, doInteractNearby);
			if ((interactableObjects.Count == 1) & flag)
			{
				GameObject gameObject = interactableObjects[0];
				Event obj = Event.New("CommandTakeObject");
				obj.SetParameter("Object", gameObject);
				obj.SetParameter("Context", E.GetStringParameter("Context"));
				obj.SetSilent(Silent: false);
				if (Opener.FireEvent(obj))
				{
					C.RemoveObject(gameObject);
				}
			}
			else if (interactableObjects.Count > 0)
			{
				bool RequestInterfaceExit = false;
				PickItem.ShowPicker(interactableObjects, ref RequestInterfaceExit, null, PickItem.PickItemDialogStyle.GetItemDialog, Opener, null, C, null, PreserveOrder: false, () => GetInteractableObjects(Opener, CC, C, doInteractNearby));
				if (RequestInterfaceExit)
				{
					E.RequestInterfaceExit();
				}
			}
			else if (Opener.IsPlayer())
			{
				if (doInteractNearby | flag2)
				{
					Popup.ShowFail("There's nothing you can interact with.");
				}
				else
				{
					Popup.ShowFail("There's nothing to take.");
				}
			}
		}
		else if (E.ID == "PerformEquip")
		{
			GameObject gameObject2 = E.GetGameObjectParameter("Object");
			BodyPart bodyPart = E.GetParameter("BodyPart") as BodyPart;
			bool flag3 = E.IsSilent();
			int intParameter = E.GetIntParameter("AutoEquipTry");
			string stringParameter = E.GetStringParameter("FailureMessage");
			List<GameObject> parameter = E.GetParameter<List<GameObject>>("WasUnequipped");
			bool flag4 = E.HasFlag("DestroyOnUnequipDeclined");
			if (bodyPart == null)
			{
				return false;
			}
			if (gameObject2.Count > 1)
			{
				gameObject2 = gameObject2.RemoveOne();
			}
			GameObject Object = bodyPart.Equipped;
			if (Object != null)
			{
				Event obj2 = Event.New("CommandUnequipObject", "BodyPart", bodyPart, "Sound", "");
				if (flag3)
				{
					obj2.SetSilent(Silent: true);
				}
				if (intParameter > 0)
				{
					obj2.SetParameter("AutoEquipTry", intParameter);
				}
				if (!stringParameter.IsNullOrEmpty())
				{
					obj2.SetParameter("FailureMessage", stringParameter);
				}
				if (flag4)
				{
					obj2.SetFlag("DestroyOnUnequipDeclined", State: true);
				}
				if (!ParentObject.FireEvent(obj2))
				{
					string stringParameter2 = obj2.GetStringParameter("FailureMessage");
					if (!stringParameter2.IsNullOrEmpty() && stringParameter2 != stringParameter)
					{
						stringParameter = stringParameter2;
						E.SetParameter("FailureMessage", stringParameter);
					}
					if (obj2.HasFlag("DestroyOnUnequipDeclined"))
					{
						flag4 = true;
						E.SetFlag("DestroyOnUnequipDeclined", State: true);
					}
					return false;
				}
				if (parameter != null && GameObject.Validate(ref Object) && !parameter.Contains(Object))
				{
					parameter.Add(Object);
				}
			}
			string FailureMessage = null;
			if (!bodyPart.DoEquip(gameObject2, ref FailureMessage, flag3, ForDeepCopy: false, UnequipOthers: true, intParameter, parameter))
			{
				if (!FailureMessage.IsNullOrEmpty() && stringParameter.IsNullOrEmpty())
				{
					stringParameter = FailureMessage;
					E.SetParameter("FailureMessage", stringParameter);
				}
				return false;
			}
			if (!flag3)
			{
				DidXToY("equip", gameObject2, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true);
				if (ParentObject.IsPlayer())
				{
					if (!E.TryGetStringParameter("Sound", out var Value))
					{
						Value = bodyPart.VariantTypeModel()?.EquipSound;
						if (Value.IsNullOrEmpty())
						{
							Value = gameObject2.GetTagOrStringProperty("EquipSound");
						}
						if (Value.IsNullOrEmpty())
						{
							Value = gameObject2.GetTagOrStringProperty("ReloadSound");
						}
					}
					PlayWorldSound(Value);
					Value = gameObject2.GetTagOrStringProperty("EquipLayerSound");
					if (!Value.IsNullOrEmpty())
					{
						DelimitedEnumeratorChar enumerator = Value.DelimitedBy(',').GetEnumerator();
						while (enumerator.MoveNext())
						{
							ReadOnlySpan<char> current = enumerator.Current;
							if (current.Length == Value.Length)
							{
								PlayWorldSound(Value);
								break;
							}
							PlayWorldSound(new string(current));
						}
					}
				}
			}
			EquippedEvent.Send(ParentObject, gameObject2, bodyPart);
			EquipperEquippedEvent.Send(ParentObject, gameObject2, bodyPart);
		}
		else if (E.ID == "PerformUnequip")
		{
			if (!(E.GetParameter("BodyPart") is BodyPart bodyPart2))
			{
				return false;
			}
			if (bodyPart2.Equipped == null)
			{
				return true;
			}
			GameObject equipped = bodyPart2.Equipped;
			bodyPart2.Unequip();
			if (equipped != null && !E.IsSilent() && ParentObject.IsPlayer())
			{
				if (!E.TryGetStringParameter("Sound", out var Value2))
				{
					Value2 = bodyPart2.VariantTypeModel()?.UnequipSound;
					if (Value2.IsNullOrEmpty())
					{
						Value2 = equipped.GetTagOrStringProperty("UnequipSound");
					}
				}
				PlayWorldSound(Value2);
			}
			ParentObject.FireEvent(Event.New("EquipperUnequipped", "Object", equipped));
		}
		else if (E.ID == "PerformTake")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Container");
			bool flag5 = E.HasFlag("NoStack");
			string text2 = null;
			if (gameObjectParameter2 != null && !gameObjectParameter2.IsCreature && gameObjectParameter2.CurrentCell != null && gameObjectParameter2.CurrentCell != ParentObject.CurrentCell)
			{
				text2 = ((!ParentObject.IsPlayer()) ? ParentObject.DescribeRelativeDirectionToward(gameObjectParameter2) : ParentObject.DescribeDirectionToward(gameObjectParameter2));
			}
			else if (gameObjectParameter2 == null)
			{
				Cell cell = gameObjectParameter.GetCurrentCell();
				Cell cell2 = ParentObject.GetCurrentCell();
				if (cell != null && cell2 != null && cell != cell2)
				{
					text2 = Directions.GetIncomingDirectionDescription(ParentObject, cell2.GetDirectionFromCell(cell));
				}
			}
			if (!Objects.Contains(gameObjectParameter))
			{
				if (ParentObject.IsPlayer() && gameObjectParameter.InInventory != ParentObject && gameObjectParameter.Equipped != ParentObject && gameObjectParameter.IsOwned())
				{
					if (Popup.ShowYesNoCancel("That is not owned by you. Are you sure you want to take it?") != DialogResult.Yes)
					{
						return false;
					}
					gameObjectParameter.Physics.BroadcastForHelp(ParentObject);
				}
				gameObjectParameter.RemoveFromContext(E);
				FlushWeightCache();
				if (!E.IsSilent())
				{
					GameObject gameObjectParameter3 = E.GetGameObjectParameter("PutBy");
					if (gameObjectParameter3 != null)
					{
						if (ParentObject.CurrentCell != null)
						{
							text2 = ((!gameObjectParameter3.IsPlayer()) ? gameObjectParameter3.DescribeRelativeDirectionToward(ParentObject) : gameObjectParameter3.DescribeDirectionToward(ParentObject));
						}
						string indirectPreposition = ParentObject.GetPart<Container>()?.Preposition ?? "in";
						IComponent<GameObject>.WDidXToYWithZ(gameObjectParameter3, "put", gameObjectParameter, indirectPreposition, ParentObject, text2, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: true, IndefiniteIndirectObjectForOthers: true);
					}
					else if (gameObjectParameter2 != null)
					{
						DidXToYWithZ("take", gameObjectParameter, "from", gameObjectParameter2, text2);
					}
					else if (text2 != null)
					{
						DidXToY("take", gameObjectParameter, text2);
					}
					else
					{
						DidXToY("take", gameObjectParameter);
					}
				}
				_ = gameObjectParameter?.Blueprint;
				AddObject(gameObjectParameter, null, Silent: false, NoStack: true, FlushTransient: true, E);
				string stringParameter3 = E.GetStringParameter("Context");
				TakenEvent.Send(gameObjectParameter, ParentObject, stringParameter3);
				TookEvent.Send(gameObjectParameter, ParentObject, stringParameter3);
				if (!flag5 && gameObjectParameter.IsValid())
				{
					CheckStacks();
				}
			}
		}
		else if (E.ID == "PerformDrop")
		{
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Object");
			if (Objects.Contains(gameObjectParameter4))
			{
				RemoveObject(gameObjectParameter4);
				FlushWeightCache();
				if (!E.IsSilent())
				{
					DidXToY("drop", gameObjectParameter4);
				}
				DroppedEvent.Send(ParentObject, gameObjectParameter4, E.HasFlag("Forced"));
			}
		}
		else if (E.ID == "CommandRemoveObject")
		{
			GameObject gameObjectParameter5 = E.GetGameObjectParameter("Object");
			if (gameObjectParameter5 != null)
			{
				if (!ParentObject.HasRegisteredEvent("BeginDrop") || ParentObject.FireEvent(Event.New("BeginDrop", "Object", gameObjectParameter5, "ForEquip", E.GetIntParameter("ForEquip"))))
				{
					if (!gameObjectParameter5.HasRegisteredEvent("BeginBeingDropped") || gameObjectParameter5.FireEvent(Event.New("BeginBeingDropped", "TakingObject", ParentObject)))
					{
						Event obj3 = Event.New("PerformDrop");
						obj3.SetParameter("Object", gameObjectParameter5);
						if (E.HasFlag("Forced"))
						{
							obj3.SetFlag("Forced", State: true);
						}
						if (E.IsSilent())
						{
							obj3.SetSilent(Silent: true);
						}
						return ParentObject.FireEvent(obj3);
					}
					return false;
				}
				return false;
			}
		}
		else if (E.ID == "CommandEquipObject" || E.ID == "CommandForceEquipObject")
		{
			GameObject Object2 = E.GetGameObjectParameter("Object");
			Physics physics = Object2.Physics;
			if (physics == null || !physics.Takeable)
			{
				MetricsManager.LogError("Attempting to equip untakeable object '" + Object2.DebugName + "' to '" + ParentObject.DebugName + "'.");
				return false;
			}
			if (Object2.IsInGraveyard())
			{
				MetricsManager.LogError("Attempting to equip graveyard object '" + Object2.DebugName + "' to '" + ParentObject.DebugName + "'.");
				return false;
			}
			if (Object2.IsInvalid())
			{
				MetricsManager.LogError("Attempting to equip invalid object '" + Object2.DebugName + "' to '" + ParentObject.DebugName + "'.");
				return false;
			}
			BodyPart bodyPart3 = E.GetParameter("BodyPart") as BodyPart;
			bool flag6 = E.ID == "CommandForceEquipObject";
			bool flag7 = E.HasFlag("SemiForced");
			bool flag8 = E.IsSilent();
			int intParameter2 = E.GetIntParameter("AutoEquipTry");
			string FailureMessage2 = E.GetStringParameter("FailureMessage");
			List<GameObject> list = (E.GetParameter("WasUnequipped") as List<GameObject>) ?? Event.NewGameObjectList();
			bool flag9 = E.HasFlag("DestroyOnUnequipDeclined");
			if (!flag6 && !flag7)
			{
				if (ParentObject.HasEffect<Stuck>())
				{
					if (!flag8 && ParentObject.IsPlayer())
					{
						FailureMessage2 = "You cannot equip items while stuck!";
						if (intParameter2 > 0)
						{
							E.SetParameter("FailureMessage", FailureMessage2);
						}
						else
						{
							Popup.ShowFail(FailureMessage2);
						}
					}
					return false;
				}
				if (!ParentObject.CanMoveExtremities(null, !flag8, Involuntary: false, AllowTelekinetic: true))
				{
					return false;
				}
			}
			Cell cell3 = Object2.CurrentCell;
			GameObject inInventory = Object2.InInventory;
			int num = 0;
			bool flag10 = false;
			try
			{
				if (!flag6 && inInventory != ParentObject && Object2.Equipped != ParentObject)
				{
					Object2.SplitFromStack();
					if (!ParentObject.ReceiveObject(Object2, NoStack: true))
					{
						Object2.CheckStack();
						return false;
					}
				}
				GameObject gameObject3 = Object2?.InInventory;
				if (gameObject3 != null && gameObject3 != ParentObject && !gameObject3.FireEvent(Event.New("BeforeContentsTaken", "Taker", ParentObject)))
				{
					return false;
				}
				if (ParentObject.IsPlayer() && Object2.InInventory != ParentObject && Object2.Equipped != ParentObject && (Object2.CurrentCell != null || Object2.InInventory != null) && !Object2.Owner.IsNullOrEmpty())
				{
					if (E.HasFlag("OwnershipViolationDeclined"))
					{
						return false;
					}
					if (!E.HasFlag("OwnershipViolationConfirmed"))
					{
						E.SetFlag("OwnershipViolationChecked", State: true);
						if (Popup.ShowYesNoCancel("That is not owned by you. Are you sure you want to take it?") != DialogResult.Yes)
						{
							E.SetFlag("OwnershipViolationDeclined", State: true);
							return false;
						}
						E.SetFlag("OwnershipViolationConfirmed", State: true);
						Object2.Physics.BroadcastForHelp(ParentObject);
					}
				}
				if (gameObject3 != null && gameObject3 != ParentObject && !gameObject3.FireEvent(Event.New("AfterContentsTaken", "Taker", ParentObject)))
				{
					return false;
				}
				List<BodyPart> list2 = QuerySlotListEvent.GetFor(ParentObject, Object2, ref FailureMessage2);
				if (!flag6 && !flag7)
				{
					if (list2.Count == 0)
					{
						if (!flag8 && ParentObject.IsPlayer())
						{
							if (FailureMessage2.IsNullOrEmpty())
							{
								FailureMessage2 = "You cannot equip " + Object2.t() + ".";
							}
							if (intParameter2 > 0)
							{
								E.SetParameter("FailureMessage", FailureMessage2);
							}
							else
							{
								Popup.ShowFail(FailureMessage2);
							}
						}
						return false;
					}
					if (bodyPart3 != null && !list2.Contains(bodyPart3))
					{
						if (!flag8 && ParentObject.IsPlayer())
						{
							if (FailureMessage2.IsNullOrEmpty())
							{
								FailureMessage2 = "You cannot equip " + Object2.t() + " on your " + bodyPart3.GetOrdinalName() + ".";
							}
							if (intParameter2 > 0)
							{
								E.SetParameter("FailureMessage", FailureMessage2);
							}
							else
							{
								Popup.ShowFail(FailureMessage2);
							}
						}
						return false;
					}
				}
				if (bodyPart3 == null && ParentObject.IsPlayer())
				{
					List<string> list3 = new List<string>(list2.Count);
					List<char> list4 = new List<char>(list2.Count);
					char c = 'a';
					foreach (BodyPart item in list2)
					{
						list3.Add(item.ToString());
						list4.Add(c);
						c = (char)(c + 1);
					}
					int defaultSelected = 0;
					if (Object2.HasTag("MeleeWeapon"))
					{
						if (list2.Any((BodyPart p) => p.Primary))
						{
							defaultSelected = list2.IndexOf(list2.First((BodyPart p) => p.Primary));
						}
					}
					else if (list2.Any((BodyPart p) => p.Type != "Hand"))
					{
						defaultSelected = list2.IndexOf(list2.First((BodyPart p) => p.Type != "Hand"));
					}
					int num2 = Popup.PickOption("", null, "", "Sounds/UI/ui_notification", list3.ToArray(), list4.ToArray(), null, null, null, null, null, 0, 60, defaultSelected, -1, AllowEscape: true);
					if (num2 == -1)
					{
						return false;
					}
					bodyPart3 = list2[num2];
				}
				int num3 = 1000;
				if (bodyPart3 != null && bodyPart3.Type == "Thrown Weapon")
				{
					num3 = 0;
				}
				else if (ParentObject.CurrentCell == null)
				{
					num3 = 0;
				}
				num = E.GetIntParameter("EnergyCost", num3);
				if (bodyPart3 == null || Object2 == null)
				{
					return false;
				}
				if (Object2.Count > 1)
				{
					Object2 = Object2.RemoveOne();
				}
				if (ParentObject.HasRegisteredEvent("BeginEquip"))
				{
					Event obj4 = Event.New("BeginEquip", "Object", Object2, "BodyPart", bodyPart3);
					if (flag8)
					{
						obj4.SetSilent(Silent: true);
					}
					if (intParameter2 > 0)
					{
						obj4.SetParameter("AutoEquipTry", intParameter2);
					}
					if (!FailureMessage2.IsNullOrEmpty())
					{
						obj4.SetParameter("FailureMessage", FailureMessage2);
					}
					if (list != null)
					{
						obj4.SetParameter("WasUnequipped", list);
					}
					if (flag9)
					{
						obj4.SetFlag("DestroyOnUnequipDeclined", State: true);
					}
					bool flag11 = ParentObject.FireEvent(obj4);
					string stringParameter4 = obj4.GetStringParameter("FailureMessage");
					if (!stringParameter4.IsNullOrEmpty() && stringParameter4 != FailureMessage2)
					{
						FailureMessage2 = stringParameter4;
						E.SetParameter("FailureMessage", FailureMessage2);
					}
					if (obj4.HasFlag("DestroyOnUnequipDeclined"))
					{
						flag9 = true;
						E.SetFlag("DestroyOnUnequipDeclined", State: true);
					}
					if (!flag11 && !flag6)
					{
						return false;
					}
				}
				if (Object2.HasRegisteredEvent("BeginBeingEquipped"))
				{
					Event obj5 = Event.New("BeginBeingEquipped", "EquippingObject", ParentObject, "Equipper", ParentObject, "BodyPart", bodyPart3);
					if (flag8)
					{
						obj5.SetSilent(Silent: true);
					}
					if (intParameter2 > 0)
					{
						obj5.SetParameter("AutoEquipTry", intParameter2);
					}
					if (!FailureMessage2.IsNullOrEmpty())
					{
						obj5.SetParameter("FailureMessage", FailureMessage2);
					}
					if (list != null)
					{
						obj5.SetParameter("WasUnequipped", list);
					}
					if (flag9)
					{
						obj5.SetFlag("DestroyOnUnequipDeclined", flag9);
					}
					bool flag12 = Object2.FireEvent(obj5);
					string stringParameter5 = obj5.GetStringParameter("FailureMessage");
					if (!stringParameter5.IsNullOrEmpty() && stringParameter5 != FailureMessage2)
					{
						FailureMessage2 = stringParameter5;
						E.SetParameter("FailureMessage", FailureMessage2);
					}
					if (obj5.HasFlag("DestroyOnUnequipDeclined"))
					{
						flag9 = true;
						E.SetFlag("DestroyOnUnequipDeclined", State: true);
					}
					if (!flag12 && !flag6)
					{
						return false;
					}
				}
				eCommandRemoveObject.SetParameter("Object", Object2);
				eCommandRemoveObject.SetFlag("ForEquip", State: true);
				eCommandRemoveObject.SetSilent(Silent: true);
				if (!ParentObject.FireEvent(eCommandRemoveObject) && !flag6)
				{
					return false;
				}
				if (Object2.CurrentCell != null && !Object2.CurrentCell.RemoveObject(Object2))
				{
					return false;
				}
				Event obj6 = Event.New("PerformEquip", "Object", Object2);
				obj6.SetParameter("BodyPart", bodyPart3);
				if (E.TryGetStringParameter("Sound", out var Value3))
				{
					obj6.SetParameter("Sound", Value3);
				}
				if (flag8)
				{
					obj6.SetSilent(Silent: true);
				}
				if (intParameter2 > 0)
				{
					obj6.SetParameter("AutoEquipTry", intParameter2);
				}
				if (!FailureMessage2.IsNullOrEmpty())
				{
					obj6.SetParameter("FailureMessage", FailureMessage2);
				}
				if (list != null)
				{
					obj6.SetParameter("WasUnequipped", list);
				}
				if (flag9)
				{
					obj6.SetFlag("DestroyOnUnequipDeclined", State: true);
				}
				bool flag13 = ParentObject.FireEvent(obj6);
				string stringParameter6 = obj6.GetStringParameter("FailureMessage");
				if (!stringParameter6.IsNullOrEmpty() && stringParameter6 != FailureMessage2)
				{
					FailureMessage2 = stringParameter6;
					E.SetParameter("FailureMessage", FailureMessage2);
				}
				if (obj6.HasFlag("DestroyOnUnequipDeclined"))
				{
					flag9 = true;
					E.SetFlag("DestroyOnUnequipDeclined", State: true);
				}
				if (!flag13 && !flag6)
				{
					Event obj7 = Event.New("CommandTakeObject");
					obj7.SetParameter("Object", Object2);
					obj7.SetParameter("Context", E.GetStringParameter("Context"));
					obj7.SetSilent(Silent: true);
					if (!ParentObject.FireEvent(obj7))
					{
						if (ParentObject.CurrentCell != null)
						{
							ParentObject.CurrentCell.AddObject(Object2, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Silent: false, Repaint: true, FlushTransient: true, null, null, null, ParentObject);
						}
						else if (ParentObject.IsPlayer())
						{
							Popup.ShowFail("Error dropping object, removing to graveyard zone! (Inventory.cs:CommandEquipObject)");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayName + "] Error dropping object, removing to graveyard zone! (Inventory.cs:CommandEquipObject)");
						}
					}
					if (!flag8 && intParameter2 <= 0 && ParentObject.IsPlayer())
					{
						string text3 = null;
						if (!FailureMessage2.IsNullOrEmpty())
						{
							text3 = FailureMessage2;
						}
						if (list != null && list.Count > 0)
						{
							string text4 = ParentObject.DescribeUnequip(list);
							if (!text4.IsNullOrEmpty())
							{
								if (text3 == null)
								{
									text3 = "";
								}
								if (text3 != null)
								{
									text3 += "\n\n";
								}
								text3 += text4;
							}
						}
						if (!text3.IsNullOrEmpty())
						{
							Popup.ShowFail(text3);
						}
					}
					return false;
				}
				flag10 = true;
			}
			finally
			{
				if (!flag10)
				{
					if (cell3 != null)
					{
						if (GameObject.Validate(ref Object2) && Object2.CurrentCell != cell3)
						{
							Object2.RemoveFromContext(E);
							cell3.AddObject(Object2, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Silent: false, Repaint: true, FlushTransient: true, null, null, null, ParentObject);
						}
					}
					else if (inInventory?.Inventory != null && GameObject.Validate(ref Object2) && Object2.InInventory != inInventory)
					{
						Object2.RemoveFromContext(E);
						inInventory.Inventory.AddObject(Object2);
					}
				}
			}
			if (flag10 && num > 0)
			{
				ParentObject.UseEnergy(num, "Equip Item");
			}
		}
		else if (E.ID == "CommandUnequipObject" || E.ID == "CommandForceUnequipObject")
		{
			bool flag14 = E.ID.Contains("Force") || E.HasFlag("Forced");
			bool flag15 = E.HasFlag("SemiForced");
			bool flag16 = E.HasFlag("NoStack");
			bool flag17 = E.IsSilent();
			int intParameter3 = E.GetIntParameter("AutoEquipTry");
			string FailureMessage3 = E.GetStringParameter("FailureMessage");
			bool DestroyOnUnequipDeclined = E.HasFlag("DestroyOnUnequipDeclined");
			BodyPart bodyPart4 = E.GetParameter("BodyPart") as BodyPart;
			if (bodyPart4 == null)
			{
				bodyPart4 = (E.GetGameObjectParameter("Target") ?? E.GetGameObjectParameter("Object"))?.EquippedOn();
				if (bodyPart4 == null)
				{
					return false;
				}
			}
			GameObject Object3 = bodyPart4.Equipped;
			if (Object3 == null)
			{
				return false;
			}
			if (!flag14 && !flag15)
			{
				if (ParentObject.HasEffect<Stuck>())
				{
					FailureMessage3 = "You cannot remove items while stuck!";
					E.SetParameter("FailureMessage", FailureMessage3);
					if (!flag17 && intParameter3 <= 0 && ParentObject.IsPlayer())
					{
						Popup.ShowFail(FailureMessage3);
					}
					return false;
				}
				if (!ParentObject.CheckFrozen(Telepathic: false, Telekinetic: true, Silent: true))
				{
					FailureMessage3 = "You are frozen solid!";
					E.SetParameter("FailureMessage", FailureMessage3);
					if (!flag17 && intParameter3 <= 0 && ParentObject.IsPlayer())
					{
						Popup.ShowFail(FailureMessage3);
					}
					return false;
				}
			}
			Event obj8 = Event.New("BeginUnequip", "BodyPart", bodyPart4);
			obj8.SetFlag("Forced", flag14);
			obj8.SetFlag("SemiForced", flag15);
			if (flag16)
			{
				obj8.SetFlag("NoStack", State: true);
			}
			if (flag17)
			{
				obj8.SetSilent(Silent: true);
			}
			if (intParameter3 > 0)
			{
				obj8.SetParameter("AutoEquipTry", intParameter3);
			}
			if (!FailureMessage3.IsNullOrEmpty())
			{
				obj8.SetParameter("FailureMessage", FailureMessage3);
			}
			if (DestroyOnUnequipDeclined)
			{
				obj8.SetFlag("DestroyOnUnequipDeclined", DestroyOnUnequipDeclined);
			}
			if (!ParentObject.FireEvent(obj8))
			{
				string stringParameter7 = obj8.GetStringParameter("FailureMessage");
				if (!stringParameter7.IsNullOrEmpty() && stringParameter7 != FailureMessage3)
				{
					FailureMessage3 = stringParameter7;
					E.SetParameter("FailureMessage", stringParameter7);
				}
				if (obj8.HasFlag("DestroyOnUnequipDeclined"))
				{
					DestroyOnUnequipDeclined = true;
					E.SetFlag("DestroyOnUnequipDeclined", State: true);
				}
				if (!flag17 && intParameter3 <= 0 && ParentObject.IsPlayer())
				{
					Popup.ShowFail(FailureMessage3);
				}
				return false;
			}
			bool flag18 = Object3.BeginBeingUnequipped(ref FailureMessage3, ref DestroyOnUnequipDeclined, ParentObject, ParentObject, bodyPart4, flag17, flag14, flag15, intParameter3);
			E.SetParameter("FailureMessage", FailureMessage3);
			E.SetFlag("DestroyOnUnequipDeclined", DestroyOnUnequipDeclined);
			if (!flag18)
			{
				if (!flag17 && intParameter3 <= 0 && ParentObject.IsPlayer())
				{
					Popup.ShowFail(FailureMessage3);
				}
				return false;
			}
			Event obj9 = Event.New("PerformUnequip");
			obj9.SetParameter("BodyPart", bodyPart4);
			obj9.SetFlag("Forced", flag14);
			obj9.SetFlag("SemiForced", flag15);
			if (E.TryGetStringParameter("Sound", out var Value4))
			{
				obj9.SetParameter("Sound", Value4);
			}
			if (flag16)
			{
				obj9.SetFlag("NoStack", State: true);
			}
			if (flag17)
			{
				obj9.SetSilent(Silent: true);
			}
			if (intParameter3 > 0)
			{
				obj9.SetParameter("AutoEquipTry", intParameter3);
			}
			if (!FailureMessage3.IsNullOrEmpty())
			{
				obj9.SetParameter("FailureMessage", FailureMessage3);
			}
			if (DestroyOnUnequipDeclined)
			{
				obj9.SetFlag("DestroyOnUnequipDeclined", State: true);
			}
			if (!ParentObject.FireEvent(obj9))
			{
				string stringParameter8 = obj9.GetStringParameter("FailureMessage");
				if (!stringParameter8.IsNullOrEmpty() && stringParameter8 != FailureMessage3)
				{
					FailureMessage3 = stringParameter8;
					E.SetParameter("FailureMessage", stringParameter8);
				}
				if (obj9.HasFlag("DestroyOnUnequipDeclined"))
				{
					DestroyOnUnequipDeclined = true;
					E.SetFlag("DestroyOnUnequipDeclined", State: true);
				}
				if (!flag17 && intParameter3 <= 0 && ParentObject.IsPlayer())
				{
					Popup.ShowFail(FailureMessage3);
				}
				return false;
			}
			if (GameObject.Validate(ref Object3))
			{
				if (!flag17 && ParentObject.IsCombat)
				{
					DidXToY("unequip", Object3, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, ParentObject);
				}
				Object3.Physics.Equipped = null;
				if (GameObject.Validate(ref Object3) && !E.HasParameter("NoTake"))
				{
					eCommandFreeTakeObject.SetParameter("Object", Object3);
					eCommandFreeTakeObject.SetFlag("NoStack", flag16);
					eCommandFreeTakeObject.SetSilent(Silent: true);
					if (Object3 != null && Object3.HasTag("DestroyWhenUnequipped"))
					{
						Object3.Destroy();
					}
					else
					{
						if (ParentObject.FireEvent(eCommandFreeTakeObject))
						{
							return true;
						}
						if (ParentObject.CurrentCell != null)
						{
							ParentObject.CurrentCell.AddObject(Object3, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Silent: false, Repaint: true, FlushTransient: true, null, null, null, ParentObject);
						}
						else if (ParentObject.IsPlayer())
						{
							Popup.ShowFail("Error dropping object, removing to graveyard zone! (Inventory.cs:CommandEquipObject)");
						}
					}
				}
			}
		}
		else if (E.ID == "CommandTakeObject")
		{
			GameObject gameObjectParameter6 = E.GetGameObjectParameter("Object");
			if (gameObjectParameter6 == null)
			{
				return false;
			}
			int intParameter4 = E.GetIntParameter("EnergyCost", 1000);
			if (gameObjectParameter6.MovingIntoWouldCreateContainmentLoop(ParentObject))
			{
				MetricsManager.LogError(ParentObject.DebugName + " taking " + gameObjectParameter6.DebugName + " would create containment loop");
				return false;
			}
			if (intParameter4 > 0 && !ParentObject.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			if (gameObjectParameter6.IsInStasis())
			{
				return ParentObject.Fail("You cannot budge " + gameObjectParameter6.t() + ".");
			}
			bool flag19 = E.HasFlag("NoStack");
			GameObject inInventory2 = gameObjectParameter6.InInventory;
			if (inInventory2 != null)
			{
				if (!inInventory2.FireEvent(Event.New("BeforeContentsTaken", "Taker", ParentObject)))
				{
					return false;
				}
				if (inInventory2.IsOwned() && ParentObject.IsPlayer() && inInventory2.HasTagOrProperty("DontWarnOnOpen") && gameObjectParameter6.GetIntProperty("StoredByPlayer") <= 0 && gameObjectParameter6.GetIntProperty("FromStoredByPlayer") <= 0 && Popup.ShowYesNo("You don't own " + inInventory2.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, inInventory2.indicativeProximal) + ". Are you sure you want to take " + gameObjectParameter6.t() + "?") != DialogResult.Yes)
				{
					return false;
				}
				if (!inInventory2.FireEvent(Event.New("AfterContentsTaken", "Taker", ParentObject)))
				{
					return false;
				}
				AfterContentsTakenEvent.Send(ParentObject, inInventory2, gameObjectParameter6);
			}
			GameObject gameObjectParameter7 = E.GetGameObjectParameter("PutBy");
			string stringParameter9 = E.GetStringParameter("Context");
			Event obj10 = Event.New("BeginTake");
			obj10.SetParameter("Object", gameObjectParameter6);
			obj10.SetParameter("PutBy", gameObjectParameter7);
			obj10.SetParameter("Container", inInventory2);
			obj10.SetParameter("Context", stringParameter9);
			obj10.SetSilent(E.IsSilent());
			if (flag19)
			{
				obj10.SetFlag("NoStack", State: true);
			}
			if (!ParentObject.FireEvent(obj10, E))
			{
				return false;
			}
			Event obj11 = Event.New("BeginBeingTaken");
			obj11.SetParameter("TakingObject", ParentObject);
			obj11.SetParameter("PutBy", gameObjectParameter7);
			obj11.SetParameter("Container", inInventory2);
			obj11.SetParameter("Context", stringParameter9);
			obj11.SetSilent(E.IsSilent());
			if (flag19)
			{
				obj11.SetFlag("NoStack", State: true);
			}
			if (!gameObjectParameter6.FireEvent(obj11, E))
			{
				return false;
			}
			Event obj12 = Event.New("PerformTake");
			obj12.SetParameter("Object", gameObjectParameter6);
			obj12.SetParameter("PutBy", gameObjectParameter7);
			obj12.SetParameter("Container", inInventory2);
			obj12.SetParameter("Context", stringParameter9);
			obj12.SetSilent(E.IsSilent());
			if (flag19)
			{
				obj12.SetFlag("NoStack", State: true);
			}
			if (!ParentObject.FireEvent(obj12, E))
			{
				return false;
			}
			if (intParameter4 > 0)
			{
				ParentObject.UseEnergy(intParameter4, "Take", stringParameter9);
			}
		}
		return base.FireEvent(E);
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			gameObject.TurnTick(TimeTick, Amount);
			if (count != Objects.Count)
			{
				count = Objects.Count;
				if (i < count && Objects[i] != gameObject)
				{
					i--;
				}
			}
		}
		CheckOverburdened();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (GameObject @object in Objects)
		{
			stringBuilder.Append(@object.DisplayName);
			stringBuilder.Append("\n");
		}
		return stringBuilder.ToString();
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (base.WantEvent(ID, Cascade))
		{
			return true;
		}
		if (ID == AddedToInventoryEvent.ID)
		{
			return true;
		}
		if (ID == SingletonEvent<EndTurnEvent>.ID)
		{
			return true;
		}
		if (ID == GetCarriedWeightEvent.ID && Objects.Count > 0)
		{
			return true;
		}
		if (ID == GetExtrinsicWeightEvent.ID && Objects.Count > 0)
		{
			return true;
		}
		if (ID == GetExtrinsicValueEvent.ID && Objects.Count > 0)
		{
			return true;
		}
		if (ID == AfterObjectCreatedEvent.ID)
		{
			return true;
		}
		if (ID == BeforeDeathRemovalEvent.ID)
		{
			return true;
		}
		if (ID == PooledEvent<StripContentsEvent>.ID)
		{
			return true;
		}
		if (ID == PooledEvent<GetContentsEvent>.ID)
		{
			return true;
		}
		if (ID == PooledEvent<StatChangeEvent>.ID)
		{
			return true;
		}
		if (ID == PooledEvent<CarryingCapacityChangedEvent>.ID)
		{
			return true;
		}
		if (ID == InventoryActionEvent.ID)
		{
			return true;
		}
		if (ID == PooledEvent<HasBlueprintEvent>.ID)
		{
			return true;
		}
		if (ID == ZoneThawedEvent.ID)
		{
			return true;
		}
		if (ID == SingletonEvent<AfterGameLoadedEvent>.ID)
		{
			return true;
		}
		if (ID == SingletonEvent<FlushWeightCacheEvent>.ID && WeightCache != -1)
		{
			return true;
		}
		if (!MinEvent.CascadeTo(Cascade, 128))
		{
			GameObject parentObject = ParentObject;
			if (parentObject != null && parentObject.OnWorldMap())
			{
				return false;
			}
		}
		if (MinEvent.CascadeTo(Cascade, 2))
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Objects[i].WantEvent(ID, Cascade))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (!base.HandleEvent(E))
		{
			return false;
		}
		if (E.CascadeTo(2))
		{
			int num = -1;
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Objects[i].WantEvent(E.ID, E.GetCascadeLevel()))
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				List<GameObject> list = Event.NewGameObjectList();
				list.AddRange(Objects);
				int j = num;
				for (int count2 = list.Count; j < count2; j++)
				{
					if (!E.Dispatch(list[j]))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "CommandDropObject" || E.Command == "CommandDropAllObject")
		{
			bool flag = E.Command == "CommandDropAllObject";
			if (E.Actor == null || E.Item == null)
			{
				return false;
			}
			if (!E.Forced && !flag && E.Actor.IsPlayer() && E.Item.TryGetPart<Stacker>(out var Part) && Part != null && Part.Number > 1)
			{
				int? num = Popup.AskNumber("How many do you want to drop?", "Sounds/UI/ui_notification", "", Part.Number, 0, Part.Number);
				if (!num.HasValue || num == 0)
				{
					return false;
				}
				int number = Part.Number;
				try
				{
					number = num.Value;
				}
				catch
				{
					number = Part.Number;
				}
				if (number <= 0)
				{
					goto IL_032e;
				}
				if (number >= Part.Number)
				{
					number = Part.Number;
				}
				else
				{
					E.Item.SplitStack(number, ParentObject);
				}
			}
			if (E.Actor.HasRegisteredEvent("BeginDrop") && !E.Actor.FireEvent(Event.New("BeginDrop", "Object", E.Item)) && !E.Forced)
			{
				return false;
			}
			if (E.Item.HasRegisteredEvent("BeginBeingDropped") && !E.Item.FireEvent(Event.New("BeginBeingDropped", "TakingObject", E.Actor)) && !E.Forced)
			{
				return false;
			}
			Event obj2 = Event.New("PerformDrop");
			obj2.SetParameter("Object", E.Item);
			if (E.Forced)
			{
				obj2.SetFlag("Forced", State: true);
			}
			if (E.Silent)
			{
				obj2.SetSilent(Silent: true);
			}
			if (!E.Actor.FireEvent(obj2) && !E.Forced)
			{
				return false;
			}
			IInventory inventory = E.InventoryTarget ?? E.CellTarget ?? ParentObject.GetCurrentCell();
			if (inventory == null)
			{
				return false;
			}
			if (ParentObject.IsPlayerControlled())
			{
				if (E.Forced)
				{
					E.Item.RemoveIntProperty("DroppedByPlayer");
					E.Item.RequirePart<SpecialItem>().RemovePlayerTake = true;
				}
				else
				{
					E.Item.SetIntProperty("DroppedByPlayer", 1);
				}
			}
			inventory.AddObjectToInventory(E.Item, ParentObject, E.Silent, NoStack: false, FlushTransient: true, null, E);
		}
		else if (E.Command == "EmptyForDisassemble")
		{
			List<GameObject> list = Event.NewGameObjectList();
			list.AddRange(Objects);
			ParentObject.GetContext(out var ObjectContext, out var CellContext);
			ObjectContext?.ReceiveObject(list);
			if (CellContext != null)
			{
				foreach (GameObject item in list)
				{
					item.RemoveFromContext(E);
					CellContext.AddObject(item);
				}
			}
		}
		goto IL_032e;
		IL_032e:
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			gameObject.HandleEvent(E);
			if (count != Objects.Count)
			{
				count = Objects.Count;
				if (i < count && Objects[i] != gameObject)
				{
					i--;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(HasBlueprintEvent E)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == E.Blueprint)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			E.Value += Objects[i].Value;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory"))
			{
				E.Weight += Objects[i].GetWeight();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCarriedWeightEvent E)
	{
		E.Weight += GetWeight();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
		else
		{
			CheckNonEmptyState();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
		else
		{
			CheckNonEmptyState();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StripContentsEvent E)
	{
		List<GameObject> list = Event.NewGameObjectList();
		foreach (GameObject @object in Objects)
		{
			if ((!E.KeepNatural || !@object.IsNatural()) && (@object.Physics == null || @object.Physics.IsReal))
			{
				list.Add(@object);
			}
		}
		foreach (GameObject item in list)
		{
			item.Obliterate(null, E.Silent);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContentsEvent E)
	{
		E.Objects.AddRange(Objects);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (!ClearOnDeath)
		{
			return true;
		}
		if (DropOnDeath && Objects.Count > 0 && !ParentObject.IsTemporary && ParentObject.GetIntProperty("SuppressCorpseDrops") <= 0)
		{
			IInventory dropInventory = ParentObject.GetDropInventory();
			if (dropInventory != null)
			{
				List<GameObject> list = Event.NewGameObjectList();
				list.AddRange(Objects);
				Objects.Clear();
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					GameObject gameObject = list[i];
					gameObject.RemoveFromContext(E);
					if (gameObject.IsReal && DropOnDeathEvent.Check(gameObject, dropInventory))
					{
						dropInventory.AddObjectToInventory(gameObject, ParentObject, Silent: false, NoStack: false, FlushTransient: true, null, E);
						DroppedEvent.Send(ParentObject, gameObject);
					}
				}
			}
			else
			{
				Objects.Clear();
			}
			FlushWeightCache();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (E.Name == "Strength" && CheckOverburdenedOnStrengthUpdateEvent.Check(ParentObject))
		{
			ParentObject.FlushCarriedWeightCache();
			CheckOverburdened();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CarryingCapacityChangedEvent E)
	{
		ParentObject.FlushCarriedWeightCache();
		CheckOverburdened();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(FlushWeightCacheEvent E)
	{
		FlushWeightCache();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		VerifyContents();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterGameLoadedEvent E)
	{
		VerifyContents();
		return base.HandleEvent(E);
	}

	public int GetOccurrences(GameObject obj)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] == obj)
			{
				num++;
			}
		}
		return num;
	}

	public int CheckStacks()
	{
		int num = 0;
		int num2 = Objects.Count - 1;
		int num3 = 0;
		while (num2 >= 0 && num3 < 100)
		{
			if (Objects[num2].CheckStack())
			{
				num++;
				num2 = Objects.Count;
			}
			num2--;
			num3++;
		}
		return num;
	}
}
