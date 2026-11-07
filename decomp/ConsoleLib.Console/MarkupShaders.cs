using System;
using System.Collections.Generic;
using UnityEngine;
using XRL;
using XRL.UI;

namespace ConsoleLib.Console;

[HasModSensitiveStaticCache]
public static class MarkupShaders
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ShaderAssetType : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class ShaderAssetFactoryMethod : Attribute
	{
		public string Type;

		public ShaderAssetFactoryMethod(string Type)
		{
			this.Type = Type;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ShaderDecoratorType : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class ShaderDecoratorFactoryMethod : Attribute
	{
		public string Type;

		public ShaderDecoratorFactoryMethod(string Type)
		{
			this.Type = Type;
		}
	}

	[ShaderAssetType]
	public class Solid : IMarkupShader
	{
		[ShaderAssetFactoryMethod("solid")]
		public static Solid Generate(string Name)
		{
			return new Solid(Name);
		}

		public Solid(string Name)
			: base(Name)
		{
		}

		public override char? GetForegroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
		{
			return Colors[0];
		}
	}

	public abstract class ISequence : IMarkupShader
	{
		public ISequence(string Name)
			: base(Name)
		{
		}

		public override char? GetForegroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
		{
			return Colors[totalPos % Colors.Length];
		}
	}

	public class SequencePattern : ISequence
	{
		public SequencePattern()
			: base("sequence pattern")
		{
		}

		public override bool IsPattern()
		{
			return true;
		}

		public override int GetPatternPriority()
		{
			return 1000;
		}

		public override IMarkupShader GetMatchHandler(string action)
		{
			if (!action.EndsWith(" sequence") || action.Length < 10)
			{
				return null;
			}
			string colors = action.Substring(0, action.Length - 9);
			SequencePattern sequencePattern = new SequencePattern();
			sequencePattern.SetColors(colors);
			return sequencePattern;
		}
	}

	[ShaderAssetType]
	public class Sequence : ISequence
	{
		[ShaderAssetFactoryMethod("sequence")]
		public static Sequence Generate(string Name)
		{
			return new Sequence(Name);
		}

		public Sequence(string Name)
			: base(Name)
		{
		}
	}

	public abstract class IAlternation : IMarkupShader
	{
		public IAlternation(string Name)
			: base(Name)
		{
		}

		public override char? GetForegroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
		{
			return Colors[totalPos * Colors.Length / totalLen];
		}
	}

	public class AlternationPattern : IAlternation
	{
		public AlternationPattern()
			: base("alternation pattern")
		{
		}

		public override bool IsPattern()
		{
			return true;
		}

		public override int GetPatternPriority()
		{
			return 1200;
		}

		public override IMarkupShader GetMatchHandler(string action)
		{
			if (!action.EndsWith(" alternation") || action.Length < 13)
			{
				return null;
			}
			string colors = action.Substring(0, action.Length - 12);
			AlternationPattern alternationPattern = new AlternationPattern();
			alternationPattern.SetColors(colors);
			return alternationPattern;
		}
	}

	[ShaderAssetType]
	public class Alternation : IAlternation
	{
		[ShaderAssetFactoryMethod("alternation")]
		public static Alternation Generate(string Name)
		{
			return new Alternation(Name);
		}

		public Alternation(string Name)
			: base(Name)
		{
		}
	}

	public abstract class IBordered : IMarkupShader
	{
		public IBordered(string Name)
			: base(Name)
		{
		}

		public override char? GetForegroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
		{
			return (localPos == 0 || localPos == localLen - 1) ? Colors[1] : Colors[0];
		}
	}

	public class BorderedPattern : IBordered
	{
		public BorderedPattern()
			: base("bordered pattern")
		{
		}

		public override bool IsPattern()
		{
			return true;
		}

		public override int GetPatternPriority()
		{
			return 1500;
		}

		public override IMarkupShader GetMatchHandler(string action)
		{
			if (!action.EndsWith(" border") || action.Length < 15 || !action.Contains(" with "))
			{
				return null;
			}
			string text = action.Substring(0, action.Length - 7);
			BorderedPattern borderedPattern = new BorderedPattern();
			borderedPattern.SetColors(text.Split(new string[1] { " with " }, 2, StringSplitOptions.None));
			return borderedPattern;
		}
	}

	[ShaderAssetType]
	public class Bordered : IBordered
	{
		[ShaderAssetFactoryMethod("bordered")]
		public static Bordered Generate(string Name)
		{
			return new Bordered(Name);
		}

		public Bordered(string Name)
			: base(Name)
		{
		}
	}

	public abstract class IDistribution : IMarkupShader
	{
		public IDistribution(string Name)
			: base(Name)
		{
		}

		public override char? GetForegroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
		{
			int num = totalLen;
			int i = 0;
			for (int num2 = Colors.Length; i < num2; i++)
			{
				num += Colors[i] * (i + 1);
			}
			System.Random random = new System.Random(num);
			for (int j = 0; j < totalPos; j++)
			{
				random.Next();
			}
			return Colors.GetRandomElement(random);
		}
	}

	public class DistributionPattern : IDistribution
	{
		public DistributionPattern()
			: base("distribution pattern")
		{
		}

		public override bool IsPattern()
		{
			return true;
		}

		public override int GetPatternPriority()
		{
			return 2000;
		}

		public override IMarkupShader GetMatchHandler(string action)
		{
			if (!action.EndsWith(" distribution") || action.Length < 14)
			{
				return null;
			}
			string colors = action.Substring(0, action.Length - 13);
			DistributionPattern distributionPattern = new DistributionPattern();
			distributionPattern.SetColors(colors);
			return distributionPattern;
		}
	}

	[ShaderAssetType]
	public class Distribution : IDistribution
	{
		[ShaderAssetFactoryMethod("distribution")]
		public static Distribution Generate(string Name)
		{
			return new Distribution(Name);
		}

		public Distribution(string Name)
			: base(Name)
		{
		}
	}

	public class Chaotic : IMarkupShader
	{
		public Chaotic()
			: base("chaotic")
		{
			SetShowInPicker(flag: true);
		}

		public override char? GetForegroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
		{
			return Markup.NORMAL_FOREGROUNDS.GetRandomElementCosmetic();
		}
	}

	public class Random : IMarkupShader
	{
		private char Color;

		public Random()
			: base("random")
		{
			SetShowInPicker(flag: true);
		}

		public override IMarkupShader GetInstanceHandler()
		{
			return new Random
			{
				Color = Markup.NORMAL_FOREGROUNDS.GetRandomElementCosmetic()
			};
		}

		public override char? GetForegroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
		{
			return Color;
		}
	}

	[ShaderDecoratorType]
	public class ShimmeringDecorator : IMarkupDecorator
	{
		public int Chance = 25;

		[ShaderDecoratorFactoryMethod("shimmering")]
		public static ShimmeringDecorator Generate(IMarkupShader Component)
		{
			return new ShimmeringDecorator(Component);
		}

		public ShimmeringDecorator()
			: base("shimmering decorator")
		{
		}

		public ShimmeringDecorator(IMarkupShader Component)
			: this()
		{
			base.Component = Component;
		}

		public override void ApplyDecoratorParameter(string param)
		{
			try
			{
				Chance = Convert.ToInt32(param);
			}
			catch
			{
				throw new Exception("invalid shimmering chance specification: " + param);
			}
		}

		public override char? GetForegroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
		{
			char? foregroundColor = Component.GetForegroundColor(ch, localPos, localLen, totalPos, totalLen);
			if (foregroundColor.HasValue && Chance.in100())
			{
				char? alternateTone = GetAlternateTone(foregroundColor.Value);
				if (alternateTone.HasValue)
				{
					return alternateTone;
				}
			}
			return foregroundColor;
		}

		public override char? GetBackgroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
		{
			char? backgroundColor = Component.GetBackgroundColor(ch, localPos, localLen, totalPos, totalLen);
			if (backgroundColor.HasValue && backgroundColor != 'k' && Chance.in100())
			{
				char? alternateTone = GetAlternateTone(backgroundColor.Value);
				if (alternateTone.HasValue)
				{
					return alternateTone;
				}
			}
			return backgroundColor;
		}
	}

	public class ShimmeringPattern : IMarkupShader
	{
		public ShimmeringPattern()
			: base("shimmering pattern")
		{
		}

		public override bool IsPattern()
		{
			return true;
		}

		public override int GetPatternPriority()
		{
			return 2500;
		}

		public override IMarkupShader GetMatchHandler(string action)
		{
			if (!action.StartsWith("shimmering ") || action.Length < 12)
			{
				return null;
			}
			string text = action.Substring(11);
			return new ShimmeringDecorator(Find(text) ?? throw new Exception("invalid inner pattern \"" + text + "\""));
		}
	}

	[ModSensitiveStaticCache(true)]
	public static List<SolidColor> PickerColors = new List<SolidColor>();

	[ModSensitiveStaticCache(true)]
	public static List<SolidColor> Colors = new List<SolidColor>();

	[ModSensitiveStaticCache(true)]
	public static List<IMarkupShader> PickerShaders = new List<IMarkupShader>();

	[ModSensitiveStaticCache(true)]
	public static List<IMarkupShader> Shaders = new List<IMarkupShader>();

	[ModSensitiveStaticCache(true)]
	public static List<IMarkupShader> Patterns = new List<IMarkupShader>();

	[ModSensitiveStaticCache(true)]
	private static Dictionary<string, IMarkupShader> ByName = new Dictionary<string, IMarkupShader>();

	private static readonly Dictionary<string, Action<XmlDataHelper>> _outerNodes = new Dictionary<string, Action<XmlDataHelper>> { { "colors", HandleInnerNode } };

	private static readonly Dictionary<string, Action<XmlDataHelper>> _innerNodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "solidcolors", HandleInnerNode },
		{ "shaders", HandleInnerNode },
		{ "solidcolor", HandleSolidColorNode },
		{ "shader", HandleShaderNode }
	};

	public static IMarkupShader Get(string name)
	{
		CheckInit();
		ByName.TryGetValue(name, out var value);
		return value;
	}

	public static IMarkupShader Find(string action)
	{
		CheckInit();
		if (string.IsNullOrEmpty(action))
		{
			return null;
		}
		if (ByName.TryGetValue(action, out var value) && !value.IsPattern())
		{
			value = value.GetInstanceHandler();
			if (value != null)
			{
				return value;
			}
		}
		int i = 0;
		for (int count = Patterns.Count; i < count; i++)
		{
			value = Patterns[i].GetMatchHandler(action);
			if (value != null)
			{
				return value;
			}
		}
		return null;
	}

	public static SolidColor GetSolidColor(char color)
	{
		foreach (SolidColor pickerColor in PickerColors)
		{
			if (pickerColor.Foreground == color)
			{
				return pickerColor;
			}
		}
		foreach (SolidColor color2 in Colors)
		{
			if (color2.Foreground == color && (color2.LightTone.HasValue || color2.DarkTone.HasValue))
			{
				return color2;
			}
		}
		foreach (SolidColor color3 in Colors)
		{
			if (color3.Foreground == color)
			{
				return color3;
			}
		}
		return null;
	}

	public static char? GetLightTone(char color)
	{
		return GetSolidColor(color)?.LightTone;
	}

	public static char? GetDarkTone(char color)
	{
		return GetSolidColor(color)?.DarkTone;
	}

	public static char? GetAlternateTone(char color)
	{
		SolidColor solidColor = GetSolidColor(color);
		if (solidColor == null)
		{
			return null;
		}
		if (solidColor.LightTone.HasValue && solidColor.DarkTone.HasValue)
		{
			if (!50.in100())
			{
				return solidColor.DarkTone;
			}
			return solidColor.LightTone;
		}
		return solidColor.LightTone ?? solidColor.DarkTone;
	}

	public static void CheckInit()
	{
		if (Patterns.Count == 0)
		{
			Init();
		}
	}

	[ModSensitiveCacheInit]
	private static void Init()
	{
		ColorUtility.LoadBaseColors();
		foreach (char key in ColorUtility.ColorMap.Keys)
		{
			Register(new SolidColor(key.ToString() ?? "", key, 'k'));
			Register(new SolidColor("&" + key, key, 'k'));
			Register(new SolidColor("^" + key, Background: key, Foreground: 'y'));
			foreach (char key2 in ColorUtility.ColorMap.Keys)
			{
				Register(new SolidColor("&" + key + "^" + key2, key, key2));
				Register(new SolidColor("^" + key2 + "&" + key, key, key2));
			}
		}
		Register(new Chaotic());
		Register(new Random());
		Register(new SequencePattern());
		Register(new AlternationPattern());
		Register(new BorderedPattern());
		Register(new DistributionPattern());
		Register(new ShimmeringPattern());
		Loading.LoadTask("Loading Colors.xml", () =>
		{
			foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("colors"))
			{
				item.HandleNodes(_outerNodes);
			}
		});
	}

	public static void HandleInnerNode(XmlDataHelper xml)
	{
		xml.HandleNodes(_innerNodes);
	}

	public static void HandleSolidColorNode(XmlDataHelper Reader)
	{
		string attribute = Reader.GetAttribute("Name");
		if (string.IsNullOrEmpty(attribute))
		{
			throw new Exception(Reader.Name + " tag had missing or empty Name attribute");
		}
		string text = Reader.GetAttribute("DisplayName");
		if (text == "")
		{
			text = null;
		}
		string attribute2 = Reader.GetAttribute("Color");
		string attribute3 = Reader.GetAttribute("BackgroundColor");
		if (attribute2 == "")
		{
			throw new Exception(Reader.Name + " tag had empty Color attribute");
		}
		if (attribute3 == "")
		{
			throw new Exception(Reader.Name + " tag had empty BackgroundColor attribute");
		}
		if (Reader.modInfo == null && attribute2 == null && attribute3 == null)
		{
			throw new Exception(Reader.Name + " tag had no Color or BackgroundColor attribute");
		}
		if (attribute2 != null && attribute2.Length != 1)
		{
			throw new Exception(Reader.Name + " tag had Color attribute of length " + attribute2.Length);
		}
		if (attribute3 != null && attribute3.Length != 1)
		{
			throw new Exception(Reader.Name + " tag had BackgroundColor attribute of length " + attribute3.Length);
		}
		string attribute4 = Reader.GetAttribute("ShowInPicker");
		bool showInPicker = attribute4.EqualsNoCase("true");
		bool flag;
		SolidColor solidColor;
		if (ByName.TryGetValue(attribute, out var value))
		{
			flag = false;
			if (Reader.modInfo == null)
			{
				throw new Exception(Reader.Name + " tag had duplicate name " + attribute);
			}
			solidColor = value as SolidColor;
			if (solidColor == null)
			{
				throw new Exception(Reader.Name + " tag referred to " + value.GetType().Name + " " + attribute);
			}
		}
		else
		{
			flag = true;
			solidColor = new SolidColor(attribute, 'y', 'k');
			if (attribute2 == null && attribute3 == null)
			{
				throw new Exception(Reader.Name + " tag had no Color or BackgroundColor attribute");
			}
		}
		if (text != null)
		{
			solidColor.SetDisplayName(text);
		}
		if (!string.IsNullOrEmpty(attribute4))
		{
			solidColor.SetShowInPicker(showInPicker);
		}
		if (attribute2 != null)
		{
			solidColor.Foreground = attribute2[0];
		}
		if (attribute3 != null)
		{
			solidColor.Background = attribute3[0];
		}
		string attribute5 = Reader.GetAttribute("LightTone");
		if (attribute5 != null)
		{
			if (attribute5 == "")
			{
				solidColor.LightTone = null;
			}
			else
			{
				if (attribute5.Length != 1)
				{
					throw new Exception(Reader.Name + " tag had LightTone attribute of length " + attribute5.Length);
				}
				solidColor.LightTone = attribute5[0];
			}
		}
		string attribute6 = Reader.GetAttribute("DarkTone");
		if (attribute6 != null)
		{
			if (attribute6 == "")
			{
				solidColor.DarkTone = null;
			}
			else
			{
				if (attribute6.Length != 1)
				{
					throw new Exception(Reader.Name + " tag had DarkTone attribute of length " + attribute6.Length);
				}
				solidColor.DarkTone = attribute6[0];
			}
		}
		if (flag)
		{
			Register(solidColor);
		}
		else if (!solidColor.GetShowInPicker())
		{
			PickerColors.Remove(solidColor);
		}
		Reader.DoneWithElement();
	}

	public static void HandleShaderNode(XmlDataHelper Reader)
	{
		_ = Reader.modInfo;
		string attribute = Reader.GetAttribute("Name");
		if (string.IsNullOrEmpty(attribute))
		{
			throw new Exception(Reader.Name + " tag had missing or empty Name attribute");
		}
		string text = Reader.GetAttribute("DisplayName");
		if (text == "")
		{
			text = null;
		}
		string attribute2 = Reader.GetAttribute("SystemSymbol");
		bool systemSymbol = attribute2.EqualsNoCase("true");
		string attribute3 = Reader.GetAttribute("ShowInPicker");
		bool showInPicker = attribute3.EqualsNoCase("true");
		string attribute4 = Reader.GetAttribute("Type");
		if (attribute4 == "")
		{
			throw new Exception(Reader.Name + " tag had empty Type attribute");
		}
		string attribute5 = Reader.GetAttribute("Colors");
		if (string.IsNullOrEmpty(attribute5))
		{
			throw new Exception(Reader.Name + " tag had missing or empty Colors attribute");
		}
		string attribute6 = Reader.GetAttribute("Decorators");
		bool flag;
		if (ByName.TryGetValue(attribute, out var value))
		{
			flag = false;
			if (value is SolidColor)
			{
				throw new Exception(Reader.Name + " tag referred to non-shader " + value.GetType().Name + " " + attribute);
			}
		}
		else
		{
			flag = true;
			if (attribute4 == null)
			{
				throw new Exception(Reader.Name + " tag had missing Type attribute");
			}
			value = IMarkupShader.InstanceByType(attribute4, attribute);
		}
		if (text != null)
		{
			value.SetDisplayName(text);
		}
		if (!string.IsNullOrEmpty(attribute2))
		{
			value.SetSystemSymbol(systemSymbol);
		}
		if (!string.IsNullOrEmpty(attribute3))
		{
			value.SetShowInPicker(showInPicker);
		}
		value.SetColors(attribute5);
		if (!string.IsNullOrEmpty(attribute6))
		{
			string[] array = attribute6.Split(',');
			foreach (string text2 in array)
			{
				if (text2.Contains(":"))
				{
					string[] array2 = text2.Split(':');
					IMarkupDecorator markupDecorator = IMarkupDecorator.InstanceByType(array2[0], value);
					int j = 1;
					for (int num = array2.Length; j < num; j++)
					{
						markupDecorator.ApplyDecoratorParameter(array2[j]);
					}
					value = markupDecorator;
				}
				else
				{
					value = IMarkupDecorator.InstanceByType(text2, value);
				}
			}
		}
		if (flag)
		{
			Register(value);
		}
		else if (!string.IsNullOrEmpty(attribute6))
		{
			ReRegister(value);
		}
		else if (!value.GetShowInPicker())
		{
			PickerShaders.Remove(value);
		}
		Reader.DoneWithElement();
	}

	public static bool Register(IMarkupShader shader)
	{
		string name = shader.GetName();
		if (string.IsNullOrEmpty(name))
		{
			Debug.LogError("missing name from " + shader.GetType().Name);
			return false;
		}
		if (ByName.TryGetValue(name, out var value))
		{
			if (value != shader)
			{
				Debug.LogError("duplicate name " + name + " from " + shader.GetType().Name + " and " + value.GetType().Name);
				return false;
			}
		}
		else
		{
			ByName.Add(name, shader);
			Shaders.Add(shader);
			if (shader.IsPattern())
			{
				Patterns.Add(shader);
				Patterns.Sort();
			}
		}
		if (shader is SolidColor solidColor)
		{
			if (!Colors.Contains(solidColor))
			{
				Colors.Add(solidColor);
			}
			if (solidColor.GetShowInPicker() && !PickerColors.Contains(solidColor))
			{
				PickerColors.Add(solidColor);
			}
		}
		else if (shader.GetShowInPicker() && !PickerShaders.Contains(shader))
		{
			PickerShaders.Add(shader);
		}
		return true;
	}

	public static bool ReRegister(IMarkupShader shader)
	{
		string name = shader.GetName();
		if (string.IsNullOrEmpty(name))
		{
			Debug.LogError("missing name from " + shader.GetType().Name);
			return false;
		}
		if (ByName.TryGetValue(name, out var value))
		{
			ByName[name] = shader;
			Shaders.Remove(value);
			Shaders.Add(shader);
			if (value.IsPattern() || shader.IsPattern())
			{
				Patterns.Remove(value);
				Patterns.Add(shader);
				Patterns.Sort();
			}
			if (shader is SolidColor solidColor)
			{
				if (value is SolidColor item)
				{
					Colors.Remove(item);
					PickerColors.Remove(item);
				}
				if (!Colors.Contains(solidColor))
				{
					Colors.Add(solidColor);
				}
				if (solidColor.GetShowInPicker() && !PickerColors.Contains(solidColor))
				{
					PickerColors.Add(solidColor);
				}
			}
			else
			{
				PickerShaders.Remove(value);
				if (shader.GetShowInPicker() && !PickerShaders.Contains(shader))
				{
					PickerShaders.Add(shader);
				}
			}
		}
		else
		{
			ByName.Add(name, shader);
			Shaders.Add(shader);
			if (shader.IsPattern())
			{
				Patterns.Add(shader);
				Patterns.Sort();
			}
			if (shader is SolidColor solidColor2)
			{
				if (!Colors.Contains(solidColor2))
				{
					Colors.Add(solidColor2);
				}
				if (solidColor2.GetShowInPicker() && !PickerColors.Contains(solidColor2))
				{
					PickerColors.Add(solidColor2);
				}
			}
			else if (shader.GetShowInPicker() && !PickerShaders.Contains(shader))
			{
				PickerShaders.Add(shader);
			}
		}
		return true;
	}
}
