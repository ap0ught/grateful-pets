using System;
using System.Collections.Generic;
using System.Reflection;
using ConsoleLib.Console;
using XRL.Wish;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
[HasWishCommand]
public class Effect : IComponent<GameObject>
{
	public class EventBinder : IEventBinder
	{
		public static readonly EventBinder Instance = new EventBinder();

		public override void WriteBind(SerializationWriter Writer, IEventHandler Handler, int ID)
		{
			Writer.WriteGameObject(((Effect)Handler)._Object, Reference: true);
			Writer.WriteTokenized(Handler.GetType());
		}

		public override IEventHandler ReadBind(SerializationReader Reader, int ID)
		{
			GameObject gameObject = Reader.ReadGameObject();
			Type type = Reader.ReadTokenizedType();
			if (gameObject?._Effects != null)
			{
				foreach (Effect effect in gameObject._Effects)
				{
					if ((object)effect.GetType() == type)
					{
						return effect;
					}
				}
			}
			return null;
		}
	}

	public static readonly int ICON_COLOR_PRIORITY = 1000;

	public const int TYPE_GENERAL = 1;

	public const int TYPE_MENTAL = 2;

	public const int TYPE_METABOLIC = 4;

	public const int TYPE_RESPIRATORY = 8;

	public const int TYPE_CIRCULATORY = 16;

	public const int TYPE_CONTACT = 32;

	public const int TYPE_FIELD = 64;

	public const int TYPE_ACTIVITY = 128;

	public const int TYPE_DIMENSIONAL = 256;

	public const int TYPE_CHEMICAL = 512;

	public const int TYPE_STRUCTURAL = 1024;

	public const int TYPE_SONIC = 2048;

	public const int TYPE_TEMPORAL = 4096;

	public const int TYPE_NEUROLOGICAL = 8192;

	public const int TYPE_DISEASE = 16384;

	public const int TYPE_PSIONIC = 32768;

	public const int TYPE_POISON = 65536;

	public const int TYPE_EQUIPMENT = 131072;

	public const int TYPE_MINOR = 16777216;

	public const int TYPE_NEGATIVE = 33554432;

	public const int TYPE_REMOVABLE = 67108864;

	public const int TYPE_VOLUNTARY = 134217728;

	public const int TYPES_MECHANISM = 16777215;

	public const int TYPES_CLASS = 251658240;

	public const int DURATION_INDEFINITE = 9999;

	public static List<string> NegativeEffects;

	[NonSerialized]
	public GameObject _Object;

	[NonSerialized]
	public StatShifter _StatShifter;

	[NonSerialized]
	public Guid ID;

	[NonSerialized]
	public string DisplayName;

	[NonSerialized]
	public int Duration;

	/// <remarks>Type.Name allocates garbage, cache it here.</remarks>
	[NonSerialized]
	private string _ClassName;

	public sealed override IEventBinder Binder => EventBinder.Instance;

	public virtual string ApplySound
	{
		get
		{
			int effectType = GetEffectType();
			if (effectType.HasBit(33554432))
			{
				if (effectType.HasBit(32768))
				{
					return "sfx_statusEffect_spacetimeWeirdness";
				}
				if (effectType.HasBit(2))
				{
					return "sfx_statusEffect_mentalImpairment";
				}
			}
			return null;
		}
	}

	public virtual string RemoveSound
	{
		get
		{
			if (Object != null && Object.IsPlayer() && !IsOfTypes(16777216))
			{
				if (IsOfTypes(33554432))
				{
					return "sfx_statusEffect_negativeStatusEffect_stop";
				}
				return "sfx_statusEffect_stop";
			}
			return null;
		}
	}

	public string ClassName => _ClassName ?? (_ClassName = GetType().Name);

	public string DisplayNameStripped => DisplayName.Strip();

	public override bool IsValid
	{
		get
		{
			if (_Object != null)
			{
				return _Object.IsValid();
			}
			return false;
		}
	}

	public GameObject Object
	{
		get
		{
			return _Object;
		}
		set
		{
			_Object = value;
			if (_StatShifter != null)
			{
				_StatShifter.Owner = _Object;
			}
		}
	}

	public StatShifter StatShifter
	{
		get
		{
			if (_StatShifter == null)
			{
				_StatShifter = new StatShifter(Object, GetDescription());
			}
			return _StatShifter;
		}
	}

	public sealed override void RegisterEvent(int EventID, int Order = 0, bool Serialize = false)
	{
		Object.RegisterEvent(this, EventID, Order, Serialize);
	}

	public sealed override void UnregisterEvent(int EventID)
	{
		Object.UnregisterEvent(this, EventID);
	}

	public Effect()
	{
		ID = Guid.NewGuid();
		DisplayName = "";
	}

	public override GameObject GetComponentBasis()
	{
		return Object;
	}

