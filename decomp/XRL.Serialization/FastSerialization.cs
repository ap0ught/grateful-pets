using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using XRL.Collections;
using XRL.World;

namespace XRL.Serialization;

public static class FastSerialization
{
	public class Cache
	{
		public bool Reserved;

		public MemoryStream MemoryStream;

		public Rack<EventRegistry> EventRegistries;

		public Rack<GameObject> GameObjects;

		public Rack<GameObjectReference> GameObjectReferences;

		public Rack<ITokenized> Tokenized;

		public Rack<Type> Types;

		public Rack<string> Names;

		public Rack<string> Strings;

		public Rack<object> Objects;

		public Cache(int Capacity)
		{
			MemoryStream = new MemoryStream(Capacity * 8);
			EventRegistries = new Rack<EventRegistry>(Capacity / 32);
			GameObjects = new Rack<GameObject>(Capacity * 4);
			GameObjectReferences = new Rack<GameObjectReference>(Capacity / 32);
			Tokenized = new Rack<ITokenized>(Capacity);
			Types = new Rack<Type>(Capacity);
			Names = new Rack<string>(Capacity);
			Strings = new Rack<string>(Capacity * 8);
			Objects = new Rack<object>(Capacity * 4);
		}
	}

	public static Cache SharedCache = new Cache(1024);

	public static readonly Encoding Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	public const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public;

	public const FieldAttributes FieldSerializationMask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized;

	public const int TokenRecordBytes = 68;

	public static Dictionary<FieldInfo, FieldSaveVersion> FieldSaveVersionInfo;

	public static IEnumerable<FieldInfo> YieldFields(Type Type, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		FieldInfo[] array = ((Flags == (BindingFlags.Instance | BindingFlags.Public)) ? Type.GetCachedFields() : Type.GetFields(Flags));
		FieldInfo[] array2 = array;
		foreach (FieldInfo fieldInfo in array2)
		{
			if ((fieldInfo.Attributes & Mask) == 0)
			{
				yield return fieldInfo;
			}
		}
	}

	public static ulong GetFieldSaveHash(Type Type, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		return Type.GetStableHashCode64(Flags, Mask);
	}

	public static void CacheFieldSaveVersions()
	{
		if (FieldSaveVersionInfo != null)
		{
			return;
		}
		FieldSaveVersionInfo = new Dictionary<FieldInfo, FieldSaveVersion>(64);
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types)
		{
			if (!typeof(IComposite).IsAssignableFrom(type) && !type.IsSubclassOfGeneric(typeof(IComponent<>)))
			{
				continue;
			}
			FieldInfo[] cachedFields = type.GetCachedFields();
			foreach (FieldInfo fieldInfo in cachedFields)
			{
				if ((fieldInfo.Attributes & (FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)) != 0 || !Attribute.IsDefined(fieldInfo, typeof(FieldSaveVersion)))
				{
					continue;
				}
				FieldSaveVersion value;
				if ((object)fieldInfo.DeclaringType == null || (object)fieldInfo.DeclaringType == type)
				{
					value = fieldInfo.GetCustomAttribute<FieldSaveVersion>();
					if (value.minimumSaveVersion < 395)
					{
						continue;
					}
				}
				else
				{
					FieldInfo field = fieldInfo.DeclaringType.GetField(fieldInfo.Name, BindingFlags.Instance | BindingFlags.Public);
					if (!FieldSaveVersionInfo.TryGetValue(field, out value))
					{
						value = field.GetCustomAttribute<FieldSaveVersion>();
						if (value.minimumSaveVersion < 395)
						{
							continue;
						}
					}
				}
				FieldSaveVersionInfo.TryAdd(fieldInfo, value);
			}
		}
	}
}
