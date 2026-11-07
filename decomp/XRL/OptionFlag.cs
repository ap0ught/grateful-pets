using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class OptionFlag : Attribute
{
	public string ID;

	public bool AllowMissing;

	public OptionFlag()
	{
	}

	public OptionFlag(string ID)
	{
		this.ID = ID;
	}
}
