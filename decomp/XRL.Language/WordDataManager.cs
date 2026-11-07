using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using Ionic.Zip;
using SimpleJSON;

namespace XRL.Language;

public static class WordDataManager
{
	private static Dictionary<string, string> BaseTextCache = new Dictionary<string, string>();

	private static Dictionary<string, JSONNode> RelatedWordsJsonCache = new Dictionary<string, JSONNode>();

	public static string GetBaseText(string text)
	{
		if (text == null)
		{
			return null;
		}
		if (!BaseTextCache.TryGetValue(text, out var value))
		{
			if (ColorUtility.HasFormatting(text))
			{
				text = ColorUtility.StripFormatting(text);
			}
			value = Regex.Replace(text.ToLower(), "[^a-z]+", " ").Trim();
			BaseTextCache[text] = value;
		}
		return value;
	}

	public static List<string> GetRelatedWords(ref string Input, bool ExcludeProper = true, string ExcludePartOfSpeech = null, string RequirePartOfSpeech = null, int MinimumEditDistanceFromEachOther = 0, int MinimumEditDistanceFromInput = 0, int Maximum = int.MaxValue)
	{
		JSONNode relatedWordsJson = GetRelatedWordsJson(ref Input);
		if (relatedWordsJson == null)
		{
			return null;
		}
		if (ExcludePartOfSpeech == "noun")
		{
			ExcludePartOfSpeech = "n";
		}
		else if (ExcludePartOfSpeech == "verb")
		{
			ExcludePartOfSpeech = "v";
		}
		List<string> list = new List<string>();
		int i = 0;
		for (int count = relatedWordsJson.Count; i < count; i++)
		{
			JSONNode jSONNode = relatedWordsJson[i];
			JSONNode jSONNode2 = jSONNode["tags"];
			bool flag = true;
			if (jSONNode2 != null)
			{
				if (flag & ExcludeProper)
				{
					int j = 0;
					for (int count2 = jSONNode2.Count; j < count2; j++)
					{
						if (jSONNode2[j].Value == "prop")
						{
							flag = false;
							break;
						}
					}
				}
				if (flag && ExcludePartOfSpeech != null)
				{
					int k = 0;
					for (int count3 = jSONNode2.Count; k < count3; k++)
					{
						if (jSONNode2[k].Value == ExcludePartOfSpeech)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag && RequirePartOfSpeech != null)
				{
					bool flag2 = false;
					int l = 0;
					for (int count4 = jSONNode2.Count; l < count4; l++)
					{
						if (jSONNode2[l].Value == RequirePartOfSpeech)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						flag = false;
					}
				}
			}
			else if (RequirePartOfSpeech != null)
			{
				flag = false;
			}
			if (!flag)
			{
				continue;
			}
			string value = jSONNode["word"].Value;
			if (flag && MinimumEditDistanceFromInput > 0 && Grammar.LevenshteinDistance(Input, value) < MinimumEditDistanceFromInput)
			{
				flag = false;
			}
			if (flag && MinimumEditDistanceFromEachOther > 0 && list.Count > 0)
			{
				int m = 0;
				for (int count5 = list.Count; m < count5; m++)
				{
					if (Grammar.LevenshteinDistance(list[m], value) < MinimumEditDistanceFromEachOther)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				list.Add(value);
				if (list.Count >= Maximum)
				{
					break;
				}
			}
		}
		return list;
	}

	public static List<string> GetRelatedWords(string Input, bool ExcludeProper = true, string ExcludePartOfSpeech = null, string RequirePartOfSpeech = null, int MinimumEditDistanceFromEachOther = 0, int MinimumEditDistanceFromInput = 0, int Maximum = int.MaxValue)
	{
		return GetRelatedWords(ref Input, ExcludeProper, ExcludePartOfSpeech, RequirePartOfSpeech, MinimumEditDistanceFromEachOther, MinimumEditDistanceFromInput, Maximum);
	}

	private static JSONNode GetRelatedWordsJson(ref string text)
	{
		text = GetBaseText(text);
		if (text == null)
		{
			return null;
		}
		if (!RelatedWordsJsonCache.TryGetValue(text, out var value))
		{
			value = GetRelatedWordsJsonInner(text);
			RelatedWordsJsonCache[text] = value;
		}
		return value;
	}

	private static JSONNode GetRelatedWordsJsonInner(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string text2 = text.Replace(" ", "_");
		JSONNode jSONNode = null;
		string text3 = Uri.EscapeUriString(text);
		string fileName = Path.Combine("WordData", "Related", text.Substring(0, 1));
		string path = DataManager.FilePath(fileName);
		WebClient webClient = new WebClient();
		string text4 = Path.Combine(path, text2 + ".zip");
		if (File.Exists(text4))
		{
			using MemoryStream memoryStream = new MemoryStream();
			using (IEnumerator<ZipEntry> enumerator = ZipFile.Read(text4).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					enumerator.Current.Extract(memoryStream);
				}
			}
			string text5 = Encoding.UTF8.GetString(memoryStream.ToArray());
			if (!string.IsNullOrEmpty(text5))
			{
				jSONNode = JSON.Parse(text5);
			}
		}
		bool flag = false;
		if (jSONNode == null)
		{
			string text6 = DataManager.SavePath(fileName);
			string text7 = Path.Combine(text6, text3 + ".zip");
			if (Directory.Exists(text6) && File.Exists(text7))
			{
				using MemoryStream memoryStream2 = new MemoryStream();
				using (IEnumerator<ZipEntry> enumerator = ZipFile.Read(text7).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						enumerator.Current.Extract(memoryStream2);
					}
				}
				string text8 = Encoding.UTF8.GetString(memoryStream2.ToArray());
				if (!string.IsNullOrEmpty(text8))
				{
					jSONNode = JSON.Parse(text8);
				}
			}
			if (jSONNode == null && !flag)
			{
				string text9 = Path.Combine(text6, text3 + ".json");
				try
				{
					Directory.CreateDirectory(text6);
					flag = true;
					string address = "https://api.datamuse.com/words?ml=" + text3;
					webClient.DownloadFile(address, text9);
					if (File.Exists(text9))
					{
						string text10 = File.ReadAllText(text9, Encoding.UTF8);
						if (!string.IsNullOrEmpty(text10))
						{
							jSONNode = JSON.Parse(text10);
						}
						if (jSONNode != null)
						{
							using ZipFile zipFile = new ZipFile();
							zipFile.AddFile(text9, "");
							zipFile.Save(text7);
						}
						File.Delete(text9);
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogError("WordData Local", x);
				}
			}
		}
		return jSONNode;
	}
}
