using System;
using System.Collections.Generic;
using System.Reflection;
using XRL;

namespace ConsoleLib.Console;

public abstract class IMarkupDecorator : IMarkupShader
{
	protected IMarkupShader Component;

	private static Dictionary<string, Func<IMarkupShader, IMarkupDecorator>> ShaderDecoratorFactoryMethods;

	public static IMarkupDecorator InstanceByType(string type, IMarkupShader shader)
	{
		if (ShaderDecoratorFactoryMethods == null)
		{
			List<MethodInfo> list = new List<MethodInfo>(ModManager.GetMethodsWithAttribute(typeof(MarkupShaders.ShaderDecoratorFactoryMethod), typeof(MarkupShaders.ShaderDecoratorType)));
			ShaderDecoratorFactoryMethods = new Dictionary<string, Func<IMarkupShader, IMarkupDecorator>>(list.Count);
			foreach (MethodInfo item in list)
			{
				MarkupShaders.ShaderDecoratorFactoryMethod[] array = item.GetCustomAttributes(typeof(MarkupShaders.ShaderDecoratorFactoryMethod), inherit: false) as MarkupShaders.ShaderDecoratorFactoryMethod[];
				foreach (MarkupShaders.ShaderDecoratorFactoryMethod shaderDecoratorFactoryMethod in array)
				{
					if (shaderDecoratorFactoryMethod.Type != null)
					{
						if (ShaderDecoratorFactoryMethods.ContainsKey(shaderDecoratorFactoryMethod.Type))
						{
							MetricsManager.LogError("duplicate shader asset type: " + shaderDecoratorFactoryMethod.Type);
						}
						else
						{
							ShaderDecoratorFactoryMethods[shaderDecoratorFactoryMethod.Type] = (Func<IMarkupShader, IMarkupDecorator>)Delegate.CreateDelegate(typeof(Func<IMarkupShader, IMarkupDecorator>), item);
						}
					}
				}
			}
		}
		if (ShaderDecoratorFactoryMethods.TryGetValue(type, out var value))
		{
			return value(shader);
		}
		throw new Exception("unsupported shader decorator type: " + type);
	}

	public IMarkupDecorator()
	{
	}

	public IMarkupDecorator(string Name)
		: base(Name)
	{
	}

	public override string GetName()
	{
		return Component.GetName();
	}

	public override string GetDisplayName()
	{
		return Component.GetDisplayName();
	}

	public override bool IsSystemSymbol()
	{
		return Component.IsSystemSymbol();
	}

	public override bool GetShowInPicker()
	{
		return Component.GetShowInPicker();
	}

	public override IMarkupShader GetBasisShader()
	{
		return Component;
	}

	public virtual void ApplyDecoratorParameter(string param)
	{
		throw new Exception("decorator parameter " + param + " not supported on " + GetType().Name);
	}
}
