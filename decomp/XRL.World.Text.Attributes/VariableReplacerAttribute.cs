using System;

namespace XRL.World.Text.Attributes;

/// <summary>Marks a simple variable replacer delegate.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class VariableReplacerAttribute : Attribute
{
	/// <summary>A range of keys used to invoke this delegate.</summary>
	public string[] Keys = Array.Empty<string>();

	/// <summary>A default string to return if applicable.</summary>
	public string Default;

	/// <summary>Generate an additional delegate with capitalized <see cref="F:XRL.World.Text.Attributes.VariableReplacerAttribute.Keys" /> and <see cref="F:XRL.World.Text.Attributes.VariableReplacerAttribute.Default" />.</summary>
	public bool Capitalization;

	/// <summary>Override a preceding delegate of the same key.</summary>
	/// <remarks>Modding utility, unused in base game.</remarks>
	public bool Override;

	/// <summary>Arbitrary flags passed directly from attribute to delegate context.</summary>
	/// <remarks>Modding utility, unused in base game.</remarks>
	public int Flags;

	public VariableReplacerAttribute()
	{
	}

	public VariableReplacerAttribute(params string[] Keys)
	{
		this.Keys = Keys;
	}
}
