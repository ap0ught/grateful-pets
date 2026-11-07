using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using XRL.Language;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL;

[HasModSensitiveStaticCache]
public static class Templates
{
	public interface ITemplateNode
	{
		void Build(StringBuilder SB, StatCollector values);
	}

	public class StatCollector
	{
		public enum BonusType
		{
			Linear,
			Percentage
		}

		public struct ModifierInfo
		{
			public int Bonus;

			public BonusType Type;

			public string Source;
		}

		public static StatCollector Instance = new StatCollector();

		public string mode;

		public string prefix = "";

		public string postfix = "";

		public Dictionary<string, (string text, bool changes, int changedState)> values = new Dictionary<string, (string, bool, int)>();

		public Dictionary<string, List<ModifierInfo>> modifiers = new Dictionary<string, List<ModifierInfo>>();

		public List<string> changes = new List<string>();

		public StatCollector Clear()
		{
			values.Clear();
			modifiers.Clear();
			prefix = "";
			postfix = "";
			return this;
		}

		public StatCollector Reset(string mode)
		{
			this.mode = mode;
			return Clear();
		}

		public void Set(string Key, string Value, bool changes = false, int changedState = 0)
		{
			values.Set(Key, (Value, changes, changedState));
		}

		public void Set(string Key, int Value, bool changes = false, int changedState = 0)
		{
			values.Set(Key, (Value.ToString(), changes, changedState));
		}

		public string Get(string Key)
		{
			if (!values.TryGetValue(Key ?? "", out (string, bool, int) value))
			{
				return null;
			}
			return value.Item1;
		}

		public void RenderPrefix(StringBuilder SB)
		{
			if (!string.IsNullOrEmpty(prefix))
			{
				SB.Append(prefix);
			}
		}

		public void RenderPostfix(StringBuilder SB)
		{
			if (!string.IsNullOrEmpty(postfix))
			{
				SB.Append(postfix);
			}
		}

		public void Render(StringBuilder SB, string Key, string Unit = null)
		{
			(string, bool, int) value;
			if (Key == "prefix")
			{
				SB.Append(prefix);
			}
			else if (Key == "postfix")
			{
				SB.Append(postfix);
			}
			else if (values.TryGetValue(Key, out value))
			{
				if (value.Item3 > 0)
				{
					SB.AppendColored("G", value.Item1);
				}
				else if (value.Item3 < 0)
				{
					SB.AppendColored("R", value.Item1);
				}
				else if (value.Item2)
				{
					SB.AppendColored("rules", value.Item1);
				}
				else
				{
					SB.Append(value.Item1);
				}
				if (Unit != null)
				{
					if (int.TryParse(value.Item1, out var result))
					{
						SB.Append(" ").Append((Math.Abs(result) != 1) ? Grammar.Pluralize(Unit) : Unit);
					}
					else
					{
						SB.Append(" ").Append(Unit);
					}
				}
			}
			else
			{
				SB.AppendColored("K", Key);
			}
		}

		public int CollectComputePowerAdjustDown(ActivatedAbilityEntry ability, string what, int baseValue, float factor, int floorDivisor = 2)
		{
			int num = GetAvailableComputePowerEvent.AdjustDown(ability.ParentObject, baseValue, factor, floorDivisor);
			AddComputePowerPostfix(what, num - baseValue);
			return num;
		}

		public int CollectComputePowerAdjustDown(ActivatedAbilityEntry ability, string what, int baseValue, float factor = 1f)
		{
			int num = GetAvailableComputePowerEvent.AdjustDown(ability.ParentObject, baseValue, factor);
			AddComputePowerPostfix(what, num - baseValue);
			return num;
		}

		public int CollectComputePowerAdjustUp(ActivatedAbilityEntry ability, string what, int baseValue, float factor = 1f)
		{
			int num = GetAvailableComputePowerEvent.AdjustUp(ability.ParentObject, baseValue, factor);
			AddComputePowerPostfix(what, num - baseValue);
			return num;
		}

