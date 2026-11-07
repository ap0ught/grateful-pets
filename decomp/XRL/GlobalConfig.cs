using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL;

[HasModSensitiveStaticCache]
public static class GlobalConfig
{
	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, string> StringSettings;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, int> IntSettings;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, bool> BoolSettings;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, float> FloatSettings;

	public static void LoadGlobalConfig()
	{
		IntSettings = new Dictionary<string, int>();
		BoolSettings = new Dictionary<string, bool>();
		FloatSettings = new Dictionary<string, float>();
		NameValueBag nameValueBag = new NameValueBag(DataManager.FilePath("GlobalConfig.json"));
		nameValueBag.Load();
		StringSettings = nameValueBag.Bag;
		ModManager.ForEachFile("GlobalConfig.json", (string fileName) =>
		{
			NameValueBag nameValueBag2 = new NameValueBag(fileName);
			nameValueBag2.Load();
			foreach (string key in nameValueBag2.Bag.Keys)
			{
				if (StringSettings.ContainsKey(key))
				{
					StringSettings[key] = nameValueBag2.Bag[key];
				}
				else
				{
					StringSettings.Add(key, nameValueBag2.Bag[key]);
				}
			}
		});
	}

	public static string GetStringSetting(string Name, string Default = null)
	{
		if (StringSettings == null)
		{
			LoadGlobalConfig();
			if (StringSettings == null)
			{
				return Default;
			}
		}
		if (StringSettings.TryGetValue(Name, out var value))
		{
			return value;
		}
		return Default;
	}

	public static bool GetBoolSetting(string Name, bool Default = false)
	{
		if (BoolSettings == null || StringSettings == null)
		{
			LoadGlobalConfig();
			if (BoolSettings == null)
			{
				return Default;
			}
		}
		if (BoolSettings.TryGetValue(Name, out var value))
		{
			return value;
		}
		if (StringSettings != null && StringSettings.TryGetValue(Name, out var value2))
		{
			try
			{
				bool flag = bool.Parse(value2);
				BoolSettings.Add(Name, flag);
				return flag;
			}
			catch (Exception)
			{
			}
		}
		return Default;
	}

	public static int GetIntSetting(string Name, int Default = 0)
	{
		if (IntSettings == null)
		{
			LoadGlobalConfig();
			if (IntSettings == null)
			{
				return Default;
			}
		}
		if (IntSettings.TryGetValue(Name, out var value))
		{
			return value;
		}
		if (StringSettings.ContainsKey(Name))
		{
			try
			{
				int num = int.Parse(StringSettings[Name]);
				IntSettings.Add(Name, num);
				return num;
			}
			catch (Exception)
			{
			}
		}
		return Default;
	}

	public static float GetFloatSetting(string Name, float Default = 0f)
	{
		if (FloatSettings == null)
		{
			LoadGlobalConfig();
			if (FloatSettings == null)
			{
				return Default;
			}
		}
		if (FloatSettings.TryGetValue(Name, out var value))
		{
			return value;
		}
		if (StringSettings.ContainsKey(Name))
		{
			try
			{
				float num = Convert.ToSingle(StringSettings[Name]);
				FloatSettings.Add(Name, num);
				return num;
			}
			catch (Exception)
			{
			}
		}
		return Default;
	}
}
