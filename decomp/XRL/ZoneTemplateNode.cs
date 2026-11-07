using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Genkit;
using UnityEngine;
using XRL.Rules;

namespace XRL;

public abstract class ZoneTemplateNode
{
	public string Name;

	public string Chance;

	public string Criteria;

	public string Filter;

	public string Hint;

	public string Style = "pickeach";

	public int Weight = 1;

	public int MaxApplications;

	public List<ZoneTemplateNode> Children = new List<ZoneTemplateNode>();

	public string VariableReplace(string input, ZoneTemplateGenerationContext Context)
	{
		foreach (string key in Context.Variables.Keys)
		{
			input = input.Replace(key, Context.Variables[key]);
		}
		return input;
	}

	public int CheckChance(ZoneTemplateGenerationContext Context)
	{
		if (string.IsNullOrEmpty(Chance))
		{
			return 1;
		}
		int num = 0;
		string[] array = Chance.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Contains("."))
			{
				throw new NotImplementedException();
			}
			int num2 = Convert.ToInt32(array[i]);
			if (Stat.Random(1, 100) <= num2)
			{
				num++;
			}
		}
		return num;
	}

	public virtual bool TestCriteria(ZoneTemplateGenerationContext Context)
	{
		if (string.IsNullOrEmpty(Criteria))
		{
			return true;
		}
		InfluenceMapRegion influenceMapRegion = Context.Regions.Regions[Context.CurrentRegion];
		string[] array = Criteria.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].ToLower();
			bool flag = false;
			if (text[0] == '!')
			{
				flag = true;
				text = text.Substring(1);
			}
			if (text.StartsWith("regionhassemantictag:"))
			{
				string[] tags = text.Split(':')[1].Split(';');
				if (!Context.currentSector.Cells.Any((Location2D loc) => tags.Any((string tag) => Context.Z.GetCell(loc).HasSemanticTag(tag))))
				{
					return false;
				}
				continue;
			}
			if (text.StartsWith("zonehassemantictag:"))
			{
				string[] tags2 = text.Split(':')[1].Split(';');
				if (!Context.Z.GetSemanticTags().Any((string t) => tags2.Contains(t.ToLower())))
				{
					return false;
				}
				continue;
			}
			switch (text)
			{
			case "furthest":
				if (Context.CurrentRegion == 0)
				{
					return true;
				}
				return false;
			case "pocket":
				if (flag)
				{
					if (influenceMapRegion.AdjacentRegions.Count == 0)
					{
						return false;
					}
				}
				else if (influenceMapRegion.AdjacentRegions.Count != 0)
				{
					return false;
				}
				break;
			case "isolated":
				if (flag)
				{
					if (!influenceMapRegion.ConnectsToTag("connection") && !influenceMapRegion.HasTag("connected"))
					{
						return false;
					}
				}
				else if (influenceMapRegion.HasTag("connected") || influenceMapRegion.ConnectsToTag("connection"))
				{
					return false;
				}
				break;
			case "connection":
				if (flag)
				{
					if (influenceMapRegion.HasTag("connection"))
					{
						return false;
					}
				}
				else if (!influenceMapRegion.HasTag("connection"))
				{
					return false;
				}
				break;
			case "deadend":
				if (flag)
				{
					if (influenceMapRegion.AdjacentRegions.Count == 1)
					{
						return false;
					}
				}
				else if (influenceMapRegion.AdjacentRegions.Count > 1)
				{
					return false;
				}
				break;
			default:
				Debug.LogWarning("Unknown criteria: " + text);
				return false;
			}
		}
		return true;
	}

	public virtual bool Execute(ZoneTemplateGenerationContext Context)
	{
		if (Style == "pickeach")
		{
			for (int i = 0; i < Children.Count; i++)
			{
				if (!Children[i].TestCriteria(Context))
				{
					continue;
				}
				int num = Children[i].CheckChance(Context);
				for (int j = 0; j < num; j++)
				{
					if (!Children[i].Execute(Context))
					{
						return false;
					}
				}
			}
		}
		else
		{
			if (!(Style == "pickone"))
			{
				throw new Exception("Unknown group style " + Style);
			}
			int num2 = 0;
			List<ZoneTemplateNode> list = new List<ZoneTemplateNode>();
			foreach (ZoneTemplateNode child in Children)
			{
				if (child.TestCriteria(Context))
				{
					list.Add(child);
					num2 += child.Weight;
				}
			}
			int num3 = Stat.Random(0, num2 - 1);
			num2 = 0;
			foreach (ZoneTemplateNode item in list)
			{
				if (num3 >= num2 && num3 < num2 + item.Weight)
				{
					if (!item.Execute(Context))
					{
						return false;
					}
					break;
				}
				num2 += item.Weight;
			}
		}
		return true;
	}

	public virtual void Load(XmlReader Reader)
	{
	}
}
