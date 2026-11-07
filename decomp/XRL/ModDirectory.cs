using System.Collections.Generic;
using Newtonsoft.Json;
using XRL.UI;

namespace XRL;

public class ModDirectory
{
	/// <summary>The paths to include in mod loading if this directory is valid.</summary>
	/// <remarks>Loading is recursive and subsumes any subdirectories, no paths will be loaded twice even if duplicated across directories.</remarks>
	public string[] Paths;

	/// <summary>A semantic version range that is required to match <see cref="F:XRL.XRLGame.MarketingVersion" />.</summary>
	/// <seealso cref="M:XRL.Version.EqualsSemantic(System.ReadOnlySpan{System.Char})" />
	public string Version;

	/// <summary>A semantic version range that is required to match <see cref="F:XRL.XRLGame.CoreVersion" />.</summary>
	/// <seealso cref="M:XRL.Version.EqualsSemantic(System.ReadOnlySpan{System.Char})" />
	public string Build;

	/// <summary>A set of required option state.</summary>
	/// <remarks>
	/// The XML file containing the referenced options is required to have "Option" somewhere in its file name
	/// in order for its option defaults to populate prior to directory resolution. 
	/// </remarks>
	public GameOption.RequiresSpec Options;

	/// <summary>A map of required dependencies with semantic version ranges.</summary>
	/// <seealso cref="M:XRL.Version.EqualsSemantic(System.ReadOnlySpan{System.Char})" />
	public Dictionary<string, string> Dependencies;

	/// <summary>A map of exlusions with semantic version ranges.</summary>
	/// <seealso cref="M:XRL.Version.EqualsSemantic(System.ReadOnlySpan{System.Char})" />
	public Dictionary<string, string> Exclusions;

	/// <summary>A single value initializer for <see cref="F:XRL.ModDirectory.Dependencies" />.</summary>
	[JsonProperty]
	private string Dependency
	{
		init
		{
			Dependencies = new Dictionary<string, string> { { value, "*" } };
		}
	}

	/// <summary>A single value initializer for <see cref="F:XRL.ModDirectory.Exclusions" />.</summary>
	[JsonProperty]
	private string Exclusion
	{
		init
		{
			Exclusions = new Dictionary<string, string> { { value, "*" } };
		}
	}

	/// <summary>A single value initializer for <see cref="F:XRL.ModDirectory.Paths" />.</summary>
	[JsonProperty]
	private string Path
	{
		init
		{
			Paths = new string[1] { value };
		}
	}

	/// <inheritdoc cref="F:XRL.ModDirectory.Options" />
	[JsonProperty]
	private GameOption.RequiresSpec Option
	{
		init
		{
			Options = value;
		}
	}
}