	public bool HasStatShifts()
	{
		if (_StatShifter != null)
		{
			return _StatShifter.HasStatShifts();
		}
		return false;
	}

	public virtual bool allowCopyOnNoEffectDeepCopy()
	{
		return false;
	}

	public virtual int GetEffectType()
	{
		return 1;
	}

	public bool IsOfType(int Mask)
	{
		return GetEffectType().HasBit(Mask);
	}

	public bool IsOfTypes(int Mask)
	{
		return GetEffectType().HasAllBits(Mask);
	}

	public virtual bool IsTonic()
	{
		return false;
	}

	public virtual string GetDescription()
	{
		if (DisplayName.Contains("<"))
		{
			return null;
		}
		return DisplayName;
	}

	public virtual string GetStateDescription()
	{
		return GetDescription() ?? DisplayName;
	}

	public virtual bool SuppressInLookDisplay()
	{
		return false;
	}

	public virtual bool SuppressInStageDisplay()
	{
		return false;
	}

	public static bool CanEffectTypeBeAppliedTo(int Type, GameObject Object)
	{
		if (Type.HasBit(2) && Object.Brain == null)
		{
			return false;
		}
		if ((Type.HasBit(4) || Type.HasBit(8)) && !Object.HasPart<Stomach>())
		{
			return false;
		}
		if (Type.HasBit(16) && Object.GetIntProperty("Bleeds") <= 0)
		{
			return false;
		}
		if (Type.HasBit(128) && !Object.HasPart<Body>())
		{
			return false;
		}
		switch (Object.GetMatterPhase())
		{
		case 1:
			if (!CanEffectTypeBeAppliedToSolid(Type))
			{
				return false;
			}
			break;
		case 2:
			if (!CanEffectTypeBeAppliedToLiquid(Type))
			{
				return false;
			}
			break;
		case 3:
			if (!CanEffectTypeBeAppliedToGas(Type))
			{
				return false;
			}
			break;
		case 4:
			if (!CanEffectTypeBeAppliedToPlasma(Type))
			{
				return false;
			}
			break;
		}
		return true;
	}

	public bool CanBeAppliedTo(GameObject Object)
	{
		return CanEffectTypeBeAppliedTo(GetEffectType(), Object);
	}

	public static bool CanEffectTypeBeAppliedToSolid(int Type, GameObject Object = null)
	{
		return true;
	}

	public static bool CanEffectTypeBeAppliedToLiquid(int Type, GameObject Object = null)
	{
		if (Type.HasBit(32))
		{
			return false;
		}
		if (Type.HasBit(1024))
		{
			return false;
		}
		return true;
	}

	public static bool CanEffectTypeBeAppliedToGas(int Type, GameObject Object = null)
	{
		if (Type.HasBit(32))
		{
			return false;
		}
		if (Type.HasBit(4))
		{
			return false;
		}
		if (Type.HasBit(1024))
		{
			return false;
		}
		return true;
	}

	public static bool CanEffectTypeBeAppliedToPlasma(int Type, GameObject Object = null)
	{
		if (Type.HasBit(32))
		{
			return false;
		}
		if (Type.HasBit(4))
		{
			return false;
		}
		if (Type.HasBit(512))
		{
			return false;
		}
		if (Type.HasBit(1024))
		{
			return false;
		}
		if (Type.HasBit(2048))
		{
			return false;
		}
		return true;
	}

	public virtual bool SameAs(Effect FX)
	{
		if (FX.ClassName != ClassName)
		{
			return false;
		}
		if (FX.DisplayName != DisplayName)
		{
			return false;
		}
		if (FX.Duration != Duration)
		{
			return false;
		}
		return true;
	}

	public virtual string GetDetails()
	{
		return "[effect details]";
	}

	public virtual void OnPaint(ScreenBuffer Buffer)
	{
	}

	public virtual bool RenderTile(ConsoleChar E)
	{
		return true;
	}

