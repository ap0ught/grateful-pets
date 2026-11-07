using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Class)]
public class RequiresMod : Attribute
{
	public string ID;

	public ulong WorkshopID;
}
