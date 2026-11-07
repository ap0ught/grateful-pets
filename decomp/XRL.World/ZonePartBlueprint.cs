using System;
using System.Collections.Generic;
using System.Reflection;
using XRL.World.ZoneParts;

namespace XRL.World;

[Serializable]
public class ZonePartBlueprint : IEquatable<ZonePartBlueprint>
{
	public static Dictionary<int, ZonePartBlueprint> Repository = new Dictionary<int, ZonePartBlueprint>();

	public string Name;

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

	public ZonePartBlueprint()
	{
	}

	public ZonePartBlueprint(string Name = null, Dictionary<string, object> Parameters = null)
		: this()
	{
		this.Name = Name;
		this.Parameters = Parameters;
	}

	public static ZonePartBlueprint Get(string Name)
	{
		return Get(Name, GetParameterBuffer());
	}

	public static ZonePartBlueprint Get(string Name, params object[] Parameters)
	{
		int key = HashCodeOf(Name, Parameters);
		if (!Repository.TryGetValue(key, out var value))
		{
			value = (Repository[key] = new ZonePartBlueprint(Name));
			value.SetParameters(Parameters);
		}
		else if (value.Name != Name || value.ParameterCount != Parameters.Length / 2)
		{
			ZonePartBlueprint zonePartBlueprint2 = new ZonePartBlueprint(Name);
			zonePartBlueprint2.SetParameters(Parameters);
			MetricsManager.LogWarning($"ZonePartBlueprint hash collision: [{zonePartBlueprint2}] x [{value}]");
			value = zonePartBlueprint2;
		}
		return value;
	}