		public (DieRoll resultRange, int changeSign) CollectComputePowerAdjustRangeUp(ActivatedAbilityEntry ability, string what, DieRoll range, float factor = 1f)
		{
			int num = GetAvailableComputePowerEvent.AdjustUp(ability.ParentObject, range.Min(), factor);
			int num2 = GetAvailableComputePowerEvent.AdjustUp(ability.ParentObject, range.Max(), factor);
			int item = AddComputePowerPostfix(what, num - range.Min(), num2 - range.Max());
			return (resultRange: $"{num}-{num2}".GetCachedDieRoll(), changeSign: item);
		}

		public int AddChangePostfix(string what, int change, string dueTo)
		{
			if (change > 0)
			{
				postfix += $"\n{what} increased by {change} due to {dueTo}.";
				return 1;
			}
			if (change < 0)
			{
				postfix += $"\n{what} reduced by {-change} due to {dueTo}.";
				return -1;
			}
			return 0;
		}

		public int AddPercentChangePostfix(string what, int change, string dueTo)
		{
			if (change > 0)
			{
				postfix += $"\n{what} increased by {change}% due to {dueTo}.";
				return 1;
			}
			if (change < 0)
			{
				postfix += $"\n{what} reduced by {-change}% due to {dueTo}.";
				return -1;
			}
			return 0;
		}

		public int AddChangePostfix(string what, int changeMin, int changeMax, string dueTo)
		{
			if (changeMin == changeMax)
			{
				return AddChangePostfix(what, changeMin, dueTo);
			}
			if (changeMin > 0 || changeMax > 0)
			{
				postfix += $"\n{what} increased by {changeMin}-{changeMax} due to {dueTo}.";
				return 1;
			}
			if (changeMax < 0 || changeMin < 0)
			{
				postfix += $"\n{what} reduced by {-changeMax}-{-changeMin} due to {dueTo}.";
				return -1;
			}
			return 0;
		}

		public void AddComputePowerPostfix(string what, int change)
		{
			AddChangePostfix(what, change, "compute power");
		}

		public int AddComputePowerPostfix(string what, int changeMin, int changeMax)
		{
			return AddChangePostfix(what, changeMin, changeMax, "compute power");
		}

		public void CollectCooldownTurns(ActivatedAbilityEntry ability, int BaseTurns, int previousCooldownDiff = 0)
		{
			if (mode.Contains("ability") && ability != null)
			{
				GetCooldownEvent getCooldownEvent = GetCooldownEvent.TryCalculateFor(ability.ParentObject, ability, BaseTurns * 10);
				if (getCooldownEvent != null)
				{
					int num = -(getCooldownEvent.Result - getCooldownEvent.Base);
					int num2 = num + previousCooldownDiff;
					Set("Cooldown", (getCooldownEvent.Result == 60 && getCooldownEvent.ResultUncapped != 60) ? 5 : ((int)Math.Ceiling((float)getCooldownEvent.Result / 10f)), num2 != 0, num2);
					if (num == 0)
					{
						return;
					}
					foreach (GetCooldownEvent.CooldownCalculation calculation in getCooldownEvent.Calculations)
					{
						num = 0;
						if (calculation.PercentageReduction != 0)
						{
							num -= calculation.PercentageReduction * BaseTurns / 100;
						}
						if (calculation.LinearReduction != 0)
						{
							num -= calculation.LinearReduction / 10;
						}
						if (num > 0)
						{
							postfix += $"\nCooldown increased by {num} due to {calculation.Reason}.";
						}
						else if (num < 0)
						{
							postfix += $"\nCooldown reduced by {-num} due to {calculation.Reason}.";
						}
					}
					if (getCooldownEvent.ResultUncapped != getCooldownEvent.Result)
					{
						string text = ((getCooldownEvent.Result == 60) ? "5" : (getCooldownEvent.Result / 10).ToString());
						postfix = postfix + "\nCooldown cannot be reduced below " + text + " rounds.";
					}
				}
				else
				{
					Set("Cooldown", BaseTurns);
				}
			}
			else
			{
				Set("Cooldown", BaseTurns);
			}
		}

