using System;
using XRL.Language;

namespace XRL.World.Capabilities;

public static class StyledStatus
{
	public static string Format(string Name, string Value, string Style = "plain")
	{
		switch (Style)
		{
		case "leet":
			return Event.NewStringBuilder().Append("{{C|[{{c|\\\\\\}}").Append(TextFilters.Leet(Name.ToUpper().Replace(" ", "/")))
				.Append("{{c|::}}")
				.Append(TextFilters.Leet(Value.ToUpper().Replace(" ", "%")))
				.Append("{{c|\\\\\\}}]}}")
				.ToString();
		case "angry":
			return Event.NewStringBuilder().Append(TextFilters.Angry(Name)).Append(": ")
				.Append(TextFilters.Angry(Value))
				.ToString();
		case "plain":
			return Event.NewStringBuilder().Append(Name).Append(": ")
				.Append(Value)
				.ToString();
		case "ooc":
			return Event.NewStringBuilder().Append("{{rules|").Append(Name)
				.Append(": ")
				.Append(Value)
				.Append("}}")
				.ToString();
		case "tech":
			return Event.NewStringBuilder().Append("{{C|[").Append(Name)
				.Append(": ")
				.Append(Value)
				.Append("]}}")
				.ToString();
		case "bio":
			if (Value == "Operational")
			{
				Value = "Healthy";
			}
			return Event.NewStringBuilder().Append("{{g|[").Append(Name)
				.Append(": ")
				.Append(Value)
				.Append("]}}")
				.ToString();
		case "structure":
			return Event.NewStringBuilder().Append("{{Y|[").Append(Name)
				.Append(": ")
				.Append(Value)
				.Append("]}}")
				.ToString();
		default:
			throw new Exception("invalid status style " + Style);
		}
	}
}
