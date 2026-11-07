using System.Collections.Generic;
using System.Text;

namespace XRL.World.Text.Delegates;

/// <remarks>Used to decouple parameters from delegate signature for mod compatibility.</remarks>
public class DelegateContext
{
	public StringBuilder Value;

	/// <summary>The target game object as defined by the attribute, if applicable.</summary>
	public GameObject Target;

	/// <summary>Pronouns of the target, defaults to the neuter pronoun provider.</summary>
	public IPronounProvider Pronouns = Gender.DefaultNeuter;

	/// <summary>The target type.</summary>
	public TargetType Type;

	/// <summary>The explicit string target, used for non-game object transformations.</summary>
	public string Explicit;

	/// <summary>An optional default string.</summary>
	/// <remarks>Defined in attribute.</remarks>
	public string Default;

	/// <summary>Whether the output should be capitalized.</summary>
	public bool Capitalize;

	/// <summary>Attribute flags, mod field.</summary>
	public int Flags;

	/// <summary>A list of XML provided parameters.</summary>
	public List<string> Parameters;

	internal static DelegateContext Instance = new DelegateContext();

	internal static DelegateContext Set(StringBuilder Value, List<string> Parameters, GameObject Target, IPronounProvider Pronouns, TargetType Type, string Explicit, string Default, bool Capitalize, int Flags)
	{
		Instance.Value = Value;
		Instance.Parameters = Parameters;
		Instance.Target = Target;
		Instance.Type = Type;
		Instance.Pronouns = Pronouns ?? Target?.GetPronounProvider() ?? Gender.DefaultNeuter;
		Instance.Explicit = Explicit;
		Instance.Default = Default;
		Instance.Capitalize = Capitalize;
		Instance.Flags = Flags;
		return Instance;
	}
}