		public void CollectCooldownTurns(ActivatedAbilityEntry ability, string BaseTurns)
		{
			if (mode.Contains("ability") && ability != null)
			{
				int num = BaseTurns.RollMinCached() * 10;
				int num2 = BaseTurns.RollMaxCached() * 10;
				GetCooldownEvent getCooldownEvent = GetCooldownEvent.TryCalculateFor(ability.ParentObject, ability, num);
				if (getCooldownEvent != null)
				{
					GetCooldownEvent getCooldownEvent2 = GetCooldownEvent.TryCalculateFor(ability.ParentObject, ability, num2);
					int num3 = -(getCooldownEvent2.Result - getCooldownEvent2.Base);
					if (getCooldownEvent.Result == getCooldownEvent2.Result)
					{
						Set("Cooldown", (int)Math.Ceiling((float)getCooldownEvent.Result / 10f), num3 != 0, num3);
					}
					else
					{
						Set("Cooldown", (int)Math.Ceiling((float)getCooldownEvent.Result / 10f) + "-" + (int)Math.Ceiling((float)getCooldownEvent2.Result / 10f), num3 != 0, num3);
					}
					if (num3 == 0)
					{
						return;
					}
					foreach (GetCooldownEvent.CooldownCalculation calculation in getCooldownEvent2.Calculations)
					{
						if (calculation.PercentageReduction > 0)
						{
							postfix += $"\nCooldown reduced by {calculation.PercentageReduction}% due to {calculation.Reason}.";
						}
						else if (calculation.PercentageReduction < 0)
						{
							postfix += $"\nCooldown increased by {-calculation.PercentageReduction}% due to {calculation.Reason}.";
						}
						if (calculation.LinearReduction > 0)
						{
							postfix += $"\nCooldown reduced by {calculation.LinearReduction / 10} due to {calculation.Reason}.";
						}
						else if (calculation.LinearReduction < 0)
						{
							postfix += $"\nCooldown increased by {-calculation.LinearReduction / 10} due to {calculation.Reason}.";
						}
					}
					if (getCooldownEvent.ResultUncapped != getCooldownEvent.Result)
					{
						string text = ((getCooldownEvent.Result == 60) ? "5" : (getCooldownEvent.Result / 10).ToString());
						postfix = postfix + "\nCooldown cannot be reduced below " + text + " rounds.";
					}
				}
				else
				{
					Set("Cooldown", BaseTurns);
				}
			}
			else
			{
				Set("Cooldown", BaseTurns);
			}
		}

		public void AddLinearBonusModifier(string stat, int bonus, string source)
		{
			if (!modifiers.TryGetValue(stat, out var value))
			{
				value = new List<ModifierInfo>();
				modifiers.Add(stat, value);
			}
			value.Add(new ModifierInfo
			{
				Bonus = bonus,
				Type = BonusType.Linear,
				Source = source
			});
		}

		public void AddPercentageBonusModifier(string stat, int bonus, string source)
		{
			if (!modifiers.TryGetValue(stat, out var value))
			{
				value = new List<ModifierInfo>();
				modifiers.Add(stat, value);
			}
			value.Add(new ModifierInfo
			{
				Bonus = bonus,
				Type = BonusType.Percentage,
				Source = source
			});
		}

