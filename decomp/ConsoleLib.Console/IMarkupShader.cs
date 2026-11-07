using System;
using System.Collections.Generic;
using System.Reflection;
using XRL;

namespace ConsoleLib.Console;

public abstract class IMarkupShader : IComparable
{
	private string Name;

	private string DisplayName;

	private bool SystemSymbol;

	private bool ShowInPicker;

	protected char[] Colors;

	private static Dictionary<string, Func<string, IMarkupShader>> ShaderAssetFactoryMethods;

	public static IMarkupShader InstanceByType(string type, string name)
	{
		if (ShaderAssetFactoryMethods == null)
		{
			List<MethodInfo> list = new List<MethodInfo>(ModManager.GetMethodsWithAttribute(typeof(MarkupShaders.ShaderAssetFactoryMethod), typeof(MarkupShaders.ShaderAssetType)));
			ShaderAssetFactoryMethods = new Dictionary<string, Func<string, IMarkupShader>>(list.Count);
			foreach (MethodInfo item in list)
			{
				MarkupShaders.ShaderAssetFactoryMethod[] array = item.GetCustomAttributes(typeof(MarkupShaders.ShaderAssetFactoryMethod), inherit: false) as MarkupShaders.ShaderAssetFactoryMethod[];
				foreach (MarkupShaders.ShaderAssetFactoryMethod shaderAssetFactoryMethod in array)
				{
					if (shaderAssetFactoryMethod.Type != null)
					{
						if (ShaderAssetFactoryMethods.ContainsKey(shaderAssetFactoryMethod.Type))
						{
							MetricsManager.LogError("duplicate shader asset type: " + shaderAssetFactoryMethod.Type);
						}
						else
						{
							ShaderAssetFactoryMethods[shaderAssetFactoryMethod.Type] = (Func<string, IMarkupShader>)Delegate.CreateDelegate(typeof(Func<string, IMarkupShader>), item);
						}
					}
				}
			}
		}
		if (ShaderAssetFactoryMethods.TryGetValue(type, out var value))
		{
			return value(name);
		}
		throw new Exception("unsupported shader type: " + type);
	}

	public IMarkupShader()
	{
	}

	public IMarkupShader(string Name)
	{
		this.Name = Name;
		DisplayName = Name;
	}

	public virtual string GetName()
	{
		return Name;
	}

	public virtual void SetDisplayName(string name)
	{
		DisplayName = name;
	}

	public virtual string GetDisplayName()
	{
		return DisplayName;
	}

	public virtual void SetSystemSymbol(bool flag)
	{
		SystemSymbol = flag;
	}

	public virtual bool IsSystemSymbol()
	{
		return SystemSymbol;
	}

	public virtual void SetShowInPicker(bool flag)
	{
		ShowInPicker = flag;
	}

	public virtual bool GetShowInPicker()
	{
		return ShowInPicker;
	}

	public void SetColors(char[] Colors)
	{
		this.Colors = Colors;
	}

	public void SetColors(string[] colorSpecs)
	{
		Colors = new char[colorSpecs.Length];
		int i = 0;
		for (int num = colorSpecs.Length; i < num; i++)
		{
			SolidColor solidColor = MarkupShaders.Get(colorSpecs[i]) as SolidColor;
			Colors[i] = ((solidColor == null) ? 'y' : (solidColor.Foreground ?? 'y'));
		}
	}

	public void SetColors(string colorSpec)
	{
		SetColors(colorSpec.Split('-'));
	}

	public virtual char? GetForegroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
	{
		return 'y';
	}

	public virtual char? GetBackgroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
	{
		return 'k';
	}

	public virtual bool IsPattern()
	{
		return false;
	}

	public virtual IMarkupShader GetInstanceHandler()
	{
		return this;
	}

	public virtual IMarkupShader GetMatchHandler(string action)
	{
		return null;
	}

	public virtual int GetPatternPriority()
	{
		return 100;
	}

	public virtual IMarkupShader GetBasisShader()
	{
		return this;
	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as IMarkupShader);
	}

	public int CompareTo(IMarkupShader o)
	{
		return o?.GetPatternPriority().CompareTo(GetPatternPriority()) ?? (-1);
	}
}
