using System;
using System.Reflection;
using XRL.World;

namespace Sheeter;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class Property : BlueprintElement
{
	public override string GetFrom(GameObjectBlueprint Blueprint)
	{
		return typeof(GameObjectBlueprint).GetProperty(Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(Blueprint)?.ToString();
	}
}
