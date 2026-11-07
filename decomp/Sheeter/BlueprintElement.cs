using System;
using XRL.World;

namespace Sheeter;

public class BlueprintElement : Attribute
{
	public string Key;

	public virtual string GetFrom(GameObjectBlueprint Blueprint)
	{
		return null;
	}
}
