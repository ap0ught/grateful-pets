using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConsoleLib.Console;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World;

[Serializable]
public class IPart : IComponent<GameObject>
{
	public class EventBinder : IEventBinder
	{
		public static readonly EventBinder Instance = new EventBinder();

		public override void WriteBind(SerializationWriter Writer, IEventHandler Handler, int ID)
		{
			Writer.WriteGameObject(((IPart)Handler)._ParentObject, Reference: true);
			Writer.WriteTokenized(Handler.GetType());
		}

		public override IEventHandler ReadBind(SerializationReader Reader, int ID)
		{
			GameObject gameObject = Reader.ReadGameObject();
			Type type = Reader.ReadTokenizedType();
			if (gameObject != null)
			{
				foreach (IPart parts in gameObject.PartsList)
				{
					if ((object)parts.GetType() == type)
					{
						return parts;
					}
				}
			}
			return null;
		}
	}

	/// <summary>Part is skipped by event cascades.</summary>
	public const int PRIORITY_SKIP = int.MinValue;

	public const int PRIORITY_VERY_LOW = 15000;

	public const int PRIORITY_LOW = 30000;

	public const int PRIORITY_MEDIUM = 45000;

	public const int PRIORITY_HIGH = 60000;

	public const int PRIORITY_VERY_HIGH = 75000;

	public const int PRIORITY_INTEGRAL = 90000;

	public const int PRIORITY_ADJUST_VERY_SMALL = 1;

	public const int PRIORITY_ADJUST_SMALL = 10;

	public const int PRIORITY_ADJUST_MEDIUM = 100;

	public const int PRIORITY_ADJUST_LARGE = 1000;

	public const int PRIORITY_ADJUST_VERY_LARGE = 10000;

	[NonSerialized]
	public GameObject _ParentObject;

	[NonSerialized]
	private string _Name;

	[NonSerialized]
	private static readonly Dictionary<IPart, Dictionary<FieldInfo, AACopyFinalEntry>> _FinalizeCopyAbilities = new Dictionary<IPart, Dictionary<FieldInfo, AACopyFinalEntry>>();

	[NonSerialized]
	private StatShifter _StatShifter;

	public sealed override IEventBinder Binder => EventBinder.Instance;

	public virtual GameObject ParentObject
	{
		get
		{
			return _ParentObject;
		}
		set
		{
			if (_StatShifter != null)
			{
				_StatShifter.Owner = value;
			}
			_ParentObject = value;
		}
	}

	/// <summary>The priority of this part within the parent object's internals, which affects serialization and event cascade order.</summary>
	public virtual int Priority => 45000;

	public string Name => _Name ?? (_Name = ModManager.ResolveTypeName(GetType()));

	public override bool IsValid
	{
		get
		{
			if (_ParentObject != null)
			{
				return _ParentObject.IsValid();
			}
			return false;
		}
	}

	/// <summary>The static pool instances of this part should be returned to.</summary>
	public virtual IPartPool Pool => null;

	private Dictionary<FieldInfo, AACopyFinalEntry> FinalizeCopyAbilities
	{
		get
		{
			if (_FinalizeCopyAbilities.TryGetValue(this, out var value))
			{
				return value;
			}
			value = new Dictionary<FieldInfo, AACopyFinalEntry>();
			_FinalizeCopyAbilities.Add(this, value);
			return value;
		}
	}

	public StatShifter StatShifter
	{
		get
		{
			if (_StatShifter == null)
			{
				_StatShifter = new StatShifter(ParentObject, (string)null);
			}
			return _StatShifter;
		}
	}

	public sealed override void RegisterEvent(int EventID, int Order = 0, bool Serialize = false)
	{
		ParentObject.RegisterEvent(this, EventID, Order, Serialize);
	}

	public sealed override void UnregisterEvent(int EventID)
	{
		ParentObject.UnregisterEvent(this, EventID);
	}

	public override GameObject GetComponentBasis()
	{
		return _ParentObject;
	}

	public virtual void UpdateImposter(QudScreenBufferExtra extra)
	{
	}

	public virtual void OnPaint(ScreenBuffer buffer)
	{
	}

	public virtual bool AllowStaticRegistration()
	{
		return false;
	}

	public virtual bool CanGenerateStacked()
	{
		return SameAs(this);
	}

	/// <summary>
	/// Generally don't use this. Used by the UI For certain naughty things.
	/// </summary>
	/// <param name="newName" />
	public void SetName(string newName)
	{
		_Name = newName;
	}

	/// <summary>Reset the fields of this part to a cleared state.</summary>
	/// <remarks>Primarily used for pooled parts.</remarks>
	public virtual void Reset()
	{
		_ParentObject = null;
		_Name = null;
	}

