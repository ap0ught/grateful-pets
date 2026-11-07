using System;
using System.Collections.Generic;
using System.Text;
using XRL.Annals;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace HistoryKit;

[Serializable]
public class HistoricEntitySnapshot : IComposite
{
	public HistoricEntity entity;

	public Dictionary<string, string> properties = new Dictionary<string, string>();

	public Dictionary<string, List<string>> listProperties = new Dictionary<string, List<string>>();

	[NonSerialized]
	private int _Tier = -1;

	[NonSerialized]
	private int _TechTier = -1;

	public static List<string> _empty = new List<string>();

	public int Tier
	{
		get
		{
			if (_Tier == -1)
			{
				_Tier = XRL.World.Capabilities.Tier.Constrain(int.Parse(GetProperty("tier")));
			}
			return _Tier;
		}
	}

	public int TechTier
	{
		get
		{
			if (_TechTier == -1)
			{
				_TechTier = (hasProperty("techTier") ? XRL.World.Capabilities.Tier.Constrain(int.Parse(GetProperty("techTier"))) : Tier);
			}
			return _TechTier;
		}
	}

	public string Name => GetProperty("name");

	public string sacredThing => QudHistoryHelpers.GetRandomProperty(this, GetProperty("defaultSacredThing"), "sacredThings");

	public string profaneThing => QudHistoryHelpers.GetRandomProperty(this, GetProperty("defaultProfaneThing"), "profaneThings");

	public HistoricEntitySnapshot()
	{
	}

	public HistoricEntitySnapshot(HistoricEntity sourceEntity)
	{
		entity = sourceEntity;
	}

	public void setProperty(string name, string value)
	{
		if (properties.ContainsKey(name))
		{
			properties[name] = value;
		}
		else
		{
			properties.Add(name, value);
		}
		if (name == "tier" || name == "techTier")
		{
			_Tier = -1;
			_TechTier = -1;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("---snapshot-----");
		stringBuilder.AppendLine("|");
		stringBuilder.AppendLine("|--properties---");
		stringBuilder.AppendLine("|");
		foreach (KeyValuePair<string, string> property in properties)
		{
			stringBuilder.Append("| ");
			stringBuilder.Append(property.Key + " = " + property.Value);
		}
		if (listProperties.Count > 0)
		{
			stringBuilder.AppendLine("|");
			stringBuilder.AppendLine("|");
			stringBuilder.AppendLine("|");
			stringBuilder.AppendLine("|--list properties---");
			foreach (KeyValuePair<string, List<string>> listProperty in listProperties)
			{
				stringBuilder.AppendLine("|");
				stringBuilder.Append("| <" + listProperty.Key + ">");
				foreach (string item in listProperty.Value)
				{
					stringBuilder.Append("| ");
					stringBuilder.Append(item);
				}
			}
		}
		return stringBuilder.ToString();
	}

	public bool hasProperty(string name)
	{
		return properties.ContainsKey(name);
	}

	public bool hasListProperty(string name)
	{
		return listProperties.ContainsKey(name);
	}

	public string GetRandomElementFromListProperty(string Name, string Default = null, Random R = null)
	{
		if (!listProperties.TryGetValue(Name, out var value) || value.Count == 0)
		{
			return Default;
		}
		if (R == null)
		{
			R = Stat.Rand;
		}
		return listProperties[Name][R.Next(0, listProperties[Name].Count)];
	}

	public List<string> GetList(string name)
	{
		if (listProperties.TryGetValue(name, out var value))
		{
			return value;
		}
		return _empty;
	}

	public string GetProperty(string name, string defaultValue = "unknown")
	{
		if (properties.TryGetValue(name, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public HistoricPerspective getPerspective(HistoricEvent theEvent)
	{
		return theEvent.GetPerspective(this);
	}

	public HistoricPerspective requirePerspective(HistoricEvent theEvent, object useFeeling = null)
	{
		return theEvent.RequirePerspective(this, useFeeling);
	}

	public HistoricEvent findEventBySameEntityPerspectiveColors(string main, string support)
	{
		foreach (HistoricEvent @event in entity.history.events)
		{
			if (@event.perspectives != null && @event.perspectives.ContainsKey(entity.id))
			{
				HistoricPerspective historicPerspective = @event.perspectives[entity.id];
				if (historicPerspective.mainColor == main && historicPerspective.supportColor == support)
				{
					return @event;
				}
			}
		}
		return null;
	}

	public void supplyPerspectiveColors(HistoricPerspective perspective)
	{
		List<string> list = GetList("palette");
		string text = null;
		string text2 = null;
		int num = 0;
		if (list != null)
		{
			do
			{
				text = list.GetRandomElement();
				text2 = list.GetRandomElement();
				if (!(text == text2))
				{
					continue;
				}
				foreach (string item in list)
				{
					if (item != text)
					{
						text2 = item;
						break;
					}
				}
			}
			while (findEventBySameEntityPerspectiveColors(text, text2) != null && ++num < 5);
		}
		else
		{
			do
			{
				text = Crayons.GetRandomColor();
				text2 = Crayons.GetRandomColor();
			}
			while (text == text2 || (findEventBySameEntityPerspectiveColors(text, text2) != null && ++num < 5));
		}
		perspective.mainColor = text;
		perspective.supportColor = text2;
	}
}