	public static ZonePartBlueprint Get(string Name, string Key1, object Value1)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		return Get(Name, parameterBuffer);
	}

	public static ZonePartBlueprint Get(string Name, string Key1, object Value1, string Key2, object Value2)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		parameterBuffer[Key2] = Value2;
		return Get(Name, parameterBuffer);
	}

	public static ZonePartBlueprint Get(string Name, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		parameterBuffer[Key2] = Value2;
		parameterBuffer[Key3] = Value3;
		return Get(Name, parameterBuffer);
	}

	public static ZonePartBlueprint Get(string Name, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		parameterBuffer[Key2] = Value2;
		parameterBuffer[Key3] = Value3;
		parameterBuffer[Key4] = Value4;
		return Get(Name, parameterBuffer);
	}

	public static ZonePartBlueprint Get(string Name, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		parameterBuffer[Key2] = Value2;
		parameterBuffer[Key3] = Value3;
		parameterBuffer[Key4] = Value4;
		parameterBuffer[Key5] = Value5;
		return Get(Name, parameterBuffer);
	}

	public static ZonePartBlueprint Get(string Name, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5, string Key6, object Value6)
	{
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		parameterBuffer[Key1] = Value1;
		parameterBuffer[Key2] = Value2;
		parameterBuffer[Key3] = Value3;
		parameterBuffer[Key4] = Value4;
		parameterBuffer[Key5] = Value5;
		parameterBuffer[Key6] = Value6;
		return Get(Name, parameterBuffer);
	}

	public static ZonePartBlueprint Get(string Name, Dictionary<string, object> Parameters)
	{
		int key = HashCodeOf(Name, Parameters);
		if (!Repository.TryGetValue(key, out var value))
		{
			value = (Repository[key] = new ZonePartBlueprint(Name));
			value.Parameters = new Dictionary<string, object>(Parameters);
		}
		else if (value.Name != Name || value.ParameterCount != (Parameters?.Count ?? 0))
		{
			ZonePartBlueprint zonePartBlueprint2 = new ZonePartBlueprint(Name);
			value.Parameters = new Dictionary<string, object>(Parameters);
			MetricsManager.LogWarning($"ZonePartBlueprint hash collision: [{zonePartBlueprint2}] x [{value}]");
			value = zonePartBlueprint2;
		}
		return value;
	}

	public void SetParameter(string Name, object Val)
	{
		if (Parameters == null)
		{
			Parameters = new Dictionary<string, object>();
		}
		Parameters[Name] = Val;
	}

	public void SetParameters(object[] Parameters)
	{
		if (!Parameters.IsNullOrEmpty())
		{
			for (int i = 0; i < Parameters.Length; i += 2)
			{
				SetParameter((string)Parameters[i], Parameters[i + 1]);
			}
		}
	}

	public object GetParameter(string Key, object Default = null)
	{
		return Parameters?.GetValue(Key, Default);
	}

	public T GetParameter<T>(string Key, T Default = default(T))
	{
		object obj = Parameters?.GetValue(Key, Default);
		if (obj is T)
		{
			return (T)obj;
		}
		return Default;
	}

	public int GetFullHashCode()
	{
		return HashCodeOf(Name, Parameters);
	}

	protected static int HashCodeOf(string Name, Dictionary<string, object> Parameters = null)
	{
		int num = 17;
		if (!Name.IsNullOrEmpty())
		{
			num = num * 23 + Name.GetHashCode();
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

	protected static int HashCodeOf(string Name, object[] Parameters = null)
	{
		int num = 17;
		if (!Name.IsNullOrEmpty())
		{
			num = num * 23 + Name.GetHashCode();
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

	public bool Equals(ZonePartBlueprint Other)
	{
		if (Other == null)
		{
			return false;
		}
		if (this == Other)
		{
			return true;
		}
		if (Other.Name != Name)
		{
			return false;
		}
		if (Parameters.IsNullOrEmpty())
		{
			return Other.Parameters.IsNullOrEmpty();
		}
		if (Other.Parameters.IsNullOrEmpty() || Parameters.Count != Other.Parameters.Count)
		{
			return false;
		}
		foreach (KeyValuePair<string, object> parameter in Parameters)
		{
			if (!Other.Parameters.TryGetValue(parameter.Key, out var value) || !value.Equals(parameter.Value))
			{
				return false;
			}
		}
		return true;
	}

	public override string ToString()
	{
		return "ZonePart<" + Name + ">";
	}

	/// <returns>Boolean indicating whether execution should continue (true) or stopped and retried from start (false).</returns>
	public bool ApplyTo(Zone Zone)
	{
		if (Name.IsNullOrEmpty())
		{
			MetricsManager.LogError("Part class is null or empty for: " + Zone.ZoneID);
			return true;
		}
		Zone.AddPart(Create(), Creation: true);
		return true;
	}

	public static ZonePartBlueprint Load(SerializationReader reader)
	{
		ZonePartBlueprint zonePartBlueprint = new ZonePartBlueprint();
		zonePartBlueprint.Name = reader.ReadString();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			zonePartBlueprint.SetParameter(reader.ReadString(), reader.ReadObject());
		}
		return zonePartBlueprint;
	}

	public static ZonePartBlueprint GetSerialized(SerializationReader Reader, int Token = -1)
	{
		string name = Reader.ReadString();
		int num = Reader.ReadInt32();
		Dictionary<string, object> parameterBuffer = GetParameterBuffer();
		for (int i = 0; i < num; i++)
		{
			parameterBuffer[Reader.ReadString()] = Reader.ReadObject();
		}
		ZonePartBlueprint zonePartBlueprint = Get(name, parameterBuffer);
		zonePartBlueprint.Token = Token;
		return zonePartBlueprint;
	}

	public static Dictionary<string, object> GetParameterBuffer()
	{
		ParameterBuffer.Clear();
		return ParameterBuffer;
	}

	public void Save(SerializationWriter writer)
	{
		writer.Write(Name);
		if (Parameters == null)
		{
			writer.Write(-1);
			return;
		}
		writer.Write(Parameters.Count);
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
			Writer.Write(Token);
			return;
		}
		Token = Sequence++;
		Writer.Write(Token);
		Save(Writer);
	}

	public static void ClearTokens()
	{
		Sequence = 0;
		foreach (ZonePartBlueprint value in Repository.Values)
		{
			value.Token = -1;
		}
	}

	public static void ClearOrphaned()
	{
		Repository.RemoveAll((KeyValuePair<int, ZonePartBlueprint> x) => x.Value.Token == -1);
	}

	public IZonePart Create()
	{
		string text = "XRL.World.ZoneParts." + Name;
		Type type = ModManager.ResolveType(text);
		if (type == null)
		{
			MetricsManager.LogError("Unknown zone part " + text + "!");
			return null;
		}
		IZonePart zonePart = Activator.CreateInstance(type) as IZonePart;
		if (Parameters.IsNullOrEmpty())
		{
			return zonePart;
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
						field.SetValue(zonePart, Convert.ChangeType(parameter.Value, field.FieldType));
					}
					else
					{
						field.SetValue(zonePart, parameter.Value);
					}
					continue;
				}
				PropertyInfo property = type.GetProperty(parameter.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)property != null && property.CanWrite)
				{
					if (parameter.Value is IConvertible)
					{
						property.SetValue(zonePart, Convert.ChangeType(parameter.Value, property.PropertyType));
					}
					else
					{
						property.SetValue(zonePart, parameter.Value);
					}
				}
			}
			catch (Exception ex)
			{
				MetricsManager.LogAssemblyError(type.Assembly, $"Error setting field '{type.FullName}.{parameter.Key}' to '{parameter.Value}': {ex}");
			}
		}
		return zonePart;
	}
}