		public int CollectBonusModifiers(string stat, int baseValue, string statDisplayName = null, bool increaseAsGood = true)
		{
			long num = baseValue;
			if (string.IsNullOrEmpty(statDisplayName))
			{
				statDisplayName = stat;
			}
			if (modifiers.TryGetValue(stat, out var value))
			{
				foreach (ModifierInfo item in value.Where((ModifierInfo bonus) => bonus.Type == BonusType.Linear && bonus.Bonus > 0))
				{
					AddChangePostfix(statDisplayName, item.Bonus, item.Source);
					num += item.Bonus;
				}
				int num2 = 100;
				foreach (ModifierInfo item2 in value.Where((ModifierInfo bonus) => bonus.Type == BonusType.Percentage && bonus.Bonus > 0))
				{
					AddPercentChangePostfix(statDisplayName, item2.Bonus, item2.Source);
					num2 += item2.Bonus;
				}
				num *= num2;
				num2 = 100;
				foreach (ModifierInfo item3 in value.Where((ModifierInfo bonus) => bonus.Type == BonusType.Percentage && bonus.Bonus < 0))
				{
					AddPercentChangePostfix(statDisplayName, item3.Bonus, item3.Source);
					num2 += item3.Bonus;
				}
				num *= num2;
				num /= 10000;
				foreach (ModifierInfo item4 in value.Where((ModifierInfo bonus) => bonus.Type == BonusType.Linear && bonus.Bonus < 0))
				{
					AddChangePostfix(statDisplayName, item4.Bonus, item4.Source);
					num += item4.Bonus;
				}
			}
			int num3 = (int)num;
			Set(stat, num3, num3 != baseValue, (num3 - baseValue) * (increaseAsGood ? 1 : (-1)));
			return num3;
		}
	}

	public class StatLineNode : StatNode
	{
		public string DisplayName;

		public string HideValue;

		public new const string NODE_NAME = "statline";

		public StatLineNode(XmlDataHelper xml)
			: base(xml)
		{
			DisplayName = xml.ParseAttribute("DisplayName", Key);
			HideValue = xml.ParseAttribute("HideValue", HideValue);
		}

		public new static IEnumerable<ITemplateNode> Parse(XmlDataHelper xml, XMLTemplateParserAdditionalData data)
		{
			yield return new StatLineNode(xml);
			xml.DoneWithElement();
			yield return NEWLINE;
		}

		public override void Build(StringBuilder SB, StatCollector values)
		{
			if (HideValue == null || !(HideValue == values.Get(Key)))
			{
				if (Key == null)
				{
					SB.Append("{{K|%%}}");
					return;
				}
				SB.Append(DisplayName).Append(": ");
				base.Build(SB, values);
			}
		}
	}

	public class StatNode : ITemplateNode
	{
		public string Key;

		public List<string> Filters;

		public string Unit;

		public static Dictionary<string, string> DefaultUnits = new Dictionary<string, string>
		{
			{ "Cooldown", "round" },
			{ "Duration", "round" }
		};

		public const string NODE_NAME = "stat";

		public StatNode(XmlDataHelper xml)
		{
			Key = xml.ParseAttribute<string>("Name", null, required: true);
			Filters = xml.ParseAttribute<string>("Filters", null).CachedCommaExpansion();
			string value = null;
			DefaultUnits.TryGetValue(Key, out value);
			Unit = xml.ParseAttribute("Unit", value);
		}

		public StatNode(string Key, string Filters)
		{
			this.Key = Key;
			this.Filters = Filters.CachedCommaExpansion();
		}

		public static IEnumerable<ITemplateNode> Parse(XmlDataHelper xml, XMLTemplateParserAdditionalData data)
		{
			yield return new StatNode(xml);
			xml.DoneWithElement();
		}

		public virtual void Build(StringBuilder SB, StatCollector values)
		{
			if (Key == null)
			{
				SB.Append("{{K|%%}}");
			}
			else
			{
				values?.Render(SB, Key, Unit);
			}
		}
	}

	public class SwitchNode : ITemplateNode
	{
		public string Key;

		public Dictionary<string, CaseNode> caseNodes = new Dictionary<string, CaseNode>();

		public CaseNode defaultNode;

		public const string NODE_NAME = "switch";

		public SwitchNode(XmlDataHelper xml)
		{
			Key = xml.ParseAttribute<string>("Name", null, required: true);
			xml.HandleNodes(new Dictionary<string, Action<XmlDataHelper>>
			{
				{ "case", ParseCaseNode },
				{ "default", ParseDefaultNode }
			});
		}

		public SwitchNode(string Key)
		{
			this.Key = Key;
		}

