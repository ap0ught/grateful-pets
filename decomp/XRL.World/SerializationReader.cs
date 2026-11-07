using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Genkit;
using UnityEngine;
using XRL.Collections;
using XRL.Serialization;

namespace XRL.World;

/// <summary>
/// A SerializationReader instance is used to read stored values and objects from a byte array.
///
/// Once an instance is created, use the various methods to read the required data.
/// The data read MUST be exactly the same type and in the same order as it was written.
/// </summary>
public sealed class SerializationReader : BinaryReader
{
	private static readonly BitArray FullyOptimizableTypedArray = new BitArray(0);

	internal const ushort GameObjectStartValue = 64206;

	internal const ushort GameObjectEndValue = 44203;

	internal const ushort GameObjectFinishValue = 52958;

	private static SerializationReader Instance = new SerializationReader(FastSerialization.SharedCache);

	public MemoryStream Stream;

	public FastSerialization.Cache Cache;

	public int FileVersion;

	public Dictionary<string, Version> ModVersions;

	internal ulong ExtensionHash;

	public int Errors;

	private string[] TokenStrings = Array.Empty<string>();

	private object[] TokenObjects = Array.Empty<object>();

	private ITokenized[] Tokenized = Array.Empty<ITokenized>();

	private Type[] TokenTypes = Array.Empty<Type>();

	private string[] TokenTypeNames = Array.Empty<string>();

	private GameObject[] TokenGameObjects = Array.Empty<GameObject>();

	private GameObjectReference[] GameObjectReferences = Array.Empty<GameObjectReference>();

	private EventRegistry[] EventRegistries = Array.Empty<EventRegistry>();

	private List<GameObject> InvalidGameObjects;

	private long StringPosition;

	private long ObjectPosition;

	private long TypePosition;

	private long GameObjectPosition;

	private long GameObjectReferencePosition;

	private long EventRegistryPosition;

	private long TokenizedPosition;

	private int GameObjectLength;

	private int EventRegistryLength;

	private int TokenizedLength;

	private bool SerializePlayer;

	public static Guid ImmutableObject = new Guid("00000000-0000-0000-0000-000000000002");

	public static Guid PlayerGuid = new Guid("00000000-0000-0000-0000-000000000001");

	public Dictionary<GameObject, Cell> Locations = new Dictionary<GameObject, Cell>();

	private object[] CapacityArgs = new object[1];

	private static BinaryFormatter formatter = null;

