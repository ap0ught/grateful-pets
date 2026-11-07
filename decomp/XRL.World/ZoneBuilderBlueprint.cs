using System;
using System.Collections.Generic;
using System.Reflection;

namespace XRL.World;

[Serializable]
public class ZoneBuilderBlueprint : IEquatable<ZoneBuilderBlueprint>
{
	public static Dictionary<int, ZoneBuilderBlueprint> Repository = new Dictionary<int, ZoneBuilderBlueprint>();

	private static readonly Type[] BuildZoneTypes = new Type[1] { typeof(Zone) };

	public string Class;

	public Dictionary<string, object> Parameters;

	[NonSerialized]
	private int Token = -1;

	[NonSerialized]
	private static Dictionary<string, object> ParameterBuffer = new Dictionary<string, object>();

	[NonSerialized]
	private static int Sequence = 0;

	public int ParameterCount
	{
		get
		{
			if (Parameters != null)
			{
				return Parameters.Count;
			}
			return 0;
		}
	}

	public static ZoneBuilderBlueprint Get(string Class)
	{
		return Get(Class, GetParameterBuffer());
	}

	public static ZoneBuilderBlueprint Get(string Class, params object[] Parameters)
	{
		int key = HashCodeOf(Class, Parameters);
		if (!Repository.TryGetValue(key, out var value))
		{
			value = (Repository[key] = new ZoneBuilderBlueprint(Class));
			value.AddParameters(Parameters);
		}
		else if (value.Class != Class || value.ParameterCount != Parameters.Length / 2)
		{
			ZoneBuilderBlueprint zoneBuilderBlueprint2 = new ZoneBuilderBlueprint(Class);
			zoneBuilderBlueprint2.AddParameters(Parameters);
			MetricsManager.LogWarning($"ZoneBuilderBlueprint hash collision: [{zoneBuilderBlueprint2}] x [{value}]");
			value = zoneBuilderBlueprint2;
		}
		return value;
	}

