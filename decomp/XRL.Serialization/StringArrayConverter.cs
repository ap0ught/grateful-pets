using System;
using Newtonsoft.Json;

namespace XRL.Serialization;

public class StringArrayConverter : JsonConverter<string[]>
{
	public override void WriteJson(JsonWriter writer, string[] value, JsonSerializer serializer)
	{
		int num = value?.Length ?? 0;
		switch (num)
		{
		case 0:
			writer.WriteValue((string?)null);
			return;
		case 1:
			writer.WriteValue(value[0]);
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < num; i++)
		{
			writer.WriteValue(value[i]);
		}
		writer.WriteEndArray();
	}

	public override string[] ReadJson(JsonReader reader, Type objectType, string[] existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			return serializer.Deserialize<string[]>(reader);
		}
		string text = (string)reader.Value;
		if (text != null)
		{
			return new string[1] { text };
		}
		return null;
	}
}