		public static IEnumerable<ITemplateNode> Parse(XmlDataHelper xml, XMLTemplateParserAdditionalData data)
		{
			yield return new SwitchNode(xml);
		}

		public void ParseCaseNode(XmlDataHelper xml)
		{
			CaseNode caseNode = new CaseNode(xml, isDefault: false);
			caseNodes.Add(caseNode.Value, caseNode);
		}

		public void ParseDefaultNode(XmlDataHelper xml)
		{
			if (defaultNode != null)
			{
				throw new Exception("Multiple 'default' nodes.");
			}
			CaseNode caseNode = new CaseNode(xml, isDefault: true);
			defaultNode = caseNode;
		}

		public virtual void Build(StringBuilder SB, StatCollector values)
		{
			string text = values?.Get(Key);
			if (text != null && caseNodes.TryGetValue(text, out var value))
			{
				value.Build(SB, values);
			}
			else if (defaultNode != null)
			{
				defaultNode.Build(SB, values);
			}
		}
	}

	public class CaseNode : ITemplateNode
	{
		public string Value;

		public List<ITemplateNode> Nodes = new List<ITemplateNode>();

		public const string NODE_NAME = "case";

		public CaseNode(XmlDataHelper xml, bool isDefault)
		{
			if (!isDefault)
			{
				Value = xml.ParseAttribute("Value", Value, required: true);
			}
			Nodes.Clear();
			Nodes.AddRange(ParseXMLTemplateNodes(xml, ignoreWhitespace: false));
		}

		public CaseNode()
		{
		}

		public virtual void Build(StringBuilder SB, StatCollector values)
		{
			foreach (ITemplateNode node in Nodes)
			{
				node.Build(SB, values);
			}
		}
	}

	public class Template
	{
		public string ID = "";

		public List<ITemplateNode> Nodes = new List<ITemplateNode>();

		public static StringBuilder SB = new StringBuilder();

		public static Template HandleTemplateNode(XmlDataHelper xml)
		{
			string text = xml.ParseAttribute<string>("ID", null, required: true);
			if (!TemplateByID.TryGetValue(text, out var value))
			{
				value = new Template();
				value.ID = text;
			}
			value.Nodes.Clear();
			value.Nodes.AddRange(ParseXMLTemplateNodes(xml));
			return value;
		}

		public string Build(Action<StatCollector> act, string mode = null)
		{
			act(StatCollector.Instance.Reset(mode));
			return Build(StatCollector.Instance);
		}

		public string Build(StatCollector values)
		{
			SB.Clear();
			values?.Render(SB, "prefix");
			foreach (ITemplateNode node in Nodes)
			{
				node.Build(SB, values);
			}
			values?.Render(SB, "postfix");
			return SB.ToString();
		}
	}

	public class TemplateNode : ITemplateNode
	{
		public string ID;

		public bool affixes;

		public const string NODE_NAME = "template";

		public TemplateNode(XmlDataHelper xml)
		{
			ID = xml.ParseAttribute<string>("ID", null, required: true);
			affixes = xml.ParseAttribute("affixes", defaultValue: false);
			xml.DoneWithElement();
		}

		public static IEnumerable<ITemplateNode> Parse(XmlDataHelper xml, XMLTemplateParserAdditionalData data)
		{
			yield return new TemplateNode(xml);
		}

		public void Build(StringBuilder SB, StatCollector values)
		{
			if (ID == null || !TemplateByID.TryGetValue(ID, out var value))
			{
				return;
			}
			foreach (ITemplateNode node in value.Nodes)
			{
				node.Build(SB, values);
			}
		}
	}

	public class TextNode : ITemplateNode
	{
		public string Text;

		public TextNode(string Text)
		{
			this.Text = Text;
		}

		public void Build(StringBuilder SB, StatCollector values)
		{
			SB.Append(Text);
		}
	}

	public struct XMLTemplateParserAdditionalData(int siblingIndex)
	{
		public int siblingIndex = siblingIndex;

