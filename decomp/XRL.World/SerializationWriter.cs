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
using XRL.Collections;
using XRL.Serialization;

namespace XRL.World;

/// <summary>
/// A SerializationWriter instance is used to store values and objects in a byte array.
///
/// Once an instance is created, use the various methods to store the required data.
/// ToArray() will return a byte[] containing all of the data required for deserialization.
/// This can be stored in the SerializationInfo parameter in an ISerializable.GetObjectData() method.
/// <para />
/// As an alternative to ToArray(), if you want to apply some post-processing to the serialized bytes, 
/// such as compression, call AppendTokenTables first to ensure that the string and object token tables 
/// are appended to the stream, and then cast BaseStream to MemoryStream. You can then access the
/// MemoryStream's internal buffer as follows:
/// <para />
/// <example><code>
/// writer.AppendTokenTables();
/// MemoryStream stream = (MemoryStream) writer.BaseStream;
///             	serializedData = MiniLZO.Compress(stream.GetBuffer(), (int) stream.Length);
/// </code></example>
/// </summary>
public sealed class SerializationWriter : BinaryWriter
{
	/// <summary>
	/// Private class used to wrap an object that is to be tokenized, and recreated at deserialization by its type.
	/// </summary>
	private class SingletonTypeWrapper : IEquatable<SingletonTypeWrapper>
	{
		public Type WrappedType;

		public SingletonTypeWrapper()
		{
		}

		public SingletonTypeWrapper(SingletonTypeWrapper Wrapper)
		{
			WrappedType = Wrapper.WrappedType;
		}

		public bool Equals(SingletonTypeWrapper other)
		{
			if (other != null)
			{
				return (object)WrappedType == other.WrappedType;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is SingletonTypeWrapper singletonTypeWrapper)
			{
				return (object)WrappedType == singletonTypeWrapper.WrappedType;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return WrappedType.GetHashCode();
		}
	}

	public class Block : IDisposable
	{
		private SerializationWriter Writer;

		private Stream ParentStream;

		private MemoryStream Stream = new MemoryStream();

		public void Start(SerializationWriter Writer)
		{
			this.Writer = Writer;
			ParentStream = Writer.OutStream;
			Writer.OutStream = Stream;
		}

		public void End()
		{
			if (ParentStream != null)
			{
				Writer.OutStream = ParentStream;
				int num = (int)Stream.Position;
				Writer.WriteOptimized(num);
				if (num > 0)
				{
					byte[] buffer = Stream.GetBuffer();
					ParentStream.Write(buffer, 0, num);
					Stream.Position = 0L;
				}
				ParentStream = null;
			}
		}

		public void Reset()
		{
			Stream.Position = 0L;
		}

		public void Dispose()
		{
			if (Writer != null)
			{
				End();
				Writer.Blocks.Push(this);
				Writer = null;
			}
		}
	}

	/// <summary>
	/// Provides a faster way to store string tokens both maintaining the order that they were added and
	/// providing a fast lookup.
	///
	/// Based on code developed by ewbi at http://ewbi.blogs.com/develops/2006/10/uniquestringlis.html
	/// </summary>
	private sealed class UniqueStringList
	{
		private const float LoadFactor = 0.72f;

		private static readonly int[] primeNumberList = new int[19]
		{
			389, 1543, 6151, 24593, 98317, 196613, 393241, 786433, 1572869, 3145739,
			6291469, 12582917, 25165843, 50331653, 100663319, 201326611, 402653189, 805306457, 1610612741
		};

		private string[] stringList;

		private int[] buckets;

		private int bucketListCapacity;

		private int stringListIndex;

		private int loadLimit;

		private int primeNumberListIndex;

		public string this[int index] => stringList[index];

		public int Count => stringListIndex;

		public UniqueStringList()
		{
			bucketListCapacity = primeNumberList[primeNumberListIndex++];
			stringList = new string[bucketListCapacity];
			buckets = new int[bucketListCapacity];
			loadLimit = (int)((float)bucketListCapacity * 0.72f);
		}

		public int Add(string value)
		{
			int bucketIndex = getBucketIndex(value);
			int num = buckets[bucketIndex];
			if (num == 0)
			{
				stringList[stringListIndex++] = value;
				buckets[bucketIndex] = stringListIndex;
				if (stringListIndex > loadLimit)
				{
					expand();
				}
				return stringListIndex - 1;
			}
			return num - 1;
		}

		public void Clear()
		{
			Array.Clear(buckets, 0, buckets.Length);
			Array.Clear(stringList, 0, stringList.Length);
			stringListIndex = 0;
		}

		private void expand()
		{
			bucketListCapacity = primeNumberList[primeNumberListIndex++];
			buckets = new int[bucketListCapacity];
			string[] array = new string[bucketListCapacity];
			stringList.CopyTo(array, 0);
			stringList = array;
			reindex();
		}

		private void reindex()
		{
			loadLimit = (int)((float)bucketListCapacity * 0.72f);
			for (int i = 0; i < stringListIndex; i++)
			{
				int bucketIndex = getBucketIndex(stringList[i]);
				buckets[bucketIndex] = i + 1;
			}
		}

		private int getBucketIndex(string value)
		{
			int num = (value.GetHashCode() & 0x7FFFFFFF) % bucketListCapacity;
			int num2 = ((num <= 1) ? 1 : num);
			int num3 = bucketListCapacity;
			while (0 < num3--)
			{
				int num4 = buckets[num];
				if (num4 == 0)
				{
					return num;
				}
				if (string.CompareOrdinal(value, stringList[num4 - 1]) == 0)
				{
					return num;
				}
				num = (num + num2) % bucketListCapacity;
			}
			throw new InvalidOperationException("Failed to locate a bucket.");
		}
	}

	/// <summary>
	/// Default capacity for the underlying MemoryStream
	/// </summary>
	public static int DefaultCapacity = 1024;

	/// <summary>
	/// The Default setting for the OptimizeForSize property.
	/// </summary>
	public static bool DefaultOptimizeForSize = true;

	/// <summary>
	/// The Default setting for the PreserveDecimalScale property.
	/// </summary>
	public static bool DefaultPreserveDecimalScale = false;

	private static List<IFastSerializationTypeSurrogate> typeSurrogates = null;

	private static HashSet<Type> BinaryFormattedTypes = new HashSet<Type>();

	/// <summary>
	/// Section masks used for packing DateTime values
	/// </summary>
	internal static readonly BitVector32.Section DateYearMask = BitVector32.CreateSection(127);

	internal static readonly BitVector32.Section DateMonthMask = BitVector32.CreateSection(12, DateYearMask);

	internal static readonly BitVector32.Section DateDayMask = BitVector32.CreateSection(31, DateMonthMask);

	internal static readonly BitVector32.Section DateHasTimeOrKindMask = BitVector32.CreateSection(1, DateDayMask);

	/// <summary>
	/// Section masks used for packing TimeSpan values
	/// </summary>
	internal static readonly BitVector32.Section IsNegativeSection = BitVector32.CreateSection(1);

	internal static readonly BitVector32.Section HasDaysSection = BitVector32.CreateSection(1, IsNegativeSection);

	internal static readonly BitVector32.Section HasTimeSection = BitVector32.CreateSection(1, HasDaysSection);

	internal static readonly BitVector32.Section HasSecondsSection = BitVector32.CreateSection(1, HasTimeSection);

	internal static readonly BitVector32.Section HasMillisecondsSection = BitVector32.CreateSection(1, HasSecondsSection);

	internal static readonly BitVector32.Section HoursSection = BitVector32.CreateSection(23, HasMillisecondsSection);

	internal static readonly BitVector32.Section MinutesSection = BitVector32.CreateSection(59, HoursSection);

	internal static readonly BitVector32.Section SecondsSection = BitVector32.CreateSection(59, MinutesSection);

	internal static readonly BitVector32.Section MillisecondsSection = BitVector32.CreateSection(127, SecondsSection);

	/// <summary>
	/// Holds the highest Int16 that can be optimized into less than the normal 2 bytes
	/// </summary>
	public const short HighestOptimizable16BitValue = 127;

	/// <summary>
	/// Holds the highest Int32 that can be optimized into less than the normal 4 bytes
	/// </summary>
	public const int HighestOptimizable32BitValue = 2097151;

	/// <summary>
	/// Holds the highest Int64 that can be optimized into less than the normal 8 bytes
	/// </summary>
	public const long HighestOptimizable64BitValue = 562949953421311L;

	internal const short OptimizationFailure16BitValue = 16384;

	internal const int OptimizationFailure32BitValue = 268435456;

	internal const long OptimizationFailure64BitValue = 72057594037927936L;

	internal const ushort GameObjectStartValue = 64206;

	internal const ushort GameObjectEndValue = 44203;

	internal const ushort GameObjectFinishValue = 52958;

	private static readonly BitArray FullyOptimizableTypedArray = new BitArray(0);

	private static SerializationWriter Instance = new SerializationWriter(FastSerialization.SharedCache);

	public MemoryStream Stream;

	private byte[] Buffer;

	private Stack<Block> Blocks;

	private HashSet<Assembly> LocalAssemblies;

	private UniqueStringList TokenStrings;

	private Rack<object> TokenObjects;

	private Dictionary<object, int> TokenByObject;

	private Rack<ITokenized> Tokenized;

	private Rack<Type> TokenTypes;

	private Dictionary<Type, int> TokenByType;

	private Rack<GameObject> TokenGameObjects;

	private Dictionary<GameObject, int> TokenByGameObject;

	private Rack<EventRegistry> EventRegistries;

	private Rack<GameObjectReference> GameObjectReferences;

	private long StringPosition;

	private long ObjectPosition;

	private long TokenizedPosition;

	private long TypePosition;

	private long GameObjectPosition;

	private long GameObjectReferencePosition;

	private long EventRegistryPosition;

	public int FileVersion;

	public bool SerializePlayer = true;

	private bool optimizeForSize = DefaultOptimizeForSize;

	private bool preserveDecimalScale = DefaultPreserveDecimalScale;

	private SingletonTypeWrapper WrapperCache;

	private static BinaryFormatter formatter = null;

	/// <summary>
	/// Holds a list of optional IFastSerializationTypeSurrogate instances which
	/// SerializationWriter and SerializationReader will use to serialize objects
	/// not directly supported.
	/// It is important to use the same list on both client and server ends to ensure
	/// that the same surrogated-types are supported.
	/// </summary>
	public static List<IFastSerializationTypeSurrogate> TypeSurrogates
	{
		get
		{
			if (typeSurrogates == null)
			{
				typeSurrogates = new List<IFastSerializationTypeSurrogate>();
				typeSurrogates.Add(new ColorSerializationSurrogate());
			}
			return typeSurrogates;
		}
	}

	/// <summary>
	/// Gets or Sets a boolean flag to indicate whether to optimize for size (default)
	/// by storing data as packed bits or sections where possible.
	/// Setting this value to false will turn off this optimization and store
	/// data directly which increases the speed.
	/// Note: This only affects optimization of data passed to the WriteObject method
	/// and direct calls to the WriteOptimized methods will always pack data into
	/// the smallest space where possible.
	/// </summary>
	public bool OptimizeForSize
	{
		get
		{
			return optimizeForSize;
		}
		set
		{
			optimizeForSize = value;
		}
	}

	/// <summary>
	/// Gets or Sets a boolean flag to indicate whether to preserve the scale within
	/// a Decimal value when it would have no effect on the represented value.
	/// Note: a 2m value and a 2.00m value represent the same value but internally they 
	/// are stored differently - the former has a value of 2 and a scale of 0 and
	/// the latter has a value of 200 and a scale of 2. 
	/// The scaling factor also preserves any trailing zeroes in a Decimal number. 
	/// Trailing zeroes do not affect the value of a Decimal number in arithmetic or 
	/// comparison operations. However, trailing zeroes can be revealed by the ToString 
	/// method if an appropriate format string is applied.
	/// From a serialization point of view, the former will take 2 bytes whereas the 
	/// latter would take 4 bytes, therefore it is preferable to not save the scale where
	/// it doesn't affect the represented value.
	/// </summary>
	public bool PreserveDecimalScale
	{
		get
		{
			return preserveDecimalScale;
		}
		set
		{
			preserveDecimalScale = value;
		}
	}