	[Obsolete("Use Register(GameObject, IEventRegistrar)")]
	public virtual void Register(GameObject Object)
	{
	}

	/// <summary>Register to events from the <see cref="T:XRL.World.GameObject" />.</summary>
	/// <param name="Object">The current game object.</param>
	/// <param name="Registrar">An event registrar with this <see cref="T:XRL.World.IPart" /> and <see cref="T:XRL.World.GameObject" /> provisioned as defaults.</param>
	public virtual void Register(GameObject Object, IEventRegistrar Registrar)
	{
	}

	/// <summary>Register to events from the <see cref="T:XRL.World.GameObject" /> while it is active in the action queue.</summary>
	/// <remarks>It is safer to register for external events here, since they're guaranteed to be cleaned up once the object goes out of scope.</remarks>
	/// <param name="Object">The current game object.</param>
	/// <param name="Registrar">An event registrar with this <see cref="T:XRL.World.IPart" /> and <see cref="T:XRL.World.GameObject" /> provisioned as defaults.</param>
	public virtual void RegisterActive(GameObject Object, IEventRegistrar Registrar)
	{
	}

	public virtual void ApplyRegistrar(GameObject Object, bool Active = false)
	{
		if (Active)
		{
			RegisterActive(Object, EventRegistrar.Get(Object, this));
			return;
		}
		Register(Object);
		Register(Object, EventRegistrar.Get(Object, this));
		if (ComponentReflection.PartEvents.TryGetValue(GetType(), out var value))
		{
			int i = 0;
			for (int num = value.Length; i < num; i++)
			{
				Object.RegisterEvent(this, value[i]);
			}
		}
		if (WantEvent(SingletonEvent<EndTurnEvent>.ID, EndTurnEvent.CascadeLevel))
		{
			Object.RegisterEvent(this, SingletonEvent<EndTurnEvent>.ID);
		}
	}

	public virtual void ApplyUnregistrar(GameObject Object, bool Active = false)
	{
		EventUnregistrar registrar = EventUnregistrar.Get(Object, this);
		if (!Active)
		{
			Register(Object, registrar);
			if (ComponentReflection.PartEvents.TryGetValue(GetType(), out var value))
			{
				int i = 0;
				for (int num = value.Length; i < num; i++)
				{
					Object.UnregisterEvent(this, value[i]);
				}
			}
			if (WantEvent(SingletonEvent<EndTurnEvent>.ID, EndTurnEvent.CascadeLevel))
			{
				Object.UnregisterEvent(this, SingletonEvent<EndTurnEvent>.ID);
			}
		}
		RegisterActive(Object, registrar);
	}

	public virtual void Attach()
	{
	}

	public virtual void Initialize()
	{
	}

	public virtual void AddedAfterCreation()
	{
	}

	public virtual void Remove()
	{
	}

	public virtual void ObjectLoaded()
	{
	}

	public virtual string[] GetStaticEvents()
	{
		return null;
	}

	public void RegisterStaticEvents(GameObject GO)
	{
		string[] staticEvents = GetStaticEvents();
		if (staticEvents != null)
		{
			int i = 0;
			for (int num = staticEvents.Length; i < num; i++)
			{
				GO.RegisterPartEvent(this, staticEvents[i]);
			}
		}
	}

	public bool IsStaticEvent(string ID)
	{
		string[] staticEvents = GetStaticEvents();
		if (staticEvents == null)
		{
			return false;
		}
		int i = 0;
		for (int num = staticEvents.Length; i < num; i++)
		{
			if (staticEvents[i] == ID)
			{
				return true;
			}
		}
		return false;
	}

	[Obsolete("Use static LoadBlueprint(GameObjectBlueprint)")]
	public virtual void LoadBlueprint()
	{
	}

	[Obsolete("Use static LoadBlueprint(GameObjectBlueprint)")]
	public virtual void GameStarted()
	{
	}

	public virtual bool RenderTile(ConsoleChar E)
	{
		return true;
	}

	public static void Save(IPart Part, SerializationWriter Writer)
	{
		SerializationWriter.Block block = Writer.StartBlock();
		Type type = null;
		try
		{
			type = Part.GetType();
			Writer.WriteTokenized(type);
			StatShifter.Save(Part._StatShifter, Writer);
			Part.SaveData(Writer);
		}
		catch (Exception ex)
		{
			block.Reset();
			MetricsManager.LogAssemblyError(type, "Skipping failed serialization of part '" + type?.FullName + "': " + ex);
		}
		finally
		{
			block.Dispose();
		}
	}

