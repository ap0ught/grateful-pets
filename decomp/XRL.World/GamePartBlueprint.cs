using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace XRL.World;

public class GamePartBlueprint
{
	public class PartReflectionCache
	{
		public delegate void SetValue(object target, object value);

		public delegate IPart NewInstance();

		public readonly Type T;

		public readonly NewInstance GetNewInstance;

		public IPartPool Pool;

		private static readonly Type[] DelegateSignature = new Type[2]
		{
			typeof(object),
			typeof(object)
		};

		public readonly ReadOnlyDictionary<string, (Type Type, SetValue SetValue, ObsoleteAttribute Obsolete)> Settables;

		public static readonly Dictionary<Type, PartReflectionCache> CacheByType = new Dictionary<Type, PartReflectionCache>();

		/// <summary>Dynamically emit a casting delegate to directly call the setter of a property without reflection overhead.</summary>
		private SetValue CreateSetterDelegate(PropertyInfo Property)
		{
			MethodInfo setMethod = Property.GetSetMethod(nonPublic: true);
			Type declaringType = Property.DeclaringType;
			Type parameterType = setMethod.GetParameters()[0].ParameterType;
			DynamicMethod dynamicMethod = new DynamicMethod("PropertySetterDelegate_" + Property.Name, typeof(void), DelegateSignature, declaringType, skipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Castclass, declaringType);
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(parameterType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameterType);
			iLGenerator.Emit(setMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, setMethod);
			iLGenerator.Emit(OpCodes.Ret);
			return (SetValue)dynamicMethod.CreateDelegate(typeof(SetValue));
		}

		/// <summary>Dynamically emit a casting delegate to directly set a field without reflection overhead.</summary>
		private SetValue CreateSetterDelegate(FieldInfo Field)
		{
			Type declaringType = Field.DeclaringType;
			Type fieldType = Field.FieldType;
			DynamicMethod dynamicMethod = new DynamicMethod("FieldSetterDelegate_" + Field.Name, typeof(void), DelegateSignature, declaringType, skipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Castclass, declaringType);
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(fieldType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, fieldType);
			iLGenerator.Emit(OpCodes.Stfld, Field);
			iLGenerator.Emit(OpCodes.Ret);
			return (SetValue)dynamicMethod.CreateDelegate(typeof(SetValue));
		}

		/// <summary>Dynamically emit a constructor delegate without reflection overhead.</summary>
		private NewInstance CreateInstanceDelegate(Type Part)
		{
			DynamicMethod dynamicMethod = new DynamicMethod("DefaultConstructorDelegate_" + Part.Name, typeof(IPart), Type.EmptyTypes, Part, skipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			iLGenerator.Emit(OpCodes.Newobj, Part.GetDefaultConstructor());
			iLGenerator.Emit(OpCodes.Ret);
			return (NewInstance)dynamicMethod.CreateDelegate(typeof(NewInstance));
		}

		private PartReflectionCache(Type T)
		{
			this.T = T;
			GetNewInstance = CreateInstanceDelegate(T);
			FieldInfo[] fields = T.GetFields(BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] properties = T.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			Dictionary<string, (Type, SetValue, ObsoleteAttribute)> dictionary = new Dictionary<string, (Type, SetValue, ObsoleteAttribute)>();
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (!fieldInfo.IsLiteral && !fieldInfo.IsInitOnly && !fieldInfo.Name.StartsWith("_"))
				{
					dictionary.TryAdd(fieldInfo.Name, (fieldInfo.FieldType, CreateSetterDelegate(fieldInfo), fieldInfo.GetCustomAttribute<ObsoleteAttribute>()));
				}
			}
			PropertyInfo[] array2 = properties;
			foreach (PropertyInfo propertyInfo in array2)
			{
				if (propertyInfo.CanWrite && !(propertyInfo.Name == "Name"))
				{
					dictionary.TryAdd(propertyInfo.Name, (propertyInfo.PropertyType, CreateSetterDelegate(propertyInfo), propertyInfo.GetCustomAttribute<ObsoleteAttribute>()));
				}
			}
			Settables = new ReadOnlyDictionary<string, (Type, SetValue, ObsoleteAttribute)>(dictionary);
			if (T.GetProperty("Pool", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) != null)
			{
				IPart part = GetNewInstance();
				Pool = part.Pool;
				Pool.Return(part);
			}
		}

		public (Type Type, SetValue SetValue, ObsoleteAttribute obsolete) GetPropertyOrField(string name)
		{
			if (Settables.TryGetValue(name, out (Type, SetValue, ObsoleteAttribute) value))
			{
				return value;
			}
			return (Type: null, SetValue: null, obsolete: null);
		}

		public IPart GetInstance()
		{
			if (Pool != null && Pool.TryGet(out var Part))
			{
				return Part;
			}
			return GetNewInstance();
		}

		public static PartReflectionCache Get(Type T)
		{
			if ((object)T == null)
			{
				return null;
			}
			if (CacheByType.TryGetValue(T, out var value))
			{
				return value;
			}
			value = new PartReflectionCache(T);
			CacheByType.Add(T, value);
			return value;
		}