	public static ZoneBuilderBlueprint Get(string Class, string Key1, object Value1)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		return Get(Class, parameterBuffer);
	}

	public static ZoneBuilderBlueprint Get(string Class, string Key1, object Value1, string Key2, object Value2)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		parameterBuffer[Key2] = Value2;
		return Get(Class, parameterBuffer);
	}

	public static ZoneBuilderBlueprint Get(string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		parameterBuffer[Key2] = Value2;
		parameterBuffer[Key3] = Value3;
		return Get(Class, parameterBuffer);
	}

	public static ZoneBuilderBlueprint Get(string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		parameterBuffer[Key2] = Value2;
		parameterBuffer[Key3] = Value3;
		parameterBuffer[Key4] = Value4;
		return Get(Class, parameterBuffer);
	}

	public static ZoneBuilderBlueprint Get(string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		parameterBuffer[Key2] = Value2;
		parameterBuffer[Key3] = Value3;
		parameterBuffer[Key4] = Value4;
		parameterBuffer[Key5] = Value5;
		return Get(Class, parameterBuffer);
	}

	public static ZoneBuilderBlueprint Get(string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5, string Key6, object Value6)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		parameterBuffer[Key2] = Value2;
		parameterBuffer[Key3] = Value3;
		parameterBuffer[Key4] = Value4;
		parameterBuffer[Key5] = Value5;
		parameterBuffer[Key6] = Value6;
		return Get(Class, parameterBuffer);
	}

	public static ZoneBuilderBlueprint Get(string Class, Dictionary<string, object> Parameters)
	{
		int key = HashCodeOf(Class, Parameters);
		if (!Repository.TryGetValue(key, out var value))
		{
			value = (Repository[key] = new ZoneBuilderBlueprint(Class));
			value.Parameters = new Dictionary<string, object>(Parameters);
		}
		else if (value.Class != Class || value.ParameterCount != (Parameters?.Count ?? 0))
		{
			ZoneBuilderBlueprint zoneBuilderBlueprint2 = new ZoneBuilderBlueprint(Class);
			value.Parameters = new Dictionary<string, object>(Parameters);
			MetricsManager.LogWarning($"ZoneBuilderBlueprint hash collision: [{zoneBuilderBlueprint2}] x [{value}]");
			value = zoneBuilderBlueprint2;
		}
		return value;
	}

	/// <summary>Legacy signature support</summary>
	public static ZoneBuilderBlueprint Get(ZoneBuilderBlueprint Blueprint)
	{
		int fullHashCode = Blueprint.GetFullHashCode();
		if (!Repository.TryGetValue(fullHashCode, out var value))
		{
			value = (Repository[fullHashCode] = Blueprint);
		}
		else if (value.Class != Blueprint.Class || value.ParameterCount != Blueprint.ParameterCount)
		{
			MetricsManager.LogWarning($"ZoneBuilderBlueprint hash collision: [{Blueprint}] x [{value}]");
			value = Blueprint;
		}
		return value;
	}

	public ZoneBuilderBlueprint()
	{
	}

	public ZoneBuilderBlueprint(string Class)
	{
		this.Class = Class;
	}

	public ZoneBuilderBlueprint(string Class, string Name, object Value)
	{
		this.Class = Class;
		AddParameter(Name, Value);
	}

	public ZoneBuilderBlueprint(string Class, string Name, object Value, string Name2, object Value2)
	{
		this.Class = Class;
		AddParameter(Name, Value);
		AddParameter(Name2, Value2);
	}

	public ZoneBuilderBlueprint(string Class, string Name, object Value, string Name2, object Value2, string Name3, object Value3)
	{
		this.Class = Class;
		AddParameter(Name, Value);
		AddParameter(Name2, Value2);
		AddParameter(Name3, Value3);
	}

	public ZoneBuilderBlueprint(string Class, string Name, object Value, string Name2, object Value2, string Name3, object Value3, string Name4, object Value4)
	{
		this.Class = Class;
		AddParameter(Name, Value);
		AddParameter(Name2, Value2);
		AddParameter(Name3, Value3);
		AddParameter(Name4, Value4);
	}

	public ZoneBuilderBlueprint(string Class, string Name, object Value, string Name2, object Value2, string Name3, object Value3, string Name4, object Value4, string Name5, object Value5)
	{
		this.Class = Class;
		AddParameter(Name, Value);
		AddParameter(Name2, Value2);
		AddParameter(Name3, Value3);
		AddParameter(Name4, Value4);
		AddParameter(Name5, Value5);
	}

	public ZoneBuilderBlueprint(string Class, string Name, object Value, string Name2, object Value2, string Name3, object Value3, string Name4, object Value4, string Name5, object Value5, string Name6, object Value6)
	{
		this.Class = Class;
		AddParameter(Name, Value);
		AddParameter(Name2, Value2);
		AddParameter(Name3, Value3);
		AddParameter(Name4, Value4);
		AddParameter(Name5, Value5);
		AddParameter(Name6, Value6);
	}

	public void AddParameter(string nam, object val)
	{
		if (Parameters == null)
		{
			Parameters = new Dictionary<string, object>();
		}
		Parameters[nam] = val;
	}

	public void AddParameters(object[] Parameters)
	{
		if (!Parameters.IsNullOrEmpty())
		{
			for (int i = 0; i < Parameters.Length; i += 2)
			{
				AddParameter((string)Parameters[i], Parameters[i + 1]);
			}
		}
	}

	public object GetParameter(string Key, object Default = null)
	{
		return Parameters?.GetValue(Key, Default);
	}

	public T GetParameter<T>(string Key, T Default = default(T))
	{
		if (Parameters != null)
		{
			return (T)Parameters.GetValue(Key, Default);
		}
		return Default;
	}

	public int GetFullHashCode()
	{
		return HashCodeOf(Class, Parameters);
	}

	protected static int HashCodeOf(string Class, Dictionary<string, object> Parameters = null)
	{
		int num = 17;
		if (!Class.IsNullOrEmpty())
		{
			num = num * 23 + Class.GetHashCode();
		}
		if (!Parameters.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, object> Parameter in Parameters)
			{
				num = num * 23 + Parameter.Key.GetHashCode();
				num = num * 23 + (Parameter.Value?.GetHashCode() ?? 0);
			}
		}
		return num;
	}

	protected static int HashCodeOf(string Class, object[] Parameters = null)
	{
		int num = 17;
		if (!Class.IsNullOrEmpty())
		{
			num = num * 23 + Class.GetHashCode();
		}
		if (!Parameters.IsNullOrEmpty())
		{
			for (int i = 0; i < Parameters.Length; i++)
			{
				num = num * 23 + (Parameters[i]?.GetHashCode() ?? 0);
			}
		}
		return num;
	}

	public bool Equals(ZoneBuilderBlueprint other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (other.Class != Class)
		{
			return false;
		}
		if (Parameters.IsNullOrEmpty())
		{
			return other.Parameters.IsNullOrEmpty();
		}
		if (other.Parameters.IsNullOrEmpty() || Parameters.Count != other.Parameters.Count)
		{
			return false;
		}
		foreach (KeyValuePair<string, object> parameter in Parameters)
		{
			if (!other.Parameters.TryGetValue(parameter.Key, out var value) || !value.Equals(parameter.Value))
			{
				return false;
			}
		}
		return true;
	}

	public override string ToString()
	{
		return "ZoneBuilder<" + Class + ">";
	}

	/// <returns>Boolean indicating whether execution should continue (true) or stopped and retried from start (false).</returns>
	public bool ApplyTo(Zone Zone)
	{
		if (Class.IsNullOrEmpty())
		{
			MetricsManager.LogError("Blueprint class is null or empty for: " + Zone.ZoneID);
			return true;
		}
		if (Class.StartsWith("ZoneTemplate:"))
		{
			try
			{
				ZoneTemplateManager.Templates[Class.GetDelimitedSubstring(':', 1)].Execute(Zone);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Applying zone template: " + Class, x);
			}
			return true;
		}
		object obj = Create();
		MethodInfo method = obj.GetType().GetMethod("BuildZone", BuildZoneTypes);
		if (method == null)
		{
			MetricsManager.LogError("No BuildZone method found for class: " + Class);
			return true;
		}
		try
		{
			if (!(bool)method.Invoke(obj, new object[1] { Zone }))
			{
				return false;
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("Executing builder '" + Class + "' for Zone '" + Zone.ZoneID + "'", x2);
			return false;
		}
		return true;
	}

	public static ZoneBuilderBlueprint Load(SerializationReader reader)
	{
		ZoneBuilderBlueprint zoneBuilderBlueprint = new ZoneBuilderBlueprint();
		zoneBuilderBlueprint.Class = reader.ReadString();
		int num = reader.ReadOptimizedInt32();
		for (int i = 0; i < num; i++)
		{
			zoneBuilderBlueprint.AddParameter(reader.ReadString(), reader.ReadObject());
		}
		return zoneBuilderBlueprint;
	}

	public static ZoneBuilderBlueprint GetSerialized(SerializationReader Reader, int Token = -1)
	{
		string text = Reader.ReadString();
		int num = Reader.ReadOptimizedInt32();
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		for (int i = 0; i < num; i++)
		{
			parameterBuffer[Reader.ReadString()] = Reader.ReadObject();
		}
		ZoneBuilderBlueprint zoneBuilderBlueprint = Get(text, parameterBuffer);
		zoneBuilderBlueprint.Token = Token;
		return zoneBuilderBlueprint;
	}

	public static Dictionary<string, object> GetParameterBuffer()
	{
		ParameterBuffer.Clear();
		return ParameterBuffer;
	}

	public void Save(SerializationWriter writer)
	{
		writer.Write(Class);
		if (Parameters == null)
		{
			writer.WriteOptimized(0);
			return;
		}
		writer.WriteOptimized(Parameters.Count);
		foreach (KeyValuePair<string, object> parameter in Parameters)
		{
			writer.Write(parameter.Key);
			writer.WriteObject(parameter.Value);
		}
	}

	public void SaveTokenized(SerializationWriter Writer)
	{
		if (Token >= 0)
		{
			Writer.WriteOptimized(Token);
			return;
		}
		Token = Sequence++;
		Writer.WriteOptimized(Token);
		Save(Writer);
	}

	public static void ClearTokens()
	{
		Sequence = 0;
		foreach (ZoneBuilderBlueprint value in Repository.Values)
		{
			value.Token = -1;
		}
	}

	public static void ClearOrphaned()
	{
		Repository.RemoveAll((KeyValuePair<int, ZoneBuilderBlueprint> x) => x.Value.Token == -1);
	}

	public object Create()
	{
		string text = "XRL.World.ZoneBuilders." + Class;
		Type type = ModManager.ResolveType(text);
		if (type == null)
		{
			MetricsManager.LogError("Unknown builder " + text + "!");
			return true;
		}
		object obj = Activator.CreateInstance(type);
		if (Parameters.IsNullOrEmpty())
		{
			return obj;
		}
		foreach (KeyValuePair<string, object> parameter in Parameters)
		{
			try
			{
				FieldInfo field = type.GetField(parameter.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)field != null && !field.IsInitOnly)
				{
					if (parameter.Value is IConvertible)
					{
						field.SetValue(obj, Convert.ChangeType(parameter.Value, field.FieldType));
					}
					else
					{
						field.SetValue(obj, parameter.Value);
					}
					continue;
				}
				PropertyInfo property = type.GetProperty(parameter.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)property != null && property.CanWrite)
				{
					if (parameter.Value is IConvertible)
					{
						property.SetValue(obj, Convert.ChangeType(parameter.Value, property.PropertyType));
					}
					else
					{
						property.SetValue(obj, parameter.Value);
					}
				}
			}
			catch (Exception ex)
			{
				MetricsManager.LogAssemblyError(type.Assembly, $"Error setting field '{type.FullName}.{parameter.Key}' to '{parameter.Value}': {ex}");
			}
		}
		return obj;
	}
}