	public long Position
	{
		get
		{
			return OutStream.Position;
		}
		set
		{
			OutStream.Position = value;
		}
	}

	public static bool TryGetShared(out SerializationWriter Writer)
	{
		GameManager.IsOnGameContext();
		if (FastSerialization.SharedCache.Reserved)
		{
			Writer = null;
			return false;
		}
		Writer = Instance;
		return FastSerialization.SharedCache.Reserved = true;
	}

	public static void ReleaseShared()
	{
		Instance.Clear();
		FastSerialization.SharedCache.Reserved = false;
	}

	public static SerializationWriter Get()
	{
		if (TryGetShared(out var Writer))
		{
			return Writer;
		}
		return new SerializationWriter(new FastSerialization.Cache(0));
	}

	public static void Release(SerializationWriter Writer)
	{
		if (Writer == Instance)
		{
			ReleaseShared();
		}
		else
		{
			Writer.Dispose();
		}
	}

	/// <summary>
	/// Creates a FastSerializer around the specified stream
	/// Note: The stream must be seekable in this version to allow the token table 
	/// offset to be written on completion 
	/// </summary>
	/// <param name="stream">The seekable stream in which to store data</param>
	/// <param name="FileVersion">The current serialization version, used for upgrading saves to newer versions.</param>
	/// <param name="SerializePlayer">
	/// Whether the player's game object should be serialized,
	/// typically true when saving the entire game and false when saving individual zones.
	/// </param>
	public SerializationWriter(MemoryStream Stream)
		: base(Stream, FastSerialization.Encoding)
	{
		this.Stream = Stream;
		if (!Stream.CanSeek)
		{
			throw new InvalidOperationException("Stream must be seekable");
		}
	}

	public SerializationWriter(FastSerialization.Cache Cache)
		: this(Cache.MemoryStream)
	{
		Initialize(Cache);
	}

	public void Start(int FileVersion, bool SerializePlayer = false)
	{
		this.FileVersion = FileVersion;
		this.SerializePlayer = SerializePlayer;
		ReserveHeader();
		LocalAssemblies.Add(typeof(int).Assembly);
		LocalAssemblies.Add(typeof(GameObject).Assembly);
		foreach (Assembly modAssembly in ModManager.ModAssemblies)
		{
			LocalAssemblies.Add(modAssembly);
		}
		SetPlayer(The.Player);
		WriteModVersions();
		WriteExtensionHash();
	}

	public void Initialize(FastSerialization.Cache Cache)
	{
		if (Buffer == null)
		{
			Buffer = new byte[16];
			Blocks = new Stack<Block>();
			TokenObjects = Cache.Objects;
			TokenByObject = new Dictionary<object, int>();
			TokenStrings = new UniqueStringList();
			TokenTypes = Cache.Types;
			TokenByType = new Dictionary<Type, int>(768);
			TokenGameObjects = Cache.GameObjects;
			GameObjectReferences = Cache.GameObjectReferences;
			TokenByGameObject = new Dictionary<GameObject, int>(2560);
			EventRegistries = Cache.EventRegistries;
			Tokenized = Cache.Tokenized;
			formatter = null;
			LocalAssemblies = new HashSet<Assembly>(ModManager.ModAssemblies.Count + 2);
		}
	}

	public void Clear()
	{
		Stream.SetLength(0L);
		TokenObjects.Clear();
		TokenByObject.Clear();
		TokenStrings.Clear();
		TokenTypes.Clear();
		TokenByType.Clear();
		TokenGameObjects.Clear();
		GameObjectReferences.Clear();
		TokenByGameObject.Clear();
		EventRegistries.Clear();
		Tokenized.Clear();
		LocalAssemblies.Clear();
		formatter = null;
	}

	public void SetPlayer(GameObject Object)
	{
		if (TokenGameObjects.Count == 0)
		{
			TokenGameObjects.Add(Object);
		}
		else
		{
			TokenGameObjects[0] = Object;
		}
		if (Object != null)
		{
			TokenByGameObject[Object] = 0;
		}
	}

