using System;
using XRL.World;

namespace Sheeter;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class Part : BlueprintElement
{
	public override string GetFrom(GameObjectBlueprint Blueprint)
	{
		if (!Blueprint.Parts.ContainsKey(Key))
		{
			return null;
		}
		return "true";
	}
}
