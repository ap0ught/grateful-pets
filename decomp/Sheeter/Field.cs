using System;
using System.Reflection;
using XRL.World;

namespace Sheeter;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class Field : BlueprintElement
{
	public override string GetFrom(GameObjectBlueprint Blueprint)
	{
		return typeof(GameObjectBlueprint).GetField(Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(Blueprint)?.ToString();
	}
}