	/// <summary>
	/// Writes an ArrayList into the stream using the fewest number of bytes possible.
	/// Stored Size: 1 byte upwards depending on data content
	/// Notes:
	/// A null Arraylist takes 1 byte.
	/// An empty ArrayList takes 2 bytes.
	/// The contents are stored using WriteOptimized(ArrayList) which should be used
	/// if the ArrayList is guaranteed never to be null.
	/// </summary>
	/// <param name="value">The ArrayList to store.</param>
	public void Write(ArrayList value)
	{
		if (value == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		writeTypeCode(SerializedType.ArrayListType);
		WriteOptimized(value);
	}

	/// <summary>
	/// Writes a BitArray value into the stream using the fewest number of bytes possible.
	/// Stored Size: 1 byte upwards depending on data content
	/// Notes:
	/// A null BitArray takes 1 byte.
	/// An empty BitArray takes 2 bytes.
	/// </summary>
	/// <param name="value">The BitArray value to store.</param>
	public void Write(BitArray value)
	{
		if (value == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		writeTypeCode(SerializedType.BitArrayType);
		WriteOptimized(value);
	}

	/// <summary>
	/// Writes a BitVector32 into the stream.
	/// Stored Size: 4 bytes.
	/// </summary>
	/// <param name="value">The BitVector32 to store.</param>
	public void Write(BitVector32 value)
	{
		base.Write(value.Data);
	}

	/// <summary>
	/// Writes a DateTime value into the stream.
	/// Stored Size: 8 bytes
	/// </summary>
	/// <param name="value">The DateTime value to store.</param>
	public void Write(DateTime value)
	{
		Write(value.ToBinary());
	}

	/// <summary>
	/// Writes a Guid into the stream.
	/// Stored Size: 16 bytes.
	/// </summary>
	/// <param name="value" />
	public void Write(Guid value)
	{
		Span<byte> destination = Buffer.AsSpan(0, 16);
		if (value.TryWriteBytes(destination))
		{
			OutStream.Write(Buffer, 0, 16);
		}
		else
		{
			base.Write(value.ToByteArray());
		}
	}

	/// <summary>
	/// Allows any object implementing IOwnedDataSerializable to serialize itself
	/// into this SerializationWriter.
	/// A context may also be used to give the object an indication of what data
	/// to store. As an example, using a BitVector32 gives a list of flags and
	/// the object can conditionally store data depending on those flags.
	/// </summary>
	/// <param name="target">The IOwnedDataSerializable object to ask for owned data</param>
	/// <param name="context">An arbtritrary object but BitVector32 recommended</param>
	public void Write(IOwnedDataSerializable target, object context)
	{
		target.SerializeOwnedData(this, context);
	}

	/// <summary>
	/// Stores an object into the stream using the fewest number of bytes possible.
	/// Stored Size: 1 byte upwards depending on type and/or content.
	///
	/// 1 byte: null, DBNull.Value, Boolean
	///
	/// 1 to 2 bytes: Int16, UInt16, Byte, SByte, Char, 
	///
	/// 1 to 4 bytes: Int32, UInt32, Single, BitVector32
	///
	/// 1 to 8 bytes: DateTime, TimeSpan, Double, Int64, UInt64
	///
	/// 1 or 16 bytes: Guid
	///
	/// 1 plus content: string, object[], byte[], char[], BitArray, Type, ArrayList
	///
	/// Any other object be stored using a .Net Binary formatter but this should 
	/// only be allowed as a last resort:
	/// Since this is effectively a different serialization session, there is a 
	/// possibility of the same shared object being serialized twice or, if the 
	/// object has a reference directly or indirectly back to the parent object, 
	/// there is a risk of looping which will throw an exception.
	///
	/// The type of object is checked with the most common types being checked first.
	/// Each 'section' can be reordered to provide optimum speed but the check for
	/// null should always be first and the default serialization always last.
	///
	/// Once the type is identified, a SerializedType byte is stored in the stream
	/// followed by the data for the object (certain types/values may not require
	/// storage of data as the SerializedType may imply the value).
	///
	/// For certain objects, if the value is within a certain range then optimized
	/// storage may be used. If the value doesn't meet the required optimization
	/// criteria then the value is stored directly.
	/// The checks for optimization may be disabled by setting the OptimizeForSize
	/// property to false in which case the value is stored directly. This could 
	/// result in a slightly larger stream but there will be a speed increate to
	/// compensate.
	/// </summary>
	/// <param name="value">The object to store.</param>
	public void WriteObject(object value)
	{
		try
		{
			if (value == null)
			{
				writeTypeCode(SerializedType.NullType);
				return;
			}
			if (value is string)
			{
				WriteOptimized((string)value);
				return;
			}
			if (value is int num)
			{
				switch (num)
				{
				case 0:
					writeTypeCode(SerializedType.ZeroInt32Type);
					return;
				case -1:
					writeTypeCode(SerializedType.MinusOneInt32Type);
					return;
				case 1:
					writeTypeCode(SerializedType.OneInt32Type);
					return;
				}
				if (optimizeForSize)
				{
					if (num > 0)
					{
						if (num <= 2097151)
						{
							writeTypeCode(SerializedType.OptimizedInt32Type);
							write7bitEncodedSigned32BitValue(num);
							return;
						}
					}
					else
					{
						int num2 = -(num + 1);
						if (num2 <= 2097151)
						{
							writeTypeCode(SerializedType.OptimizedInt32NegativeType);
							write7bitEncodedSigned32BitValue(num2);
							return;
						}
					}
				}
				writeTypeCode(SerializedType.Int32Type);
				Write(num);
				return;
			}
			if (value == DBNull.Value)
			{
				writeTypeCode(SerializedType.DBNullType);
				return;
			}
			if (value is bool)
			{
				writeTypeCode(((bool)value) ? SerializedType.BooleanTrueType : SerializedType.BooleanFalseType);
				return;
			}
			if (value is decimal num3)
			{
				if (num3 == 0m)
				{
					writeTypeCode(SerializedType.ZeroDecimalType);
					return;
				}
				if (num3 == 1m)
				{
					writeTypeCode(SerializedType.OneDecimalType);
					return;
				}
				writeTypeCode(SerializedType.DecimalType);
				WriteOptimized(num3);
				return;
			}
			if (value is DateTime dateTime)
			{
				if (dateTime == DateTime.MinValue)
				{
					writeTypeCode(SerializedType.MinDateTimeType);
				}
				else if (dateTime == DateTime.MaxValue)
				{
					writeTypeCode(SerializedType.MaxDateTimeType);
				}
				else if (optimizeForSize && dateTime.Ticks % 10000 == 0L)
				{
					writeTypeCode(SerializedType.OptimizedDateTimeType);
					WriteOptimized(dateTime);
				}
				else
				{
					writeTypeCode(SerializedType.DateTimeType);
					Write(dateTime);
				}
				return;
			}
			if (value is double num4)
			{
				if (num4 == 0.0)
				{
					writeTypeCode(SerializedType.ZeroDoubleType);
					return;
				}
				if (num4 == 1.0)
				{
					writeTypeCode(SerializedType.OneDoubleType);
					return;
				}
				writeTypeCode(SerializedType.DoubleType);
				Write(num4);
				return;
			}
			if (value is float num5)
			{
				if (num5 == 0f)
				{
					writeTypeCode(SerializedType.ZeroSingleType);
					return;
				}
				if (num5 == 1f)
				{
					writeTypeCode(SerializedType.OneSingleType);
					return;
				}
				writeTypeCode(SerializedType.SingleType);
				Write(num5);
				return;
			}
			if (value is short num6)
			{
				switch (num6)
				{
				case 0:
					writeTypeCode(SerializedType.ZeroInt16Type);
					return;
				case -1:
					writeTypeCode(SerializedType.MinusOneInt16Type);
					return;
				case 1:
					writeTypeCode(SerializedType.OneInt16Type);
					return;
				}
				if (optimizeForSize)
				{
					if (num6 > 0)
					{
						if (num6 <= 127)
						{
							writeTypeCode(SerializedType.OptimizedInt16Type);
							write7bitEncodedSigned32BitValue(num6);
							return;
						}
					}
					else
					{
						int num7 = -(num6 + 1);
						if (num7 <= 127)
						{
							writeTypeCode(SerializedType.OptimizedInt16NegativeType);
							write7bitEncodedSigned32BitValue(num7);
							return;
						}
					}
				}
				writeTypeCode(SerializedType.Int16Type);
				Write(num6);
				return;
			}
			if (value is Guid guid)
			{
				if (guid == Guid.Empty)
				{
					writeTypeCode(SerializedType.EmptyGuidType);
					return;
				}
				writeTypeCode(SerializedType.GuidType);
				Write(guid);
				return;
			}
			if (value is long num8)
			{
				switch (num8)
				{
				case 0L:
					writeTypeCode(SerializedType.ZeroInt64Type);
					return;
				case -1L:
					writeTypeCode(SerializedType.MinusOneInt64Type);
					return;
				case 1L:
					writeTypeCode(SerializedType.OneInt64Type);
					return;
				}
				if (optimizeForSize)
				{
					if (num8 > 0)
					{
						if (num8 <= 562949953421311L)
						{
							writeTypeCode(SerializedType.OptimizedInt64Type);
							write7bitEncodedSigned64BitValue(num8);
							return;
						}
					}
					else
					{
						long num9 = -(num8 + 1);
						if (num9 <= 562949953421311L)
						{
							writeTypeCode(SerializedType.OptimizedInt64NegativeType);
							write7bitEncodedSigned64BitValue(num9);
							return;
						}
					}
				}
				writeTypeCode(SerializedType.Int64Type);
				Write(num8);
				return;
			}
			if (value is byte b)
			{
				switch (b)
				{
				case 0:
					writeTypeCode(SerializedType.ZeroByteType);
					break;
				case 1:
					writeTypeCode(SerializedType.OneByteType);
					break;
				default:
					writeTypeCode(SerializedType.ByteType);
					Write(b);
					break;
				}
				return;
			}
			if (value is char c)
			{
				switch (c)
				{
				case '\0':
					writeTypeCode(SerializedType.ZeroCharType);
					break;
				case '\u0001':
					writeTypeCode(SerializedType.OneCharType);
					break;
				default:
					writeTypeCode(SerializedType.CharType);
					Write(c);
					break;
				}
				return;
			}
			if (value is sbyte b2)
			{
				switch (b2)
				{
				case 0:
					writeTypeCode(SerializedType.ZeroSByteType);
					break;
				case 1:
					writeTypeCode(SerializedType.OneSByteType);
					break;
				default:
					writeTypeCode(SerializedType.SByteType);
					Write(b2);
					break;
				}
				return;
			}
			if (value is uint num10)
			{
				switch (num10)
				{
				case 0u:
					writeTypeCode(SerializedType.ZeroUInt32Type);
					return;
				case 1u:
					writeTypeCode(SerializedType.OneUInt32Type);
					return;
				}
				if (optimizeForSize && num10 <= 2097151)
				{
					writeTypeCode(SerializedType.OptimizedUInt32Type);
					write7bitEncodedUnsigned32BitValue(num10);
				}
				else
				{
					writeTypeCode(SerializedType.UInt32Type);
					Write(num10);
				}
				return;
			}
			if (value is ushort num11)
			{
				switch (num11)
				{
				case 0:
					writeTypeCode(SerializedType.ZeroUInt16Type);
					return;
				case 1:
					writeTypeCode(SerializedType.OneUInt16Type);
					return;
				}
				if (optimizeForSize && num11 <= 127)
				{
					writeTypeCode(SerializedType.OptimizedUInt16Type);
					write7bitEncodedUnsigned32BitValue(num11);
				}
				else
				{
					writeTypeCode(SerializedType.UInt16Type);
					Write(num11);
				}
				return;
			}
			if (value is ulong num12)
			{
				switch (num12)
				{
				case 0uL:
					writeTypeCode(SerializedType.ZeroUInt64Type);
					return;
				case 1uL:
					writeTypeCode(SerializedType.OneUInt64Type);
					return;
				}
				if (optimizeForSize && num12 <= 562949953421311L)
				{
					writeTypeCode(SerializedType.OptimizedUInt64Type);
					WriteOptimized(num12);
				}
				else
				{
					writeTypeCode(SerializedType.UInt64Type);
					Write(num12);
				}
				return;
			}
			if (value is TimeSpan timeSpan)
			{
				if (timeSpan == TimeSpan.Zero)
				{
					writeTypeCode(SerializedType.ZeroTimeSpanType);
				}
				else if (optimizeForSize && timeSpan.Ticks % 10000 == 0L)
				{
					writeTypeCode(SerializedType.OptimizedTimeSpanType);
					WriteOptimized(timeSpan);
				}
				else
				{
					writeTypeCode(SerializedType.TimeSpanType);
					Write(timeSpan);
				}
				return;
			}
			if (value is GameObject gameObject)
			{
				writeTypeCode(SerializedType.GameObjectType);
				WriteGameObject(gameObject);
				return;
			}
			if (value is IComposite value2)
			{
				SerializeComposite(value2);
				return;
			}
			if (value is Array { Rank: 1 } array)
			{
				writeTypedArray(array, storeType: true);
				return;
			}
			if (value is Type type)
			{
				writeTypeCode(SerializedType.TypeType);
				WriteTokenized(type);
				return;
			}
			if (value is BitArray)
			{
				writeTypeCode(SerializedType.BitArrayType);
				WriteOptimized((BitArray)value);
				return;
			}
			if (value is BitVector32)
			{
				writeTypeCode(SerializedType.BitVector32Type);
				Write((BitVector32)value);
				return;
			}
			if (value is SingletonTypeWrapper)
			{
				writeTypeCode(SerializedType.SingleInstanceType);
				WriteTokenized(((SingletonTypeWrapper)value).WrappedType);
				return;
			}
			if (value is List<string>)
			{
				writeTypeCode(SerializedType.StringListType);
				Write(value as List<string>);
				return;
			}
			if (value is ArrayList)
			{
				writeTypeCode(SerializedType.ArrayListType);
				WriteOptimized(value as ArrayList);
				return;
			}
			if (value is IDictionary)
			{
				writeTypeCode(SerializedType.IDictionaryType);
				WriteTokenized(value.GetType());
				Write((IDictionary)value);
				return;
			}
			if (value is IList && !(value is Array))
			{
				writeTypeCode(SerializedType.IListType);
				WriteTokenized(value.GetType());
				Write((IList)value);
				return;
			}
			if (value is Enum)
			{
				Type type2 = value.GetType();
				Type underlyingType = Enum.GetUnderlyingType(type2);
				if (underlyingType == typeof(int) || underlyingType == typeof(uint))
				{
					uint num13 = ((underlyingType == typeof(int)) ? ((uint)(int)value) : ((uint)value));
					if (num13 <= 2097151)
					{
						writeTypeCode(SerializedType.OptimizedEnumType);
						WriteTokenized(type2);
						write7bitEncodedUnsigned32BitValue(num13);
					}
					else
					{
						writeTypeCode(SerializedType.EnumType);
						WriteTokenized(type2);
						Write(num13);
					}
					return;
				}
				if (underlyingType == typeof(long) || underlyingType == typeof(ulong))
				{
					ulong num14 = ((underlyingType == typeof(long)) ? ((ulong)(long)value) : ((ulong)value));
					if (num14 <= 562949953421311L)
					{
						writeTypeCode(SerializedType.OptimizedEnumType);
						WriteTokenized(type2);
						write7bitEncodedUnsigned64BitValue(num14);
					}
					else
					{
						writeTypeCode(SerializedType.EnumType);
						WriteTokenized(type2);
						Write(num14);
					}
					return;
				}
				writeTypeCode(SerializedType.EnumType);
				WriteTokenized(type2);
				if (underlyingType == typeof(byte))
				{
					Write((byte)value);
				}
				else if (underlyingType == typeof(sbyte))
				{
					Write((sbyte)value);
				}
				else if (underlyingType == typeof(short))
				{
					Write((short)value);
				}
				else
				{
					Write((ushort)value);
				}
				return;
			}
			Type type3 = value.GetType();
			if (isTypeRecreatable(type3))
			{
				writeTypeCode(SerializedType.OwnedDataSerializableAndRecreatableType);
				WriteTokenized(type3);
				Write((IOwnedDataSerializable)value, null);
				return;
			}
			IFastSerializationTypeSurrogate fastSerializationTypeSurrogate;
			if ((fastSerializationTypeSurrogate = findSurrogateForType(type3)) != null)
			{
				writeTypeCode(SerializedType.SurrogateHandledType);
				WriteTokenized(type3);
				fastSerializationTypeSurrogate.Serialize(this, value);
				return;
			}
			try
			{
				writeTypeCode(SerializedType.OtherType);
				createBinaryFormatter().Serialize(OutStream, value);
				if (BinaryFormattedTypes.Add(type3) && type3.Assembly != Assembly.GetExecutingAssembly())
				{
					MetricsManager.LogAssemblyError(type3, $"Using binary formatter for type '{type3}', convert it to an IComposite or serialize it manually.");
				}
			}
			catch (Exception innerException)
			{
				throw new Exception($"exception serializing value '{value}' of type '{type3}'", innerException);
			}
		}
		catch (Exception innerException2)
		{
			throw new Exception("exception serializing object " + value.ToString() + " of " + value.GetType().Name + " : ", innerException2);
		}
	}

	/// <summary>
	/// Calls WriteOptimized(string).
	/// This override to hide base BinaryWriter.Write(string).
	/// </summary>
	/// <param name="value">The string to store.</param>
	public override void Write(string value)
	{
		WriteOptimized(value);
	}

	/// <summary>
	/// Writes a TimeSpan value into the stream.
	/// Stored Size: 8 bytes
	/// </summary>
	/// <param name="value">The TimeSpan value to store.</param>
	public void Write(TimeSpan value)
	{
		Write(value.Ticks);
	}

	/// <summary>
	/// Stores a Type object into the stream.
	/// Stored Size: Depends on the length of the Type's name and whether the fullyQualified parameter is set.
	/// A null Type takes 1 byte.
	/// </summary>
	/// <param name="value">The Type to store.</param>
	/// <param name="fullyQualified">true to store the AssemblyQualifiedName or false to store the FullName. </param>
	public void Write(Type value, bool fullyQualified)
	{
		if (value == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		writeTypeCode(SerializedType.TypeType);
		WriteTokenized(value);
	}

	/// <summary>
	/// Writes an non-null ArrayList into the stream using the fewest number of bytes possible.
	/// Stored Size: 1 byte upwards depending on data content
	/// Notes:
	/// An empty ArrayList takes 1 byte.
	/// </summary>
	/// <param name="value">The ArrayList to store. Must not be null.</param>
	public void WriteOptimized(ArrayList value)
	{
		writeObjectArray(value.ToArray());
	}

	/// <summary>
	/// Writes a BitArray into the stream using the fewest number of bytes possible.
	/// Stored Size: 1 byte upwards depending on data content
	/// Notes:
	/// An empty BitArray takes 1 byte.
	/// </summary>
	/// <param name="value">The BitArray value to store. Must not be null.</param>
	public void WriteOptimized(BitArray value)
	{
		write7bitEncodedSigned32BitValue(value.Length);
		if (value.Length > 0)
		{
			byte[] array = new byte[(value.Length + 7) / 8];
			value.CopyTo(array, 0);
			base.Write(array, 0, array.Length);
		}
	}

	/// <summary>
	/// Writes a BitVector32 into the stream using the fewest number of bytes possible.
	/// Stored Size: 1 to 4 bytes. (.Net is 4 bytes)
	///  1 to  7 bits takes 1 byte
	///  8 to 14 bits takes 2 bytes
	/// 15 to 21 bits takes 3 bytes
	/// 22 to 28 bits takes 4 bytes
	/// -------------------------------------------------------------------
	/// 29 to 32 bits takes 5 bytes - use Write(BitVector32) method instead
	///
	/// Try to order the BitVector32 masks so that the highest bits are least-likely
	/// to be set.
	/// </summary>
	/// <param name="value">The BitVector32 to store. Must not use more than 28 bits.</param>
	public void WriteOptimized(BitVector32 value)
	{
		write7bitEncodedSigned32BitValue(value.Data);
	}

	/// <summary>
	/// Writes a DateTime value into the stream using the fewest number of bytes possible.
	/// Stored Size: 3 bytes to 7 bytes (.Net is 8 bytes)
	/// Notes:
	/// A DateTime containing only a date takes 3 bytes
	/// (except a .NET 2.0 Date with a specified DateTimeKind which will take a minimum
	/// of 5 bytes - no further optimization for this situation felt necessary since it
	/// is unlikely that a DateTimeKind would be specified without hh:mm also)
	/// Date plus hh:mm takes 5 bytes.
	/// Date plus hh:mm:ss takes 6 bytes.
	/// Date plus hh:mm:ss.fff takes 7 bytes.
	/// </summary>
	/// <param name="value">The DateTime value to store. Must not contain sub-millisecond data.</param>
	public void WriteOptimized(DateTime value)
	{
		BitVector32 bitVector = new BitVector32
		{
			[DateYearMask] = value.Year,
			[DateMonthMask] = value.Month,
			[DateDayMask] = value.Day
		};
		int num = 0;
		bool flag = value != value.Date;
		num = (int)value.Kind;
		flag |= num != 0;
		bitVector[DateHasTimeOrKindMask] = (flag ? 1 : 0);
		int data = bitVector.Data;
		Write((byte)data);
		Write((byte)(data >> 8));
		Write((byte)(data >> 16));
		if (flag)
		{
			encodeTimeSpan(value.TimeOfDay, partOfDateTime: true, num);
		}
	}

	/// <summary>
	/// Writes a Decimal value into the stream using the fewest number of bytes possible.
	/// Stored Size: 1 byte to 14 bytes (.Net is 16 bytes)
	/// Restrictions: None
	/// </summary>
	/// <param name="value">The Decimal value to store</param>
	public void WriteOptimized(decimal value)
	{
		int[] bits = decimal.GetBits(value);
		byte b = (byte)(bits[3] >> 16);
		byte b2 = 0;
		if (b != 0 && !preserveDecimalScale && optimizeForSize)
		{
			decimal num = decimal.Truncate(value);
			if (num == value)
			{
				bits = decimal.GetBits(num);
				b = 0;
			}
		}
		if ((bits[3] & int.MinValue) != 0)
		{
			b2 |= 1;
		}
		if (b != 0)
		{
			b2 |= 2;
		}
		if (bits[0] == 0)
		{
			b2 |= 4;
		}
		else if (bits[0] <= 2097151 && bits[0] >= 0)
		{
			b2 |= 0x20;
		}
		if (bits[1] == 0)
		{
			b2 |= 8;
		}
		else if (bits[1] <= 2097151 && bits[1] >= 0)
		{
			b2 |= 0x40;
		}
		if (bits[2] == 0)
		{
			b2 |= 0x10;
		}
		else if (bits[2] <= 2097151 && bits[2] >= 0)
		{
			b2 |= 0x80;
		}
		Write(b2);
		if (b != 0)
		{
			Write(b);
		}
		if ((b2 & 4) == 0)
		{
			if ((b2 & 0x20) != 0)
			{
				write7bitEncodedSigned32BitValue(bits[0]);
			}
			else
			{
				Write(bits[0]);
			}
		}
		if ((b2 & 8) == 0)
		{
			if ((b2 & 0x40) != 0)
			{
				write7bitEncodedSigned32BitValue(bits[1]);
			}
			else
			{
				Write(bits[1]);
			}
		}
		if ((b2 & 0x10) == 0)
		{
			if ((b2 & 0x80) != 0)
			{
				write7bitEncodedSigned32BitValue(bits[2]);
			}
			else
			{
				Write(bits[2]);
			}
		}
	}

	/// <summary>
	/// Write an Int16 value using the fewest number of bytes possible.
	/// </summary>
	/// <remarks>
	/// 0x0000 - 0x007f (0 to 127) takes 1 byte
	/// 0x0080 - 0x03FF (128 to 16,383) takes 2 bytes
	/// ----------------------------------------------------------------
	/// 0x0400 - 0x7FFF (16,384 to 32,767) takes 3 bytes
	/// All negative numbers take 3 bytes
	///
	/// Only call this method if the value is known to be between 0 and 
	/// 16,383 otherwise use Write(Int16 value)
	/// </remarks>
	/// <param name="value">The Int16 to store. Must be between 0 and 16,383 inclusive.</param>
	public void WriteOptimized(short value)
	{
		write7bitEncodedSigned32BitValue(value);
	}

	/// <summary>
	/// Write an Int32 value using the fewest number of bytes possible.
	/// </summary>
	/// <remarks>
	/// 0x00000000 - 0x0000007f (0 to 127) takes 1 byte
	/// 0x00000080 - 0x000003FF (128 to 16,383) takes 2 bytes
	/// 0x00000400 - 0x001FFFFF (16,384 to 2,097,151) takes 3 bytes
	/// 0x00200000 - 0x0FFFFFFF (2,097,152 to 268,435,455) takes 4 bytes
	/// ----------------------------------------------------------------
	/// 0x10000000 - 0x07FFFFFF (268,435,456 and above) takes 5 bytes
	/// All negative numbers take 5 bytes
	///
	/// Only call this method if the value is known to be between 0 and 
	/// 268,435,455 otherwise use Write(Int32 value)
	/// </remarks>
	/// <param name="value">The Int32 to store. Must be between 0 and 268,435,455 inclusive.</param>
	public void WriteOptimized(int value)
	{
		write7bitEncodedSigned32BitValue(value);
	}

	/// <summary>
	/// Write an Int64 value using the fewest number of bytes possible.
	/// </summary>
	/// <remarks>
	/// 0x0000000000000000 - 0x000000000000007f (0 to 127) takes 1 byte
	/// 0x0000000000000080 - 0x00000000000003FF (128 to 16,383) takes 2 bytes
	/// 0x0000000000000400 - 0x00000000001FFFFF (16,384 to 2,097,151) takes 3 bytes
	/// 0x0000000000200000 - 0x000000000FFFFFFF (2,097,152 to 268,435,455) takes 4 bytes
	/// 0x0000000010000000 - 0x00000007FFFFFFFF (268,435,456 to 34,359,738,367) takes 5 bytes
	/// 0x0000000800000000 - 0x000003FFFFFFFFFF (34,359,738,368 to 4,398,046,511,103) takes 6 bytes
	/// 0x0000040000000000 - 0x0001FFFFFFFFFFFF (4,398,046,511,104 to 562,949,953,421,311) takes 7 bytes
	/// 0x0002000000000000 - 0x00FFFFFFFFFFFFFF (562,949,953,421,312 to 72,057,594,037,927,935) takes 8 bytes
	/// ------------------------------------------------------------------
	/// 0x0100000000000000 - 0x7FFFFFFFFFFFFFFF (72,057,594,037,927,936 to 9,223,372,036,854,775,807) takes 9 bytes
	/// 0x7FFFFFFFFFFFFFFF - 0xFFFFFFFFFFFFFFFF (9,223,372,036,854,775,807 and above) takes 10 bytes
	/// All negative numbers take 10 bytes
	///
	/// Only call this method if the value is known to be between 0 and
	/// 72,057,594,037,927,935 otherwise use Write(Int64 value)
	/// </remarks>
	/// <param name="value">The Int64 to store. Must be between 0 and 72,057,594,037,927,935 inclusive.</param>
	public void WriteOptimized(long value)
	{
		write7bitEncodedSigned64BitValue(value);
	}

	/// <summary>
	/// Writes a string value into the stream using the fewest number of bytes possible.
	/// Stored Size: 1 byte upwards depending on string length
	/// Notes:
	/// Encodes null, Empty, 'Y', 'N', ' ' values as a single byte
	/// Any other single char string is stored as two bytes
	/// All other strings are stored in a string token list:
	///
	/// The TypeCode representing the current string token list is written first (1 byte), 
	/// followed by the string token itself (1-4 bytes)
	///
	/// When the current string list has reached 128 values then a new string list
	/// is generated and that is used for generating future string tokens. This continues
	/// until the maximum number (128) of string lists is in use, after which the string 
	/// lists are used in a round-robin fashion.
	/// By doing this, more lists are created with fewer items which allows a smaller 
	/// token size to be used for more strings.
	///
	/// The first 16,384 strings will use a 1 byte token.
	/// The next 2,097,152 strings will use a 2 byte token. (This should suffice for most uses!)
	/// The next 268,435,456 strings will use a 3 byte token. (My, that is a lot!!)
	/// The next 34,359,738,368 strings will use a 4 byte token. (only shown for completeness!!!)
	/// </summary>
	/// <param name="value">The string to store.</param>
	public void WriteOptimized(string value)
	{
		if (value == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (value.Length == 1)
		{
			char c = value[0];
			switch (c)
			{
			case 'Y':
				writeTypeCode(SerializedType.YStringType);
				break;
			case 'N':
				writeTypeCode(SerializedType.NStringType);
				break;
			case ' ':
				writeTypeCode(SerializedType.SingleSpaceType);
				break;
			default:
				writeTypeCode(SerializedType.SingleCharStringType);
				Write(c);
				break;
			}
		}
		else if (value.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyStringType);
		}
		else
		{
			int num = TokenStrings.Add(value);
			Write((byte)(num % 128));
			write7bitEncodedSigned32BitValue(num >> 7);
		}
	}

	/// <summary>
	/// Writes a TimeSpan value into the stream using the fewest number of bytes possible.
	/// Stored Size: 2 bytes to 8 bytes (.Net is 8 bytes)
	/// Notes:
	/// hh:mm (time) are always stored together and take 2 bytes.
	/// If seconds are present then 3 bytes unless (time) is not present in which case 2 bytes
	/// since the seconds are stored in the minutes position.
	/// If milliseconds are present then 4 bytes.
	/// In addition, if days are present they will add 1 to 4 bytes to the above.
	/// </summary>
	/// <param name="value">The TimeSpan value to store. Must not contain sub-millisecond data.</param>
	public void WriteOptimized(TimeSpan value)
	{
		encodeTimeSpan(value, partOfDateTime: false, 0);
	}

	/// <summary>
	/// Stores a non-null Type object into the stream.
	/// Stored Size: Depends on the length of the Type's name.
	/// If the type is a System type (mscorlib) then it is stored without assembly name information,
	/// otherwise the Type's AssemblyQualifiedName is used.
	/// </summary>
	/// <param name="value">The Type to store. Must not be null.</param>
	public void WriteOptimized(Type value)
	{
		Assembly assembly = value.Assembly;
		if (LocalAssemblies.Contains(assembly))
		{
			WriteOptimized(value.FullName);
		}
		else
		{
			WriteOptimized(value.AssemblyQualifiedName);
		}
	}

	public void WriteDirect(Type value)
	{
		Assembly assembly = value.Assembly;
		if (LocalAssemblies.Contains(assembly))
		{
			base.Write(value.FullName);
		}
		else
		{
			base.Write(value.AssemblyQualifiedName);
		}
	}

	/// <summary>
	/// Write a UInt16 value using the fewest number of bytes possible.
	/// </summary>
	/// <remarks>
	/// 0x0000 - 0x007f (0 to 127) takes 1 byte
	/// 0x0080 - 0x03FF (128 to 16,383) takes 2 bytes
	/// ----------------------------------------------------------------
	/// 0x0400 - 0xFFFF (16,384 to 65,536) takes 3 bytes
	///
	/// Only call this method if the value is known to  be between 0 and 
	/// 16,383 otherwise use Write(UInt16 value)
	/// </remarks>
	/// <param name="value">The UInt16 to store. Must be between 0 and 16,383 inclusive.</param>
	[CLSCompliant(false)]
	public void WriteOptimized(ushort value)
	{
		write7bitEncodedUnsigned32BitValue(value);
	}

	[CLSCompliant(false)]
	public void WriteOptimized(uint value)
	{
		write7bitEncodedUnsigned32BitValue(value);
	}

	/// <summary>
	/// Write a UInt64 value using the fewest number of bytes possible.
	/// </summary>
	/// <remarks>
	/// 0x0000000000000000 - 0x000000000000007f (0 to 127) takes 1 byte
	/// 0x0000000000000080 - 0x00000000000003FF (128 to 16,383) takes 2 bytes
	/// 0x0000000000000400 - 0x00000000001FFFFF (16,384 to 2,097,151) takes 3 bytes
	/// 0x0000000000200000 - 0x000000000FFFFFFF (2,097,152 to 268,435,455) takes 4 bytes
	/// 0x0000000010000000 - 0x00000007FFFFFFFF (268,435,456 to 34,359,738,367) takes 5 bytes
	/// 0x0000000800000000 - 0x000003FFFFFFFFFF (34,359,738,368 to 4,398,046,511,103) takes 6 bytes
	/// 0x0000040000000000 - 0x0001FFFFFFFFFFFF (4,398,046,511,104 to 562,949,953,421,311) takes 7 bytes
	/// 0x0002000000000000 - 0x00FFFFFFFFFFFFFF (562,949,953,421,312 to 72,057,594,037,927,935) takes 8 bytes
	/// ------------------------------------------------------------------
	/// 0x0100000000000000 - 0x7FFFFFFFFFFFFFFF (72,057,594,037,927,936 to 9,223,372,036,854,775,807) takes 9 bytes
	/// 0x7FFFFFFFFFFFFFFF - 0xFFFFFFFFFFFFFFFF (9,223,372,036,854,775,807 and above) takes 10 bytes
	///
	/// Only call this method if the value is known to be between 0 and
	/// 72,057,594,037,927,935 otherwise use Write(UInt64 value)
	/// </remarks>
	/// <param name="value">The UInt64 to store. Must be between 0 and 72,057,594,037,927,935 inclusive.</param>
	[CLSCompliant(false)]
	public void WriteOptimized(ulong value)
	{
		write7bitEncodedUnsigned64BitValue(value);
	}

	/// <summary>
	/// Writes a Boolean[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// Calls WriteOptimized(Boolean[]).
	/// </summary>
	/// <param name="values">The Boolean[] to store.</param>
	public void Write(bool[] values)
	{
		WriteOptimized(values);
	}

	/// <summary>
	/// Writes a Byte[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Byte[] to store.</param>
	public override void Write(byte[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	/// <summary>
	/// Writes a Char[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Char[] to store.</param>
	public override void Write(char[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	/// <summary>
	/// Writes a DateTime[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The DateTime[] to store.</param>
	public void Write(DateTime[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	/// <summary>
	/// Writes a Decimal[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// Calls WriteOptimized(Decimal[]).
	/// </summary>
	/// <param name="values">The Decimal[] to store.</param>
	public void Write(decimal[] values)
	{
		WriteOptimized(values);
	}

	/// <summary>
	/// Writes a Double[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Double[] to store.</param>
	public void Write(double[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	/// <summary>
	/// Writes a Single[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Single[] to store.</param>
	public void Write(float[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	/// <summary>
	/// Writes a Guid[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Guid[] to store.</param>
	public void Write(Guid[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	/// <summary>
	/// Writes an Int32[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Int32[] to store.</param>
	public void Write(int[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	/// <summary>
	/// Writes an Int64[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Int64[] to store.</param>
	public void Write(long[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	/// <summary>
	/// Writes an object[] into the stream.
	/// Stored Size: 2 bytes upwards depending on data content
	/// Notes:
	/// A null object[] takes 1 byte.
	/// An empty object[] takes 2 bytes.
	/// The contents of the array will be stored optimized.
	/// </summary>
	/// <param name="values">The object[] to store.</param>
	public void Write(object[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyObjectArrayType);
			return;
		}
		writeTypeCode(SerializedType.ObjectArrayType);
		writeObjectArray(values);
	}

	/// <summary>
	/// Writes an SByte[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The SByte[] to store.</param>
	[CLSCompliant(false)]
	public void Write(sbyte[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	/// <summary>
	/// Writes an Int16[]or a null into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// Calls WriteOptimized(decimal[]).
	/// </summary>
	/// <param name="values">The Int16[] to store.</param>
	public void Write(short[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	/// <summary>
	/// Writes a TimeSpan[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The TimeSpan[] to store.</param>
	public void Write(TimeSpan[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	/// <summary>
	/// Writes a UInt32[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The UInt32[] to store.</param>
	[CLSCompliant(false)]
	public void Write(uint[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	/// <summary>
	/// Writes a UInt64[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The UInt64[] to store.</param>
	[CLSCompliant(false)]
	public void Write(ulong[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	/// <summary>
	/// Writes a UInt16[] into the stream.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The UInt16[] to store.</param>
	[CLSCompliant(false)]
	public void Write(ushort[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	/// <summary>
	/// Writes an optimized Boolean[] into the stream using the fewest possible bytes.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// Stored as a BitArray.
	/// </summary>
	/// <param name="values">The Boolean[] to store.</param>
	public void WriteOptimized(bool[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.FullyOptimizedTypedArrayType);
		writeArray(values);
	}

	/// <summary>
	/// Writes a DateTime[] into the stream using the fewest possible bytes.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The DateTime[] to store.</param>
	public void WriteOptimized(DateTime[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i].Ticks % 10000 != 0L)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	/// <summary>
	/// Writes a Decimal[] into the stream using the fewest possible bytes.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Decimal[] to store.</param>
	public void WriteOptimized(decimal[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.FullyOptimizedTypedArrayType);
		writeArray(values);
	}

	/// <summary>
	/// Writes a not-null object[] into the stream using the fewest number of bytes possible.
	/// Stored Size: 2 bytes upwards depending on data content
	/// Notes:
	/// An empty object[] takes 1 byte.
	/// The contents of the array will be stored optimized.
	/// </summary>
	/// <param name="values">The object[] to store. Must not be null.</param>
	public void WriteOptimized(object[] values)
	{
		writeObjectArray(values);
	}

	/// <summary>
	/// Writes a pair of object[] arrays into the stream using the fewest number of bytes possible.
	/// The arrays must not be null and must have the same length
	/// The first array's values are written optimized
	/// The second array's values are compared against the first and, where identical, will be stored
	/// using a single byte.
	/// Useful for storing entity data where there is a before-change and after-change set of value pairs
	/// and, typically, only a few of the values will have changed.
	/// </summary>
	/// <param name="values1">The first object[] value which must not be null and must have the same length as values2</param>
	/// <param name="values2">The second object[] value which must not be null and must have the same length as values1</param>
	public void WriteOptimized(object[] values1, object[] values2)
	{
		writeObjectArray(values1);
		int num = values2.Length - 1;
		for (int i = 0; i < values2.Length; i++)
		{
			object obj = values2[i];
			if (obj?.Equals(values1[i]) ?? (values1[i] == null))
			{
				int num2 = 0;
				for (; i < num && ((values2[i + 1] == null) ? (values1[i + 1] == null) : values2[i + 1].Equals(values1[i + 1])); i++)
				{
					num2++;
				}
				if (num2 == 0)
				{
					writeTypeCode(SerializedType.DuplicateValueType);
					continue;
				}
				writeTypeCode(SerializedType.DuplicateValueSequenceType);
				write7bitEncodedSigned32BitValue(num2);
			}
			else if (obj == null)
			{
				int num3 = 0;
				for (; i < num && values2[i + 1] == null; i++)
				{
					num3++;
				}
				if (num3 == 0)
				{
					writeTypeCode(SerializedType.NullType);
					continue;
				}
				writeTypeCode(SerializedType.NullSequenceType);
				write7bitEncodedSigned32BitValue(num3);
			}
			else if (obj == DBNull.Value)
			{
				int num4 = 0;
				for (; i < num && values2[i + 1] == DBNull.Value; i++)
				{
					num4++;
				}
				if (num4 == 0)
				{
					writeTypeCode(SerializedType.DBNullType);
					continue;
				}
				writeTypeCode(SerializedType.DBNullSequenceType);
				write7bitEncodedSigned32BitValue(num4);
			}
			else
			{
				WriteObject(obj);
			}
		}
	}

	/// <summary>
	/// Writes an Int16[] into the stream using the fewest possible bytes.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Int16[] to store.</param>
	public void WriteOptimized(short[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] < 0 || values[i] > 127)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	/// <summary>
	/// Writes an Int32[] into the stream using the fewest possible bytes.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Int32[] to store.</param>
	public void WriteOptimized(int[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] < 0 || values[i] > 2097151)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	/// <summary>
	/// Writes an Int64[] into the stream using the fewest possible bytes.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The Int64[] to store.</param>
	public void WriteOptimized(long[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] < 0 || values[i] > 562949953421311L)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	/// <summary>
	/// Writes a TimeSpan[] into the stream using the fewest possible bytes.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The TimeSpan[] to store.</param>
	public void WriteOptimized(TimeSpan[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i].Ticks % 10000 != 0L)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	/// <summary>
	/// Writes a UInt16[] into the stream using the fewest possible bytes.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The UInt16[] to store.</param>
	[CLSCompliant(false)]
	public void WriteOptimized(ushort[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] > 127)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	/// <summary>
	/// Writes a UInt32[] into the stream using the fewest possible bytes.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The UInt32[] to store.</param>
	[CLSCompliant(false)]
	public void WriteOptimized(uint[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] > 2097151)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	/// <summary>
	/// Writes a UInt64[] into the stream using the fewest possible bytes.
	/// Notes:
	/// A null or empty array will take 1 byte.
	/// </summary>
	/// <param name="values">The UInt64[] to store.</param>
	[CLSCompliant(false)]
	public void WriteOptimized(ulong[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] > 562949953421311L)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	public void WriteOptimized(Version Value)
	{
		write7bitEncodedUnsigned32BitValue((uint)Value.Major);
		write7bitEncodedUnsigned32BitValue((uint)Value.Minor);
		write7bitEncodedUnsigned32BitValue((uint)Value.Build);
		write7bitEncodedUnsigned32BitValue((uint)Value.Revision);
	}

	/// <summary>
	/// Writes a Nullable type into the stream.
	/// Synonym for WriteObject().
	/// </summary>
	/// <param name="value">The Nullable value to store.</param>
	public void WriteNullable(ValueType value)
	{
		WriteObject(value);
	}

	/// <summary>
	/// Writes a non-null generic Dictionary into the stream.
	/// </summary>
	/// <remarks>
	/// The key and value types themselves are not stored - they must be 
	/// supplied at deserialization time.
	/// <para />
	/// An array of keys is stored followed by an array of values.
	/// </remarks>
	/// <typeparam name="K">The key Type.</typeparam>
	/// <typeparam name="V">The value Type.</typeparam>
	/// <param name="value">The generic dictionary.</param>
	public void Write<K, V>(Dictionary<K, V> value)
	{
		if (value == null)
		{
			Write(-1);
			return;
		}
		Write(value.Keys.Count);
		foreach (KeyValuePair<K, V> item in value)
		{
			WriteObject(item.Key);
			WriteObject(item.Value);
		}
	}

	public void Write(IDictionary Value)
	{
		if (Value == null)
		{
			Write(-1);
			return;
		}
		Write(Value.Keys.Count);
		foreach (DictionaryEntry item in Value)
		{
			WriteObject(item.Key);
			WriteObject(item.Value);
		}
	}

	/// <summary>
	/// Writes a non-null generic List into the stream.
	/// </summary>
	/// <remarks>
	/// The list type itself is not stored - it must be supplied
	/// at deserialization time.
	/// <para />
	/// The list contents are stored as an array.
	/// </remarks>
	/// <typeparam name="T">The list Type.</typeparam>
	/// <param name="value">The generic List.</param>
	public void Write<T>(List<T> value)
	{
		if (value == null)
		{
			Write(-1);
			return;
		}
		Write(value.Count);
		for (int i = 0; i < value.Count; i++)
		{
			WriteObject(value[i]);
		}
	}

	public void Write(List<string> value)
	{
		if (value == null)
		{
			Write(-1);
			return;
		}
		Write(value.Count);
		for (int i = 0; i < value.Count; i++)
		{
			Write(value[i]);
		}
	}

	public void Write(IList Value)
	{
		if (Value == null)
		{
			Write(-1);
			return;
		}
		Write(Value.Count);
		foreach (object item in Value)
		{
			WriteObject(item);
		}
	}

	public void Write(Location2D Value)
	{
		if (Value == null)
		{
			Write(-1);
			return;
		}
		WriteOptimized(Value.X);
		WriteOptimized(Value.Y);
	}

	public void Write(List<Location2D> Value)
	{
		if (Value == null)
		{
			Write(-1);
			return;
		}
		Write(Value.Count);
		foreach (Location2D item in Value)
		{
			Write(item);
		}
	}

	public void Write(Cell Value)
	{
		string text = Value?.ParentZone?.ZoneID;
		WriteOptimized(text);
		if (text != null)
		{
			WriteOptimized(Value.X);
			WriteOptimized(Value.Y);
		}
	}

	/// <remark>
	/// Serialization of event registries are delayed until the finalization step,
	/// ensuring all external event bindings can reference their targets correctly.
	/// </remark>
	public void Write(EventRegistry Registry)
	{
		if (Registry == null || !Registry.Serialize)
		{
			write7bitEncodedSigned32BitValue(0);
			return;
		}
		write7bitEncodedSigned32BitValue(EventRegistries.Count + 1);
		EventRegistries.Add(Registry);
	}

	/// <remark>
	/// Serialization of object references are delayed until the finalization step.
	/// </remark>
	public void Write(GameObjectReference Reference)
	{
		if (Reference == null)
		{
			write7bitEncodedSigned32BitValue(0);
			return;
		}
		write7bitEncodedSigned32BitValue(GameObjectReferences.Count + 1);
		GameObjectReferences.Add(Reference);
	}

	public void Write(IComposite Value)
	{
		if (Value == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else
		{
			SerializeComposite(Value);
		}
	}

	private void SerializeComposite(IComposite Value)
	{
		bool wantFieldReflection = Value.WantFieldReflection;
		writeTypeCode(wantFieldReflection ? SerializedType.ICompositeFieldType : SerializedType.ICompositeType);
		Type type = Value.GetType();
		using (StartBlock())
		{
			WriteTokenized(type);
			if (wantFieldReflection)
			{
				WriteTypeFields(Value, type);
			}
			Value.Write(this);
		}
	}

	public void WriteComposite<T>(T Value) where T : IComposite, new()
	{
		if (Value == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		bool wantFieldReflection = Value.WantFieldReflection;
		writeTypeCode(wantFieldReflection ? SerializedType.ICompositeFieldType : SerializedType.ICompositeType);
		using (StartBlock())
		{
			if (wantFieldReflection)
			{
				WriteFields(Value);
			}
			Value.Write(this);
		}
	}

	public void WriteComposite<T>(List<T> Value) where T : IComposite
	{
		if (Value == null)
		{
			WriteOptimized(0);
			return;
		}
		int count = Value.Count;
		WriteOptimized(count + 1);
		for (int i = 0; i < count; i++)
		{
			Write(Value[i]);
		}
	}

	/// <summary>
	/// Writes a null or a typed array into the stream.
	/// </summary>
	/// <param name="values">The array to store.</param>
	public void WriteTypedArray(Array values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else
		{
			writeTypedArray(values, storeType: true);
		}
	}

	/// <summary>
	/// Writes the contents of the string and object token tables into the stream.
	/// Also write the starting offset into the first 4 bytes of the stream.
	/// Notes:
	/// Called automatically by ToArray().
	/// Can be used to ensure that the complete graph is written before using an
	/// alternate technique of extracting a Byte[] such as using compression on
	/// the underlying stream.
	/// </summary>
	/// <returns>The length of the string and object tables.</returns>
	public int AppendTokenTables()
	{
		GameObjectReferencePosition = OutStream.Position;
		int count = GameObjectReferences.Count;
		write7bitEncodedSigned32BitValue(count);
		for (int i = 0; i < count; i++)
		{
			GameObjectReferences[i].Write(this);
		}
		ObjectPosition = OutStream.Position;
		count = TokenObjects.Count;
		write7bitEncodedSigned32BitValue(count);
		for (int j = 0; j < count; j++)
		{
			WriteObject(TokenObjects[j]);
		}
		TokenizedPosition = OutStream.Position;
		count = Tokenized.Count;
		write7bitEncodedSigned32BitValue(count);
		for (int k = 0; k < count; k++)
		{
			Write(Tokenized[k]);
			Tokenized[k].Token = 0;
		}
		TypePosition = OutStream.Position;
		count = TokenTypes.Count;
		write7bitEncodedSigned32BitValue(count);
		for (int l = 0; l < count; l++)
		{
			WriteDirect(TokenTypes[l]);
		}
		StringPosition = OutStream.Position;
		count = TokenStrings.Count;
		write7bitEncodedSigned32BitValue(count);
		for (int m = 0; m < count; m++)
		{
			base.Write(TokenStrings[m]);
		}
		formatter = null;
		return (int)(OutStream.Position - ObjectPosition);
	}

	public void FinalizeWrite()
	{
		WriteGameObjects();
		WriteEventRegistries();
		AppendTokenTables();
		WriteHeader();
	}

	public void ReserveHeader()
	{
		for (int i = 0; i < 68; i++)
		{
			OutStream.WriteByte(0);
		}
	}

	public void WriteHeader()
	{
		long position = OutStream.Position;
		OutStream.Position = 0L;
		Write(FileVersion);
		Write(GameObjectPosition);
		Write(GameObjectReferencePosition);
		Write(EventRegistryPosition);
		Write(TokenizedPosition);
		Write(ObjectPosition);
		Write(TypePosition);
		Write(StringPosition);
		Write(TokenGameObjects.Count);
		Write(EventRegistries.Count);
		OutStream.Position = position;
	}

	/// <summary>
	/// Returns a byte[] containing all of the serialized data.
	///
	/// The current implementation has the data in 3 sections:
	/// 1) A 4 byte Int32 giving the offset to the 3rd section.
	/// 2) The main serialized data.
	/// 3) The serialized string tokenization lists and object
	///    tokenization lists.
	///
	/// Only call this method once all of the data has been serialized.
	///
	/// This method appends all of the tokenized data (string and object)
	/// to the end of the stream and ensures that the first four bytes
	/// reflect the offset of the tokenized data so that it can be
	/// deserialized first.
	/// This is the reason for requiring a rewindable stream.
	///
	/// Future implementations may also allow the serialized data to be
	/// accessed via 2 byte[] arrays. This would remove the requirement
	/// for a rewindable stream opening the possibility of streaming the
	/// serialized data directly over the network allowing simultaneous
	/// of partially simultaneous deserialization.
	/// </summary>
	/// <returns>A byte[] containing all serialized data.</returns>
	public byte[] ToArray()
	{
		AppendTokenTables();
		return Stream.ToArray();
	}

	/// <summary>
	/// Writes a byte[] directly into the stream.
	/// The size of the array is not stored so only use this method when
	/// the number of bytes will be known at deserialization time.
	///
	/// A null value will throw an exception
	/// </summary>
	/// <param name="value">The byte[] to store. Must not be null.</param>
	public void WriteBytesDirect(byte[] value)
	{
		base.Write(value);
	}

	/// <summary>
	/// Writes a non-null string directly to the stream without tokenization.
	/// </summary>
	/// <param name="value">The string to store. Must not be null.</param>
	public void WriteStringDirect(string value)
	{
		base.Write(value);
	}

	public void WriteFields<T>(T Value, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		WriteTypeFields(Value, typeof(T), Flags, Mask);
	}

	public void WriteObjectFields(object Value, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		WriteTypeFields(Value, Value.GetType(), Flags, Mask);
	}

	public void WriteTypeFields(object Value, Type Type, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		FieldInfo[] array = ((Flags == (BindingFlags.Instance | BindingFlags.Public)) ? Type.GetCachedFields() : Type.GetFields(Flags));
		foreach (FieldInfo fieldInfo in array)
		{
			if ((fieldInfo.Attributes & Mask) == 0)
			{
				WriteObject(fieldInfo.GetValue(Value));
			}
		}
	}

	public void WriteNamedFields(object Value, Type Type, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		FieldInfo[] array = ((Flags == (BindingFlags.Instance | BindingFlags.Public)) ? Type.GetCachedFields() : Type.GetFields(Flags));
		int num = array.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if ((array[i].Attributes & Mask) == 0)
			{
				num2++;
			}
		}
		WriteOptimized(num2);
		for (int j = 0; j < num; j++)
		{
			if (num2 <= 0)
			{
				break;
			}
			FieldInfo fieldInfo = array[j];
			if ((fieldInfo.Attributes & Mask) == 0)
			{
				WriteOptimized(fieldInfo.Name);
				WriteObject(fieldInfo.GetValue(Value));
				num2--;
			}
		}
	}

	public void WriteGameObjectList(List<GameObject> List)
	{
		Write(List.Count);
		for (int i = 0; i < List.Count; i++)
		{
			WriteGameObject(List[i]);
		}
	}

	public void WriteGameObject(GameObject Object)
	{
		if (Object == null)
		{
			write7bitEncodedSigned32BitValue(0);
			return;
		}
		if (TokenByGameObject.TryGetValue(Object, out var value))
		{
			write7bitEncodedSigned32BitValue(value + 2);
			return;
		}
		Object.Flags |= 32;
		value = TokenGameObjects.Count;
		TokenByGameObject[Object] = value;
		TokenGameObjects.Add(Object);
		write7bitEncodedSigned32BitValue(value + 2);
	}

	public void WriteGameObject(GameObject Object, bool Reference)
	{
		int value;
		string value2;
		if (Object == null)
		{
			write7bitEncodedSigned32BitValue(0);
		}
		else if (TokenByGameObject.TryGetValue(Object, out value))
		{
			write7bitEncodedSigned32BitValue(value + 2);
		}
		else if (!Reference)
		{
			Object.Flags |= 32;
			value = TokenGameObjects.Count;
			TokenByGameObject[Object] = value;
			TokenGameObjects.Add(Object);
			write7bitEncodedSigned32BitValue(value + 2);
		}
		else if (Object.Property.TryGetValue("id", out value2))
		{
			write7bitEncodedSigned32BitValue(1);
			WriteOptimized(value2);
		}
		else
		{
			write7bitEncodedSigned32BitValue(0);
		}
	}

	public void WriteGameObjects()
	{
		GameObjectPosition = OutStream.Position;
		for (int i = ((!SerializePlayer) ? 1 : 0); i < TokenGameObjects.Count; i++)
		{
			Write((ushort)64206);
			write7bitEncodedSigned32BitValue(i);
			GameObject gameObject = TokenGameObjects[i];
			gameObject.Save(this);
			Write((ushort)44203);
			gameObject.Flags &= -33;
		}
		for (int j = 0; j < TokenGameObjects.Count; j++)
		{
			List<GameObject> list = TokenGameObjects[j]?.Inventory?.Objects;
			if (list.IsNullOrEmpty())
			{
				continue;
			}
			foreach (GameObject item in list)
			{
				if (item.Flags.HasBit(32))
				{
					MetricsManager.LogError("!POOLEDOBJECT Inventory object '" + item.DebugName + "' in '" + TokenGameObjects[j].DebugName + "' was tokenized but never wrote its data.");
				}
			}
		}
		Write((ushort)52958);
	}

	public void WriteEventRegistries()
	{
		EventRegistryPosition = OutStream.Position;
		int i = 0;
		for (int count = EventRegistries.Count; i < count; i++)
		{
			EventRegistries[i].Write(this);
		}
	}

	/// <summary>
	/// Writes a token (an Int32 taking 1 to 4 bytes) into the stream that represents the object instance.
	/// The same token will always be used for the same object instance.
	///
	/// The object will be serialized once and recreated at deserialization time.
	/// Calls to SerializationReader.ReadTokenizedObject() will retrieve the same object instance.
	///
	/// </summary>
	/// <param name="value">The object to tokenize. Must not be null and must not be a string.</param>
	public void WriteTokenizedObject(object value)
	{
		WriteTokenizedObject(value, recreateFromType: false);
	}

	/// <summary>
	/// Writes a token (an Int32 taking 1 to 4 bytes) into the stream that represents the object instance.
	/// The same token will always be used for the same object instance.
	///
	/// When recreateFromType is set to true, the object's Type will be stored and the object recreated using 
	/// Activator.GetInstance with a parameterless contructor. This is useful for stateless, factory-type classes.
	///
	/// When recreateFromType is set to false, the object will be serialized once and recreated at deserialization time.
	///
	/// Calls to SerializationReader.ReadTokenizedObject() will retrieve the same object instance.
	/// </summary>
	/// <param name="value">The object to tokenize. Must not be null and must not be a string.</param>
	/// <param name="recreateFromType">true if the object can be recreated using a parameterless constructor; 
	/// false if the object should be serialized as-is</param>
	public void WriteTokenizedObject(object value, bool recreateFromType)
	{
		if (recreateFromType)
		{
			if (WrapperCache == null)
			{
				WrapperCache = new SingletonTypeWrapper();
			}
			WrapperCache.WrappedType = value.GetType();
			value = WrapperCache;
		}
		object obj = TokenByObject[value];
		if (obj != null)
		{
			write7bitEncodedSigned32BitValue((int)obj);
			return;
		}
		if (recreateFromType)
		{
			value = new SingletonTypeWrapper(WrapperCache);
		}
		int count = TokenObjects.Count;
		TokenObjects.Add(value);
		TokenByObject[value] = count;
		write7bitEncodedSigned32BitValue(count);
	}

	public void WriteTokenized(Type Type)
	{
		if ((object)Type == null)
		{
			write7bitEncodedSigned32BitValue(0);
			return;
		}
		if (!TokenByType.TryGetValue(Type, out var value))
		{
			value = TokenTypes.Count + 1;
			TokenTypes.Add(Type);
			TokenByType[Type] = value;
		}
		write7bitEncodedSigned32BitValue(value);
	}

	public void WriteTokenized(ITokenized Value)
	{
		int num = 0;
		if (Value != null)
		{
			num = Value.Token;
			if (num <= 0)
			{
				num = (Value.Token = Tokenized.Count + 1);
				Tokenized.Add(Value);
			}
		}
		write7bitEncodedSigned32BitValue(num);
	}

	public void WriteModVersions()
	{
		int num = ModManager.ActiveMods?.Count ?? 0;
		WriteOptimized(num);
		for (int i = 0; i < num; i++)
		{
			ModInfo modInfo = ModManager.ActiveMods[i];
			WriteOptimized(modInfo.ID);
			WriteOptimized(modInfo.Manifest?.Version ?? Version.Zero);
		}
	}

	internal void WriteExtensionHash()
	{
		WriteOptimized(ComponentReflection.ExtensionHash);
	}

	internal static IFastSerializationTypeSurrogate findSurrogateForType(Type type)
	{
		foreach (IFastSerializationTypeSurrogate typeSurrogate in TypeSurrogates)
		{
			if (typeSurrogate.SupportsType(type))
			{
				return typeSurrogate;
			}
		}
		return null;
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
	/// Encodes a TimeSpan into the fewest number of bytes.
	/// Has been separated from the WriteOptimized(TimeSpan) method so that WriteOptimized(DateTime)
	/// can also use this for .NET 2.0 DateTimeKind information.
	/// By taking advantage of the fact that a DateTime's TimeOfDay portion will never use the IsNegative
	/// and HasDays flags, we can use these 2 bits to store the DateTimeKind and, since DateTimeKind is
	/// unlikely to be set without a Time, we need no additional bytes to support a .NET 2.0 DateTime.
	/// </summary>
	/// <param name="value">The TimeSpan to store.</param>
	/// <param name="partOfDateTime">True if the TimeSpan is the TimeOfDay from a DateTime; False if a real TimeSpan.</param>
	/// <param name="initialData">The intial data for the BitVector32 - contains DateTimeKind or 0</param>
	private void encodeTimeSpan(TimeSpan value, bool partOfDateTime, int initialData)
	{
		BitVector32 bitVector = new BitVector32(initialData);
		int num = Math.Abs(value.Hours);
		int num2 = Math.Abs(value.Minutes);
		int num3 = Math.Abs(value.Seconds);
		int num4 = Math.Abs(value.Milliseconds);
		bool flag = num != 0 || num2 != 0;
		int num5 = 0;
		int num6;
		if (partOfDateTime)
		{
			num6 = 0;
		}
		else
		{
			num6 = Math.Abs(value.Days);
			bitVector[IsNegativeSection] = ((value.Ticks < 0) ? 1 : 0);
			bitVector[HasDaysSection] = ((num6 != 0) ? 1 : 0);
		}
		if (flag)
		{
			bitVector[HasTimeSection] = 1;
			bitVector[HoursSection] = num;
			bitVector[MinutesSection] = num2;
		}
		if (num3 != 0)
		{
			bitVector[HasSecondsSection] = 1;
			if (!flag && num4 == 0)
			{
				bitVector[MinutesSection] = num3;
			}
			else
			{
				bitVector[SecondsSection] = num3;
				num5++;
			}
		}
		if (num4 != 0)
		{
			bitVector[HasMillisecondsSection] = 1;
			bitVector[MillisecondsSection] = num4;
			num5 = 2;
		}
		int data = bitVector.Data;
		Write((byte)data);
		Write((byte)(data >> 8));
		if (num5 > 0)
		{
			Write((byte)(data >> 16));
		}
		if (num5 > 1)
		{
			Write((byte)(data >> 24));
		}
		if (num6 != 0)
		{
			write7bitEncodedSigned32BitValue(num6);
		}
	}

	/// <summary>
	/// Checks whether an optimization condition has been met and throw an exception if not.
	///
	/// This method has been made conditional on THROW_IF_NOT_OPTIMIZABLE being set at compile time.
	/// By default, this is set if DEBUG is set but could be set explicitly if exceptions are required and
	/// the evaluation overhead is acceptable. 
	/// If not set, then this method and all references to it are removed at compile time.
	///
	/// Leave at the default for optimum usage.
	/// </summary>
	/// <param name="condition">An expression evaluating to true if the optimization condition is met, false otherwise.</param>
	/// <param name="message">The message to include in the exception should the optimization condition not be met.</param>
	[Conditional("THROW_IF_NOT_OPTIMIZABLE")]
	private static void checkOptimizable(bool condition, string message)
	{
		if (!condition)
		{
			throw new OptimizationException(message);
		}
	}

	/// <summary>
	/// Stores a 32-bit signed value into the stream using 7-bit encoding.
	///
	/// The value is written 7 bits at a time (starting with the least-significant bits) until there are no more bits to write.
	/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
	///
	/// See Write(Int32) for details of the values that are optimizable.
	/// </summary>
	/// <param name="value">The Int32 value to encode.</param>
	private void write7bitEncodedSigned32BitValue(int value)
	{
		uint num;
		for (num = (uint)value; num >= 128; num >>= 7)
		{
			Write((byte)(num | 0x80));
		}
		Write((byte)num);
	}

	/// <summary>
	/// Stores a 64-bit signed value into the stream using 7-bit encoding.
	///
	/// The value is written 7 bits at a time (starting with the least-significant bits) until there are no more bits to write.
	/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
	///
	/// See Write(Int64) for details of the values that are optimizable.
	/// </summary>
	/// <param name="value">The Int64 value to encode.</param>
	private void write7bitEncodedSigned64BitValue(long value)
	{
		ulong num;
		for (num = (ulong)value; num >= 128; num >>= 7)
		{
			Write((byte)(num | 0x80));
		}
		Write((byte)num);
	}

	/// <summary>
	/// Stores a 32-bit unsigned value into the stream using 7-bit encoding.
	///
	/// The value is written 7 bits at a time (starting with the least-significant bits) until there are no more bits to write.
	/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
	///
	/// See Write(UInt32) for details of the values that are optimizable.
	/// </summary>
	/// <param name="value">The UInt32 value to encode.</param>
	private void write7bitEncodedUnsigned32BitValue(uint value)
	{
		while (value >= 128)
		{
			Write((byte)(value | 0x80));
			value >>= 7;
		}
		Write((byte)value);
	}

	/// <summary>
	/// Stores a 64-bit unsigned value into the stream using 7-bit encoding.
	///
	/// The value is written 7 bits at a time (starting with the least-significant bits) until there are no more bits to write.
	/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
	///
	/// See Write(ULong) for details of the values that are optimizable.
	/// </summary>
	/// <param name="value">The ULong value to encode.</param>
	private void write7bitEncodedUnsigned64BitValue(ulong value)
	{
		while (value >= 128)
		{
			Write((byte)(value | 0x80));
			value >>= 7;
		}
		Write((byte)value);
	}

	/// <summary>
	/// Internal implementation to store a non-null Boolean[].
	/// </summary>
	/// <remarks>
	/// Stored as a BitArray for optimization.
	/// </remarks>
	/// <param name="values">The Boolean[] to store.</param>
	private void writeArray(bool[] values)
	{
		WriteOptimized(new BitArray(values));
	}

	/// <summary>
	/// Internal implementation to store a non-null Byte[].
	/// </summary>
	/// <param name="values">The Byte[] to store.</param>
	private void writeArray(byte[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		if (values.Length != 0)
		{
			base.Write(values);
		}
	}

	/// <summary>
	/// Internal implementation to store a non-null Char[].
	/// </summary>
	/// <param name="values">The Char[] to store.</param>
	private void writeArray(char[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		if (values.Length != 0)
		{
			base.Write(values);
		}
	}

	/// <summary>
	/// Internal implementation to write a non, null DateTime[] using a BitArray to 
	/// determine which elements are optimizable.
	/// </summary>
	/// <param name="values">The DateTime[] to store.</param>
	/// <param name="optimizeFlags">A BitArray indicating which of the elements which are optimizable; 
	/// a reference to constant FullyOptimizableValueArray if all the elements are optimizable; or null
	/// if none of the elements are optimizable.</param>
	private void writeArray(DateTime[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				WriteOptimized(values[i]);
			}
		}
	}

	/// <summary>
	/// Internal implementation to store a non-null Decimal[].
	/// </summary>
	/// <remarks>
	/// All elements are stored optimized.
	/// </remarks>
	/// <param name="values">The Decimal[] to store.</param>
	private void writeArray(decimal[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			WriteOptimized(values[i]);
		}
	}

	/// <summary>
	/// Internal implementation to store a non-null Double[].
	/// </summary>
	/// <param name="values">The Double[] to store.</param>
	private void writeArray(double[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (double value in values)
		{
			Write(value);
		}
	}

	/// <summary>
	/// Internal implementation to store a non-null Single[].
	/// </summary>
	/// <param name="values">The Single[] to store.</param>
	private void writeArray(float[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (float value in values)
		{
			Write(value);
		}
	}

	/// <summary>
	/// Internal implementation to store a non-null Guid[].
	/// </summary>
	/// <param name="values">The Guid[] to store.</param>
	private void writeArray(Guid[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (Guid value in values)
		{
			Write(value);
		}
	}

	/// <summary>
	/// Internal implementation to write a non-null Int16[] using a BitArray to determine which elements are optimizable.
	/// </summary>
	/// <param name="values">The Int16[] to store.</param>
	/// <param name="optimizeFlags">A BitArray indicating which of the elements which are optimizable; 
	/// a reference to constant FullyOptimizableValueArray if all the elements are optimizable; or null
	/// if none of the elements are optimizable.</param>
	private void writeArray(short[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedSigned32BitValue(values[i]);
			}
		}
	}

	/// <summary>
	/// Internal implementation to write a non-null Int32[] using a BitArray to determine which elements are optimizable.
	/// </summary>
	/// <param name="values">The Int32[] to store.</param>
	/// <param name="optimizeFlags">A BitArray indicating which of the elements which are optimizable; 
	/// a reference to constant FullyOptimizableValueArray if all the elements are optimizable; or null
	/// if none of the elements are optimizable.</param>
	private void writeArray(int[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedSigned32BitValue(values[i]);
			}
		}
	}

	/// <summary>
	/// Internal implementation to writes a non-null Int64[] using a BitArray to determine which elements are optimizable.
	/// </summary>
	/// <param name="values">The Int64[] to store.</param>
	/// <param name="optimizeFlags">A BitArray indicating which of the elements which are optimizable; 
	/// a reference to constant FullyOptimizableValueArray if all the elements are optimizable; or null
	/// if none of the elements are optimizable.</param>
	private void writeArray(long[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedSigned64BitValue(values[i]);
			}
		}
	}

	/// <summary>
	/// Internal implementation to store a non-null SByte[].
	/// </summary>
	/// <param name="values">The SByte[] to store.</param>
	private void writeArray(sbyte[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (sbyte value in values)
		{
			Write(value);
		}
	}

	/// <summary>
	/// Internal implementation to store a non-null Int16[].
	/// </summary>
	/// <param name="values">The Int16[] to store.</param>
	private void writeArray(short[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (short value in values)
		{
			Write(value);
		}
	}

	/// <summary>
	/// Internal implementation to write a non-null TimeSpan[] using a BitArray to determine which elements are optimizable.
	/// </summary>
	/// <param name="values">The TimeSpan[] to store.</param>
	/// <param name="optimizeFlags">A BitArray indicating which of the elements which are optimizable; 
	/// a reference to constant FullyOptimizableValueArray if all the elements are optimizable; or null
	/// if none of the elements are optimizable.</param>
	private void writeArray(TimeSpan[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				WriteOptimized(values[i]);
			}
		}
	}

	/// <summary>
	/// Internal implementation to write a non-null UInt16[] using a BitArray to determine which elements are optimizable.
	/// </summary>
	/// <param name="values">The UInt16[] to store.</param>
	/// <param name="optimizeFlags">A BitArray indicating which of the elements which are optimizable; 
	/// a reference to constant FullyOptimizableValueArray if all the elements are optimizable; or null
	/// if none of the elements are optimizable.</param>
	private void writeArray(ushort[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedUnsigned32BitValue(values[i]);
			}
		}
	}

	/// <summary>
	/// Internal implementation to write a non-null UInt32[] using a BitArray to determine which elements are optimizable.
	/// </summary>
	/// <param name="values">The UInt32[] to store.</param>
	/// <param name="optimizeFlags">A BitArray indicating which of the elements which are optimizable; 
	/// a reference to constant FullyOptimizableValueArray if all the elements are optimizable; or null
	/// if none of the elements are optimizable.</param>
	private void writeArray(uint[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedUnsigned32BitValue(values[i]);
			}
		}
	}

	/// <summary>
	/// Internal implementation to store a non-null UInt16[].
	/// </summary>
	/// <param name="values">The UIn16[] to store.</param>
	private void writeArray(ushort[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (ushort value in values)
		{
			Write(value);
		}
	}

	/// <summary>
	/// Internal implementation to write a non-null UInt64[] using a BitArray to determine which elements are optimizable.
	/// </summary>
	/// <param name="values">The UInt64[] to store.</param>
	/// <param name="optimizeFlags">A BitArray indicating which of the elements which are optimizable; 
	/// a reference to constant FullyOptimizableValueArray if all the elements are optimizable; or null
	/// if none of the elements are optimizable.</param>
	private void writeArray(ulong[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedUnsigned64BitValue(values[i]);
			}
		}
	}

	/// <summary>
	/// Writes the values in the non-null object[] into the stream.
	///
	/// Sequences of null values and sequences of DBNull.Values are stored with a flag and optimized count.
	/// Other values are stored using WriteObject().
	///
	/// This routine is called by the Write(object[]), WriteOptimized(object[]) and Write(object[], object[])) methods.
	/// </summary>
	/// <param name="values" />
	private void writeObjectArray(object[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		int num = values.Length - 1;
		for (int i = 0; i < values.Length; i++)
		{
			object obj = values[i];
			if (i < num && (obj?.Equals(values[i + 1]) ?? (values[i + 1] == null)))
			{
				int num2 = 1;
				if (obj == null)
				{
					writeTypeCode(SerializedType.NullSequenceType);
					for (i++; i < num && values[i + 1] == null; i++)
					{
						num2++;
					}
				}
				else if (obj == DBNull.Value)
				{
					writeTypeCode(SerializedType.DBNullSequenceType);
					for (i++; i < num && values[i + 1] == DBNull.Value; i++)
					{
						num2++;
					}
				}
				else
				{
					writeTypeCode(SerializedType.DuplicateValueSequenceType);
					for (i++; i < num && obj.Equals(values[i + 1]); i++)
					{
						num2++;
					}
					WriteObject(obj);
				}
				write7bitEncodedSigned32BitValue(num2);
			}
			else
			{
				WriteObject(obj);
			}
		}
	}

	/// <summary>
	/// Stores the specified SerializedType code into the stream.
	///
	/// By using a centralized method, it is possible to collect statistics for the
	/// type of data being stored in DEBUG mode.
	///
	/// Use the DumpTypeUsage() method to show a list of used SerializedTypes and
	/// the number of times each has been used. This method and the collection code
	/// will be optimized out when compiling in Release mode.
	/// </summary>
	/// <param name="typeCode">The SerializedType to store.</param>
	private void writeTypeCode(SerializedType typeCode)
	{
		Write((byte)typeCode);
	}

	/// <summary>
	/// Internal implementation to write a non-null typed array into the stream.
	/// </summary>
	/// <remarks>
	/// Checks first to see if the element type is a primitive type and calls the 
	/// correct routine if so. Otherwise determines the best, optimized method
	/// to store the array contents.
	/// <para />
	/// An array of object elements never stores its type.
	/// </remarks>
	/// <param name="value">The non-null typed array to store.</param>
	/// <param name="storeType">True if the type should be stored; false otherwise</param>
	private void writeTypedArray(Array value, bool storeType)
	{
		Type elementType = value.GetType().GetElementType();
		if (elementType == typeof(object))
		{
			storeType = false;
		}
		if (elementType == typeof(string))
		{
			writeTypeCode(SerializedType.StringArrayType);
			WriteOptimized((object[])value);
			return;
		}
		if (elementType == typeof(int))
		{
			writeTypeCode(SerializedType.Int32ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((int[])value);
			}
			else
			{
				Write((int[])value);
			}
			return;
		}
		if (elementType == typeof(short))
		{
			writeTypeCode(SerializedType.Int16ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((short[])value);
			}
			else
			{
				Write((short[])value);
			}
			return;
		}
		if (elementType == typeof(long))
		{
			writeTypeCode(SerializedType.Int64ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((long[])value);
			}
			else
			{
				Write((long[])value);
			}
			return;
		}
		if (elementType == typeof(uint))
		{
			writeTypeCode(SerializedType.UInt32ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((uint[])value);
			}
			else
			{
				Write((uint[])value);
			}
			return;
		}
		if (elementType == typeof(ushort))
		{
			writeTypeCode(SerializedType.UInt16ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((ushort[])value);
			}
			else
			{
				Write((ushort[])value);
			}
			return;
		}
		if (elementType == typeof(ulong))
		{
			writeTypeCode(SerializedType.UInt64ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((ulong[])value);
			}
			else
			{
				Write((ulong[])value);
			}
			return;
		}
		if (elementType == typeof(float))
		{
			writeTypeCode(SerializedType.SingleArrayType);
			writeArray((float[])value);
			return;
		}
		if (elementType == typeof(double))
		{
			writeTypeCode(SerializedType.DoubleArrayType);
			writeArray((double[])value);
			return;
		}
		if (elementType == typeof(decimal))
		{
			writeTypeCode(SerializedType.DecimalArrayType);
			writeArray((decimal[])value);
			return;
		}
		if (elementType == typeof(DateTime))
		{
			writeTypeCode(SerializedType.DateTimeArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((DateTime[])value);
			}
			else
			{
				Write((DateTime[])value);
			}
			return;
		}
		if (elementType == typeof(TimeSpan))
		{
			writeTypeCode(SerializedType.TimeSpanArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((TimeSpan[])value);
			}
			else
			{
				Write((TimeSpan[])value);
			}
			return;
		}
		if (elementType == typeof(Guid))
		{
			writeTypeCode(SerializedType.GuidArrayType);
			writeArray((Guid[])value);
			return;
		}
		if (elementType == typeof(sbyte))
		{
			writeTypeCode(SerializedType.SByteArrayType);
			writeArray((sbyte[])value);
			return;
		}
		if (elementType == typeof(bool))
		{
			writeTypeCode(SerializedType.BooleanArrayType);
			writeArray((bool[])value);
			return;
		}
		if (elementType == typeof(byte))
		{
			writeTypeCode(SerializedType.ByteArrayType);
			writeArray((byte[])value);
			return;
		}
		if (elementType == typeof(char))
		{
			writeTypeCode(SerializedType.CharArrayType);
			writeArray((char[])value);
			return;
		}
		if (value.Length == 0)
		{
			writeTypeCode((elementType == typeof(object)) ? SerializedType.EmptyObjectArrayType : SerializedType.EmptyTypedArrayType);
			if (storeType)
			{
				WriteTokenized(elementType);
			}
			return;
		}
		if (elementType == typeof(object))
		{
			writeTypeCode(SerializedType.ObjectArrayType);
			writeObjectArray((object[])value);
			return;
		}
		BitArray bitArray = (isTypeRecreatable(elementType) ? FullyOptimizableTypedArray : null);
		if (!elementType.IsValueType)
		{
			if (bitArray == null || !arrayElementsAreSameType((object[])value, elementType))
			{
				if (!storeType)
				{
					writeTypeCode(SerializedType.ObjectArrayType);
				}
				else
				{
					writeTypeCode(SerializedType.OtherTypedArrayType);
					WriteTokenized(elementType);
				}
				writeObjectArray((object[])value);
				return;
			}
			for (int i = 0; i < value.Length; i++)
			{
				if (value.GetValue(i) == null)
				{
					if (bitArray == FullyOptimizableTypedArray)
					{
						bitArray = new BitArray(value.Length);
					}
					bitArray[i] = true;
				}
			}
		}
		writeTypedArrayTypeCode(bitArray, value.Length);
		if (storeType)
		{
			WriteTokenized(elementType);
		}
		for (int j = 0; j < value.Length; j++)
		{
			if (bitArray == null)
			{
				WriteObject(value.GetValue(j));
			}
			else if (bitArray == FullyOptimizableTypedArray || !bitArray[j])
			{
				Write((IOwnedDataSerializable)value.GetValue(j), null);
			}
		}
	}

	/// <summary>
	/// Checks whether instances of a Type can be created.
	/// </summary>
	/// <remarks>
	/// A Value Type only needs to implement IOwnedDataSerializable. 
	/// A Reference Type needs to implement IOwnedDataSerializableAndRecreatable and provide a default constructor.
	/// </remarks>
	/// <param name="type">The Type to check</param>
	/// <returns>true if the Type is recreatable; false otherwise.</returns>
	private static bool isTypeRecreatable(Type type)
	{
		if (type.IsValueType)
		{
			return typeof(IOwnedDataSerializable).IsAssignableFrom(type);
		}
		if (typeof(IOwnedDataSerializableAndRecreatable).IsAssignableFrom(type))
		{
			return type.GetConstructor(Type.EmptyTypes) != null;
		}
		return false;
	}

	/// <summary>
	/// Checks whether each element in an array is of the same type.
	/// </summary>
	/// <param name="values">The array to check</param>
	/// <param name="elementType">The expected element type.</param>
	/// <returns />
	private static bool arrayElementsAreSameType(object[] values, Type elementType)
	{
		foreach (object obj in values)
		{
			if (obj != null && obj.GetType() != elementType)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Writes the TypeCode for the Typed Array followed by the number of elements.
	/// </summary>
	/// <param name="optimizeFlags" />
	/// <param name="length" />
	private void writeTypedArrayTypeCode(BitArray optimizeFlags, int length)
	{
		if (optimizeFlags == null)
		{
			writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		}
		else if (optimizeFlags == FullyOptimizableTypedArray)
		{
			writeTypeCode(SerializedType.FullyOptimizedTypedArrayType);
		}
		else
		{
			writeTypeCode(SerializedType.PartiallyOptimizedTypedArrayType);
			WriteOptimized(optimizeFlags);
		}
		write7bitEncodedSigned32BitValue(length);
	}

	[Conditional("DEBUG")]
	public void DumpTypeUsage()
	{
		StringBuilder value = new StringBuilder("Type Usage Dump\r\n---------------\r\n");
		for (int i = 0; i < 256; i++)
		{
		}
		Console.WriteLine(value);
	}

	public Block StartBlock()
	{
		if (!Blocks.TryPop(out var result))
		{
			result = new Block();
		}
		result.Start(this);
		return result;
	}
}