	public static IPart Load(GameObject Basis, SerializationReader Reader)
	{
		Reader.StartBlock(out var Position, out var Length);
		if (Length == 0)
		{
			return null;
		}
		Type type = null;
		IPart part = null;
		try
		{
			type = Reader.ReadTokenizedType();
			part = GamePartBlueprint.PartReflectionCache.Get(type).GetInstance() ?? ((IPart)Activator.CreateInstance(type));
			part._ParentObject = Basis;
			part._StatShifter = StatShifter.Load(Reader, part._ParentObject);
			part.LoadData(Reader);
		}
		catch (Exception exception)
		{
			if (part == null || !part.ReadError(exception, Reader, Position, Length))
			{
				Reader.SkipBlock(exception, type, Position, Length);
			}
		}
		return part;
	}

	[Obsolete("Override Write(GameObject, SerializationWriter")]
	public virtual void SaveData(SerializationWriter Writer)
	{
		Write(_ParentObject, Writer);
	}

	[Obsolete("Override Read(GameObject, SerializationReader")]
	public virtual void LoadData(SerializationReader Reader)
	{
		Read(_ParentObject, Reader);
	}

	[Obsolete("Override FinalizeRead(SerializationReader).")]
	public virtual void FinalizeLoad()
	{
	}

	public virtual void FinalizeRead(SerializationReader Reader)
	{
		FinalizeLoad();
	}

	public virtual IPart DeepCopy(GameObject Parent)
	{
		IPart part = (IPart)Activator.CreateInstance(GetType());
		Dictionary<string, string> source = new Dictionary<string, string>();
		Dictionary<string, string> dest = new Dictionary<string, string>();
		source.ToList().ForEach((KeyValuePair<string, string> kv) =>
		{
			dest.Add(kv.Key, kv.Value);
		});
		FieldInfo[] fields = GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) != 0 || fieldInfo.IsLiteral)
			{
				continue;
			}
			if (fieldInfo.FieldType.FullName.Contains("ActivatedAbilityEntry"))
			{
				if (fieldInfo.GetValue(this) != null)
				{
					part.FinalizeCopyAbilities.Add(fieldInfo, new AACopyFinalEntry(((ActivatedAbilityEntry)fieldInfo.GetValue(this)).ID, part));
				}
			}
			else
			{
				fieldInfo.SetValue(part, fieldInfo.GetValue(this));
			}
		}
		part.ParentObject = Parent;
		if (HasStatShifts())
		{
			part._StatShifter = new StatShifter(Parent, _StatShifter);
		}
		return part;
	}

	public virtual IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		return DeepCopy(Parent);
	}

	public virtual void FinalizeCopyEarly(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		if (FinalizeCopyAbilities == null)
		{
			return;
		}
		ActivatedAbilities part = ParentObject.GetPart<ActivatedAbilities>();
		if (part == null)
		{
			return;
		}
		foreach (FieldInfo key in FinalizeCopyAbilities.Keys)
		{
			key.SetValue(FinalizeCopyAbilities[key].Part, part.AbilityByGuid[FinalizeCopyAbilities[key].ID]);
		}
		_FinalizeCopyAbilities.Remove(this);
	}

	public virtual void FinalizeCopy(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
	}

	public virtual void FinalizeCopyLate(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
	}

	public override string ToString()
	{
		return Name;
	}

	public virtual bool SameAs(IPart p)
	{
		return Name == p.Name;
	}

	public void ForceDropSelf()
	{
		if (ParentObject.CurrentCell != null)
		{
			return;
		}
		if (ParentObject.Equipped != null)
		{
			BodyPart bodyPart = ParentObject.Equipped.Body?.FindEquippedItem(ParentObject);
			if (bodyPart != null)
			{
				ParentObject.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", bodyPart));
			}
		}
		GameObject inInventory = ParentObject.InInventory;
		if (inInventory != null)
		{
			InventoryActionEvent.Check(inInventory, inInventory, ParentObject, "CommandDropObject", Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, Forced: true);
		}
	}

	public bool ShouldUsePsychometry(GameObject who)
	{
		if (who.TryGetPart<Psychometry>(out var Part))
		{
			return Part.Advisable(ParentObject);
		}
		return false;
	}

	public bool ShouldUsePsychometry()
	{
		return ShouldUsePsychometry(IComponent<GameObject>.ThePlayer);
	}

	public bool UsePsychometry(GameObject Actor, GameObject Subject)
	{
		return (Actor?.GetPart<Psychometry>())?.Activate(Subject) ?? false;
	}

	public bool HasStatShifts()
	{
		if (_StatShifter != null)
		{
			return _StatShifter.HasStatShifts();
		}
		return false;
	}

	public void CombatJuiceWait(float t)
	{
		ParentObject?.CombatJuiceWait(t);
	}
}