	public static ResolveEventHandler modAssemblyResolveHandler = (object sender, ResolveEventArgs args) =>
	{
		AssemblyName assemblyName = new AssemblyName(args.Name);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assemblyName.Name);
		foreach (ModInfo mod in ModManager.Mods)
		{
			if (assemblyName.Name == mod.Assembly?.GetName()?.Name)
			{
				return mod.Assembly;
			}
			if (fileNameWithoutExtension == mod.ID && !mod.IsEnabled)
			{
				MetricsManager.LogModError(mod, "Required mod " + mod.DisplayTitleStripped + " has status: " + Enum.GetName(typeof(ModState), mod.State));
				return (Assembly)null;
			}
		}
		MetricsManager.LogError("Unable to resolve mod assembly: " + assemblyName.Name);
		return (Assembly)null;
	};

	public static ResolveEventHandler assemblyResolveHandler = (object a, ResolveEventArgs b) => Assembly.GetExecutingAssembly();

	/// <summary>
	/// Returns the number of bytes or serialized remaining to be processed.
	/// Useful for checking that deserialization is complete.
	///
	/// Warning: Retrieving the Position in certain stream types can be expensive,
	/// e.g. a FileStream, so use sparingly unless known to be a MemoryStream.
	/// </summary>
	public long BytesRemaining => Stream.Length - Stream.Position;

	public static bool TryGetShared(out SerializationReader Reader)
	{
		GameManager.IsOnGameContext();
		if (FastSerialization.SharedCache.Reserved)
		{
			Reader = null;
			return false;
		}
		Reader = Instance;
		return FastSerialization.SharedCache.Reserved = true;
	}

	public static void ReleaseShared()
	{
		Instance.Clear();
		FastSerialization.SharedCache.Reserved = false;
	}

	public static SerializationReader Get()
	{
		if (TryGetShared(out var Reader))
		{
			return Reader;
		}
		return new SerializationReader(new FastSerialization.Cache(0));
	}

	public static void Release(SerializationReader Reader)
	{
		if (Reader == Instance)
		{
			ReleaseShared();
		}
		else
		{
			Reader.Dispose();
		}
	}

	/// <summary>
	/// Creates a SerializationReader based on the passed Stream.
	/// </summary>
	/// <param name="Stream">The stream containing the serialized data</param>
	public SerializationReader(FastSerialization.Cache Cache)
		: base(Cache.MemoryStream, FastSerialization.Encoding, leaveOpen: true)
	{
		Stream = Cache.MemoryStream;
		this.Cache = Cache;
	}

	public void Start(bool SerializePlayer = false)
	{
		this.SerializePlayer = SerializePlayer;
		ReadHeader();
		MemoryStream stream = Stream;
		int eventRegistryLength = EventRegistryLength;
		EventRegistries = Cache.EventRegistries.GetArray(eventRegistryLength);
		for (int i = 0; i < eventRegistryLength; i++)
		{
			EventRegistries[i] = EventRegistry.Get();
		}
		eventRegistryLength = GameObjectLength;
		TokenGameObjects = Cache.GameObjects.GetArray(eventRegistryLength);
		for (int j = 0; j < eventRegistryLength; j++)
		{
			TokenGameObjects[j] = GameObject.Get();
		}
		stream.Position = StringPosition;
		eventRegistryLength = ReadOptimizedInt32();
		TokenStrings = Cache.Strings.GetArray(eventRegistryLength);
		for (int k = 0; k < eventRegistryLength; k++)
		{
			TokenStrings[k] = base.ReadString();
		}
		stream.Position = 68L;
		ReadModVersions();
		ReadExtensionHash();
		long position = stream.Position;
		stream.Position = TypePosition;
		eventRegistryLength = ReadOptimizedInt32();
		TokenTypes = Cache.Types.GetArray(eventRegistryLength);
		TokenTypeNames = Cache.Names.GetArray(eventRegistryLength);
		for (int l = 0; l < eventRegistryLength; l++)
		{
			string text = "";
			try
			{
				text = (TokenTypeNames[l] = base.ReadString());
				TokenTypes[l] = ModManager.ResolveType(text, IgnoreCase: false, ThrowOnError: true);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("SerializationReader.TokenTypes:" + text, x);
			}
		}
		stream.Position = GameObjectReferencePosition;
		eventRegistryLength = ReadOptimizedInt32();
		GameObjectReferences = Cache.GameObjectReferences.GetArray(eventRegistryLength);
		for (int m = 0; m < eventRegistryLength; m++)
		{
			GameObjectReferences[m] = new GameObjectReference();
			GameObjectReferences[m].Read(this);
		}
		stream.Position = ObjectPosition;
		eventRegistryLength = ReadOptimizedInt32();
		TokenObjects = Cache.Objects.GetArray(eventRegistryLength);
		for (int n = 0; n < eventRegistryLength; n++)
		{
			try
			{
				TokenObjects[n] = ReadObject();
			}
			catch (Exception x2)
			{
				MetricsManager.LogException("SerializationReader.TokenObjects", x2);
			}
		}
		stream.Position = TokenizedPosition;
		eventRegistryLength = ReadOptimizedInt32();
		Tokenized = Cache.Tokenized.GetArray(eventRegistryLength);
		for (int num = 0; num < eventRegistryLength; num++)
		{
			Tokenized[num] = (ITokenized)ReadComposite();
		}
		stream.Position = position;
	}

	public void Clear()
	{
		Stream.SetLength(0L);
		ModVersions?.Clear();
		InvalidGameObjects?.Clear();
		Locations?.Clear();
		Array.Clear(TokenStrings, 0, TokenStrings.Length);
		Array.Clear(TokenObjects, 0, TokenObjects.Length);
		Array.Clear(Tokenized, 0, Tokenized.Length);
		Array.Clear(TokenTypes, 0, TokenTypes.Length);
		Array.Clear(TokenTypeNames, 0, TokenTypeNames.Length);
		Array.Clear(TokenGameObjects, 0, TokenGameObjects.Length);
		Array.Clear(GameObjectReferences, 0, GameObjectReferences.Length);
		Array.Clear(EventRegistries, 0, EventRegistries.Length);
		Errors = 0;
		formatter = null;
	}

	protected override void Dispose(bool disposing)
	{
		formatter = null;
		base.Dispose(disposing);
	}

	public void ReadHeader()
	{
		FileVersion = ReadInt32();
		GameObjectPosition = ReadInt64();
		GameObjectReferencePosition = ReadInt64();
		EventRegistryPosition = ReadInt64();
		TokenizedPosition = ReadInt64();
		ObjectPosition = ReadInt64();
		TypePosition = ReadInt64();
		StringPosition = ReadInt64();
		GameObjectLength = ReadInt32();
		EventRegistryLength = ReadInt32();
	}

	/// <summary>
	/// Returns an ArrayList or null from the stream.
	/// </summary>
	/// <returns>An ArrayList instance.</returns>
	public ArrayList ReadArrayList()
	{
		if (readTypeCode() == SerializedType.NullType)
		{
			return null;
		}
		return new ArrayList(ReadOptimizedObjectArray());
	}

	/// <summary>
	/// Returns a BitArray or null from the stream.
	/// </summary>
	/// <returns>A BitArray instance.</returns>
	public BitArray ReadBitArray()
	{
		if (readTypeCode() == SerializedType.NullType)
		{
			return null;
		}
		return ReadOptimizedBitArray();
	}

	/// <summary>
	/// Returns a BitVector32 value from the stream.
	/// </summary>
	/// <returns>A BitVector32 value.</returns>
	public BitVector32 ReadBitVector32()
	{
		return new BitVector32(ReadInt32());
	}

	/// <summary>
	/// Reads the specified number of bytes directly from the stream.
	/// </summary>
	/// <param name="count">The number of bytes to read</param>
	/// <returns>A byte[] containing the read bytes</returns>
	public byte[] ReadBytesDirect(int count)
	{
		return base.ReadBytes(count);
	}

	/// <summary>
	/// Returns a DateTime value from the stream.
	/// </summary>
	/// <returns>A DateTime value.</returns>
	public DateTime ReadDateTime()
	{
		return DateTime.FromBinary(ReadInt64());
	}

	/// <summary>
	/// Returns a Guid value from the stream.
	/// </summary>
	/// <returns>A DateTime value.</returns>
	public Guid ReadGuid()
	{
		return new Guid(ReadBytes(16));
	}

	/// <summary>
	/// Returns an object based on the SerializedType read next from the stream.
	/// </summary>
	/// <returns>An object instance.</returns>
	public object ReadObject()
	{
		return processObject((SerializedType)ReadByte());
	}

	public T ReadInstanceFields<T>(BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized) where T : new()
	{
		T val = new T();
		ReadTypeFields(val, typeof(T), Flags, Mask);
		return val;
	}

	public void ReadFields<T>(T Instance, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		ReadTypeFields(Instance, typeof(T), Flags, Mask);
	}

	public void ReadTypeFields(object Instance, Type Type, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		FieldInfo[] array = ((Flags == (BindingFlags.Instance | BindingFlags.Public)) ? Type.GetCachedFields() : Type.GetFields(Flags));
		foreach (FieldInfo fieldInfo in array)
		{
			if ((fieldInfo.Attributes & Mask) == 0)
			{
				FieldSaveVersion customAttribute = fieldInfo.GetCustomAttribute<FieldSaveVersion>();
				if (customAttribute == null || customAttribute.minimumSaveVersion <= -1 || customAttribute.minimumSaveVersion <= FileVersion)
				{
					fieldInfo.SetValue(Instance, ReadObject());
				}
			}
		}
	}

	public void ReadNamedFields(object Instance, Type Type, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		FieldInfo[] array = ((Flags == (BindingFlags.Instance | BindingFlags.Public)) ? Type.GetCachedFields() : Type.GetFields(Flags));
		int num = ReadOptimizedInt32();
		int i = 0;
		int num2 = array.Length;
		for (; i < num; i++)
		{
			string text = ReadOptimizedString();
			object value = ReadObject();
			for (int j = 0; j < num2; j++)
			{
				FieldInfo fieldInfo = array[j];
				if (fieldInfo.Name == text)
				{
					fieldInfo.SetValue(Instance, value);
					break;
				}
			}
		}
	}

	public void ReadGameObjectList(List<GameObject> List, string Forensics = null)
	{
		List.Clear();
		int num = ReadInt32();
		for (int i = 0; i < num; i++)
		{
			List.Add(ReadGameObject(Forensics));
		}
	}

	public GameObject GetPlayer()
	{
		return TokenGameObjects[0];
	}

	public GameObject ReadGameObject(string Forensics = null)
	{
		int num = ReadOptimizedInt32() - 2;
		switch (num)
		{
		case -2:
			return null;
		case -1:
			return GameObject.FindByID(ReadOptimizedString());
		case 0:
			if (!SerializePlayer)
			{
				return The.Player;
			}
			return GetPlayer();
		default:
			return TokenGameObjects[num];
		}
	}

	public bool UnspoolTo(ushort Value)
	{
		Span<byte> span = stackalloc byte[2];
		span[0] = (byte)(Value & 0xFF);
		span[1] = (byte)(Value >> 8);
		long num = BytesRemaining;
		int num2 = 0;
		int length = span.Length;
		while (num > 0)
		{
			if (ReadByte() == span[num2])
			{
				num2++;
				if (num2 >= length)
				{
					return true;
				}
			}
			else
			{
				num2 = 0;
			}
			num--;
		}
		return false;
	}

	public bool UnspoolTo(int Value, bool Prior = false)
	{
		Span<byte> destination = stackalloc byte[4];
		BitConverter.TryWriteBytes(destination, Value);
		long num = BytesRemaining;
		int num2 = 0;
		int length = destination.Length;
		while (num > 0)
		{
			if (ReadByte() == destination[num2])
			{
				num2++;
				if (num2 >= length)
				{
					if (Prior)
					{
						Stream.Position -= 4L;
					}
					return true;
				}
			}
			else
			{
				num2 = 0;
			}
			num--;
		}
		return false;
	}

	public void ReadEndVal()
	{
		if (ReadUInt16() != 44203)
		{
			throw new SerializationException("Invalid game object end value.");
		}
	}

	public bool ReadStartVal()
	{
		return ReadUInt16() switch
		{
			64206 => true, 
			52958 => false, 
			_ => throw new SerializationException("Invalid game object start value."), 
		};
	}

	public void FlagObjectForRemoval(GameObject Object)
	{
		if (InvalidGameObjects == null)
		{
			InvalidGameObjects = new List<GameObject>();
		}
		if (Object != null && !InvalidGameObjects.Contains(Object))
		{
			InvalidGameObjects.Add(Object);
		}
	}

	public void HandleObjectError(GameObject Object)
	{
		if (Object == null)
		{
			return;
		}
		if (Object.IsValid())
		{
			foreach (IPart parts in Object.PartsList)
			{
				parts.BasisError(Object, this);
			}
			if (Object._Effects.IsNullOrEmpty())
			{
				return;
			}
			{
				foreach (Effect effect in Object._Effects)
				{
					effect.BasisError(Object, this);
				}
				return;
			}
		}
		FlagObjectForRemoval(Object);
	}

	public void ReadGameObjects()
	{
		int num = ((!SerializePlayer) ? 1 : 0);
		int num2 = 0;
		try
		{
			Stream.Position = GameObjectPosition;
			GameObject gameObject = null;
			long length = Stream.Length;
			long num3 = -1L;
			while (num3 < length)
			{
				try
				{
					num2 = Errors;
					if (!ReadStartVal())
					{
						break;
					}
					num++;
					gameObject = TokenGameObjects[ReadOptimizedInt32()];
					gameObject.Load(this);
					ReadEndVal();
					num3 = -1L;
					if (Errors != num2)
					{
						HandleObjectError(gameObject);
					}
				}
				catch (Exception ex)
				{
					Errors++;
					FieldInfo fieldInfo = ex.Data["Field"] as FieldInfo;
					Type type = (ex.Data["Type"] as Type) ?? fieldInfo?.DeclaringType;
					Type type2 = ex.Data["LastType"] as Type;
					string text = ex.Data["TypeName"] as string;
					StringBuilder stringBuilder = Event.NewStringBuilder();
					stringBuilder.Append("Blueprint: ").Append(gameObject?.Blueprint ?? "Unknown");
					if (type != null)
					{
						stringBuilder.Compound("Type: ", ", ").Append(type.FullName);
					}
					else if (text != null)
					{
						stringBuilder.Compound("Type: ", ", ").Append(text);
					}
					else if (type2 != null)
					{
						stringBuilder.Compound("LastType: ", ", ").Append(type2.Name);
					}
					if (fieldInfo != null)
					{
						stringBuilder.Compound("Field: ", ", ").Append(fieldInfo.Name);
					}
					stringBuilder.Compound(ex.ToString(), "\n\n");
					HandleObjectError(gameObject);
					if (num3 >= 0 && Stream.CanSeek)
					{
						Stream.Position = num3;
						num3 = -1L;
					}
					if (!UnspoolTo((ushort)44203))
					{
						throw;
					}
					num3 = Stream.Position;
					stringBuilder.Insert(0, "Recovered from game object deserialization error.\n");
					ModInfo Mod;
					StackFrame Frame;
					if (type != null)
					{
						MetricsManager.LogAssemblyError(type, stringBuilder.ToString());
					}
					else if (ModManager.TryGetStackMod(ex, out Mod, out Frame))
					{
						Mod.Error(stringBuilder.ToString());
					}
					else
					{
						MetricsManager.LogError(stringBuilder.ToString());
					}
				}
				gameObject = null;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Unrecoverable GameObject read error", x);
			throw;
		}
		finally
		{
			if (GameObjectLength != num)
			{
				MetricsManager.LogError($"All requested game objects have not been read ({num}/{GameObjectLength}, spool errors: {num2})");
			}
		}
	}

	public void FinalizeRead()
	{
		ReadGameObjects();
		ReadEventRegistries();
		int i = 0;
		for (int gameObjectLength = GameObjectLength; i < gameObjectLength; i++)
		{
			TokenGameObjects[i].FinalizeRead(this);
		}
		GameObject.ExternalLoadBindings.Clear();
		SaveCompatibility();
		RemoveInvalidObjects();
	}

	public void ReadEventRegistries()
	{
		if (EventRegistryLength == 0)
		{
			return;
		}
		try
		{
			Stream.Position = EventRegistryPosition;
			int i = 0;
			for (int eventRegistryLength = EventRegistryLength; i < eventRegistryLength; i++)
			{
				EventRegistries[i].Read(this);
			}
		}
		catch (Exception x)
		{
			Errors++;
			Stream.Position = GameObjectReferencePosition;
			MetricsManager.LogException("Exception when reading event registries", x);
		}
	}

	public void SaveCompatibility()
	{
		if (FileVersion > 400)
		{
			return;
		}
		GameObject[] tokenGameObjects = TokenGameObjects;
		foreach (GameObject gameObject in tokenGameObjects)
		{
			switch (gameObject?.Blueprint)
			{
			case "CarbideFist":
			case "FulleriteFist":
			case "CrysteelFist":
				gameObject.SetStringProperty("TemporaryDefaultBehavior", "CyberneticsFistReplacement");
				break;
			}
		}
		tokenGameObjects = TokenGameObjects;
		for (int i = 0; i < tokenGameObjects.Length; i++)
		{
			tokenGameObjects[i]?.Body?.RegenerateDefaultEquipment();
		}
	}

	private static bool IsRemovable(GameObject Object)
	{
		if (Object.Physics == null)
		{
			return true;
		}
		if (!Object.Physics.Takeable)
		{
			if (!Object.IsNatural())
			{
				return true;
			}
			Object.Physics.Takeable = true;
		}
		return false;
	}

	public void RemoveInvalidObjects()
	{
		if (InvalidGameObjects.IsNullOrEmpty())
		{
			return;
		}
		Dictionary<string, GameObject> cachedObjects = The.ZoneManager.CachedObjects;
		foreach (GameObject invalidGameObject in InvalidGameObjects)
		{
			invalidGameObject.RemoveFromContext();
			if (invalidGameObject.HasID)
			{
				cachedObjects.Remove(invalidGameObject.ID);
			}
		}
	}

	/// <summary>
	/// Called ReadOptimizedString().
	/// This override to hide base BinaryReader.ReadString().
	/// </summary>
	/// <returns>A string value.</returns>
	public override string ReadString()
	{
		return ReadOptimizedString();
	}

	/// <summary>
	/// Returns a string value from the stream.
	/// </summary>
	/// <returns>A string value.</returns>
	public string ReadStringDirect()
	{
		return base.ReadString();
	}

	/// <summary>
	/// Returns a TimeSpan value from the stream.
	/// </summary>
	/// <returns>A TimeSpan value.</returns>
	public TimeSpan ReadTimeSpan()
	{
		return new TimeSpan(ReadInt64());
	}

	/// <summary>
	/// Returns a Type or null from the stream.
	///
	/// Throws an exception if the Type cannot be found.
	/// </summary>
	/// <returns>A Type instance.</returns>
	public Type ReadType()
	{
		return ReadType(throwOnError: true);
	}

	/// <summary>
	/// Returns a Type or null from the stream.
	///
	/// Throws an exception if the Type cannot be found and throwOnError is true.
	/// </summary>
	/// <returns>A Type instance.</returns>
	public Type ReadType(bool throwOnError)
	{
		if (readTypeCode() == SerializedType.NullType)
		{
			return null;
		}
		return Type.GetType(ReadOptimizedString(), throwOnError);
	}

	/// <summary>
	/// Returns an ArrayList from the stream that was stored optimized.
	/// </summary>
	/// <returns>An ArrayList instance.</returns>
	public ArrayList ReadOptimizedArrayList()
	{
		return new ArrayList(ReadOptimizedObjectArray());
	}

	/// <summary>
	/// Returns a BitArray from the stream that was stored optimized.
	/// </summary>
	/// <returns>A BitArray instance.</returns>
	public BitArray ReadOptimizedBitArray()
	{
		int num = ReadOptimizedInt32();
		if (num == 0)
		{
			return FullyOptimizableTypedArray;
		}
		return new BitArray(base.ReadBytes((num + 7) / 8))
		{
			Length = num
		};
	}

	/// <summary>
	/// Returns a BitVector32 value from the stream that was stored optimized.
	/// </summary>
	/// <returns>A BitVector32 value.</returns>
	public BitVector32 ReadOptimizedBitVector32()
	{
		return new BitVector32(Read7BitEncodedInt());
	}

	/// <summary>
	/// Returns a DateTime value from the stream that was stored optimized.
	/// </summary>
	/// <returns>A DateTime value.</returns>
	public DateTime ReadOptimizedDateTime()
	{
		BitVector32 bitVector = new BitVector32(ReadByte() | (ReadByte() << 8) | (ReadByte() << 16));
		DateTime dateTime = new DateTime(bitVector[SerializationWriter.DateYearMask], bitVector[SerializationWriter.DateMonthMask], bitVector[SerializationWriter.DateDayMask]);
		if (bitVector[SerializationWriter.DateHasTimeOrKindMask] == 1)
		{
			byte b = ReadByte();
			DateTimeKind dateTimeKind = (DateTimeKind)(b & 3);
			b &= 0xFC;
			if (dateTimeKind != DateTimeKind.Unspecified)
			{
				dateTime = DateTime.SpecifyKind(dateTime, dateTimeKind);
			}
			if (b == 0)
			{
				ReadByte();
			}
			else
			{
				dateTime = dateTime.Add(decodeTimeSpan(b));
			}
		}
		return dateTime;
	}

	/// <summary>
	/// Returns a Decimal value from the stream that was stored optimized.
	/// </summary>
	/// <returns>A Decimal value.</returns>
	public decimal ReadOptimizedDecimal()
	{
		byte b = ReadByte();
		int lo = 0;
		int mid = 0;
		int hi = 0;
		byte scale = 0;
		if ((b & 2) != 0)
		{
			scale = ReadByte();
		}
		if ((b & 4) == 0)
		{
			lo = (((b & 0x20) == 0) ? ReadInt32() : ReadOptimizedInt32());
		}
		if ((b & 8) == 0)
		{
			mid = (((b & 0x40) == 0) ? ReadInt32() : ReadOptimizedInt32());
		}
		if ((b & 0x10) == 0)
		{
			hi = (((b & 0x80) == 0) ? ReadInt32() : ReadOptimizedInt32());
		}
		return new decimal(lo, mid, hi, (b & 1) != 0, scale);
	}

	/// <summary>
	/// Returns an Int32 value from the stream that was stored optimized.
	/// </summary>
	/// <returns>An Int32 value.</returns>
	public int ReadOptimizedInt32()
	{
		int num = 0;
		int num2 = 0;
		byte b;
		do
		{
			b = ReadByte();
			num |= (b & 0x7F) << num2;
			num2 += 7;
		}
		while ((b & 0x80) != 0);
		return num;
	}

	/// <summary>
	/// Returns an Int16 value from the stream that was stored optimized.
	/// </summary>
	/// <returns>An Int16 value.</returns>
	public short ReadOptimizedInt16()
	{
		return (short)ReadOptimizedInt32();
	}

	/// <summary>
	/// Returns an Int64 value from the stream that was stored optimized.
	/// </summary>
	/// <returns>An Int64 value.</returns>
	public long ReadOptimizedInt64()
	{
		long num = 0L;
		int num2 = 0;
		byte b;
		do
		{
			b = ReadByte();
			num |= (long)(((ulong)b & 0x7FuL) << num2);
			num2 += 7;
		}
		while ((b & 0x80) != 0);
		return num;
	}

	/// <summary>
	/// Returns an object[] from the stream that was stored optimized.
	/// </summary>
	/// <returns>An object[] instance.</returns>
	public object[] ReadOptimizedObjectArray()
	{
		return ReadOptimizedObjectArray(null);
	}

	/// <summary>
	/// Returns an object[] from the stream that was stored optimized.
	/// The returned array will be typed according to the specified element type
	/// and the resulting array can be cast to the expected type.
	/// e.g.
	/// string[] myStrings = (string[]) reader.ReadOptimizedObjectArray(typeof(string));
	///
	/// An exception will be thrown if any of the deserialized values cannot be
	/// cast to the specified elementType.
	///
	/// </summary>
	/// <param name="elementType">The Type of the expected array elements. null will return a plain object[].</param>
	/// <returns>An object[] instance.</returns>
	public object[] ReadOptimizedObjectArray(Type elementType)
	{
		int num = ReadOptimizedInt32();
		object[] array = (object[])((elementType == null) ? new object[num] : Array.CreateInstance(elementType, num));
		for (int i = 0; i < array.Length; i++)
		{
			SerializedType serializedType = (SerializedType)ReadByte();
			switch (serializedType)
			{
			case SerializedType.NullSequenceType:
				i += ReadOptimizedInt32();
				break;
			case SerializedType.DuplicateValueSequenceType:
			{
				object obj = (array[i] = ReadObject());
				int num3 = ReadOptimizedInt32();
				while (num3-- > 0)
				{
					array[++i] = obj;
				}
				break;
			}
			case SerializedType.DBNullSequenceType:
			{
				int num2 = ReadOptimizedInt32();
				array[i] = DBNull.Value;
				while (num2-- > 0)
				{
					array[++i] = DBNull.Value;
				}
				break;
			}
			default:
				array[i] = processObject(serializedType);
				break;
			case SerializedType.NullType:
				break;
			}
		}
		return array;
	}

	/// <summary>
	/// Returns a pair of object[] arrays from the stream that were stored optimized.
	/// </summary>
	/// <returns>A pair of object[] arrays.</returns>
	public void ReadOptimizedObjectArrayPair(out object[] values1, out object[] values2)
	{
		values1 = ReadOptimizedObjectArray(null);
		values2 = new object[values1.Length];
		for (int i = 0; i < values2.Length; i++)
		{
			SerializedType serializedType = (SerializedType)ReadByte();
			switch (serializedType)
			{
			case SerializedType.DuplicateValueSequenceType:
			{
				values2[i] = values1[i];
				int num2 = ReadOptimizedInt32();
				while (num2-- > 0)
				{
					values2[++i] = values1[i];
				}
				break;
			}
			case SerializedType.DuplicateValueType:
				values2[i] = values1[i];
				break;
			case SerializedType.NullSequenceType:
				i += ReadOptimizedInt32();
				break;
			case SerializedType.DBNullSequenceType:
			{
				int num = ReadOptimizedInt32();
				values2[i] = DBNull.Value;
				while (num-- > 0)
				{
					values2[++i] = DBNull.Value;
				}
				break;
			}
			default:
				values2[i] = processObject(serializedType);
				break;
			case SerializedType.NullType:
				break;
			}
		}
	}

	/// <summary>
	/// Returns a string value from the stream that was stored optimized.
	/// </summary>
	/// <returns>A string value.</returns>
	public string ReadOptimizedString()
	{
		SerializedType serializedType = readTypeCode();
		if ((int)serializedType < 128)
		{
			return readTokenizedString((int)serializedType);
		}
		return serializedType switch
		{
			SerializedType.NullType => null, 
			SerializedType.YStringType => "Y", 
			SerializedType.NStringType => "N", 
			SerializedType.SingleCharStringType => char.ToString(ReadChar()), 
			SerializedType.SingleSpaceType => " ", 
			SerializedType.EmptyStringType => string.Empty, 
			_ => throw new InvalidOperationException("Unrecognized TypeCode, expected a type of string instead got: " + serializedType), 
		};
	}

	/// <summary>
	/// Returns a TimeSpan value from the stream that was stored optimized.
	/// </summary>
	/// <returns>A TimeSpan value.</returns>
	public TimeSpan ReadOptimizedTimeSpan()
	{
		return decodeTimeSpan(ReadByte());
	}

	/// <summary>
	/// Returns a Type from the stream.
	///
	/// Throws an exception if the Type cannot be found.
	/// </summary>
	/// <returns>A Type instance.</returns>
	public Type ReadOptimizedType()
	{
		return ReadOptimizedType(throwOnError: true);
	}

	/// <summary>
	/// Returns a Type from the stream.
	///
	/// Throws an exception if the Type cannot be found and throwOnError is true.
	/// </summary>
	/// <returns>A Type instance.</returns>
	public Type ReadOptimizedType(bool throwOnError)
	{
		return ModManager.ResolveType(ReadOptimizedString());
	}

	public Type ReadDirectType(bool ThrowOnError = false)
	{
		return ModManager.ResolveType(base.ReadString(), IgnoreCase: false, ThrowOnError);
	}

	/// <summary>
	/// Returns a UInt16 value from the stream that was stored optimized.
	/// </summary>
	/// <returns>A UInt16 value.</returns>
	[CLSCompliant(false)]
	public ushort ReadOptimizedUInt16()
	{
		return (ushort)ReadOptimizedUInt32();
	}

	/// <summary>
	/// Returns a UInt32 value from the stream that was stored optimized.
	/// </summary>
	/// <returns>A UInt32 value.</returns>
	[CLSCompliant(false)]
	public uint ReadOptimizedUInt32()
	{
		uint num = 0u;
		int num2 = 0;
		byte b;
		do
		{
			b = ReadByte();
			num |= (uint)((b & 0x7F) << num2);
			num2 += 7;
		}
		while ((b & 0x80) != 0);
		return num;
	}

	/// <summary>
	/// Returns a UInt64 value from the stream that was stored optimized.
	/// </summary>
	/// <returns>A UInt64 value.</returns>
	[CLSCompliant(false)]
	public ulong ReadOptimizedUInt64()
	{
		ulong num = 0uL;
		int num2 = 0;
		byte b;
		do
		{
			b = ReadByte();
			num |= ((ulong)b & 0x7FuL) << num2;
			num2 += 7;
		}
		while ((b & 0x80) != 0);
		return num;
	}

	/// <summary>
	/// Returns a typed array from the stream.
	/// </summary>
	/// <returns>A typed array.</returns>
	public Array ReadTypedArray()
	{
		return (Array)processArrayTypes(readTypeCode(), null);
	}

	/// <summary>
	/// Returns a new, simple generic dictionary populated with keys and values from the stream.
	/// </summary>
	/// <typeparam name="K">The key Type.</typeparam>
	/// <typeparam name="V">The value Type.</typeparam>
	/// <returns>A new, simple, populated generic Dictionary.</returns>
	public Dictionary<K, V> ReadDictionary<K, V>()
	{
		int num = ReadInt32();
		if (num == -1)
		{
			return null;
		}
		Dictionary<K, V> dictionary = new Dictionary<K, V>(num);
		for (int i = 0; i < num; i++)
		{
			dictionary.Add((K)ReadObject(), (V)ReadObject());
		}
		return dictionary;
	}

	/// <summary>
	/// Populates a pre-existing generic dictionary with keys and values from the stream.
	/// This allows a generic dictionary to be created without using the default constructor.
	/// </summary>
	/// <typeparam name="K">The key Type.</typeparam>
	/// <typeparam name="V">The value Type.</typeparam>
	public void ReadDictionary<K, V>(Dictionary<K, V> dictionary)
	{
		K[] array = (K[])processArrayTypes(readTypeCode(), typeof(K));
		V[] array2 = (V[])processArrayTypes(readTypeCode(), typeof(V));
		if (dictionary == null)
		{
			dictionary = new Dictionary<K, V>(array.Length);
		}
		for (int i = 0; i < array.Length; i++)
		{
			dictionary.Add(array[i], array2[i]);
		}
	}

	/// <summary>
	/// Returns a generic List populated with values from the stream.
	/// </summary>
	/// <typeparam name="T">The list Type.</typeparam>
	/// <returns>A new generic List.</returns>
	public List<T> ReadList<T>()
	{
		int num = ReadInt32();
		if (num == -1)
		{
			return null;
		}
		List<T> list = new List<T>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add((T)ReadObject());
		}
		return list;
	}

	public void ReadList<T>(List<T> List)
	{
		int num = ReadInt32();
		List.EnsureCapacity(num);
		for (int i = 0; i < num; i++)
		{
			List.Add((T)ReadObject());
		}
	}

	public List<string> ReadStringList()
	{
		int num = ReadInt32();
		if (num == -1)
		{
			return null;
		}
		List<string> list = new List<string>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add((string)ReadObject());
		}
		return list;
	}

	public Location2D ReadLocation2D()
	{
		int num = ReadOptimizedInt32();
		if (num < 0)
		{
			return null;
		}
		int y = ReadOptimizedInt32();
		return Location2D.Get(num, y);
	}

	public List<Location2D> ReadLocation2DList()
	{
		int num = ReadInt32();
		if (num == -1)
		{
			return null;
		}
		List<Location2D> list = new List<Location2D>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(ReadLocation2D());
		}
		return list;
	}

	public Cell ReadCell()
	{
		string text = ReadOptimizedString();
		if (text == null)
		{
			return null;
		}
		int x = ReadOptimizedInt32();
		int y = ReadOptimizedInt32();
		return The.ZoneManager.GetZone(text).GetCell(x, y);
	}

	public IComposite ReadComposite()
	{
		SerializedType serializedType = readTypeCode();
		if (serializedType == SerializedType.NullType)
		{
			return null;
		}
		return DeserializeComposite(serializedType);
	}

	private IComposite DeserializeComposite(SerializedType Code)
	{
		StartBlock(out var Position, out var Length);
		Type type = null;
		IComposite composite = null;
		try
		{
			type = ReadTokenizedType();
			composite = (IComposite)Activator.CreateInstance(type, nonPublic: true);
			if (Code == SerializedType.ICompositeFieldType)
			{
				ReadTypeFields(composite, type);
			}
			composite.Read(this);
		}
		catch (Exception exception)
		{
			SkipBlock(exception, type, Position, Length);
		}
		return composite;
	}

	public T ReadComposite<T>() where T : IComposite, new()
	{
		SerializedType serializedType = readTypeCode();
		if (serializedType == SerializedType.NullType)
		{
			return default;
		}
		StartBlock(out var Position, out var Length);
		T val = new T();
		try
		{
			if (serializedType == SerializedType.ICompositeFieldType)
			{
				ReadTypeFields(val, typeof(T));
			}
			val.Read(this);
		}
		catch (Exception exception)
		{
			SkipBlock(exception, typeof(T), Position, Length);
		}
		return val;
	}

	public List<T> ReadCompositeList<T>() where T : IComposite
	{
		int num = ReadOptimizedInt32() - 1;
		if (num == -1)
		{
			return null;
		}
		List<T> list = new List<T>(num);
		for (int i = 0; i < num; i++)
		{
			SerializedType serializedType = readTypeCode();
			if (serializedType == SerializedType.NullType)
			{
				list.Add(default);
			}
			T val = (T)DeserializeComposite(serializedType);
			if (val != null)
			{
				list.Add(val);
			}
		}
		return list;
	}

	/// <summary>
	/// Returns a Nullable struct from the stream.
	/// The value returned must be cast to the correct Nullable type.
	/// Synonym for ReadObject();
	/// </summary>
	/// <returns>A struct value or null</returns>
	public ValueType ReadNullable()
	{
		return (ValueType)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable Boolean from the stream.
	/// </summary>
	/// <returns>A Nullable Boolean.</returns>
	public bool? ReadNullableBoolean()
	{
		return (bool?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable Byte from the stream.
	/// </summary>
	/// <returns>A Nullable Byte.</returns>
	public byte? ReadNullableByte()
	{
		return (byte?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable Char from the stream.
	/// </summary>
	/// <returns>A Nullable Char.</returns>
	public char? ReadNullableChar()
	{
		return (char?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable DateTime from the stream.
	/// </summary>
	/// <returns>A Nullable DateTime.</returns>
	public DateTime? ReadNullableDateTime()
	{
		return (DateTime?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable Decimal from the stream.
	/// </summary>
	/// <returns>A Nullable Decimal.</returns>
	public decimal? ReadNullableDecimal()
	{
		return (decimal?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable Double from the stream.
	/// </summary>
	/// <returns>A Nullable Double.</returns>
	public double? ReadNullableDouble()
	{
		return (double?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable Guid from the stream.
	/// </summary>
	/// <returns>A Nullable Guid.</returns>
	public Guid? ReadNullableGuid()
	{
		return (Guid?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable Int16 from the stream.
	/// </summary>
	/// <returns>A Nullable Int16.</returns>
	public short? ReadNullableInt16()
	{
		return (short?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable Int32 from the stream.
	/// </summary>
	/// <returns>A Nullable Int32.</returns>
	public int? ReadNullableInt32()
	{
		return (int?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable Int64 from the stream.
	/// </summary>
	/// <returns>A Nullable Int64.</returns>
	public long? ReadNullableInt64()
	{
		return (long?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable SByte from the stream.
	/// </summary>
	/// <returns>A Nullable SByte.</returns>
	[CLSCompliant(false)]
	public sbyte? ReadNullableSByte()
	{
		return (sbyte?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable Single from the stream.
	/// </summary>
	/// <returns>A Nullable Single.</returns>
	public float? ReadNullableSingle()
	{
		return (float?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable TimeSpan from the stream.
	/// </summary>
	/// <returns>A Nullable TimeSpan.</returns>
	public TimeSpan? ReadNullableTimeSpan()
	{
		return (TimeSpan?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable UInt16 from the stream.
	/// </summary>
	/// <returns>A Nullable UInt16.</returns>
	[CLSCompliant(false)]
	public ushort? ReadNullableUInt16()
	{
		return (ushort?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable UInt32 from the stream.
	/// </summary>
	/// <returns>A Nullable UInt32.</returns>
	[CLSCompliant(false)]
	public uint? ReadNullableUInt32()
	{
		return (uint?)ReadObject();
	}

	/// <summary>
	/// Returns a Nullable UInt64 from the stream.
	/// </summary>
	/// <returns>A Nullable UInt64.</returns>
	[CLSCompliant(false)]
	public ulong? ReadNullableUInt64()
	{
		return (ulong?)ReadObject();
	}

	/// <summary>
	/// Returns a Byte[] from the stream.
	/// </summary>
	/// <returns>A Byte instance; or null.</returns>
	public byte[] ReadByteArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new byte[0], 
			_ => readByteArray(), 
		};
	}

	/// <summary>
	/// Returns a Char[] from the stream.
	/// </summary>
	/// <returns>A Char[] value; or null.</returns>
	public char[] ReadCharArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new char[0], 
			_ => readCharArray(), 
		};
	}

	/// <summary>
	/// Returns a Double[] from the stream.
	/// </summary>
	/// <returns>A Double[] instance; or null.</returns>
	public double[] ReadDoubleArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new double[0], 
			_ => readDoubleArray(), 
		};
	}

	/// <summary>
	/// Returns a Guid[] from the stream.
	/// </summary>
	/// <returns>A Guid[] instance; or null.</returns>
	public Guid[] ReadGuidArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new Guid[0], 
			_ => readGuidArray(), 
		};
	}

	/// <summary>
	/// Returns an Int16[] from the stream.
	/// </summary>
	/// <returns>An Int16[] instance; or null.</returns>
	public short[] ReadInt16Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new short[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			short[] array = new short[ReadOptimizedInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadInt16();
				}
				else
				{
					array[i] = ReadOptimizedInt16();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	/// Returns an object[] or null from the stream.
	/// </summary>
	/// <returns>A DateTime value.</returns>
	public object[] ReadObjectArray()
	{
		return ReadObjectArray(null);
	}

	/// <summary>
	/// Returns an object[] or null from the stream.
	/// The returned array will be typed according to the specified element type
	/// and the resulting array can be cast to the expected type.
	/// e.g.
	/// string[] myStrings = (string[]) reader.ReadObjectArray(typeof(string));
	///
	/// An exception will be thrown if any of the deserialized values cannot be
	/// cast to the specified elementType.
	///
	/// </summary>
	/// <param name="elementType">The Type of the expected array elements. null will return a plain object[].</param>
	/// <returns>An object[] instance.</returns>
	public object[] ReadObjectArray(Type elementType)
	{
		switch (readTypeCode())
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyObjectArrayType:
			if (!(elementType == null))
			{
				return (object[])Array.CreateInstance(elementType, 0);
			}
			return new object[0];
		case SerializedType.EmptyTypedArrayType:
			throw new Exception();
		default:
			return ReadOptimizedObjectArray(elementType);
		}
	}

	/// <summary>
	/// Returns a Single[] from the stream.
	/// </summary>
	/// <returns>A Single[] instance; or null.</returns>
	public float[] ReadSingleArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new float[0], 
			_ => readSingleArray(), 
		};
	}

	/// <summary>
	/// Returns an SByte[] from the stream.
	/// </summary>
	/// <returns>An SByte[] instance; or null.</returns>
	[CLSCompliant(false)]
	public sbyte[] ReadSByteArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new sbyte[0], 
			_ => readSByteArray(), 
		};
	}

	/// <summary>
	/// Returns a string[] or null from the stream.
	/// </summary>
	/// <returns>An string[] instance.</returns>
	public string[] ReadStringArray()
	{
		return (string[])ReadObjectArray(typeof(string));
	}

	/// <summary>
	/// Returns a UInt16[] from the stream.
	/// </summary>
	/// <returns>A UInt16[] instance; or null.</returns>
	[CLSCompliant(false)]
	public ushort[] ReadUInt16Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new ushort[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			ushort[] array = new ushort[ReadOptimizedUInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadUInt16();
				}
				else
				{
					array[i] = ReadOptimizedUInt16();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	/// Returns a Boolean[] from the stream.
	/// </summary>
	/// <returns>A Boolean[] instance; or null.</returns>
	public bool[] ReadBooleanArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new bool[0], 
			_ => readBooleanArray(), 
		};
	}

	/// <summary>
	/// Returns a DateTime[] from the stream.
	/// </summary>
	/// <returns>A DateTime[] instance; or null.</returns>
	public DateTime[] ReadDateTimeArray()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new DateTime[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			DateTime[] array = new DateTime[ReadOptimizedInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadDateTime();
				}
				else
				{
					array[i] = ReadOptimizedDateTime();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	/// Returns a Decimal[] from the stream.
	/// </summary>
	/// <returns>A Decimal[] instance; or null.</returns>
	public decimal[] ReadDecimalArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new decimal[0], 
			_ => readDecimalArray(), 
		};
	}

	/// <summary>
	/// Returns an Int32[] from the stream.
	/// </summary>
	/// <returns>An Int32[] instance; or null.</returns>
	public int[] ReadInt32Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new int[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			int[] array = new int[ReadOptimizedInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadInt32();
				}
				else
				{
					array[i] = ReadOptimizedInt32();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	/// Returns an Int64[] from the stream.
	/// </summary>
	/// <returns>An Int64[] instance; or null.</returns>
	public long[] ReadInt64Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new long[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			long[] array = new long[ReadOptimizedInt64()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadInt64();
				}
				else
				{
					array[i] = ReadOptimizedInt64();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	/// Returns a string[] from the stream that was stored optimized.
	/// </summary>
	/// <returns>An string[] instance.</returns>
	public string[] ReadOptimizedStringArray()
	{
		return (string[])ReadOptimizedObjectArray(typeof(string));
	}

	/// <summary>
	/// Returns a TimeSpan[] from the stream.
	/// </summary>
	/// <returns>A TimeSpan[] instance; or null.</returns>
	public TimeSpan[] ReadTimeSpanArray()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new TimeSpan[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			TimeSpan[] array = new TimeSpan[ReadOptimizedInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadTimeSpan();
				}
				else
				{
					array[i] = ReadOptimizedTimeSpan();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	/// Returns a UInt[] from the stream.
	/// </summary>
	/// <returns>A UInt[] instance; or null.</returns>
	[CLSCompliant(false)]
	public uint[] ReadUInt32Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new uint[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			uint[] array = new uint[ReadOptimizedUInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadUInt32();
				}
				else
				{
					array[i] = ReadOptimizedUInt32();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	/// Returns a UInt64[] from the stream.
	/// </summary>
	/// <returns>A UInt64[] instance; or null.</returns>
	[CLSCompliant(false)]
	public ulong[] ReadUInt64Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new ulong[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			ulong[] array = new ulong[ReadOptimizedInt64()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadUInt64();
				}
				else
				{
					array[i] = ReadOptimizedUInt64();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	/// Returns a Boolean[] from the stream.
	/// </summary>
	/// <returns>A Boolean[] instance; or null.</returns>
	public bool[] ReadOptimizedBooleanArray()
	{
		return ReadBooleanArray();
	}

	/// <summary>
	/// Returns a DateTime[] from the stream.
	/// </summary>
	/// <returns>A DateTime[] instance; or null.</returns>
	public DateTime[] ReadOptimizedDateTimeArray()
	{
		return ReadDateTimeArray();
	}

	/// <summary>
	/// Returns a Decimal[] from the stream.
	/// </summary>
	/// <returns>A Decimal[] instance; or null.</returns>
	public decimal[] ReadOptimizedDecimalArray()
	{
		return ReadDecimalArray();
	}

	/// <summary>
	/// Returns a Int16[] from the stream.
	/// </summary>
	/// <returns>An Int16[] instance; or null.</returns>
	public short[] ReadOptimizedInt16Array()
	{
		return ReadInt16Array();
	}

	/// <summary>
	/// Returns a Int32[] from the stream.
	/// </summary>
	/// <returns>An Int32[] instance; or null.</returns>
	public int[] ReadOptimizedInt32Array()
	{
		return ReadInt32Array();
	}

	/// <summary>
	/// Returns a Int64[] from the stream.
	/// </summary>
	/// <returns>A Int64[] instance; or null.</returns>
	public long[] ReadOptimizedInt64Array()
	{
		return ReadInt64Array();
	}

	/// <summary>
	/// Returns a TimeSpan[] from the stream.
	/// </summary>
	/// <returns>A TimeSpan[] instance; or null.</returns>
	public TimeSpan[] ReadOptimizedTimeSpanArray()
	{
		return ReadTimeSpanArray();
	}

	/// <summary>
	/// Returns a UInt16[] from the stream.
	/// </summary>
	/// <returns>A UInt16[] instance; or null.</returns>
	[CLSCompliant(false)]
	public ushort[] ReadOptimizedUInt16Array()
	{
		return ReadUInt16Array();
	}

	/// <summary>
	/// Returns a UInt32[] from the stream.
	/// </summary>
	/// <returns>A UInt32[] instance; or null.</returns>
	[CLSCompliant(false)]
	public uint[] ReadOptimizedUInt32Array()
	{
		return ReadUInt32Array();
	}

	/// <summary>
	/// Returns a UInt64[] from the stream.
	/// </summary>
	/// <returns>A UInt64[] instance; or null.</returns>
	[CLSCompliant(false)]
	public ulong[] ReadOptimizedUInt64Array()
	{
		return ReadUInt64Array();
	}

	public Version ReadOptimizedVersion()
	{
		if (FileVersion < 400)
		{
			return new Version(ReadOptimizedInt32(), ReadOptimizedInt32(), ReadOptimizedInt32(), ReadOptimizedInt32());
		}
		return new Version((int)ReadOptimizedUInt32(), (int)ReadOptimizedUInt32(), (int)ReadOptimizedUInt32(), (int)ReadOptimizedUInt32());
	}

	/// <summary>
	/// Allows an existing object, implementing IOwnedDataSerializable, to 
	/// retrieve its owned data from the stream.
	/// </summary>
	/// <param name="target">Any IOwnedDataSerializable object.</param>
	/// <param name="context">An optional, arbitrary object to allow context to be provided.</param>
	public void ReadOwnedData(IOwnedDataSerializable target, object context)
	{
		target.DeserializeOwnedData(this, context);
	}

	/// <summary>
	/// Returns the object associated with the object token read next from the stream.
	/// </summary>
	/// <returns>An object.</returns>
	public object ReadTokenizedObject()
	{
		return TokenObjects[ReadOptimizedInt32()];
	}

	public ITokenized ReadTokenized()
	{
		int num = ReadOptimizedInt32() - 1;
		if (num < 0)
		{
			return null;
		}
		return Tokenized[num];
	}

	public Type ReadTokenizedType()
	{
		int num = ReadOptimizedInt32() - 1;
		if (num == -1)
		{
			return null;
		}
		Type obj = TokenTypes[num];
		if ((object)obj == null)
		{
			TypeLoadException ex = new TypeLoadException("No type by name '" + TokenTypeNames[num] + "' is loaded");
			ex.Data["TypeName"] = TokenTypeNames[num];
			throw ex;
		}
		return obj;
	}

	public EventRegistry ReadEventRegistry()
	{
		int num = ReadOptimizedInt32() - 1;
		if (num == -1)
		{
			return null;
		}
		return EventRegistries[num];
	}

	public GameObjectReference ReadGameObjectReference()
	{
		int num = ReadOptimizedInt32() - 1;
		if (num == -1)
		{
			return null;
		}
		return GameObjectReferences[num];
	}

	public void ReadModVersions()
	{
		int num = ReadOptimizedInt32();
		ModVersions = new Dictionary<string, Version>(num);
		for (int i = 0; i < num; i++)
		{
			ModVersions.Add(ReadOptimizedString(), ReadOptimizedVersion());
		}
	}

	internal void ReadExtensionHash()
	{
		if (FileVersion >= 408)
		{
			ExtensionHash = ReadOptimizedUInt64();
		}
	}

	public IEnumerable<string> GetSavedMods()
	{
		return ModVersions.Keys;
	}

	public void StartBlock(out long Position, out int Length)
	{
		Length = ReadOptimizedInt32();
		Position = Stream.Position;
	}

	public void SkipBlock(Exception Exception, Type Type, long Position, int Length)
	{
		Errors++;
		long num = Position + Length - Stream.Position;
		Stream.Position = Position + Length;
		string arg = Type?.FullName ?? (Exception.Data["TypeName"] as string) ?? "unknown type";
		MetricsManager.LogException($"Exception when deserializing '{arg}' (Skipping {num}/{Length} bytes)", Exception);
	}

	/// <summary>
	/// Returns a TimeSpan decoded from packed data.
	/// This routine is called from ReadOptimizedDateTime() and ReadOptimizedTimeSpan().
	/// <remarks>
	/// This routine uses a parameter to allow ReadOptimizedDateTime() to 'peek' at the
	/// next byte and extract the DateTimeKind from bits one and two (IsNegative and HasDays)
	/// which are never set for a Time portion of a DateTime.
	/// </remarks>
	/// </summary>
	/// <param name="initialByte">The first of two always-present bytes.</param>
	/// <returns>A decoded TimeSpan</returns>
	private TimeSpan decodeTimeSpan(byte initialByte)
	{
		long num = 0L;
		BitVector32 bitVector = new BitVector32(initialByte | (ReadByte() << 8));
		bool flag = bitVector[SerializationWriter.HasTimeSection] == 1;
		bool flag2 = bitVector[SerializationWriter.HasSecondsSection] == 1;
		bool flag3 = bitVector[SerializationWriter.HasMillisecondsSection] == 1;
		if (flag3)
		{
			bitVector = new BitVector32(bitVector.Data | (ReadByte() << 16) | (ReadByte() << 24));
		}
		else if (flag2 & flag)
		{
			bitVector = new BitVector32(bitVector.Data | (ReadByte() << 16));
		}
		if (flag)
		{
			num += bitVector[SerializationWriter.HoursSection] * 36000000000L;
			num += (long)bitVector[SerializationWriter.MinutesSection] * 600000000L;
		}
		if (flag2)
		{
			num += (long)bitVector[(!flag && !flag3) ? SerializationWriter.MinutesSection : SerializationWriter.SecondsSection] * 10000000L;
		}
		if (flag3)
		{
			num += (long)bitVector[SerializationWriter.MillisecondsSection] * 10000L;
		}
		if (bitVector[SerializationWriter.HasDaysSection] == 1)
		{
			num += ReadOptimizedInt32() * 864000000000L;
		}
		if (bitVector[SerializationWriter.IsNegativeSection] == 1)
		{
			num = -num;
		}
		return new TimeSpan(num);
	}

	/// <summary>
	/// Creates a BitArray representing which elements of a typed array
	/// are serializable.
	/// </summary>
	/// <param name="serializedType">The type of typed array.</param>
	/// <returns>A BitArray denoting which elements are serializable.</returns>
	private BitArray readTypedArrayOptimizeFlags(SerializedType serializedType)
	{
		BitArray result = null;
		switch (serializedType)
		{
		case SerializedType.FullyOptimizedTypedArrayType:
			result = FullyOptimizableTypedArray;
			break;
		case SerializedType.PartiallyOptimizedTypedArrayType:
			result = ReadOptimizedBitArray();
			break;
		}
		return result;
	}

	/// <summary>
	/// Returns an object based on supplied SerializedType.
	/// </summary>
	/// <returns>An object instance.</returns>
	private object processObject(SerializedType typeCode)
	{
		if (typeCode == SerializedType.NullType)
		{
			return null;
		}
		if (typeCode == SerializedType.Int32Type)
		{
			return ReadInt32();
		}
		if (typeCode == SerializedType.EmptyStringType)
		{
			return string.Empty;
		}
		if ((int)typeCode < 128)
		{
			return readTokenizedString((int)typeCode);
		}
		switch (typeCode)
		{
		case SerializedType.BooleanFalseType:
			return false;
		case SerializedType.ZeroInt32Type:
			return 0;
		case SerializedType.OptimizedInt32Type:
			return ReadOptimizedInt32();
		case SerializedType.OptimizedInt32NegativeType:
			return -ReadOptimizedInt32() - 1;
		case SerializedType.DecimalType:
			return ReadOptimizedDecimal();
		case SerializedType.ZeroDecimalType:
			return 0m;
		case SerializedType.YStringType:
			return "Y";
		case SerializedType.DateTimeType:
			return ReadDateTime();
		case SerializedType.OptimizedDateTimeType:
			return ReadOptimizedDateTime();
		case SerializedType.SingleCharStringType:
			return char.ToString(ReadChar());
		case SerializedType.SingleSpaceType:
			return " ";
		case SerializedType.OneInt32Type:
			return 1;
		case SerializedType.OptimizedInt16Type:
			return ReadOptimizedInt16();
		case SerializedType.OptimizedInt16NegativeType:
			return -ReadOptimizedInt16() - 1;
		case SerializedType.OneDecimalType:
			return 1m;
		case SerializedType.BooleanTrueType:
			return true;
		case SerializedType.NStringType:
			return "N";
		case SerializedType.DBNullType:
			return DBNull.Value;
		case SerializedType.ObjectArrayType:
			return ReadOptimizedObjectArray();
		case SerializedType.EmptyObjectArrayType:
			return new object[0];
		case SerializedType.MinusOneInt32Type:
			return -1;
		case SerializedType.MinusOneInt64Type:
			return -1L;
		case SerializedType.MinusOneInt16Type:
			return (short)(-1);
		case SerializedType.MinDateTimeType:
			return DateTime.MinValue;
		case SerializedType.GuidType:
			return ReadGuid();
		case SerializedType.EmptyGuidType:
			return Guid.Empty;
		case SerializedType.TimeSpanType:
			return ReadTimeSpan();
		case SerializedType.MaxDateTimeType:
			return DateTime.MaxValue;
		case SerializedType.ZeroTimeSpanType:
			return TimeSpan.Zero;
		case SerializedType.OptimizedTimeSpanType:
			return ReadOptimizedTimeSpan();
		case SerializedType.DoubleType:
			return ReadDouble();
		case SerializedType.ZeroDoubleType:
			return 0.0;
		case SerializedType.Int64Type:
			return ReadInt64();
		case SerializedType.ZeroInt64Type:
			return 0L;
		case SerializedType.OptimizedInt64Type:
			return ReadOptimizedInt64();
		case SerializedType.OptimizedInt64NegativeType:
			return -ReadOptimizedInt64() - 1;
		case SerializedType.Int16Type:
			return ReadInt16();
		case SerializedType.ZeroInt16Type:
			return (short)0;
		case SerializedType.SingleType:
			return ReadSingle();
		case SerializedType.ZeroSingleType:
			return 0f;
		case SerializedType.ByteType:
			return ReadByte();
		case SerializedType.ZeroByteType:
			return (byte)0;
		case SerializedType.GameObjectType:
			return ReadGameObject();
		case SerializedType.OtherType:
		{
			AppDomain.CurrentDomain.AssemblyResolve += assemblyResolveHandler;
			object result = null;
			try
			{
				result = createBinaryFormatter().Deserialize(Stream);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("Exception deserializing an unknown type: " + ex);
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= assemblyResolveHandler;
			}
			return result;
		}
		case SerializedType.UInt16Type:
			return ReadUInt16();
		case SerializedType.ZeroUInt16Type:
			return (ushort)0;
		case SerializedType.UInt32Type:
			return ReadUInt32();
		case SerializedType.ZeroUInt32Type:
			return 0u;
		case SerializedType.OptimizedUInt32Type:
			return ReadOptimizedUInt32();
		case SerializedType.UInt64Type:
			return ReadUInt64();
		case SerializedType.ZeroUInt64Type:
			return 0uL;
		case SerializedType.OptimizedUInt64Type:
			return ReadOptimizedUInt64();
		case SerializedType.BitVector32Type:
			return ReadBitVector32();
		case SerializedType.CharType:
			return ReadChar();
		case SerializedType.ZeroCharType:
			return '\0';
		case SerializedType.SByteType:
			return ReadSByte();
		case SerializedType.ZeroSByteType:
			return (sbyte)0;
		case SerializedType.OneByteType:
			return (byte)1;
		case SerializedType.OneDoubleType:
			return 1.0;
		case SerializedType.OneCharType:
			return '\u0001';
		case SerializedType.OneInt16Type:
			return (short)1;
		case SerializedType.OneInt64Type:
			return 1L;
		case SerializedType.OneUInt16Type:
			return (ushort)1;
		case SerializedType.OptimizedUInt16Type:
			return ReadOptimizedUInt16();
		case SerializedType.OneUInt32Type:
			return 1u;
		case SerializedType.OneUInt64Type:
			return 1uL;
		case SerializedType.OneSByteType:
			return (sbyte)1;
		case SerializedType.OneSingleType:
			return 1f;
		case SerializedType.BitArrayType:
			return ReadOptimizedBitArray();
		case SerializedType.TypeType:
			return ReadTokenizedType();
		case SerializedType.ArrayListType:
			return ReadOptimizedArrayList();
		case SerializedType.StringListType:
			return ReadStringList();
		case SerializedType.IDictionaryType:
		{
			Type type3 = ReadTokenizedType();
			int num2 = ReadInt32();
			CapacityArgs[0] = num2;
			IDictionary dictionary = (IDictionary)Activator.CreateInstance(type3, CapacityArgs);
			for (int j = 0; j < num2; j++)
			{
				dictionary.Add(ReadObject(), ReadObject());
			}
			return dictionary;
		}
		case SerializedType.IListType:
		{
			Type type2 = ReadTokenizedType();
			int num = ReadInt32();
			CapacityArgs[0] = num;
			IList list = (IList)Activator.CreateInstance(type2, CapacityArgs);
			for (int i = 0; i < num; i++)
			{
				list.Add(ReadObject());
			}
			return list;
		}
		case SerializedType.ICompositeType:
		case SerializedType.ICompositeFieldType:
			return DeserializeComposite(typeCode);
		case SerializedType.SingleInstanceType:
			try
			{
				return Activator.CreateInstance(ReadTokenizedType(), nonPublic: true);
			}
			catch
			{
				return null;
			}
		case SerializedType.OwnedDataSerializableAndRecreatableType:
		{
			object obj2 = Activator.CreateInstance(ReadTokenizedType());
			ReadOwnedData((IOwnedDataSerializable)obj2, null);
			return obj2;
		}
		case SerializedType.OptimizedEnumType:
		{
			Type enumType2 = ReadTokenizedType();
			Type underlyingType2 = Enum.GetUnderlyingType(enumType2);
			if (underlyingType2 == typeof(int) || underlyingType2 == typeof(uint) || underlyingType2 == typeof(long) || underlyingType2 == typeof(ulong))
			{
				return Enum.ToObject(enumType2, ReadOptimizedUInt64());
			}
			return Enum.ToObject(enumType2, ReadUInt64());
		}
		case SerializedType.EnumType:
		{
			Type enumType = ReadTokenizedType();
			Type underlyingType = Enum.GetUnderlyingType(enumType);
			if (underlyingType == typeof(int))
			{
				return Enum.ToObject(enumType, ReadInt32());
			}
			if (underlyingType == typeof(byte))
			{
				return Enum.ToObject(enumType, ReadByte());
			}
			if (underlyingType == typeof(short))
			{
				return Enum.ToObject(enumType, ReadInt16());
			}
			if (underlyingType == typeof(uint))
			{
				return Enum.ToObject(enumType, ReadUInt32());
			}
			if (underlyingType == typeof(long))
			{
				return Enum.ToObject(enumType, ReadInt64());
			}
			if (underlyingType == typeof(sbyte))
			{
				return Enum.ToObject(enumType, ReadSByte());
			}
			if (underlyingType == typeof(ushort))
			{
				return Enum.ToObject(enumType, ReadUInt16());
			}
			return Enum.ToObject(enumType, ReadUInt64());
		}
		case SerializedType.SurrogateHandledType:
		{
			Type type = ReadTokenizedType();
			return SerializationWriter.findSurrogateForType(type).Deserialize(this, type);
		}
		default:
		{
			object obj = processArrayTypes(typeCode, null);
			if (obj != null)
			{
				return obj;
			}
			throw new InvalidOperationException("Unrecognized TypeCode: " + typeCode);
		}
		}
	}

	private static IFormatter createBinaryFormatter()
	{
		if (formatter == null)
		{
			Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
			formatter = new BinaryFormatter();
			formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			formatter.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
		}
		return formatter;
	}

	/// <summary>
	/// Determine whether the passed-in type code refers to an array type
	/// and deserializes the array if it is.
	/// Returns null if not an array type.
	/// </summary>
	/// <param name="typeCode">The SerializedType to check.</param>
	/// <param name="defaultElementType">The Type of array element; null if to be read from stream.</param>
	/// <returns />
	private object processArrayTypes(SerializedType typeCode, Type defaultElementType)
	{
		switch (typeCode)
		{
		case SerializedType.StringArrayType:
			return ReadOptimizedStringArray();
		case SerializedType.Int32ArrayType:
			return ReadInt32Array();
		case SerializedType.Int64ArrayType:
			return ReadInt64Array();
		case SerializedType.DecimalArrayType:
			return readDecimalArray();
		case SerializedType.TimeSpanArrayType:
			return ReadTimeSpanArray();
		case SerializedType.UInt32ArrayType:
			return ReadUInt32Array();
		case SerializedType.UInt64ArrayType:
			return ReadUInt64Array();
		case SerializedType.DateTimeArrayType:
			return ReadDateTimeArray();
		case SerializedType.BooleanArrayType:
			return readBooleanArray();
		case SerializedType.ByteArrayType:
			return readByteArray();
		case SerializedType.CharArrayType:
			return readCharArray();
		case SerializedType.DoubleArrayType:
			return readDoubleArray();
		case SerializedType.SingleArrayType:
			return readSingleArray();
		case SerializedType.GuidArrayType:
			return readGuidArray();
		case SerializedType.SByteArrayType:
			return readSByteArray();
		case SerializedType.Int16ArrayType:
			return ReadInt16Array();
		case SerializedType.UInt16ArrayType:
			return ReadUInt16Array();
		case SerializedType.EmptyTypedArrayType:
			return Array.CreateInstance((defaultElementType != null) ? defaultElementType : ReadTokenizedType(), 0);
		case SerializedType.OtherTypedArrayType:
			return ReadOptimizedObjectArray(ReadTokenizedType());
		case SerializedType.ObjectArrayType:
			return ReadOptimizedObjectArray(defaultElementType);
		case SerializedType.NonOptimizedTypedArrayType:
		case SerializedType.FullyOptimizedTypedArrayType:
		case SerializedType.PartiallyOptimizedTypedArrayType:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(typeCode);
			int num = ReadOptimizedInt32();
			if (defaultElementType == null)
			{
				defaultElementType = ReadTokenizedType();
			}
			Array array = Array.CreateInstance(defaultElementType, num);
			for (int i = 0; i < num; i++)
			{
				if (bitArray == null)
				{
					array.SetValue(ReadObject(), i);
				}
				else if (bitArray == FullyOptimizableTypedArray || !bitArray[i])
				{
					IOwnedDataSerializable ownedDataSerializable = (IOwnedDataSerializable)Activator.CreateInstance(defaultElementType);
					ReadOwnedData(ownedDataSerializable, null);
					array.SetValue(ownedDataSerializable, i);
				}
			}
			return array;
		}
		default:
			return null;
		}
	}

	/// <summary>
	/// Returns the string value associated with the string token read next from the stream.
	/// </summary>
	/// <returns>A DateTime value.</returns>
	private string readTokenizedString(int bucket)
	{
		int num = ReadOptimizedInt32();
		return TokenStrings[(num << 7) + bucket];
	}

	/// <summary>
	/// Returns the SerializedType read next from the stream.
	/// </summary>
	/// <returns>A SerializedType value.</returns>
	private SerializedType readTypeCode()
	{
		return (SerializedType)ReadByte();
	}

	/// <summary>
	/// Internal implementation returning a Bool[].
	/// </summary>
	/// <returns>A Bool[].</returns>
	private bool[] readBooleanArray()
	{
		BitArray bitArray = ReadOptimizedBitArray();
		bool[] array = new bool[bitArray.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = bitArray[i];
		}
		return array;
	}

	/// <summary>
	/// Internal implementation returning a Byte[].
	/// </summary>
	/// <returns>A Byte[].</returns>
	private byte[] readByteArray()
	{
		return base.ReadBytes(ReadOptimizedInt32());
	}

	/// <summary>
	/// Internal implementation returning a Char[].
	/// </summary>
	/// <returns>A Char[].</returns>
	private char[] readCharArray()
	{
		return base.ReadChars(ReadOptimizedInt32());
	}

	/// <summary>
	/// Internal implementation returning a Decimal[].
	/// </summary>
	/// <returns>A Decimal[].</returns>
	private decimal[] readDecimalArray()
	{
		decimal[] array = new decimal[ReadOptimizedInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadOptimizedDecimal();
		}
		return array;
	}

	/// <summary>
	/// Internal implementation returning a Double[].
	/// </summary>
	/// <returns>A Double[].</returns>
	private double[] readDoubleArray()
	{
		double[] array = new double[ReadOptimizedInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadDouble();
		}
		return array;
	}

	/// <summary>
	/// Internal implementation returning a Guid[].
	/// </summary>
	/// <returns>A Guid[].</returns>
	private Guid[] readGuidArray()
	{
		Guid[] array = new Guid[ReadOptimizedInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadGuid();
		}
		return array;
	}

	/// <summary>
	/// Internal implementation returning an SByte[].
	/// </summary>
	/// <returns>An SByte[].</returns>
	private sbyte[] readSByteArray()
	{
		sbyte[] array = new sbyte[ReadOptimizedInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadSByte();
		}
		return array;
	}

	/// <summary>
	/// Internal implementation returning a Single[].
	/// </summary>
	/// <returns>A Single[].</returns>
	private float[] readSingleArray()
	{
		float[] array = new float[ReadOptimizedInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadSingle();
		}
		return array;
	}

	[Conditional("DEBUG")]
	public void DumpStringTables(ArrayList list)
	{
		list.AddRange(TokenStrings);
	}
}
