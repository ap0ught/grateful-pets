using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using XRL;

namespace Genkit;

public static class WaveCollapseTools
{
	public static Dictionary<string, WaveTemplateEntry> waveTemplates = new Dictionary<string, WaveTemplateEntry>();

	public static void LoadTemplates(bool force = false)
	{
		if (!force && waveTemplates.Count > 0)
		{
			return;
		}
		waveTemplates = new Dictionary<string, WaveTemplateEntry>();
		ModManager.ForEachFileIn("wavetemplates", (string path, ModInfo info) =>
		{
			try
			{
				if (!string.IsNullOrEmpty(path) && path.ToUpper().EndsWith(".PNG"))
				{
					Texture2D texture2D = LoadPNG(path);
					if (texture2D == null)
					{
						Debug.Log("skipping file: " + path + " because tex returned NULL");
					}
					else
					{
						WaveTemplateEntry waveTemplateEntry = new WaveTemplateEntry();
						if (waveTemplateEntry == null)
						{
							Debug.Log("newEntry is null!");
						}
						if (texture2D == null)
						{
							Debug.Log("tex is null!");
						}
						waveTemplateEntry.name = Path.GetFileNameWithoutExtension(path);
						waveTemplateEntry.width = texture2D.width;
						waveTemplateEntry.height = texture2D.height;
						waveTemplateEntry.pixels = texture2D.GetPixels32();
						if (waveTemplates == null)
						{
							Debug.Log("waveTemplates is null!");
						}
						waveTemplates.Add(waveTemplateEntry.name.ToLower(), waveTemplateEntry);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.Log("skipping file: " + path + " because of " + ex.ToString());
			}
		}, bIncludeBase: true);
	}

	public static Texture2D LoadPNG(string filePath)
	{
		byte[] array = File.ReadAllBytes(filePath);
		if (array == null)
		{
			Debug.Log("failed to read file data from: " + filePath);
		}
		Texture2D texture2D = new Texture2D(2, 2);
		texture2D.LoadImage(array);
		if (array == null)
		{
			Debug.Log("failed to read file data because LoadImage puked: " + filePath);
		}
		return texture2D;
	}

	public static Color32[] GetPixelsFromPNG(string path)
	{
		return LoadPNG(path).GetPixels32();
	}

	public static bool equals(Color32 a, Color32 b)
	{
		if (a.r == b.r && a.g == b.g && a.b == b.b)
		{
			return a.a == b.a;
		}
		return false;
	}

	public static int Random(this List<double> a, double r)
	{
		double num = a.Sum();
		if (num == 0.0)
		{
			for (int i = 0; i < Enumerable.Count(a); i++)
			{
				a[i] = 1.0;
			}
			num = a.Sum();
		}
		for (int j = 0; j < Enumerable.Count(a); j++)
		{
			a[j] /= num;
		}
		int k = 0;
		double num2 = 0.0;
		for (; k < Enumerable.Count(a); k++)
		{
			num2 += a[k];
			if (r <= num2)
			{
				return k;
			}
		}
		return 0;
	}

	public static int Random(this double[] a, double r)
	{
		double num = a.Sum();
		if (num == 0.0)
		{
			for (int i = 0; i < a.Count(); i++)
			{
				a[i] = 1.0;
			}
			num = a.Sum();
		}
		for (int j = 0; j < a.Count(); j++)
		{
			a[j] /= num;
		}
		int k = 0;
		double num2 = 0.0;
		for (; k < a.Count(); k++)
		{
			num2 += a[k];
			if (r <= num2)
			{
				return k;
			}
		}
		return 0;
	}

	public static long Power(int a, int n)
	{
		long num = 1L;
		for (int i = 0; i < n; i++)
		{
			num *= a;
		}
		return num;
	}

	public static T Get<T>(this XmlNode node, string attribute, T defaultT = default(T))
	{
		string attribute2 = ((XmlElement)node).GetAttribute(attribute);
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
		if (!(attribute2 == ""))
		{
			return (T)converter.ConvertFromInvariantString(attribute2);
		}
		return defaultT;
	}
}