	public virtual Effect DeepCopy(GameObject Parent)
	{
		Effect effect = (Effect)Activator.CreateInstance(GetType());
		FieldInfo[] fields = GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0 && !fieldInfo.IsLiteral)
			{
				fieldInfo.SetValue(effect, fieldInfo.GetValue(this));
			}
		}
		effect.Object = Parent;
		if (_StatShifter != null)
		{
			effect._StatShifter = new StatShifter(Parent, _StatShifter);
		}
		return effect;
	}

	public virtual Effect DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		return DeepCopy(Parent);
	}

	public static void Save(Effect Effect, SerializationWriter Writer)
	{
		SerializationWriter.Block block = Writer.StartBlock();
		Type type = null;
		try
		{
			type = Effect.GetType();
			Writer.WriteTokenized(type);
			Writer.Write(Effect.ID);
			Writer.WriteOptimized(Effect.DisplayName);
			Writer.WriteOptimized(Effect.Duration);
			StatShifter.Save(Effect._StatShifter, Writer);
			Effect.SaveData(Writer);
		}
		catch (Exception ex)
		{
			block.Reset();
			MetricsManager.LogAssemblyError(type, "Skipping failed serializion of effect '" + type?.FullName + "': " + ex);
		}
		finally
		{
			block.Dispose();
		}
	}

	public static Effect Load(GameObject Basis, SerializationReader Reader)
	{
		Reader.StartBlock(out var Position, out var Length);
		if (Length == 0)
		{
			return null;
		}
		Type type = null;
		Effect effect = null;
		try
		{
			type = Reader.ReadTokenizedType();
			effect = (Effect)Activator.CreateInstance(type);
			effect._Object = Basis;
			effect.ID = Reader.ReadGuid();
			effect.DisplayName = Reader.ReadOptimizedString();
			effect.Duration = Reader.ReadOptimizedInt32();
			effect._StatShifter = StatShifter.Load(Reader, effect._Object);
			effect.LoadData(Reader);
		}
		catch (Exception exception)
		{
			if (effect == null || !effect.ReadError(exception, Reader, Position, Length))
			{
				Reader.SkipBlock(exception, type, Position, Length);
			}
		}
		return effect;
	}

	[Obsolete("Override Write(GameObject, SerializationWriter")]
	public virtual void SaveData(SerializationWriter Writer)
	{
		Write(_Object, Writer);
	}

	[Obsolete("Override Read(GameObject, SerializationReader")]
	public virtual void LoadData(SerializationReader Reader)
	{
		Read(_Object, Reader);
	}

	[Obsolete("Override FinalizeRead(SerializationReader).")]
	public virtual void FinalizeLoad()
	{
	}

	public virtual void FinalizeRead(SerializationReader Reader)
	{
		FinalizeLoad();
	}

	public virtual bool Apply(GameObject Object)
	{
		return true;
	}

	public virtual void Remove(GameObject Object)
	{
	}

	public virtual void Applied(GameObject Object)
	{
	}

	public virtual bool CanApplyToStack()
	{
		return false;
	}

	public virtual void WasUnstackedFrom(GameObject obj)
	{
	}

	[Obsolete("Use Register(GameObject, IEventRegistrar)")]
	public virtual void Register(GameObject Object)
	{
	}

	[Obsolete("Use Register(GameObject, IEventRegistrar)")]
	public virtual void Unregister(GameObject Object)
	{
	}

	/// <summary>Register to events from the <see cref="T:XRL.World.GameObject" />.</summary>
	/// <param name="Object">The current game object.</param>
	/// <param name="Registrar">An event registrar with this <see cref="T:XRL.World.Effect" /> and <see cref="T:XRL.World.GameObject" /> provisioned as defaults.</param>
	public virtual void Register(GameObject Object, IEventRegistrar Registrar)
	{
	}

	/// <summary>Register to events from the <see cref="T:XRL.World.GameObject" /> while it is active in the action queue.</summary>
	/// <remarks>It is safer to register for external events here, since they're guaranteed to be cleaned up once the object goes out of scope.</remarks>
	/// <param name="Object">The current game object.</param>
	/// <param name="Registrar">An event registrar with this <see cref="T:XRL.World.Effect" /> and <see cref="T:XRL.World.GameObject" /> provisioned as defaults.</param>
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
		if (ComponentReflection.EffectEvents.TryGetValue(GetType(), out var value))
		{
			Object.CurrentCell?.FlushRenderCache();
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
			Unregister(Object);
			Register(Object, registrar);
			if (ComponentReflection.EffectEvents.TryGetValue(GetType(), out var value))
			{
				Object.CurrentCell?.FlushRenderCache();
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

	public virtual void Expired()
	{
	}

	public virtual bool UseStandardDurationCountdown()
	{
		return false;
	}

	public virtual bool UseThawEventToUpdateDuration()
	{
		return false;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && (ID != SingletonEvent<BeforeBeginTakeActionEvent>.ID || !UseStandardDurationCountdown() || Object?.Brain == null) && (ID != SingletonEvent<EndTurnEvent>.ID || !UseStandardDurationCountdown() || Object?.Brain != null))
		{
			if (ID == ZoneThawedEvent.ID)
			{
				return UseThawEventToUpdateDuration();
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		if (UseThawEventToUpdateDuration() && E.TicksFrozen > 0)
		{
			if (E.TicksFrozen > Duration)
			{
				Duration = 0;
			}
			else
			{
				Duration -= (int)E.TicksFrozen;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		if (UseStandardDurationCountdown() && Object?.Brain != null && Duration > 0 && Duration != 9999)
		{
			Duration--;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (UseStandardDurationCountdown() && Object?.Brain == null && Duration > 0 && Duration != 9999)
		{
			Duration--;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Duration", Duration);
		return base.HandleEvent(E);
	}
}