		public static void ClearOrphaned()
		{
			HashSet<Assembly> assemblies = ModManager.ActiveAssemblies.ToHashSet();
			int num = CacheByType?.RemoveAll((KeyValuePair<Type, PartReflectionCache> x) => !assemblies.Contains(x.Key.Assembly)) ?? 0;
			MetricsManager.LogInfo($"Cleared {num} orphaned part reflection caches.");
		}
	}

	protected struct PartSetter(PartReflectionCache.SetValue SetValue, XmlDataHelper.AttributeParser.ParseDelegate ParseValue, string OriginalValue, object ParsedValue)
	{
		public PartReflectionCache.SetValue SetValue = SetValue;

		public XmlDataHelper.AttributeParser.ParseDelegate ParseValue = ParseValue;

		public string OriginalValue = OriginalValue;

		public object ParsedValue = ParsedValue;

		public object GetParsedValue()
		{
			return ParsedValue ?? ParseValue(OriginalValue);
		}
	}

	public PartReflectionCache Reflector;

	public string Name = "";

	public string Namespace = "XRL.World.Parts";

	public int ChanceOneIn = 1;

	private Type _T;

	protected Dictionary<string, PartSetter> _SettersCache;

	public Dictionary<string, string> Parameters
	{
		[Obsolete("Parameters are now stored parsed.  Use TryGetParameter<T> to get the parsed value, or GetParameterString() to get the original string.  If you really need a full iterator for Parameters to strings, use GetParameterStrings() -- Will remove Q2 2023")]
		get
		{
			return _SettersCache?.ToDictionary((KeyValuePair<string, PartSetter> _) => _.Key, (KeyValuePair<string, PartSetter> _) => _.Value.OriginalValue);
		}
		set
		{
			_SettersCache = new Dictionary<string, PartSetter>(value?.Count ?? 0);
			foreach (KeyValuePair<string, string> item in value)
			{
				if (item.Key == "ChanceOneIn")
				{
					if (int.TryParse(item.Value, out var result))
					{
						ChanceOneIn = result;
					}
					continue;
				}
				if (item.Key == "ChanceIn10000")
				{
					if (int.TryParse(item.Value, out var result2))
					{
						ChanceOneIn = 10000 / result2;
					}
					continue;
				}
				(Type Type, PartReflectionCache.SetValue SetValue, ObsoleteAttribute obsolete) propertyOrField = Reflector.GetPropertyOrField(item.Key);
				var (type, setValue, _) = propertyOrField;
				_ = propertyOrField.obsolete;
				if (type == null || setValue == null)
				{
					type = typeof(string);
				}
				if (item.Key == "Name")
				{
					continue;
				}
				XmlDataHelper.AttributeParser attributeParser = XmlDataHelper.TryGetAttributeParser(type);
				if (attributeParser != null)
				{
					XmlDataHelper.AttributeParser.ParseDelegate parse = attributeParser._Parse;
					object parsedValue = null;
					if (type.IsValueType || type == typeof(string))
					{
						parsedValue = parse(item.Value);
					}
					_SettersCache.Add(item.Key, new PartSetter(setValue, parse, item.Value, parsedValue));
				}
			}
		}
	}

	public Type T => _T ?? (_T = ModManager.ResolveType(Namespace, Name));

	public GamePartBlueprint(string Name)
		: this("XRL.World.Parts", Name)
	{
	}

	public GamePartBlueprint(string Namespace, string Name)
	{
		this.Namespace = Namespace;
		this.Name = Name;
		Reflector = PartReflectionCache.Get(T);
	}

	public IEnumerable<KeyValuePair<string, string>> GetParameterStrings()
	{
		if (_SettersCache == null)
		{
			yield break;
		}
		foreach (KeyValuePair<string, PartSetter> item in _SettersCache)
		{
			yield return new KeyValuePair<string, string>(item.Key, item.Value.OriginalValue);
		}
	}

	public bool HasParameter(string Parameter)
	{
		if (_SettersCache != null)
		{
			return _SettersCache.ContainsKey(Parameter);
		}
		return false;
	}

	public string GetParameterString(string Parameter, string Default = null)
	{
		if (_SettersCache != null && _SettersCache.TryGetValue(Parameter, out var value))
		{
			return value.OriginalValue;
		}
		return Default;
	}

	public T GetParameter<T>(string Name, T Default = default(T))
	{
		if (_SettersCache != null && _SettersCache.TryGetValue(Name, out var value))
		{
			return (T)value.GetParsedValue();
		}
		return Default;
	}

	[Obsolete("Use TryGetParameter")]
	public bool TryGetParameterValue<T>(string Name, out T Value)
	{
		return TryGetParameter<T>(Name, out Value);
	}

	public bool TryGetParameter<T>(string Name, out T Value)
	{
		Value = default;
		if (_SettersCache != null && _SettersCache.TryGetValue(Name, out var value))
		{
			Value = (T)value.GetParsedValue();
			return true;
		}
		return false;
	}

	public void InitializePartInstance(object NewPart)
	{
		if (_SettersCache == null)
		{
			return;
		}
		foreach (KeyValuePair<string, PartSetter> item in _SettersCache)
		{
			PartSetter value = item.Value;
			value.SetValue?.Invoke(NewPart, value.GetParsedValue());
		}
	}

	public void CopyFrom(GamePartBlueprint Source)
	{
		Name = Source.Name;
		Reflector = Source.Reflector;
		_SettersCache = Source._SettersCache;
	}
}
