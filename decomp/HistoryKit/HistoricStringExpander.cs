using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;
using XRL.Language;

namespace HistoryKit;

public static class HistoricStringExpander
{
	public static History nullHistory = new History(0L);

	private static Dictionary<string, string> VariableCache = new Dictionary<string, string>();

	public static Dictionary<string, string> GetVariableCache()
	{
		VariableCache.Clear();
		return VariableCache;
	}

	public static string ExpandQuery(string query, HistoricEntitySnapshot entity, History history, Dictionary<string, string> vars, Dictionary<string, JToken> nodeVars, System.Random R = null)
	{
		string text = query;
		string value = query;
		string text2 = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (history == null)
		{
			if (nullHistory == null)
			{
				nullHistory = new History(0L);
			}
			history = nullHistory;
		}
		if (R == null)
		{
			R = history.r;
		}
		if (query.Length > 0 && query[0] == '<')
		{
			query = query.Substring(1, query.Length - 2);
		}
		if (query.EndsWith(".capitalize"))
		{
			query = query.Remove(query.Length - ".capitalize".Length);
			flag2 = true;
		}
		if (query.EndsWith(".article"))
		{
			query = query.Remove(query.Length - ".article".Length);
			flag = true;
		}
		if (query.EndsWith(".pluralize"))
		{
			query = query.Remove(query.Length - ".pluralize".Length);
			flag3 = true;
		}
		string[] array = query.Split('.');
		if (!vars.IsNullOrEmpty())
		{
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				if (vars.TryGetValue(array[i], out var value2))
				{
					array[i] = value2;
				}
			}
		}
		if (array.Length < 2)
		{
			string text3 = null;
			if (array[0].Contains("="))
			{
				string[] array2 = array[0].Split('=');
				array[0] = array2[1];
				text2 = array2[0];
			}
			else
			{
				text2 = null;
			}
			text3 = array[0];
			if (nodeVars.TryGetValue(text3, out var value3))
			{
				value = value3.ToString();
			}
			else if (!vars.TryGetValue(text3, out value))
			{
				value = text3;
			}
		}
		else
		{
			string text4 = null;
			if (query.Contains("="))
			{
				string[] array3 = query.Split('=');
				text2 = array3[0];
				array[0] = array3[1].Split('.')[0];
			}
			else
			{
				text2 = null;
			}
			text4 = array[0];
			if (text4.StartsWith("spice") || text4[0] == '$')
			{
				JToken jToken = ((!(text4 == "spice")) ? nodeVars[text4] : HistoricSpice.root);
				for (int j = 1; j < array.Length; j++)
				{
					JObject jObject = jToken as JObject;
					if (array[j] == "!random")
					{
						if (j == array.Length - 1)
						{
							if (jToken is IList<JToken> { Count: >0 } list)
							{
								JToken jToken2 = list[R.Next(0, list.Count)];
								if (jToken2 is JProperty jProperty)
								{
									value = jProperty.Name;
									jToken2 = jProperty.Value;
								}
								else
								{
									value = jToken2.ToString();
								}
								if (string.IsNullOrEmpty(text2) || text2[0] != '$')
								{
									break;
								}
								nodeVars[text2] = jToken2;
								return "";
							}
							if (jToken is JValue jValue)
							{
								value = (string?)(JToken?)jValue;
								break;
							}
							Debug.LogError("Invalid randomization root: " + text);
						}
						else
						{
							if (!(jToken is IList<JToken> { Count: >0 } list2))
							{
								Debug.LogError("no spice root after random in " + text);
								return "";
							}
							jToken = list2[R.Next(0, list2.Count)];
							if (jToken is JProperty jProperty2)
							{
								jToken = jProperty2.Value;
							}
						}
						continue;
					}
					if (array[j].StartsWith("entity$"))
					{
						string[] array4 = array[j].Split('$');
						if (array4[1].Contains("["))
						{
							string[] array5 = array4[1].Split('[');
							array5[1] = array5[1].Replace("]", "");
							if (!entity.listProperties.ContainsKey(array5[0]))
							{
								if (!entity.properties.ContainsKey(array4[1]))
								{
									if (jObject?["_failureredirect"] != null)
									{
										StringBuilder stringBuilder = new StringBuilder(jObject["_failureredirect"].ToString());
										for (int k = j; k < array.Length; k++)
										{
											stringBuilder.Append(".");
											stringBuilder.Append(array[k]);
										}
										return ExpandQuery(stringBuilder.ToString(), entity, history, vars, nodeVars);
									}
									if (jObject?["_staticfailureredirect"] != null)
									{
										return ExpandQuery(jObject["_staticfailureredirect"].ToString(), entity, history, vars, nodeVars);
									}
									return "<undefined entity property " + array4[1] + ">";
								}
								return "<undefined entity list " + array5[0] + ">";
							}
							List<string> list3 = entity.GetList(array5[0]);
							if (list3.Count == 0)
							{
								return "<empty entity list " + array5[0] + ">";
							}
							int num2 = 0;
							num2 = ((!(array5[1] == "random")) ? int.Parse(array5[1]) : R.Next(0, list3.Count));
							jToken = jToken[list3[num2]];
						}
						else
						{
							if (!entity.properties.ContainsKey(array4[1]))
							{
								if (jObject?["_failureredirect"] != null)
								{
									StringBuilder stringBuilder2 = new StringBuilder(jObject["_failureredirect"].ToString());
									for (int l = j; l < array.Length; l++)
									{
										stringBuilder2.Append(".");
										stringBuilder2.Append(array[l]);
									}
									return ExpandQuery(stringBuilder2.ToString(), entity, history, vars, nodeVars);
								}
								if (jObject?["_staticfailureredirect"] != null)
								{
									return ExpandQuery(jObject["_staticfailureredirect"].ToString(), entity, history, vars, nodeVars);
								}
								return "<undefined entity property " + array4[1] + ">";
							}
							jToken = jToken[entity.GetProperty(array4[1])];
						}
					}
					else
					{
						try
						{
							jToken = jToken[array[j]];
						}
						catch (Exception)
						{
							jToken = null;
						}
						if (jToken == null)
						{
							if (jObject?["_failureredirect"] != null)
							{
								StringBuilder stringBuilder3 = new StringBuilder(jObject["_failureredirect"].ToString());
								for (int m = j; m < array.Length; m++)
								{
									stringBuilder3.Append('.').Append(array[m]);
								}
								return ExpandQuery(stringBuilder3.ToString(), entity, history, vars, nodeVars);
							}
							if (jObject?["_staticfailureredirect"] != null)
							{
								return ExpandQuery(jObject["_staticfailureredirect"].ToString(), entity, history, vars, nodeVars);
							}
						}
					}
					if (jToken == null)
					{
						MetricsManager.LogError("spice reference " + array[j] + " in " + text + " wasn't a node");
						return "";
					}
					if (j == array.Length - 1)
					{
						if (jToken is JProperty jProperty3)
						{
							jToken = jProperty3.Value;
						}
						if (jToken is JValue jValue2)
						{
							value = (string?)(JToken?)jValue2;
						}
						else
						{
							Debug.LogError("Invalid value at '" + array[j] + "' in '" + text + "' ");
						}
						if (string.IsNullOrEmpty(text2) || text2[0] != '$')
						{
							break;
						}
						nodeVars[text2] = jToken;
						return "";
					}
				}
			}
			else if (text4.StartsWith("entity"))
			{
				HistoricEntitySnapshot historicEntitySnapshot = entity;
				if (text4.StartsWith("entity["))
				{
					historicEntitySnapshot = history.GetEntity(text4.Split('[')[1].Replace("]", "")).GetSnapshotAtYear(history.currentYear);
				}
				if (historicEntitySnapshot == null)
				{
					return "<unknown entity>";
				}
				if (array.Length != 2)
				{
					return "<unknown format " + query + ">";
				}
				if (array[1].Contains("["))
				{
					string[] array6 = array[1].Split('[');
					array6[1] = array6[1].Replace("]", "");
					if (!entity.listProperties.ContainsKey(array6[0]))
					{
						return "<undefined entity list " + array6[0] + ">";
					}
					if (entity.GetList(array6[0]).Count == 0)
					{
						return "<empty entity list " + array6[0] + ">";
					}
					int num3 = 0;
					num3 = ((!(array6[1] == "random")) ? int.Parse(array6[1]) : R.Next(0, entity.GetList(array6[0]).Count));
					value = historicEntitySnapshot.GetList(array6[0])[num3];
				}
				else
				{
					value = historicEntitySnapshot.GetProperty(array[1]);
				}
			}
			else
			{
				text4.StartsWith("history");
			}
		}
		if (value == " ")
		{
			value = "";
		}
		if (flag3 && value.Length > 0)
		{
			if (value[0] != '<')
			{
				value = ((value[0] != '=') ? Grammar.Pluralize(value) : ("=pluralize=" + value));
			}
			else if (value.Substring(value.IndexOf('>') - ".pluralize".Length) != ".pluralize")
			{
				value = value.Insert(value.IndexOf('>'), ".pluralize");
			}
		}
		if (flag && value.Length > 0)
		{
			value = ((value[0] != '=') ? Grammar.A(value, flag2) : ((flag2 ? "=Article=" : "=article=") + value));
			flag2 = false;
		}
		if (flag2 && value.Length > 0)
		{
			if (value[0] != '<')
			{
				value = ((value[0] != '=') ? (char.ToUpper(value[0]) + value.Substring(1)) : ("=capitalize=" + value));
			}
			else if (value.Substring(value.IndexOf('>') - ".capitalize".Length) != ".capitalize")
			{
				value = value.Insert(value.IndexOf('>'), ".capitalize");
			}
		}
		if (text2 != null)
		{
			if (vars.ContainsKey(text2))
			{
				vars[text2] = value;
			}
			else
			{
				vars.Add(text2, value);
			}
			return "";
		}
		if (vars != null)
		{
			foreach (KeyValuePair<string, string> var in vars)
			{
				int num4 = 0;
				while (value.Contains(var.Key) && ++num4 < 5)
				{
					value = value.Replace(var.Key, var.Value);
				}
			}
		}
		return value;
	}

	public static string ExpandString(string input, System.Random Random = null)
	{
		return ExpandString(input, null, null, null, Random);
	}

	public static string ExpandString(string input, HistoricEntitySnapshot entity, History history, Dictionary<string, string> vars = null, System.Random Random = null)
	{
		if (string.IsNullOrEmpty(input))
		{
			return "";
		}
		if (history == null)
		{
			if (nullHistory == null)
			{
				nullHistory = new History(0L);
			}
			history = nullHistory;
		}
		StringBuilder stringBuilder = new StringBuilder(input);
		if (vars == null)
		{
			vars = new Dictionary<string, string>();
		}
		Dictionary<string, JToken> nodeVars = new Dictionary<string, JToken>();
		Match match = Regex.Match(input, "<.*?>");
		int num = 0;
		while (match != null && !string.IsNullOrEmpty(match.Value))
		{
			int index = match.Groups[0].Index;
			int length = match.Groups[0].Length;
			stringBuilder.Remove(index, length);
			stringBuilder.Insert(index, ExpandQuery(match.Groups[0].Value, entity, history, vars, nodeVars, Random));
			if (stringBuilder.Equals(input))
			{
				break;
			}
			num++;
			if (num > 25)
			{
				MetricsManager.LogError("Maximum recursion reached on " + input + " current " + stringBuilder.ToString());
				break;
			}
			match = Regex.Match(stringBuilder.ToString(), "<.*?>");
		}
		if (vars != null)
		{
			foreach (KeyValuePair<string, string> var in vars)
			{
				num = 0;
				while (stringBuilder.Contains(var.Key) && num < 5)
				{
					stringBuilder.Replace(var.Key, var.Value);
					num++;
				}
			}
		}
		return stringBuilder.ToString();
	}
}