		public bool firstChild => siblingIndex == 0;
	}

	public delegate IEnumerable<ITemplateNode> XMLTemplateParser(XmlDataHelper xml, XMLTemplateParserAdditionalData data);

	[ModSensitiveStaticCache(true)]
	public static Dictionary<string, Template> TemplateByID = new Dictionary<string, Template>();

	public static Dictionary<string, Action<XmlDataHelper>> _nodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "templates", HandleNodes },
		{ "template", ReadTemplateNode }
	};

	private static Dictionary<string, XMLTemplateParser> TemplateParsers = new Dictionary<string, XMLTemplateParser>(StringComparer.OrdinalIgnoreCase)
	{
		{ "p", ParseP },
		{ "br", ParseBR },
		{
			"statline",
			StatLineNode.Parse
		},
		{
			"stat",
			StatNode.Parse
		},
		{
			"template",
			TemplateNode.Parse
		},
		{
			"switch",
			SwitchNode.Parse
		}
	};

	public static TextNode NEWLINE = new TextNode("\n");

	[ModSensitiveCacheInit]
	public static void Init()
	{
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("templates"))
		{
			HandleNodes(item);
		}
	}

	public static void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(_nodes);
	}

	public static void ReadTemplateNode(XmlDataHelper xml)
	{
		Template template = Template.HandleTemplateNode(xml);
		if (template != null)
		{
			TemplateByID[template.ID] = template;
		}
	}

	public static void LoadTemplateFromExternal(string ID, XmlDataHelper xml)
	{
		if (!TemplateByID.TryGetValue(ID, out var value))
		{
			value = new Template();
			value.ID = ID;
		}
		value.Nodes.Clear();
		value.Nodes.AddRange(ParseXMLTemplateNodes(xml));
		TemplateByID[ID] = value;
	}

	public static IEnumerable<ITemplateNode> ParseP(XmlDataHelper xml, XMLTemplateParserAdditionalData data)
	{
		foreach (ITemplateNode item in ParseXMLTemplateNodes(xml, ignoreWhitespace: false))
		{
			yield return item;
		}
		yield return NEWLINE;
	}

	public static IEnumerable<ITemplateNode> ParseBR(XmlDataHelper xml, XMLTemplateParserAdditionalData data)
	{
		yield return NEWLINE;
		xml.DoneWithElement();
	}

	public static IEnumerable<ITemplateNode> ParseXMLTemplateNodes(XmlDataHelper xml, bool ignoreWhitespace = true)
	{
		xml.WhitespaceHandling = WhitespaceHandling.All;
		if (xml.IsEmptyElement || xml.NodeType == XmlNodeType.EndElement)
		{
			yield break;
		}
		XMLTemplateParserAdditionalData data = new XMLTemplateParserAdditionalData(0);
		string NameAtOpen = xml.Name;
		while (true)
		{
			if (xml.Read())
			{
				switch (xml.NodeType)
				{
				case XmlNodeType.EndElement:
					if (!(xml.Name == NameAtOpen))
					{
						xml.ParseWarning($"Unexpected EndElement for \"{xml.Name}\"");
						goto default;
					}
					yield break;
				case XmlNodeType.Whitespace:
				case XmlNodeType.SignificantWhitespace:
					if (ignoreWhitespace)
					{
						break;
					}
					yield return new TextNode(xml.Value);
					goto default;
				case XmlNodeType.Text:
					yield return new TextNode(xml.Value);
					goto default;
				case XmlNodeType.Element:
				{
					if (TemplateParsers.TryGetValue(xml.Name, out var value))
					{
						foreach (ITemplateNode item in value(xml, data))
						{
							yield return item;
						}
					}
					else
					{
						xml.ParseWarning("Unknown " + xml.Name + " node in template");
					}
					goto default;
				}
				default:
					data = new XMLTemplateParserAdditionalData(data.siblingIndex + 1);
					break;
				}
				continue;
			}
			xml.WhitespaceHandling = WhitespaceHandling.None;
			break;
		}
	}
}
