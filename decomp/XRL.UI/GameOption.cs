using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Cysharp.Text;
using Newtonsoft.Json;

namespace XRL.UI;

public class GameOption
{
	[JsonConverter(typeof(JsonConverter))]
	public class RequiresSpec
	{
		private record ParsedSpec(string Option, string Value, bool Result);

		public class JsonConverter : JsonConverter<RequiresSpec>
		{
			public override void WriteJson(JsonWriter writer, RequiresSpec value, JsonSerializer serializer)
			{
				int num = value.Parsed.Length;
				switch (num)
				{
				case 0:
					writer.WriteValue((string?)null);
					return;
				case 1:
					writer.WriteValue(value.ToString());
					return;
				}
				writer.WriteStartArray();
				for (int i = 0; i < num; i++)
				{
					writer.WriteValue(value.ToString(i));
				}
				writer.WriteEndArray();
			}

			public override RequiresSpec ReadJson(JsonReader reader, Type objectType, RequiresSpec existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				if (reader.TokenType == JsonToken.StartArray)
				{
					return ParseArray(serializer.Deserialize<string[]>(reader));
				}
				return ParseString((string)reader.Value);
			}
		}

		private ParsedSpec[] Parsed;

		private static readonly Regex partParse = new Regex("^\\s*(?<Option>.*?)\\s*(?<Test>[!=]=)\\s*(?<Value>.*?)\\s*$", RegexOptions.Compiled);

		public bool RequirementsMet
		{
			get
			{
				int i = 0;
				for (int num = Parsed.Length; i < num; i++)
				{
					ParsedSpec parsedSpec = Parsed[i];
					if (Options.GetOption(parsedSpec.Option) == parsedSpec.Value != parsedSpec.Result)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override string ToString()
		{
			using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
			int i = 0;
			for (int num = Parsed.Length; i < num; i++)
			{
				if (i != 0)
				{
					utf16ValueStringBuilder.Append(',');
				}
				utf16ValueStringBuilder.Append(Parsed[i].Option);
				utf16ValueStringBuilder.Append(Parsed[i].Result ? "==" : "!=");
				utf16ValueStringBuilder.Append(Parsed[i].Value);
			}
			return utf16ValueStringBuilder.ToString();
		}

		public string ToString(int Index)
		{
			using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
			utf16ValueStringBuilder.Append(Parsed[Index].Option);
			utf16ValueStringBuilder.Append(Parsed[Index].Result ? "==" : "!=");
			utf16ValueStringBuilder.Append(Parsed[Index].Value);
			return utf16ValueStringBuilder.ToString();
		}

		[XmlDataHelper.AttributeParser(typeof(RequiresSpec))]
		public static RequiresSpec ParseString(string input)
		{
			if (input.IsNullOrEmpty())
			{
				return null;
			}
			return ParseInternal(input.Split(','));
		}

		public static RequiresSpec ParseArray(string[] input)
		{
			if (input.IsNullOrEmpty())
			{
				return null;
			}
			return ParseInternal(input);
		}

		private static RequiresSpec ParseInternal(string[] Specs)
		{
			ParsedSpec[] array = new ParsedSpec[Specs.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Match match = partParse.Match(Specs[i]);
				if (match.Success)
				{
					array[i] = new ParsedSpec(match.Groups["Option"].Value, match.Groups["Value"].Value, match.Groups["Test"].Value == "==");
					continue;
				}
				throw new Exception("Could not parse '" + Specs[i] + "' as a valid option state.");
			}
			return new RequiresSpec
			{
				Parsed = array
			};
		}
	}

	public string ID;

	public string DisplayText;

	public string ConsoleDisplayText;

	public string SwitchDisplayText;

	public string Category;

	public string Type;

	public string Default;

	public string SearchKeywords;

	public CapabilityManager.CapabilityType? RequiresCapability;

	public string HelpText;

	public MethodInfo OnClick;

	public RequiresSpec Requires;

	public string[] DisplayValues = Array.Empty<string>();

	public string[] Values = Array.Empty<string>();

	public int Min;

	public int Max;

	public int Increment;

	public bool Restart;

	public bool RequiresCapabilityMet()
	{
		return CapabilityManager.HasCapability(RequiresCapability);
	}
}
