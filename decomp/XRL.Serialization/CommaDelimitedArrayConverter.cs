using System;
using Newtonsoft.Json;

namespace XRL.Serialization;

public class CommaDelimitedArrayConverter : JsonConverter<string[]>
{
	public override void WriteJson(JsonWriter writer, string[] value, JsonSerializer serializer)
	{
		switch (value?.Length ?? 0)
		{
		case 0:
			writer.WriteValue((string?)null);
			break;
		case 1:
			writer.WriteValue(value[0]);
			break;
		default:
			writer.WriteValue(string.Join(',', value));
			break;
		}
	}

	public override string[] ReadJson(JsonReader reader, Type objectType, string[] existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			return serializer.Deserialize<string[]>(reader);
		}
		return ((string)reader.Value)?.Split(',');
	}
}
